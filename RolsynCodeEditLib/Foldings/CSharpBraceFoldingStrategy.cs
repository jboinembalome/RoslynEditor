using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Folding;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using RoslynCodeEditLib.Enums;
using RoslynCodeEditLib.Models;

namespace RoslynCodeEditLib.Foldings
{
    /// <summary>
    /// Allows producing foldings from a document based on braces and regions.
    /// </summary>
    public class CSharpBraceFoldingStrategy : BraceFoldingStrategy
    {
        /// <summary>
        /// Create <see cref="NewFolding"/>s for the specified document.
        /// </summary>
        public override IEnumerable<NewFolding> CreateNewFoldings(ITextSource document)
        {
            var newFoldings = base.CreateNewFoldings(document) as List<NewFolding>;
            var lineOffsets = new Stack<FoldLine>();
            var lines = document.Text.Split('\n');

            if (lines != null)
            {
                var offset = 0;
                var regexStartRegion = "([ ]*)?#region([ A-Za-z]*)?";
                var regexEndRegion = "([ ]*)?#endregion([ A-Za-z]*)?";
                ////var reStartThreeSlashComment = "([ ])?[/][/][/].*";

                foreach (var item in lines)
                {
                    if (Regex.Match(item, regexStartRegion, RegexOptions.IgnoreCase).Success)
                        lineOffsets.Push(new FoldLine() { Name = item, Offset = offset, TypeOfFold = FoldType.Line });
                    else
                    {
                        if (Regex.Match(item, regexEndRegion, RegexOptions.IgnoreCase).Success == true)
                        {
                            var line = lineOffsets.Pop();

                            // don't fold if opening and closing brace are on the same line
                            if (line.Offset < offset)
                                newFoldings.Add(new NewFolding(line.Offset, offset + (item.Length > 0 ? item.Length - 1 : 0))
                                { Name = line.Name });
                        }
                    }

                    offset += item.Length + 1;
                }
            }

            newFoldings.Sort((a, b) => a.StartOffset.CompareTo(b.StartOffset));

            return newFoldings;
        }

    }
}
