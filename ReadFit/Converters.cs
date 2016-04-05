using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ReadFit
{
    public class InvBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                return Visibility.Visible;
            }

            if (value is Boolean)
            {
                return ((bool)value) ? Visibility.Collapsed : Visibility.Visible;
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class DateTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
            {
                DateTime y;
                DateTime.TryParse(value.ToString(), out y);
                return y.ToString(parameter.ToString());
            }
            else
            {
                return String.Empty;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return DateTime.Parse(value.ToString());
        }
    }

    public class FormatStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                return string.Empty;
            }

            double myNumber = (double)value;

            if (!Properties.Settings.Default.IsMetric)
            {
                myNumber *= 3.2808399;
            }

            return !string.IsNullOrEmpty(parameter.ToString()) ? string.Format(culture, parameter.ToString(), myNumber) : myNumber.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }

    public class FormatDistanceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                return string.Empty;
            }

            double myNumber = (double)value;

            if (!Properties.Settings.Default.IsMetric)
            {
                myNumber = (myNumber * 3.2808399) / 5280.0;
            }
            else
            {
                myNumber /= 1000.0;
            }

            return !string.IsNullOrEmpty(parameter.ToString()) ? string.Format(culture, parameter.ToString(), myNumber) : myNumber.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }

    public class FormatStringValueConverter : IValueConverter   //converts objects to formatted strings
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                return string.Empty;
            }

            return !string.IsNullOrEmpty(parameter.ToString()) ? string.Format(culture, parameter.ToString(), value) : value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }

    public class FormatSecondsToHHMMSS : IValueConverter    //convert seconds to hh:mm:ss.ff
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            TimeSpan myElasped;

            if (value == null)
            {
                return string.Empty;
            }

            myElasped = TimeSpan.FromSeconds((double)value);

            return myElasped.ToString(@"hh\:mm\:ss\.ff");
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }

    public class FormatSpeedValue : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                return string.Empty;
            }

            if ((double)value == -1.0)
            {
                return "n/a";
            }

            double myNumber = (double)value;

            if (!Properties.Settings.Default.IsMetric)
            {
                myNumber = (double)value * 2.23693629;  //meters per second to miles per hour
            }

            return !string.IsNullOrEmpty(parameter.ToString()) ? string.Format(culture, parameter.ToString(), myNumber) : myNumber.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }

    public class myValueConverter : IValueConverter
    {
        #region IValueConverter Members

        object IValueConverter.Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((string)value == "NP")
            {
                return 1;
            }
            else if ((string)value == "SR")
            {
                return -1;
            }
            else
            {
                return 0;
            }
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion
    }

    public class CheckForValue : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (string.IsNullOrEmpty((string)value))
            {
                return "Transparent";
            }

            return value;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ConvertTemperature : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double mytemperature = System.Convert.ToDouble(value);

            if (!Properties.Settings.Default.IsMetric)
            {
                return System.Convert.ToInt32((mytemperature * 1.8) + 32.0);
            }
            else
            {
                return (int)value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    //public class ConvertItemToIndex : IValueConverter
    //{
    //    #region IValueConverter Members
    //    //Convert the Item to an Index
    //    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    //    {
    //        string parm = (string)parameter;

    //        try
    //        {
    //            //Get the DataRowView that is being passed to the Converter
    //            //System.Data.DataRowView drv = (System.Data.DataRowView)value;

    //            //Get the CollectionView from the DataGrid that is using the converter
    //            DataGrid dg = (DataGrid)Application.Current.MainWindow.FindName(parm);

    //            CollectionView cv = (CollectionView)dg.Items;

    //            //Get the index of the DataRowView from the CollectionView
    //            //int rowindex = cv.IndexOf(drv) + 1;

    //            //return rowindex.ToString();
    //            return ("1");
    //        }
    //        catch (Exception e)
    //        {
    //            throw new NotImplementedException(e.Message);
    //        }
    //    }
    //    //One way binding, so ConvertBack is not implemented
    //    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    #endregion
    //}

    public class DatagridRowNbr : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Object item = values[0];

            DataGrid grdPassedGrid = values[1] as DataGrid;

            int intRowNumber = grdPassedGrid.Items.IndexOf(item) + 1;

            return intRowNumber.ToString().PadLeft(2, '0');
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
