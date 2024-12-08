using System.Text.RegularExpressions;

public static class Formats
{
    private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);
    private static readonly Regex PhoneRegex = new(@"^\+?[0-9\s-]{7,15}$", RegexOptions.Compiled);

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
}