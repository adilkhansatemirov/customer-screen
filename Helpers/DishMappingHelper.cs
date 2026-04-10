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
        public string ModifierName { get; set; }
        public Guid? ModifierProductId { get; set; }
    }

    public static class DishMappingHelper
    {
        /// <summary>
        /// Maps an API <c>items[].name</c> string to a dish by <see cref="IProduct.ForeignName"/>
        /// (Название на иностранном языке), trimmed and compared case-insensitively.
        /// </summary>
        public static DishMappingResult MapStringToDish(string value, List<IProduct> allDishes)
        {
            if (string.IsNullOrWhiteSpace(value) || allDishes == null || allDishes.Count == 0)
                return null;

            var trimmed = value.Trim();
            var dish = allDishes.FirstOrDefault(d =>
                !string.IsNullOrWhiteSpace(d.ForeignName) &&
                d.ForeignName.Trim().Equals(trimmed, StringComparison.OrdinalIgnoreCase));

            if (dish == null)
                return null;

            return new DishMappingResult
            {
                Product = dish,
                Size = null,
                DisplayName = dish.Name
            };
        }
    }
}
