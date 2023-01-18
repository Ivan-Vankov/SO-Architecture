using System.Linq;

namespace Vaflov {
    public static partial class StringUtil {
        public static string RemoveWhitespaces(this string self) {
            return new string(self.ToCharArray()
                .Where(c => !char.IsWhiteSpace(c))
                .ToArray());
        }

        public static string Append(this string self, string toAppend) {
            return self + toAppend;
        }
    }
}
