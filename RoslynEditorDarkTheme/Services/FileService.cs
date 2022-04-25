using System.IO;
using System.Text;
using RoslynEditorDarkTheme.Interfaces;
using RoslynEditorDarkTheme.Models;
using ICSharpCode.AvalonEdit.Utils;

namespace RoslynEditorDarkTheme.Services
{
    public class FileService : IFileService
    {
        public FileInformation ReadAllText(string filePath) => !string.IsNullOrWhiteSpace(filePath) 
            ? new() { Text = File.ReadAllText(filePath, Encoding.UTF8), CurrentEncoding = Encoding.UTF8 } 
            : new() { Text = string.Empty };

        public FileInformation ReadAllText(string filePath, Encoding encoding) => !string.IsNullOrWhiteSpace(filePath)
            ? new() { Text = File.ReadAllText(filePath, encoding), CurrentEncoding = encoding }
            : new() { Text = string.Empty };

        public FileInformation ReadToEnd(string filePath, Encoding encoding)
        {
            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var reader = FileReader.OpenStream(fileStream, encoding);
            var text = reader.ReadToEnd();

            // assign encoding after ReadToEnd() so that the StreamReader can autodetect the encoding
            var currentEncoding = reader.CurrentEncoding;

            return new FileInformation { Text = text, CurrentEncoding = currentEncoding };
        }

        public Encoding GetEncoding(string filePath)
        {
            // Read the BOM
            var bom = new byte[4];
            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            _ = fileStream.Read(bom, 0, 4);

            // Analyze the BOM and return the good encoding
            return bom[0] switch
            {
                //0x2b when bom[1] == 0x2f && bom[2] == 0x76 => Encoding.UTF7,
                0xef when bom[1] == 0xbb && bom[2] == 0xbf => Encoding.UTF8,
                0xff when bom[1] == 0xfe => Encoding.Unicode,//UTF-16LE
                0xfe when bom[1] == 0xff => Encoding.BigEndianUnicode,//UTF-16BE
                0 when bom[1] == 0 && bom[2] == 0xfe && bom[3] == 0xff => Encoding.UTF32,
                _ => Encoding.Default,
            };
        }
    }
}
