using System;

namespace IAD.Security
{
    internal static class AppSession
    {
        public static bool IsAuthenticated { get; private set; }
        public static string CurrentRole { get; private set; }
        public static long CurrentProductId { get; private set; }
        public static event EventHandler CurrentProductChanged;

        public static void SignIn(string role)
        {
            CurrentRole = role ?? string.Empty;
            IsAuthenticated = !string.IsNullOrEmpty(CurrentRole);
        }

        public static void SignOut()
        {
            CurrentRole = string.Empty;
            IsAuthenticated = false;
            SelectProduct(0);
        }

        public static void SelectProduct(long productId)
        {
            long normalizedProductId = Math.Max(0, productId);
            if (CurrentProductId == normalizedProductId) return;

            CurrentProductId = normalizedProductId;
            EventHandler handler = CurrentProductChanged;
            if (handler != null) handler(null, EventArgs.Empty);
        }
    }
}
