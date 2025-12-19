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
        public static Party Map(Dictionary<string, object> data)
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
                TaxScheme = taxScheme
            };
            // Only set CompanyId if provided (null is OK, empty string is not)
            var taxId = DictionaryHelper.GetString(data, "taxId");
            if (!string.IsNullOrEmpty(taxId))
                partyTaxScheme.CompanyId = taxId;

            // Map the Address for the customer
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

            // Create and populate the Party object
            var party = new Party
            {
                LegalEntity = legalEntity,
                PartyTaxScheme = partyTaxScheme,
                PostalAddress = address
            };

            // Set party identification if available
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
