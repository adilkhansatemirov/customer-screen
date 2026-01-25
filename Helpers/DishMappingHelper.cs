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

                // Baklava with sizes
                { "baklava", (dishes, getSizes) => MapDishWithSizeById(dishes, getSizes, Guid.Parse("71f24063-2e2e-4d14-87b6-c5330a21bc0a"), Guid.Parse("d81d684e-35a3-4394-a3da-415244c7ddc0")) },
                { "baklava long", (dishes, getSizes) => MapDishWithSizeById(dishes, getSizes, Guid.Parse("71f24063-2e2e-4d14-87b6-c5330a21bc0a"), Guid.Parse("06e9f4c7-7e57-4ecb-8e92-6ca57c7fc84f")) },

                // Chicken Garnish with sizes
                { "chicken garnish", (dishes, getSizes) => MapDishWithSizeById(dishes, getSizes, Guid.Parse("70d4f74a-09cc-4140-ad0b-564d602548ab"), Guid.Parse("07137b5f-4475-498a-88b2-4a5259df55d9")) },
                { "chicken garnish half", (dishes, getSizes) => MapDishWithSizeById(dishes, getSizes, Guid.Parse("70d4f74a-09cc-4140-ad0b-564d602548ab"), Guid.Parse("f9c8ee71-0521-4eb2-b854-d7220f0ad37e")) },

                // Meat Garnish with sizes
                { "meat garnish", (dishes, getSizes) => MapDishWithSizeById(dishes, getSizes, Guid.Parse("c0155373-0cf6-46e4-a2da-ec6a049573c1"), Guid.Parse("07137b5f-4475-498a-88b2-4a5259df55d9")) },
                { "meat garnish half", (dishes, getSizes) => MapDishWithSizeById(dishes, getSizes, Guid.Parse("c0155373-0cf6-46e4-a2da-ec6a049573c1"), Guid.Parse("f9c8ee71-0521-4eb2-b854-d7220f0ad37e")) },

                // Coffee with modifiers
                { "coffee", (dishes, getSizes) => MapDishById(dishes, Guid.Parse("4e6ba5d3-28c6-4203-8263-dea4129bebb1")) },
                { "coffee 3 in 1", (dishes, getSizes) => MapDishWithModifierById(dishes, Guid.Parse("4e6ba5d3-28c6-4203-8263-dea4129bebb1"), Guid.Parse("fde67fac-0a45-43ec-ae7d-3d585420412f")) },

                // Cola with sizes
                { "cola 0.3", (dishes, getSizes) => MapDishWithSizeById(dishes, getSizes, Guid.Parse("c1a6f8a7-305b-4020-9014-b3a6126c0859"), Guid.Parse("8592a3ba-bfc2-4d09-8bbb-43b61d04fa76")) },
                { "cola 0.5", (dishes, getSizes) => MapDishWithSizeById(dishes, getSizes, Guid.Parse("c1a6f8a7-305b-4020-9014-b3a6126c0859"), Guid.Parse("dbf111b3-5ed6-457a-9c75-3dcba20d958b")) },
                { "cola 1l", (dishes, getSizes) => MapDishWithSizeById(dishes, getSizes, Guid.Parse("c1a6f8a7-305b-4020-9014-b3a6126c0859"), Guid.Parse("fb0e24f1-873b-4b72-988f-d1242ee95cbb")) },
                { "cola 1l zero", (dishes, getSizes) => MapDishWithSizeById(dishes, getSizes, Guid.Parse("c1a6f8a7-305b-4020-9014-b3a6126c0859"), Guid.Parse("d54c6bbe-0c16-4015-93fb-8dc3ad145d83")) },

                // Fanta with sizes
                { "fanta", (dishes, getSizes) => MapDishWithSizeById(dishes, getSizes, Guid.Parse("9bdb9e1e-21d7-4c10-9dc3-1b0d20abf9ed"), Guid.Parse("dbf111b3-5ed6-457a-9c75-3dcba20d958b")) },
                { "fanta 0.3", (dishes, getSizes) => MapDishWithSizeById(dishes, getSizes, Guid.Parse("9bdb9e1e-21d7-4c10-9dc3-1b0d20abf9ed"), Guid.Parse("8592a3ba-bfc2-4d09-8bbb-43b61d04fa76")) },

                // Fuse with sizes
                { "fuse 0.3", (dishes, getSizes) => MapDishWithSizeById(dishes, getSizes, Guid.Parse("50dc11b2-1ee0-4367-b4ae-0cc1aa12a920"), Guid.Parse("8592a3ba-bfc2-4d09-8bbb-43b61d04fa76")) },
                { "fuse 0.5", (dishes, getSizes) => MapDishWithSizeById(dishes, getSizes, Guid.Parse("50dc11b2-1ee0-4367-b4ae-0cc1aa12a920"), Guid.Parse("dbf111b3-5ed6-457a-9c75-3dcba20d958b")) },
                { "fuse 1l", (dishes, getSizes) => MapDishWithSizeById(dishes, getSizes, Guid.Parse("50dc11b2-1ee0-4367-b4ae-0cc1aa12a920"), Guid.Parse("fb0e24f1-873b-4b72-988f-d1242ee95cbb")) },

                // Lemonade with sizes
                { "lemonade", (dishes, getSizes) => MapDishWithSizeById(dishes, getSizes, Guid.Parse("291d97ef-aa53-4b82-9604-f3f3c7b8195f"), Guid.Parse("dbf111b3-5ed6-457a-9c75-3dcba20d958b")) },
                { "lemonade glass", (dishes, getSizes) => MapDishWithSizeById(dishes, getSizes, Guid.Parse("291d97ef-aa53-4b82-9604-f3f3c7b8195f"), Guid.Parse("160e4142-bce8-42ed-b526-71d920aaaff7")) },

                // Maxi Tea with sizes
                { "maxi tea", (dishes, getSizes) => MapDishWithSizeById(dishes, getSizes, Guid.Parse("b31cbfaf-3330-4c16-ada3-0a4dbdbf6d0a"), Guid.Parse("dbf111b3-5ed6-457a-9c75-3dcba20d958b")) },
                { "maxi tea 1.2", (dishes, getSizes) => MapDishWithSizeById(dishes, getSizes, Guid.Parse("b31cbfaf-3330-4c16-ada3-0a4dbdbf6d0a"), Guid.Parse("fb0e24f1-873b-4b72-988f-d1242ee95cbb")) },

                // Tea with sizes and modifiers
                { "tea", (dishes, getSizes) => MapDishWithSizeById(dishes, getSizes, Guid.Parse("ecdfc085-6137-4f08-a69f-5327876f05a6"), Guid.Parse("a008195b-402d-4207-bc3e-be1334b0bd66")) },
                { "tea green", (dishes, getSizes) => MapDishWithModifierById(dishes, Guid.Parse("ecdfc085-6137-4f08-a69f-5327876f05a6"), Guid.Parse("bb295802-0cc8-434c-94c4-1ae9cab7dffa")) },
                { "tea teapot", (dishes, getSizes) => MapDishWithSizeById(dishes, getSizes, Guid.Parse("ecdfc085-6137-4f08-a69f-5327876f05a6"), Guid.Parse("96f32557-9404-42f8-a029-ef3c7c1960e6")) },

                // Samsa with modifiers
                { "samsa cheese", (dishes, getSizes) => MapDishWithModifierById(dishes, Guid.Parse("657f6b5e-95e9-4561-bd0f-a2925ab03c91"), Guid.Parse("83e545b7-bbed-4300-a582-db860b3a99b3")) },
                { "samsa chicken", (dishes, getSizes) => MapDishWithModifierById(dishes, Guid.Parse("657f6b5e-95e9-4561-bd0f-a2925ab03c91"), Guid.Parse("cab327ff-b462-499f-b359-137d4aeb2d24")) },
                { "samsa meat", (dishes, getSizes) => MapDishWithModifierById(dishes, Guid.Parse("657f6b5e-95e9-4561-bd0f-a2925ab03c91"), Guid.Parse("3b8a3605-1525-4e52-b219-32db7b1c2834")) },

                // Tandyr Samsa with modifiers
                { "tandyr samsa chicken", (dishes, getSizes) => MapDishWithModifierById(dishes, Guid.Parse("f15362c7-a00e-4390-acfd-bb7c1d9581e7"), Guid.Parse("421e3bed-a33c-453f-bd3f-47dc42b6eeb5")) },
                { "tandyr samsa meat", (dishes, getSizes) => MapDishWithModifierById(dishes, Guid.Parse("f15362c7-a00e-4390-acfd-bb7c1d9581e7"), Guid.Parse("388c2000-6654-48b2-8466-d197cccdf245")) },
                { "tamdyr samsa meat", (dishes, getSizes) => MapDishWithModifierById(dishes, Guid.Parse("f15362c7-a00e-4390-acfd-bb7c1d9581e7"), Guid.Parse("388c2000-6654-48b2-8466-d197cccdf245")) },

                // Doner dishes
                { "донер в батоне", (dishes, getSizes) => MapDishById(dishes, Guid.Parse("86f003ae-e62d-4ad2-a1cc-9b80573c9e93")) },
                { "донер в лаваше", (dishes, getSizes) => MapDishById(dishes, Guid.Parse("f1370bbc-56f6-4676-8da6-f7ca9149dd6a")) },

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
