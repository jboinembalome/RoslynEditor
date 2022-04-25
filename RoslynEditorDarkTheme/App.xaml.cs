using System;
using RoslynEditorDarkTheme.Interfaces;
using RoslynEditorDarkTheme.Services;
using RoslynEditorDarkTheme.ViewModels;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using MLib;
using MLib.Interfaces;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using RoslynEditorDarkTheme.Models;
using Settings;
using Settings.Interfaces;
using System.Threading;
using System.Globalization;
using Settings.UserProfile;
using MWindowInterfacesLib.Interfaces;
using HighlightingLib.Manager;
using Microsoft.CodeAnalysis.Host;

namespace RoslynEditorDarkTheme
{
    /// <summary>
    /// Logique d'interaction pour App.xaml
    /// </summary>
    public partial class App : Application
    {
        #region Fields

        private AppViewModel _appViewModel;
        private MainWindow _mainWindow;
        #endregion

        #region Constructors

        static App()
        {
            Services = ConfigureServices();

            Ioc.Default.ConfigureServices(Services);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the <see cref="IServiceProvider"/> instance to resolve application services.
        /// </summary>
        public static IServiceProvider Services { get; }
        #endregion

        #region Methods
        /// <summary>
        /// Configures the services for the application.
        /// </summary>
        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();
            var appearance = AppearanceManager.GetInstance();

            // Services
            services.AddSingleton<IDocumentationProviderService, DocumentationProviderService>();
            services.AddSingleton<IDialogService, DialogService>();
            services.AddSingleton<IFileService, FileService>();
            services.AddSingleton<IOpenFileService, OpenFileService>();
            services.AddSingleton(SettingsManager.GetInstance(appearance.CreateThemeInfos()));
            services.AddSingleton(appearance);
            services.AddSingleton(ThemedHighlightingManager.Instance);

            // ViewModels
            services.AddTransient<ThemeViewModel>();
            services.AddTransient<MainViewModel>();

            return services.BuildServiceProvider();
        }


        private void Application_Startup(object sender, StartupEventArgs e)
        {
            try
            {
                // Set shutdown mode here (and reset further below) to enable showing custom dialogs (messageboxes)
                // durring start-up without shutting down application when the custom dialogs (messagebox) closes
                ShutdownMode = ShutdownMode.OnExplicitShutdown;
            }
            catch
            {
            }

            var settings = Ioc.Default.GetRequiredService<ISettingsManager>(); // add the default themes
            var appearance = Ioc.Default.GetRequiredService<IAppearanceManager>();
            AppLifeCycleViewModel lifeCycle = null;

            try
            {
                lifeCycle = new AppLifeCycleViewModel();
                lifeCycle.LoadConfigOnAppStartup(settings, appearance);

                var t = Ioc.Default.GetRequiredService<IAppearanceManager>();
                var ezs = settings.Options.GetOptionValue<string>("Appearance", "ThemeDisplayName");


                appearance.SetTheme(settings.Themes, settings.Options.GetOptionValue<string>("Appearance", "ThemeDisplayName"),
                    ThemeViewModel.GetCurrentAccentColor(settings));

                var tt = Ioc.Default.GetRequiredService<IAppearanceManager>();
                var esd = settings.Options.GetOptionValue<string>("Appearance", "ThemeDisplayName");


                // Construct Application ViewMOdel and mainWindow
                _appViewModel = new AppViewModel(lifeCycle);
                AppViewModel.SetSessionData(settings.SessionData);

                // Customize services specific items for this application
                // Program message box service for Modern UI (Metro Light and Dark)
                //                var msgBox = GetService<IMessageBoxService>();
                //                msgBox.Style = MsgBoxStyle.WPFThemed;

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            try
            {
                var selectedLanguage = settings.Options.GetOptionValue<string>("Options", "LanguageSelected");

                Thread.CurrentThread.CurrentCulture = new CultureInfo(selectedLanguage);
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(selectedLanguage);
            }
            catch
            {
            }

            // Create the optional appearance viewmodel and apply
            // current settings to start-up with correct colors etc...
            ////var appearSettings = new AppearanceViewModel(settings.Themes);
            ////appearSettings.ApplyOptionsFromModel(settings.Options);


            // Initialize WPF theming and friends ...
            _appViewModel.InitForMainWindow(Ioc.Default.GetRequiredService<IAppearanceManager>(),
                settings.Options.GetOptionValue<string>("Appearance", "ThemeDisplayName"));

            Current.MainWindow = _mainWindow = new MainWindow();
            MainWindow.DataContext = _appViewModel;

            AppCore.CreateAppDataFolder();

            if (MainWindow != null && _appViewModel != null)
            {
                // and show it to the user ...
                MainWindow.Loaded += MainWindowLoaded;
                MainWindow.Closing += MainWindowOnClosing;

                // When the ViewModel asks to be closed, close the window.
                // Source: http://msdn.microsoft.com/en-us/magazine/dd419663.aspx
                MainWindow.Closed += delegate
                {
                    // Save session data and close application
                    OnClosed(_appViewModel, _mainWindow);

                    if (_appViewModel is IDisposable dispose)
                        dispose.Dispose();

                    _mainWindow.DataContext = null;
                    _appViewModel = null;
                    _mainWindow = null;
                };

                ConstructMainWindowSession(_appViewModel, _mainWindow);

                MainWindow.Show();
            }
        }

        /// <summary>
        /// Method is invoked when the mainwindow is loaded and visble to the user.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindowLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                ShutdownMode = ShutdownMode.OnLastWindowClose;
            }
            catch (Exception exp)
            {
                Debug.WriteLine(exp.ToString());
            }

