﻿using RoslynEditorDarkTheme.Models;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows.Input;
using MLib.Interfaces;
using Settings.Interfaces;
using System.Collections.Generic;
using Settings.UserProfile;

namespace RoslynEditorDarkTheme.ViewModels
{
    /// <summary>
    /// Implements application life cycle relevant properties and methods,
    /// such as: state for shutdown, shutdown_cancel, command for shutdown,
    /// and methods for save and load application configuration.
    /// </summary>
    public class AppLifeCycleViewModel : ObservableRecipient
    {
        #region fields
        private bool? _DialogCloseResult;
        private bool _ShutDownInProgress;
        private bool _ShutDownInProgress_Cancel;

        private ICommand _ExitApp;

        // List shared XAML Resources between all Dark based WPF themes
        private readonly Uri[] DarkResources =
        {
             new Uri("/MLib;component/Themes/DarkTheme.xaml", UriKind.RelativeOrAbsolute),
             new Uri("/MWindowLib;component/Themes/DarkTheme.xaml", UriKind.RelativeOrAbsolute),
             new Uri("/RoslynCodeEditLib;component/Themes/DarkBrushs.xaml", UriKind.RelativeOrAbsolute),
             new Uri("/RoslynCodeEditLib;component/Themes/Icons.xaml", UriKind.RelativeOrAbsolute)
        };

        // List shared XAML Resources between all Light based WPF themes
        private readonly Uri[] LightResources =
        {
             new Uri("/MLib;component/Themes/LightTheme.xaml", UriKind.RelativeOrAbsolute),
             new Uri("/MWindowLib;component/Themes/LightTheme.xaml", UriKind.RelativeOrAbsolute),
             new Uri("/RoslynCodeEditLib;component/Themes/LightBrushs.xaml", UriKind.RelativeOrAbsolute),
             new Uri("/RoslynCodeEditLib;component/Themes/Icons.xaml", UriKind.RelativeOrAbsolute)
        };


        private readonly string[][] _WpfThemes =
        {
            //           WPF Theme Name       Theme Base   HighlightingThemeKey
            new string[]{"Dark"              ,"Dark" ,     "Dark" },
            new string[]{"Light"             ,"Light",     "Light"},
            new string[]{ "True Blue (Dark)" ,"Dark" ,     "TrueBlue" },
            new string[]{ "True Blue (Light)","Light",     "TrueBlue" },
            new string[]{ "VS 2019 (Light)"  ,"Light",     "VS2019_Light" },
            new string[]{ "VS 2019 (Dark)"   ,"Dark" ,     "VS2019_Dark" }
        };
        #endregion fields

        #region properties
        /// <summary>
        /// Gets a string for display of the application title.
        /// </summary>
        public static string Application_Title => AppCore.Application_Title;

        /// <summary>
        /// Get path and file name to application specific settings file
        /// </summary>
        public static string DirFileAppSettingsData =>
            Path.Combine(AppCore.DirAppData, string.Format(CultureInfo.InvariantCulture, "{0}.App.settings", AppCore.AssemblyTitle));

        /// <summary>
        /// This can be used to close the attached view via ViewModel
        /// 
        /// Source: http://stackoverflow.com/questions/501886/wpf-mvvm-newbie-how-should-the-viewmodel-close-the-form
        /// </summary>
        public bool? DialogCloseResult
        {
            get => _DialogCloseResult;
            private set => SetProperty(ref _DialogCloseResult, value);
        }

        /// <summary>
        /// Gets a command to exit (end) the application.
        /// </summary>
        public ICommand ExitApp
        {
            get
            {
                if (_ExitApp == null)
                    _ExitApp = new RelayCommand<object>((p) => AppExit_CommandExecuted(),
                                                        (p) => Closing_CanExecute());

                return _ExitApp;
            }
        }

        /// <summary>
        /// Get a path to the directory where the user store his documents
        /// </summary>
        public static string MyDocumentsUserDir => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        public bool ShutDownInProgress_Cancel
        {
            get => _ShutDownInProgress_Cancel;
            set
            {
                if (_ShutDownInProgress_Cancel != value)
                    _ShutDownInProgress_Cancel = value;
            }
        }
        #endregion properties

        #region methods
        private void CreateDefaultsSettings(ISettingsManager settings, IAppearanceManager appearance)
        {
            var themeInfos = settings.Themes;
            for (int i = 0; i < _WpfThemes.GetLength(0); i++)
            {
                var item = _WpfThemes[i];
                var WpfResources = item[1] switch
                {
                    "Light" => new List<Uri>(LightResources),
                    "Dark" => new List<Uri>(DarkResources),
                    _ => throw new ArgumentOutOfRangeException("WPF theme base:" + item[1] + " not supported."),
                };

                try
                {
                    // Combine resources into one consistent model
                    var theme = new ThemeDefinition(item[0], WpfResources, item[2]);
                    themeInfos.AddThemeInfo(theme);
                }
                catch (Exception exc)
                {
                    Debug.WriteLine(exc.StackTrace);
                }
            }

            appearance.SetDefaultTheme(themeInfos, "Dark"); // configure a default WPF theme

            try
            {
                // Create a general settings model to make sure the app is at least governed by defaults
                // if there are no customized settings on first ever start-up of application
                var options = settings.Options;

                SettingDefaults.CreateGeneralSettings(options);
                SettingDefaults.CreateAppearanceSettings(options, settings);

                settings.Options.SetUndirty();
            }
            catch
            {
            }
        }

