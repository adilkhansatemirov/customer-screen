using Resto.Front.Api.Data.Assortment;
using Resto.Front.Api.Data.Orders;
using Resto.Front.Api.Data.Payments;
using Resto.Front.Api.Editors;
using Resto.Front.Api.Extensions;
using Resto.Front.Api.CustomerScreen.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Resto.Front.Api.CustomerScreen.View
{
    public partial class CustomerScreenWindow : Window, INotifyPropertyChanged
    {
        public bool CanBeClosed = false;
        private IOrder currentOrder;

        public event PropertyChangedEventHandler PropertyChanged;

        private ScreenType currentScreen = ScreenType.Welcome;
        public ScreenType CurrentScreen
        {
            get => currentScreen;
            set
            {
                currentScreen = value;
                PluginContext.Log.Info("CurrentScreen set to: " + currentScreen);
                OnPropertyChanged(nameof(CurrentScreen));
                
                // Show payment types popup when entering payment screen
                if (currentScreen == ScreenType.Payment)
                {
                    ShowPaymentTypesPopup();
                }
            }
        }

        private LanguageEnum selectedLanguage;
        public LanguageEnum SelectedLanguage
        {
            get => selectedLanguage;
            set
            {
                selectedLanguage = value;
                OnPropertyChanged(nameof(SelectedLanguage));
                OnPropertyChanged("Item[]"); // refresh bindings for dictionary lookups
            }
        }

        private ObservableCollection<User> users;
        public ObservableCollection<User> Users
        {
            get => users;
            set
            {
                users = value;
                OnPropertyChanged(nameof(Users));
            }
        }

        private List<IProduct> dishes;
        public List<IProduct> Dishes
        {
            get => dishes;
            set
            {
                dishes = value;
                OnPropertyChanged(nameof(Dishes));
            }
        }

        private ObservableCollection<IProduct> orderDishes;
        public ObservableCollection<IProduct> OrderDishes
        {
            get => orderDishes;
            set
            {
                orderDishes = value;
                OnPropertyChanged(nameof(OrderDishes));
            }
        }

        private Dictionary<Guid, string> dishDisplayNames = new Dictionary<Guid, string>();
        public Dictionary<Guid, string> DishDisplayNames
        {
            get => dishDisplayNames;
            set
            {
                dishDisplayNames = value;
                OnPropertyChanged(nameof(DishDisplayNames));
            }
        }

        private string errorMessage;
        public string ErrorMessage
        {
            get => errorMessage;
            set
            {
                errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
            }
        }

        // Translation indexer
        public string this[string key]
        {
            get
            {
                if (Translations.Data.ContainsKey(SelectedLanguage) &&
                    Translations.Data[SelectedLanguage].ContainsKey(key))
                {
                    return Translations.Data[SelectedLanguage][key];
                }
                return $"[{key}]"; // fallback so you can see missing keys
            }
        }

        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public CustomerScreenWindow()
        {
            InitializeComponent();
            DataContext = this;
            CurrentScreen = ScreenType.Welcome;
            OrderDishes = null; // Initialize as null - will be set only after random selection
            PluginContext.Log.Info("Set screen to: " + CurrentScreen);
        }

        void CustomerScreenWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            PluginContext.Log.InfoFormat("Customer window try to close.");
            if (CanBeClosed)
            {
                PluginContext.Log.InfoFormat("Customer window closed.");
                return;
            }
            e.Cancel = true;
            PluginContext.Log.InfoFormat("Customer window closing aborted.");
        }

        private void CustomerScreenWindow_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
                WindowState = WindowState.Maximized;
        }

        void CustomerScreenWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (WindowState == WindowState.Minimized)
                WindowState = WindowState.Maximized;
        }

        public void ChangeSumChanged(decimal sum)
        {
            //ctlResultSum.ChangeSumChanged(sum);
        }

        private void RussianLanguageButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedLanguage = LanguageEnum.Russian;
            CurrentScreen = ScreenType.Scanning;
        }

        private void KazakhLanguageButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedLanguage = LanguageEnum.Kazakh;
            CurrentScreen = ScreenType.Scanning;
        }

        private async void ApiRequestButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentScreen = ScreenType.Loading;
            try
            {
                await MakeApiRequestAsync();
                
                // Clear OrderDishes initially - don't show all dishes
                OrderDishes = null;
                
                var credentials = PluginContext.Operations.GetDefaultCredentials();
                var editSession = PluginContext.Operations.CreateEditSession();
                var newOrder = editSession.CreateOrder(null);
                editSession.ChangeOrderOriginName("Customer Screen", newOrder);
                var guest1 = editSession.AddOrderGuest("Bratishka", newOrder);
                
                // Use dishes from MakeApiRequestAsync
                var allProducts = Dishes;
                
                if (allProducts == null || allProducts.Count == 0)
                {
                    PluginContext.Log.Info("No products available in menu");
                    ErrorMessage = "No products available in menu";
                    CurrentScreen = ScreenType.Error;
                    return;
                }
                
                var apiResponseItems = OrderPopulationHelper.FetchMenuItemNamesFromApi();
                PluginContext.Log.Info($"Menu API returned {apiResponseItems.Count} items: {string.Join(", ", apiResponseItems)}");
                
                // Map API response strings to dishes
                var mappedDishes = new List<DishMappingResult>();
                foreach (var item in apiResponseItems)
                {
                    var mapped = DishMappingHelper.MapStringToDish(item, allProducts);
                    if (mapped != null)
                    {
                        mappedDishes.Add(mapped);
                        PluginContext.Log.Info($"Mapped '{item}' to dish: {mapped.DisplayName}");
                    }
                    else
                    {
                        PluginContext.Log.Info($"Warning: Could not map '{item}' to any dish");
                    }
                }
                
                if (mappedDishes.Count == 0)
                {
                    PluginContext.Log.Info("No dishes could be mapped from API response");
                    ErrorMessage = "No dishes could be mapped from API response";
                    CurrentScreen = ScreenType.Error;
                    return;
                }
                
                // Store mapped dishes for UI display
                var productsForDisplay = mappedDishes.Select(m => m.Product).ToList();
                OrderDishes = new ObservableCollection<IProduct>(productsForDisplay);
                
                // Store display names for UI
                DishDisplayNames = mappedDishes.ToDictionary(m => m.Product.Id, m => m.DisplayName);

                CurrentScreen = ScreenType.Success;
                
                // Add mapped products to order
                foreach (var mappedDish in mappedDishes)
                {
                    var product = mappedDish.Product;
                    var size = mappedDish.Size;

                    // If product has a scale but no size was mapped, try to get default or first available
                    if (product.Scale != null && size == null)
                    {
                        // Try to use default size first
                        size = product.Scale.DefaultSize;

                        // If no default size, get the first available size from the scale
                        if (size == null)
                        {
                            var availableSizes = PluginContext.Operations.GetProductScaleSizes(product.Scale)
                                .Except(PluginContext.Operations.GetDisabledSizesByProduct(product))
                                .ToList();

                            if (availableSizes.Count > 0)
                            {
                                size = availableSizes.FirstOrDefault();
                                PluginContext.Log.Info($"No mapped size for {product.Name}, using first available: {size?.Name}");
                            }
                        }

                        if (size == null)
                        {
                            PluginContext.Log.Info($"Warning: Product {product.Name} has scale but no sizes available");
                        }
                    }
                    
                    var productStub = editSession.AddOrderProductItem(1m, product, newOrder, guest1, size);
                    // Add modifiers to every other product (products at index 0 and 2 will have modifiers)
                    // if (i % 2 == 0)
                    // {
                    // Add simple modifiers
                    var simpleModifiers = product.GetSimpleModifiers(null);
                            // .Where(x => x.DefaultAmount != 0)
                            // .Take(2); // Limit to 2 modifiers per product
                        foreach (var modifier in simpleModifiers)
                        {
                            PluginContext.Log.Info($"Simple Modifier for product {product.Name}: {modifier.MinimumAmount}, DefaultAmount: {modifier.DefaultAmount}");
                            // editSession.AddOrderModifierItem(modifier.DefaultAmount, modifier.Product, null, newOrder, productStub);
                        }

                    // Add group modifiers
                    //var groupModifiers = product.GetGroupModifiers(null);
                    //foreach (var groupModifier in groupModifiers)
                    //{
                    //var itemsToAdd = groupModifier.Items;
                    //        // .Where(x => x.DefaultAmount != 0)
                    //        // .Take(1); // Add one item from each group
                    //    foreach (var item in itemsToAdd)
                    //    {
                    //        PluginContext.Log.Info($"Group Modifier for product {product.Name}: {groupModifier.MinimumAmount}, DefaultAmount: {item.DefaultAmount}");
                    //        // editSession.AddOrderModifierItem(item.DefaultAmount, item.Product, groupModifier.ProductGroup, newOrder, productStub);
                    //    }
                    //}
                    // Add group modifiers - MUST handle required groups (MinimumAmount > 0)
                    var groupModifiers = product.GetGroupModifiers(null);
                    foreach (var groupModifier in groupModifiers)
                    {
                        // If we have a specific modifier ID from mapping, use it (preferred)
                        if (mappedDish.ModifierProductId.HasValue)
                        {
                            var modifierItem = groupModifier.Items.FirstOrDefault(item => 
                                item.Product.Id == mappedDish.ModifierProductId.Value);
                            
                            if (modifierItem != null)
                            {
                                var amount = Math.Max(1, modifierItem.MinimumAmount);
                                if (amount == 0) amount = 1;
                                editSession.AddOrderModifierItem(amount, modifierItem.Product, groupModifier.ProductGroup, newOrder, productStub);
                                PluginContext.Log.Info($"Added mapped group modifier {modifierItem.Product.Name} (ID: {modifierItem.Product.Id}) for {product.Name}");
                                continue; // Skip default handling for this group
                            }
                        }
                        // Fallback: If we have a specific modifier name from mapping, try to use it
                        else if (!string.IsNullOrEmpty(mappedDish.ModifierName))
                        {
                            var modifierItem = groupModifier.Items.FirstOrDefault(item => 
                                item.Product.Name.Equals(mappedDish.ModifierName, StringComparison.OrdinalIgnoreCase));
                            
                            if (modifierItem != null)
                            {
                                var amount = Math.Max(1, modifierItem.MinimumAmount);
                                if (amount == 0) amount = 1;
                                editSession.AddOrderModifierItem(amount, modifierItem.Product, groupModifier.ProductGroup, newOrder, productStub);
                                PluginContext.Log.Info($"Added mapped group modifier {modifierItem.Product.Name} for {product.Name}");
                                continue; // Skip default handling for this group
                            }
                        }
                        
                        // Check if this group is required (has minimum amount > 0)
                        if (groupModifier.MinimumAmount > 0)
                        {
                            // This is a REQUIRED group - we must add at least MinimumAmount items
                            var count = 0;
                            var itemsToAdd = groupModifier.Items
                                .OrderBy(item => item.MinimumAmount)
                                .ThenBy(item => item.DefaultAmount)
                                .ToList();

                            foreach (var item in itemsToAdd)
                            {
                                var amount = item.MinimumAmount;
                                if (amount == 0 || item.DefaultAmount != 0)
                                    amount = item.DefaultAmount;
                                if (amount == 0)
                                    amount = Math.Min(item.MaximumAmount, groupModifier.MaximumAmount - count);

                                if (amount > 0)
                                {
                                    editSession.AddOrderModifierItem(amount, item.Product, groupModifier.ProductGroup, newOrder, productStub);
                                    count += amount;
                                    PluginContext.Log.Info($"Added required group modifier {item.Product.Name} (amount: {amount}) for {product.Name}");

                                    // Stop if we've met the minimum requirement and reached max
                                    if (count >= groupModifier.MaximumAmount)
                                        break;
                                }
                            }

                            // Verify we met the minimum requirement
                            if (count < groupModifier.MinimumAmount)
                            {
                                PluginContext.Log.Info($"Warning: Required group modifier {groupModifier.ProductGroup.Name} for {product.Name} needs at least {groupModifier.MinimumAmount} items, but only {count} were added");
                            }
                        }
                        else
                        {
                            // Optional group - only add items with default amounts
                            var itemsToAdd = groupModifier.Items.Where(x => x.DefaultAmount > 0);
                            foreach (var item in itemsToAdd)
                            {
                                editSession.AddOrderModifierItem(item.DefaultAmount, item.Product, groupModifier.ProductGroup, newOrder, productStub);
                                PluginContext.Log.Info($"Added optional group modifier {item.Product.Name} for {product.Name}");
                            }
                        }
                    }
                }
                
                var result = PluginContext.Operations.SubmitChanges(editSession, credentials);
                currentOrder = result.Get(newOrder);
            }
            catch (Exception ex)
            {
                PluginContext.Log.Info("API request failed: " + ex.Message);
                ErrorMessage = ex.Message;
                CurrentScreen = ScreenType.Error;
            }
        }

        private async Task MakeApiRequestAsync()
        {
            // Get all active products (dishes) from menu
            var allProducts = PluginContext.Operations.GetActiveProducts()
                .Where(p => p.Type == ProductType.Dish && p.Template == null)
                .ToList();
            
            Dishes = allProducts;
        }

        private void RepeatScanButton_Click(object sender, RoutedEventArgs e)
        {
            PluginContext.Log.Info("Repeat scan clicked.");
            CurrentScreen = ScreenType.Scanning;
        }

        private void PayButton_Click(object sender, RoutedEventArgs e)
        {
            PluginContext.Log.Info("Pay clicked.");
            CurrentScreen = ScreenType.Payment;
            // Popup will be shown automatically via CurrentScreen setter
        }

        private void ShowPaymentTypesPopup()
        {
            try
            {
                var paymentTypes = PluginContext.Operations.GetPaymentTypes().ToList();
                
                if (paymentTypes.Count == 0)
                {
                    // MessageBox.Show("No payment types available.", "Payment Types", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var paymentTypesList = string.Join("\n", paymentTypes.Select(pt => 
                    $"- {pt.Name} ({pt.Kind})"));
                
                var message = $"Available Payment Types:\n\n{paymentTypesList}";
                // MessageBox.Show(message, "Payment Types", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                PluginContext.Log.Info("Error showing payment types popup: " + ex.Message);
            }
        }

        private async void KaspiButton_Click(object sender, RoutedEventArgs e)
        {
            //PluginContext.Log.Info("Kaspi payment selected.");
            //MessageBox.Show("Оплата через Kaspi успешно выполнена!", "Kaspi", MessageBoxButton.OK, MessageBoxImage.Information);
            await Task.Delay(1000); // short delay to simulate processing
            
            // Close/complete the order with payment (emulating Guest Bill button)
            if (currentOrder != null)
            {
                try
                {
                    var credentials = PluginContext.Operations.GetDefaultCredentials();
                    // Refresh order to get latest state
                    currentOrder = PluginContext.Operations.GetOrderById(currentOrder.Id);
                    
                    // Step 0: Print all order items before billing (required for bill cheque)
                    var itemsToPrint = currentOrder.Items.OfType<IOrderCookingItem>().ToList();
                    if (itemsToPrint.Count > 0)
                    {
                        PluginContext.Operations.PrintOrderItems(currentOrder, itemsToPrint, credentials);
                    }
                    
                    // Step 1: Bill the order first (like Guest Bill button)
                    PluginContext.Operations.BillOrder(currentOrder, 0, credentials);
                    
                    // Refresh order after billing
                    currentOrder = PluginContext.Operations.GetOrderById(currentOrder.Id);
                    
                    // Step 2: Get Kaspi payment type (try to find by name, or use Card type)
                    var paymentType = PluginContext.Operations.GetPaymentTypes()
                        .FirstOrDefault(x => x.Name.ToUpper().Contains("KASPI") || 
                                            (x.Kind == PaymentTypeKind.Card && x.Name.ToUpper().Contains("CARD"))) 
                        ?? PluginContext.Operations.GetPaymentTypes().FirstOrDefault(x => x.Kind == PaymentTypeKind.Card);
                    
                    if (paymentType == null)
                    {
                        PluginContext.Log.Info("No card payment type found, using first available payment type");
                        paymentType = PluginContext.Operations.GetPaymentTypes().FirstOrDefault();
                    }
                    
                    if (paymentType != null)
                    {
                        // Step 3: Add payment item for the full order amount
                        PluginContext.Operations.AddPaymentItem(
                            currentOrder.ResultSum, 
                            null, // or CardPaymentItemAdditionalData if needed
                            paymentType, 
                            currentOrder, 
                            credentials);
                        
                        // Refresh order after adding payment
                        currentOrder = PluginContext.Operations.GetOrderById(currentOrder.Id);
                        
                        // Step 4: Pay the order and pay out on user (marks as paid/closed)
                        PluginContext.Operations.PayOrderAndPayOutOnUser(currentOrder, true, paymentType, currentOrder.ResultSum, credentials);
                        
                        // Step 5: Refresh order to get updated status (should be Closed)
                        currentOrder = PluginContext.Operations.GetOrderById(currentOrder.Id);
                        
                        // Verify order is closed
                        if (currentOrder.Status == OrderStatus.Closed)
                        {
                            PluginContext.Log.Info($"Order billed, paid and completed after Kaspi payment. Status: {currentOrder.Status}");
                        }
                        else
                        {
                            PluginContext.Log.Info($"Order payment processed but status is {currentOrder.Status}, expected Closed");
                        }
                    }
                    else
                    {
                        PluginContext.Log.Info("No payment types available");
                    }
                    
                    currentOrder = null;
                    OrderDishes = null; // Clear order dishes
                }
                catch (Exception ex)
                {
                    PluginContext.Log.Info("Error processing Kaspi payment: " + ex.Message);
                }
            }
            
            CurrentScreen = ScreenType.Final;
            await Task.Delay(1000);
            CurrentScreen = ScreenType.Welcome;
        }

        private async void CashButton_Click(object sender, RoutedEventArgs e)
        {
            //PluginContext.Log.Info("Cash payment selected.");
            //MessageBox.Show("Оплата наличными успешно выполнена!", "Наличные", MessageBoxButton.OK, MessageBoxImage.Information);
            await Task.Delay(1000);
            
            // Close/complete the order with payment (emulating Guest Bill button)
            if (currentOrder != null)
            {
                try
                {
                    var credentials = PluginContext.Operations.GetDefaultCredentials();
                    // Refresh order to get latest state
                    currentOrder = PluginContext.Operations.GetOrderById(currentOrder.Id);
                    
                    // Step 0: Print all order items before billing (required for bill cheque)
                    var itemsToPrint = currentOrder.Items.OfType<IOrderCookingItem>().ToList();
                    if (itemsToPrint.Count > 0)
                    {
                        PluginContext.Operations.PrintOrderItems(currentOrder, itemsToPrint, credentials);
                    }
                    
                    // Step 1: Bill the order first (like Guest Bill button)
                    PluginContext.Operations.BillOrder(currentOrder, 0, credentials);
                    
                    // Refresh order after billing
                    currentOrder = PluginContext.Operations.GetOrderById(currentOrder.Id);
                    
                    // Step 2: Get cash payment type
                    var paymentType = PluginContext.Operations.GetPaymentTypes()
                        .FirstOrDefault(x => x.Kind == PaymentTypeKind.Cash);
                    
                    if (paymentType == null)
                    {
                        PluginContext.Log.Info("No cash payment type found, using first available payment type");
                        paymentType = PluginContext.Operations.GetPaymentTypes().FirstOrDefault();
                    }
                    
                    if (paymentType != null)
                    {
                        // Step 3: Add payment item for the full order amount
                        PluginContext.Operations.AddPaymentItem(
                            currentOrder.ResultSum, 
                            null, 
                            paymentType, 
                            currentOrder, 
                            credentials);
                        
                        // Refresh order after adding payment
                        currentOrder = PluginContext.Operations.GetOrderById(currentOrder.Id);
                        
                        // Step 4: Pay the order and pay out on user (marks as paid/closed)
                        PluginContext.Operations.PayOrderAndPayOutOnUser(currentOrder, true, paymentType, currentOrder.ResultSum, credentials);
                        
                        // Step 5: Refresh order to get updated status (should be Closed)
                        currentOrder = PluginContext.Operations.GetOrderById(currentOrder.Id);
                        
                        // Verify order is closed
                        if (currentOrder.Status == OrderStatus.Closed)
                        {
                            PluginContext.Log.Info($"Order billed, paid and completed after Cash payment. Status: {currentOrder.Status}");
                        }
                        else
                        {
                            PluginContext.Log.Info($"Order payment processed but status is {currentOrder.Status}, expected Closed");
                        }
                    }
                    else
                    {
                        PluginContext.Log.Info("No payment types available");
                    }
                    
                    currentOrder = null;
                    OrderDishes = null; // Clear order dishes
                }
                catch (Exception ex)
                {
                    PluginContext.Log.Info("Error processing Cash payment: " + ex.Message);
                }
            }
            
            CurrentScreen = ScreenType.Final;
            await Task.Delay(1000);
            CurrentScreen = ScreenType.Welcome;
        }

        public class User
        {
            public int id { get; set; }
            public string email { get; set; }
            public string first_name { get; set; }
            public string last_name { get; set; }

            public override string ToString() => $"{first_name} {last_name} ({email})";
        }

        public class UserResponse
        {
            public List<User> data { get; set; }
        }
    }

    public class ScreenTemplateSelector : System.Windows.Controls.DataTemplateSelector
    {
        public System.Windows.DataTemplate LanguageSelectorTemplate { get; set; }
        public System.Windows.DataTemplate ScanningTemplate { get; set; }
        public System.Windows.DataTemplate LoadingTemplate { get; set; }
        public System.Windows.DataTemplate SuccessTemplate { get; set; }
        public System.Windows.DataTemplate PaymentTemplate { get; set; }
        public System.Windows.DataTemplate ErrorTemplate { get; set; }
        public System.Windows.DataTemplate FinalTemplate { get; set; }

        public override System.Windows.DataTemplate SelectTemplate(object item, System.Windows.DependencyObject container)
        {
            if (item is ScreenType screenType)
            {
                switch (screenType)
                {
                    case ScreenType.Welcome:
                        return LanguageSelectorTemplate;
                    case ScreenType.Scanning:
                        return ScanningTemplate;
                    case ScreenType.Loading:
                        return LoadingTemplate;
                    case ScreenType.Success:
                        return SuccessTemplate;
                    case ScreenType.Payment:
                        return PaymentTemplate;
                    case ScreenType.Error:
                        return ErrorTemplate;
                    case ScreenType.Final:
                        return FinalTemplate;
                }
            }
            return base.SelectTemplate(item, container);
        }
    }

    public enum LanguageEnum
    {
        Russian,
        Kazakh
    }

    public enum ScreenType
    {
        Welcome,
        Scanning,
        Loading,
        Success,
        Error,
        Payment,
        Final
    }
}
