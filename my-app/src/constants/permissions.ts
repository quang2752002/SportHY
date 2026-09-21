/**
 * Dynamic Permissions Proxy:
 * Tự động tạo chuỗi 'Permissions.<Module>.<Action>' khi truy cập bất kỳ thuộc tính nào.
 * Ví dụ: Permissions.Tournaments.View -> "Permissions.Tournaments.View"
 * Không cần khai báo thủ công danh sách module hay action, tự động tương thích 100% với Backend!
 */
export const Permissions: any = new Proxy(
  {},
  {
    get(_target, moduleName: string) {
      return new Proxy(
        {},
        {
          get(_subTarget, actionName: string) {
            return `Permissions.${moduleName}.${actionName}`;
          },
        }
      );
    },
  }
);

