using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Folding;
using System.Collections.Generic;

namespace RoslynCodeEditLib.Foldings
{
    /// <summary>
    /// Allows producing foldings from a document based on braces.
    /// </summary>
    public class BraceFoldingStrategy : AbstractFoldingStrategy
    {
        /// <summary>
        /// Gets/Sets the opening brace. The default value is '{'.
        /// </summary>
        public char OpeningBrace { get; set; } = '{';

        /// <summary>
        /// Gets/Sets the closing brace. The default value is '}'.
        /// </summary>
        public char ClosingBrace { get; set; } = '}';

        /// <summary>
        /// Create <see cref="NewFolding"/>s for the specified document.
        /// </summary>
        public override IEnumerable<NewFolding> CreateNewFoldings(TextDocument document, out int firstErrorOffset)
        {
            firstErrorOffset = -1;

            // Lets not crash the application over something as silly as foldings ...
            try
            {
                return CreateNewFoldings(document);
            }
            catch
            {
            }

            return new List<NewFolding>();
        }

        /// <summary>
        /// Create <see cref="NewFolding"/>s for the specified document.
        /// </summary>
        public virtual IEnumerable<NewFolding> CreateNewFoldings(ITextSource document)
        {
            var newFoldings = new List<NewFolding>();

            if (document == null)
                return newFoldings;

            var startOffsets = new Stack<int>();
            var lastNewLineOffset = 0;

            for (int i = 0; i < document.TextLength; i++)
            {
                var character = document.GetCharAt(i);

                if (character == OpeningBrace)
                    startOffsets.Push(i);
                else if (character == ClosingBrace && startOffsets.Count > 0)
                {
                    var startOffset = startOffsets.Pop();

                    // don't fold if opening and closing brace are on the same line
                    if (startOffset < lastNewLineOffset)
                        newFoldings.Add(new NewFolding(startOffset, i + 1));
                }
                else if (character == '\n' || character == '\r')
                    lastNewLineOffset = i + 1;
            }

            newFoldings.Sort((a, b) => a.StartOffset.CompareTo(b.StartOffset));

            return newFoldings;
        }
    }
}
