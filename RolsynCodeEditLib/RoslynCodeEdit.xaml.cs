using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Rendering;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using RoslynCodeEditLib.Foldings;
using RoslynCodeEditLib.Helpers;
using RoslynPad.Editor;
using RoslynCodeEditLib.Extensions;

namespace RoslynCodeEditLib
{
    /// <summary>
    /// Implements an <see cref="RoslynCodeEditor"/> control with extensions.
    /// </summary>
    public class RoslynCodeEdit : RoslynCodeEditor
    {
        #region Fields

        private FoldingManager foldingManager;
        private object foldingStrategy;
        #endregion

        #region Dependency Properties

        #region EditorCurrentLine Highlighting Colors

        /// <summary>
        /// Editor current line background property.
        /// </summary>
        public static readonly DependencyProperty EditorCurrentLineBackgroundProperty =
            DependencyProperty.Register("EditorCurrentLineBackground",
                                         typeof(Brush),
                                         typeof(RoslynCodeEdit),
                                         new UIPropertyMetadata(new SolidColorBrush(Colors.Transparent)));

        /// <summary>
        /// Editor current line border dependency property.
        /// </summary>
        public static readonly DependencyProperty EditorCurrentLineBorderProperty =
            DependencyProperty.Register("EditorCurrentLineBorder", typeof(Brush),
                typeof(RoslynCodeEdit), new PropertyMetadata(new SolidColorBrush(
                    Color.FromArgb(0x60, SystemColors.HighlightBrush.Color.R,
                                         SystemColors.HighlightBrush.Color.G,
                                         SystemColors.HighlightBrush.Color.B))));

        /// <summary>
        /// Editor current line border thickness dependency property.
        /// </summary>
        public static readonly DependencyProperty EditorCurrentLineBorderThicknessProperty =
            DependencyProperty.Register("EditorCurrentLineBorderThickness", typeof(double),
                typeof(RoslynCodeEdit), new PropertyMetadata(2.0d));
        #endregion EditorCurrentLine Highlighting Colors

        #region CaretPosition
        /// <summary>
        /// Column dependency property.
        /// </summary>
        private static readonly DependencyProperty ColumnProperty =
            DependencyProperty.Register("Column", typeof(int),
                typeof(RoslynCodeEdit), new UIPropertyMetadata(1));

        /// <summary>
        /// Line dependency property.
        /// </summary>
        private static readonly DependencyProperty LineProperty =
            DependencyProperty.Register("Line", typeof(int),
                typeof(RoslynCodeEdit), new UIPropertyMetadata(1));
        #endregion CaretPosition
        #endregion

        #region Constructors

        /// <summary>
        /// Static class constructor
        /// </summary>
        static RoslynCodeEdit() => DefaultStyleKeyProperty.OverrideMetadata(typeof(RoslynCodeEdit),
                new FrameworkPropertyMetadata(typeof(RoslynCodeEdit)));

        /// <summary>
        /// Class constructor
        /// </summary>
        public RoslynCodeEdit()
        {
            Loaded += RoslynCodeEditLoaded;
            Unloaded += RoslynCodeEditUnloaded;

            _ = CommandBindings.Add(
                new CommandBinding(RoslynCodeEditCommands.FoldsCollapseAll, FoldsCollapseAll, FoldsCollapseExpandCanExecute));
            _ = CommandBindings.Add(
                new CommandBinding(RoslynCodeEditCommands.FoldsExpandAll, FoldsExpandAll, FoldsCollapseExpandCanExecute));
        }
        #endregion

        #region Properties

        #region EditorCurrentLine Highlighting Colors
        /// <summary>
        /// Gets/sets the background color of the current editor line.
        /// </summary>
        public Brush EditorCurrentLineBackground
        {
            get => (Brush)GetValue(EditorCurrentLineBackgroundProperty);
            set => SetValue(EditorCurrentLineBackgroundProperty, value);
        }

        /// <summary>
        /// Gets/sets the border color of the current editor line.
        /// </summary>
        public Brush EditorCurrentLineBorder
        {
            get => (Brush)GetValue(EditorCurrentLineBorderProperty);
            set => SetValue(EditorCurrentLineBorderProperty, value);
        }

        /// <summary>
        /// Gets/sets the the thickness of the border of the current editor line.
        /// </summary>
        public double EditorCurrentLineBorderThickness
        {
            get => (double)GetValue(EditorCurrentLineBorderThicknessProperty);
            set => SetValue(EditorCurrentLineBorderThicknessProperty, value);
        }
        #endregion EditorCurrentLine Highlighting Colors

        #region CaretPosition
        /// <summary>
        /// Get/set the current column of the editor caret.
        /// </summary>
        public int Column
        {
            get => (int)GetValue(ColumnProperty);
            set => SetValue(ColumnProperty, value);
        }

        /// <summary>
        /// Get/set the current line of the editor caret.
        /// </summary>
        public int Line
        {
            get => (int)GetValue(LineProperty);
            set => SetValue(LineProperty, value);
        }
        #endregion CaretPosition
        #endregion

        #region Commands

        #region Fold Unfold Command
        /// <summary>
        /// Determines whether a folding command can be executed or not and sets correspondind
        /// <paramref name="e"/>.CanExecute property value.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void FoldsCollapseExpandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
            e.Handled = true;

            if (sender is not RoslynCodeEdit editoredi || editoredi.foldingManager == null || editoredi.foldingManager.AllFoldings == null)
                return;

