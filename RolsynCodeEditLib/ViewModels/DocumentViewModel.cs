using HighlightingLib.Interfaces;
using ICSharpCode.AvalonEdit.Highlighting;
using Microsoft.CodeAnalysis;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using RoslynPad.Roslyn;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Media;
using UnitComboLib;
using UnitComboLib.Models;
using UnitComboLib.Models.Unit;
using UnitComboLib.Models.Unit.Screen;
using UnitComboLib.ViewModels;

namespace RoslynCodeEditLib.ViewModels
{
    public class DocumentViewModel : ObservableRecipient
    {
        #region Fields
        private readonly IThemedHighlightingManager _hlManager;

        //private ICSharpCode.AvalonEdit.Document.TextDocument document;
        private DocumentId _id;
        private bool isReadOnly;
        private int synchronizedColumn;
        private int synchronizedLine;
        private string filePath;
        private Encoding fileEncoding;
        private bool isContentLoaded;
        private IDisposable _viewDisposable;
        private IHighlightingDefinition highlightingDefinition;

        #endregion

        #region Properties
        public RoslynHost Host { get; set; }

        public DocumentId Id
        {
            get => _id;
            private set => _ = SetProperty(ref _id, value);
        }

        public bool HasError { get; private set; }

        private string _scriptText;
        public string ScriptText
        {
            get => _scriptText;
            set => _ = SetProperty(ref _scriptText, value);
        }

        /// <summary>
        /// Gets the scale view of text in percentage of font size.
        /// </summary>
        public IUnitViewModel SizeUnitLabel { get; }

        /// <summary>
        /// Gets/Sets the property that checks if the content of the document is loaded.
        /// </summary>
        public bool IsContentLoaded
        {
            get => isContentLoaded;
            set => SetProperty(ref isContentLoaded, value);
        }

        /// <summary>
        /// Gets/Sets the property that checks if the content of the document is read only.
        /// </summary>
        public bool IsReadOnly
        {
            get => isReadOnly;
            set => SetProperty(ref isReadOnly, value);
        }

        /// <summary>
        /// Gets/Sets the caret positions column from the last time when the
        /// caret position in the left view has been synchronzied with the right view (or vice versa).
        /// </summary>
        public int SynchronizedColumn
        {
            get => synchronizedColumn;
            set => SetProperty(ref synchronizedColumn, value);
        }

        /// <summary>
        /// Gets/Sets the caret positions line from the last time when the
        /// caret position in the left view has been synchronzied with the right view (or vice versa).
        /// </summary>
        public int SynchronizedLine
        {
            get => synchronizedLine;
            set => SetProperty(ref synchronizedLine, value);
        }

        /// <summary>
        /// Gets/Sets the path of the file that was opened by the user.
        /// </summary>
        public string FilePath
        {
            get => filePath;
            set => SetProperty(ref filePath, value);
        }

        /// <summary>
        /// Gets/Sets the file encoding of current text file.
        /// </summary>
        public Encoding FileEncoding
        {
            get => fileEncoding;
            set
            {
                if (!Equals(fileEncoding, value))
                {
                    _ = SetProperty(ref fileEncoding, value);
                    OnPropertyChanged(nameof(FileEncodingDescription));
                }
            }
        }

        /// <summary>
        /// Gets the description of file encoding.
        /// </summary>
        public string FileEncodingDescription =>
            $"{fileEncoding.EncodingName}, Header: {fileEncoding.HeaderName} Body: {fileEncoding.BodyName}";

        /// <summary>
        /// Gets an (ordered by Name) list copy of all highlightings defined in this object
        /// or an empty collection if there is no highlighting definition available.
        /// </summary>
        public ReadOnlyCollection<IHighlightingDefinition> HighlightingDefinitions
        {
            get
            {
                if (_hlManager != null)
                    return _hlManager.HighlightingDefinitions;

                return null;
            }
        }

        /// <summary>
        /// AvalonEdit exposes a Highlighting property that controls whether keywords,
        /// comments and other interesting text parts are colored or highlighted in any
        /// other visual way. This property exposes the highlighting information for the
        /// text file managed in this viewmodel class.
        /// </summary>
        public IHighlightingDefinition HighlightingDefinition
        {
            get => highlightingDefinition;
            set => SetProperty(ref highlightingDefinition, value);
        }

        #endregion

        #region Constructors

        public DocumentViewModel(RoslynHost host)
        {
            SizeUnitLabel = UnitViewModeService.CreateInstance(
                new ObservableCollection<ListItem>(GenerateScreenUnitList()),
                new ScreenConverter(), 0);

            fileEncoding = Encoding.Default;

            Host = host;
        }

        /// <summary>
        /// Class constructor from AvalonEdit <see cref="HighlightingManager"/> instance.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="hlManager"></param>
        public DocumentViewModel(RoslynHost host, IThemedHighlightingManager hlManager) 
            : this(host) => _hlManager = hlManager;

        #endregion

        #region Methods

        public void Initialize(DocumentId id)
        {
            Id = id;
        }

        public void CloseDocument(DocumentViewModel document)
        {
            if (document == null)
                return;

            if (document.Id != null)
                Host.CloseDocument(document.Id);

            ClearDocument(document);

            document.Close();
        }

