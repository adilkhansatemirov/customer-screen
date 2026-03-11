using Resto.Front.Api.Data.Assortment;
using Resto.Front.Api.Data.Orders;
using Resto.Front.Api.Data.Security;
using Resto.Front.Api.Editors;
using Resto.Front.Api.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using Resto.Front.Api;

namespace Resto.Front.Api.CustomerScreen.Helpers
{
    public static class OrderPopulationHelper
    {
        /// <summary>
        /// Collects all dish products from the hierarchical menu (all categories).
        /// When logDishes is true, logs every dish Id, Name, and category path (use only on plugin load).
        /// </summary>
        public static List<IProduct> GetDishesFromAllCategories(bool logDishes = false)
        {
            var menu = PluginContext.Operations.GetHierarchicalMenu();
            var seenIds = new HashSet<Guid>();
            var result = new List<IProduct>();

            void CollectFromProducts(IEnumerable<IProduct> products, string categoryPath)
            {
                foreach (var p in products ?? Enumerable.Empty<IProduct>())
                {
                    if (p.Type != ProductType.Dish || p.Template != null)
                        continue;
                    if (seenIds.Add(p.Id))
                    {
                        result.Add(p);
                        if (logDishes)
                            PluginContext.Log.Info($"[Dish] Id={p.Id}, Name=\"{p.Name}\", Category=\"{categoryPath}\"");
                    }
                }
            }

            void CollectFromGroup(IProductGroup group, string categoryPath)
            {
                var path = string.IsNullOrEmpty(categoryPath) ? group.Name : categoryPath + " / " + group.Name;
                var childProducts = PluginContext.Operations.GetChildProductsByProductGroup(group);
                var childGroups = PluginContext.Operations.GetChildGroupsByProductGroup(group);
                CollectFromProducts(childProducts, path);
                foreach (var child in childGroups ?? Enumerable.Empty<IProductGroup>())
                    CollectFromGroup(child, path);
            }

            CollectFromProducts(menu.Products, "(root)");
            foreach (var group in menu.ProductGroups ?? Enumerable.Empty<IProductGroup>())
                CollectFromGroup(group, "");

            if (logDishes)
                PluginContext.Log.Info($"[Dishes] Total from all categories: {result.Count}. All IDs: {string.Join(", ", result.Select(p => p.Id))}");
            return result;
        }

        public static List<IProduct> GetDishes()
        {
            return GetDishesFromAllCategories(logDishes: false);
        }

        public static List<string> EmulateApiResponse()
        {
            var allPossibleItems = new List<string>
            {
                "ayran", "baklava", "baklava long", "belyshi", "bouillon", "bread", "cake",
                "canned 0.45", "carcade", "ceazer", "cheesecake", "chicken garnish",
                "chicken garnish half", "chicken no garnish", "coffee", "coffee 3 in 1",
                "cola 0.3", "cola 0.5", "cola 1l", "cola 1l zero", "compote", "dizzy canned",
                "fanta", "fanta 0.3", "fuse 0.3", "fuse 0.5", "fuse 1l", "garnish",
                "golubets bouillon", "gorilla", "lemonade", "lemonade glass", "manty",
                "maxi tea", "maxi tea 1.2", "meat garnish", "meat garnish half",
                "meat no garnish", "medovic", "milk tea", "olivier", "pelmeni", "pepsi 0.5",
                "pirozhok", "plov", "poacha", "quyrdak", "rolled bread", "salad",
                "samsa cheese", "samsa chicken", "samsa meat", "sausage with dough",
                "soup", "tamdyr samsa meat", "tandyr samsa chicken", "tandyr samsa meat",
                "tea", "tea green", "tea teapot", "tsoman", "vareniki", "water 0.5"
            };

            var random = new Random();
            var count = random.Next(1, 11);
            return allPossibleItems.OrderBy(x => random.Next()).Take(count).ToList();
        }

