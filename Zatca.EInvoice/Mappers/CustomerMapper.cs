using System.Collections.Generic;
using Zatca.EInvoice.Models;
using Zatca.EInvoice.Models.Party;
using Zatca.EInvoice.Models.Financial;
using Zatca.EInvoice.Helpers;

namespace Zatca.EInvoice.Mappers
{
    /// <summary>
    /// Maps customer data (dictionary) to a Party object.
    ///
    /// Expected input structure:
    /// {
    ///   "taxScheme": { "id": "VAT" },
    ///   "registrationName": "Customer Name",
    ///   "taxId": "1234567890",
    ///   "address": {
    ///       "street": "Main Street",
    ///       "buildingNumber": "123",
    ///       "subdivision": "Subdivision",
    ///       "city": "City Name",
    ///       "postalZone": "12345",
    ///       "country": "SA"
    ///   },
    ///   "identificationId": "UniqueCustomerId", // optional
    ///   "identificationType": "IDType"          // optional
    /// }
    /// </summary>
    public class CustomerMapper
    {
        /// <summary>
        /// Maps customer data dictionary to a Party object.
        /// </summary>
        /// <param name="data">Customer data.</param>
        /// <returns>The mapped customer as a Party object.</returns>
        public Party Map(Dictionary<string, object> data)
        {
            if (data == null || data.Count == 0)
            {
                return new Party();
            }

            // Map the TaxScheme for the customer
            var taxSchemeData = DictionaryHelper.GetDictionary(data, "taxScheme");
            var taxScheme = new TaxScheme
            {
                Id = DictionaryHelper.GetString(taxSchemeData, "id", "VAT")
            };

            // Map the LegalEntity for the customer
            var legalEntity = new LegalEntity
            {
                RegistrationName = DictionaryHelper.GetString(data, "registrationName", string.Empty)
            };

            // Map the PartyTaxScheme for the customer
            var partyTaxScheme = new PartyTaxScheme
            {
                TaxScheme = taxScheme,
                CompanyId = DictionaryHelper.GetString(data, "taxId", string.Empty)
            };

            // Map the Address for the customer
            var addressData = DictionaryHelper.GetDictionary(data, "address");
            var address = new Address
            {
                StreetName = DictionaryHelper.GetString(addressData, "street", string.Empty),
                BuildingNumber = DictionaryHelper.GetString(addressData, "buildingNumber", string.Empty),
                CitySubdivisionName = DictionaryHelper.GetString(addressData, "subdivision", string.Empty),
                CityName = DictionaryHelper.GetString(addressData, "city", string.Empty),
                PostalZone = DictionaryHelper.GetString(addressData, "postalZone", string.Empty),
                Country = DictionaryHelper.GetString(addressData, "country", "SA")
            };

            // Create and populate the Party object
            var party = new Party
            {
                LegalEntity = legalEntity,
                PartyTaxScheme = partyTaxScheme,
                PostalAddress = address
            };

            // Set party identification if available
            if (data.ContainsKey("identificationId"))
            {
                party.PartyIdentification = DictionaryHelper.GetString(data, "identificationId", string.Empty);

                if (data.ContainsKey("identificationType"))
                {
                    party.PartyIdentificationId = DictionaryHelper.GetString(data, "identificationType", string.Empty);
                }
            }

            return party;
        }
    }
}
