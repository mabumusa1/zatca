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
    public static class SupplierMapper
    {
        /// <summary>
        /// Maps supplier data dictionary to a Party object.
        /// </summary>
        /// <param name="data">Supplier data.</param>
        /// <returns>The mapped supplier as a Party object.</returns>
        public static Party Map(Dictionary<string, object> data)
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
                TaxScheme = taxScheme
            };
            // Only set CompanyId if provided (null is OK, empty string is not)
            var taxId = DictionaryHelper.GetString(data, "taxId");
            if (!string.IsNullOrEmpty(taxId))
                partyTaxScheme.CompanyId = taxId;

            // Map the Address for the supplier
            var addressData = DictionaryHelper.GetDictionary(data, "address");
            var address = new Address();

            // Only set address fields if they have values (null is OK, empty string is not)
            var street = DictionaryHelper.GetString(addressData, "street");
            if (!string.IsNullOrEmpty(street)) address.StreetName = street;

            var buildingNumber = DictionaryHelper.GetString(addressData, "buildingNumber");
            if (!string.IsNullOrEmpty(buildingNumber)) address.BuildingNumber = buildingNumber;

            var subdivision = DictionaryHelper.GetString(addressData, "subdivision");
            if (!string.IsNullOrEmpty(subdivision)) address.CitySubdivisionName = subdivision;

            var city = DictionaryHelper.GetString(addressData, "city");
            if (!string.IsNullOrEmpty(city)) address.CityName = city;

            var postalZone = DictionaryHelper.GetString(addressData, "postalZone");
            if (!string.IsNullOrEmpty(postalZone)) address.PostalZone = postalZone;

            var country = DictionaryHelper.GetString(addressData, "country");
            address.Country = !string.IsNullOrEmpty(country) ? country : "SA";

            // Create and return the Party object with the mapped data
            var party = new Party
            {
                LegalEntity = legalEntity,
                PartyTaxScheme = partyTaxScheme,
                PostalAddress = address
            };

            // Only set party identification if provided (null is OK, empty string is not)
            var identificationId = DictionaryHelper.GetString(data, "identificationId");
            if (!string.IsNullOrEmpty(identificationId))
            {
                party.PartyIdentification = identificationId;
                var identificationType = DictionaryHelper.GetString(data, "identificationType");
                if (!string.IsNullOrEmpty(identificationType))
                    party.PartyIdentificationId = identificationType;
            }

            return party;
        }
    }
}
