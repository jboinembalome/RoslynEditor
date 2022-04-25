
namespace RoslynEditorDarkTheme.Interfaces
{
    public interface IOpenFileService
    {
        string OpenFileDialog(bool checkFileExists = true);
        string OpenFileDialog(string defaultPath, bool checkFileExists = true);
    }
}
