using System.Collections.Generic;
using System.Text.Json;
using Zatca.EInvoice.Models;

namespace Zatca.EInvoice.Mappers
{
    /// <summary>
    /// Interface for mapping invoice data to Invoice objects.
    /// </summary>
    public interface IInvoiceMapper
    {
        /// <summary>
        /// Maps input data to an Invoice object.
        /// </summary>
        /// <param name="data">Invoice data as a dictionary.</param>
        /// <returns>The mapped Invoice object.</returns>
        Invoice MapToInvoice(Dictionary<string, object> data);

        /// <summary>
        /// Maps input data to an Invoice object.
        /// </summary>
        /// <param name="jsonData">Invoice data as a JSON string.</param>
        /// <returns>The mapped Invoice object.</returns>
        Invoice MapToInvoice(string jsonData);

        /// <summary>
        /// Maps input data to an Invoice object.
        /// </summary>
        /// <param name="jsonElement">Invoice data as a JsonElement.</param>
        /// <returns>The mapped Invoice object.</returns>
        Invoice MapToInvoice(JsonElement jsonElement);
    }
}
