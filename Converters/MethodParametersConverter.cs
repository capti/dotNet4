using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Avalonia.Data.Converters;

namespace AircraftApp.Converters
{
    public class MethodParametersConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is MethodInfo method)
            {
                var parameters = method.GetParameters();
                return string.Join(", ", parameters.Select(p => $"{p.ParameterType.Name} {p.Name}"));
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 