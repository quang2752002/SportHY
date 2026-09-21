'use client';

import React, { useEffect, useState, use } from 'react';
import { useRouter } from 'next/navigation';
import Link from 'next/link';
import {
  Row,
  Col,
  Card,
  CardBody,
  Button,
  Form,
  FormGroup,
  Label,
  Input,
  Spinner,
} from 'reactstrap';
import { giaiDauService, khoiService, monTheThaoService } from '@/services';
import {
  CreateUpdateGiaiDau,
  PhamViGiaiDau,
  PhamViGiaiDauLabels,
  TrangThaiGiaiDau,
  TrangThaiGiaiDauLabels,
  MonTheThao,
  CreateUpdateDieuLeGiaiDau,
} from '@/types';
import { useAuth, useToast } from '@/context/AuthContext';
import { Permissions } from '@/constants/permissions';
import { generateSlug } from '@/lib/slug';

export default function EditGiaiDauPage({ params }: { params: Promise<{ id: string }> }) {
  const unwrappedParams = use(params);
  const id = Number(unwrappedParams.id);

  const router = useRouter();
  const { hasPermission, user } = useAuth();
  const toast = useToast();
  const isAdmin = (user?.roles || []).some((r) => String(r).toLowerCase() === 'admin');
  const canEdit = isAdmin || hasPermission(Permissions.GiaiDau.Edit);

  const [formData, setFormData] = useState<CreateUpdateGiaiDau>({
    ma: '',
    ten: '',
    moTa: '',
    ngayBatDau: '',
    ngayKetThuc: '',
    hanDangKy: '',
    diaDiem: '',
    phamVi: PhamViGiaiDau.TatCa,
    trangThai: TrangThaiGiaiDau.Nhap,
    hinhAnh: '',
    khoiIds: [],
    monTheThaoIds: [],
    dieuLes: [],
  });

  const [availableKhois, setAvailableKhois] = useState<{ id: number; ma: string; ten: string }[]>([]);
  const [availableMons, setAvailableMons] = useState<MonTheThao[]>([]);
  const [loadingInitial, setLoadingInitial] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [uploadingBanner, setUploadingBanner] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleBannerUpload = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    setUploadingBanner(true);
    try {
      const res = await giaiDauService.uploadBanner(file);
      if (res?.url) {
        setFormData((prev) => ({ ...prev, hinhAnh: res.url }));
        toast.success('Tải lên ảnh banner giải đấu thành công!');
      }
    } catch (err: any) {
      toast.error('Không thể tải lên banner: ' + (err?.response?.data?.message || err?.message || 'Lỗi server'));
    } finally {
      setUploadingBanner(false);
      e.target.value = '';
    }
  };

  const [uploadingDieuLeIndex, setUploadingDieuLeIndex] = useState<number | null>(null);

  const handleDieuLeFileUpload = async (index: number, e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    setUploadingDieuLeIndex(index);
    try {
      const res = await giaiDauService.uploadDieuLeFile(file);
      if (res?.url) {
        handleDieuLeChange(index, 'tepDinhKem', res.url);
        toast.success('Tải lên tệp điều lệ thành công!');
      }
    } catch (err: any) {
      toast.error('Không thể tải lên tệp điều lệ: ' + (err?.response?.data?.message || err?.message || 'Lỗi server'));
    } finally {
      setUploadingDieuLeIndex(null);
      e.target.value = '';
    }
  };

  useEffect(() => {
    async function loadData() {
      try {
        const [khoiData, monData, giaiDauData] = await Promise.all([
          khoiService.getAll(),
          monTheThaoService.getAll(),
          giaiDauService.getById(id),
        ]);

        setAvailableKhois(khoiData || []);
        setAvailableMons(monData || []);

        if (giaiDauData) {
          // Format date yyyy-MM-dd cho input date HTML (tránh lệch ngày do múi giờ UTC)
          const formatDate = (dateStr?: string | Date) => {
            if (!dateStr) return '';
            if (typeof dateStr === 'string') {
              const match = dateStr.match(/^\d{4}-\d{2}-\d{2}/);
              if (match) return match[0];
            }
            const d = new Date(dateStr);
            if (isNaN(d.getTime())) return '';
            const year = d.getFullYear();
            const month = String(d.getMonth() + 1).padStart(2, '0');
            const day = String(d.getDate()).padStart(2, '0');
            return `${year}-${month}-${day}`;
          };

          setFormData({
            ma: giaiDauData.ma || '',
            ten: giaiDauData.ten || '',
            slug: giaiDauData.slug || '',
            moTa: giaiDauData.moTa || '',
            hinhAnh: giaiDauData.hinhAnh || '',
            ngayBatDau: formatDate(giaiDauData.ngayBatDau),
            ngayKetThuc: formatDate(giaiDauData.ngayKetThuc),
            hanDangKy: formatDate(giaiDauData.hanDangKy),
            diaDiem: giaiDauData.diaDiem || '',
            phamVi: giaiDauData.phamVi,
            trangThai: giaiDauData.trangThai,
            khoiIds: Array.from(new Set(giaiDauData.khoiIds || [])),
            monTheThaoIds: Array.from(new Set(giaiDauData.monTheThaoIds || [])),
            dieuLes: (giaiDauData.dieuLeGiaiDaus || []).map((dl) => ({
              id: dl.id,
              tieuDe: dl.tieuDe,
              noiDung: dl.noiDung,
              tepDinhKem: dl.tepDinhKem,
              thuTu: dl.thuTu,
              trangThai: dl.trangThai,
            })),
          });
        }
      } catch (err: any) {
        console.error('Lỗi khi nạp dữ liệu giải đấu:', err);
        setError(err?.message || 'Không thể tải thông tin giải đấu.');
      } finally {
        setLoadingInitial(false);
      }
    }

    if (id) {
      loadData();
    }
  }, [id]);

  const handleKhoiToggle = (khoiId: number) => {
    setFormData((prev) => {
      const current = prev.khoiIds || [];
      return {
        ...prev,
        khoiIds: current.includes(khoiId) ? current.filter((i) => i !== khoiId) : [...current, khoiId],
      };
    });
  };

  const handleMonToggle = (monId: number) => {
    setFormData((prev) => {
      const current = prev.monTheThaoIds || [];
      return {
        ...prev,
        monTheThaoIds: current.includes(monId) ? current.filter((i) => i !== monId) : [...current, monId],
      };
    });
  };

  const handleSelectAllMons = () => {
    setFormData((prev) => {
      const current = prev.monTheThaoIds || [];
      return {
        ...prev,
        monTheThaoIds: current.length === availableMons.length ? [] : availableMons.map((m) => m.id),
      };
    });
  };

  // Quản lý Điều lệ
  const handleAddDieuLe = () => {
    setFormData((prev) => {
      const current = prev.dieuLes || [];
      const newDieuLe: CreateUpdateDieuLeGiaiDau = {
        tieuDe: '',
        noiDung: '',
        tepDinhKem: '',
        thuTu: current.length + 1,
        trangThai: true,
      };
      return {
        ...prev,
        dieuLes: [...current, newDieuLe],
      };
    });
  };

  const handleRemoveDieuLe = (index: number) => {
    setFormData((prev) => {
      const current = [...(prev.dieuLes || [])];
      current.splice(index, 1);
      return { ...prev, dieuLes: current };
    });
  };

  const handleDieuLeChange = (
    index: number,
    field: keyof CreateUpdateDieuLeGiaiDau,
    value: any
  ) => {
    setFormData((prev) => {
      const current = [...(prev.dieuLes || [])];
      current[index] = { ...current[index], [field]: value };
      return { ...prev, dieuLes: current };
    });
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    // 1. Kiểm tra các trường bắt buộc bằng Toast popup
    if (!formData.ma?.trim()) {
      toast.error('Vui lòng nhập Mã giải đấu!', 'Thiếu thông tin');
      const el = document.getElementById('input_ma');
      if (el) el.focus();
      return;
    }

    if (!formData.ten?.trim()) {
      toast.error('Vui lòng nhập Tên giải đấu!', 'Thiếu thông tin');
      const el = document.getElementById('input_ten');
      if (el) el.focus();
      return;
    }

    if (!formData.ngayBatDau) {
      toast.error('Vui lòng chọn Ngày bắt đầu giải đấu!', 'Thiếu thông tin');
      const el = document.getElementById('input_ngayBatDau');
      if (el) el.focus();
      return;
    }

    if (!formData.ngayKetThuc) {
      toast.error('Vui lòng chọn Ngày kết thúc giải đấu!', 'Thiếu thông tin');
      const el = document.getElementById('input_ngayKetThuc');
      if (el) el.focus();
      return;
    }

    if (formData.ngayBatDau && formData.ngayKetThuc && formData.ngayKetThuc < formData.ngayBatDau) {
      toast.warning('Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu!', 'Thời gian không hợp lệ');
      return;
    }

    if (!formData.monTheThaoIds || formData.monTheThaoIds.length === 0) {
      toast.error('Vui lòng chọn ít nhất một môn thể thao tổ chức!', 'Thiếu môn thi đấu');
      return;
    }

    if (formData.phamVi === PhamViGiaiDau.TheoKhoi && (!formData.khoiIds || formData.khoiIds.length === 0)) {
      toast.error('Vui lòng chọn ít nhất một Khối tham gia khi áp dụng theo khối ngành!', 'Thiếu khối tham gia');
      return;
    }

    setSubmitting(true);
    setError(null);
    try {
      await giaiDauService.update(id, formData);
      toast.success('Cập nhật giải đấu thành công!', 'Hoàn tất');
      router.push('/quan-ly-giai/giai-dau');
    } catch (err: any) {
      console.error('Lỗi khi cập nhật giải đấu:', err);
      const msg = err?.response?.data?.message || err?.message || 'Có lỗi xảy ra khi cập nhật giải đấu.';
      setError(msg);
      toast.error(msg, 'Lỗi cập nhật giải đấu');
    } finally {
      setSubmitting(false);
    }
  };

  if (loadingInitial) {
    return (
      <div className="text-center py-5">
        <Spinner color="primary" />
        <p className="mt-2 text-muted small">Đang nạp dữ liệu giải đấu...</p>
      </div>
    );
  }

  return (
    <div className="pb-5">
      {/* Header Điều Hướng */}
      <div className="d-flex align-items-center justify-content-between mb-4">
        <div className="d-flex align-items-center gap-3">
          <Link
            href="/quan-ly-giai/giai-dau"
            className="btn btn-outline-secondary btn-sm rounded-circle d-flex align-items-center justify-content-center"
            style={{ width: '38px', height: '38px' }}
          >
            <i className="bi bi-arrow-left fs-5"></i>
          </Link>
          <div>
            <h4 className="fw-bold mb-0 text-dark">
              Chỉnh Sửa Giải Đấu <span className="text-primary font-monospace">#{formData.ma}</span>
            </h4>
            <span className="text-muted small">
              Cập nhật thông tin, danh sách môn thể thao &amp; các điều lệ của giải đấu
            </span>
          </div>
        </div>
        <div className="d-flex gap-2">
          <Link
            href={`/quan-ly-giai/giai-dau/${id}/du-lieu-thi-dau`}
            className="btn btn-outline-danger rounded-3 px-3 d-inline-flex align-items-center gap-2"
            title="Quản lý & xóa các hồ sơ đăng ký, trận đấu để có thể gỡ môn thi đấu"
          >
            <i className="bi bi-shield-x"></i>
            Đăng Ký &amp; Trận Đấu
          </Link>
          <Link
            href={`/quan-ly-giai/lich-thi-dau?giaiDauId=${id}`}
            className="btn btn-outline-primary rounded-3 px-3 d-inline-flex align-items-center gap-2"
            title="Xếp lịch thi đấu chi tiết"
          >
            <i className="bi bi-calendar3"></i>
            Lịch Thi Đấu
          </Link>
          <Link href="/quan-ly-giai/giai-dau" className="btn btn-light rounded-3 px-3">
            Hủy bỏ
          </Link>
          <Button
            color="primary"
            onClick={handleSubmit}
            disabled={submitting || !canEdit}
            className="rounded-3 px-4 fw-semibold d-inline-flex align-items-center gap-2"
          >
            {submitting ? <Spinner size="sm" /> : <i className="bi bi-save"></i>}
            Cập Nhật Giải Đấu
          </Button>
        </div>
      </div>

      {error && (
        <div className="alert alert-danger d-flex align-items-center justify-content-between mb-4 rounded-3 shadow-sm" role="alert">
          <div className="d-flex align-items-center">
            <i className="bi bi-exclamation-triangle-fill me-2 fs-5 text-danger flex-shrink-0"></i>
            <div>{error}</div>
          </div>
          {(error.includes('đăng ký') || error.includes('trận đấu') || error.includes('bảng đấu') || error.includes('bỏ chọn môn')) && (
            <Link
              href={`/quan-ly-giai/giai-dau/${id}/du-lieu-thi-dau`}
              className="btn btn-sm btn-danger rounded-pill px-3 ms-3 text-nowrap d-inline-flex align-items-center gap-1 shadow-sm"
            >
              <i className="bi bi-trash3"></i>
              Mở Trang Xóa Dữ Liệu
            </Link>
          )}
        </div>
      )}

      <Form onSubmit={handleSubmit}>
        <Row className="g-4">
          {/* CỘT TRÁI */}
          <Col lg={8}>
            {/* THÔNG TIN CHUNG */}
            <Card className="border-0 shadow-sm rounded-4 mb-4">
              <CardBody className="p-4">
                <h5 className="fw-bold text-dark mb-3 d-flex align-items-center gap-2">
                  <i className="bi bi-info-circle text-primary"></i> Thông Tin Chung
                </h5>

                <Row className="g-3">
                  <Col md={4}>
                    <FormGroup>
                      <Label className="fw-semibold small">
                        Mã giải đấu <span className="text-danger">*</span>
                      </Label>
                      <Input
                        id="input_ma"
                        type="text"
                        required
                        value={formData.ma}
                        onChange={(e) => setFormData((prev) => ({ ...prev, ma: e.target.value }))}
                        className="rounded-3"
                      />
                    </FormGroup>
                  </Col>
                  <Col md={8}>
                    <FormGroup>
                      <Label className="fw-semibold small">
                        Tên giải đấu <span className="text-danger">*</span>
                      </Label>
                      <Input
                        id="input_ten"
                        type="text"
                        required
                        value={formData.ten}
                        onChange={(e) => {
                          const newTen = e.target.value;
                          setFormData((prev) => ({
                            ...prev,
                            ten: newTen,
                            slug: generateSlug(newTen),
                          }));
                        }}
                        className="rounded-3"
                      />
                    </FormGroup>
                  </Col>

                  <Col md={12}>
                    <FormGroup>
                      <div className="d-flex justify-content-between align-items-center mb-1">
                        <Label className="fw-semibold small mb-0">
                          Đường dẫn thân thiện SEO (Slug URL) <span className="text-danger">*</span>
                        </Label>
                        <div className="d-flex align-items-center gap-2">
                          <button
                            type="button"
                            className="btn btn-link btn-sm p-0 text-decoration-none text-primary"
                            onClick={() => setFormData((prev) => ({ ...prev, slug: generateSlug(prev.ten) }))}
                          >
                            <i className="bi bi-arrow-repeat me-1"></i>Tạo lại từ tên
                          </button>
                        </div>
                      </div>
                      <div className="input-group">
                        <span className="input-group-text bg-light text-muted small border-end-0">
                          /giai-dau/
                        </span>
                        <Input
                          type="text"
                          readOnly
                          placeholder="ten-giai-dau-2026"
                          value={formData.slug || ''}
                          className="rounded-end-3 font-monospace small bg-light text-muted"
                        />
                      </div>
                    </FormGroup>
                  </Col>

                  <Col md={4}>
                    <FormGroup>
                      <Label className="fw-semibold small">
                        Ngày bắt đầu <span className="text-danger">*</span>
                      </Label>
                      <Input
                        id="input_ngayBatDau"
                        type="date"
                        required
                        value={formData.ngayBatDau}
                        onChange={(e) => setFormData((prev) => ({ ...prev, ngayBatDau: e.target.value }))}
                        className="rounded-3"
                      />
                    </FormGroup>
                  </Col>
                  <Col md={4}>
                    <FormGroup>
                      <Label className="fw-semibold small">
                        Ngày kết thúc <span className="text-danger">*</span>
                      </Label>
                      <Input
                        id="input_ngayKetThuc"
                        type="date"
                        required
                        value={formData.ngayKetThuc}
                        onChange={(e) => setFormData((prev) => ({ ...prev, ngayKetThuc: e.target.value }))}
                        className="rounded-3"
                      />
                    </FormGroup>
                  </Col>
                  <Col md={4}>
                    <FormGroup>
                      <Label className="fw-semibold small">
                        Hạn chót đăng ký
                      </Label>
                      <Input
                        id="input_hanDangKy"
                        type="date"
                        value={formData.hanDangKy || ''}
                        onChange={(e) => setFormData((prev) => ({ ...prev, hanDangKy: e.target.value }))}
                        className="rounded-3"
                      />
                      <small className="text-muted d-block mt-1" style={{ fontSize: '11px' }}>
                        Mặc định bằng ngày bắt đầu nếu để trống. Quá hạn các đoàn không thể nộp/sửa/xóa hồ sơ.
                      </small>
                    </FormGroup>
                  </Col>

                  <Col md={12}>
                    <FormGroup>
                      <Label className="fw-semibold small">Địa điểm tổ chức</Label>
                      <Input
                        type="text"
                        placeholder="VD: Nhà thi đấu Đa năng Tỉnh..."
                        value={formData.diaDiem}
                        onChange={(e) => setFormData((prev) => ({ ...prev, diaDiem: e.target.value }))}
                        className="rounded-3"
                      />
                    </FormGroup>
                  </Col>

                  <Col md={12}>
                    <FormGroup>
                      <Label className="fw-semibold small">Mô tả tóm tắt giải đấu</Label>
                      <Input
                        type="textarea"
                        rows={3}
                        value={formData.moTa}
                        onChange={(e) => setFormData((prev) => ({ ...prev, moTa: e.target.value }))}
                        className="rounded-3"
                      />
                    </FormGroup>
                  </Col>
                </Row>
              </CardBody>
            </Card>

            {/* CÁC MÔN THI ĐẤU TỔ CHỨC */}
            <Card className="border-0 shadow-sm rounded-4 mb-4">
              <CardBody className="p-4">
                <div className="d-flex justify-content-between align-items-center mb-3">
                  <h5 className="fw-bold text-dark mb-0 d-flex align-items-center gap-2">
                    <i className="bi bi-dribbble text-primary"></i> Các Môn Thể Thao Tổ Chức
                  </h5>
                  <div className="d-flex gap-2">
                    <Link
                      href={`/quan-ly-giai/giai-dau/${id}/du-lieu-thi-dau`}
                      className="btn btn-sm btn-outline-danger rounded-pill px-3 d-inline-flex align-items-center gap-1"
                      title="Xem và xóa các đăng ký hoặc lịch thi đấu nếu cần gỡ môn"
                    >
                      <i className="bi bi-trash3"></i>
                      Quản lý / Xóa dữ liệu thi đấu
                    </Link>
                    <Button
                      size="sm"
                      color="outline-primary"
                      onClick={handleSelectAllMons}
                      className="rounded-pill px-3"
                    >
                      {(formData.monTheThaoIds || []).length === availableMons.length
                        ? 'Bỏ chọn tất cả'
                        : 'Chọn tất cả'}
                    </Button>
                  </div>
                </div>

                <p className="text-muted small mb-3">
                  Tích chọn các môn thi đấu diễn ra trong khuôn khổ giải đấu này:
                </p>

                {availableMons.length === 0 ? (
                  <p className="text-muted italic">Chưa có môn thể thao nào trong hệ thống.</p>
                ) : (
                  <div className="row g-2">
                    {availableMons.map((mon) => {
                      const isChecked = (formData.monTheThaoIds || []).includes(mon.id);
                      return (
                        <div key={mon.id} className="col-md-6 col-lg-4">
                          <div
                            onClick={() => handleMonToggle(mon.id)}
                            className={`p-2.5 rounded-3 border d-flex align-items-center gap-2 cursor-pointer transition-all ${isChecked
                                ? 'bg-primary-subtle border-primary text-primary fw-semibold'
                                : 'bg-light border-light-subtle text-dark'
                              }`}
                            style={{ cursor: 'pointer' }}
                          >
                            <Input
                              type="checkbox"
                              checked={isChecked}
                              onChange={() => { }}
                              className="m-0"
                            />
                            <span className="small text-truncate">{mon.ten}</span>
                            {mon.tenDanhMuc && (
                              <span className="badge bg-secondary-subtle text-secondary small ms-auto font-monospace" style={{ fontSize: '10px' }}>
                                {mon.tenDanhMuc}
                              </span>
                            )}
                          </div>
                        </div>
                      );
                    })}
                  </div>
                )}
              </CardBody>
            </Card>

            {/* ĐIỀU LỆ GIẢI ĐẤU */}
            <Card className="border-0 shadow-sm rounded-4 mb-4">
              <CardBody className="p-4">
                <div className="d-flex justify-content-between align-items-center mb-3">
                  <div>
                    <h5 className="fw-bold text-dark mb-0 d-flex align-items-center gap-2">
                      <i className="bi bi-file-earmark-text-fill text-primary"></i> Điều Lệ Giải Đấu
                    </h5>
                    <small className="text-muted">
                      Các chương, điều khoản, quy định khen thưởng, kỷ luật trong giải
                    </small>
                  </div>
                  <Button
                    size="sm"
                    color="primary"
                    onClick={handleAddDieuLe}
                    className="rounded-3 d-inline-flex align-items-center gap-1"
                  >
                    <i className="bi bi-plus-lg"></i> Thêm mục điều lệ
                  </Button>
                </div>

                {(!formData.dieuLes || formData.dieuLes.length === 0) ? (
                  <div className="text-center py-4 bg-light rounded-3 border border-dashed">
                    <i className="bi bi-journal-plus fs-2 text-muted d-block mb-1"></i>
                    <p className="text-muted small mb-2">Chưa có mục điều lệ nào được thêm.</p>
                    <Button size="sm" color="outline-primary" onClick={handleAddDieuLe}>
                      Thêm Điều Lệ Đầu Tiên
                    </Button>
                  </div>
                ) : (
                  <div className="d-flex flex-column gap-3">
                    {formData.dieuLes.map((item, index) => (
                      <div
                        key={index}
                        className="p-3 bg-light rounded-3 border position-relative"
                      >
                        <div className="d-flex justify-content-between align-items-center mb-2">
                          <span className="badge bg-primary rounded-pill px-2.5 py-1">
                            Mục #{index + 1}
                          </span>
                          <Button
                            size="sm"
                            color="light"
                            className="btn-icon text-danger py-0 px-2"
                            title="Xóa mục điều lệ"
                            onClick={() => handleRemoveDieuLe(index)}
                          >
                            <i className="bi bi-trash"></i>
                          </Button>
                        </div>

                        <Row className="g-2">
                          <Col md={8}>
                            <FormGroup className="mb-2">
                              <Label className="small fw-semibold">
                                Tiêu đề mục điều lệ <span className="text-danger">*</span>
                              </Label>
                              <Input
                                type="text"
                                placeholder="VD: Điều 1: Đối tượng và điều kiện tham dự"
                                value={item.tieuDe}
                                onChange={(e) => handleDieuLeChange(index, 'tieuDe', e.target.value)}
                                className="bg-white rounded-2"
                              />
                            </FormGroup>
                          </Col>
                          <Col md={4}>
                            <FormGroup className="mb-2">
                              <Label className="small fw-semibold">Thứ tự hiển thị</Label>
                              <Input
                                type="number"
                                value={item.thuTu}
                                onChange={(e) =>
                                  handleDieuLeChange(index, 'thuTu', Number(e.target.value))
                                }
                                className="bg-white rounded-2"
                              />
                            </FormGroup>
                          </Col>
                          <Col md={12}>
                            <FormGroup className="mb-2">
                              <Label className="small fw-semibold">Nội dung chi tiết điều lệ</Label>
                              <Input
                                type="textarea"
                                rows={4}
                                placeholder="Nhập quy định chi tiết, thể thức thi đấu, tiêu chuẩn tính điểm..."
                                value={item.noiDung}
                                onChange={(e) => handleDieuLeChange(index, 'noiDung', e.target.value)}
                                className="bg-white rounded-2"
                              />
                            </FormGroup>
                          </Col>
                          <Col md={12}>
                            <FormGroup className="mb-0">
                              <Label className="small fw-semibold">Tệp văn bản điều lệ đính kèm (PDF / Word / Excel / Ảnh)</Label>
                              <div className="d-flex flex-wrap align-items-center gap-2">
                                <div className="flex-grow-1">
                                  <Input
                                    type="text"
                                    placeholder="Đường dẫn tệp (hoặc bấm Tải tệp lên ở bên cạnh)..."
                                    value={item.tepDinhKem || ''}
                                    onChange={(e) =>
                                      handleDieuLeChange(index, 'tepDinhKem', e.target.value)
                                    }
                                    className="bg-white rounded-2"
                                  />
                                </div>
                                <div>
                                  <label className="btn btn-outline-primary btn-sm rounded-2 mb-0 cursor-pointer d-inline-flex align-items-center gap-1.5">
                                    {uploadingDieuLeIndex === index ? (
                                      <>
                                        <Spinner size="sm" />
                                        <span>Đang tải lên...</span>
                                      </>
                                    ) : (
                                      <>
                                        <i className="bi bi-paperclip"></i>
                                        <span>{item.tepDinhKem ? 'Đổi tệp khác' : 'Tải tệp lên'}</span>
                                      </>
                                    )}
                                    <input
                                      type="file"
                                      accept=".pdf,.doc,.docx,.xls,.xlsx,image/*"
                                      className="d-none"
                                      disabled={uploadingDieuLeIndex === index}
                                      onChange={(e) => handleDieuLeFileUpload(index, e)}
                                    />
                                  </label>
                                </div>
                                {item.tepDinhKem && (
                                  <>
                                    <a
                                      href={
                                        item.tepDinhKem.startsWith('http')
                                          ? item.tepDinhKem
                                          : `${process.env.NEXT_PUBLIC_API_URL || 'https://localhost:7085'}${item.tepDinhKem}`
                                      }
                                      target="_blank"
                                      rel="noreferrer"
                                      className="btn btn-light btn-sm rounded-2 text-primary border d-inline-flex align-items-center gap-1"
                                      title="Xem tệp đã tải lên"
                                    >
                                      <i className="bi bi-box-arrow-up-right"></i> Xem tệp
                                    </a>
                                    <Button
                                      size="sm"
                                      color="light"
                                      className="btn-icon text-danger rounded-2 py-1 px-2 border"
                                      title="Xóa tệp đính kèm"
                                      onClick={() => handleDieuLeChange(index, 'tepDinhKem', '')}
                                    >
                                      <i className="bi bi-x-lg"></i>
                                    </Button>
                                  </>
                                )}
                              </div>
                              <small className="text-muted" style={{ fontSize: '11px' }}>
                                Tệp sẽ được lưu vào thư mục <code>/dieu-le/</code> trên server. Hỗ trợ PDF, Word, Excel, Hình ảnh.
                              </small>
                            </FormGroup>
                          </Col>
                        </Row>
                      </div>
                    ))}
                  </div>
                )}
              </CardBody>
            </Card>
          </Col>

          {/* CỘT PHẢI: BANNER & THIẾT LẬP PHẠM VI & TRẠNG THÁI */}
          <Col lg={4}>
            {/* HÌNH ẢNH BANNER */}
            <Card className="border-0 shadow-sm rounded-4 mb-4">
              <CardBody className="p-4">
                <h5 className="fw-bold text-dark mb-3 d-flex align-items-center gap-2">
                  <i className="bi bi-image text-primary"></i> Banner Giải Đấu
                </h5>
                <p className="text-muted small mb-3">
                  Tải lên hình ảnh banner đại diện cho giải đấu (Lưu trữ trực tiếp trên hệ thống).
                </p>

                {formData.hinhAnh ? (
                  <div className="position-relative mb-3">
                    <div
                      className="rounded-3 overflow-hidden border shadow-sm"
                      style={{ height: '160px', backgroundColor: '#f8f9fa' }}
                    >
                      <img
                        src={
                          formData.hinhAnh.startsWith('http')
                            ? formData.hinhAnh
                            : `${process.env.NEXT_PUBLIC_API_URL || 'https://localhost:7085'}${formData.hinhAnh}`
                        }
                        alt="Banner Preview"
                        className="w-100 h-100 object-fit-cover"
                      />
                    </div>
                    <div className="mt-2 d-flex justify-content-between align-items-center">
                      <small className="text-muted font-monospace text-truncate" style={{ maxWidth: '200px' }}>
                        {formData.hinhAnh}
                      </small>
                      <Button
                        size="sm"
                        color="danger"
                        outline
                        className="rounded-pill px-2.5 py-1 text-xs"
                        onClick={() => setFormData((prev) => ({ ...prev, hinhAnh: '' }))}
                      >
                        <i className="bi bi-trash me-1"></i> Xóa banner
                      </Button>
                    </div>
                  </div>
                ) : (
                  <div className="text-center py-4 bg-light rounded-3 border border-dashed mb-3">
                    <i className="bi bi-cloud-arrow-up fs-2 text-primary d-block mb-1"></i>
                    <p className="text-muted small mb-0">Chưa có ảnh banner</p>
                    <small className="text-secondary" style={{ fontSize: '11px' }}>
                      (Định dạng: JPG, PNG, WEBP)
                    </small>
                  </div>
                )}

                <div className="d-grid">
                  <label className="btn btn-outline-primary rounded-3 btn-sm mb-0 cursor-pointer d-flex align-items-center justify-content-center gap-2">
                    {uploadingBanner ? (
                      <>
                        <Spinner size="sm" />
                        <span>Đang tải ảnh lên...</span>
                      </>
                    ) : (
                      <>
                        <i className="bi bi-upload"></i>
                        <span>{formData.hinhAnh ? 'Thay đổi Banner' : 'Tải lên Banner'}</span>
                      </>
                    )}
                    <input
                      type="file"
                      accept="image/*"
                      className="d-none"
                      disabled={uploadingBanner}
                      onChange={handleBannerUpload}
                    />
                  </label>
                </div>
              </CardBody>
            </Card>

            <Card className="border-0 shadow-sm rounded-4 mb-4">
              <CardBody className="p-4">
                <h5 className="fw-bold text-dark mb-3 d-flex align-items-center gap-2">
                  <i className="bi bi-sliders text-primary"></i> Phân Loại &amp; Trạng Thái
                </h5>

                <FormGroup className="mb-3">
                  <Label className="fw-semibold small">Trạng thái giải đấu</Label>
                  <Input
                    type="select"
                    value={formData.trangThai}
                    onChange={(e) =>
                      setFormData((prev) => ({
                        ...prev,
                        trangThai: Number(e.target.value) as TrangThaiGiaiDau,
                      }))
                    }
                    className="rounded-3"
                  >
                    <option value={TrangThaiGiaiDau.Nhap}>
                      {TrangThaiGiaiDauLabels[TrangThaiGiaiDau.Nhap]}
                    </option>
                    <option value={TrangThaiGiaiDau.SapDienRa}>
                      {TrangThaiGiaiDauLabels[TrangThaiGiaiDau.SapDienRa]}
                    </option>
                    <option value={TrangThaiGiaiDau.DangDienRa}>
                      {TrangThaiGiaiDauLabels[TrangThaiGiaiDau.DangDienRa]}
                    </option>
                    <option value={TrangThaiGiaiDau.KetThuc}>
                      {TrangThaiGiaiDauLabels[TrangThaiGiaiDau.KetThuc]}
                    </option>
                    <option value={TrangThaiGiaiDau.Huy}>
                      {TrangThaiGiaiDauLabels[TrangThaiGiaiDau.Huy]}
                    </option>
                  </Input>
                </FormGroup>

                <FormGroup className="mb-3">
                  <Label className="fw-semibold small">Phạm vi tham gia</Label>
                  <Input
                    type="select"
                    value={formData.phamVi}
                    onChange={(e) =>
                      setFormData((prev) => ({
                        ...prev,
                        phamVi: Number(e.target.value) as PhamViGiaiDau,
                      }))
                    }
                    className="rounded-3"
                  >
                    <option value={PhamViGiaiDau.TatCa}>
                      {PhamViGiaiDauLabels[PhamViGiaiDau.TatCa]}
                    </option>
                    <option value={PhamViGiaiDau.TheoKhoi}>
                      {PhamViGiaiDauLabels[PhamViGiaiDau.TheoKhoi]}
                    </option>
                  </Input>
                </FormGroup>

                {/* Chọn khối nếu phạm vi là TheoKhoi */}
                {formData.phamVi === PhamViGiaiDau.TheoKhoi && (
                  <div className="p-3 bg-light rounded-3 border mt-3">
                    <Label className="fw-semibold small mb-2 d-block">
                      Chọn các Khối tham gia:
                    </Label>
                    <div className="d-flex flex-column gap-2">
                      {availableKhois.map((k) => (
                        <div key={k.id} className="form-check">
                          <Input
                            type="checkbox"
                            id={`khoi_${k.id}`}
                            checked={(formData.khoiIds || []).includes(k.id)}
                            onChange={() => handleKhoiToggle(k.id)}
                          />
                          <Label check htmlFor={`khoi_${k.id}`} className="small">
                            {k.ten}
                          </Label>
                        </div>
                      ))}
                    </div>
                  </div>
                )}
              </CardBody>
            </Card>

            {/* CARD THỐNG KÊ NHANH */}
            <Card className="border-0 shadow-sm rounded-4 bg-primary text-white">
              <CardBody className="p-4">
                <h6 className="fw-bold mb-3 d-flex align-items-center gap-2">
                  <i className="bi bi-stars"></i> Tóm Tắt Thiết Lập
                </h6>
                <div className="d-flex justify-content-between py-2 border-bottom border-white border-opacity-25 small">
                  <span>Môn thể thao:</span>
                  <span className="fw-bold">{(formData.monTheThaoIds || []).length} môn</span>
                </div>
                {(formData.monTheThaoIds || []).length > 0 && (
                  <div className="py-2 border-bottom border-white border-opacity-25">
                    <div className="text-white-50 small mb-1" style={{ fontSize: '11.5px' }}>
                      Các môn đã chọn:
                    </div>
                    <div className="d-flex flex-column gap-1">
                      {(formData.monTheThaoIds || []).map((monId) => {
                        const mon = availableMons.find((m) => m.id === monId);
                        return (
                          <div
                            key={monId}
                            className="d-flex justify-content-between align-items-center small ps-2"
                            style={{ fontSize: '12px' }}
                          >
                            <span className="text-truncate text-white-75" style={{ maxWidth: '200px' }}>
                              • {mon?.ten || `Môn #${monId}`}
                            </span>
                          </div>
                        );
                      })}
                    </div>
                  </div>
                )}
                <div className="d-flex justify-content-between py-2 border-bottom border-white border-opacity-25 small">
                  <span>Mục điều lệ:</span>
                  <span className="fw-bold">{(formData.dieuLes || []).length} điều khoản</span>
                </div>
                <div className="d-flex justify-content-between py-2 small">
                  <span>Phạm vi áp dụng:</span>
                  <span className="fw-bold">
                    {formData.phamVi === PhamViGiaiDau.TheoKhoi
                      ? `${(formData.khoiIds || []).length} khối ngành`
                      : 'Tất cả đơn vị'}
                  </span>
                </div>
              </CardBody>
            </Card>
          </Col>
        </Row>
      </Form>
    </div>
  );
}
