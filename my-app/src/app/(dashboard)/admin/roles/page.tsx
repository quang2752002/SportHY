'use client';

import React, { useEffect, useState } from 'react';
import {
  Row,
  Col,
  Card,
  CardBody,
  Button,
  Spinner,
  Badge,
  Input,
  Label,
  Alert,
} from 'reactstrap';
import { roleService } from '@/services';
import { Role, PermissionGroup } from '@/types';

// Bảng dịch tên hiển thị và mô tả tiếng Việt cho các Vai trò (Roles)
const ROLE_VIETNAMESE_INFO: Record<string, { title: string; subtitle: string; icon: string }> = {
  Admin: {
    title: 'Quản Trị Viên Hệ Thống',
    subtitle: 'Toàn quyền cấu hình & sao lưu hệ thống',
    icon: 'bi bi-shield-lock-fill text-warning',
  },
  Manager: {
    title: 'Ban Tổ Chức Giải Đấu',
    subtitle: 'Điều hành môn thi, bảng đấu & hạt giống',
    icon: 'bi bi-trophy-fill text-primary',
  },
  HeadReferee: {
    title: 'Trưởng Ban Trọng Tài',
    subtitle: 'Phân công trọng tài & giám sát lịch thi',
    icon: 'bi bi-people-fill text-success',
  },
  Referee: {
    title: 'Trọng Tài Điều Khiển',
    subtitle: 'Cập nhật trực tiếp tỷ số & thẻ phạt',
    icon: 'bi bi-stopwatch-fill text-warning',
  },
  Secretary: {
    title: 'Thư Ký Bàn Giải Đấu',
    subtitle: 'Kiểm duyệt biên bản & xuất PDF/Excel',
    icon: 'bi bi-file-earmark-check-fill text-info',
  },
  Delegation: {
    title: 'Đoàn Thể Thao / Đơn Vị',
    subtitle: 'Quản lý & đăng ký danh sách VĐV',
    icon: 'bi bi-building-fill text-danger',
  },
};

// Bảng dịch tiếng Việt cho Tên Nhóm Quyền (Permission Groups)
const GROUP_VIETNAMESE_NAME: Record<string, string> = {
  System: 'Quản Trị Hệ Thống & Cài Đặt',
  Tournaments: 'Quản Lý Giải Đấu (Tournaments)',
  Sports: 'Quản Lý Môn Thể Thao (Sports)',
  TournamentSports: 'Môn Thi Thuộc Giải Đấu',
  Groups: 'Quản Lý Bảng Đấu (Groups)',
  Teams: 'Quản Lý Đội Thi Đấu (Teams)',
  Athletes: 'Quản Lý Vận Động Viên (Athletes)',
  Matches: 'Lịch Thi Đấu & Kết Quả (Matches)',
  Referees: 'Điều Hành & Phân Công Trọng Tài',
  Results: 'Báo Cáo & Biên Bản Thi Đấu',
  Delegations: 'Quản Lý Đoàn Thể Thao Tham Gia',
  Categories: 'Phân Loại Môn Thể Thao (Categories)',
  Users: 'Quản Lý Tài Khoản Người Dùng',
  Menus: 'Cấu Hình Menu Điều Hướng',
  SystemSettings: 'Tham Số Cấu Hình Hệ Thống',
  Repairs: 'Quản Lý Sửa Chữa Dịch Vụ',
  RepairBookings: 'Quản Lý Đặt Lịch Hẹn Dịch Vụ',
};

// Bảng dịch tiếng Việt cho Tên Thao Tác (Actions)
const ACTION_VIETNAMESE_NAME: Record<string, string> = {
  View: 'Xem danh sách & chi tiết',
  Create: 'Tạo mới bản ghi',
  Edit: 'Chỉnh sửa, cập nhật',
  Delete: 'Xóa bản ghi khỏi hệ thống',
  UpdateScore: 'Cập nhật tỷ số, diễn biến trực tiếp',
  Assign: 'Phân công trọng tài chính / phụ',
  Supervise: 'Giám sát tiến độ trận đấu',
  VerifyReport: 'Xác nhận kiểm duyệt biên bản',
  ExportReport: 'Xuất biên bản kết quả (PDF/Excel)',
  ManageAthletes: 'Quản lý & đăng ký hồ sơ VĐV',
  ViewTeams: 'Xem danh sách các đội thi',
  AssignManager: 'Chỉ định điều hành giải đấu',
  ManageUsers: 'Quản lý cấp tài khoản',
  ConfigSettings: 'Cấu hình tham số',
  BackupData: 'Sao lưu & phục hồi dữ liệu',
  ManageRoles: 'Phân quyền vai trò người dùng',
};

