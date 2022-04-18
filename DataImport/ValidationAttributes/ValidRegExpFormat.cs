using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataImport.ValidationAttributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    sealed public class ValidSelectOption : ValidationAttribute
    {
        private string strVal;
        private static Regex requiredEx = new Regex(@"(^\d{2}.*$)|(^\s*$)", RegexOptions.Compiled);
        private static Regex notRequiredEx = new Regex(@"(^\d{1,2}.*$)|(^\s*$)", RegexOptions.Compiled);

        public override bool IsValid(object value)
        {
            if (value == null)
            {
                return true;
            }

            strVal = (string)value;
            if (ValidationConfig.NeedRequiredConfig)
            {
                return requiredEx.IsMatch(strVal);
            }
            else
            {
                return notRequiredEx.IsMatch(strVal);
            }
        }

        public override string FormatErrorMessage(string name)
        {
            return string.Format("The {0} field could not be parsed to a valid Select Field.", name);
        }
    }
}
