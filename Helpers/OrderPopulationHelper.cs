using Newtonsoft.Json;
using Resto.Front.Api.Data.Assortment;
using Resto.Front.Api.Data.Orders;
using Resto.Front.Api.Data.Security;
using Resto.Front.Api.Editors;
using Resto.Front.Api.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Resto.Front.Api;

namespace Resto.Front.Api.CustomerScreen.Helpers
{
    public static class OrderPopulationHelper
    {
        private const string MenuApiUrl = "http://172.20.10.3:8081/scan";

        private static readonly HttpClient MenuHttpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(15)
        };

        private sealed class MenuApiResponseDto
        {
            [JsonProperty("items")]
            public List<MenuApiItemDto> Items { get; set; }
        }

        private sealed class MenuApiItemDto
        {
            [JsonProperty("name")]
            public string Name { get; set; }
        }

        // TEMP: pool for random mock scan responses (same strings as expected ForeignName / API names)
        //private static readonly object MockScanRngLock = new object();
        //private static readonly Random MockScanRng = new Random();

        //private static readonly string[] MockScanCatalog =
        //{
        //    "baklava",
        //    "baklava sarma",
        //    "belyshi",
        //    "bouillon",
        //    "bread",
        //    "cake baklava european nut",
        //    "cake black forest",
        //    "cake cheesecake",
        //    "cake chocolate",
        //    "cake curd",
        //    "cake curd cranberry",
        //    "cake curd lemon",
        //    "cake medovik",
        //    "cake medovik homemade",
        //    "cake medovik nuts",
        //    "cake milk girl",
        //    "cake napoleon square",
        //    "cake napoleon triangle",
        //    "ceazer",
        //    "chicken garnish",
        //    "chicken garnish half",
        //    "chicken no garnish",
        //    "chicken zapekanka",
        //    "coffee",
        //    "coffee 3 in 1",
        //    "drink arzu 0.5",
        //    "drink arzu 1",
        //    "drink ayran",
        //    "drink carcade",
        //    "drink cola 0.3",
        //    "drink cola 0.5",
        //    "drink cola 1",
        //    "drink cola zero 1",
        //    "drink compote",
        //    "drink dizzy canned",
        //    "drink dizzy glass",
        //    "drink fanta 0.3",
        //    "drink fanta 0.5",
        //    "drink fanta 1",
        //    "drink fest berry 0.5",
        //    "drink fest berry 1",
        //    "drink fest berry canned",
        //    "drink fuse 0.3",
        //    "drink fuse 0.5",
        //    "drink fuse 1",
        //    "drink garden",
        //    "drink gorilla",
        //    "drink granat riks",
        //    "drink happy lemonade",
        //    "drink ice tea riks 0.5",
        //    "drink ice tea riks 1",
        //    "drink kvas",
        //    "drink lemonade glass riks",
        //    "drink lemonade glass zlatoyar",
        //    "drink lemonade plastic zlatoyar",
        //    "drink lime time 0.5",
        //    "drink lime time 1",
        //    "drink maxi tea 0.5",
        //    "drink maxi tea 1",
        //    "drink maxi tea 2",
        //    "drink mohito ochakovo",
        //    "drink mohito riks",
        //    "drink piko 1",
        //    "drink sevens water",
        //    "drink sprite 0.5",
        //    "drink tassay 1",
        //    "drink zet",
        //    "garnish",
        //    "golubets bouillon",
        //    "manty",
        //    "manty half",
        //    "meat garnish",
        //    "meat garnish half",
        //    "meat no garnish",
        //    "milk tea",
        //    "olivier",
        //    "pegodi round",
        //    "pelmeni",
        //    "pepsi 0.5",
        //    "pirozhok",
        //    "plov",
        //    "plov half",
        //    "poacha",
        //    "quyrdak",
        //    "rolled bread",
        //    "salad",
        //    "samsa cheese",
        //    "samsa chicken",
        //    "samsa meat",
        //    "sausage with dough",
        //    "soup",
        //    "tandyr samsa chicken",
        //    "tandyr samsa meat",
        //    "tea bag",
        //    "tea black",
        //    "tea green",
        //    "tea teapot",
        //    "tea with milk",
        //    "tsoman",
        //    "vareniki",
        //};

        /// <summary>
        /// GET menu endpoint; returns dish name strings from <c>items[].name</c>.
        /// On failure logs and returns an empty list.
        /// </summary>
        public static List<string> FetchMenuItemNamesFromApi()
        {
            // TEMP: mock scan — random 6–7 picks from catalog each call (uncomment HTTP block below for real API)
            //List<string> mockPicks;
            //lock (MockScanRngLock)
            //{
            //    var pickCount = MockScanRng.Next(6, 8); // 6 or 7
            //    mockPicks = MockScanCatalog
            //        .OrderBy(_ => MockScanRng.Next())
            //        .Take(pickCount)
            //        .ToList();
            //}

            //PluginContext.Log.Info(
            //    $"FetchMenuItemNamesFromApi: mock ({mockPicks.Count} items, HTTP disabled): {string.Join(", ", mockPicks)}");
            //return mockPicks;

            
            try
            {
                var json = MenuHttpClient.GetStringAsync(MenuApiUrl).GetAwaiter().GetResult();
                var dto = JsonConvert.DeserializeObject<MenuApiResponseDto>(json);
                if (dto?.Items == null)
                    return new List<string>();
                return dto.Items
                    .Where(i => i != null && !string.IsNullOrWhiteSpace(i.Name))
                    .Select(i => i.Name.Trim())
                    .ToList();
            }
            catch (Exception ex)
            {
                PluginContext.Log.Info("Menu API GET failed: " + ex.Message);
                return new List<string>();
            }
            
        }

        /// <summary>
        /// Collects all dish products from the hierarchical menu (all categories).
        /// When logDishes is true, logs every dish Id, Name, ForeignName (Название на иностранном языке), and category path (use only on plugin load).
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
                            PluginContext.Log.Info($"[Dish] Id={p.Id}, Name=\"{p.Name}\", ForeignName=\"{p.ForeignName}\", Category=\"{categoryPath}\"");
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

        /// <summary>
        /// Gets dishes, loads item names from the menu HTTP API, maps to products, and adds them to the given order.
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

            var apiResponseItems = FetchMenuItemNamesFromApi();
            PluginContext.Log.Info($"Menu API returned {apiResponseItems.Count} items: {string.Join(", ", apiResponseItems)}");

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
            PluginContext.Log.Info($"Added {mappedDishes.Count} dishes from menu API to order.");
            return true;
        }
    }
}
