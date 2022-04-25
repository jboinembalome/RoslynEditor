using System.Text;

namespace RoslynEditorDarkTheme.Models
{
    /// <summary>
    /// Simple class that encapsulates properties relating to the information in a file read.
    /// </summary>
    public class FileInformation
    {
        /// <summary>
        /// Text present inside a file.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Current Encoding of a file.
        /// </summary>
        public Encoding CurrentEncoding { get; set; }
    }
}
