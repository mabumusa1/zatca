using System.Collections.Generic;

namespace Zatca.EInvoice.Api
{
    /// <summary>
    /// Static class containing ZATCA API endpoint URLs for each environment.
    /// </summary>
    public static class ZatcaApiEndpoints
    {
        /// <summary>
        /// API version used for all requests.
        /// </summary>
        public const string ApiVersion = "V2";

        /// <summary>
        /// Sandbox environment base URL.
        /// </summary>
        public const string SandboxBaseUrl = "https://gw-fatoora.zatca.gov.sa/e-invoicing/developer-portal/";

        /// <summary>
        /// Simulation environment base URL.
        /// </summary>
        public const string SimulationBaseUrl = "https://gw-fatoora.zatca.gov.sa/e-invoicing/simulation/";

        /// <summary>
        /// Production environment base URL.
        /// </summary>
        public const string ProductionBaseUrl = "https://gw-fatoora.zatca.gov.sa/e-invoicing/core/";

        /// <summary>
        /// Gets the base URL for the specified environment.
        /// </summary>
        /// <param name="environment">The ZATCA environment.</param>
        /// <returns>The base URL for the environment.</returns>
        public static string GetBaseUrl(ZatcaEnvironment environment)
        {
            return environment switch
            {
                ZatcaEnvironment.Sandbox => SandboxBaseUrl,
                ZatcaEnvironment.Simulation => SimulationBaseUrl,
                ZatcaEnvironment.Production => ProductionBaseUrl,
                _ => throw new System.ArgumentException($"Invalid environment: {environment}", nameof(environment))
            };
        }

        /// <summary>
        /// Compliance certificate request endpoint.
        /// </summary>
        public const string ComplianceCertificate = "compliance";

        /// <summary>
        /// Compliance invoice validation endpoint.
        /// </summary>
        public const string ComplianceInvoices = "compliance/invoices";

        /// <summary>
        /// Production certificate request endpoint.
        /// </summary>
        public const string ProductionCertificate = "production/csids";

        /// <summary>
        /// Clearance invoice submission endpoint.
        /// </summary>
        public const string ClearanceInvoice = "invoices/clearance/single";

        /// <summary>
        /// Reporting invoice submission endpoint.
        /// </summary>
        public const string ReportingInvoice = "invoices/reporting/single";
    }
}
