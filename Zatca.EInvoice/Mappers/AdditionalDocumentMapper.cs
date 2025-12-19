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
                var docRef = MapSingleDocument(docObj);
                if (docRef != null)
                    additionalDocs.Add(docRef);
            }

            EnsureQrDocumentExists(additionalDocs);

            return additionalDocs;
        }

        private AdditionalDocumentReference? MapSingleDocument(object docObj)
        {
            if (docObj is not Dictionary<string, object> doc)
                return null;

            var docId = DictionaryHelper.GetString(doc, "id", string.Empty);
            if (string.IsNullOrEmpty(docId))
                return null;

            var docRef = new AdditionalDocumentReference { Id = docId };

            var uuid = DictionaryHelper.GetString(doc, "uuid", null);
            if (!string.IsNullOrEmpty(uuid))
                docRef.UUID = uuid;

            if (docId == "PIH")
                MapPihAttachment(docRef, doc);

            return docRef;
        }

        private static void MapPihAttachment(AdditionalDocumentReference docRef, Dictionary<string, object> doc)
        {
            if (!doc.ContainsKey("attachment"))
                return;

            var attachmentData = DictionaryHelper.GetDictionary(doc, "attachment");
            if (attachmentData == null)
                return;

            docRef.Attachment = new Attachment
            {
                EmbeddedDocumentBinaryObject = DictionaryHelper.GetString(attachmentData, "content") ?? string.Empty,
                MimeCode = DictionaryHelper.GetString(attachmentData, "mimeCode") ?? "base64",
                MimeType = DictionaryHelper.GetString(attachmentData, "mimeType") ?? "text/plain"
            };
        }

        private static void EnsureQrDocumentExists(List<AdditionalDocumentReference> additionalDocs)
        {
            if (!additionalDocs.Any(d => d.Id == "QR"))
                additionalDocs.Add(new AdditionalDocumentReference { Id = "QR" });
        }
    }
}
