using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataImport.ValidationAttributes
{
   [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]

   public sealed class HasItems : ValidationAttribute
   {
      public override bool IsValid(object value)
      {
         if (value == null)
         {
            return true;
         }
         else if (value is ICollection<object>)
         {
            return ((ICollection<object>)value).Count > 0;
         }
         else if (value is IEnumerable<object>)
         {
            return ((IEnumerable<object>)value).Count() > 0;
         }
         else
         {
            return true;
         }

      }
   }
}
