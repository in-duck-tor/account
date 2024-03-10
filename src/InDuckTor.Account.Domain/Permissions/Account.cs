namespace InDuckTor.Account.Domain;

public static partial class Permission
{
    public static class Account
    {
        /// <summary>
        /// Разрешение информацию по любым счётам
        /// </summary>
        public const string Read = "account.read";
        public const string Manage = "account.manage";
    }
}