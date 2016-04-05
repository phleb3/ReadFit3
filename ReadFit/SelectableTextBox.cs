using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;

namespace ReadFit
{
    public static class SelectableTextBox
    {
        #region SelectAllOnClick attached property
        public static readonly DependencyProperty SelectAllOnInputProperty =
            DependencyProperty.RegisterAttached("SelectAllOnInput", typeof(bool), typeof(SelectableTextBox),
                new FrameworkPropertyMetadata((bool)false,
                    FrameworkPropertyMetadataOptions.None,
                    new PropertyChangedCallback(OnSelectAllOnClickChanged)));

        public static bool GetSelectAllOnInput(DependencyObject d)
        {
            return (bool)d.GetValue(SelectAllOnInputProperty);
        }

        public static void SetSelectAllOnInput(DependencyObject d, bool value)
        {
            d.SetValue(SelectAllOnInputProperty, value);
        }
        #endregion

        /// <summary>
        /// Handles changes to the SelectAllOnClick property.
        /// </summary>
        private static void OnSelectAllOnClickChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender as TextBox != null && (bool)e.NewValue)
            {
                ((TextBox)sender).AddHandler(TextBox.MouseUpEvent, new RoutedEventHandler(OnSelectAllText), true);
                ((TextBox)sender).AddHandler(TextBox.MouseDownEvent, new RoutedEventHandler(OnSelectAllText));
                ((TextBox)sender).AddHandler(TextBox.GotFocusEvent, new RoutedEventHandler(OnSelectAllText));
            }
            else if (sender as TextBox != null && !(bool)e.NewValue)
            {
                ((TextBox)sender).RemoveHandler(TextBox.MouseUpEvent, new RoutedEventHandler(OnSelectAllText));
                ((TextBox)sender).RemoveHandler(TextBox.MouseDownEvent, new RoutedEventHandler(OnSelectAllText));
                ((TextBox)sender).RemoveHandler(TextBox.GotFocusEvent, new RoutedEventHandler(OnSelectAllText));
            }
        }

        /// <summary>
        /// Handler that select all TextBox's text
        /// </summary>
        private static void OnSelectAllText(object sender, RoutedEventArgs e)
        {
            if (sender as TextBox != null)
            {
                ((TextBox)sender).SelectAll();
            }
        }
    }
}