            /***
                        try
                        {
                            Application.Current.MainWindow = mMainWin = new MainWindow();
                            ShutdownMode = System.Windows.ShutdownMode.OnLastWindowClose;

                            AppCore.CreateAppDataFolder();

                            if (mMainWin != null && app != null)
                            {
                                mMainWin.Closing += OnClosing;


                                ConstructMainWindowSession(app, mMainWin);
                                mMainWin.Show();
                            }
                        }
                        catch (Exception exp)
                        {
                            Logger.Error(exp);
                        }
            ***/
        }

        /// <summary>
        /// COnstruct MainWindow an attach datacontext to it.
        /// </summary>
        /// <param name="workSpace"></param>
        /// <param name="win"></param>
        private static void ConstructMainWindowSession(AppViewModel workSpace, IViewSize win)
        {
            try
            {
                var settings = Ioc.Default.GetRequiredService<ISettingsManager>();

                // Establish command binding to accept user input via commanding framework
                // workSpace.InitCommandBinding(win);

                settings.SessionData.WindowPosSz.TryGetValue(settings.SessionData.MainWindowName, out ViewPosSizeModel viewSz);

                viewSz.SetWindowsState(win);

                var lastActiveFile = settings.SessionData.LastActiveSolution;
                var mainWin = win as MainWindow;
            }
            catch (Exception exp)
            {
                Debug.WriteLine(exp.ToString());
            }
        }

        /// <summary>
        /// Save session data on closing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindowOnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                if (MainWindow.DataContext is AppViewModel appViewModel)
                {
                    if (MainWindow is IMetroWindow MainWindowCanClose)
                        if (MainWindowCanClose.IsContentDialogVisible)
                        {
                            e.Cancel = true;     // Lets not close with open dialog

                            return;
                        }

                    // Close all open files and check whether application is ready to close
                    if (appViewModel.AppLifeCycle.Exit_CheckConditions(appViewModel) == true)
                    {
                        // (other than exception and error handling)
                        appViewModel.AppLifeCycle.OnRequestClose(true);

                        e.Cancel = false;
                    }
                    else
                    {
                        appViewModel.AppLifeCycle.CancelShutDown();
                        e.Cancel = true;
                    }
                }
            }
            catch (Exception exp)
            {
                Debug.WriteLine(exp.ToString());
            }
        }

        /// <summary>
        /// Execute closing function and persist session data to be reloaded on next restart
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnClosed(AppViewModel appVM, IViewSize win)
        {
            try
            {
                var settings = Ioc.Default.GetRequiredService<ISettingsManager>();

                settings.SessionData.WindowPosSz.TryGetValue(settings.SessionData.MainWindowName, out ViewPosSizeModel viewSz);
                viewSz.GetWindowsState(win);

                AppViewModel.GetSessionData(settings.SessionData);

                // Save/initialize program options that determine global programm behaviour
                AppLifeCycleViewModel.SaveConfigOnAppClosed(win);
            }
            catch (Exception exp)
            {
                Debug.WriteLine(exp.ToString());
                ////                var msg = GetService<IMessageBoxService>();
                ////
                ////                msg.Show(exp.ToString(), "Unexpected Error",
                ////                                MsgBox.MsgBoxButtons.OK, MsgBox.MsgBoxImage.Error);
            }
        }


        #endregion methods

    }
}