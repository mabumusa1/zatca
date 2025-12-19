using System.Collections.Generic;
using Zatca.EInvoice.Models;
using Zatca.EInvoice.Models.Financial;
using Zatca.EInvoice.Helpers;

namespace Zatca.EInvoice.Mappers
{
    /// <summary>
    /// Maps invoice line data (from a dictionary) to an array of InvoiceLine objects.
    ///
    /// Expected input for each line:
    /// {
    ///   "id": "1",
    ///   "unitCode": "PCE",
    ///   "lineExtensionAmount": 100.0,
    ///   "quantity": 10,
    ///   "item": { ... },     // Data for item mapping
    ///   "price": { ... },    // Data for price mapping
    ///   "taxTotal": {        // Data for tax total mapping
    ///       "taxAmount": 15.0,
    ///       "roundingAmount": 0.0
    ///   }
    /// }
    /// </summary>
    public class InvoiceLineMapper
    {
        private readonly ItemMapper _itemMapper;
        private readonly PriceMapper _priceMapper;

        /// <summary>
        /// InvoiceLineMapper constructor.
        /// Initializes the dependent mappers.
        /// </summary>
        public InvoiceLineMapper()
        {
            _itemMapper = new ItemMapper();
            _priceMapper = new PriceMapper();
        }

        /// <summary>
        /// InvoiceLineMapper constructor with dependency injection.
        /// </summary>
        /// <param name="itemMapper">The item mapper.</param>
        /// <param name="priceMapper">The price mapper.</param>
        public InvoiceLineMapper(ItemMapper itemMapper, PriceMapper priceMapper)
        {
            _itemMapper = itemMapper;
            _priceMapper = priceMapper;
        }

        /// <summary>
        /// Maps an array of invoice line data to an array of InvoiceLine objects.
        /// </summary>
        /// <param name="lines">Array of invoice lines data.</param>
        /// <returns>Array of mapped InvoiceLine objects.</returns>
        public List<InvoiceLine> MapInvoiceLines(IEnumerable<object>? lines)
        {
            var invoiceLines = new List<InvoiceLine>();

            if (lines == null)
            {
                return invoiceLines;
            }

            foreach (var lineObj in lines)
            {
                if (lineObj is Dictionary<string, object> line)
                {
                    // Map item data using ItemMapper
                    var itemData = DictionaryHelper.GetDictionary(line, "item");
                    var item = _itemMapper.Map(itemData);

                    // Map price data using PriceMapper
                    var priceData = DictionaryHelper.GetDictionary(line, "price");
                    var price = _priceMapper.Map(priceData);

                    // Map line tax total data
                    var taxTotalData = DictionaryHelper.GetDictionary(line, "taxTotal");
                    var taxTotal = MapLineTaxTotal(taxTotalData);

                    // Create and populate the InvoiceLine object
                    var invoiceLine = new InvoiceLine
                    {
                        Id = DictionaryHelper.GetString(line, "id") ?? "1",
                        UnitCode = DictionaryHelper.GetString(line, "unitCode") ?? "PCE",
                        InvoicedQuantity = DictionaryHelper.GetDecimal(line, "quantity", 0m),
                        LineExtensionAmount = DictionaryHelper.GetDecimal(line, "lineExtensionAmount", 0m),
                        Item = item,
                        Price = price,
                        TaxTotal = taxTotal
                    };

                    invoiceLines.Add(invoiceLine);
                }
            }

            return invoiceLines;
        }

        /// <summary>
        /// Maps line tax total data to a TaxTotal object.
        /// </summary>
        /// <param name="data">Dictionary of line tax total data.</param>
        /// <returns>The mapped TaxTotal object.</returns>
        private TaxTotal MapLineTaxTotal(Dictionary<string, object> data)
        {
            if (data == null)
            {
                data = new Dictionary<string, object>();
            }

            return new TaxTotal
            {
                TaxAmount = DictionaryHelper.GetDecimal(data, "taxAmount", 0m),
                RoundingAmount = DictionaryHelper.GetDecimal(data, "roundingAmount", 0m)
            };
        }
    }
}
