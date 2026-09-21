'use client';

import React, { useEffect, useState, useCallback } from 'react';
import {Row,Col,Table,Card,CardBody,Button,Input,Spinner,Modal,ModalHeader,ModalBody,ModalFooter,Form,FormGroup,Label,Badge,} from 'reactstrap';
import { donViService, khoiService } from '@/services';
import { DonVi, CreateUpdateDonVi, Khoi } from '@/types';
import { PaginationComponent } from '@/components/common/PaginationComponent';
import { useAuth } from '@/context/AuthContext';
import { Permissions } from '@/constants/permissions';

export default function AdminDonViPage() {
  const { hasPermission } = useAuth();
  const canCreate = hasPermission(Permissions.DonVi.Create);
  const canEdit = hasPermission(Permissions.DonVi.Edit);
  const canDelete = hasPermission(Permissions.DonVi.Delete);

  const [donVis, setDonVis] = useState<DonVi[]>([]);
  const [khois, setKhois] = useState<Khoi[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(1);
  const [pageIndex, setPageIndex] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [keyword, setKeyword] = useState('');
  const [khoiFilter, setKhoiFilter] = useState<string>('');
  const [trangThaiFilter, setTrangThaiFilter] = useState<string>('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Modal Thêm / Sửa
  const [modalOpen, setModalOpen] = useState(false);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [formData, setFormData] = useState<CreateUpdateDonVi>({
    ma: '',
    ten: '',
    khoiId: undefined,
    donViChaId: undefined,
    loaiDonVi: 'Doan',
    diaChi: '',
    nguoiDaiDien: '',
    soDienThoai: '',
    email: '',
    moTa: '',
    hinhAnh: '',
    trangThai: true,
  });
  const [submitting, setSubmitting] = useState(false);
  const [uploadingImage, setUploadingImage] = useState(false);

  const handleImageUpload = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    setUploadingImage(true);
    try {
      const res = await donViService.uploadImage(file);
      if (res?.url) {
        setFormData((prev) => ({ ...prev, hinhAnh: res.url }));
      }
    } catch (err: any) {
      alert('Không thể tải lên hình ảnh đơn vị: ' + (err?.response?.data?.message || err?.message || 'Lỗi server'));
    } finally {
      setUploadingImage(false);
    }
  };

  // Modal Xóa
  const [deleteModalOpen, setDeleteModalOpen] = useState(false);
  const [deletingItem, setDeletingItem] = useState<DonVi | null>(null);

  // Load danh sách khối để chọn
  useEffect(() => {
    const fetchKhois = async () => {
      try {
        const res = await khoiService.getAll();
        setKhois(res || []);
      } catch (err) {
        console.error('Lỗi khi tải danh sách khối:', err);
      }
    };
    fetchKhois();
  }, []);

  // Fetch dữ liệu Đơn vị từ backend
  const loadDonVis = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await donViService.getPaged({
        pageIndex,
        pageSize,
        keyword: keyword.trim() || undefined,
        khoiId: khoiFilter ? parseInt(khoiFilter, 10) : undefined,
        trangThai: trangThaiFilter === '' ? undefined : trangThaiFilter === 'true',
      });
      setDonVis(data.items || []);
      setTotalCount(data.totalCount || 0);
      setTotalPages(data.totalPages || 1);
    } catch (err: any) {
      console.error('Lỗi khi tải danh sách đơn vị:', err);
      setError(err?.message || 'Không thể kết nối đến máy chủ Backend.');
    } finally {
      setLoading(false);
    }
  }, [pageIndex, pageSize, keyword, khoiFilter, trangThaiFilter]);

  useEffect(() => {
    loadDonVis();
  }, [loadDonVis]);

  const handleSearchSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setPageIndex(1);
    loadDonVis();
  };

  // Xử lý mở modal Tạo mới
  const handleOpenCreateModal = () => {
    setEditingId(null);
    setFormData({
      ma: '',
      ten: '',
      khoiId: khois.length > 0 ? khois[0].id : undefined,
      donViChaId: undefined,
      loaiDonVi: 'Doan',
      diaChi: '',
      nguoiDaiDien: '',
      soDienThoai: '',
      email: '',
      moTa: '',
      hinhAnh: '',
      trangThai: true,
    });
    setModalOpen(true);
  };

  // Xử lý mở modal Chỉnh sửa
  const handleOpenEditModal = (item: DonVi) => {
    setEditingId(item.id);
    setFormData({
      ma: item.ma,
      ten: item.ten,
      khoiId: item.khoiId,
      donViChaId: item.donViChaId,
      loaiDonVi: item.loaiDonVi || 'Doan',
      diaChi: item.diaChi || '',
      nguoiDaiDien: item.nguoiDaiDien || '',
      soDienThoai: item.soDienThoai || '',
      email: item.email || '',
      moTa: item.moTa || '',
      hinhAnh: item.hinhAnh || '',
      trangThai: item.trangThai,
    });
    setModalOpen(true);
  };

  // Lưu Form (Tạo mới hoặc Sửa)
  const handleSubmitForm = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!formData.ma.trim() || !formData.ten.trim()) {
      alert('Vui lòng nhập đầy đủ Mã đơn vị và Tên đơn vị!');
      return;
    }

    setSubmitting(true);
    try {
      if (editingId) {
        await donViService.update(editingId, formData);
      } else {
        await donViService.create(formData);
      }
      setModalOpen(false);
      loadDonVis();
    } catch (err: any) {
      console.error('Lỗi khi lưu đơn vị:', err);
      alert(err?.response?.data?.message || err?.message || 'Có lỗi xảy ra khi lưu đơn vị.');
    } finally {
      setSubmitting(false);
    }
  };

  // Mở modal xác nhận xóa
  const handleOpenDeleteModal = (item: DonVi) => {
    setDeletingItem(item);
    setDeleteModalOpen(true);
  };

  // Xác nhận xóa
  const handleConfirmDelete = async () => {
    if (!deletingItem) return;
    setSubmitting(true);
    try {
      await donViService.delete(deletingItem.id);
      setDeleteModalOpen(false);
      setDeletingItem(null);
      loadDonVis();
    } catch (err: any) {
      console.error('Lỗi khi xóa đơn vị:', err);
      alert(err?.response?.data?.message || err?.message || 'Không thể xóa đơn vị này.');
    } finally {
      setSubmitting(false);
    }
  };

  const renderTrangThai = (trangThai: boolean) => {
    if (trangThai) {
      return (
        <span className="badge rounded-pill bg-success-subtle text-success border border-success-subtle px-3 py-2 fw-medium d-inline-flex align-items-center gap-1">
          <span className="p-1 bg-success rounded-circle d-inline-block" />
          Hoạt động
        </span>
      );
    }
    return (
      <span className="badge rounded-pill bg-secondary-subtle text-secondary border border-secondary-subtle px-3 py-2 fw-medium d-inline-flex align-items-center gap-1">
        <span className="p-1 bg-secondary rounded-circle d-inline-block" />
        Tạm dừng
      </span>
    );
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
                    <i className="bi bi-building fs-4"></i>
                  </div>
                  <div>
                    <h4 className="fw-bold mb-0 text-dark">Quản lý Đơn vị / Đoàn tham gia </h4>
                    <p className="text-muted small mb-0">
                      Danh sách cơ quan, câu lạc bộ, trường học, đoàn thể thao trực thuộc các khối ({totalCount} đơn vị)
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
                    <i className="bi bi-plus-lg"></i> Thêm đơn vị mới
                  </Button>
                </div>
              )}
            </div>

            {/* Thanh tìm kiếm và bộ lọc */}
            <Form onSubmit={handleSearchSubmit} className="mb-4">
              <div className="p-3 bg-light rounded-3">
                <Row className="g-2 align-items-center">
                  <Col md={5} lg={4}>
                    <div className="input-group bg-white rounded-3 overflow-hidden border">
                      <span className="input-group-text bg-white border-0 text-muted ps-3">
                        <i className="bi bi-search"></i>
                      </span>
                      <Input
                        type="text"
                        className="border-0 shadow-none ps-2"
                        placeholder="Tìm kiếm mã, tên, người đại diện..."
                        value={keyword}
                        onChange={(e) => setKeyword(e.target.value)}
                      />
                    </div>
                  </Col>
                  <Col md={3} lg={3}>
                    <Input
                      type="select"
                      className="bg-white border rounded-3 shadow-none"
                      value={khoiFilter}
                      onChange={(e) => {
                        setKhoiFilter(e.target.value);
                        setPageIndex(1);
                      }}
                    >
                      <option value="">-- Tất cả khối --</option>
                      {khois.map((k) => (
                        <option key={k.id} value={k.id}>
                          {k.ten}
                        </option>
                      ))}
                    </Input>
                  </Col>
                  <Col md={2} lg={3}>
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

            {/* Thông báo lỗi nếu có */}
            {error && (
              <div className="alert alert-danger d-flex align-items-center mb-4" role="alert">
                <i className="bi bi-exclamation-triangle-fill me-2 fs-5"></i>
                <div>{error}</div>
              </div>
            )}

            {/* Bảng dữ liệu Đơn vị */}
            {loading ? (
              <div className="text-center py-5">
                <Spinner color="primary" />
                <p className="mt-2 text-muted small">Đang tải danh sách đơn vị...</p>
              </div>
            ) : donVis.length === 0 ? (
              <div className="text-center py-5 text-muted border rounded-3 bg-light">
                <i className="bi bi-inbox fs-1 d-block mb-2 text-secondary"></i>
                Chưa có đơn vị nào được ghi nhận.
              </div>
            ) : (
              <div className="table-responsive">
                <Table hover className="align-middle mb-0">
                  <thead className="table-light text-uppercase fs-7 text-muted">
                    <tr>
                      <th style={{ width: '50px' }}>#</th>
                      <th>Mã</th>
                      <th>Tên Đơn Vị / Đoàn</th>
                      <th>Khối Trực Thuộc</th>
                      <th>Đại Diện / Liên Hệ</th>
                      <th className="text-center">VĐV / Đội</th>
                      <th>Trạng Thái</th>
                      <th className="text-end" style={{ width: '130px' }}>
                        Thao tác
                      </th>
                    </tr>
                  </thead>
                  <tbody>
                    {donVis.map((item, index) => (
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
                                className="rounded-2 overflow-hidden border flex-shrink-0"
                                style={{ width: '40px', height: '40px', backgroundColor: '#f8f9fa' }}
                              >
                                <img
                                  src={
                                    item.hinhAnh.startsWith('http')
                                      ? item.hinhAnh
                                      : `${process.env.NEXT_PUBLIC_API_URL || 'https://localhost:7085'}${item.hinhAnh}`
                                  }
                                  alt={item.ten}
                                  className="w-100 h-100 object-fit-cover"
                                />
                              </div>
                            ) : (
                              <div
                                className="rounded-2 border flex-shrink-0 d-flex align-items-center justify-content-center bg-light text-muted"
                                style={{ width: '40px', height: '40px' }}
                              >
                                <i className="bi bi-building" style={{ fontSize: '16px' }}></i>
                              </div>
                            )}
                            <div>
                              <div className="fw-semibold text-dark">{item.ten}</div>
                              {item.loaiDonVi && (
                                <small className="text-muted d-block">Loại: {item.loaiDonVi}</small>
                              )}
                            </div>
                          </div>
                        </td>
                        <td>
                          {item.tenKhoi ? (
                            <Badge color="info" pill>
                              {item.tenKhoi}
                            </Badge>
                          ) : (
                            <span className="text-muted small">---</span>
                          )}
                        </td>
                        <td>
                          <div className="small">
                            {item.nguoiDaiDien && (
                              <div className="text-dark">
                                <i className="bi bi-person me-1 text-secondary"></i>
                                {item.nguoiDaiDien}
                              </div>
                            )}
                            {item.soDienThoai && (
                              <div className="text-muted">
                                <i className="bi bi-telephone me-1"></i>
                                {item.soDienThoai}
                              </div>
                            )}
                            {!item.nguoiDaiDien && !item.soDienThoai && (
                              <span className="text-muted">---</span>
                            )}
                          </div>
                        </td>
                        <td className="text-center">
                          <div className="d-flex flex-column gap-1 align-items-center">
                            <span className="badge bg-primary-subtle text-primary px-2 py-1">
                              {item.soVanDongVien ?? 0} VĐV
                            </span>
                            <span className="badge bg-secondary-subtle text-secondary px-2 py-1">
                              {item.soDoi ?? 0} Đội
                            </span>
                          </div>
                        </td>
                        <td>{renderTrangThai(item.trangThai)}</td>
                        <td className="text-end">
                          <div className="d-flex justify-content-end gap-1">
                            {canEdit && (
                              <Button
                                size="sm"
                                color="light"
                                className="btn-icon text-primary"
                                title="Chỉnh sửa đơn vị"
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
                                title="Xóa đơn vị"
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

        {/* MODAL THÊM / SỬA ĐƠN VỊ */}
        <Modal isOpen={modalOpen} toggle={() => setModalOpen(!modalOpen)} backdrop="static" centered size="lg">
          <ModalHeader toggle={() => setModalOpen(!modalOpen)}>
            <i className={`bi ${editingId ? 'bi-pencil-square' : 'bi-plus-circle'} me-2 text-primary`}></i>
            {editingId ? 'Cập nhật Đơn Vị / Đoàn' : 'Thêm Đơn Vị / Đoàn Mới'}
          </ModalHeader>
          <Form onSubmit={handleSubmitForm}>
            <ModalBody>
              <Row className="g-3">
                <Col md={6}>
                  <FormGroup>
                    <Label className="fw-semibold">
                      Mã Đơn Vị <span className="text-danger">*</span>
                    </Label>
                    <Input
                      type="text"
                      placeholder="VD: DV_DHQG, DV_SXD..."
                      value={formData.ma}
                      onChange={(e) => setFormData({ ...formData, ma: e.target.value.toUpperCase() })}
                      required
                    />
                  </FormGroup>
                </Col>

                <Col md={6}>
                  <FormGroup>
                    <Label className="fw-semibold">
                      Tên Đơn Vị <span className="text-danger">*</span>
                    </Label>
                    <Input
                      type="text"
                      placeholder="VD: Đoàn Đại học Quốc Gia..."
                      value={formData.ten}
                      onChange={(e) => setFormData({ ...formData, ten: e.target.value })}
                      required
                    />
                  </FormGroup>
                </Col>

                <Col md={6}>
                  <FormGroup>
                    <Label className="fw-semibold">Khối Trực Thuộc</Label>
                    <Input
                      type="select"
                      value={formData.khoiId ?? ''}
                      onChange={(e) =>
                        setFormData({
                          ...formData,
                          khoiId: e.target.value ? parseInt(e.target.value, 10) : undefined,
                        })
                      }
                    >
                      <option value="">-- Chọn Khối --</option>
                      {khois.map((k) => (
                        <option key={k.id} value={k.id}>
                          {k.ten}
                        </option>
                      ))}
                    </Input>
                  </FormGroup>
                </Col>

                <Col md={6}>
                  <FormGroup>
                    <Label className="fw-semibold">Loại Đơn Vị</Label>
                    <Input
                      type="select"
                      value={formData.loaiDonVi ?? 'Doan'}
                      onChange={(e) => setFormData({ ...formData, loaiDonVi: e.target.value })}
                    >
                      <option value="Doan">Đoàn thể thao</option>
                      <option value="CLB">Câu lạc bộ</option>
                      <option value="TruongHoc">Trường học</option>
                      <option value="DoanhNghiep">Doanh nghiệp</option>
                      <option value="Khac">Khác</option>
                    </Input>
                  </FormGroup>
                </Col>

                <Col md={6}>
                  <FormGroup>
                    <Label className="fw-semibold">Người Đại Diện / Trưởng Đoàn</Label>
                    <Input
                      type="text"
                      placeholder="Họ tên người đại diện"
                      value={formData.nguoiDaiDien || ''}
                      onChange={(e) => setFormData({ ...formData, nguoiDaiDien: e.target.value })}
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
                    <Label className="fw-semibold">Email Liên Hệ</Label>
                    <Input
                      type="email"
                      placeholder="email@example.com"
                      value={formData.email || ''}
                      onChange={(e) => setFormData({ ...formData, email: e.target.value })}
                    />
                  </FormGroup>
                </Col>

                <Col md={6}>
                  <FormGroup>
                    <Label className="fw-semibold">Địa Chỉ Trụ Sở</Label>
                    <Input
                      type="text"
                      placeholder="Số nhà, đường, quận/huyện, tỉnh/thành..."
                      value={formData.diaChi || ''}
                      onChange={(e) => setFormData({ ...formData, diaChi: e.target.value })}
                    />
                  </FormGroup>
                </Col>

                <Col md={12}>
                  <FormGroup>
                    <Label className="fw-semibold">Hình Ảnh / Logo Đơn Vị</Label>
                    <div className="d-flex align-items-center gap-3">
                      {formData.hinhAnh ? (
                        <div className="position-relative">
                          <div
                            className="rounded-3 border overflow-hidden shadow-sm"
                            style={{ width: '80px', height: '80px', backgroundColor: '#f8f9fa' }}
                          >
                            <img
                              src={
                                formData.hinhAnh.startsWith('http')
                                  ? formData.hinhAnh
                                  : `${process.env.NEXT_PUBLIC_API_URL || 'https://localhost:7085'}${formData.hinhAnh}`
                              }
                              alt="Logo"
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
                          className="rounded-3 border border-dashed d-flex flex-column align-items-center justify-content-center bg-light text-muted"
                          style={{ width: '80px', height: '80px' }}
                        >
                          <i className="bi bi-image fs-4"></i>
                        </div>
                      )}
                      <div>
                        <label className="btn btn-outline-primary btn-sm rounded-3 mb-1 cursor-pointer d-inline-flex align-items-center gap-2">
                          {uploadingImage ? (
                            <>
                              <Spinner size="sm" />
                              <span>Đang tải lên...</span>
                            </>
                          ) : (
                            <>
                              <i className="bi bi-upload"></i>
                              <span>{formData.hinhAnh ? 'Đổi ảnh khác' : 'Chọn tệp ảnh logo'}</span>
                            </>
                          )}
                          <input
                            type="file"
                            accept="image/*"
                            className="d-none"
                            disabled={uploadingImage}
                            onChange={handleImageUpload}
                          />
                        </label>
                        <div className="text-muted small" style={{ fontSize: '11px' }}>
                          Lưu vào root <code>/don-vi/</code> (JPG, PNG, WEBP)
                        </div>
                      </div>
                    </div>
                  </FormGroup>
                </Col>

                <Col md={12}>
                  <FormGroup>
                    <Label className="fw-semibold">Mô Tả / Giới Thiệu</Label>
                    <Input
                      type="textarea"
                      rows={2}
                      placeholder="Ghi chú thêm về đơn vị..."
                      value={formData.moTa || ''}
                      onChange={(e) => setFormData({ ...formData, moTa: e.target.value })}
                    />
                  </FormGroup>
                </Col>
              </Row>

              <FormGroup switch className="mt-2">
                <Input
                  type="switch"
                  id="donViTrangThaiSwitch"
                  checked={formData.trangThai}
                  onChange={(e) => setFormData({ ...formData, trangThai: e.target.checked })}
                />
                <Label check for="donViTrangThaiSwitch" className="fw-semibold ms-2">
                  Kích hoạt trạng thái hoạt động
                </Label>
              </FormGroup>
            </ModalBody>
            <ModalFooter>
              <Button color="secondary" outline onClick={() => setModalOpen(false)} disabled={submitting}>
                Hủy
              </Button>
              <Button color="primary" type="submit" disabled={submitting} className="d-inline-flex align-items-center gap-1">
                {submitting ? <Spinner size="sm" /> : <i className="bi bi-check-circle"></i>}
                {editingId ? 'Cập Nhật' : 'Lưu Lại'}
              </Button>
            </ModalFooter>
          </Form>
        </Modal>

        {/* MODAL XÁC NHẬN XÓA */}
        <Modal isOpen={deleteModalOpen} toggle={() => setDeleteModalOpen(!deleteModalOpen)} centered size="sm">
          <ModalHeader toggle={() => setDeleteModalOpen(!deleteModalOpen)} className="text-danger">
            <i className="bi bi-exclamation-triangle-fill me-2"></i> Xác nhận xóa
          </ModalHeader>
          <ModalBody>
            Bạn có chắc chắn muốn xóa đơn vị <strong>{deletingItem?.ten}</strong> ({deletingItem?.ma}) không?
            <p className="text-muted small mt-2 mb-0">Hành động này sẽ chuyển trạng thái đơn vị sang đã xóa.</p>
          </ModalBody>
          <ModalFooter>
            <Button color="secondary" outline onClick={() => setDeleteModalOpen(false)} disabled={submitting}>
              Hủy
            </Button>
            <Button color="danger" onClick={handleConfirmDelete} disabled={submitting}>
              {submitting ? <Spinner size="sm" /> : 'Đồng Ý Xóa'}
            </Button>
          </ModalFooter>
        </Modal>
      </Col>
    </Row>
  );
}
