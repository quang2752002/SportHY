namespace Dms.Domain.Common
{
    public static class AppConstants
    {
        public static class Pagination
        {
            public const int DefaultPageIndex = 1;
            public const int DefaultPageSize = 10;
            public const int MaxPageSize = 100;
        }

        public static class System
        {
            public const string DefaultCreatedBy = "System";
        }

        public static class SystemSettingKeys
        {
            public const string PhoneNumber = "ContactPhoneNumber";
        }
    }
}
