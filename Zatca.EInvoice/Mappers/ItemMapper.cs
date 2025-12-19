using System.Collections.Generic;
using Zatca.EInvoice.Models;
using Zatca.EInvoice.Models.Items;
using Zatca.EInvoice.Models.Financial;
using Zatca.EInvoice.Helpers;

namespace Zatca.EInvoice.Mappers
{
    /// <summary>
    /// Maps item data provided as a dictionary to an Item object.
    ///
    /// Expected input structure:
    /// {
    ///     "name": "Product Name",
    ///     "classifiedTaxCategory": [
    ///         {
    ///             "taxScheme": { "id": "VAT" },
    ///             "percent": 15
    ///         }
    ///     ]
    /// }
    /// </summary>
    public class ItemMapper
    {
        /// <summary>
        /// Maps item data dictionary to an Item object.
        /// </summary>
        /// <param name="data">The item data.</param>
        /// <returns>The mapped Item object.</returns>
        public Item Map(Dictionary<string, object> data)
        {
            if (data == null)
            {
                data = new Dictionary<string, object>();
            }

            // Map classified tax category for the item
            var classifiedTaxCategories = new List<ClassifiedTaxCategory>();

            if (data.TryGetValue("classifiedTaxCategory", out var classifiedTaxCategoryObj) && classifiedTaxCategoryObj is IEnumerable<object> taxCategoryList)
            {
                foreach (var taxCategoryItem in taxCategoryList)
                {
                    if (taxCategoryItem is Dictionary<string, object> taxCategory)
                    {
                        // Map TaxScheme for the item
                        var taxSchemeData = DictionaryHelper.GetDictionary(taxCategory, "taxScheme");
                        var taxScheme = new TaxScheme
                        {
                            Id = DictionaryHelper.GetString(taxSchemeData, "id", "VAT")
                        };

                        // Create and add a new ClassifiedTaxCategory object
                        var classifiedTaxCategory = new ClassifiedTaxCategory
                        {
                            Percent = DictionaryHelper.GetDecimal(taxCategory, "percent", 15m),
                            TaxScheme = taxScheme
                        };

                        classifiedTaxCategories.Add(classifiedTaxCategory);
                    }
                }
            }

            // Create and return the Item object with mapped data
            var item = new Item
            {
                Name = DictionaryHelper.GetString(data, "name", "Product"),
                ClassifiedTaxCategories = classifiedTaxCategories
            };

            return item;
        }
    }
}
