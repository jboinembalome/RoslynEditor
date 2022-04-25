using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.CSharp.Scripting.Hosting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Scripting.Hosting;
using RoslynPad.Roslyn;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using RoslynCodeEditLib.ViewModels;
using RoslynEditorDarkTheme.Interfaces;
using System.IO;
using System.Threading;
using RoslynEditorDarkTheme.Events;
using RoslynEditorDarkTheme.Common;
using HighlightingLib.Interfaces;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Host;

namespace RoslynEditorDarkTheme.ViewModels
{
    public class MainViewModel : ObservableRecipient
    {
        #region Private Variables
        private readonly IThemedHighlightingManager _hlManager;
        private readonly IOpenFileService _openFileService;
        private readonly IFileService _fileService;
        private readonly IDialogService _dialogService;
        private readonly IDocumentationProviderService _documentationProviderService;
        private string _result;
        DocumentViewModel _documentViewModel;
        private RelayCommand _openFile;
        private RelayCommand _getText;
        private RelayCommand _runScript;
        private RelayCommand _formatDocument;

        private CancellationTokenSource? _runCts;
        #endregion

        #region Constructors
        public MainViewModel(IOpenFileService openFileService, IFileService fileService, IDialogService dialogService, IDocumentationProviderService documentationProviderService)
        {
            _openFileService = openFileService;
            _fileService = fileService;
            _dialogService = dialogService;
            _documentationProviderService = documentationProviderService;

            SendTextEvent = new SendTextEvent();

            var host = CreateDefaultHost();
            DocumentViewModel = new DocumentViewModel(host);
        }

        public MainViewModel(IOpenFileService openFileService, IFileService fileService, IDialogService dialogService, IDocumentationProviderService documentationProviderService,
            IThemedHighlightingManager hlManager) : this(openFileService, fileService, dialogService, documentationProviderService)
        {
            _hlManager = hlManager;

            var host = CreateDefaultHost();
            DocumentViewModel = new DocumentViewModel(host, _hlManager);
        }
        #endregion

        #region Properties
        public SendTextEvent SendTextEvent { get; private set; }

        /// <summary>
        /// Gets the DocumentViewModel and all its properties and commands.
        /// </summary>
        public DocumentViewModel DocumentViewModel
        {
            get => _documentViewModel;
            private set => _ = SetProperty(ref _documentViewModel, value);
        }

