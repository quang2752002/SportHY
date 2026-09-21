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
import { monTheThaoService, danhMucMonTheThaoService, cauHinhLichThiDauService } from '@/services';
import { MonTheThao, CreateUpdateMonTheThao, DanhMucMonTheThao, CreateUpdateCauHinhLichThiDauRequest } from '@/types';
import { PaginationComponent } from '@/components/common/PaginationComponent';
import { useAuth } from '@/context/AuthContext';
import { Permissions } from '@/constants/permissions';

const HINH_THUC_OPTIONS = [
  { value: 'LoaiTrucTiep', label: 'Loại trực tiếp (Knockout)', badge: 'danger' },
  { value: 'VongBang', label: 'Vòng tròn / Vòng bảng (Round Robin)', badge: 'primary' },
  { value: 'KetHopVongBangVaLoaiTrucTiep', label: 'Vòng bảng + Knockout', badge: 'success' },
  { value: 'NhanhThangNhanhThua', label: 'Nhánh thắng - Nhánh thua', badge: 'warning' },
  { value: 'HeThuySi', label: 'Hệ Thụy Sĩ (Swiss System)', badge: 'info' },
  { value: 'TinhDiemXepHang', label: 'Tính điểm xếp hạng / Tính giờ', badge: 'secondary' },
];

const GIOI_TINH_OPTIONS = [
  { value: 'Nam', label: 'Nam (♂)', badge: 'primary' },
  { value: 'Nu', label: 'Nữ (♀)', badge: 'danger' },
  { value: 'HonHop', label: 'Hỗn hợp / Mọi giới tính (⚥)', badge: 'info' },
];

const renderGioiTinh = (gioiTinh?: string) => {
  switch (gioiTinh) {
    case 'Nam':
      return (
        <Badge color="primary" pill className="px-2 py-1">
          <i className="bi bi-gender-male me-1"></i>Nam
        </Badge>
      );
    case 'Nu':
      return (
        <Badge color="danger" pill className="px-2 py-1" style={{ backgroundColor: '#ec4899' }}>
          <i className="bi bi-gender-female me-1"></i>Nữ
        </Badge>
      );
    case 'HonHop':
    default:
      return (
        <Badge color="info" pill className="px-2 py-1">
          <i className="bi bi-gender-ambiguous me-1"></i>Hỗn hợp
        </Badge>
      );
  }
};

