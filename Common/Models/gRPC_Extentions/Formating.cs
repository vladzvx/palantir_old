using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public partial class Formating
    {
        public static bool IsEmpty(Formating formating)
        {
            return formating.Type == FormatingType.Bold && formating.Offset == 0 && formating.Length == 0 && string.IsNullOrEmpty(formating.Content);
        }

        public static bool IsEmpty( IEnumerable<Formating> formatings)
        {
            foreach (Formating formating in formatings)
            {
                if (Formating.IsEmpty(formating)) return true;
            }
            return false;
        }
    }
}
