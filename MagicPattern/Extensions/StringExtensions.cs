using System.Text;

namespace MagicPattern.Core
{
    public static class StringExtensions
    {
        public static string ToTitleCase(this string str)
        {
            var sb = new StringBuilder(str.Length);
            var flag = true;

            for (int i = 0; i < str.Length; i++)
            {
                var c = str[i];
                if (char.IsLetterOrDigit(c))
                {
                    sb.Append(flag ? char.ToUpper(c) : c);
                    flag = false;
                }
                else
                {
                    flag = true;
                }
            }

            return sb.ToString();
        }

        public static string CreateUniqueClassNameFromPlural(this string plural)
        {
            plural = plural.Remove(plural.Length - 1, 1);

            return ToTitleCase(plural);
        }

        public static string CreateUniqueClassName(this string name)
        {
            return ToTitleCase(name);
        }
    }
}