export default function AdminMonTheThaoPage() {
  const { hasPermission } = useAuth();
  const canCreate = hasPermission(Permissions.MonTheThao.Create);
  const canEdit = hasPermission(Permissions.MonTheThao.Edit);
  const canDelete = hasPermission(Permissions.MonTheThao.Delete);

  const [monTheThaos, setMonTheThaos] = useState<MonTheThao[]>([]);
  const [danhMucs, setDanhMucs] = useState<DanhMucMonTheThao[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(1);
  const [pageIndex, setPageIndex] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [keyword, setKeyword] = useState('');
  const [danhMucFilter, setDanhMucFilter] = useState<string>('');
  const [gioiTinhFilter, setGioiTinhFilter] = useState<string>('');
  const [trangThaiFilter, setTrangThaiFilter] = useState<string>('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Modal Thêm / Sửa
  const [modalOpen, setModalOpen] = useState(false);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [formData, setFormData] = useState<CreateUpdateMonTheThao>({
    danhMucId: 0,
    ma: '',
    ten: '',
    moTa: '',
    laMonDongDoi: false,
    gioiTinh: 'HonHop',
    hinhThucThiDau: 'LoaiTrucTiep',
    soLuongVanDongVienToiThieu: 1,
    soLuongVanDongVienToiDa: 1,
    soDoiToiDa: undefined,
    trangThai: true,
  });
  const [submitting, setSubmitting] = useState(false);

  // Modal Xóa
  const [deleteModalOpen, setDeleteModalOpen] = useState(false);
  const [deletingItem, setDeletingItem] = useState<MonTheThao | null>(null);

  // Modal Cấu hình Xếp lịch
  const [configModalOpen, setConfigModalOpen] = useState(false);
  const [configLoading, setConfigLoading] = useState(false);
  const [configSubmitting, setConfigSubmitting] = useState(false);
  const [selectedMonForConfig, setSelectedMonForConfig] = useState<MonTheThao | null>(null);
  const [configData, setConfigData] = useState<CreateUpdateCauHinhLichThiDauRequest>({
    monTheThaoId: 0,
    moiVongMotNgay: true,
    khoangCachGiuaCacVongGio: 12,
    uuTienChungKetNgayCuoi: true,
    soTranToiDaMoiDoiMoiNgay: 1,
    nghiToiThieuGiua2TranPhut: 120,
    chiaCaThiDau: true,
    caSangBatDau: '08:00',
    caSangKetThuc: '11:30',
    caChieuBatDau: '14:00',
    caChieuKetThuc: '17:30',
    caToBatDau: '',
    caToKetThuc: '',
    thoiGianDemDonSanPhut: 15,
    soTranToiDaMoiTrongTaiMoiNgay: 4,
    nghiToiThieuTrongTaiPhut: 15,
    thoiGianDemDiChuyenPhut: 30,
    thoiLuongTranMacDinhPhut: 60,
    soHiepDauMacDinh: 0,
    thoiGianMoiHiepPhut: 0,
    ghiChu: '',
  });

  const handleOpenConfigModal = async (item: MonTheThao) => {
    setSelectedMonForConfig(item);
    setConfigLoading(true);
    setConfigModalOpen(true);
    try {
      const existing = await cauHinhLichThiDauService.getByMonTheThao(item.id);
      if (existing) {
        setConfigData({
          monTheThaoId: item.id,
          moiVongMotNgay: existing.moiVongMotNgay,
          khoangCachGiuaCacVongGio: existing.khoangCachGiuaCacVongGio,
          uuTienChungKetNgayCuoi: existing.uuTienChungKetNgayCuoi,
          soTranToiDaMoiDoiMoiNgay: existing.soTranToiDaMoiDoiMoiNgay,
          nghiToiThieuGiua2TranPhut: existing.nghiToiThieuGiua2TranPhut,
          chiaCaThiDau: existing.chiaCaThiDau,
          caSangBatDau: existing.caSangBatDau || '08:00',
          caSangKetThuc: existing.caSangKetThuc || '11:30',
          caChieuBatDau: existing.caChieuBatDau || '14:00',
          caChieuKetThuc: existing.caChieuKetThuc || '17:30',
          caToBatDau: existing.caToBatDau || '',
          caToKetThuc: existing.caToKetThuc || '',
          thoiGianDemDonSanPhut: existing.thoiGianDemDonSanPhut,
          soTranToiDaMoiTrongTaiMoiNgay: existing.soTranToiDaMoiTrongTaiMoiNgay,
          nghiToiThieuTrongTaiPhut: existing.nghiToiThieuTrongTaiPhut,
          thoiGianDemDiChuyenPhut: existing.thoiGianDemDiChuyenPhut,
          thoiLuongTranMacDinhPhut: existing.thoiLuongTranMacDinhPhut,
          soHiepDauMacDinh: existing.soHiepDauMacDinh,
          thoiGianMoiHiepPhut: existing.thoiGianMoiHiepPhut,
          ghiChu: existing.ghiChu || '',
        });
      } else {
        setConfigData({
          monTheThaoId: item.id,
          moiVongMotNgay: true,
          khoangCachGiuaCacVongGio: 12,
          uuTienChungKetNgayCuoi: true,
          soTranToiDaMoiDoiMoiNgay: 1,
          nghiToiThieuGiua2TranPhut: 120,
          chiaCaThiDau: true,
          caSangBatDau: '08:00',
          caSangKetThuc: '11:30',
          caChieuBatDau: '14:00',
          caChieuKetThuc: '17:30',
          caToBatDau: '',
          caToKetThuc: '',
          thoiGianDemDonSanPhut: 15,
          soTranToiDaMoiTrongTaiMoiNgay: 4,
          nghiToiThieuTrongTaiPhut: 15,
          thoiGianDemDiChuyenPhut: 30,
          thoiLuongTranMacDinhPhut: 60,
          soHiepDauMacDinh: 0,
          thoiGianMoiHiepPhut: 0,
          ghiChu: '',
        });
      }
    } catch (err) {
      console.error('Lỗi khi tải cấu hình xếp lịch:', err);
    } finally {
      setConfigLoading(false);
    }
  };

  const handleSaveConfig = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedMonForConfig) return;
    setConfigSubmitting(true);
    try {
      await cauHinhLichThiDauService.upsert(configData);
      alert(`Đã lưu cấu hình xếp lịch cho môn "${selectedMonForConfig.ten}" thành công!`);
      setConfigModalOpen(false);
    } catch (err: any) {
      console.error('Lỗi khi lưu cấu hình xếp lịch:', err);
      alert(err?.response?.data?.message || err?.message || 'Có lỗi xảy ra khi lưu cấu hình.');
    } finally {
      setConfigSubmitting(false);
    }
  };

  const handleResetConfig = async () => {
    if (!selectedMonForConfig) return;
    if (!confirm(`Bạn có chắc chắn muốn đặt lại cấu hình xếp lịch cho môn "${selectedMonForConfig.ten}" về mặc định hệ thống?`)) return;
    setConfigSubmitting(true);
    try {
      await cauHinhLichThiDauService.delete(selectedMonForConfig.id);
      alert(`Đã đặt lại cấu hình môn "${selectedMonForConfig.ten}" về mặc định.`);
      setConfigModalOpen(false);
    } catch (err: any) {
      console.error('Lỗi khi reset cấu hình:', err);
      alert(err?.response?.data?.message || 'Không thể đặt lại cấu hình.');
    } finally {
      setConfigSubmitting(false);
    }
  };

  // Load danh mục môn để lọc và chọn trong dropdown
  useEffect(() => {
    const fetchDanhMucs = async () => {
      try {
        const res = await danhMucMonTheThaoService.getAll();
        setDanhMucs(res || []);
      } catch (err) {
        console.error('Lỗi khi tải danh mục môn:', err);
      }
    };
    fetchDanhMucs();
  }, []);

  // Fetch dữ liệu Môn thể thao từ backend
  const loadMonTheThaos = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await monTheThaoService.getPaged({
        pageIndex,
        pageSize,
        keyword: keyword.trim() || undefined,
        danhMucId: danhMucFilter ? parseInt(danhMucFilter, 10) : undefined,
        trangThai: trangThaiFilter === '' ? undefined : trangThaiFilter === 'true',
        gioiTinh: gioiTinhFilter || undefined,
      });
      setMonTheThaos(data.items || []);
      setTotalCount(data.totalCount || 0);
      setTotalPages(data.totalPages || 1);
    } catch (err: any) {
      console.error('Lỗi khi tải danh sách môn thể thao:', err);
      setError(err?.message || 'Không thể kết nối đến máy chủ Backend.');
    } finally {
      setLoading(false);
    }
  }, [pageIndex, pageSize, keyword, danhMucFilter, trangThaiFilter, gioiTinhFilter]);

  useEffect(() => {
    loadMonTheThaos();
  }, [loadMonTheThaos]);

  const handleSearchSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setPageIndex(1);
    loadMonTheThaos();
  };

  // Xử lý mở modal Tạo mới
  const handleOpenCreateModal = () => {
    setEditingId(null);
    setFormData({
      danhMucId: danhMucs.length > 0 ? danhMucs[0].id : 0,
      ma: '',
      ten: '',
      moTa: '',
      laMonDongDoi: false,
      gioiTinh: 'HonHop',
      hinhThucThiDau: 'LoaiTrucTiep',
      soLuongVanDongVienToiThieu: 1,
      soLuongVanDongVienToiDa: 1,
      soDoiToiDa: undefined,
      trangThai: true,
    });
    setModalOpen(true);
  };

  // Xử lý mở modal Chỉnh sửa
  const handleOpenEditModal = (item: MonTheThao) => {
    setEditingId(item.id);
    setFormData({
      danhMucId: item.danhMucId,
      ma: item.ma,
      ten: item.ten,
      moTa: item.moTa || '',
      laMonDongDoi: item.laMonDongDoi,
      gioiTinh: item.gioiTinh || 'HonHop',
      hinhThucThiDau: item.hinhThucThiDau || 'LoaiTrucTiep',
      soLuongVanDongVienToiThieu: item.soLuongVanDongVienToiThieu ?? (item.laMonDongDoi ? 5 : 1),
      soLuongVanDongVienToiDa: item.soLuongVanDongVienToiDa ?? (item.laMonDongDoi ? 10 : 1),
      soDoiToiDa: item.soDoiToiDa,
      trangThai: item.trangThai,
    });
    setModalOpen(true);
  };

  // Lưu Form (Tạo mới hoặc Sửa)
  const handleSubmitForm = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!formData.ma.trim() || !formData.ten.trim()) {
      alert('Vui lòng nhập đầy đủ Mã môn và Tên môn thể thao!');
      return;
    }
    if (!formData.danhMucId) {
      alert('Vui lòng chọn Danh mục môn thể thao!');
      return;
    }

    setSubmitting(true);
    try {
      if (editingId) {
        await monTheThaoService.update(editingId, formData);
      } else {
        await monTheThaoService.create(formData);
      }
      setModalOpen(false);
      loadMonTheThaos();
    } catch (err: any) {
      console.error('Lỗi khi lưu môn thể thao:', err);
      alert(err?.response?.data?.message || err?.message || 'Có lỗi xảy ra khi lưu môn thể thao.');
    } finally {
      setSubmitting(false);
    }
  };

  // Mở modal xác nhận xóa
  const handleOpenDeleteModal = (item: MonTheThao) => {
    setDeletingItem(item);
    setDeleteModalOpen(true);
  };

  // Xác nhận xóa
  const handleConfirmDelete = async () => {
    if (!deletingItem) return;
    setSubmitting(true);
    try {
      await monTheThaoService.delete(deletingItem.id);
      setDeleteModalOpen(false);
      setDeletingItem(null);
      loadMonTheThaos();
    } catch (err: any) {
      console.error('Lỗi khi xóa môn thể thao:', err);
      alert(err?.response?.data?.message || err?.message || 'Không thể xóa môn thể thao này.');
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
                    <i className="bi bi-dribbble fs-4"></i>
                  </div>
                  <div>
                    <h4 className="fw-bold mb-0 text-dark">Quản lý Môn Thể Thao </h4>
                    <p className="text-muted small mb-0">
                      Danh sách các môn thi đấu, hình thức cá nhân / đồng đội, danh mục phân loại ({totalCount} môn)
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
                    <i className="bi bi-plus-lg"></i> Thêm môn thi đấu mới
                  </Button>
                </div>
              )}
            </div>

            {/* Thanh tìm kiếm và bộ lọc */}
            <Form onSubmit={handleSearchSubmit} className="mb-4">
              <div className="p-3 bg-light rounded-3">
                <Row className="g-2 align-items-center">
                  <Col md={4} lg={3}>
                    <div className="input-group bg-white rounded-3 overflow-hidden border">
                      <span className="input-group-text bg-white border-0 text-muted ps-3">
                        <i className="bi bi-search"></i>
                      </span>
                      <Input
                        type="text"
                        className="border-0 shadow-none ps-2"
                        placeholder="Tìm kiếm mã, tên môn..."
                        value={keyword}
                        onChange={(e) => setKeyword(e.target.value)}
                      />
                    </div>
                  </Col>
                  <Col md={3} lg={3}>
                    <Input
                      type="select"
                      className="bg-white border rounded-3 shadow-none"
                      value={danhMucFilter}
                      onChange={(e) => {
                        setDanhMucFilter(e.target.value);
                        setPageIndex(1);
                      }}
                    >
                      <option value="">-- Tất cả danh mục --</option>
                      {danhMucs.map((dm) => (
                        <option key={dm.id} value={dm.id}>
                          {dm.ten}
                        </option>
                      ))}
                    </Input>
                  </Col>
                  <Col md={2} lg={2}>
                    <Input
                      type="select"
                      className="bg-white border rounded-3 shadow-none"
                      value={gioiTinhFilter}
                      onChange={(e) => {
                        setGioiTinhFilter(e.target.value);
                        setPageIndex(1);
                      }}
                    >
                      <option value="">-- Giới tính (Tất cả) --</option>
                      <option value="Nam">Nam (♂)</option>
                      <option value="Nu">Nữ (♀)</option>
                      <option value="HonHop">Hỗn hợp (⚥)</option>
                    </Input>
                  </Col>
                  <Col md={2} lg={2}>
                    <Input
                      type="select"
                      className="bg-white border rounded-3 shadow-none"
                      value={trangThaiFilter}
                      onChange={(e) => {
                        setTrangThaiFilter(e.target.value);
                        setPageIndex(1);
                      }}
                    >
                      <option value="">-- Trạng thái (Tất cả) --</option>
                      <option value="true">Đang hoạt động</option>
                      <option value="false">Tạm dừng / Khóa</option>
                    </Input>
                  </Col>
                  <Col md={1} lg={2}>
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

            {/* Bảng danh sách Môn thể thao */}
            {loading ? (
              <div className="text-center py-5">
                <Spinner color="primary" />
                <p className="mt-2 text-muted small">Đang tải dữ liệu...</p>
              </div>
            ) : monTheThaos.length === 0 ? (
              <div className="text-center py-5 text-muted border rounded-3 bg-light">
                <i className="bi bi-inbox fs-1 d-block mb-2 text-secondary"></i>
                Chưa có môn thể thao nào được ghi nhận.
              </div>
            ) : (
              <div className="table-responsive">
                <Table hover className="align-middle mb-0">
                  <thead className="table-light text-uppercase fs-7 text-muted">
                    <tr>
                      <th style={{ width: '50px' }}>#</th>
                      <th>Mã Môn</th>
                      <th>Tên Môn Thể Thao</th>
                      <th>Danh Mục Phân Loại</th>
                      <th className="text-center">Loại & Thể Thức</th>
                      <th className="text-center">Quy Mô VĐV/Đội</th>
                      <th>Mô Tả</th>
                      <th>Trạng Thái</th>
                      <th className="text-end" style={{ width: '130px' }}>
                        Thao tác
                      </th>
                    </tr>
                  </thead>
                  <tbody>
                    {monTheThaos.map((item, index) => (
                      <tr key={item.id}>
                        <td className="text-muted">{(pageIndex - 1) * pageSize + index + 1}</td>
                        <td>
                          <span className="badge bg-light text-primary border font-monospace">
                            {item.ma}
                          </span>
                        </td>
                        <td>
                          <div className="fw-semibold text-dark d-flex align-items-center gap-2">
                            <span>{item.ten}</span>
                            {renderGioiTinh(item.gioiTinh)}
                          </div>
                        </td>
                        <td>
                          {item.tenDanhMuc ? (
                            <Badge color="info" pill>
                              {item.tenDanhMuc}
                            </Badge>
                          ) : (
                            <span className="text-muted small">Chưa phân loại</span>
                          )}
                        </td>
                        <td className="text-center">
                          <div className="d-flex flex-column align-items-center gap-1">
                            {item.laMonDongDoi ? (
                              <Badge color="warning" className="text-dark" pill>
                                <i className="bi bi-people me-1"></i> Đồng đội
                              </Badge>
                            ) : (
                              <Badge color="primary" pill>
                                <i className="bi bi-person me-1"></i> Cá nhân
                              </Badge>
                            )}
                            <Badge color={HINH_THUC_OPTIONS.find(h => h.value === item.hinhThucThiDau)?.badge || 'secondary'} className="small">
                              {HINH_THUC_OPTIONS.find(h => h.value === item.hinhThucThiDau)?.label || item.hinhThucThiDau || 'Loại trực tiếp'}
                            </Badge>
                          </div>
                        </td>
                        <td className="text-center">
                          {item.laMonDongDoi ? (
                            <span className="small fw-semibold text-dark">
                              {item.soLuongVanDongVienToiThieu ?? 2} - {item.soLuongVanDongVienToiDa ?? '∞'} VĐV
                            </span>
                          ) : (
                            <span className="small text-muted">1 VĐV / hồ sơ</span>
                          )}
                          {item.soDoiToiDa ? (
                            <div className="text-xs text-muted">Tối đa {item.soDoiToiDa} đội</div>
                          ) : null}
                        </td>
                        <td>
                          <div className="small text-muted text-truncate" style={{ maxWidth: '200px' }}>
                            {item.moTa || '---'}
                          </div>
                        </td>
                        <td>{renderTrangThai(item.trangThai)}</td>
                        <td className="text-end">
                          <div className="d-flex justify-content-end gap-1">
                            <Button
                              size="sm"
                              color="light"
                              className="btn-icon text-dark"
                              title="⚙️ Cấu hình xếp lịch thi đấu"
                              onClick={() => handleOpenConfigModal(item)}
                            >
                              <i className="bi bi-gear-fill"></i>
                            </Button>
                            {canEdit && (
                              <Button
                                size="sm"
                                color="light"
                                className="btn-icon text-primary"
                                title="Chỉnh sửa môn"
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
                                title="Xóa môn"
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

        {/* MODAL THÊM / SỬA MÔN */}
        <Modal isOpen={modalOpen} toggle={() => setModalOpen(!modalOpen)} backdrop="static" centered>
          <ModalHeader toggle={() => setModalOpen(!modalOpen)}>
            <i className={`bi ${editingId ? 'bi-pencil-square' : 'bi-plus-circle'} me-2 text-primary`}></i>
            {editingId ? 'Cập nhật Môn Thể Thao' : 'Thêm Môn Thể Thao Mới'}
          </ModalHeader>
          <Form onSubmit={handleSubmitForm}>
            <ModalBody>
              <FormGroup>
                <Label className="fw-semibold">
                  Danh Mục Môn <span className="text-danger">*</span>
                </Label>
                <Input
                  type="select"
                  value={formData.danhMucId}
                  onChange={(e) =>
                    setFormData({
                      ...formData,
                      danhMucId: parseInt(e.target.value, 10),
                    })
                  }
                  required
                >
                  <option value="">-- Chọn Danh Mục --</option>
                  {danhMucs.map((dm) => (
                    <option key={dm.id} value={dm.id}>
                      {dm.ten}
                    </option>
                  ))}
                </Input>
              </FormGroup>

              <FormGroup>
                <Label className="fw-semibold">
                  Mã Môn <span className="text-danger">*</span>
                </Label>
                <Input
                  type="text"
                  placeholder="VD: MON_BONGDA, MON_CAULONG..."
                  value={formData.ma}
                  onChange={(e) => setFormData({ ...formData, ma: e.target.value.toUpperCase() })}
                  required
                />
              </FormGroup>

              <FormGroup>
                <Label className="fw-semibold">
                  Tên Môn Thể Thao <span className="text-danger">*</span>
                </Label>
                <Input
                  type="text"
                  placeholder="VD: Bóng đá mini 5 người, Cầu lông..."
                  value={formData.ten}
                  onChange={(e) => setFormData({ ...formData, ten: e.target.value })}
                  required
                />
              </FormGroup>

              <FormGroup>
                <Label className="fw-semibold">Mô Tả</Label>
                <Input
                  type="textarea"
                  rows={2}
                  placeholder="Mô tả tóm tắt môn thể thao, thể thức cơ bản..."
                  value={formData.moTa}
                  onChange={(e) => setFormData({ ...formData, moTa: e.target.value })}
                />
              </FormGroup>

              <FormGroup>
                <Label className="fw-semibold">
                  Hình Thức / Thể Thức Thi Đấu <span className="text-danger">*</span>
                </Label>
                <Input
                  type="select"
                  value={formData.hinhThucThiDau || 'LoaiTrucTiep'}
                  onChange={(e) => setFormData({ ...formData, hinhThucThiDau: e.target.value })}
                  required
                >
                  {HINH_THUC_OPTIONS.map((ht) => (
                    <option key={ht.value} value={ht.value}>
                      {ht.label}
                    </option>
                  ))}
                </Input>
                <div className="form-text text-muted small">
                  Xác định cách thức sinh bảng đấu, nhánh đấu và xếp lịch thi đấu tự động cho môn này.
                </div>
              </FormGroup>

              <FormGroup>
                <Label className="fw-semibold">
                  Giới Tính Thi Đấu <span className="text-danger">*</span>
                </Label>
                <div className="d-flex gap-2">
                  <div
                    className={`p-2.5 rounded-3 border cursor-pointer flex-fill text-center transition-all ${
                      formData.gioiTinh === 'Nam'
                        ? 'border-primary bg-primary bg-opacity-10 text-primary fw-bold shadow-sm'
                        : 'bg-light text-muted'
                    }`}
                    onClick={() => setFormData({ ...formData, gioiTinh: 'Nam' })}
                  >
                    <i className="bi bi-gender-male me-1 fs-5 d-block"></i>
                    <span>Nam (♂)</span>
                  </div>
                  <div
                    className={`p-2.5 rounded-3 border cursor-pointer flex-fill text-center transition-all ${
                      formData.gioiTinh === 'Nu'
                        ? 'border-danger bg-danger bg-opacity-10 text-danger fw-bold shadow-sm'
                        : 'bg-light text-muted'
                    }`}
                    onClick={() => setFormData({ ...formData, gioiTinh: 'Nu' })}
                  >
                    <i className="bi bi-gender-female me-1 fs-5 d-block"></i>
                    <span>Nữ (♀)</span>
                  </div>
                  <div
                    className={`p-2.5 rounded-3 border cursor-pointer flex-fill text-center transition-all ${
                      formData.gioiTinh === 'HonHop'
                        ? 'border-info bg-info bg-opacity-10 text-info fw-bold shadow-sm'
                        : 'bg-light text-muted'
                    }`}
                    onClick={() => setFormData({ ...formData, gioiTinh: 'HonHop' })}
                  >
                    <i className="bi bi-gender-ambiguous me-1 fs-5 d-block"></i>
                    <span>Hỗn hợp / Tất cả (⚥)</span>
                  </div>
                </div>
                <div className="form-text text-muted small mt-1">
                  Quy định giới tính vận động viên được phép đăng ký và tham gia thi đấu cho môn này.
                </div>
              </FormGroup>

              <FormGroup switch className="mt-3">
                <Input
                  type="switch"
                  id="dongDoiSwitch"
                  checked={formData.laMonDongDoi}
                  onChange={(e) => {
                    const isTeam = e.target.checked;
                    setFormData({
                      ...formData,
                      laMonDongDoi: isTeam,
                      soLuongVanDongVienToiThieu: isTeam ? (formData.soLuongVanDongVienToiThieu && formData.soLuongVanDongVienToiThieu > 1 ? formData.soLuongVanDongVienToiThieu : 5) : 1,
                      soLuongVanDongVienToiDa: isTeam ? (formData.soLuongVanDongVienToiDa && formData.soLuongVanDongVienToiDa > 1 ? formData.soLuongVanDongVienToiDa : 10) : 1,
                    });
                  }}
                />
                <Label check for="dongDoiSwitch" className="fw-semibold ms-2">
                  Môn thi đấu tập thể / đồng đội (nhiều người trên 1 đội)
                </Label>
              </FormGroup>

              {formData.laMonDongDoi ? (
                <div className="p-3 bg-light rounded-3 mt-2 border">
                  <div className="fw-semibold small text-primary mb-2">
                    <i className="bi bi-people-fill me-1"></i> Cấu hình số lượng VĐV & Đội tham gia:
                  </div>
                  <Row className="g-2">
                    <Col md={4}>
                      <Label className="small fw-semibold">VĐV tối thiểu / đội</Label>
                      <Input
                        type="number"
                        min={1}
                        value={formData.soLuongVanDongVienToiThieu ?? ''}
                        onChange={(e) =>
                          setFormData({
                            ...formData,
                            soLuongVanDongVienToiThieu: e.target.value ? parseInt(e.target.value, 10) : undefined,
                          })
                        }
                        placeholder="VD: 5"
                      />
                    </Col>
                    <Col md={4}>
                      <Label className="small fw-semibold">VĐV tối đa / đội</Label>
                      <Input
                        type="number"
                        min={formData.soLuongVanDongVienToiThieu ?? 1}
                        value={formData.soLuongVanDongVienToiDa ?? ''}
                        onChange={(e) =>
                          setFormData({
                            ...formData,
                            soLuongVanDongVienToiDa: e.target.value ? parseInt(e.target.value, 10) : undefined,
                          })
                        }
                        placeholder="VD: 10"
                      />
                    </Col>
                    <Col md={4}>
                      <Label className="small fw-semibold">Số đội tối đa</Label>
                      <Input
                        type="number"
                        min={2}
                        value={formData.soDoiToiDa ?? ''}
                        onChange={(e) =>
                          setFormData({
                            ...formData,
                            soDoiToiDa: e.target.value ? parseInt(e.target.value, 10) : undefined,
                          })
                        }
                        placeholder="Không giới hạn"
                      />
                    </Col>
                  </Row>
                </div>
              ) : (
                <div className="p-2 bg-light rounded mt-2 border text-muted small">
                  <i className="bi bi-info-circle me-1 text-primary"></i> Môn cá nhân: Mỗi hồ sơ đăng ký thi đấu có đúng <strong>1 vận động viên</strong>.
                </div>
              )}

              <FormGroup switch className="mt-2">
                <Input
                  type="switch"
                  id="monTrangThaiSwitch"
                  checked={formData.trangThai}
                  onChange={(e) => setFormData({ ...formData, trangThai: e.target.checked })}
                />
                <Label check for="monTrangThaiSwitch" className="fw-semibold ms-2">
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
            Bạn có chắc chắn muốn xóa môn <strong>{deletingItem?.ten}</strong> ({deletingItem?.ma}) không?
            <p className="text-muted small mt-2 mb-0">Hành động này sẽ chuyển trạng thái môn thể thao sang đã xóa.</p>
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
        {/* MODAL CẤU HÌNH XẾP LỊCH THEO MÔN */}
        <Modal isOpen={configModalOpen} toggle={() => setConfigModalOpen(!configModalOpen)} size="lg" centered backdrop="static">
          <ModalHeader toggle={() => setConfigModalOpen(!configModalOpen)} className="border-bottom bg-light">
            <div className="d-flex align-items-center gap-2">
              <i className="bi bi-gear-wide-connected text-primary fs-5"></i>
              <span>Cấu Hình Xếp Lịch Thi Đấu Mặc Định: <strong>{selectedMonForConfig?.ten}</strong></span>
            </div>
          </ModalHeader>
          <Form onSubmit={handleSaveConfig}>
            <ModalBody className="p-4" style={{ maxHeight: '75vh', overflowY: 'auto' }}>
              {configLoading ? (
                <div className="text-center py-5">
                  <Spinner color="primary" />
                  <p className="mt-2 text-muted small">Đang tải cấu hình xếp lịch...</p>
                </div>
              ) : (
                <Row className="g-3">
                  {/* Section 1: Dàn Trải Lịch Thi Đấu */}
                  <Col xs={12}>
                    <div className="p-3 rounded-3 bg-light border">
                      <h6 className="fw-bold text-primary mb-3 d-flex align-items-center gap-2">
                        <i className="bi bi-calendar-range"></i> 1. Quy Tắc Dàn Trải Lịch Thi Đấu
                      </h6>
                      <Row className="g-3">
                        <Col md={6}>
                          <FormGroup switch className="mb-0">
                            <Input
                              type="switch"
                              id="moiVongMotNgay"
                              checked={configData.moiVongMotNgay}
                              onChange={(e) => setConfigData({ ...configData, moiVongMotNgay: e.target.checked })}
                            />
                            <Label check for="moiVongMotNgay" className="fw-semibold small ms-2">
                              Mỗi vòng thi đấu 1 ngày riêng biệt
                            </Label>
                          </FormGroup>
                        </Col>
                        <Col md={6}>
                          <FormGroup switch className="mb-0">
                            <Input
                              type="switch"
                              id="uuTienChungKetNgayCuoi"
                              checked={configData.uuTienChungKetNgayCuoi}
                              onChange={(e) => setConfigData({ ...configData, uuTienChungKetNgayCuoi: e.target.checked })}
                            />
                            <Label check for="uuTienChungKetNgayCuoi" className="fw-semibold small ms-2">
                              Ưu tiên xếp Chung kết vào ngày bế mạc
                            </Label>
                          </FormGroup>
                        </Col>
                        <Col md={6}>
                          <Label className="small fw-semibold">Khoảng cách giữa các vòng (giờ)</Label>
                          <Input
                            type="number"
                            bsSize="sm"
                            min={1}

                            value={configData.khoangCachGiuaCacVongGio}
                            onChange={(e) => setConfigData({ ...configData, khoangCachGiuaCacVongGio: Number(e.target.value) })}
                          />
                        </Col>
                      </Row>
                    </div>
                  </Col>

                  {/* Section 2: Giảm Tải Thể Lực VĐV */}
                  <Col xs={12}>
                    <div className="p-3 rounded-3 bg-light border">
                      <h6 className="fw-bold text-danger mb-3 d-flex align-items-center gap-2">
                        <i className="bi bi-heart-pulse"></i> 2. Giảm Tải Thể Lực VĐV & Giới Hạn Trận
                      </h6>
                      <Row className="g-3">
                        <Col md={6}>
                          <Label className="small fw-semibold">Số trận tối đa / đội (VĐV) / 1 ngày</Label>
                          <Input
                            type="number"
                            bsSize="sm"
                            min={1}
                            max={5}
                            value={configData.soTranToiDaMoiDoiMoiNgay}
                            onChange={(e) => setConfigData({ ...configData, soTranToiDaMoiDoiMoiNgay: Number(e.target.value) })}
                          />
                          <span className="text-muted" style={{ fontSize: '11px' }}>
                            Khống chế không cho 1 đội/VĐV thi quá số trận này trong cùng 1 ngày.
                          </span>
                        </Col>
                        <Col md={6}>
                          <Label className="small fw-semibold">Thời gian nghỉ tối thiểu giữa 2 trận (phút)</Label>
                          <Input
                            type="number"
                            bsSize="sm"
                            min={15}

                            step={15}
                            value={configData.nghiToiThieuGiua2TranPhut}
                            onChange={(e) => setConfigData({ ...configData, nghiToiThieuGiua2TranPhut: Number(e.target.value) })}
                          />
                        </Col>
                      </Row>
                    </div>
                  </Col>

                  {/* Section 3: Ca Thi Đấu */}
                  <Col xs={12}>
                    <div className="p-3 rounded-3 bg-light border">
                      <h6 className="fw-bold text-dark mb-3 d-flex align-items-center gap-2">
                        <i className="bi bi-clock-history text-warning"></i> 3. Khung Giờ Ca Thi Đấu (Sáng / Chiều)
                      </h6>
                      <FormGroup switch className="mb-3">
                        <Input
                          type="switch"
                          id="chiaCaThiDau"
                          checked={configData.chiaCaThiDau}
                          onChange={(e) => setConfigData({ ...configData, chiaCaThiDau: e.target.checked })}
                        />
                        <Label check for="chiaCaThiDau" className="fw-semibold small ms-2">
                          Bật chế độ chia ca (tránh giờ nghỉ trưa 11:30–14:00)
                        </Label>
                      </FormGroup>

                      {configData.chiaCaThiDau && (
                        <Row className="g-3">
                          <Col md={6}>
                            <Label className="small fw-semibold">Ca Sáng</Label>
                            <div className="d-flex align-items-center gap-2">
                              <Input
                                type="time"
                                bsSize="sm"
                                value={configData.caSangBatDau}
                                onChange={(e) => setConfigData({ ...configData, caSangBatDau: e.target.value })}
                              />
                              <span>→</span>
                              <Input
                                type="time"
                                bsSize="sm"
                                value={configData.caSangKetThuc}
                                onChange={(e) => setConfigData({ ...configData, caSangKetThuc: e.target.value })}
                              />
                            </div>
                          </Col>
                          <Col md={6}>
                            <Label className="small fw-semibold">Ca Chiều</Label>
                            <div className="d-flex align-items-center gap-2">
                              <Input
                                type="time"
                                bsSize="sm"
                                value={configData.caChieuBatDau}
                                onChange={(e) => setConfigData({ ...configData, caChieuBatDau: e.target.value })}
                              />
                              <span>→</span>
                              <Input
                                type="time"
                                bsSize="sm"
                                value={configData.caChieuKetThuc}
                                onChange={(e) => setConfigData({ ...configData, caChieuKetThuc: e.target.value })}
                              />
                            </div>
                          </Col>
                        </Row>
                      )}
                    </div>
                  </Col>

                  {/* Section 4: Sân đấu & Trọng tài & VĐV Đa Môn */}
                  <Col xs={12}>
                    <div className="p-3 rounded-3 bg-light border">
                      <h6 className="fw-bold text-success mb-3 d-flex align-items-center gap-2">
                        <i className="bi bi-shield-check"></i> 4. Sân Đấu, Trọng Tài & VĐV Đa Môn
                      </h6>
                      <Row className="g-3">
                        <Col md={4}>
                          <Label className="small fw-semibold">Đệm dọn sân giữa các trận (phút)</Label>
                          <Input
                            type="number"
                            bsSize="sm"
                            min={0}
                            max={60}
                            value={configData.thoiGianDemDonSanPhut}
                            onChange={(e) => setConfigData({ ...configData, thoiGianDemDonSanPhut: Number(e.target.value) })}
                          />
                        </Col>
                        <Col md={4}>
                          <Label className="small fw-semibold">Max trận / 1 Trọng tài / ngày</Label>
                          <Input
                            type="number"
                            bsSize="sm"
                            min={1}
                            max={10}
                            value={configData.soTranToiDaMoiTrongTaiMoiNgay}
                            onChange={(e) => setConfigData({ ...configData, soTranToiDaMoiTrongTaiMoiNgay: Number(e.target.value) })}
                          />
                        </Col>
                        <Col md={4}>
                          <Label className="small fw-semibold">Buffer di chuyển VĐV đa môn (phút)</Label>
                          <Input
                            type="number"
                            bsSize="sm"
                            min={0}
                            max={120}
                            value={configData.thoiGianDemDiChuyenPhut}
                            onChange={(e) => setConfigData({ ...configData, thoiGianDemDiChuyenPhut: Number(e.target.value) })}
                          />
                        </Col>
                      </Row>
                    </div>
                  </Col>

                  {/* Section 5: Giá trị mặc định cho Modal xếp lịch */}
                  <Col xs={12}>
                    <div className="p-3 rounded-3 bg-light border">
                      <h6 className="fw-bold text-secondary mb-3 d-flex align-items-center gap-2">
                        <i className="bi bi-sliders"></i> 5. Giá Trị Mặc Định Điền Sẵn Khi Mở Modal Xếp Lịch
                      </h6>
                      <Row className="g-3">
                        <Col md={4}>
                          <Label className="small fw-semibold">Thời lượng trận (phút)</Label>
                          <Input
                            type="number"
                            bsSize="sm"
                            value={configData.thoiLuongTranMacDinhPhut}
                            onChange={(e) => setConfigData({ ...configData, thoiLuongTranMacDinhPhut: Number(e.target.value) })}
                          />
                        </Col>
                        <Col md={4}>
                          <Label className="small fw-semibold">Số hiệp đấu</Label>
                          <Input
                            type="number"
                            bsSize="sm"
                            value={configData.soHiepDauMacDinh}
                            onChange={(e) => setConfigData({ ...configData, soHiepDauMacDinh: Number(e.target.value) })}
                          />
                        </Col>
                        <Col md={4}>
                          <Label className="small fw-semibold">Thời gian 1 hiệp (phút)</Label>
                          <Input
                            type="number"
                            bsSize="sm"
                            value={configData.thoiGianMoiHiepPhut}
                            onChange={(e) => setConfigData({ ...configData, thoiGianMoiHiepPhut: Number(e.target.value) })}
                          />
                        </Col>
                      </Row>
                    </div>
                  </Col>
                </Row>
              )}
            </ModalBody>
            <ModalFooter className="border-top d-flex justify-content-between">
              <Button color="outline-danger" size="sm" type="button" onClick={handleResetConfig} disabled={configSubmitting}>
                <i className="bi bi-arrow-counterclockwise me-1"></i> Đặt Lại Mặc Định
              </Button>
              <div className="d-flex gap-2">
                <Button color="secondary" size="sm" outline onClick={() => setConfigModalOpen(false)} disabled={configSubmitting}>
                  Hủy
                </Button>
                <Button color="primary" size="sm" type="submit" disabled={configSubmitting} className="fw-semibold px-4">
                  {configSubmitting ? <Spinner size="sm" /> : <i className="bi bi-save me-1"></i>}
                  Lưu Cấu Hình
                </Button>
              </div>
            </ModalFooter>
          </Form>
        </Modal>
      </Col>
    </Row>
  );
}