        private static MethodInfo HasSubmissionResult { get; } =
                typeof(Compilation).GetMethod(nameof(HasSubmissionResult), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        private static PrintOptions PrintOptions { get; } =
            new PrintOptions { MemberDisplayFormat = MemberDisplayFormat.SeparateLines };


        public Script<object> Script { get; private set; }

        public ScriptState ScriptState { get; private set; }

        public bool HasError { get; private set; }

        public string Result
        {
            get => _result;
            private set => _ = SetProperty(ref _result, value);
        }

        #endregion

        #region Commands
        public RelayCommand OpenFileCommand => _openFile
                    ?? (_openFile = new RelayCommand(
                                          () =>
                                          {
                                              var filePath = _openFileService.OpenFileDialog();

                                              if (File.Exists(filePath))
                                              {
                                                  var fileEncoding = _fileService.GetEncoding(filePath);
                                                  var fileInformation = _fileService.ReadToEnd(filePath, fileEncoding);
                                                  var fileExtension = Path.GetExtension(filePath);

                                                  // Send the text to the editor with an event
                                                  // see: https://github.com/icsharpcode/AvalonEdit/issues/84
                                                  SendTextEvent.SendText(fileInformation.Text);
                                              }
                                          }));

        public RelayCommand GetTextCommand => _getText
                    ?? (_getText = new RelayCommand(
                                          () =>
                                          {
                                              _dialogService.ShowMessage(DocumentViewModel.ScriptText);
                                          }));

        public RelayCommand RunScriptCommand => _runScript
                    ?? (_runScript = new RelayCommand(
                                          async () =>
                                          {
                                              await TrySubmit();
                                          }));

        public RelayCommand FormatDocumentCommand => _formatDocument
                   ?? (_formatDocument = new RelayCommand(
                                         async () =>
                                         {
                                             await FormatDocument();
                                         }));
        #endregion

        #region Private and Internal methods

        private async Task FormatDocument()
        {
            var document = DocumentViewModel.Host.GetDocument(DocumentViewModel.Id);
            var formattedDocument = await Formatter.FormatAsync(document).ConfigureAwait(false);
            DocumentViewModel.Host.UpdateDocument(formattedDocument);
        }

        private async Task<bool> TrySubmit()
        {
            Reset();

            Result = null;
            var cancellationToken = _runCts!.Token;

            var code = await GetCodeAsync(cancellationToken).ConfigureAwait(true);

            Script = Script?.ContinueWith(code) ??
                CSharpScript.Create(code, ScriptOptions.Default
                .WithReferences(references: DocumentViewModel.Host.DefaultReferences)
                .WithImports(DocumentViewModel.Host.DefaultImports)
                .WithSourceResolver(new RemoteFileResolver()));

            var compilation = Script.GetCompilation();
            var hasResult = (bool)HasSubmissionResult.Invoke(compilation, null);
            var diagnostics = Script.Compile();
            if (diagnostics.Any(t => t.Severity == DiagnosticSeverity.Error))
            {
                Result = string.Join(Environment.NewLine, diagnostics.Select(FormatObject));
                return false;
            }

            //IsReadOnly = true;

            if (ScriptState != null)
                await ExecuteLast(hasResult);
            else
                await Execute(hasResult);

            return true;
        }

        private async Task Execute(bool hasResult)
        {
            try
            {
                ScriptState = await Script.RunAsync();

                if (ScriptState.Exception != null)
                {
                    HasError = true;
                    Result = FormatException(ScriptState.Exception);
                }
                else
                {
                    Result = hasResult ? FormatObject(ScriptState.ReturnValue) : null;
                }
            }
            catch (Exception ex)
            {
                HasError = true;
                Result = FormatException(ex);
                Script = null;
            }
        }

        private async Task ExecuteLast(bool hasResult)
        {
            try
            {
                ScriptState = await Script.RunFromAsync(ScriptState);

                if (ScriptState.Exception != null)
                {
                    HasError = true;
                    Result = FormatException(ScriptState.Exception);
                }
                else
                {
                    Result = hasResult ? FormatObject(ScriptState.ReturnValue) : null;
                }
            }
            catch (Exception ex)
            {
                HasError = true;
                Result = FormatException(ex);
                Script = null;
            }
        }

        private async Task<string> GetCodeAsync(CancellationToken cancellationToken)
        {
            var document = DocumentViewModel.Host.GetDocument(DocumentViewModel.Id);

            if (document == null)
                return string.Empty;

            return (await document.GetTextAsync(cancellationToken)
                .ConfigureAwait(false)).ToString();
        }

        private void Reset()
        {
            if (_runCts != null)
            {
                _runCts.Cancel();
                _runCts.Dispose();
            }
            _runCts = new CancellationTokenSource();
        }

        private static string FormatException(Exception ex)
        {
            return CSharpObjectFormatter.Instance.FormatException(ex);
        }

        private static string FormatObject(object o)
        {
            return CSharpObjectFormatter.Instance.FormatObject(o, PrintOptions);
        }

        private RoslynHost CreateDefaultHost()
        {
            var messageBoxLocation = typeof(MessageBox).Assembly.Location;
            var objectLocation = typeof(object).Assembly.Location;
            var regexLocation = typeof(System.Text.RegularExpressions.Regex).Assembly.Location;
            var enumerableLocation = typeof(System.Linq.Enumerable).Assembly.Location;
            var mainViewModelLocation = typeof(MainViewModel).Assembly.Location;
            var documentViewModelLocation = typeof(DocumentViewModel).Assembly.Location;

            var host = new RoslynHost(additionalAssemblies: new[]
            {
                Assembly.Load("RoslynPad.Roslyn.Windows"),
                Assembly.Load("RoslynPad.Editor.Windows"),
                Assembly.Load("RoslynEditorDarkTheme"),
                Assembly.Load("RoslynCodeEditLib"),
            }, RoslynHostReferences.NamespaceDefault.With(new[]
            {
                MetadataReference.CreateFromFile(messageBoxLocation, documentation:_documentationProviderService.GetDocumentationProvider(messageBoxLocation)),
                MetadataReference.CreateFromFile(objectLocation, documentation:_documentationProviderService.GetDocumentationProvider(objectLocation)),
                MetadataReference.CreateFromFile(regexLocation, documentation:_documentationProviderService.GetDocumentationProvider(regexLocation)),
                MetadataReference.CreateFromFile(enumerableLocation, documentation:_documentationProviderService.GetDocumentationProvider(enumerableLocation)),
                MetadataReference.CreateFromFile(mainViewModelLocation, documentation:_documentationProviderService.GetDocumentationProvider(mainViewModelLocation)),
                MetadataReference.CreateFromFile(documentViewModelLocation, documentation:_documentationProviderService.GetDocumentationProvider(messageBoxLocation)),
            }, new[]
            {
                        "System",
                        "System.Threading",
                        "System.Threading.Tasks",
                        "System.Collections",
                        "System.Collections.Generic",
                        "System.Text",
                        "System.Text.RegularExpressions",
                        "System.Linq",
                        "System.IO",
                        "System.Reflection",
                        "System.Windows"
            }));

            return host;
        }

        #endregion
    }
}
