﻿using System;
using System.Windows;
using System.Windows.Data;

namespace Chronos.Client.Win.Controls
{
    public class ResizeBorderThicknessToCaptionMarginConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Thickness thickness = (Thickness)values[0];
            WindowState windowState = (WindowState)values[1];
            if (windowState == WindowState.Maximized)
            {
                return new Thickness(thickness.Left, 0, thickness.Right, 0);
            }
            return new Thickness(0, -thickness.Top, 0, 0);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
