using Nop.Core.Configuration;

namespace SaljiDalje.Core
{
    /// <summary>
    /// Represents settings of the Facebook authentication method
    /// </summary>
    public static class GoogleExternalAuthSettings
    {
        /// <summary>
        /// Gets or sets OAuth2 client identifier
        /// </summary>
        public static string ClientKeyIdentifier { get; set; } = "471762266413-agf6fbmsk4rn5e8q7j9sdqpgf50oa2s2.apps.googleusercontent.com";

        /// <summary>
        /// Gets or sets OAuth2 client secret
        /// </summary>
        public static string ClientSecret { get; set; } = "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";
    }
}