        private void ClearDocument(DocumentViewModel document)
        {
            if (document == null) 
                return;

            document = null;
        }

        public void Close()
        {
            _viewDisposable?.Dispose();
        }

        /// <summary>
        /// Initialize Scale View with useful units in percent and font point size.
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<ListItem> GenerateScreenUnitList()
        {
            List<ListItem> unitList = new();

            var percentDefaults = new ObservableCollection<string>() { "25", "50", "75", "100", "125", "150", "175", "200", "300", "400", "500" };
            var pointsDefaults = new ObservableCollection<string>() { "3", "6", "8", "9", "10", "12", "14", "16", "18", "20", "24", "26", "32", "48", "60" };

            unitList.Add(new ListItem(Itemkey.ScreenPercent, "Percent", "%", percentDefaults));
            unitList.Add(new ListItem(Itemkey.ScreenFontPoints, "Point", "pt", pointsDefaults));

            return unitList;
        }

        /// <summary>
        /// Re-define an existing <seealso cref="SolidColorBrush"/> and backup the originial color
        /// as it was before the application of the custom coloring.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="newColor"></param>
        private static void ApplyToDynamicResource(ComponentResourceKey key, Color? newColor)
        {
            if (Application.Current.Resources[key] == null || newColor == null)
                return;

            // Re-coloring works with SolidColorBrushs linked as DynamicResource
            if (Application.Current.Resources[key] is SolidColorBrush)
            {
                //backupDynResources.Add(resourceName);

                var newColorBrush = new SolidColorBrush((Color)newColor);
                newColorBrush.Freeze();

                Application.Current.Resources[key] = newColorBrush;
            }
        }

        /// <summary>
        /// Invoke this method to apply a change of theme to the content of the document
        /// (eg: Adjust the highlighting colors when changing from "Dark" to "Light"
        ///      WITH current text document loaded.)
        /// </summary>
        public void OnAppThemeChanged()
        {
            if (_hlManager == null)
                return;

            // Does this highlighting definition have an associated highlighting theme?
            if (_hlManager.CurrentTheme.HlTheme != null)
            {
                // A highlighting theme with GlobalStyles?
                // Apply these styles to the resource keys of the editor
                foreach (var item in _hlManager.CurrentTheme.HlTheme.GlobalStyles)
                {
                    switch (item.TypeName)
                    {
                        case "DefaultStyle":
                            ApplyToDynamicResource(Themes.ResourceKeys.EditorBackground, item.Backgroundcolor);
                            ApplyToDynamicResource(Themes.ResourceKeys.EditorForeground, item.Foregroundcolor);
                            break;

                        case "CurrentLineBackground":
                            ApplyToDynamicResource(Themes.ResourceKeys.EditorCurrentLineBackgroundBrushKey, item.Backgroundcolor);
                            ApplyToDynamicResource(Themes.ResourceKeys.EditorCurrentLineBorderBrushKey, item.Bordercolor);
                            break;

                        case "LineNumbersForeground":
                            ApplyToDynamicResource(Themes.ResourceKeys.EditorLineNumbersForeground, item.Foregroundcolor);
                            break;

                        case "Selection":
                            ApplyToDynamicResource(Themes.ResourceKeys.EditorSelectionBrush, item.Backgroundcolor);
                            ApplyToDynamicResource(Themes.ResourceKeys.EditorSelectionBorder, item.Bordercolor);
                            break;

                        case "Hyperlink":
                            ApplyToDynamicResource(Themes.ResourceKeys.EditorLinkTextBackgroundBrush, item.Backgroundcolor);
                            ApplyToDynamicResource(Themes.ResourceKeys.EditorLinkTextForegroundBrush, item.Foregroundcolor);
                            break;

                        case "NonPrintableCharacter":
                            ApplyToDynamicResource(Themes.ResourceKeys.EditorNonPrintableCharacterBrush, item.Foregroundcolor);
                            break;

                        default:
                            throw new System.ArgumentOutOfRangeException("GlobalStyle named '{0}' is not supported.", item.TypeName);
                    }
                }
            }

            // 1st try: Find highlighting based on currently selected highlighting
            // The highlighting name may be the same as before, but the highlighting theme has just changed
            if (HighlightingDefinition != null)
            {
                // Reset property for currently select highlighting definition
                HighlightingDefinition = _hlManager.GetDefinition(HighlightingDefinition.Name);
                OnPropertyChanged(nameof(HighlightingDefinitions));

                if (HighlightingDefinition != null)
                    return;
            }

            // 2nd try: Find highlighting based on extension of file currenlty being viewed
            if (string.IsNullOrEmpty(FilePath))
            {
                // No file path is available -> no default highlighting
                HighlightingDefinition = null;
                OnPropertyChanged(nameof(HighlightingDefinitions));
                return;
            }

            string extension = System.IO.Path.GetExtension(FilePath);
            if (string.IsNullOrEmpty(extension))
            {
                // File path is available but no extension to speak of -> no default highlighting
                HighlightingDefinition = null;
                OnPropertyChanged(nameof(HighlightingDefinitions));
                return;
            }

            // Reset property for currently select highlighting definition
            HighlightingDefinition = _hlManager.GetDefinitionByExtension(extension);
            OnPropertyChanged(nameof(HighlightingDefinitions));
        }



        #endregion
    }
}
