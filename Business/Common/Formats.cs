using System.Text.RegularExpressions;

namespace CompanyDirectory.Common
{
    public static partial class Formats
    {
        private static readonly Regex EmailRegex = EmailRegexFunc();
        private static readonly Regex PhoneRegex = PhoneRegexFunc();

        public static bool IsValidEmail(string email)
        {
            return !string.IsNullOrWhiteSpace(email) && EmailRegex.IsMatch(email);
        }

        public static bool IsValidName(string name)
        {
            // Autorise tout caractère non vide
            return !string.IsNullOrWhiteSpace(name);
        }

        public static bool IsValidPhoneNumber(string phoneNumber)
        {
            return !string.IsNullOrWhiteSpace(phoneNumber) && PhoneRegex.IsMatch(phoneNumber);
        }

        [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled)]
        private static partial Regex EmailRegexFunc();

        [GeneratedRegex(@"^\+?[0-9\s-]{7,15}$", RegexOptions.Compiled)]
        private static partial Regex PhoneRegexFunc();
    }
}