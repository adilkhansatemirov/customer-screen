using Resto.Front.Api.Data.Assortment;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Resto.Front.Api.CustomerScreen.Helpers
{
    public class DishMappingResult
    {
        public IProduct Product { get; set; }
        public IProductSize Size { get; set; }
        public string DisplayName { get; set; }
        public string ModifierName { get; set; } // For group modifiers
        public Guid? ModifierProductId { get; set; } // For group modifiers by ID
    }

    public static class DishMappingHelper
    {
        private static Dictionary<string, Func<List<IProduct>, Func<IProductScale, IEnumerable<IProductSize>>, DishMappingResult>> mappingRules;

        static DishMappingHelper()
        {
            mappingRules = new Dictionary<string, Func<List<IProduct>, Func<IProductScale, IEnumerable<IProductSize>>, DishMappingResult>>(StringComparer.OrdinalIgnoreCase)
            {
                // Simple dish mappings (no size) - using IDs from logs
                { "ayran", (dishes, getSizes) => MapDishById(dishes, Guid.Parse("a38062e8-5f25-43b8-aa95-0025f1a6bf07")) },
                { "belyshi", (dishes, getSizes) => MapDishById(dishes, Guid.Parse("59c65b1f-f29a-4492-ad78-2ed1cc270b68")) },
                { "bouillon", (dishes, getSizes) => MapDishById(dishes, Guid.Parse("efb06fd7-e133-48da-ad7e-b7593d28e41b")) },
                { "bread", (dishes, getSizes) => MapDishById(dishes, Guid.Parse("8b8e3b94-84df-4ee9-a4e0-534fb261b9a9")) },
                { "cake", (dishes, getSizes) => MapDishById(dishes, Guid.Parse("22b4ac6b-beac-4809-8158-dc365799052f")) },
                { "carcade", (dishes, getSizes) => MapDishById(dishes, Guid.Parse("ceae78c3-6ce1-46d3-bd9c-8797f5e2f352")) },
                { "ceazer", (dishes, getSizes) => MapDishById(dishes, Guid.Parse("c15024b4-af9a-4033-9a72-5b29f63a06a5")) },
                { "cheesecake", (dishes, getSizes) => MapDishById(dishes, Guid.Parse("c2cf76ff-2944-49fd-926e-ab1f0c4241cc")) },
                { "chicken no garnish", (dishes, getSizes) => MapDishById(dishes, Guid.Parse("9a4e55bf-2a6d-45ba-9490-8ee31051ee75")) },
                { "compote", (dishes, getSizes) => MapDishById(dishes, Guid.Parse("e2bdc4a2-de20-453c-a1e3-2d69f1e47da2")) },
                { "dizzy canned", (dishes, getSizes) => MapDishById(dishes, Guid.Parse("92c24cbe-239b-4354-bca5-88e8722b29d1")) },
                { "garnish", (dishes, getSizes) => MapDishById(dishes, Guid.Parse("e488beb7-3350-496b-9fc5-2a68d9f469ee")) },
                { "golubets bouillon", (dishes, getSizes) => MapDishById(dishes, Guid.Parse("25cb00fe-d91d-4abf-9e02-4a9ec9e3ded3")) },
                { "gorilla", (dishes, getSizes) => MapDishById(dishes, Guid.Parse("129ce656-eb3d-4f26-9405-e3011f64a30a")) },
                { "manty", (dishes, getSizes) => MapDishById(dishes, Guid.Parse("cf925541-6dbf-46ac-afee-957cf0f7f801")) },
                { "meat no garnish", (dishes, getSizes) => MapDishById(dishes, Guid.Parse("ddb4551a-c982-4a2d-b109-3ffd431c0d06")) },
                { "medovic", (dishes, getSizes) => MapDishById(dishes, Guid.Parse("da5ffdd3-1b1b-444f-bef7-8f89af25d434")) },
                { "olivier", (dishes, getSizes) => MapDishById(dishes, Guid.Parse("7d9ab24e-5924-4177-afbd-b628e7872dec")) },
                { "pelmeni", (dishes, getSizes) => MapDishById(dishes, Guid.Parse("5a95b5d4-d54f-40a2-8284-6ab5a7fb5721")) },
                { "pirozhok", (dishes, getSizes) => MapDishById(dishes, Guid.Parse("5b31c967-0e1b-46ee-a5b3-931ee3816568")) },
                { "plov", (dishes, getSizes) => MapDishById(dishes, Guid.Parse("b32296f4-8bef-46de-a55c-57882c90b115")) },
                { "poacha", (dishes, getSizes) => MapDishById(dishes, Guid.Parse("0c974758-6881-4597-82ae-ed042a5be136")) },
                { "quyrdak", (dishes, getSizes) => MapDishById(dishes, Guid.Parse("d8ddcd8c-923b-415e-b539-69dcb451db67")) },
                { "rolled bread", (dishes, getSizes) => MapDishById(dishes, Guid.Parse("ad4269c3-5a61-497c-bfae-bfe05741a663")) },
                { "salad", (dishes, getSizes) => MapDishById(dishes, Guid.Parse("29224811-09b5-44b6-a581-f7fd2f853a42")) },
                { "sausage with dough", (dishes, getSizes) => MapDishById(dishes, Guid.Parse("1d4fac8b-0e0c-4e3d-b382-f026b00eb900")) },
                { "soup", (dishes, getSizes) => MapDishById(dishes, Guid.Parse("4409cac9-1776-4b53-bcf2-f82ab5c5e350")) },
                { "tsoman", (dishes, getSizes) => MapDishById(dishes, Guid.Parse("8d103a09-a6fa-4b94-93cc-edb036ec3f79")) },
                { "vareniki", (dishes, getSizes) => MapDishById(dishes, Guid.Parse("55ea063d-3e6d-4862-9af1-00252c1d0d41")) },

                // Baklava
                { "baklava", (dishes, getSizes) => MapDishById(dishes, Guid.Parse("71f24063-2e2e-4d14-87b6-c5330a21bc0a")) },
                { "baklava long", (dishes, getSizes) => MapDishById(dishes, Guid.Parse("71f24063-2e2e-4d14-87b6-c5330a21bc0a")) },

                // Chicken Garnish
                { "chicken garnish", (dishes, getSizes) => MapDishById(dishes, Guid.Parse("70d4f74a-09cc-4140-ad0b-564d602548ab")) },
                { "chicken garnish half", (dishes, getSizes) => MapDishById(dishes, Guid.Parse("156732f7-06e7-409c-bbed-a39f8cca00c4")) },

                // Meat Garnish
                { "meat garnish", (dishes, getSizes) => MapDishById(dishes, Guid.Parse("c0155373-0cf6-46e4-a2da-ec6a049573c1")) },
                { "meat garnish half", (dishes, getSizes) => MapDishById(dishes, Guid.Parse("d14b354d-be6b-4591-bc08-c11cd7faea00")) },

                // Coffee
                { "coffee", (dishes, getSizes) => MapDishById(dishes, Guid.Parse("4e6ba5d3-28c6-4203-8263-dea4129bebb1")) },
                { "coffee 3 in 1", (dishes, getSizes) => MapDishById(dishes, Guid.Parse("97a68dc6-9522-4919-afed-b2fc0762e7fd")) },

                // Cola
                { "cola 0.3", (dishes, getSizes) => MapDishById(dishes, Guid.Parse("c1a6f8a7-305b-4020-9014-b3a6126c0859")) },
                { "cola 0.5", (dishes, getSizes) => MapDishById(dishes, Guid.Parse("c1a6f8a7-305b-4020-9014-b3a6126c0859")) },
                { "cola 1l", (dishes, getSizes) => MapDishById(dishes, Guid.Parse("c1a6f8a7-305b-4020-9014-b3a6126c0859")) },
                { "cola 1l zero", (dishes, getSizes) => MapDishById(dishes, Guid.Parse("c1a6f8a7-305b-4020-9014-b3a6126c0859")) },

                // Fanta
                { "fanta", (dishes, getSizes) => MapDishById(dishes, Guid.Parse("9bdb9e1e-21d7-4c10-9dc3-1b0d20abf9ed")) },
                { "fanta 0.3", (dishes, getSizes) => MapDishById(dishes, Guid.Parse("9bdb9e1e-21d7-4c10-9dc3-1b0d20abf9ed")) },

                // Fuse (separate products per size in menu)
                { "fuse 0.3", (dishes, getSizes) => MapDishById(dishes, Guid.Parse("ba61bcb9-00a8-45f1-8ea9-914d7f9318f2")) },
                { "fuse 0.5", (dishes, getSizes) => MapDishById(dishes, Guid.Parse("a4020ce9-b64b-485b-b04b-6bc67881a75b")) },
                { "fuse 1l", (dishes, getSizes) => MapDishById(dishes, Guid.Parse("50dc11b2-1ee0-4367-b4ae-0cc1aa12a920")) },

                // Lemonade
                { "lemonade", (dishes, getSizes) => MapDishById(dishes, Guid.Parse("291d97ef-aa53-4b82-9604-f3f3c7b8195f")) },
                { "lemonade glass", (dishes, getSizes) => MapDishById(dishes, Guid.Parse("d8a57b3a-8d0d-4f4b-99ba-8e2e65e62f65")) },

                // Maxi Tea
                { "maxi tea", (dishes, getSizes) => MapDishWithSizeById(dishes, getSizes, Guid.Parse("b31cbfaf-3330-4c16-ada3-0a4dbdbf6d0a"), Guid.Parse("dbf111b3-5ed6-457a-9c75-3dcba20d958b")) },
                { "maxi tea 1.2", (dishes, getSizes) => MapDishById(dishes, Guid.Parse("eb821282-6e71-4ed3-a3e6-65220501b0c4")) },

                // Tea
                { "tea", (dishes, getSizes) => MapDishById(dishes, Guid.Parse("ecdfc085-6137-4f08-a69f-5327876f05a6")) },
                { "tea green", (dishes, getSizes) => MapDishById(dishes, Guid.Parse("dede55fd-7e7c-419b-bbfe-b2d4042fadb9")) },
                { "tea teapot", (dishes, getSizes) => MapDishById(dishes, Guid.Parse("60719545-4741-4ec7-a897-5cc9330db7ab")) },

                // Samsa (separate products in menu)
                { "samsa cheese", (dishes, getSizes) => MapDishById(dishes, Guid.Parse("657f6b5e-95e9-4561-bd0f-a2925ab03c91")) },
                { "samsa chicken", (dishes, getSizes) => MapDishById(dishes, Guid.Parse("83556e08-520c-41d4-a5c6-f99512576358")) },
                { "samsa meat", (dishes, getSizes) => MapDishById(dishes, Guid.Parse("850ae161-8fe3-472b-911e-edcdbf94573e")) },

                // Tandyr Samsa (separate products in menu)
                { "tandyr samsa chicken", (dishes, getSizes) => MapDishById(dishes, Guid.Parse("f15362c7-a00e-4390-acfd-bb7c1d9581e7")) },
                { "tandyr samsa meat", (dishes, getSizes) => MapDishById(dishes, Guid.Parse("93819595-cfb7-40ac-9fc5-474670e096e6")) },
                { "tamdyr samsa meat", (dishes, getSizes) => MapDishById(dishes, Guid.Parse("93819595-cfb7-40ac-9fc5-474670e096e6")) },

                // Doner dishes
                { "донер в батоне", (dishes, getSizes) => MapDishById(dishes, Guid.Parse("86f003ae-e62d-4ad2-a1cc-9b80573c9e93")) },
                { "донер в лаваше", (dishes, getSizes) => MapDishById(dishes, Guid.Parse("ff1d6fe8-f6e8-4e6a-ac55-19ee8b9e7a55")) },

                // Canned drinks - using Dizzy canned with Canned size
                { "canned 0.45", (dishes, getSizes) => MapDishWithSizeById(dishes, getSizes, Guid.Parse("92c24cbe-239b-4354-bca5-88e8722b29d1"), Guid.Parse("d2a91ca6-efd2-4b2a-8a01-5724c2c2df58")) },
            };
        }

        private static DishMappingResult MapDishById(List<IProduct> dishes, Guid dishId)
        {
            var dish = dishes.FirstOrDefault(d => d.Id == dishId);
            if (dish == null)
                return null;

            return new DishMappingResult
            {
                Product = dish,
                Size = null,
                DisplayName = dish.Name
            };
        }

        private static DishMappingResult MapDishWithSizeById(List<IProduct> dishes, Func<IProductScale, IEnumerable<IProductSize>> getSizes, Guid dishId, Guid sizeId)
        {
            var dish = dishes.FirstOrDefault(d => d.Id == dishId);
            if (dish == null)
                return null;

            IProductSize size = null;
            if (dish.Scale != null && getSizes != null)
            {
                var allSizes = getSizes(dish.Scale).ToList();
                size = allSizes.FirstOrDefault(s => s.Id == sizeId);
            }

            return new DishMappingResult
            {
                Product = dish,
                Size = size,
                DisplayName = size != null ? $"{dish.Name} ({size.Name})" : dish.Name
            };
        }

        private static DishMappingResult MapDishWithModifierById(List<IProduct> dishes, Guid dishId, Guid modifierProductId)
        {
            var dish = dishes.FirstOrDefault(d => d.Id == dishId);
            if (dish == null)
                return null;

            // Find modifier product name for display
            var modifierProduct = dishes.FirstOrDefault(d => d.Id == modifierProductId);
            var modifierName = modifierProduct?.Name ?? modifierProductId.ToString();

            return new DishMappingResult
            {
                Product = dish,
                Size = null,
                DisplayName = $"{dish.Name} ({modifierName})",
                ModifierName = modifierName,
                ModifierProductId = modifierProductId
            };
        }

        // Legacy methods for backward compatibility (using names)
        private static DishMappingResult MapDish(List<IProduct> dishes, string dishName)
        {
            var dish = dishes.FirstOrDefault(d => d.Name.Equals(dishName, StringComparison.OrdinalIgnoreCase));
            if (dish == null)
                return null;

            return new DishMappingResult
            {
                Product = dish,
                Size = null,
                DisplayName = dishName
            };
        }

        private static DishMappingResult MapDishWithSize(List<IProduct> dishes, Func<IProductScale, IEnumerable<IProductSize>> getSizes, string dishName, string sizeName)
        {
            var dish = dishes.FirstOrDefault(d => d.Name.Equals(dishName, StringComparison.OrdinalIgnoreCase));
            if (dish == null)
                return null;

            IProductSize size = null;
            if (dish.Scale != null && getSizes != null)
            {
                var allSizes = getSizes(dish.Scale).ToList();
                size = allSizes.FirstOrDefault(s => s.Name.Equals(sizeName, StringComparison.OrdinalIgnoreCase));
            }

            return new DishMappingResult
            {
                Product = dish,
                Size = size,
                DisplayName = $"{dishName} ({sizeName})"
            };
        }

        private static DishMappingResult MapDishWithModifier(List<IProduct> dishes, string dishName, string modifierName)
        {
            var dish = dishes.FirstOrDefault(d => d.Name.Equals(dishName, StringComparison.OrdinalIgnoreCase));
            if (dish == null)
                return null;

            return new DishMappingResult
            {
                Product = dish,
                Size = null,
                DisplayName = $"{dishName} ({modifierName})",
                ModifierName = modifierName
            };
        }

        public static DishMappingResult MapStringToDish(string value, List<IProduct> allDishes, Func<IProductScale, IEnumerable<IProductSize>> getSizes)
        {
            if (string.IsNullOrWhiteSpace(value) || allDishes == null || allDishes.Count == 0)
                return null;

            // Try exact match first
            if (mappingRules.ContainsKey(value))
            {
                return mappingRules[value](allDishes, getSizes);
            }

            // Try case-insensitive match
            var key = mappingRules.Keys.FirstOrDefault(k => k.Equals(value, StringComparison.OrdinalIgnoreCase));
            if (key != null)
            {
                return mappingRules[key](allDishes, getSizes);
            }

            // Fallback: try to find by name (partial match)
            var dish = allDishes.FirstOrDefault(d => 
                d.Name.Equals(value, StringComparison.OrdinalIgnoreCase) ||
                d.Name.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0 ||
                value.IndexOf(d.Name, StringComparison.OrdinalIgnoreCase) >= 0);

            if (dish != null)
            {
                return new DishMappingResult
                {
                    Product = dish,
                    Size = null,
                    DisplayName = dish.Name
                };
            }

            return null;
        }

        public static List<string> GetAllMappableValues()
        {
            return mappingRules.Keys.ToList();
        }
    }
}
