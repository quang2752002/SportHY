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
import { trongTaiService } from '@/services';
import { TrongTai, CreateUpdateTrongTai } from '@/types';
import { PaginationComponent } from '@/components/common/PaginationComponent';
import { useAuth } from '@/context/AuthContext';
import { Permissions } from '@/constants/permissions';

export default function AdminTrongTaiPage() {
  const { hasPermission } = useAuth();
  const canCreate = hasPermission(Permissions.TrongTai.Create);
  const canEdit = hasPermission(Permissions.TrongTai.Edit);
  const canDelete = hasPermission(Permissions.TrongTai.Delete);

  const [trongTais, setTrongTais] = useState<TrongTai[]>([]);
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
  const [formData, setFormData] = useState<CreateUpdateTrongTai>({
    ma: '',
    hoTen: '',
    gioiTinh: 'Nam',
    soDienThoai: '',
    email: '',
    capBac: '',
    trangThai: true,
  });
  const [submitting, setSubmitting] = useState(false);

  // Fetch dữ liệu từ backend
  const loadTrongTais = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await trongTaiService.getPaged({
        pageIndex,
        pageSize,
        keyword: keyword.trim() || undefined,
        trangThai: trangThaiFilter !== '' ? trangThaiFilter === 'true' : undefined,
      });
      setTrongTais(data.items || []);
      setTotalCount(data.totalCount || 0);
      setTotalPages(data.totalPages || 1);
    } catch (err: any) {
      console.error('Lỗi khi tải danh sách trọng tài:', err);
      setError(err?.message || 'Không thể kết nối đến máy chủ Backend.');
    } finally {
      setLoading(false);
    }
  }, [pageIndex, pageSize, keyword, trangThaiFilter]);

  useEffect(() => {
    loadTrongTais();
  }, [loadTrongTais]);

  const handleSearchSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setPageIndex(1);
    loadTrongTais();
  };

  const handleOpenCreateModal = () => {
    setEditingId(null);
    setFormData({
      ma: '',
      hoTen: '',
      gioiTinh: 'Nam',
      soDienThoai: '',
      email: '',
      capBac: '',
      trangThai: true,
    });
    setModalOpen(true);
  };

  const handleOpenEditModal = (item: TrongTai) => {
    setEditingId(item.id);
    setFormData({
      ma: item.ma,
      hoTen: item.hoTen,
      gioiTinh: item.gioiTinh || 'Nam',
      soDienThoai: item.soDienThoai || '',
      email: item.email || '',
      capBac: item.capBac || '',
      trangThai: item.trangThai ?? true,
    });
    setModalOpen(true);
  };

  const handleDelete = async (id: number) => {
    if (!confirm('Bạn có chắc chắn muốn xóa trọng tài này?')) return;
    try {
      await trongTaiService.delete(id);
      loadTrongTais();
    } catch (err: any) {
      alert('Không thể xóa trọng tài: ' + (err?.message || 'Lỗi server'));
    }
  };

  const handleSubmitForm = async (e: React.FormEvent) => {
    e.preventDefault();
    setSubmitting(true);
    try {
      if (editingId) {
        await trongTaiService.update(editingId, formData);
      } else {
        await trongTaiService.create(formData);
      }
      setModalOpen(false);
      loadTrongTais();
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
                  <div className="p-2 bg-primary-subtle text-primary rounded-3 d-inline-flex">
                    <i className="bi bi-whistle fs-4"></i>
                  </div>
                  <div>
                    <h4 className="fw-bold mb-0 text-dark">Quản lý Trọng tài </h4>
                    <p className="text-muted small mb-0">
                      Danh mục hồ sơ trọng tài, điều khiển các trận đấu ({totalCount} trọng tài)
                    </p>
                  </div>
                </div>
              </div>
              {canCreate && (
                <div>
                  <Button
                    color="primary"
                    onClick={handleOpenCreateModal}
                    className="px-3 py-2 fw-semibold rounded-3 d-inline-flex align-items-center gap-2 shadow-sm"
                  >
                    <i className="bi bi-plus-lg"></i> Thêm trọng tài mới
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
                        placeholder="Tìm kiếm theo mã, họ tên, số điện thoại..."
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

            {/* Bảng dữ liệu trọng tài */}
            {loading ? (
              <div className="text-center py-5">
                <Spinner color="primary" />
                <p className="mt-2 text-muted small">Đang tải danh sách trọng tài...</p>
              </div>
            ) : trongTais.length === 0 ? (
              <div className="text-center py-5 text-muted border rounded-3 bg-light">
                <i className="bi bi-inbox fs-1 d-block mb-2 text-secondary"></i>
                Chưa có hồ sơ trọng tài nào.
              </div>
            ) : (
              <div className="table-responsive">
                <Table hover className="align-middle mb-0">
                  <thead className="table-light text-uppercase fs-7 text-muted">
                    <tr>
                      <th style={{ width: '50px' }}>#</th>
                      <th>Mã Trọng Tài</th>
                      <th>Họ và Tên</th>
                      <th>Giới Tính</th>
                      <th>Cấp Bậc / Bằng Cấp</th>
                      <th>Liên Hệ</th>
                      <th>Trạng Thái</th>
                      <th className="text-end" style={{ width: '130px' }}>
                        Thao tác
                      </th>
                    </tr>
                  </thead>
                  <tbody>
                    {trongTais.map((item, index) => (
                      <tr key={item.id}>
                        <td className="text-muted">{(pageIndex - 1) * pageSize + index + 1}</td>
                        <td>
                          <span className="badge bg-light text-primary border font-monospace">
                            {item.ma}
                          </span>
                        </td>
                        <td>
                          <div className="fw-semibold text-dark">{item.hoTen}</div>
                        </td>
                        <td>
                          <span className="small text-muted">{item.gioiTinh || '---'}</span>
                        </td>
                        <td>
                          {item.capBac ? (
                            <Badge color="info" pill>
                              {item.capBac}
                            </Badge>
                          ) : (
                            <span className="text-muted small">---</span>
                          )}
                        </td>
                        <td>
                          <div className="small">
                            {item.soDienThoai && (
                              <div>
                                <i className="bi bi-telephone me-1 text-primary"></i>
                                {item.soDienThoai}
                              </div>
                            )}
                            {item.email && (
                              <div className="text-muted">
                                <i className="bi bi-envelope me-1 text-secondary"></i>
                                {item.email}
                              </div>
                            )}
                            {!item.soDienThoai && !item.email && '---'}
                          </div>
                        </td>
                        <td>
                          {item.trangThai ? (
                            <span className="badge rounded-pill bg-success-subtle text-success border border-success-subtle px-3 py-2 fw-medium d-inline-flex align-items-center gap-1">
                              <span className="p-1 bg-success rounded-circle d-inline-block" />
                              Hoạt động
                            </span>
                          ) : (
                            <span className="badge rounded-pill bg-secondary-subtle text-secondary border border-secondary-subtle px-3 py-2 fw-medium d-inline-flex align-items-center gap-1">
                              <span className="p-1 bg-secondary rounded-circle d-inline-block" />
                              Tạm dừng
                            </span>
                          )}
                        </td>
                        <td className="text-end">
                          <div className="d-flex justify-content-end gap-1">
                            {canEdit && (
                              <Button
                                size="sm"
                                color="light"
                                className="btn-icon text-primary"
                                title="Chỉnh sửa trọng tài"
                                onClick={() => handleOpenEditModal(item)}
                              >
                                <i className="bi bi-pencil-square"></i>
                              </Button>
                            )}
                            {canDelete && (
                              <Button
                                size="sm"
                                color="light"
                                className="btn-icon text-danger"
                                title="Xóa trọng tài"
                                onClick={() => handleDelete(item.id)}
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

      {/* Modal Thêm/Sửa Trọng Tài */}
      <Modal isOpen={modalOpen} toggle={() => setModalOpen(!modalOpen)} size="lg" centered>
        <ModalHeader toggle={() => setModalOpen(!modalOpen)} className="border-bottom">
          <div className="d-flex align-items-center gap-2">
            <i className={`bi ${editingId ? 'bi-pencil-square text-primary' : 'bi-plus-circle-fill text-success'}`}></i>
            <span className="fw-bold">{editingId ? 'Cập Nhật Trọng Tài' : 'Thêm Trọng Tài Mới'}</span>
          </div>
        </ModalHeader>
        <Form onSubmit={handleSubmitForm}>
          <ModalBody className="p-4">
            <Row className="g-3">
              <Col md={4}>
                <FormGroup>
                  <Label className="fw-semibold small">
                    Mã trọng tài <span className="text-danger">*</span>
                  </Label>
                  <Input
                    type="text"
                    required
                    placeholder="VD: TT_001"
                    value={formData.ma}
                    onChange={(e) => setFormData({ ...formData, ma: e.target.value })}
                  />
                </FormGroup>
              </Col>
              <Col md={8}>
                <FormGroup>
                  <Label className="fw-semibold small">
                    Họ và tên trọng tài <span className="text-danger">*</span>
                  </Label>
                  <Input
                    type="text"
                    required
                    placeholder="VD: Nguyễn Văn Nam"
                    value={formData.hoTen}
                    onChange={(e) => setFormData({ ...formData, hoTen: e.target.value })}
                  />
                </FormGroup>
              </Col>

              <Col md={6}>
                <FormGroup>
                  <Label className="fw-semibold small">Giới tính</Label>
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
                <FormGroup>
                  <Label className="fw-semibold small">Cấp bậc / Bằng cấp trọng tài</Label>
                  <Input
                    type="text"
                    placeholder="VD: Cấp Quốc Gia, Trọng tài FIFA, Cấp Tỉnh..."
                    value={formData.capBac}
                    onChange={(e) => setFormData({ ...formData, capBac: e.target.value })}
                  />
                </FormGroup>
              </Col>

              <Col md={6}>
                <FormGroup>
                  <Label className="fw-semibold small">Số điện thoại</Label>
                  <Input
                    type="text"
                    placeholder="VD: 0988123456"
                    value={formData.soDienThoai}
                    onChange={(e) => setFormData({ ...formData, soDienThoai: e.target.value })}
                  />
                </FormGroup>
              </Col>

              <Col md={6}>
                <FormGroup>
                  <Label className="fw-semibold small">Email</Label>
                  <Input
                    type="email"
                    placeholder="VD: trongtai@example.com"
                    value={formData.email}
                    onChange={(e) => setFormData({ ...formData, email: e.target.value })}
                  />
                </FormGroup>
              </Col>

              <Col md={12}>
                <FormGroup check className="mt-2">
                  <Input
                    type="checkbox"
                    id="trangThaiTrongTai"
                    checked={formData.trangThai}
                    onChange={(e) => setFormData({ ...formData, trangThai: e.target.checked })}
                  />
                  <Label check htmlFor="trangThaiTrongTai" className="fw-semibold small">
                    Đang hoạt động (cho phép phân công trận đấu)
                  </Label>
                </FormGroup>
              </Col>
            </Row>
          </ModalBody>
          <ModalFooter className="border-top">
            <Button color="light" onClick={() => setModalOpen(false)} disabled={submitting}>
              Hủy
            </Button>
            <Button color="primary" type="submit" disabled={submitting} className="d-inline-flex align-items-center gap-2">
              {submitting && <Spinner size="sm" />}
              {editingId ? 'Cập nhật' : 'Tạo trọng tài'}
            </Button>
          </ModalFooter>
        </Form>
      </Modal>
    </Row>
  );
}
