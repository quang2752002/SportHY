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
import { vanDongVienService, donViService } from '@/services';
import { VanDongVien, CreateUpdateVanDongVien, DonVi } from '@/types';
import { PaginationComponent } from '@/components/common/PaginationComponent';
import { useAuth } from '@/context/AuthContext';
import { Permissions } from '@/constants/permissions';

export default function AdminVanDongVienPage() {
  const { hasPermission } = useAuth();
  const canCreate = hasPermission(Permissions.VanDongVien.Create);
  const canEdit = hasPermission(Permissions.VanDongVien.Edit);
  const canDelete = hasPermission(Permissions.VanDongVien.Delete);

  const [vdvs, setVdvs] = useState<VanDongVien[]>([]);
  const [donVis, setDonVis] = useState<DonVi[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(1);
  const [pageIndex, setPageIndex] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [keyword, setKeyword] = useState('');
  const [donViFilter, setDonViFilter] = useState<string>('');
  const [trangThaiFilter, setTrangThaiFilter] = useState<string>('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Modal Thêm / Sửa
  const [modalOpen, setModalOpen] = useState(false);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [formData, setFormData] = useState<CreateUpdateVanDongVien>({
    ma: '',
    hoTen: '',
    donViId: undefined,
    ngaySinh: '',
    gioiTinh: 'Nam',
    soDienThoai: '',
    email: '',
    soCCCD: '',
    diaChi: '',
    hinhAnh: '',
    trangThai: true,
  });
  const [submitting, setSubmitting] = useState(false);
  const [uploadingAvatar, setUploadingAvatar] = useState(false);

  // Modal Xóa
  const [deleteModalOpen, setDeleteModalOpen] = useState(false);
  const [deletingItem, setDeletingItem] = useState<VanDongVien | null>(null);

  // Load danh sách đơn vị để lọc và chọn
  useEffect(() => {
    const fetchDonVis = async () => {
      try {
        const res = await donViService.getAll();
        setDonVis(res || []);
      } catch (err) {
        console.error('Lỗi khi tải danh sách đơn vị:', err);
      }
    };
    fetchDonVis();
  }, []);

  // Handler upload ảnh đại diện / chân dung VĐV
  const handleAvatarUpload = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    setUploadingAvatar(true);
    try {
      const res = await vanDongVienService.uploadAvatar(file);
      if (res?.url) {
        setFormData((prev) => ({ ...prev, hinhAnh: res.url }));
      }
    } catch (err: any) {
      alert('Không thể tải lên ảnh VĐV: ' + (err?.response?.data?.message || err?.message || 'Lỗi server'));
    } finally {
      setUploadingAvatar(false);
    }
  };

  // Nạp danh sách VĐV
  const loadVdvs = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const params: any = {
        pageIndex,
        pageSize,
        keyword: keyword.trim() || undefined,
      };
      if (donViFilter) params.donViId = Number(donViFilter);
      if (trangThaiFilter !== '') params.trangThai = trangThaiFilter === 'true';

      const res = await vanDongVienService.getPaged(params);
      if (res) {
        setVdvs(res.items || []);
        setTotalCount(res.totalCount || 0);
        setTotalPages(res.totalPages || 1);
      }
    } catch (err: any) {
      console.error('Lỗi khi tải danh sách VĐV:', err);
      setError(err?.response?.data?.message || err?.message || 'Không thể tải danh sách vận động viên.');
    } finally {
      setLoading(false);
    }
  }, [pageIndex, pageSize, keyword, donViFilter, trangThaiFilter]);

  useEffect(() => {
    loadVdvs();
  }, [loadVdvs]);

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    setPageIndex(1);
    loadVdvs();
  };

  // Mở modal Thêm mới
  const handleOpenCreateModal = () => {
    setEditingId(null);
    setFormData({
      ma: '',
      hoTen: '',
      donViId: donVis.length > 0 ? donVis[0].id : undefined,
      ngaySinh: '',
      gioiTinh: 'Nam',
      soDienThoai: '',
      email: '',
      soCCCD: '',
      diaChi: '',
      hinhAnh: '',
      trangThai: true,
    });
    setModalOpen(true);
  };

  // Mở modal Sửa
  const handleOpenEditModal = (item: VanDongVien) => {
    setEditingId(item.id);
    const formatDate = (dateStr?: string) => {
      if (!dateStr) return '';
      return new Date(dateStr).toISOString().split('T')[0];
    };
    setFormData({
      ma: item.ma,
      hoTen: item.hoTen,
      donViId: item.donViId,
      ngaySinh: formatDate(item.ngaySinh),
      gioiTinh: item.gioiTinh || 'Nam',
      soDienThoai: item.soDienThoai || '',
      email: item.email || '',
      soCCCD: item.soCCCD || '',
      diaChi: item.diaChi || '',
      hinhAnh: item.hinhAnh || '',
      trangThai: item.trangThai,
    });
    setModalOpen(true);
  };

  // Lưu VĐV
  const handleSubmitForm = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!formData.ma.trim() || !formData.hoTen.trim()) {
      alert('Vui lòng nhập đầy đủ Mã VĐV và Họ tên!');
      return;
    }

    setSubmitting(true);
    try {
      if (editingId) {
        await vanDongVienService.update(editingId, formData);
      } else {
        await vanDongVienService.create(formData);
      }
      setModalOpen(false);
      loadVdvs();
    } catch (err: any) {
      console.error('Lỗi khi lưu VĐV:', err);
      alert(err?.response?.data?.message || err?.message || 'Có lỗi xảy ra khi lưu VĐV.');
    } finally {
      setSubmitting(false);
    }
  };

  // Xác nhận xóa
  const handleOpenDeleteModal = (item: VanDongVien) => {
    setDeletingItem(item);
    setDeleteModalOpen(true);
  };

  const handleConfirmDelete = async () => {
    if (!deletingItem) return;
    setSubmitting(true);
    try {
      await vanDongVienService.delete(deletingItem.id);
      setDeleteModalOpen(false);
      setDeletingItem(null);
      loadVdvs();
    } catch (err: any) {
      console.error('Lỗi khi xóa VĐV:', err);
      alert(err?.response?.data?.message || err?.message || 'Không thể xóa VĐV này.');
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
              <div className="d-flex align-items-center gap-2">
                <div className="p-2 bg-primary-subtle text-primary rounded-3 d-inline-flex">
                  <i className="bi bi-person-walking fs-4"></i>
                </div>
                <div>
                  <h4 className="fw-bold mb-0 text-dark">Quản lý Vận Động Viên </h4>
                  <p className="text-muted small mb-0">
                    Danh mục hồ sơ vận động viên, thẻ VĐV &amp; hình ảnh chân dung ({totalCount} VĐV)
                  </p>
                </div>
              </div>
              {canCreate && (
                <Button
                  color="primary"
                  className="px-3 py-2 fw-semibold rounded-3 d-inline-flex align-items-center gap-2 shadow-sm"
                  onClick={handleOpenCreateModal}
                >
                  <i className="bi bi-plus-lg"></i> Thêm Vận Động Viên
                </Button>
              )}
            </div>

            {/* Bộ lọc tìm kiếm */}
            <Form onSubmit={handleSearch} className="mb-4">
              <div className="p-3 bg-light rounded-3 border">
                <Row className="g-2">
                  <Col md={4} lg={4}>
                    <Input
                      type="text"
                      placeholder="Tìm theo họ tên, mã VĐV, CCCD, SĐT..."
                      className="bg-white border rounded-3 shadow-none"
                      value={keyword}
                      onChange={(e) => setKeyword(e.target.value)}
                    />
                  </Col>
                  <Col md={3} lg={3}>
                    <Input
                      type="select"
                      className="bg-white border rounded-3 shadow-none"
                      value={donViFilter}
                      onChange={(e) => {
                        setDonViFilter(e.target.value);
                        setPageIndex(1);
                      }}
                    >
                      <option value="">-- Tất cả đơn vị / đoàn --</option>
                      {donVis.map((dv) => (
                        <option key={dv.id} value={dv.id}>
                          {dv.ten} ({dv.ma})
                        </option>
                      ))}
                    </Input>
                  </Col>
                  <Col md={3} lg={3}>
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
                      <option value="false">Tạm dừng / Khóa</option>
                    </Input>
                  </Col>
                  <Col md={2} lg={2}>
                    <Button color="dark" type="submit" className="w-100 rounded-3">
                      Tìm kiếm
                    </Button>
                  </Col>
                </Row>
              </div>
            </Form>

            {/* Thông báo lỗi */}
            {error && (
              <div className="alert alert-danger d-flex align-items-center mb-4" role="alert">
                <i className="bi bi-exclamation-triangle-fill me-2 fs-5"></i>
                <div>{error}</div>
              </div>
            )}

            {/* Bảng dữ liệu VĐV */}
            {loading ? (
              <div className="text-center py-5">
                <Spinner color="primary" />
                <p className="mt-2 text-muted small">Đang tải danh sách vận động viên...</p>
              </div>
            ) : vdvs.length === 0 ? (
              <div className="text-center py-5 text-muted border rounded-3 bg-light">
                <i className="bi bi-inbox fs-1 d-block mb-2 text-secondary"></i>
                Chưa có vận động viên nào được ghi nhận.
              </div>
            ) : (
              <div className="table-responsive">
                <Table hover className="align-middle mb-0">
                  <thead className="table-light text-uppercase fs-7 text-muted">
                    <tr>
                      <th style={{ width: '50px' }}>#</th>
                      <th>Mã VĐV</th>
                      <th>Vận Động Viên</th>
                      <th>Đơn Vị Trực Thuộc</th>
                      <th>Giới Tính</th>
                      <th>Ngày Sinh</th>
                      <th>Số CCCD</th>
                      <th>Liên Hệ</th>
                      <th>Trạng Thái</th>
                      <th className="text-end" style={{ width: '120px' }}>
                        Thao tác
                      </th>
                    </tr>
                  </thead>
                  <tbody>
                    {vdvs.map((item, index) => (
                      <tr key={item.id}>
                        <td className="text-muted">{(pageIndex - 1) * pageSize + index + 1}</td>
                        <td>
                          <span className="badge bg-light text-primary border font-monospace">
                            {item.ma}
                          </span>
                        </td>
                        <td>
                          <div className="d-flex align-items-center gap-2.5">
                            {item.hinhAnh ? (
                              <div
                                className="rounded-circle overflow-hidden border flex-shrink-0"
                                style={{ width: '40px', height: '40px', backgroundColor: '#f8f9fa' }}
                              >
                                <img
                                  src={
                                    item.hinhAnh.startsWith('http')
                                      ? item.hinhAnh
                                      : `${process.env.NEXT_PUBLIC_API_URL || 'https://localhost:7085'}${item.hinhAnh}`
                                  }
                                  alt={item.hoTen}
                                  className="w-100 h-100 object-fit-cover"
                                />
                              </div>
                            ) : (
                              <div
                                className="rounded-circle border flex-shrink-0 d-flex align-items-center justify-content-center bg-light text-muted"
                                style={{ width: '40px', height: '40px' }}
                              >
                                <i className="bi bi-person-fill" style={{ fontSize: '18px' }}></i>
                              </div>
                            )}
                            <div>
                              <div className="fw-semibold text-dark">{item.hoTen}</div>
                              {item.diaChi && (
                                <small className="text-muted d-block text-truncate" style={{ maxWidth: '180px' }}>
                                  {item.diaChi}
                                </small>
                              )}
                            </div>
                          </div>
                        </td>
                        <td>
                          {item.tenDonVi ? (
                            <Badge color="info" pill>
                              {item.tenDonVi}
                            </Badge>
                          ) : (
                            <span className="text-muted small">---</span>
                          )}
                        </td>
                        <td>
                          <Badge color={item.gioiTinh === 'Nam' ? 'primary' : 'warning'} pill>
                            {item.gioiTinh}
                          </Badge>
                        </td>
                        <td>
                          <span className="small text-muted">
                            {item.ngaySinh ? new Date(item.ngaySinh).toLocaleDateString('vi-VN') : '---'}
                          </span>
                        </td>
                        <td>
                          <span className="font-monospace small text-dark">
                            {item.soCCCD || '---'}
                          </span>
                        </td>
                        <td>
                          <div className="small">
                            {item.soDienThoai && <div>{item.soDienThoai}</div>}
                            {item.email && <div className="text-muted">{item.email}</div>}
                            {!item.soDienThoai && !item.email && <span className="text-muted">---</span>}
                          </div>
                        </td>
                        <td>
                          {item.trangThai ? (
                            <span className="badge rounded-pill bg-success-subtle text-success border border-success-subtle px-3 py-1.5 fw-medium">
                              Đang thi đấu
                            </span>
                          ) : (
                            <span className="badge rounded-pill bg-danger-subtle text-danger border border-danger-subtle px-3 py-1.5 fw-medium">
                              Tạm ngừng
                            </span>
                          )}
                        </td>
                        <td className="text-end">
                          <div className="d-flex justify-content-end gap-1">
                            {canEdit && (
                              <Button
                                size="sm"
                                color="light"
                                className="btn-icon text-primary rounded-circle"
                                title="Chỉnh sửa VĐV"
                                onClick={() => handleOpenEditModal(item)}
                              >
                                <i className="bi bi-pencil"></i>
                              </Button>
                            )}
                            {canDelete && (
                              <Button
                                size="sm"
                                color="light"
                                className="btn-icon text-danger rounded-circle"
                                title="Xóa VĐV"
                                onClick={() => handleOpenDeleteModal(item)}
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

        {/* MODAL THÊM / SỬA VĐV */}
        <Modal isOpen={modalOpen} toggle={() => setModalOpen(!modalOpen)} backdrop="static" centered size="lg">
          <ModalHeader toggle={() => setModalOpen(!modalOpen)}>
            <i className={`bi ${editingId ? 'bi-pencil-square' : 'bi-plus-circle'} me-2 text-primary`}></i>
            {editingId ? 'Cập nhật Thông Tin Vận Động Viên' : 'Thêm Mới Vận Động Viên'}
          </ModalHeader>
          <Form onSubmit={handleSubmitForm}>
            <ModalBody>
              <Row className="g-3">
                <Col md={6}>
                  <FormGroup>
                    <Label className="fw-semibold">
                      Mã Vận Động Viên <span className="text-danger">*</span>
                    </Label>
                    <Input
                      type="text"
                      placeholder="VD: VDV_001, VDV_BD01..."
                      value={formData.ma}
                      onChange={(e) => setFormData({ ...formData, ma: e.target.value.toUpperCase() })}
                      required
                    />
                  </FormGroup>
                </Col>

                <Col md={6}>
                  <FormGroup>
                    <Label className="fw-semibold">
                      Họ và Tên VĐV <span className="text-danger">*</span>
                    </Label>
                    <Input
                      type="text"
                      placeholder="VD: Nguyễn Văn Hoàng"
                      value={formData.hoTen}
                      onChange={(e) => setFormData({ ...formData, hoTen: e.target.value })}
                      required
                    />
                  </FormGroup>
                </Col>

                <Col md={6}>
                  <FormGroup>
                    <Label className="fw-semibold">Đơn Vị / Đoàn Tham Gia</Label>
                    <Input
                      type="select"
                      value={formData.donViId ?? ''}
                      onChange={(e) =>
                        setFormData({
                          ...formData,
                          donViId: e.target.value ? Number(e.target.value) : undefined,
                        })
                      }
                    >
                      <option value="">-- Không trực thuộc đơn vị --</option>
                      {donVis.map((dv) => (
                        <option key={dv.id} value={dv.id}>
                          {dv.ten} ({dv.ma})
                        </option>
                      ))}
                    </Input>
                  </FormGroup>
                </Col>

                <Col md={3}>
                  <FormGroup>
                    <Label className="fw-semibold">Giới Tính</Label>
                    <Input
                      type="select"
                      value={formData.gioiTinh}
                      onChange={(e) => setFormData({ ...formData, gioiTinh: e.target.value })}
                    >
                      <option value="Nam">Nam</option>
                      <option value="Nữ">Nữ</option>
                    </Input>
                  </FormGroup>
                </Col>

                <Col md={3}>
                  <FormGroup>
                    <Label className="fw-semibold">Ngày Sinh</Label>
                    <Input
                      type="date"
                      value={formData.ngaySinh || ''}
                      onChange={(e) => setFormData({ ...formData, ngaySinh: e.target.value })}
                    />
                  </FormGroup>
                </Col>

                <Col md={6}>
                  <FormGroup>
                    <Label className="fw-semibold">Số CCCD / Hộ Chiếu</Label>
                    <Input
                      type="text"
                      placeholder="VD: 038098001234"
                      value={formData.soCCCD || ''}
                      onChange={(e) => setFormData({ ...formData, soCCCD: e.target.value })}
                    />
                  </FormGroup>
                </Col>

                <Col md={6}>
                  <FormGroup>
                    <Label className="fw-semibold">Số Điện Thoại</Label>
                    <Input
                      type="text"
                      placeholder="VD: 0987654321"
                      value={formData.soDienThoai || ''}
                      onChange={(e) => setFormData({ ...formData, soDienThoai: e.target.value })}
                    />
                  </FormGroup>
                </Col>

                <Col md={6}>
                  <FormGroup>
                    <Label className="fw-semibold">Email</Label>
                    <Input
                      type="email"
                      placeholder="vdv@example.com"
                      value={formData.email || ''}
                      onChange={(e) => setFormData({ ...formData, email: e.target.value })}
                    />
                  </FormGroup>
                </Col>

                <Col md={6}>
                  <FormGroup>
                    <Label className="fw-semibold">Địa Chỉ Thường Trú</Label>
                    <Input
                      type="text"
                      placeholder="Địa chỉ cư trú của VĐV..."
                      value={formData.diaChi || ''}
                      onChange={(e) => setFormData({ ...formData, diaChi: e.target.value })}
                    />
                  </FormGroup>
                </Col>

                {/* HÌNH ẢNH CHÂN DUNG VĐV */}
                <Col md={12}>
                  <FormGroup>
                    <Label className="fw-semibold">Hình Ảnh / Ảnh Chân Dung VĐV</Label>
                    <div className="d-flex align-items-center gap-3">
                      {formData.hinhAnh ? (
                        <div className="position-relative">
                          <div
                            className="rounded-circle border overflow-hidden shadow-sm"
                            style={{ width: '80px', height: '80px', backgroundColor: '#f8f9fa' }}
                          >
                            <img
                              src={
                                formData.hinhAnh.startsWith('http')
                                  ? formData.hinhAnh
                                  : `${process.env.NEXT_PUBLIC_API_URL || 'https://localhost:7085'}${formData.hinhAnh}`
                              }
                              alt="Avatar"
                              className="w-100 h-100 object-fit-cover"
                            />
                          </div>
                          <button
                            type="button"
                            className="btn btn-sm btn-danger rounded-circle position-absolute top-0 start-100 translate-middle p-1 lh-1"
                            title="Xóa ảnh"
                            onClick={() => setFormData((prev) => ({ ...prev, hinhAnh: '' }))}
                          >
                            <i className="bi bi-x fs-6"></i>
                          </button>
                        </div>
                      ) : (
                        <div
                          className="rounded-circle border border-dashed d-flex flex-column align-items-center justify-content-center bg-light text-muted"
                          style={{ width: '80px', height: '80px' }}
                        >
                          <i className="bi bi-person fs-3"></i>
                        </div>
                      )}
                      <div>
                        <label className="btn btn-outline-primary btn-sm rounded-3 mb-1 cursor-pointer d-inline-flex align-items-center gap-2">
                          {uploadingAvatar ? (
                            <>
                              <Spinner size="sm" />
                              <span>Đang tải lên...</span>
                            </>
                          ) : (
                            <>
                              <i className="bi bi-upload"></i>
                              <span>{formData.hinhAnh ? 'Đổi ảnh chân dung' : 'Chọn ảnh chân dung VĐV'}</span>
                            </>
                          )}
                          <input
                            type="file"
                            accept="image/*"
                            className="d-none"
                            disabled={uploadingAvatar}
                            onChange={handleAvatarUpload}
                          />
                        </label>
                        <div className="text-muted small" style={{ fontSize: '11px' }}>
                          Lưu vào root <code>/vdv/</code> (JPG, PNG, WEBP)
                        </div>
                      </div>
                    </div>
                  </FormGroup>
                </Col>
              </Row>

              <FormGroup switch className="mt-2">
                <Input
                  type="switch"
                  id="vdvTrangThaiSwitch"
                  checked={formData.trangThai}
                  onChange={(e) => setFormData({ ...formData, trangThai: e.target.checked })}
                />
                <Label check for="vdvTrangThaiSwitch" className="fw-semibold ms-2">
                  Trạng thái: {formData.trangThai ? 'Đang hoạt động / Đủ điều kiện thi đấu' : 'Tạm khóa / Tạm dừng'}
                </Label>
              </FormGroup>
            </ModalBody>
            <ModalFooter className="bg-light">
              <Button color="secondary" outline onClick={() => setModalOpen(false)} disabled={submitting}>
                Hủy bỏ
              </Button>
              <Button color="primary" type="submit" disabled={submitting}>
                {submitting ? <Spinner size="sm" /> : editingId ? 'Cập Nhật VĐV' : 'Thêm Vận Động Viên'}
              </Button>
            </ModalFooter>
          </Form>
        </Modal>

        {/* MODAL XÁC NHẬN XÓA */}
        <Modal isOpen={deleteModalOpen} toggle={() => setDeleteModalOpen(!deleteModalOpen)} centered size="sm">
          <ModalHeader toggle={() => setDeleteModalOpen(!deleteModalOpen)}>
            <i className="bi bi-exclamation-triangle-fill text-danger me-2"></i>
            Xác nhận xóa
          </ModalHeader>
          <ModalBody>
            Bạn có chắc chắn muốn xóa Vận Động Viên <strong>{deletingItem?.hoTen}</strong> (Mã:{' '}
            <code>{deletingItem?.ma}</code>) không?
          </ModalBody>
          <ModalFooter>
            <Button color="secondary" outline onClick={() => setDeleteModalOpen(false)} disabled={submitting}>
              Hủy
            </Button>
            <Button color="danger" onClick={handleConfirmDelete} disabled={submitting}>
              {submitting ? <Spinner size="sm" /> : 'Xóa VĐV'}
            </Button>
          </ModalFooter>
        </Modal>
      </Col>
    </Row>
  );
}
