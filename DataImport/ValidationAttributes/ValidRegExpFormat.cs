using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataImport.ValidationAttributes
{
   [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
   public sealed class ValidRegExpFormat : ValidationAttribute
   {
   }
}