        #region Save Load Application configuration
        /// <summary>
        /// Save application settings when the application is being closed down
        /// </summary>
        public static void SaveConfigOnAppClosed(IViewSize win)
        {
            /***
                        try
                        {
                            Models.AppCore.CreateAppDataFolder();

                            // Save App view model fields
                            var settings = base.GetService<ISettingsManager>();

                            //// settings.SessionData.LastActiveSourceFile = this.mStringDiff.SourceFilePath;
                            //// settings.SessionData.LastActiveTargetFile = this.mStringDiff.TargetFilePath;

                            // Save program options only if there are un-saved changes that need persistence
                            // This can be caused when WPF theme was changed or something else
                            // but should normally not occur as often as saving session data
                            if (settings.Options.IsDirty == true)
                            {
                                ////settings.SaveOptions(AppCore.DirFileAppSettingsData, settings.SettingData);
                                settings.Options.WriteXML(DirFileAppSettingsData);
                            }

                            settings.SaveSessionData(Models.AppCore.DirFileAppSessionData, settings.SessionData);
                        }
                        catch (Exception exp)
                        {
                            var msg = GetService<IMessageBoxService>();
                            msg.Show(exp, "Unexpected Error" // Local.Strings.STR_UnexpectedError_Caption
                                        , MsgBoxButtons.OK, MsgBoxImage.Error);
                        }
            ***/
        }

        /// <summary>
        /// Load configuration from persistence on startup of application
        /// </summary>
        public void LoadConfigOnAppStartup(ISettingsManager settings, IAppearanceManager appearance)
        {
            try
            {
                CreateDefaultsSettings(settings, appearance);

                /***
                    // Re/Load program options and user profile session data to control global behaviour of program
                    settings.LoadOptions(AppCore.DirFileAppSettingsData);
                    settings.Options.ReadXML(DirFileAppSettingsData);
                    settings.LoadSessionData(Models.AppCore.DirFileAppSessionData);

                    settings.CheckSettingsOnLoad(SystemParameters.VirtualScreenLeft,
                                                    SystemParameters.VirtualScreenTop);
                ***/
            }
            catch
            {
            }
        }
        #endregion Save Load Application configuration

        #region StartUp/ShutDown
        private void AppExit_CommandExecuted()
        {
            try
            {
                if (Closing_CanExecute() == true)
                {
                    _ShutDownInProgress_Cancel = false;
                    OnRequestClose();
                }
            }
            catch (Exception exp)
            {
                Debug.WriteLine(exp.ToString());

                ////                var msg = GetService<IMessageBoxService>();
                ////                msg.Show(exp, "Unknown Error",
                ////                MsgBoxButtons.OK, MsgBoxImage.Error, MsgBoxResult.NoDefaultButton);
            }
        }

        private static bool Closing_CanExecute() => true;

        /// <summary>
        /// Check if pre-requisites for closing application are available.
        /// Save session data on closing and cancel closing process if necessary.
        /// </summary>
        /// <returns>true if application is OK to proceed closing with closed, otherwise false.</returns>
        public bool Exit_CheckConditions(object sender)
        {
            //// var msg = ServiceLocator.ServiceContainer.Instance.GetService<IMessageBoxService>();
            try
            {
                if (_ShutDownInProgress == true)
                    return true;

                // this return is normally computed if there are documents open with unsaved data
                return true;

                ////// Do layout serialization after saving/closing files
                ////// since changes implemented by shut-down process are otherwise lost
                ////try
                ////{
                ////    App.CreateAppDataFolder();
                ////    this.SerializeLayout(sender);            // Store the current layout for later retrieval
                ////}
                ////catch
                ////{
                ////}
            }
            catch (Exception exp)
            {
                Debug.WriteLine(exp.ToString());

                ////                var msg = GetService<IMessageBoxService>();
                ////
                ////                msg.Show(exp, "Unexpected Error"//Local.Strings.STR_UnexpectedError_Caption,
                ////                            , MsgBoxButtons.OK, MsgBoxImage.Error, MsgBoxResult.NoDefaultButton);
                ////                //App.IssueTrackerLink, App.IssueTrackerLink, Util.Local.Strings.STR_MSG_IssueTrackerText, null, true);
            }

            return true;
        }

        #region RequestClose [event]
        /// <summary>
        /// Raised when this workspace should be removed from the UI.
        /// </summary>
        ////public event EventHandler ApplicationClosed;

        /// <summary>
        /// Method to be executed when user (or program) tries to close the application
        /// </summary>
        public void OnRequestClose(bool ShutDownAfterClosing = true)
        {
            try
            {
                if (ShutDownAfterClosing == true)
                {
                    if (_ShutDownInProgress == false)
                    {
                        if (DialogCloseResult == null)
                            DialogCloseResult = true;      // Execute Closing event via attached property

                        if (_ShutDownInProgress_Cancel == true)
                        {
                            _ShutDownInProgress = false;
                            _ShutDownInProgress_Cancel = false;
                            DialogCloseResult = null;
                        }
                    }
                }
                else
                    _ShutDownInProgress = true;

                CommandManager.InvalidateRequerySuggested();

                ////EventHandler handler = ApplicationClosed;
                ////if (handler != null)
                ////  handler(this, EventArgs.Empty);
            }
            catch (Exception exp)
            {
                _ShutDownInProgress = false;

                Debug.WriteLine(exp.ToString());

                ////                var msg = GetService<IMessageBoxService>();
                ////                msg.Show(exp, "Unexpected Error" //Local.Strings.STR_UnexpectedError_Caption
                ////                            , MsgBoxButtons.OK, MsgBoxImage.Error, MsgBoxResult.NoDefaultButton);
            }
        }

        public void CancelShutDown() => DialogCloseResult = null;
        #endregion // RequestClose [event]
        #endregion StartUp/ShutDown
        #endregion methods
    }
}
