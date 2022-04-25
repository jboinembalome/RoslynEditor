using RoslynCodeEditLib.ViewModels;
using MWindowLib;
using RoslynEditorDarkTheme.Themes;
using RoslynEditorDarkTheme.ViewModels;
using Settings.UserProfile;
using System.IO;
using System.Windows;

namespace RoslynEditorDarkTheme
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow, IViewSize  // Implements saving and loading/repositioning of Window
    {
        public MainWindow()
        {
            InitializeComponent();
            //editor.TextArea.SelectionCornerRadius = 0;
            //editor.TextArea.SelectionBorder = new Pen(new SolidColorBrush(Colors.Black), 0);
            //editor.TextArea.SelectionBrush = new SolidColorBrush(Color.FromArgb(100, 51, 153, 255));
            //editor.TextArea.SelectionForeground = new SolidColorBrush(Color.FromRgb(220, 220, 220));

            //editor.SearchReplacePanel.MarkerBrush = new SolidColorBrush(Color.FromRgb(119, 56, 0));

            Loaded += MainWindowLoaded;
        }

        private void MainWindowLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= MainWindowLoaded;

            var appViewModel = DataContext as AppViewModel;
            appViewModel.Main.SendTextEvent.OnTextReceived += TextEventOnTextReceived;

        }

        private void TextEventOnTextReceived(object sender, string e)
        {
            editor.Text = e;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            editor.Focus();
            var viewModel = (DocumentViewModel)args.NewValue;

            if (viewModel != null)
            {
                var workingDirectory = Directory.GetCurrentDirectory();
                var documentId = editor.Initialize(viewModel.Host, new DarkModeHighlightColors(),
                       workingDirectory, viewModel.ScriptText ?? "/*code*/");

                viewModel.Initialize(documentId);
            }
        }
    }
}