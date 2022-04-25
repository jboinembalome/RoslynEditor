using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Rendering;
using System.Windows;
using System.Windows.Media;

namespace RoslynCodeEditLib.Extensions
{
    /// <summary>
    /// AvalonEdit: highlight current line even when not focused
    /// 
    /// Source: http://stackoverflow.com/questions/5072761/avalonedit-highlight-current-line-even-when-not-focused
    /// </summary>
    internal class HighlightCurrentLineBackgroundRenderer : IBackgroundRenderer
    {
        #region Fields
        private readonly RoslynCodeEdit _Editor;
        #endregion Fields

        #region Constructors
        /// <summary>
        /// Class Constructor from editor and SolidColorBrush definition
        /// </summary>
        /// <param name="editor"></param>
        public HighlightCurrentLineBackgroundRenderer(RoslynCodeEdit editor)
            : this() => _Editor = editor;

        /// <summary>
        /// Hidden class standard constructor
        /// </summary>
        protected HighlightCurrentLineBackgroundRenderer()
        {
            // Nothing to initialize here...
        }
        #endregion Constructors

        #region Properties
        /// <summary>
        /// Get the <seealso cref="KnownLayer"/> of the <seealso cref="TextEditor"/> control.
        /// </summary>
        public KnownLayer Layer => KnownLayer.Background;
        #endregion Properties

        #region Methods
        /// <summary>
        /// Draw the background line highlighting of the current line.
        /// </summary>
        /// <param name="textView"></param>
        /// <param name="drawingContext"></param>
        public void Draw(TextView textView, DrawingContext drawingContext)
        {
            if (_Editor == null || _Editor.Document == null || _Editor.Document.TextLength == 0
                || _Editor.EditorCurrentLineBorderThickness == 0 && _Editor.EditorCurrentLineBackground == null)
                return;

            Pen borderPen = null;

            if (_Editor.EditorCurrentLineBorder != null)
            {
                borderPen = new Pen(_Editor.EditorCurrentLineBorder, _Editor.EditorCurrentLineBorderThickness);

                if (borderPen.CanFreeze)
                    borderPen.Freeze();
            }

            textView.EnsureVisualLines();

            var currentLine = _Editor.Document.GetLineByOffset(_Editor.CaretOffset);

            foreach (var rect in BackgroundGeometryBuilder.GetRectsForSegment(textView, currentLine))
            {
                var rectangle = new Rect(rect.Location, new Size(textView.ActualWidth, rect.Height));
                drawingContext.DrawRectangle(_Editor.EditorCurrentLineBackground, borderPen, rectangle);
            }
        }
        #endregion Methods
    }
}
