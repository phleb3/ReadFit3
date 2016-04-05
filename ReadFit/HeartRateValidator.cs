using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ReadFit
{
    public class HeartRateValidator : ValidationRule
    {
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            int nbr = 0;

            string str = value as string;

            if (string.IsNullOrWhiteSpace(str))
            {
                return new ValidationResult(false, "You have to enter a number");
            }
            else if (Int32.TryParse(str, out nbr))
            {
                if (nbr == 0)   //enter zero to clear
                {
                    return new ValidationResult(true, null);
                }
                else if (nbr < 100)
                {
                    return new ValidationResult(false, "Heartrate too low");
                }
                else if (nbr > 200)
                {
                    return new ValidationResult(false, "Heartrate too high");
                }
            }
            else
            {
                return new ValidationResult(false, "Heartrate must be numeric");
            }

            return new ValidationResult(true, null);
        }
    }
}