        /// <summary>
        /// Gets dishes, emulates API response, maps to products, and adds them to the given order.
        /// Uses the first guest of the order. Creates an edit session, adds items, then submits.
        /// </summary>
        /// <returns>True if items were added and submitted; false if no dishes, no mapping, or no guest.</returns>
        public static bool PopulateOrderWithEmulatedDishes(IOrder order, IOperationService operations, ICredentials credentials)
        {
            var editSession = operations.CreateEditSession();
            var allProducts = GetDishes();
            if (allProducts == null || allProducts.Count == 0)
            {
                PluginContext.Log.Info("No products available in menu");
                return false;
            }

            var guest = order.Guests.FirstOrDefault();
            if (guest == null)
            {
                PluginContext.Log.Info("Order has no guests");
                return false;
            }

            var apiResponseItems = EmulateApiResponse();
            PluginContext.Log.Info($"Emulated API response returned {apiResponseItems.Count} items: {string.Join(", ", apiResponseItems)}");

            Func<IProductScale, IEnumerable<IProductSize>> getSizes = scale => PluginContext.Operations.GetProductScaleSizes(scale);
            var mappedDishes = new List<DishMappingResult>();
            foreach (var item in apiResponseItems)
            {
                var mapped = DishMappingHelper.MapStringToDish(item, allProducts, getSizes);
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
                return false;
            }

            foreach (var mappedDish in mappedDishes)
            {
                var product = mappedDish.Product;
                var size = mappedDish.Size;

                if (product.Scale != null && size == null)
                {
                    size = product.Scale.DefaultSize;
                    if (size == null)
                    {
                        var availableSizes = PluginContext.Operations.GetProductScaleSizes(product.Scale)
                            .Except(PluginContext.Operations.GetDisabledSizesByProduct(product))
                            .ToList();
                        if (availableSizes.Count > 0)
                            size = availableSizes.FirstOrDefault();
                        if (size == null)
                            PluginContext.Log.Info($"Warning: Product {product.Name} has scale but no sizes available");
                    }
                }

                var productStub = editSession.AddOrderProductItem(1m, product, order, guest, size);

                var groupModifiers = product.GetGroupModifiers(null);
                foreach (var groupModifier in groupModifiers)
                {
                    if (mappedDish.ModifierProductId.HasValue)
                    {
                        var modifierItem = groupModifier.Items.FirstOrDefault(i => i.Product.Id == mappedDish.ModifierProductId.Value);
                        if (modifierItem != null)
                        {
                            var amount = Math.Max(1, modifierItem.MinimumAmount);
                            if (amount == 0) amount = 1;
                            editSession.AddOrderModifierItem(amount, modifierItem.Product, groupModifier.ProductGroup, order, productStub);
                            continue;
                        }
                    }
                    if (!string.IsNullOrEmpty(mappedDish.ModifierName))
                    {
                        var modifierItem = groupModifier.Items.FirstOrDefault(i =>
                            i.Product.Name.Equals(mappedDish.ModifierName, StringComparison.OrdinalIgnoreCase));
                        if (modifierItem != null)
                        {
                            var amount = Math.Max(1, modifierItem.MinimumAmount);
                            if (amount == 0) amount = 1;
                            editSession.AddOrderModifierItem(amount, modifierItem.Product, groupModifier.ProductGroup, order, productStub);
                            continue;
                        }
                    }

                    if (groupModifier.MinimumAmount > 0)
                    {
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
                                editSession.AddOrderModifierItem(amount, item.Product, groupModifier.ProductGroup, order, productStub);
                                count += amount;
                                if (count >= groupModifier.MaximumAmount)
                                    break;
                            }
                        }
                    }
                    else
                    {
                        foreach (var item in groupModifier.Items.Where(x => x.DefaultAmount > 0))
                        {
                            editSession.AddOrderModifierItem(item.DefaultAmount, item.Product, groupModifier.ProductGroup, order, productStub);
                        }
                    }
                }
            }

            operations.SubmitChanges(editSession, credentials);
            PluginContext.Log.Info($"Added {mappedDishes.Count} emulated dishes to order.");
            return true;
        }
    }
}
