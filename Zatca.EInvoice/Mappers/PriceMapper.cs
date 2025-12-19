using System.Collections.Generic;
using Zatca.EInvoice.Models;
using Zatca.EInvoice.Models.Items;
using Zatca.EInvoice.Models.Financial;
using Zatca.EInvoice.Models.Enums;
using Zatca.EInvoice.Helpers;

namespace Zatca.EInvoice.Mappers
{
    /// <summary>
    /// Maps price data (provided as a dictionary) to a Price object.
    ///
    /// Expected input structure:
    /// {
    ///   "unitCode": "UNIT",   // Optional, defaults to UnitCode.PCE if not provided
    ///   "amount": 100.0,      // Price amount
    ///   "allowanceCharges": [ // Optional, an array of allowance charge data
    ///       {
    ///           "isCharge": true,
    ///           "reason": "discount",
    ///           "amount": 5.0
    ///       }
    ///   ]
    /// }
    /// </summary>
    public static class PriceMapper
    {
        /// <summary>
        /// Maps price data dictionary to a Price object.
        /// </summary>
        /// <param name="data">The price data.</param>
        /// <returns>The mapped Price object.</returns>
        public static Price Map(Dictionary<string, object> data)
        {
            if (data == null)
            {
                data = new Dictionary<string, object>();
            }

            // Parse unit code - UnitCode is a static class with constants, not an enum
            var unitCodeStr = DictionaryHelper.GetString(data, "unitCode") ?? UnitCode.PCE;

            // Validate the unit code against known values or use as-is
            var resolvedUnitCode = unitCodeStr.ToUpperInvariant() switch
            {
                "UNIT" or "C62" => UnitCode.UNIT,
                "PIECE" or "H87" => UnitCode.PIECE,
                "MON" => UnitCode.MON,
                "PCE" => UnitCode.PCE,
                _ => unitCodeStr // Use as-is if not a known constant
            };

            // Create a new Price object and set the unit code and price amount
            var price = new Price
            {
                UnitCode = resolvedUnitCode,
                PriceAmount = DictionaryHelper.GetDecimal(data, "amount", 0m)
            };

            // Map allowance charges if provided
            if (data.TryGetValue("allowanceCharges", out var allowanceChargesObj) && allowanceChargesObj is IEnumerable<object> allowanceChargeList)
            {
                var allowanceCharges = new List<AllowanceCharge>();

                foreach (var chargeObj in allowanceChargeList)
                {
                    if (chargeObj is Dictionary<string, object> charge)
                    {
                        var allowanceCharge = new AllowanceCharge
                        {
                            ChargeIndicator = DictionaryHelper.GetBoolean(charge, "isCharge", true),
                            AllowanceChargeReason = DictionaryHelper.GetString(charge, "reason", "discount"),
                            Amount = DictionaryHelper.GetDecimal(charge, "amount", 0m)
                        };

                        allowanceCharges.Add(allowanceCharge);
                    }
                }

                price.AllowanceCharges = allowanceCharges;
            }

            return price;
        }
    }
}
