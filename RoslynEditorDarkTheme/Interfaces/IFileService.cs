
using RoslynEditorDarkTheme.Models;
using System.Text;

namespace RoslynEditorDarkTheme.Interfaces
{
    public interface IFileService
    {
        FileInformation ReadAllText(string filePath);
        FileInformation ReadAllText(string filePath, Encoding encoding);
        FileInformation ReadToEnd(string filePath, Encoding encoding);
        Encoding GetEncoding(string filePath);
    }
}
