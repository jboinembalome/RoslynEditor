using Microsoft.Toolkit.Mvvm.ComponentModel;
using MLib.Interfaces;

namespace RoslynEditorDarkTheme.ViewModels
{
    public class ThemeDefinitionViewModel : ObservableRecipient
    {
        #region Fields

        private bool _IsSelected;
        readonly private IThemeInfo _model;
        #endregion

        #region Constructors

        /// <summary>
        /// Class constructor
        /// </summary>
        /// <param name="model"></param>
        public ThemeDefinitionViewModel(IThemeInfo model)
        {
            _model = model;
        }

        protected ThemeDefinitionViewModel()
        {
            _model = null;
            _IsSelected = false;
        }
        #endregion

        #region Properties

        /// <summary>
        /// Gets the static theme model based data items.
        /// </summary>
        public IThemeInfo Model => _model;

        /// <summary>
        /// Determines whether this theme is currently selected or not.
        /// </summary>
        public bool IsSelected
        {
            get => _IsSelected;
            set => SetProperty(ref _IsSelected, value);
        }
        #endregion
    }
}
