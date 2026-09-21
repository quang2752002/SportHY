namespace Dms.Application.Common
{
    // Giữ alias tương thích ngược trỏ về Dms.Domain.Common.AppRoles
    public static class AppRoles
    {
        public const string Admin = Dms.Domain.Common.AppRoles.Admin;
        public const string Manager = Dms.Domain.Common.AppRoles.Manager;
        public const string HeadReferee = Dms.Domain.Common.AppRoles.HeadReferee;
        public const string Referee = Dms.Domain.Common.AppRoles.Referee;
        public const string Secretary = Dms.Domain.Common.AppRoles.Secretary;
        public const string Delegation = Dms.Domain.Common.AppRoles.Delegation;
        public const string SportCoordinator = Dms.Domain.Common.AppRoles.SportCoordinator;

        public static string[] AllRoles => Dms.Domain.Common.AppRoles.AllRoles;
    }
}
