using System;
using System.Globalization;
using System.Windows.Data;

namespace RoslynCodeEditLib.Converters
{
    /// <summary>
    /// Converts a boolean value into a configurable string value.
    /// </summary>
    [ValueConversion(typeof(bool), typeof(string))]
    public sealed class BoolToStringPropConverter : IValueConverter
    {
        #region Properties
        /// <summary>
        /// Gets/sets the <see cref="Visibility"/> value that is associated
        /// (converted into) with the boolean true value.
        /// </summary>
        public string TrueValue { get; set; } = "True";

        /// <summary>
        /// Gets/sets the <see cref="Visibility"/> value that is associated
        /// (converted into) with the boolean false value.
        /// </summary>
        public string FalseValue { get; set; } = "False";
        #endregion Properties

        #region Methods
        /// <summary>
        /// Converts a bool value into <see cref="Visibility"/> as configured in the
        /// <see cref="TrueValue"/> and <see cref="FalseValue"/> properties.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            value is not bool boolean ? null : boolean ? TrueValue : FalseValue;

        /// <summary>
        /// Converts a <see cref="Visibility"/> value into bool as configured in the
        /// <see cref="TrueValue"/> and <see cref="FalseValue"/> properties.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            Equals(value, TrueValue) ? true : Equals(value, FalseValue) ? false : null;
        #endregion Methods
    }
}
