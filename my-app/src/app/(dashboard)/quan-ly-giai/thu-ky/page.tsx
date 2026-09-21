'use client';

import React, { useEffect, useState, useCallback } from 'react';
import {
  Row,
  Col,
  Table,
  Card,
  CardBody,
  Button,
  Input,
  Spinner,
  Modal,
  ModalHeader,
  ModalBody,
  ModalFooter,
  Form,
  FormGroup,
  Label,
  Badge,
} from 'reactstrap';
import { thuKyService } from '@/services';
import { ThuKy, CreateUpdateThuKy } from '@/types';
import { PaginationComponent } from '@/components/common/PaginationComponent';
import { useAuth } from '@/context/AuthContext';
import { Permissions } from '@/constants/permissions';

export default function AdminThuKyPage() {
  const { hasPermission, user } = useAuth();
  const isAdmin = (user?.roles || []).some((r) => String(r).toLowerCase() === 'admin');
  const canCreate = isAdmin || hasPermission(Permissions.ThuKy.Create);
  const canEdit = isAdmin || hasPermission(Permissions.ThuKy.Edit);
  const canDelete = isAdmin || hasPermission(Permissions.ThuKy.Delete);

  const [thuKys, setThuKys] = useState<ThuKy[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(1);
  const [pageIndex, setPageIndex] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [keyword, setKeyword] = useState('');
  const [trangThaiFilter, setTrangThaiFilter] = useState<string>('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Modal Thêm / Sửa
  const [modalOpen, setModalOpen] = useState(false);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [formData, setFormData] = useState<CreateUpdateThuKy>({
    ma: '',
    hoTen: '',
    gioiTinh: 'Nam',
    soDienThoai: '',
    email: '',
    chucVu: '',
    donViCongTac: '',
    trangThai: true,
  });
  const [submitting, setSubmitting] = useState(false);

  // Fetch dữ liệu từ backend
  const loadThuKys = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await thuKyService.getPaged({
        pageIndex,
        pageSize,
        keyword: keyword.trim() || undefined,
        trangThai: trangThaiFilter !== '' ? trangThaiFilter === 'true' : undefined,
      });
      setThuKys(data.items || []);
      setTotalCount(data.totalCount || 0);
      setTotalPages(data.totalPages || 1);
    } catch (err: any) {
      console.error('Lỗi khi tải danh sách thư ký:', err);
      setError(err?.message || 'Không thể kết nối đến máy chủ Backend.');
    } finally {
      setLoading(false);
    }
  }, [pageIndex, pageSize, keyword, trangThaiFilter]);

  useEffect(() => {
    loadThuKys();
  }, [loadThuKys]);

  const handleSearchSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setPageIndex(1);
    loadThuKys();
  };

  const handleOpenCreateModal = () => {
    setEditingId(null);
    setFormData({
      ma: '',
      hoTen: '',
      gioiTinh: 'Nam',
      soDienThoai: '',
      email: '',
      chucVu: '',
      donViCongTac: '',
      trangThai: true,
    });
    setModalOpen(true);
  };

  const handleOpenEditModal = (item: ThuKy) => {
    setEditingId(item.id);
    setFormData({
      ma: item.ma,
      hoTen: item.hoTen,
      gioiTinh: item.gioiTinh || 'Nam',
      soDienThoai: item.soDienThoai || '',
      email: item.email || '',
      chucVu: item.chucVu || '',
      donViCongTac: item.donViCongTac || '',
      trangThai: item.trangThai ?? true,
    });
    setModalOpen(true);
  };

  const handleDelete = async (id: number) => {
    if (!confirm('Bạn có chắc chắn muốn xóa thư ký này?')) return;
    try {
      await thuKyService.delete(id);
      loadThuKys();
    } catch (err: any) {
      alert('Không thể xóa thư ký: ' + (err?.message || 'Lỗi server'));
    }
  };

  const handleSubmitForm = async (e: React.FormEvent) => {
    e.preventDefault();
    setSubmitting(true);
    try {
      if (editingId) {
        await thuKyService.update(editingId, formData);
      } else {
        await thuKyService.create(formData);
      }
      setModalOpen(false);
      loadThuKys();
    } catch (err: any) {
      alert('Lỗi lưu thông tin: ' + (err?.message || 'Lỗi server'));
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <Row>
      <Col lg="12">
        <Card className="border-0 shadow-sm rounded-4 overflow-hidden mb-4">
          <CardBody className="p-4">
            {/* Header */}
            <div className="d-flex flex-wrap justify-content-between align-items-center gap-3 pb-3 mb-4 border-bottom">
              <div>
                <div className="d-flex align-items-center gap-2">
                  <div className="p-2 bg-info-subtle text-info rounded-3 d-inline-flex">
                    <i className="bi bi-file-earmark-person-fill fs-4"></i>
                  </div>
                  <div>
                    <h4 className="fw-bold mb-0 text-dark">Quản lý Thư ký Giải đấu </h4>
                    <p className="text-muted small mb-0">
                      Danh mục hồ sơ thư ký bàn, thư ký kiểm duyệt biên bản trận đấu ({totalCount} thư ký)
                    </p>
                  </div>
                </div>
              </div>
              {canCreate && (
                <div>
                  <Button
                    color="info"
                    onClick={handleOpenCreateModal}
                    className="px-3 py-2 fw-semibold rounded-3 d-inline-flex align-items-center gap-2 shadow-sm text-white"
                  >
                    <i className="bi bi-plus-lg"></i> Thêm thư ký mới
                  </Button>
                </div>
              )}
            </div>

            {/* Thanh tìm kiếm và bộ lọc */}
            <Form onSubmit={handleSearchSubmit} className="mb-4">
              <div className="p-3 bg-light rounded-3">
                <Row className="g-2 align-items-center">
                  <Col md={6} lg={5}>
                    <div className="input-group bg-white rounded-3 overflow-hidden border">
                      <span className="input-group-text bg-white border-0 text-muted ps-3">
                        <i className="bi bi-search"></i>
                      </span>
                      <Input
                        type="text"
                        className="border-0 shadow-none ps-2"
                        placeholder="Tìm kiếm theo mã, họ tên, SĐT, cơ quan..."
                        value={keyword}
                        onChange={(e) => setKeyword(e.target.value)}
                      />
                    </div>
                  </Col>
                  <Col md={3} lg={4}>
                    <Input
                      type="select"
                      className="bg-white border rounded-3 shadow-none"
                      value={trangThaiFilter}
                      onChange={(e) => {
                        setTrangThaiFilter(e.target.value);
                        setPageIndex(1);
                      }}
                    >
                      <option value="">-- Tất cả trạng thái --</option>
                      <option value="true">Đang hoạt động</option>
                      <option value="false">Tạm dừng / Ngừng</option>
                    </Input>
                  </Col>
                  <Col md={3} lg={3}>
                    <Button color="dark" type="submit" className="w-100 rounded-3">
                      Tìm kiếm
                    </Button>
                  </Col>
                </Row>
              </div>
            </Form>

            {/* Thông báo lỗi nếu có */}
            {error && (
              <div className="alert alert-danger d-flex align-items-center mb-4" role="alert">
                <i className="bi bi-exclamation-triangle-fill me-2 fs-5"></i>
                <div>{error}</div>
              </div>
            )}

            {/* Bảng dữ liệu thư ký */}
            {loading ? (
              <div className="text-center py-5">
                <Spinner color="info" />
                <p className="mt-2 text-muted small">Đang tải danh sách thư ký...</p>
              </div>
            ) : thuKys.length === 0 ? (
              <div className="text-center py-5 text-muted border rounded-3 bg-light">
                <i className="bi bi-inbox fs-1 d-block mb-2 text-secondary"></i>
                Chưa có hồ sơ thư ký nào.
              </div>
            ) : (
              <div className="table-responsive">
                <Table hover className="align-middle mb-0">
                  <thead className="table-light text-uppercase fs-7 text-muted">
                    <tr>
                      <th style={{ width: '50px' }}>#</th>
                      <th>Mã Thư Ký</th>
                      <th>Họ và Tên</th>
                      <th>Giới Tính</th>
                      <th>Chức Vụ &amp; Cơ Quan</th>
                      <th>Liên Hệ</th>
                      <th>Trạng Thái</th>
                      <th className="text-end">Hành động</th>
                    </tr>
                  </thead>
                  <tbody>
                    {thuKys.map((item, index) => (
                      <tr key={item.id}>
                        <td className="text-muted small">
                          {(pageIndex - 1) * pageSize + index + 1}
                        </td>
                        <td>
                          <span className="badge bg-light text-dark border font-monospace px-2 py-1">
                            {item.ma}
                          </span>
                        </td>
                        <td>
                          <div className="fw-bold text-dark">{item.hoTen}</div>
                        </td>
                        <td>
                          <span className="badge bg-light text-secondary border">
                            {item.gioiTinh || '—'}
                          </span>
                        </td>
                        <td>
                          <div className="small fw-semibold text-dark">{item.chucVu || '—'}</div>
                          {item.donViCongTac && (
                            <div className="text-muted" style={{ fontSize: '12px' }}>
                              <i className="bi bi-building me-1"></i>
                              {item.donViCongTac}
                            </div>
                          )}
                        </td>
                        <td>
                          <div className="small">
                            {item.soDienThoai && (
                              <div className="text-dark">
                                <i className="bi bi-telephone text-muted me-1"></i>
                                {item.soDienThoai}
                              </div>
                            )}
                            {item.email && (
                              <div className="text-muted" style={{ fontSize: '12px' }}>
                                <i className="bi bi-envelope me-1"></i>
                                {item.email}
                              </div>
                            )}
                            {!item.soDienThoai && !item.email && (
                              <span className="text-muted">—</span>
                            )}
                          </div>
                        </td>
                        <td>
                          <Badge
                            color={item.trangThai ? 'success' : 'secondary'}
                            pill
                            className="px-2 py-1 fw-normal"
                          >
                            {item.trangThai ? 'Hoạt động' : 'Tạm dừng'}
                          </Badge>
                        </td>
                        <td className="text-end">
                          <div className="d-flex justify-content-end gap-1">
                            {canEdit && (
                              <Button
                                size="sm"
                                color="light"
                                className="border text-primary"
                                onClick={() => handleOpenEditModal(item)}
                                title="Chỉnh sửa"
                              >
                                <i className="bi bi-pencil-square"></i>
                              </Button>
                            )}
                            {canDelete && (
                              <Button
                                size="sm"
                                color="light"
                                className="border text-danger"
                                onClick={() => handleDelete(item.id)}
                                title="Xóa"
                              >
                                <i className="bi bi-trash"></i>
                              </Button>
                            )}
                          </div>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </Table>
              </div>
            )}

            {/* Phân trang */}
            {!loading && totalCount > 0 && (
              <div className="mt-4 pt-3 border-top">
                <PaginationComponent
                  pageIndex={pageIndex}
                  totalPages={totalPages}
                  totalCount={totalCount}
                  pageSize={pageSize}
                  onPageChange={(page) => setPageIndex(page)}
                  onPageSizeChange={(size) => {
                    setPageSize(size);
                    setPageIndex(1);
                  }}
                />
              </div>
            )}
          </CardBody>
        </Card>
      </Col>

      {/* Modal Thêm/Sửa Thư Ký */}
      <Modal isOpen={modalOpen} toggle={() => setModalOpen(!modalOpen)} size="lg" centered>
        <Form onSubmit={handleSubmitForm}>
          <ModalHeader toggle={() => setModalOpen(!modalOpen)}>
            <div className="fw-bold d-flex align-items-center gap-2">
              <i className="bi bi-file-earmark-person-fill text-info"></i>
              <span>{editingId ? 'Chỉnh sửa hồ sơ Thư ký' : 'Thêm Thư ký mới'}</span>
            </div>
          </ModalHeader>
          <ModalBody className="p-4">
            <Row>
              <Col md={6}>
                <FormGroup className="mb-3">
                  <Label className="fw-semibold small">Mã Thư Ký *</Label>
                  <Input
                    type="text"
                    required
                    placeholder="VD: TK001, TK_BAN..."
                    value={formData.ma}
                    onChange={(e) => setFormData({ ...formData, ma: e.target.value })}
                  />
                </FormGroup>
              </Col>
              <Col md={6}>
                <FormGroup className="mb-3">
                  <Label className="fw-semibold small">Họ và Tên *</Label>
                  <Input
                    type="text"
                    required
                    placeholder="Nhập họ và tên thư ký..."
                    value={formData.hoTen}
                    onChange={(e) => setFormData({ ...formData, hoTen: e.target.value })}
                  />
                </FormGroup>
              </Col>
            </Row>

            <Row>
              <Col md={6}>
                <FormGroup className="mb-3">
                  <Label className="fw-semibold small">Giới Tính</Label>
                  <Input
                    type="select"
                    value={formData.gioiTinh}
                    onChange={(e) => setFormData({ ...formData, gioiTinh: e.target.value })}
                  >
                    <option value="Nam">Nam</option>
                    <option value="Nữ">Nữ</option>
                    <option value="Khác">Khác</option>
                  </Input>
                </FormGroup>
              </Col>
              <Col md={6}>
                <FormGroup className="mb-3">
                  <Label className="fw-semibold small">Chức Vụ</Label>
                  <Input
                    type="text"
                    placeholder="VD: Thư ký bàn, Trưởng ban biên bản..."
                    value={formData.chucVu}
                    onChange={(e) => setFormData({ ...formData, chucVu: e.target.value })}
                  />
                </FormGroup>
              </Col>
            </Row>

            <FormGroup className="mb-3">
              <Label className="fw-semibold small">Đơn Vị Công Tác / Cơ Quan</Label>
              <Input
                type="text"
                placeholder="VD: Trung tâm Huấn luyện TT Hải Phòng, Sở VHTT..."
                value={formData.donViCongTac}
                onChange={(e) => setFormData({ ...formData, donViCongTac: e.target.value })}
              />
            </FormGroup>

            <Row>
              <Col md={6}>
                <FormGroup className="mb-3">
                  <Label className="fw-semibold small">Số Điện Thoại</Label>
                  <Input
                    type="text"
                    placeholder="0912..."
                    value={formData.soDienThoai}
                    onChange={(e) => setFormData({ ...formData, soDienThoai: e.target.value })}
                  />
                </FormGroup>
              </Col>
              <Col md={6}>
                <FormGroup className="mb-3">
                  <Label className="fw-semibold small">Email</Label>
                  <Input
                    type="email"
                    placeholder="email@sport.vn..."
                    value={formData.email}
                    onChange={(e) => setFormData({ ...formData, email: e.target.value })}
                  />
                </FormGroup>
              </Col>
            </Row>

            <FormGroup switch className="mt-2">
              <Input
                type="switch"
                checked={formData.trangThai}
                onChange={(e) => setFormData({ ...formData, trangThai: e.target.checked })}
              />
              <Label check className="fw-semibold small">
                Đang hoạt động (sẵn sàng làm nhiệm vụ bàn thư ký)
              </Label>
            </FormGroup>
          </ModalBody>
          <ModalFooter>
            <Button color="secondary" onClick={() => setModalOpen(false)}>
              Hủy
            </Button>
            <Button color="info" type="submit" disabled={submitting} className="text-white">
              {submitting ? <Spinner size="sm" /> : editingId ? 'Lưu cập nhật' : 'Tạo mới'}
            </Button>
          </ModalFooter>
        </Form>
      </Modal>
    </Row>
  );
}
