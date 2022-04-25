using System;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace RoslynEditorDarkTheme.Models
{
    /// <summary>
    /// Class supplies a set of common static helper methodes that help
    /// localizing application specific items such as setting folders etc.
    /// </summary>
    public class AppCore
    {
        #region properties
        /// <summary>
        /// Get the name of the executing assembly (usually name of *.exe file)
        /// </summary>
        internal static string AssemblyTitle => Assembly.GetEntryAssembly().GetName().Name;

        //
        // Summary:
        //     Gets the path or UNC location of the loaded file that contains the manifest.
        //
        // Returns:
        //     The location of the loaded file that contains the manifest. If the loaded
        //     file was shadow-copied, the location is that of the file after being shadow-copied.
        //     If the assembly is loaded from a byte array, such as when using the System.Reflection.Assembly.Load(System.Byte[])
        //     method overload, the value returned is an empty string ("").
        internal static string AssemblyEntryLocation => Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

        /// <summary>
        /// Get a path to the directory where the user store his documents
        /// </summary>
        public static string MyDocumentsUserDir => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        public static string Company => "RoslynEditorDarkTheme";

        public static string Application_Title => "RoslynEditorDarkTheme";

        /// <summary>
        /// Get a path to the directory where the application
        /// can persist/load user data on session exit and re-start.
        /// </summary>
        public static string DirAppData => 
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar + Company;

        ////        /// <summary>
        ////        /// Get path and file name to application specific settings file
        ////        /// </summary>
        ////        public static string DirFileAppSettingsData
        ////        {
        ////            get
        ////            {
        ////                return System.IO.Path.Combine(AppCore.DirAppData,
        ////                                              string.Format(CultureInfo.InvariantCulture, "{0}.App.settings", AppCore.AssemblyTitle));
        ////            }
        ////        }

        /// <summary>
        /// Get path and file name to application specific session file
        /// </summary>
        public static string DirFileAppSessionData => 
            Path.Combine(DirAppData, string.Format(CultureInfo.InvariantCulture, "{0}.App.session", AssemblyTitle));
        #endregion properties

        #region methods
        /// <summary>
        /// Create a dedicated directory to store program settings and session data
        /// </summary>
        /// <returns></returns>
        public static bool CreateAppDataFolder()
        {
            try
            {
                if (Directory.Exists(DirAppData) == false)
                    Directory.CreateDirectory(DirAppData);
            }
            catch
            {
                return false;
            }

            return true;
        }
        #endregion methods
    }
}
