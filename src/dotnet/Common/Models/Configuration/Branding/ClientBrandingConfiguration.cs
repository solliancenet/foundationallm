using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Models.Configuration.Branding
{
    /// <summary>
    /// Provides configuration for client branding.
    /// </summary>
    public class ClientBrandingConfiguration
    {
        /// <summary>
        /// The display name of the client.
        /// </summary>
        public string? CompanyName { get; set; }
        /// <summary>
        /// The page title of the client. 
        /// </summary>
        public string? PageTitle { get; set; }
        /// <summary>
        /// The URL of the client's favicon. Can be an absolute URL or a relative URL.
        /// </summary>
        public string? FavIconUrl { get; set; }
        /// <summary>
        /// The URL of the client's logo. Can be an absolute URL or a relative URL.
        /// </summary>
        public string? LogoUrl { get; set; }
        /// <summary>
        /// The text to display next to the logo in the nav pane. Leave blank to not display any text.
        /// </summary>
        public string? LogoText { get; set; }
        /// <summary>
        /// The primary color of the client in hex format.
        /// </summary>
        public string? PrimaryColor { get; set; }
        /// <summary>
        /// The secondary color of the client in hex format.
        /// </summary>
        public string? SecondaryColor { get; set; }
        /// <summary>
        /// The accent color of the client in hex format.
        /// </summary>
        public string? AccentColor { get; set; }
        /// <summary>
        /// The accent text color of the client in hex format.
        /// </summary>
        public string? AccentTextColor { get; set; }
        /// <summary>
        /// The background color of the client in hex format.
        /// </summary>
        public string? BackgroundColor { get; set; }
        /// <summary>
        /// The background color of the client's primary button in hex format.
        /// </summary>
        public string? PrimaryButtonBackgroundColor { get; set; }
        /// <summary>
        /// The text color of the client's primary button in hex format.
        /// </summary>
        public string? PrimaryButtonTextColor { get; set; }
        /// <summary>
        /// The background color of the client's secondary button in hex format.
        /// </summary>
        public string? SecondaryButtonBackgroundColor { get; set; }
        /// <summary>
        /// The text color of the client's secondary button in hex format.
        /// </summary>
        public string? SecondaryButtonTextColor { get; set; }
        /// <summary>
        /// Flag indicating whether we use kiosk mode or not.
        /// </summary>
        public bool KioskMode { get; set; }
        /// <summary>
        /// The text color that overlays the <see cref="PrimaryColor"/> of the client in hex format.
        /// </summary>
        public string? PrimaryTextColor { get; set; }
        /// <summary>
        /// The text color that overlays the <see cref="SecondaryColor"/> of the client in hex format.
        /// </summary>
        public string? SecondaryTextColor { get; set; }
    }
}
