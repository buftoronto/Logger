using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataImport.ValidationAttributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    sealed public class ValidRequired : RequiredAttribute
    {
        public override bool IsValid(object value)
        {
            if (!ValidationConfig.NeedRequiredConfig)
            {
                return true;
            }

            return base.IsValid(value);
        }
    }
}
