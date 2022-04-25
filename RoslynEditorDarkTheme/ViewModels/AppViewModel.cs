using System;
using System.Windows;
using System.Windows.Input;
using HighlightingLib.Interfaces;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Input;
using MLib.Interfaces;
using RoslynEditorDarkTheme.Models;
using Settings.Interfaces;

namespace RoslynEditorDarkTheme.ViewModels
{
    /// <summary>
    /// Main ViewModel class that manages session start-up, life span, and shutdown
    /// of the application.
    /// </summary>
    public class AppViewModel : ObservableRecipient, IDisposable
    {
        #region Fields
        private bool mDisposed;
        private readonly AppLifeCycleViewModel _AppLifeCycle;

        private bool _isInitialized;       // application should be initialized through one method ONLY!
        private readonly object _lockObject = new(); // thread lock semaphore

        private ICommand _ThemeSelectionChangedCommand;

        private ThemeViewModel _themeViewModel = Ioc.Default.GetRequiredService<ThemeViewModel>();
        #endregion

        #region Constructors
        /// <summary>
        /// Standard Constructor
        /// </summary>
        public AppViewModel(AppLifeCycleViewModel lifecycle) 
            : this() => _AppLifeCycle = lifecycle;

        /// <summary>
        /// Hidden standard constructor
        /// </summary>
        protected AppViewModel() { }
        #endregion

        #region Properties
        public AppLifeCycleViewModel AppLifeCycle => _AppLifeCycle;

        #region app theme
        /// <summary>
        /// Command executes when the user has selected
        /// a different UI theme to display.
        /// 
        /// Command Parameter is the <seealso cref="ThemeDefinitionViewModel"/> object
        /// that should be selected next. This object can be handed over as:
        /// 1> an object[] array at object[0] or as simple object
        /// 2> <seealso cref="ThemeDefinitionViewModel"/> p
        /// </summary>
        public ICommand ThemeSelectionChangedCommand
        {
            get
            {
                if (_ThemeSelectionChangedCommand == null)
                {
                    _ThemeSelectionChangedCommand = new RelayCommand<object>((p) =>
                    {
                        if (mDisposed == true)
                            return;

                        ThemeDefinitionViewModel theme = null;

                        // Try to convert object[0] command parameter
                        if (p is object[] paramets)
                        {
                            if (paramets.Length == 1)
                            {
                                theme = paramets[0] as ThemeDefinitionViewModel;
                            }
                        }

                        // Try to convert ThemeDefinitionViewModel command parameter
                        if (theme == null)
                            theme = p as ThemeDefinitionViewModel;

                        if (Application.Current == null)
                            return;

                        if (Application.Current.MainWindow == null)
                            return;

                        if (theme != null)
                        {
                            _themeViewModel.ApplyTheme(Application.Current.MainWindow, theme.Model.DisplayName);

                            var hlManager = Ioc.Default.GetRequiredService<IThemedHighlightingManager>();
                            var themeDef = theme.Model as ThemeDefinition;

                            // Lets not apply a highlighting theme that is already applicable
                            hlManager.SetCurrentTheme(themeDef.HighlightingThemeName);

                            Main.DocumentViewModel.OnAppThemeChanged();
                        }
                    });
                }

                return _ThemeSelectionChangedCommand;
            }
        }

        /// <summary>
        /// Gets the currently selected application theme object.
        /// </summary>
        public ThemeViewModel AppTheme
        {
            get => _themeViewModel;
            private set => SetProperty(ref _themeViewModel, value);
        }
        #endregion

        /// <summary>
        /// Gets the demo viewmodel and all its properties and commands
        /// </summary>
        public MainViewModel Main { get; } = Ioc.Default.GetRequiredService<MainViewModel>();
        #endregion

        #region Methods
        #region Get/set Session Application Data
        internal static void GetSessionData(IProfile sessionData)
        {
            /***
                        if (sessionData.LastActiveTargetFile != TargetFile.FileName)
                            sessionData.LastActiveTargetFile = TargetFile.FileName;

                        sessionData.LastActiveSourceFiles = new List<SettingsModel.Models.FileReference>();
                        if (SourceFiles != null)
                        {
                            foreach (var item in SourceFiles)
                                sessionData.LastActiveSourceFiles.Add(new SettingsModel.Models.FileReference()
                                { path = item.FileName }
                                                                     );
                        }
            ***/
        }

        internal static void SetSessionData(IProfile sessionData)
        {
            /***
                        TargetFile.FileName = sessionData.LastActiveTargetFile;

                        _SourceFiles = new ObservableCollection<FileInfoViewModel>();
                        if (sessionData.LastActiveSourceFiles != null)
                        {
                            foreach (var item in sessionData.LastActiveSourceFiles)
                                _SourceFiles.Add(new FileInfoViewModel(item.path));
                        }
            ***/
        }
        #endregion Get/set Session Application Data

        /// <summary>
        /// Call this method if you want to initialize a headless
        /// (command line) application. This method will initialize only
        /// Non-WPF related items.
        /// 
        /// Method should not be called after <seealso cref="InitForMainWindow"/>
        /// </summary>
        public void InitWithoutMainWindow()
        {
            lock (_lockObject)
            {
                if (_isInitialized == true)
                    throw new Exception("AppViewModel initizialized twice.");

                _isInitialized = true;
            }
        }

        /// <summary>
        /// Call this to initialize application specific items that should be initialized
        /// before loading and display of mainWindow.
        /// 
        /// Invocation of This method is REQUIRED if UI is used in this application instance.
        /// 
        /// Method should not be called after <seealso cref="InitWithoutMainWindow"/>
        /// </summary>
        public void InitForMainWindow(IAppearanceManager appearance
                                      , string themeDisplayName)
        {
            // Initialize base that does not require UI
            InitWithoutMainWindow();

            appearance.AccentColorChanged += Appearance_AccentColorChanged;

            // Initialize UI specific stuff here
            this.AppTheme.ApplyTheme(Application.Current.MainWindow, themeDisplayName);
        }


        /// <summary>
        /// Standard dispose method of the <seealso cref="IDisposable" /> interface.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Source: http://www.codeproject.com/Articles/15360/Implementing-IDisposable-and-the-Dispose-Pattern-P
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (mDisposed == false)
            {
                if (disposing == true)
                {
                    // Dispose of the curently displayed content
                    ////mContent.Dispose();
                }

                // There are no unmanaged resources to release, but
                // if we add them, they need to be released here.
            }

            mDisposed = true;

            //// If it is available, make the call to the
            //// base class's Dispose(Boolean) method
            ////base.Dispose(disposing);
        }

        /// <summary>
        /// Method is invoked when theme manager is asked
        /// to change the accent color and has actually changed it.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Appearance_AccentColorChanged(object sender, MLib.Events.ColorChangedEventArgs e)
        {

        }
        #endregion
    }
}
