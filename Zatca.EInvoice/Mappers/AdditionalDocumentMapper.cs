using System.Collections.Generic;
using System.Linq;
using Zatca.EInvoice.Models;
using Zatca.EInvoice.Models.References;
using Zatca.EInvoice.Helpers;

namespace Zatca.EInvoice.Mappers
{
    /// <summary>
    /// This class maps additional document reference data (from a dictionary)
    /// into an array of AdditionalDocumentReference objects.
    ///
    /// Expected input for each document:
    /// {
    ///   "id": "ICV",           // Required
    ///   "uuid": "unique-id",   // Optional
    ///   "attachment": {        // Optional (mainly for PIH documents)
    ///       "content": "base64content",
    ///       "mimeCode": "base64",
    ///       "mimeType": "text/plain"
    ///   }
    /// }
    /// </summary>
    public class AdditionalDocumentMapper
    {
        /// <summary>
        /// Maps additional documents data to an array of AdditionalDocumentReference objects.
        /// </summary>
        /// <param name="documents">An array of additional document data.</param>
        /// <returns>Array of mapped AdditionalDocumentReference objects.</returns>
        public List<AdditionalDocumentReference> MapAdditionalDocuments(IEnumerable<object>? documents)
        {
            var additionalDocs = new List<AdditionalDocumentReference>();
            var documentsList = documents ?? new List<object>();

            foreach (var docObj in documentsList)
            {
                if (docObj is Dictionary<string, object> doc)
                {
                    // Ensure a valid document ID is provided
                    var docId = DictionaryHelper.GetString(doc, "id", string.Empty);
                    if (string.IsNullOrEmpty(docId))
                    {
                        continue; // Skip documents without an ID
                    }

                    var docRef = new AdditionalDocumentReference
                    {
                        Id = docId
                    };

                    // Set UUID if provided
                    var uuid = DictionaryHelper.GetString(doc, "uuid", null);
                    if (!string.IsNullOrEmpty(uuid))
                    {
                        docRef.UUID = uuid;
                    }

                    // If document ID is 'PIH', map the attachment if provided
                    if (docId == "PIH" && doc.ContainsKey("attachment"))
                    {
                        var attachmentData = DictionaryHelper.GetDictionary(doc, "attachment");
                        if (attachmentData != null)
                        {
                            var attachment = new Attachment
                            {
                                EmbeddedDocumentBinaryObject = DictionaryHelper.GetString(attachmentData, "content") ?? string.Empty,
                                MimeCode = DictionaryHelper.GetString(attachmentData, "mimeCode") ?? "base64",
                                MimeType = DictionaryHelper.GetString(attachmentData, "mimeType") ?? "text/plain"
                            };

                            docRef.Attachment = attachment;
                        }
                    }

                    additionalDocs.Add(docRef);
                }
            }

            // Append a default additional document reference for QR code if not already present
            var qrExists = additionalDocs.Any(d => d.Id == "QR");
            if (!qrExists)
            {
                additionalDocs.Add(new AdditionalDocumentReference { Id = "QR" });
            }

            return additionalDocs;
        }
    }
}
