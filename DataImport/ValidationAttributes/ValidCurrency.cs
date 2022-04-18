using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataImport.ValidationAttributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public sealed class ValidCurrency : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            string strVal = (value as string);

            if (strVal == null)
            {
                return true;
            }

            strVal = strVal.Replace("$", string.Empty).Trim();

            if (string.IsNullOrEmpty(strVal))
            {
                return true;
            }
            else
            {
                decimal toss;
                return decimal.TryParse(strVal, out toss);
            }
        }

        public override string FormatErrorMessage(string name)
        {
            return string.Format("Field: [{0}] can not be parsed to a valid currency type.", name);
        }
    }
}
