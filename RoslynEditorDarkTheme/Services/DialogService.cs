using System.Windows;
using RoslynEditorDarkTheme.Interfaces;

namespace RoslynEditorDarkTheme.Services
{
    public class DialogService : IDialogService
    {
        public void ShowMessage(string message) => MessageBox.Show(message);
    }
}
