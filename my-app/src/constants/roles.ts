export const AppRoles = {
  Admin: 'Admin',                     // Quản trị toàn bộ phần mềm
  Manager: 'Manager',                 // Ban tổ chức / Quản lý giải
  HeadReferee: 'HeadReferee',         // Trưởng ban trọng tài
  Referee: 'Referee',                 // Trọng tài
  Secretary: 'Secretary',             // Thư ký
  Delegation: 'Delegation',           // Đơn vị / Đoàn tham gia
} as const;

export type AppRoleType = (typeof AppRoles)[keyof typeof AppRoles];

/**
 * Cấu hình đường dẫn mặc định sau khi đăng nhập theo từng role
 */
export const ROLE_DEFAULT_REDIRECT: Record<string, string> = {
  [AppRoles.Admin]: '/admin',
  [AppRoles.Manager]: '/quan-ly-giai',
  [AppRoles.HeadReferee]: '/truong-ban-trong-tai',
  [AppRoles.Referee]: '/trong-tai',
  [AppRoles.Secretary]: '/thu-ky',
  [AppRoles.Delegation]: '/don-vi',
};

/**
 * Định nghĩa phân quyền truy cập theo Route prefix
 * Đường dẫn nào cần quyền gì (Admin luôn được phép truy cập tất cả)
 */
export const ROUTE_ROLE_PERMISSIONS: Array<{ prefix: string; allowedRoles: string[] }> = [
  { prefix: '/admin', allowedRoles: [AppRoles.Admin] },
  { prefix: '/quan-ly-giai', allowedRoles: [AppRoles.Admin, AppRoles.Manager] },
  { prefix: '/truong-ban-trong-tai', allowedRoles: [AppRoles.Admin, AppRoles.HeadReferee] },
  { prefix: '/trong-tai', allowedRoles: [AppRoles.Admin, AppRoles.Referee, AppRoles.HeadReferee] },
  { prefix: '/thu-ky', allowedRoles: [AppRoles.Admin, AppRoles.Secretary] },
  { prefix: '/don-vi', allowedRoles: [AppRoles.Admin, AppRoles.Delegation] },
];
