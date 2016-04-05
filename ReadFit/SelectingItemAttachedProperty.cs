using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace ReadFit
{
    public class SelectingItemAttachedProperty
    {
        public static readonly DependencyProperty SelectingItemProperty =
            DependencyProperty.RegisterAttached("SelectingItem",
            typeof(string),
            typeof(SelectingItemAttachedProperty),
            new PropertyMetadata(default(string), OnSelectingItemChanged));

        public static string GetSelectingItem(DependencyObject target)
        {
            return (string)target.GetValue(SelectingItemProperty);
        }

        public static void SetSelectingItem(DependencyObject target, string value)
        {
            target.SetValue(SelectingItemProperty, value);
        }

        static void OnSelectingItemChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var grid = sender as DataGrid;
            if (grid == null || grid.SelectedItem == null)
                return;

             //Works with .Net 4.5
            grid.Dispatcher.InvokeAsync(() =>
            {
                grid.UpdateLayout();
                grid.ScrollIntoView(grid.SelectedItem, null);
            });

            // Works with .Net 4.0
            //grid.Dispatcher.BeginInvoke((Action)(() =>
            //{
            //    grid.UpdateLayout();
            //    grid.ScrollIntoView(grid.SelectedItem, null);
            //}));
        }
    }
}
