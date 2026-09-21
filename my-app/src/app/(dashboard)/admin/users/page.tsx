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
  Modal,
  ModalHeader,
  ModalBody,
  ModalFooter,
  Form,
  FormGroup,
  Table,
} from 'reactstrap';
import { roleService } from '@/services';
import { Role, UserManagement, MapSources } from '@/types';

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

export default function AdminUsersManagementPage() {
  const [roles, setRoles] = useState<Role[]>([]);
  const [users, setUsers] = useState<UserManagement[]>([]);
  const [mapSources, setMapSources] = useState<MapSources>({ donVis: [], trongTais: [], thuKys: [] });
  const [loading, setLoading] = useState(false);
  const [message, setMessage] = useState<{ type: 'success' | 'danger'; text: string } | null>(null);

  // Modal cấp tài khoản mới
  const [modalCreateOpen, setModalCreateOpen] = useState(false);
  const [createForm, setCreateForm] = useState({
    username: '',
    email: '',
    password: '',
    fullName: '',
    phoneNumber: '',
    role: 'Manager',
    donViId: '' as string | number,
    trongTaiId: '' as string | number,
    thuKyId: '' as string | number,
  });
  const [creatingUser, setCreatingUser] = useState(false);

  // Modal đổi role
  const [modalRoleOpen, setModalRoleOpen] = useState(false);
  const [selectedUserForRole, setSelectedUserForRole] = useState<UserManagement | null>(null);
  const [selectedNewRole, setSelectedNewRole] = useState('');
  const [selectedDonViId, setSelectedDonViId] = useState<string | number>('');
  const [selectedTrongTaiId, setSelectedTrongTaiId] = useState<string | number>('');
  const [selectedThuKyId, setSelectedThuKyId] = useState<string | number>('');
  const [updatingRole, setUpdatingRole] = useState(false);

  const fetchData = async () => {
    setLoading(true);
    try {
      const [usersData, rolesData, mapSourcesData] = await Promise.all([
        roleService.getUsers(),
        roleService.getRoles(),
        roleService.getMapSources(),
      ]);
      setUsers(usersData);
      setRoles(rolesData);
      setMapSources(mapSourcesData);
    } catch (err: any) {
      console.error('Lỗi khi tải dữ liệu người dùng:', err);
      setMessage({ type: 'danger', text: 'Không thể kết nối đến máy chủ Backend để tải danh sách.' });
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
  }, []);

  const handleCreateUser = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!createForm.username || !createForm.password) {
      alert('Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu.');
      return;
    }

    if (createForm.role === 'Delegation' && !createForm.donViId) {
      alert('Vui lòng chọn Đơn vị / Đoàn thể thao liên kết cho tài khoản này.');
      return;
    }

    if ((createForm.role === 'Referee' || createForm.role === 'HeadReferee') && !createForm.trongTaiId) {
      alert('Vui lòng chọn Trọng tài liên kết cho tài khoản này.');
      return;
    }

    if (createForm.role === 'Secretary' && !createForm.thuKyId) {
      alert('Vui lòng chọn Thư ký bàn / Thư ký giải liên kết cho tài khoản này.');
      return;
    }

    setCreatingUser(true);
    try {
      const res = await roleService.createUserWithRole({
        ...createForm,
        donViId: createForm.role === 'Delegation' && createForm.donViId ? Number(createForm.donViId) : null,
        trongTaiId: (createForm.role === 'Referee' || createForm.role === 'HeadReferee') && createForm.trongTaiId ? Number(createForm.trongTaiId) : null,
        thuKyId: createForm.role === 'Secretary' && createForm.thuKyId ? Number(createForm.thuKyId) : null,
      });
      setMessage({ type: 'success', text: res.message || 'Cấp tài khoản mới thành công!' });
      setModalCreateOpen(false);
      setCreateForm({
        username: '',
        email: '',
        password: '',
        fullName: '',
        phoneNumber: '',
        role: 'Manager',
        donViId: '',
        trongTaiId: '',
        thuKyId: '',
      });
      fetchData();
    } catch (err: any) {
      alert(err?.response?.data?.message || err?.message || 'Có lỗi khi cấp tài khoản.');
    } finally {
      setCreatingUser(false);
    }
  };

  const handleUpdateRole = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedUserForRole) return;

    if (selectedNewRole === 'Delegation' && !selectedDonViId) {
      alert('Vui lòng chọn Đơn vị / Đoàn thể thao liên kết.');
      return;
    }

    if ((selectedNewRole === 'Referee' || selectedNewRole === 'HeadReferee') && !selectedTrongTaiId) {
      alert('Vui lòng chọn Trọng tài liên kết.');
      return;
    }

    if (selectedNewRole === 'Secretary' && !selectedThuKyId) {
      alert('Vui lòng chọn Thư ký liên kết.');
      return;
    }

    setUpdatingRole(true);
    try {
      const res = await roleService.updateUserRole({
        userId: selectedUserForRole.id,
        role: selectedNewRole,
        donViId: selectedNewRole === 'Delegation' && selectedDonViId ? Number(selectedDonViId) : null,
        trongTaiId: (selectedNewRole === 'Referee' || selectedNewRole === 'HeadReferee') && selectedTrongTaiId ? Number(selectedTrongTaiId) : null,
        thuKyId: selectedNewRole === 'Secretary' && selectedThuKyId ? Number(selectedThuKyId) : null,
      });
      setMessage({ type: 'success', text: res.message || 'Cập nhật vai trò thành công!' });
      setModalRoleOpen(false);
      fetchData();
    } catch (err: any) {
      alert(err?.response?.data?.message || err?.message || 'Có lỗi khi đổi vai trò.');
    } finally {
      setUpdatingRole(false);
    }
  };

  return (
    <Row>
      <Col lg="12">
        <Card className="border-0 shadow-sm rounded-4 overflow-hidden mb-4">
          <CardBody className="p-4">
            {/* Header */}
            <div className="d-flex flex-wrap justify-content-between align-items-center gap-3 pb-3 mb-4 border-bottom">
              <div className="d-flex align-items-center gap-2">
                <div className="p-2 bg-success-subtle text-success rounded-3 d-inline-flex">
                  <i className="bi bi-person-badge-fill fs-4"></i>
                </div>
                <div>
                  <h4 className="fw-bold mb-0 text-dark">Quản Lý &amp; Cấp Tài Khoản Người Dùng</h4>
                  <p className="text-muted small mb-0">
                    Tạo tài khoản, chỉ định vai trò và liên kết trực tiếp với Đơn vị hoặc Trọng tài
                  </p>
                </div>
              </div>
              <div>
                <Button
                  color="success"
                  onClick={() => setModalCreateOpen(true)}
                  className="px-4 py-2 fw-semibold rounded-3 d-inline-flex align-items-center gap-2 shadow-sm text-white"
                >
                  <i className="bi bi-person-plus-fill"></i>
                  <span>Cấp Tài Khoản Mới</span>
                </Button>
              </div>
            </div>

            {/* Alert */}
            {message && (
              <Alert
                color={message.type}
                className="py-2 px-3 rounded-3 small d-flex align-items-center gap-2 mb-4"
              >
                <i className={`bi ${message.type === 'success' ? 'bi-check-circle-fill' : 'bi-exclamation-triangle-fill'} fs-5`}></i>
                <div>{message.text}</div>
              </Alert>
            )}

            {/* Table */}
            {loading ? (
              <div className="text-center py-5 text-muted">
                <Spinner color="primary" className="mb-2" />
                <p>Đang tải danh sách tài khoản người dùng...</p>
              </div>
            ) : (
              <div className="table-responsive bg-white rounded-3 border">
                <Table hover className="align-middle mb-0">
                  <thead className="table-light">
                    <tr>
                      <th className="py-3 ps-3">Tên đăng nhập</th>
                      <th className="py-3">Họ và tên</th>
                      <th className="py-3">Email &amp; SĐT</th>
                      <th className="py-3">Vai trò phân công</th>
                      <th className="py-3">Đối tượng liên kết</th>
                      <th className="py-3 text-end pe-3">Thao tác</th>
                    </tr>
                  </thead>
                  <tbody>
                    {users.map((u) => {
                      const primaryRole = u.roles[0] || 'User';
                      const roleInfo = ROLE_VIETNAMESE_INFO[primaryRole];

                      return (
                        <tr key={u.id}>
                          <td className="ps-3 fw-bold text-dark">
                            <div className="d-flex align-items-center gap-2">
                              <div className="p-2 bg-primary-subtle text-primary rounded-circle d-inline-flex">
                                <i className="bi bi-person-fill"></i>
                              </div>
                              <span>{u.username}</span>
                            </div>
                          </td>
                          <td className="text-secondary">{u.fullName || '—'}</td>
                          <td>
                            <div className="small text-dark">{u.email || '—'}</div>
                            {u.phoneNumber && (
                              <div className="text-muted" style={{ fontSize: '12px' }}>
                                <i className="bi bi-telephone me-1"></i>
                                {u.phoneNumber}
                              </div>
                            )}
                          </td>
                          <td>
                            <Badge
                              color={
                                primaryRole === 'Admin'
                                  ? 'warning'
                                  : primaryRole === 'Manager'
                                    ? 'primary'
                                    : primaryRole === 'HeadReferee' || primaryRole === 'Referee'
                                      ? 'success'
                                      : primaryRole === 'Delegation'
                                        ? 'danger'
                                        : 'info'
                              }
                              pill
                              className="px-2.5 py-1.5 fw-semibold"
                            >
                              {roleInfo?.title || primaryRole}
                            </Badge>
                          </td>
                          <td>
                            {u.tenDonVi ? (
                              <span className="badge bg-danger-subtle text-danger border border-danger-subtle px-2.5 py-1.5 rounded-pill d-inline-flex align-items-center gap-1">
                                <i className="bi bi-building"></i>
                                <span>{u.tenDonVi}</span>
                              </span>
                            ) : u.tenTrongTai ? (
                              <span className="badge bg-success-subtle text-success border border-success-subtle px-2.5 py-1.5 rounded-pill d-inline-flex align-items-center gap-1">
                                <i className="bi bi-stopwatch"></i>
                                <span>{u.tenTrongTai}</span>
                              </span>
                            ) : u.tenThuKy ? (
                              <span className="badge bg-info-subtle text-info border border-info-subtle px-2.5 py-1.5 rounded-pill d-inline-flex align-items-center gap-1">
                                <i className="bi bi-file-earmark-person"></i>
                                <span>{u.tenThuKy}</span>
                              </span>
                            ) : (
                              <span className="text-muted small">— Không liên kết —</span>
                            )}
                          </td>
                          <td className="text-end pe-3">
                            <Button
                              size="sm"
                              color="light"
                              className="border text-primary fw-semibold"
                              onClick={() => {
                                setSelectedUserForRole(u);
                                setSelectedNewRole(u.roles[0] || 'Manager');
                                setSelectedDonViId(u.donViId || '');
                                setSelectedTrongTaiId(u.trongTaiId || '');
                                setSelectedThuKyId(u.thuKyId || '');
                                setModalRoleOpen(true);
                              }}
                            >
                              <i className="bi bi-shield-shaded me-1"></i>
                              Đổi Vai Trò
                            </Button>
                          </td>
                        </tr>
                      );
                    })}
                  </tbody>
                </Table>
              </div>
            )}
          </CardBody>
        </Card>
      </Col>

      {/* MODAL CẤP TÀI KHOẢN MỚI */}
      <Modal isOpen={modalCreateOpen} toggle={() => setModalCreateOpen(!modalCreateOpen)} centered>
        <Form onSubmit={handleCreateUser}>
          <ModalHeader toggle={() => setModalCreateOpen(!modalCreateOpen)}>
            <div className="fw-bold d-flex align-items-center gap-2">
              <i className="bi bi-person-plus-fill text-success"></i>
              <span>Cấp Tài Khoản Người Dùng Mới</span>
            </div>
          </ModalHeader>
          <ModalBody className="p-4">
            <FormGroup className="mb-3">
              <Label className="fw-semibold">Tên đăng nhập (Username) *</Label>
              <Input
                required
                value={createForm.username}
                onChange={(e) => setCreateForm({ ...createForm, username: e.target.value })}
                placeholder="VD: doan_hanoi, trongtai_minh..."
              />
            </FormGroup>

            <FormGroup className="mb-3">
              <Label className="fw-semibold">Mật khẩu ban đầu *</Label>
              <Input
                required
                type="password"
                value={createForm.password}
                onChange={(e) => setCreateForm({ ...createForm, password: e.target.value })}
                placeholder="Tối thiểu 6 ký tự..."
              />
            </FormGroup>

            <FormGroup className="mb-3">
              <Label className="fw-semibold">Họ và tên</Label>
              <Input
                value={createForm.fullName}
                onChange={(e) => setCreateForm({ ...createForm, fullName: e.target.value })}
                placeholder="VD: Nguyễn Văn A..."
              />
            </FormGroup>

            <Row>
              <Col md={6}>
                <FormGroup className="mb-3">
                  <Label className="fw-semibold">Email</Label>
                  <Input
                    type="email"
                    value={createForm.email}
                    onChange={(e) => setCreateForm({ ...createForm, email: e.target.value })}
                    placeholder="email@sport.vn..."
                  />
                </FormGroup>
              </Col>
              <Col md={6}>
                <FormGroup className="mb-3">
                  <Label className="fw-semibold">Số điện thoại</Label>
                  <Input
                    value={createForm.phoneNumber}
                    onChange={(e) => setCreateForm({ ...createForm, phoneNumber: e.target.value })}
                    placeholder="0912..."
                  />
                </FormGroup>
              </Col>
            </Row>

            <FormGroup className="mb-3">
              <Label className="fw-semibold text-primary">Gán Vai Trò (Role) *</Label>
              <Input
                type="select"
                value={createForm.role}
                onChange={(e) => setCreateForm({ ...createForm, role: e.target.value })}
              >
                {roles.map((r) => (
                  <option key={r.name} value={r.name}>
                    {ROLE_VIETNAMESE_INFO[r.name]?.title || r.name} ({r.name})
                  </option>
                ))}
              </Input>
            </FormGroup>

            {/* Nếu chọn Role Delegation -> Chọn Đơn vị */}
            {createForm.role === 'Delegation' && (
              <FormGroup className="mb-3 p-3 bg-danger-subtle rounded-3 border border-danger-subtle">
                <Label className="fw-bold text-danger d-flex align-items-center gap-1">
                  <i className="bi bi-building"></i>
                  <span>Chọn Đơn Vị / Đoàn Thể Thao Liên Kết *</span>
                </Label>
                <Input
                  type="select"
                  required
                  value={createForm.donViId}
                  onChange={(e) => setCreateForm({ ...createForm, donViId: e.target.value })}
                >
                  <option value="">-- Chọn Đơn vị trực thuộc --</option>
                  {mapSources.donVis.map((d) => (
                    <option key={d.id} value={d.id}>
                      {d.name} {d.code ? `(${d.code})` : ''}
                    </option>
                  ))}
                </Input>
                <small className="text-muted mt-1 d-block">
                  Tài khoản này chỉ được quản lý VĐV &amp; danh sách đoàn thuộc đơn vị đã chọn.
                </small>
              </FormGroup>
            )}

            {/* Nếu chọn Role Referee / HeadReferee -> Chọn Trọng tài */}
            {(createForm.role === 'Referee' || createForm.role === 'HeadReferee') && (
              <FormGroup className="mb-3 p-3 bg-success-subtle rounded-3 border border-success-subtle">
                <Label className="fw-bold text-success d-flex align-items-center gap-1">
                  <i className="bi bi-stopwatch"></i>
                  <span>Chọn Trọng Tài Liên Kết *</span>
                </Label>
                <Input
                  type="select"
                  required
                  value={createForm.trongTaiId}
                  onChange={(e) => setCreateForm({ ...createForm, trongTaiId: e.target.value })}
                >
                  <option value="">-- Chọn hồ sơ Trọng tài tương ứng --</option>
                  {mapSources.trongTais.map((t) => (
                    <option key={t.id} value={t.id}>
                      {t.name} {t.code ? `(${t.code})` : ''}
                    </option>
                  ))}
                </Input>
                <small className="text-muted mt-1 d-block">
                  Tài khoản này sẽ liên kết trực tiếp với lịch phân công trọng tài của giải.
                </small>
              </FormGroup>
            )}

            {/* Nếu chọn Role Secretary -> Chọn Thư ký */}
            {createForm.role === 'Secretary' && (
              <FormGroup className="mb-3 p-3 bg-info-subtle rounded-3 border border-info-subtle">
                <Label className="fw-bold text-info d-flex align-items-center gap-1">
                  <i className="bi bi-file-earmark-person"></i>
                  <span>Chọn Thư Ký Bàn / Thư Ký Giải Liên Kết *</span>
                </Label>
                <Input
                  type="select"
                  required
                  value={createForm.thuKyId}
                  onChange={(e) => setCreateForm({ ...createForm, thuKyId: e.target.value })}
                >
                  <option value="">-- Chọn hồ sơ Thư ký tương ứng --</option>
                  {mapSources.thuKys.map((k) => (
                    <option key={k.id} value={k.id}>
                      {k.name} {k.code ? `(${k.code})` : ''}
                    </option>
                  ))}
                </Input>
                <small className="text-muted mt-1 d-block">
                  Tài khoản này sẽ đại diện kiểm duyệt biên bản, xuất kết quả thi đấu.
                </small>
              </FormGroup>
            )}
          </ModalBody>
          <ModalFooter>
            <Button color="secondary" onClick={() => setModalCreateOpen(false)}>
              Hủy
            </Button>
            <Button color="success" type="submit" disabled={creatingUser} className="text-white">
              {creatingUser ? <Spinner size="sm" /> : 'Tạo Tài Khoản & Gán Role'}
            </Button>
          </ModalFooter>
        </Form>
      </Modal>

      {/* MODAL ĐỔI VAI TRÒ */}
      <Modal isOpen={modalRoleOpen} toggle={() => setModalRoleOpen(!modalRoleOpen)} centered>
        <Form onSubmit={handleUpdateRole}>
          <ModalHeader toggle={() => setModalRoleOpen(!modalRoleOpen)}>
            <div className="fw-bold d-flex align-items-center gap-2">
              <i className="bi bi-shield-shaded text-primary"></i>
              <span>Phân Bổ Lại Vai Trò Cho Tài Khoản</span>
            </div>
          </ModalHeader>
          <ModalBody className="p-4">
            <div className="p-3 bg-light rounded-3 mb-3">
              <div className="small text-muted">Tài khoản:</div>
              <div className="fw-bold text-dark fs-6">{selectedUserForRole?.username}</div>
              <div className="small text-muted mt-1">
                Họ tên: <span className="text-dark">{selectedUserForRole?.fullName || '—'}</span>
              </div>
            </div>

            <FormGroup className="mb-3">
              <Label className="fw-semibold">Chọn Vai Trò (Role) Mới *</Label>
              <Input
                type="select"
                value={selectedNewRole}
                onChange={(e) => setSelectedNewRole(e.target.value)}
              >
                {roles.map((r) => (
                  <option key={r.name} value={r.name}>
                    {ROLE_VIETNAMESE_INFO[r.name]?.title || r.name} ({r.name})
                  </option>
                ))}
              </Input>
            </FormGroup>

            {/* Nếu chọn Role Delegation -> Chọn Đơn vị */}
            {selectedNewRole === 'Delegation' && (
              <FormGroup className="mb-3 p-3 bg-danger-subtle rounded-3 border border-danger-subtle">
                <Label className="fw-bold text-danger d-flex align-items-center gap-1">
                  <i className="bi bi-building"></i>
                  <span>Chọn Đơn Vị / Đoàn Thể Thao Liên Kết *</span>
                </Label>
                <Input
                  type="select"
                  required
                  value={selectedDonViId}
                  onChange={(e) => setSelectedDonViId(e.target.value)}
                >
                  <option value="">-- Chọn Đơn vị trực thuộc --</option>
                  {mapSources.donVis.map((d) => (
                    <option key={d.id} value={d.id}>
                      {d.name} {d.code ? `(${d.code})` : ''}
                    </option>
                  ))}
                </Input>
              </FormGroup>
            )}

            {/* Nếu chọn Role Referee / HeadReferee -> Chọn Trọng tài */}
            {(selectedNewRole === 'Referee' || selectedNewRole === 'HeadReferee') && (
              <FormGroup className="mb-3 p-3 bg-success-subtle rounded-3 border border-success-subtle">
                <Label className="fw-bold text-success d-flex align-items-center gap-1">
                  <i className="bi bi-stopwatch"></i>
                  <span>Chọn Trọng Tài Liên Kết *</span>
                </Label>
                <Input
                  type="select"
                  required
                  value={selectedTrongTaiId}
                  onChange={(e) => setSelectedTrongTaiId(e.target.value)}
                >
                  <option value="">-- Chọn hồ sơ Trọng tài tương ứng --</option>
                  {mapSources.trongTais.map((t) => (
                    <option key={t.id} value={t.id}>
                      {t.name} {t.code ? `(${t.code})` : ''}
                    </option>
                  ))}
                </Input>
              </FormGroup>
            )}

            {/* Nếu chọn Role Secretary -> Chọn Thư ký */}
            {selectedNewRole === 'Secretary' && (
              <FormGroup className="mb-3 p-3 bg-info-subtle rounded-3 border border-info-subtle">
                <Label className="fw-bold text-info d-flex align-items-center gap-1">
                  <i className="bi bi-file-earmark-person"></i>
                  <span>Chọn Thư Ký Bàn / Thư Ký Giải Liên Kết *</span>
                </Label>
                <Input
                  type="select"
                  required
                  value={selectedThuKyId}
                  onChange={(e) => setSelectedThuKyId(e.target.value)}
                >
                  <option value="">-- Chọn hồ sơ Thư ký tương ứng --</option>
                  {mapSources.thuKys.map((k) => (
                    <option key={k.id} value={k.id}>
                      {k.name} {k.code ? `(${k.code})` : ''}
                    </option>
                  ))}
                </Input>
              </FormGroup>
            )}
          </ModalBody>
          <ModalFooter>
            <Button color="secondary" onClick={() => setModalRoleOpen(false)}>
              Hủy
            </Button>
            <Button color="primary" type="submit" disabled={updatingRole}>
              {updatingRole ? <Spinner size="sm" /> : 'Lưu Thay Đổi'}
            </Button>
          </ModalFooter>
        </Form>
      </Modal>
    </Row>
  );
}