            e.CanExecute = true;
        }

        /// <summary>
        /// Executes the collapse all folds command (which folds all text foldings but the first).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void FoldsCollapseAll(object sender, ExecutedRoutedEventArgs e)
        {
            if (sender is not RoslynCodeEdit editor)
                return;

            editor.CollapseAllTextfoldings();
        }

        /// <summary>
        /// Executes the collapse all folds command (which folds all text foldings but the first).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void FoldsExpandAll(object sender, ExecutedRoutedEventArgs e)
        {
            if (sender is not RoslynCodeEdit editor)
                return;

            editor.ExpandAllTextFoldings();
        }

        /// <summary>
        /// Goes through all foldings in the displayed text and folds them
        /// so that users can explore the text in a top down manner.
        /// </summary>
        private void CollapseAllTextfoldings()
        {
            if (foldingManager == null || foldingManager.AllFoldings == null)
                return;

            foreach (var loFolding in foldingManager.AllFoldings)
                loFolding.IsFolded = true;

            //// Unfold the first fold (if any) to give a useful overview on content
            //var foldSection = foldingManager.GetNextFolding(0);

            //if (foldSection != null)
            //    foldSection.IsFolded = false;
        }

        /// <summary>
        /// Goes through all foldings in the displayed text and unfolds them
        /// so that users can see all text items (without having to play with folding).
        /// </summary>
        private void ExpandAllTextFoldings()
        {
            if (foldingManager == null || foldingManager.AllFoldings == null)
                return;

            foreach (var loFolding in foldingManager.AllFoldings)
                loFolding.IsFolded = false;
        }
        #endregion Fold Unfold Command
        #endregion

        #region Events

        private void RoslynCodeEditLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= RoslynCodeEditLoaded;

            // Attach mouse wheel CTRL-key zoom support
            //PreviewMouseWheel += new MouseWheelEventHandler(RoslynCodeEditPreviewMouseWheel);
            TextArea.Caret.PositionChanged += CaretPositionChanged;

            AdjustCurrentLineBackground();

            AddFolding();
        }

        private void RoslynCodeEditUnloaded(object sender, RoutedEventArgs e)
        {
            Unloaded -= RoslynCodeEditUnloaded;

            // Detach mouse wheel CTRL-key zoom support
            PreviewMouseWheel -= RoslynCodeEditPreviewMouseWheel;

            TextArea.Caret.PositionChanged -= CaretPositionChanged;
        }

        protected override void OnTextChanged(EventArgs e)
        {
            UpdateFoldings();

            base.OnTextChanged(e);
        }

        /// <summary>
        /// This method is triggered on a MouseWheel preview event to check if the user
        /// is also holding down the CTRL Key and adjust the current font size if so.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RoslynCodeEditPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                double fontSize = FontSize + e.Delta / 25.0;
                FontSize = fontSize < 6 ? 6 : fontSize > 200 ? 200 : fontSize;

                e.Handled = true;
            }
        }

        /// <summary>
        /// Update Column and Line position properties when caret position is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CaretPositionChanged(object sender, EventArgs e)
        {
            TextArea.TextView.InvalidateLayer(KnownLayer.Background); //Update current line highlighting

            if (TextArea != null)
            {
                Column = TextArea.Caret.Column;
                Line = TextArea.Caret.Line;
            }
            else
            {
                Column = 0;
                Line = 0;
            }
        }
      
        #endregion

        #region Methods

        /// <summary>
        /// Reset the <seealso cref="SolidColorBrush"/> to be used for highlighting the current editor line.
        /// </summary>
        private void AdjustCurrentLineBackground()
        {
            HighlightCurrentLineBackgroundRenderer oldRenderer = null;

            // Make sure there is only one of this type of background renderer
            // Otherwise, we might keep adding and WPF keeps drawing them on top of each other
            foreach (var item in TextArea.TextView.BackgroundRenderers)
            {
                if (item != null && item is HighlightCurrentLineBackgroundRenderer)
                    oldRenderer = item as HighlightCurrentLineBackgroundRenderer;
            }

            if (oldRenderer != null)
                TextArea.TextView.BackgroundRenderers.Remove(oldRenderer);

            TextArea.TextView.BackgroundRenderers.Add(new HighlightCurrentLineBackgroundRenderer(this));
        }

        /// <summary>
        /// Adds the foldings strategy to the text document.
        /// </summary>
        private void AddFolding()
        {
            TextArea.IndentationStrategy = new ICSharpCode.AvalonEdit.Indentation.CSharp.CSharpIndentationStrategy(Options);
            foldingStrategy = new CSharpBraceFoldingStrategy();
            foldingManager = FoldingManager.Install(TextArea);

            UpdateFoldings();
        }

        /// <summary>
        /// Updat the foldings strategy of the text document.
        /// </summary>
        private void UpdateFoldings()
        {
            if (foldingStrategy is CSharpBraceFoldingStrategy cSharpBraceFoldingStrategy)
                cSharpBraceFoldingStrategy.UpdateFoldings(foldingManager, Document);
        }

        /// <summary>
        /// Goes through all region foldings in the displayed text and folds them.
        /// </summary>
        private void CollapseRegionfoldings()
        {
            if (foldingManager == null || foldingManager.AllFoldings == null)
                return;

            foreach (var loFolding in foldingManager.AllFoldings.Where(f => f.Title != null && f.Title.Contains("#region")))
                loFolding.IsFolded = true;
        }
        #endregion
    }
}
