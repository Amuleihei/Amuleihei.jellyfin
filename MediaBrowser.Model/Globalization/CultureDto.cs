#pragma warning disable CS1591
#pragma warning disable CA1819 // Properties should not return arrays

using System;
using System.Linq;

namespace MediaBrowser.Model.Globalization
{
    /// <summary>
    /// Class CultureDto.
    /// </summary>
    public class CultureDto
    {
        public CultureDto()
        {
            ThreeLetterISOLanguageNames = Array.Empty<string>();
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        /// <value>The display name.</value>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the name of the two letter ISO language.
        /// </summary>
        /// <value>The name of the two letter ISO language.</value>
        public string TwoLetterISOLanguageName { get; set; }

        /// <summary>
        /// Gets the name of the three letter ISO language.
        /// </summary>
        /// <value>The name of the three letter ISO language.</value>
        public string ThreeLetterISOLanguageName
        {
            get => ThreeLetterISOLanguageNames.FirstOrDefault();
        }

        public string[] ThreeLetterISOLanguageNames { get; set; }
    }
}
