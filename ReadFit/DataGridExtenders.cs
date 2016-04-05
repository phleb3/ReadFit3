using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace ReadFit
{
    class DataGridExtenders : DependencyObject
    {
        #region Properties

        public static readonly DependencyProperty AutoScrollToCurrentItemProperty =
            DependencyProperty.RegisterAttached("AutoScrollToCurrentItem",
            typeof(bool),
            typeof(DataGridExtenders),
            new UIPropertyMetadata(default(bool), OnAutoScrollToCurrentItemChanged));

        /// <summary>
        /// Returns the value of the AutoScrollToCurrentItemProperty
        /// </summary>
        /// <param name="obj">The dependency-object whichs value should be returned</param>
        /// <returns>The value of the given property</returns>
        public static bool GetAutoScrollToCurrentItem(DependencyObject obj)
        {
            return (bool)obj.GetValue(AutoScrollToCurrentItemProperty);
        }

        /// <summary>
        /// Sets the value of the AutoScrollToCurrentItemProperty
        /// </summary>
        /// <param name="obj">The dependency-object whichs value should be set</param>
        /// <param name="value">The value which should be assigned to the AutoScrollToCurrentItemProperty</param>
        public static void SetAutoScrollToCurrentItem(DependencyObject obj, bool value)
        {
            obj.SetValue(AutoScrollToCurrentItemProperty, value);
        }

        #endregion

        #region Events

        /// <summary>
        /// This method will be called when the AutoScrollToCurrentItem
        /// property was changed
        /// </summary>
        /// <param name="s">The sender (the ListBox)</param>
        /// <param name="e">Some additional information</param>
        public static void OnAutoScrollToCurrentItemChanged(DependencyObject s, DependencyPropertyChangedEventArgs e)
        {
            var dataGrid = s as DataGrid;

            if (dataGrid != null)
            {
                var dataGridItems = dataGrid.Items;
                if (dataGridItems != null)
                {
                    var newValue = (bool)e.NewValue;

                    var autoScrollToCurrentItemWorker = new EventHandler((s1, e2) => OnAutoScrollToCurrentItem(dataGrid, dataGrid.Items.CurrentPosition));

                    if (newValue)
                    {
                        if (!isHandlerLoaded)
                        {
                            dataGridItems.CurrentChanged += autoScrollToCurrentItemWorker;
                            isHandlerLoaded = true;
                        }
                    }
                    else
                    {
                        dataGridItems.CurrentChanged -= autoScrollToCurrentItemWorker;
                        isHandlerLoaded = false;
                    }
                }
            }
        }

        /// <summary>
        /// This method will be called when the ListBox should
        /// be scrolled to the given index
        /// </summary>
        /// <param name="listBox">The ListBox which should be scrolled</param>
        /// <param name="index">The index of the item to which it should be scrolled</param>
        public static void OnAutoScrollToCurrentItem(DataGrid dataGrid, int index)
        {
            if (dataGrid != null && dataGrid.Items != null && dataGrid.Items.Count > index && index >= 0)
            {
                dataGrid.ScrollIntoView(dataGrid.Items[index]);
            }
        }

        private static bool isHandlerLoaded { get; set; }

        #endregion
    }
}
