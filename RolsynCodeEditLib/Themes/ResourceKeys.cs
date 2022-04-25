using System.Windows;

namespace RoslynCodeEditLib.Themes
{
    public static class ResourceKeys
    {
        #region Accent Keys
        /// <summary>
        /// Accent Color Key - This Color key is used to accent elements in the UI
        /// (e.g.: Color of Activated Normal Window Frame, ResizeGrip, Focus or MouseOver input elements)
        /// </summary>
        public static readonly ComponentResourceKey ControlAccentColorKey = new(typeof(ResourceKeys), "ControlAccentColorKey");

        /// <summary>
        /// Accent Brush Key - This Brush key is used to accent elements in the UI
        /// (e.g.: Color of Activated Normal Window Frame, ResizeGrip, Focus or MouseOver input elements)
        /// </summary>
        public static readonly ComponentResourceKey ControlAccentBrushKey = new(typeof(ResourceKeys), "ControlAccentBrushKey");
        #endregion Accent Keys

        #region TextEditor BrushKeys
        public static readonly ComponentResourceKey EditorBackground = new(typeof(ResourceKeys), "EditorBackground");
        public static readonly ComponentResourceKey EditorForeground = new(typeof(ResourceKeys), "EditorForeground");
        public static readonly ComponentResourceKey EditorLineNumbersForeground = new(typeof(ResourceKeys), "EditorLineNumbersForeground");
        public static readonly ComponentResourceKey EditorSelectionBrush = new(typeof(ResourceKeys), "EditorSelectionBrush");
        public static readonly ComponentResourceKey EditorSelectionBorder = new(typeof(ResourceKeys), "EditorSelectionBorder");
        public static readonly ComponentResourceKey EditorNonPrintableCharacterBrush = new(typeof(ResourceKeys), "EditorNonPrintableCharacterBrush");
        public static readonly ComponentResourceKey EditorLinkTextForegroundBrush = new(typeof(ResourceKeys), "EditorLinkTextForegroundBrush");
        public static readonly ComponentResourceKey EditorLinkTextBackgroundBrush = new(typeof(ResourceKeys), "EditorLinkTextBackgroundBrush");

        #region DiffView Currentline Keys
        /// <summary>
        /// Gets the background color for highlighting for the currently highlighed line.
        /// </summary>
        public static readonly ComponentResourceKey EditorCurrentLineBackgroundBrushKey = new(typeof(ResourceKeys), "EditorCurrentLineBackgroundBrushKey");

        /// <summary>
        /// Gets the border color for highlighting for the currently highlighed line.
        /// </summary>
        public static readonly ComponentResourceKey EditorCurrentLineBorderBrushKey = new(typeof(ResourceKeys), "EditorCurrentLineBorderBrushKey");

        /// <summary>
        /// Gets the border thickness for highlighting for the currently highlighed line.
        /// </summary>
        public static readonly ComponentResourceKey EditorCurrentLineBorderThicknessKey = new(typeof(ResourceKeys), "EditorCurrentLineBorderThicknessKey");
        #endregion DiffView Currentline Keys
        #endregion TextEditor BrushKeys
    }
}
