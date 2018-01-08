namespace Nat.Web.Controls.GenerationClasses
{
    internal static class StringFilterExtension
    {
        public static string ToFilterTypeString(this string value)
        {
            return "(" + value.ToLower() + "): ";
        }

        public static string ToFilterTypeStringBit(this string value)
        {
            return "(" + value.ToLower() + ")";
        }
    }
}