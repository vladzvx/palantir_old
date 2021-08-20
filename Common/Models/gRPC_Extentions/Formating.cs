using System.Collections.Generic;

namespace Common
{
    public partial class Formating
    {
        public static bool IsEmpty(Formating formating)
        {
            return formating.Type == FormatingType.Bold && formating.Offset == 0 && formating.Length == 0 && string.IsNullOrEmpty(formating.Content);
        }

        public static bool IsEmpty(IEnumerable<Formating> formatings)
        {
            foreach (Formating formating in formatings)
            {
                if (Formating.IsEmpty(formating))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool ClearEmpty(IEnumerable<Formating> formatings, out List<Formating> result)
        {
            result = new List<Formating>();
            foreach (Formating formating in formatings)
            {
                if (!Formating.IsEmpty(formating))
                {
                    result.Add(formating);
                }
            }
            return result.Count > 0;
        }

        public static bool TryGetNonEmpty(IEnumerable<Formating> formatings, out List<Formating> result)
        {
            result = new List<Formating>();
            if (formatings == null)
            {
                return false;
            }

            foreach (Formating formating in formatings)
            {
                if (!Formating.IsEmpty(formating))
                {
                    result.Add(formating);
                }
            }
            return result.Count > 0;
        }
    }
}
