using System.Collections.Generic;
using Zatca.EInvoice.Models;
using Zatca.EInvoice.Models.Party;
using Zatca.EInvoice.Models.Financial;
using Zatca.EInvoice.Helpers;

namespace Zatca.EInvoice.Mappers
{
    /// <summary>
    /// Maps supplier data (provided as a dictionary) to a Party object.
    ///
    /// Expected input structure:
    /// {
    ///   "taxScheme": { "id": "VAT" },
    ///   "registrationName": "Supplier Name",
    ///   "taxId": "1234567890",
    ///   "address": {
    ///       "street": "Main Street",
    ///       "buildingNumber": "123",
    ///       "subdivision": "Subdivision Name",
    ///       "city": "City Name",
    ///       "postalZone": "12345",
    ///       "country": "SA"
    ///   },
    ///   "identificationId": "SupplierUniqueID",  // Optional
    ///   "identificationType": "CRN"              // Optional
    /// }
    /// </summary>
    public class SupplierMapper
    {
        /// <summary>
        /// Maps supplier data dictionary to a Party object.
        /// </summary>
        /// <param name="data">Supplier data.</param>
        /// <returns>The mapped supplier as a Party object.</returns>
        public Party Map(Dictionary<string, object> data)
        {
            if (data == null)
            {
                data = new Dictionary<string, object>();
            }

            // Map the TaxScheme for the supplier
            var taxSchemeData = DictionaryHelper.GetDictionary(data, "taxScheme");
            var taxScheme = new TaxScheme
            {
                Id = DictionaryHelper.GetString(taxSchemeData, "id", "VAT")
            };

            // Map the LegalEntity for the supplier
            var legalEntity = new LegalEntity
            {
                RegistrationName = DictionaryHelper.GetString(data, "registrationName", string.Empty)
            };

            // Map the PartyTaxScheme for the supplier
            var partyTaxScheme = new PartyTaxScheme
            {
                TaxScheme = taxScheme,
                CompanyId = DictionaryHelper.GetString(data, "taxId", string.Empty)
            };

            // Map the Address for the supplier
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

            // Create and return the Party object with the mapped data
            var party = new Party
            {
                PartyIdentification = DictionaryHelper.GetString(data, "identificationId", string.Empty),
                PartyIdentificationId = DictionaryHelper.GetString(data, "identificationType", "CRN"),
                LegalEntity = legalEntity,
                PartyTaxScheme = partyTaxScheme,
                PostalAddress = address
            };

            return party;
        }
    }
}
