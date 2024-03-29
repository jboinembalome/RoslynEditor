﻿using MLib.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RoslynEditorDarkTheme.Models
{
    /// <summary>
    /// Implements a model that keeps track of all elements belonging to a WPF Theme
    /// including highlighting theme, display name, required resources (XAML) and so forth.
    /// </summary>
    public class ThemeDefinition : IThemeInfo
    {
        #region constructors
        /// <summary>
        /// Class constructor
        /// </summary>
        /// <param name="themeName"></param>
        /// <param name="themeSources"></param>
        public ThemeDefinition(string themeName, List<Uri> themeSources, string highlightingThemeName) : this()
        {
            DisplayName = themeName;

            if (themeSources != null)
                ThemeSources.AddRange(themeSources.Select(item => new Uri(item.OriginalString, UriKind.Relative)));

            HighlightingThemeName = highlightingThemeName;
        }

        /// <summary>
        /// Copy constructor from <see cref="IThemeInfo"/> parameter.
        /// </summary>
        /// <param name="theme"></param>
        public ThemeDefinition(IThemeInfo theme) : this()
        {
            DisplayName = theme.DisplayName;
            ThemeSources = new List<Uri>(theme.ThemeSources);
        }

        /// <summary>
        /// Hidden standard constructor
        /// </summary>
        protected ThemeDefinition()
        {
            DisplayName = string.Empty;
            ThemeSources = new List<Uri>();
        }
        #endregion constructors

        #region properties
        /// <summary>
        /// Gets the displayable (localized) name for this theme.
        /// </summary>
        public string DisplayName { get; private set; }

        /// <summary>
        /// Gets the Uri sources for this theme.
        /// </summary>
        public List<Uri> ThemeSources { get; private set; }

        /// <summary>
        /// Gets the name of the associated Highlighting Theme for AvalonEdit.
        /// 
        /// This highlighting theme should be configured such that it matches the
        /// themeing colors of the overall WPF theme that is defined in this object.
        /// </summary>
        public string HighlightingThemeName { get; private set; }
        #endregion properties

        #region methods
        /// <summary>
        /// Adds additional resource file references into the existing theme definition.
        /// </summary>
        /// <param name="additionalResource"></param>
        public void AddResources(List<Uri> additionalResource) => ThemeSources.AddRange(additionalResource);
        #endregion methods
    }
}
