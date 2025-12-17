namespace Zatca.EInvoice.Api
{
    /// <summary>
    /// Represents the ZATCA API environments.
    /// </summary>
    public enum ZatcaEnvironment
    {
        /// <summary>
        /// Sandbox environment for development and testing.
        /// </summary>
        Sandbox,

        /// <summary>
        /// Simulation environment for testing with production-like behavior.
        /// </summary>
        Simulation,

        /// <summary>
        /// Production environment for live operations.
        /// </summary>
        Production
    }
}
