﻿using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using MLib.Interfaces;
using Settings.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace RoslynEditorDarkTheme.ViewModels
{
    /// <summary>
    /// ViewModel class that manages theme properties for binding and display in WPF UI.
    /// </summary>
    public class ThemeViewModel : ObservableRecipient
    {
        #region Fields

        private readonly ThemeDefinitionViewModel _DefaultTheme = null;
        private readonly Dictionary<string, ThemeDefinitionViewModel> _ListOfThemes = null;
        private ThemeDefinitionViewModel _SelectedTheme = null;
        private bool _IsEnabled = true;
        #endregion 

        #region Constructors
        /// <summary>
        /// Standard Constructor
        /// </summary>
        public ThemeViewModel()
        {
            var settings = Ioc.Default.GetRequiredService<ISettingsManager>(); // add the default themes

            _ListOfThemes = new Dictionary<string, ThemeDefinitionViewModel>();

            var appearance = Ioc.Default.GetRequiredService<IAppearanceManager>();

            // Go through all WPF Themes lazily initialized in AppLifeCycleViewModel
            // and make theme available in themes handling viewmodel
            foreach (var item in settings.Themes.GetThemeInfos())
                _ListOfThemes.Add(item.DisplayName, new ThemeDefinitionViewModel(item));

            var defaultTheme = appearance.GetDefaultTheme();

            // Lets make sure there is a default
            _ListOfThemes.TryGetValue(defaultTheme.DisplayName, out _DefaultTheme);

            // and something sensible is selected
            _SelectedTheme = _DefaultTheme;
            _SelectedTheme.IsSelected = true;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Returns a default theme that should be applied when nothing else is available.
        /// </summary>
        public ThemeDefinitionViewModel DefaultTheme => _DefaultTheme;

        /// <summary>
        /// Returns a list of theme definitons.
        /// </summary>
        public List<ThemeDefinitionViewModel> ListOfThemes => _ListOfThemes.Select(it => it.Value).ToList();

        /// <summary>
        /// Gets the currently selected theme (or desfault on applaiction start-up)
        /// </summary>
        public ThemeDefinitionViewModel SelectedTheme
        {
            get => _SelectedTheme;
            private set
            {
                if (_SelectedTheme != value)
                {
                    if (_SelectedTheme != null)
                        _SelectedTheme.IsSelected = false;

                    _SelectedTheme = value;

                    if (_SelectedTheme != null)
                        _SelectedTheme.IsSelected = true;

                    OnPropertyChanged(nameof(SelectedTheme));
                }
            }
        }

        /// <summary>
        /// Gets whether a different theme can be selected right now or not.
        /// This property should be bound to the UI that selects a different
        /// theme to avoid the case in which a user could select a theme and
        /// select a different theme while the first theme change request is
        /// still processed.
        /// </summary>
        public bool IsEnabled
        {
            get { return _IsEnabled; }
            private set => SetProperty(ref _IsEnabled, value);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Applies a new theme based on the changed selection in the input element.
        /// </summary>
        /// <param name="ts"></param>
        public void ApplyTheme(FrameworkElement fe, string themeName)
        {
            if (themeName != null)
            {
                IsEnabled = false;
                try
                {
                    var settings = Ioc.Default.GetRequiredService<ISettingsManager>(); // add the default themes

                    Color AccentColor = ThemeViewModel.GetCurrentAccentColor(settings);
                    Ioc.Default.GetRequiredService<IAppearanceManager>().SetTheme(settings.Themes, themeName, AccentColor);

                    _ListOfThemes.TryGetValue(themeName, out ThemeDefinitionViewModel o);
                    SelectedTheme = o;
                }
                catch
                {
                }
                finally
                {
                    IsEnabled = true;
                }
            }
        }

        public static Color GetCurrentAccentColor(ISettingsManager settings)
        {
            Color AccentColor = default;

            if (settings.Options.GetOptionValue<bool>("Appearance", "ApplyWindowsDefaultAccent"))
            {
                try
                {
                    AccentColor = SystemParameters.WindowGlassColor;
                }
                catch
                {
                }

                // This may be black on Windows 7 and the experience is black & white then :-(
                if (AccentColor == default || AccentColor == Colors.Black || AccentColor.A == 0)
                    AccentColor = Color.FromRgb(0x1b, 0xa1, 0xe2);
            }
            else
                AccentColor = settings.Options.GetOptionValue<Color>("Appearance", "AccentColor");

            return AccentColor;
        }
        #endregion
    }
}
