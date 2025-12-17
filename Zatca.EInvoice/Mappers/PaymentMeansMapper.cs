using System.Collections.Generic;
using Zatca.EInvoice.Models;
using Zatca.EInvoice.Models.References;
using Zatca.EInvoice.Helpers;

namespace Zatca.EInvoice.Mappers
{
    /// <summary>
    /// Maps payment means data provided as a dictionary to a PaymentMeans object.
    ///
    /// Expected input structure:
    /// {
    ///     "code": "10",  // Payment means code, e.g., "10" for cash
    ///     "note": "Payment note" // Optional instruction note
    /// }
    /// </summary>
    public class PaymentMeansMapper
    {
        /// <summary>
        /// Maps payment means data dictionary to a PaymentMeans object.
        /// </summary>
        /// <param name="data">The payment means data.</param>
        /// <returns>The mapped PaymentMeans object.</returns>
        public PaymentMeans Map(Dictionary<string, object> data)
        {
            if (data == null)
            {
                data = new Dictionary<string, object>();
            }

            return new PaymentMeans
            {
                PaymentMeansCode = DictionaryHelper.GetString(data, "code", "10"),
                InstructionNote = DictionaryHelper.GetString(data, "note", null)
            };
        }
    }
}