export default function AdminRolesPermissionsPage() {
  const [roles, setRoles] = useState<Role[]>([]);
  const [permissionGroups, setPermissionGroups] = useState<PermissionGroup[]>([]);
  const [selectedRole, setSelectedRole] = useState<Role | null>(null);
  const [selectedPermissions, setSelectedPermissions] = useState<Set<string>>(new Set());

  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [message, setMessage] = useState<{ type: 'success' | 'danger'; text: string } | null>(null);

  // Tải dữ liệu danh sách Roles và Cây Permissions
  useEffect(() => {
    const fetchData = async () => {
      setLoading(true);
      try {
        const [rolesData, treeData] = await Promise.all([
          roleService.getRoles(),
          roleService.getPermissionsTree(),
        ]);
        setRoles(rolesData);
        setPermissionGroups(treeData);

        if (rolesData.length > 0) {
          const defaultRole = rolesData.find((r) => r.name === 'Manager') || rolesData[0];
          selectRole(defaultRole);
        }
      } catch (err: any) {
        console.error('Lỗi khi tải dữ liệu phân quyền:', err);
        setMessage({ type: 'danger', text: 'Không thể kết nối đến máy chủ Backend để tải quyền.' });
      } finally {
        setLoading(false);
      }
    };

    fetchData();
  }, []);

  // Chọn 1 vai trò
  const selectRole = (role: Role) => {
    setSelectedRole(role);
    setSelectedPermissions(new Set(role.permissions || []));
    setMessage(null);
  };

  // Toggle 1 quyền đơn lẻ qua checkbox
  const handleTogglePermission = (permValue: string) => {
    if (selectedRole?.name === 'Admin') return;

    const next = new Set(selectedPermissions);
    if (next.has(permValue)) {
      next.delete(permValue);
    } else {
      next.add(permValue);
    }
    setSelectedPermissions(next);
  };

  // Toggle toàn bộ quyền trong 1 nhóm
  const handleToggleGroup = (group: PermissionGroup) => {
    if (selectedRole?.name === 'Admin') return;

    const groupPermValues = group.permissions.map((p) => p.value);
    const isAllChecked = groupPermValues.every((val) => selectedPermissions.has(val));

    const next = new Set(selectedPermissions);
    if (isAllChecked) {
      groupPermValues.forEach((val) => next.delete(val));
    } else {
      groupPermValues.forEach((val) => next.add(val));
    }
    setSelectedPermissions(next);
  };

  // Chọn tất cả / Bỏ chọn tất cả
  const handleSelectAll = (checkAll: boolean) => {
    if (selectedRole?.name === 'Admin') return;

    if (checkAll) {
      const all = new Set<string>();
      permissionGroups.forEach((g) => g.permissions.forEach((p) => all.add(p.value)));
      setSelectedPermissions(all);
    } else {
      setSelectedPermissions(new Set());
    }
  };

  // Lưu quyền cho vai trò
  const handleSavePermissions = async () => {
    if (!selectedRole) return;
    if (selectedRole.name === 'Admin') {
      alert('Vai trò Quản trị viên (Admin) luôn sở hữu 100% quyền hệ thống, không cần lưu!');
      return;
    }

    setSaving(true);
    setMessage(null);
    try {
      const res = await roleService.updateRolePermissions({
        roleName: selectedRole.name,
        permissions: Array.from(selectedPermissions),
      });

      setRoles((prev) =>
        prev.map((r) =>
          r.name === selectedRole.name ? { ...r, permissions: Array.from(selectedPermissions) } : r
        )
      );

      setMessage({ type: 'success', text: res.message || 'Cập nhật phân quyền thành công!' });
    } catch (err: any) {
      console.error('Lỗi khi lưu phân quyền:', err);
      setMessage({
        type: 'danger',
        text: err?.response?.data?.message || err?.message || 'Có lỗi xảy ra khi lưu quyền.',
      });
    } finally {
      setSaving(false);
    }
  };

  const isAdminRole = selectedRole?.name === 'Admin';
  const selectedRoleVi = selectedRole ? ROLE_VIETNAMESE_INFO[selectedRole.name] : null;

  return (
    <Row>
      <Col lg="12">
        <Card className="border-0 shadow-sm rounded-4 overflow-hidden mb-4">
          <CardBody className="p-4">
            {/* Header trang */}
            <div className="d-flex flex-wrap justify-content-between align-items-center gap-3 pb-3 mb-4 border-bottom">
              <div className="d-flex align-items-center gap-2">
                <div className="p-2 bg-primary-subtle text-primary rounded-3 d-inline-flex">
                  <i className="bi bi-shield-check fs-4"></i>
                </div>
                <div>
                  <h4 className="fw-bold mb-0 text-dark">Phân Quyền Theo Vai Trò </h4>
                  <p className="text-muted small mb-0">
                    Cấu hình và cấp các quyền thao tác cho từng vai trò người dùng trong hệ thống bằng hộp kiểm (Checkbox)
                  </p>
                </div>
              </div>
              <div>
                <Button
                  color="primary"
                  disabled={saving || isAdminRole || !selectedRole}
                  onClick={handleSavePermissions}
                  className="px-4 py-2 fw-semibold rounded-3 d-inline-flex align-items-center gap-2 shadow-sm"
                >
                  {saving ? (
                    <>
                      <Spinner size="sm" />
                      <span>Đang lưu dữ liệu...</span>
                    </>
                  ) : (
                    <>
                      <i className="bi bi-check-lg"></i>
                      <span>Lưu Thay Đổi Quyền</span>
                    </>
                  )}
                </Button>
              </div>
            </div>

            {/* Thông báo Alert */}
            {message && (
              <Alert
                color={message.type}
                className="py-2 px-3 rounded-3 small d-flex align-items-center gap-2 mb-4"
              >
                <i className={`bi ${message.type === 'success' ? 'bi-check-circle-fill' : 'bi-exclamation-triangle-fill'} fs-5`}></i>
                <div>{message.text}</div>
              </Alert>
            )}

            {loading ? (
              <div className="text-center py-5 text-muted">
                <Spinner color="primary" className="mb-2" />
                <p>Đang tải danh sách vai trò và cây phân quyền...</p>
              </div>
            ) : (
              <Row className="g-4">
                {/* Cột trái: Danh sách các Vai Trò (Roles) */}
                <Col md={4} lg={3}>
                  <div className="p-3 bg-light rounded-3 border">
                    <h6 className="fw-bold text-dark mb-3 d-flex align-items-center gap-2">
                      <i className="bi bi-person-badge text-primary"></i>
                      <span>Danh Sách Vai Trò</span>
                    </h6>
                    <div className="d-flex flex-column gap-2">
                      {roles.map((r) => {
                        const isSelected = selectedRole?.name === r.name;
                        const roleVi = ROLE_VIETNAMESE_INFO[r.name];

                        return (
                          <div
                            key={r.name}
                            onClick={() => selectRole(r)}
                            className={`p-3 rounded-3 cursor-pointer transition-all border ${isSelected
                                ? 'bg-white border-primary shadow-sm'
                                : 'bg-white-50 border-transparent hover-bg-light'
                              }`}
                          >
                            <div className="d-flex align-items-center justify-content-between mb-1">
                              <div className="fw-bold text-dark d-flex align-items-center gap-2">
                                <i className={roleVi?.icon || 'bi bi-person text-secondary'}></i>
                                <span>{roleVi?.title || r.name}</span>
                              </div>
                              {r.name === 'Admin' && (
                                <Badge color="warning" pill>
                                  Full
                                </Badge>
                              )}
                            </div>
                            <div className="small text-muted font-monospace">{r.name}</div>
                            <div className="mt-2 d-flex align-items-center justify-content-between text-muted" style={{ fontSize: '12px' }}>
                              <span>{r.name === 'Admin' ? 'Toàn quyền' : `${r.permissions.length} quyền`}</span>
                              {isSelected && <i className="bi bi-arrow-right-short fs-5 text-primary"></i>}
                            </div>
                          </div>
                        );
                      })}
                    </div>
                  </div>
                </Col>

                {/* Cột phải: Cấu hình Checkbox Permissions */}
                <Col md={8} lg={9}>
                  {selectedRole ? (
                    <div className="border rounded-3 p-4 bg-white shadow-sm">
                      {/* Tiêu đề vai trò được chọn */}
                      <div className="d-flex flex-wrap align-items-center justify-content-between gap-3 pb-3 mb-3 border-bottom">
                        <div>
                          <div className="d-flex align-items-center gap-2">
                            <h5 className="fw-bold mb-0 text-dark">
                              {selectedRoleVi?.title || selectedRole.name}
                            </h5>
                            <Badge color="info" className="font-monospace">
                              {selectedRole.name}
                            </Badge>
                          </div>
                          <p className="text-muted small mb-0 mt-1">
                            {selectedRoleVi?.subtitle || 'Tùy chỉnh các quyền thao tác cho vai trò này'}
                          </p>
                        </div>

                        {/* Nút Chọn tất cả / Bỏ chọn */}
                        {!isAdminRole && (
                          <div className="d-flex align-items-center gap-2">
                            <Button
                              size="sm"
                              color="light"
                              className="border text-dark"
                              onClick={() => handleSelectAll(true)}
                            >
                              Chọn tất cả
                            </Button>
                            <Button
                              size="sm"
                              color="light"
                              className="border text-dark"
                              onClick={() => handleSelectAll(false)}
                            >
                              Bỏ chọn
                            </Button>
                          </div>
                        )}
                      </div>

                      {isAdminRole && (
                        <div className="alert alert-warning py-2 mb-3 small d-flex align-items-center gap-2 rounded-3">
                          <i className="bi bi-info-circle-fill fs-5"></i>
                          <div>
                            Tài khoản vai trò <strong>Quản Trị Viên (Admin)</strong> mặc định sở hữu 100% tất cả các quyền Permissions trong toàn bộ hệ thống để đảm bảo luôn có thể quản trị phần mềm.
                          </div>
                        </div>
                      )}

                      {/* Danh sách các nhóm quyền Checkbox */}
                      <div className="d-flex flex-column gap-3">
                        {permissionGroups.map((group) => {
                          const groupPermValues = group.permissions.map((p) => p.value);
                          const isAllGroupChecked =
                            groupPermValues.length > 0 &&
                            groupPermValues.every((val) => selectedPermissions.has(val));

                          const groupViName = GROUP_VIETNAMESE_NAME[group.groupName] || group.groupName;

                          return (
                            <div key={group.groupName} className="border rounded-3 p-3 bg-light-subtle">
                              {/* Header nhóm quyền */}
                              <div className="d-flex align-items-center justify-content-between pb-2 mb-2 border-bottom">
                                <div className="d-flex align-items-center gap-2">
                                  <Input
                                    type="checkbox"
                                    id={`group-${group.groupName}`}
                                    className="cursor-pointer"
                                    checked={isAdminRole || isAllGroupChecked}
                                    disabled={isAdminRole}
                                    onChange={() => handleToggleGroup(group)}
                                  />
                                  <Label
                                    for={`group-${group.groupName}`}
                                    className="fw-bold text-dark mb-0 cursor-pointer d-flex align-items-center gap-1.5"
                                  >
                                    <span>{groupViName}</span>
                                    <span className="text-muted fw-normal small">({group.description})</span>
                                  </Label>
                                </div>
                                <span className="badge bg-white text-secondary border font-monospace small">
                                  {group.permissions.length} quyền
                                </span>
                              </div>

                              {/* Danh sách checkboxes con của từng quyền */}
                              <Row className="g-2">
                                {group.permissions.map((perm) => {
                                  const isChecked = isAdminRole || selectedPermissions.has(perm.value);
                                  const actionViName = ACTION_VIETNAMESE_NAME[perm.name] || perm.name;

                                  return (
                                    <Col md={6} lg={4} key={perm.value}>
                                      <div
                                        className={`p-2 rounded-2 border d-flex align-items-start gap-2 h-100 transition-all ${isChecked
                                            ? 'bg-primary-subtle border-primary-subtle'
                                            : 'bg-white border-light-subtle'
                                          }`}
                                      >
                                        <Input
                                          type="checkbox"
                                          id={`perm-${perm.value}`}
                                          className="mt-1 cursor-pointer"
                                          checked={isChecked}
                                          disabled={isAdminRole}
                                          onChange={() => handleTogglePermission(perm.value)}
                                        />
                                        <Label
                                          for={`perm-${perm.value}`}
                                          className="mb-0 cursor-pointer w-100"
                                        >
                                          <div className="fw-semibold text-dark small">{actionViName}</div>
                                          <div className="text-muted" style={{ fontSize: '11px' }}>
                                            {perm.description || perm.value}
                                          </div>
                                        </Label>
                                      </div>
                                    </Col>
                                  );
                                })}
                              </Row>
                            </div>
                          );
                        })}
                      </div>
                    </div>
                  ) : (
                    <div className="text-center py-5 text-muted">
                      Vui lòng chọn 1 vai trò ở danh sách bên trái để xem và chỉnh sửa quyền.
                    </div>
                  )}
                </Col>
              </Row>
            )}
          </CardBody>
        </Card>
      </Col>
    </Row>
  );
}
