namespace IAD.Security
{
    internal static class AppSession
    {
        public static bool IsAuthenticated { get; private set; }
        public static string CurrentRole { get; private set; }

        public static void SignIn(string role)
        {
            CurrentRole = role ?? string.Empty;
            IsAuthenticated = !string.IsNullOrEmpty(CurrentRole);
        }

        public static void SignOut()
        {
            CurrentRole = string.Empty;
            IsAuthenticated = false;
        }
    }
}
