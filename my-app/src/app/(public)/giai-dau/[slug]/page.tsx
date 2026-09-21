import { Metadata } from 'next';
import Link from 'next/link';
import { notFound } from 'next/navigation';
import { giaiDauService } from '@/services/giaiDauService';
import {
  PhamViGiaiDau,
  PhamViGiaiDauLabels,
  TrangThaiGiaiDau,
  TrangThaiGiaiDauLabels,
} from '@/types/giaiDau';

interface Props {
  params: Promise<{ slug: string }>;
}

// 1. Tự động sinh Meta Tags chuẩn SEO (Title, Description, OpenGraph) theo tên giải đấu
export async function generateMetadata({ params }: Props): Promise<Metadata> {
  const { slug } = await params;
  try {
    const giaiDau = await giaiDauService.getBySlug(slug);
    if (!giaiDau) {
      return {
        title: 'Không tìm thấy giải đấu - Hệ Thống Thể Thao',
        description: 'Thông tin giải đấu thể thao không tồn tại hoặc đã bị xóa.',
      };
    }

    const title = `${giaiDau.ten} | Thông Tin & Điều Lệ Giải Đấu`;
    const description =
      giaiDau.moTa ||
      `Theo dõi thông tin chi tiết giải đấu ${giaiDau.ten}, thời gian tổ chức từ ${new Date(
        giaiDau.ngayBatDau
      ).toLocaleDateString('vi-VN')} đến ${new Date(
        giaiDau.ngayKetThuc
      ).toLocaleDateString('vi-VN')} tại ${giaiDau.diaDiem || 'Địa điểm cập nhật sau'}.`;

    return {
      title,
      description,
      openGraph: {
        title,
        description,
        type: 'website',
        url: `/giai-dau/${slug}`,
      },
      twitter: {
        card: 'summary_large_image',
        title,
        description,
      },
    };
  } catch {
    return {
      title: 'Giải Đấu Thể Thao',
      description: 'Cổng thông tin giải đấu thể thao',
    };
  }
}

// 2. Giao diện trang hiển thị công khai cho người dùng
export default async function PublicGiaiDauDetailPage({ params }: Props) {
  const { slug } = await params;
  let giaiDau = null;

  try {
    giaiDau = await giaiDauService.getBySlug(slug);
  } catch (e) {
    console.error('Lỗi nạp giải đấu theo slug:', e);
  }

  if (!giaiDau) {
    notFound();
  }

  const ngayBatDauStr = giaiDau.ngayBatDau
    ? new Date(giaiDau.ngayBatDau).toLocaleDateString('vi-VN', {
        day: '2-digit',
        month: '2-digit',
        year: 'numeric',
      })
    : 'Chưa xác định';

  const ngayKetThucStr = giaiDau.ngayKetThuc
    ? new Date(giaiDau.ngayKetThuc).toLocaleDateString('vi-VN', {
        day: '2-digit',
        month: '2-digit',
        year: 'numeric',
      })
    : 'Chưa xác định';

  return (
    <div className="py-4 py-md-5">
      <div className="container" style={{ maxWidth: '1080px' }}>
        {/* Nút quay lại */}
        <div className="mb-4 d-flex align-items-center gap-2">
          <Link
            href="/"
            className="btn btn-outline-secondary btn-sm rounded-pill px-3 d-inline-flex align-items-center gap-2"
          >
            <i className="bi bi-house-door"></i> Trang chủ
          </Link>
          <Link
            href="/admin/giai-dau"
            className="btn btn-light btn-sm rounded-pill px-3 text-muted d-inline-flex align-items-center gap-2 border"
          >
            <i className="bi bi-arrow-left"></i> Quản trị giải đấu
          </Link>
        </div>

        {/* Hero Card thông tin giải đấu */}
        <div className="card border-0 shadow-sm rounded-4 overflow-hidden mb-4 position-relative">
          {/* Background image overlay nếu có banner */}
          {giaiDau.hinhAnh && (
            <div
              className="position-absolute top-0 start-0 w-100 h-100"
              style={{
                backgroundImage: `url(${
                  giaiDau.hinhAnh.startsWith('http')
                    ? giaiDau.hinhAnh
                    : `${process.env.NEXT_PUBLIC_API_URL || 'https://localhost:7015'}${
                        giaiDau.hinhAnh.startsWith('/') ? '' : '/'
                      }${giaiDau.hinhAnh}`
                })`,
                backgroundSize: 'cover',
                backgroundPosition: 'center',
                filter: 'brightness(0.35)',
                zIndex: 0,
              }}
            />
          )}

          <div
            className="p-4 p-md-5 text-white position-relative"
            style={{
              zIndex: 1,
              background: giaiDau.hinhAnh
                ? 'linear-gradient(180deg, rgba(15, 23, 42, 0.5) 0%, rgba(15, 23, 42, 0.9) 100%)'
                : 'linear-gradient(135deg, #1e3c72 0%, #2a5298 100%)',
            }}
          >
            <div className="d-flex flex-wrap align-items-center gap-2 mb-3">
              <span className="badge bg-white text-dark font-monospace px-3 py-1.5 rounded-pill">
                Mã: {giaiDau.ma}
              </span>
              <span className="badge bg-warning text-dark px-3 py-1.5 rounded-pill fw-semibold">
                {giaiDau.trangThaiText ||
                  TrangThaiGiaiDauLabels[giaiDau.trangThai] ||
                  'Đang cập nhật'}
              </span>
              <span className="badge bg-info-subtle text-info-emphasis px-3 py-1.5 rounded-pill">
                {giaiDau.phamViText ||
                  PhamViGiaiDauLabels[giaiDau.phamVi] ||
                  'Tất cả đơn vị'}
              </span>
            </div>

            <h1 className="fw-bold display-6 mb-3">{giaiDau.ten}</h1>

            {giaiDau.moTa && (
              <p className="lead opacity-90 mb-4" style={{ maxWidth: '850px' }}>
                {giaiDau.moTa}
              </p>
            )}

            <div className="row g-3 pt-3 border-top border-white border-opacity-25">
              <div className="col-md-4">
                <div className="d-flex align-items-center gap-2 text-white-50 small">
                  <i className="bi bi-calendar-event fs-5 text-white"></i>
                  <div>
                    <div className="text-white fw-bold">
                      {ngayBatDauStr} - {ngayKetThucStr}
                    </div>
                    <span>Thời gian tổ chức</span>
                  </div>
                </div>
              </div>
              <div className="col-md-8">
                <div className="d-flex align-items-center gap-2 text-white-50 small">
                  <i className="bi bi-geo-alt fs-5 text-white"></i>
                  <div>
                    <div className="text-white fw-bold">
                      {giaiDau.diaDiem || 'Địa điểm đang được ban tổ chức bố trí'}
                    </div>
                    <span>Địa điểm thi đấu</span>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <div className="row g-4">
          {/* Cột chính (Trái - 8 cột): Mô tả chi tiết & Toàn bộ Điều lệ giải đấu */}
          <div className="col-lg-8">
            {/* 1. Phần Mô tả chi tiết giải đấu */}
            <div className="card border-0 shadow-sm rounded-4 p-4 p-md-4 mb-4 bg-white">
              <div className="d-flex align-items-center gap-2 mb-3 pb-2 border-bottom">
                <div
                  className="rounded-3 p-2 d-flex align-items-center justify-content-center"
                  style={{ backgroundColor: '#fff7ed', color: '#ea580c' }}
                >
                  <i className="bi bi-card-text fs-5"></i>
                </div>
                <div>
                  <h5 className="fw-bold text-dark mb-0">Giới Thiệu &amp; Mô Tả Giải Đấu</h5>
                  <span className="text-muted small">Thông tin tổng quan về mục đích, đối tượng và quy mô giải</span>
                </div>
              </div>

              {giaiDau.moTa?.trim() ? (
                <div
                  className="text-secondary ps-1"
                  style={{
                    whiteSpace: 'pre-line',
                    lineHeight: '1.8',
                    fontSize: '0.98rem',
                  }}
                >
                  {giaiDau.moTa}
                </div>
              ) : (
                <div className="text-muted small py-4 px-3 bg-light rounded-3 text-center border">
                  <i className="bi bi-info-circle fs-4 text-secondary d-block mb-1"></i>
                  Chưa có thông tin mô tả chi tiết cho giải đấu này.
                </div>
              )}
            </div>

            {/* 2. Phần Điều Lệ & Thể Lệ Giải Đấu kèm Tệp Đính Kèm */}
            <div className="card border-0 shadow-sm rounded-4 p-4 p-md-4 mb-4 bg-white">
              <div className="d-flex flex-wrap justify-content-between align-items-center gap-2 mb-4 pb-2 border-bottom">
                <div className="d-flex align-items-center gap-2">
                  <div
                    className="rounded-3 p-2 d-flex align-items-center justify-content-center"
                    style={{ backgroundColor: '#eff6ff', color: '#2563eb' }}
                  >
                    <i className="bi bi-file-earmark-ruled-fill fs-5"></i>
                  </div>
                  <div>
                    <h5 className="fw-bold text-dark mb-0">Điều Lệ &amp; Thể Lệ Thi Đấu</h5>
                    <span className="text-muted small">Quy chế, đối tượng, thể thức và điều khoản của giải</span>
                  </div>
                </div>

                {giaiDau.dieuLeGiaiDaus && giaiDau.dieuLeGiaiDaus.length > 0 && (
                  <span
                    className="badge rounded-pill px-3 py-1.5 fw-semibold"
                    style={{ backgroundColor: '#fff7ed', color: '#c2410c', fontSize: '0.82rem' }}
                  >
                    {giaiDau.dieuLeGiaiDaus.length} điều lệ
                  </span>
                )}
              </div>

              {(!giaiDau.dieuLeGiaiDaus || giaiDau.dieuLeGiaiDaus.length === 0) ? (
                <div className="text-center py-5 text-muted bg-light rounded-3 border">
                  <i className="bi bi-journal-text fs-1 text-secondary d-block mb-2"></i>
                  Điều lệ giải đấu đang được ban tổ chức biên soạn và sẽ sớm công bố.
                </div>
              ) : (
                <div className="d-flex flex-column gap-3">
                  {giaiDau.dieuLeGiaiDaus.map((dl, idx) => (
                    <div
                      key={dl.id || idx}
                      className="p-3.5 rounded-3 border transition-hover"
                      style={{ backgroundColor: '#fafbfc' }}
                    >
                      <div className="d-flex align-items-center gap-2 mb-2">
                        <span
                          className="badge rounded-circle d-inline-flex align-items-center justify-content-center fw-bold flex-shrink-0"
                          style={{
                            width: '26px',
                            height: '26px',
                            backgroundColor: '#ea580c',
                            color: '#ffffff',
                            fontSize: '0.8rem',
                          }}
                        >
                          {dl.thuTu || idx + 1}
                        </span>
                        <h6 className="fw-bold text-dark mb-0" style={{ fontSize: '1rem' }}>
                          {dl.tieuDe}
                        </h6>
                      </div>

                      {dl.noiDung && (
                        <div
                          className="text-secondary small mb-3 ps-4"
                          style={{ whiteSpace: 'pre-line', lineHeight: '1.7', fontSize: '0.92rem' }}
                        >
                          {dl.noiDung}
                        </div>
                      )}

                      {/* Tệp điều lệ đính kèm */}
                      {dl.tepDinhKem && (
                        <div className="ps-4 mt-2">
                          <div className="p-3 bg-white border rounded-3 d-flex flex-wrap align-items-center justify-content-between gap-3 shadow-xs">
                            <div className="d-flex align-items-center gap-2.5 min-w-0">
                              <div
                                className="rounded-2 p-2 d-flex align-items-center justify-content-center"
                                style={{ backgroundColor: '#fef2f2', color: '#dc2626' }}
                              >
                                <i className="bi bi-file-earmark-pdf-fill fs-5"></i>
                              </div>
                              <div className="text-truncate">
                                <span className="fw-semibold text-dark small d-block text-truncate">
                                  {dl.tepDinhKem.split('/').pop() || 'Tệp văn bản điều lệ chính thức'}
                                </span>
                                <span className="text-muted" style={{ fontSize: '0.74rem' }}>
                                  Văn bản / Tài liệu chính thức ban hành (PDF / Word)
                                </span>
                              </div>
                            </div>
                            <div className="d-flex align-items-center gap-2 flex-shrink-0">
                              <a
                                href={
                                  dl.tepDinhKem.startsWith('http')
                                    ? dl.tepDinhKem
                                    : `${process.env.NEXT_PUBLIC_API_URL || 'https://localhost:7015'}${
                                        dl.tepDinhKem.startsWith('/') ? '' : '/'
                                      }${dl.tepDinhKem}`
                                }
                                target="_blank"
                                rel="noopener noreferrer"
                                className="btn btn-sm text-white rounded-pill px-3.5 py-1.5 fw-semibold d-inline-flex align-items-center gap-1.5 shadow-sm"
                                style={{ fontSize: '0.82rem', backgroundColor: '#ea580c', borderColor: '#ea580c' }}
                              >
                                <i className="bi bi-download"></i>
                                <span>Tải tệp điều lệ</span>
                              </a>
                            </div>
                          </div>
                        </div>
                      )}
                    </div>
                  ))}
                </div>
              )}
            </div>
          </div>

          {/* Cột bên phải (Phụ - 4 cột): Tổng quan giải đấu & Danh sách Môn thi đấu */}
          <div className="col-lg-4">
            {/* 1. Khối Thông Tin Tổng Quan Giải */}
            <div className="card border-0 shadow-sm rounded-4 p-4 mb-4 bg-white">
              <div className="d-flex align-items-center gap-2 mb-3 pb-2 border-bottom">
                <i className="bi bi-info-square-fill text-warning fs-5"></i>
                <h5 className="fw-bold text-dark mb-0">Thông Tin Giải Đấu</h5>
              </div>

              <div className="d-flex flex-column gap-2.5 small">
                <div className="d-flex justify-content-between align-items-center py-1 border-bottom border-light-subtle">
                  <span className="text-muted">Mã giải:</span>
                  <span className="fw-bold font-monospace badge bg-light text-dark border px-2 py-1">
                    {giaiDau.ma}
                  </span>
                </div>

                <div className="d-flex justify-content-between align-items-center py-1 border-bottom border-light-subtle">
                  <span className="text-muted">Trạng thái:</span>
                  <span className="badge bg-warning text-dark px-2.5 py-1 rounded-pill fw-semibold">
                    {giaiDau.trangThaiText || 'Đang cập nhật'}
                  </span>
                </div>

                <div className="d-flex justify-content-between align-items-center py-1 border-bottom border-light-subtle">
                  <span className="text-muted">Phạm vi:</span>
                  <span className="fw-semibold text-dark">
                    {giaiDau.phamViText || 'Tất cả đơn vị'}
                  </span>
                </div>

                <div className="d-flex justify-content-between align-items-center py-1 border-bottom border-light-subtle">
                  <span className="text-muted">Thời gian bắt đầu:</span>
                  <span className="fw-semibold text-success">{ngayBatDauStr}</span>
                </div>

                <div className="d-flex justify-content-between align-items-center py-1 border-bottom border-light-subtle">
                  <span className="text-muted">Thời gian kết thúc:</span>
                  <span className="fw-semibold text-danger">{ngayKetThucStr}</span>
                </div>

                <div className="d-flex justify-content-between align-items-start py-1 border-bottom border-light-subtle">
                  <span className="text-muted flex-shrink-0 me-2">Địa điểm:</span>
                  <span className="fw-semibold text-dark text-end text-break">
                    {giaiDau.diaDiem || 'Đang cập nhật'}
                  </span>
                </div>

                <div className="d-flex justify-content-between align-items-center py-1">
                  <span className="text-muted">Số môn tổ chức:</span>
                  <span className="fw-bold badge bg-primary text-white rounded-pill px-2.5 py-1">
                    {giaiDau.monTheThaos?.length || giaiDau.monTheThaoIds?.length || 0} môn
                  </span>
                </div>
              </div>
            </div>

            {/* 2. Khối Danh Sách Môn Thi Đấu Trong Giải */}
            <div className="card border-0 shadow-sm rounded-4 p-4 mb-4 bg-white">
              <div className="d-flex justify-content-between align-items-center mb-3 pb-2 border-bottom">
                <div className="d-flex align-items-center gap-2">
                  <i className="bi bi-trophy-fill text-warning fs-5"></i>
                  <h5 className="fw-bold text-dark mb-0">Môn Thi Đấu</h5>
                </div>
                <span className="badge rounded-pill bg-light text-secondary border px-2.5 py-1 fw-medium">
                  {giaiDau.monTheThaos?.length || 0} môn
                </span>
              </div>

              {(!giaiDau.monTheThaos || giaiDau.monTheThaos.length === 0) ? (
                <div className="text-center py-4 text-muted small bg-light rounded-3 border">
                  Chưa có môn thi đấu nào được cấu hình cho giải.
                </div>
              ) : (
                <div className="d-flex flex-column gap-2">
                  {giaiDau.monTheThaos.map((m, i) => (
                    <div
                      key={m.id || m.monTheThaoId}
                      className="p-2.5 rounded-3 bg-light border d-flex align-items-center justify-content-between gap-2 transition-hover"
                    >
                      <div className="d-flex align-items-center gap-2 min-w-0">
                        <span
                          className="badge rounded-circle bg-secondary bg-opacity-25 text-dark fw-bold d-inline-flex align-items-center justify-content-center flex-shrink-0"
                          style={{ width: '22px', height: '22px', fontSize: '0.72rem' }}
                        >
                          {i + 1}
                        </span>
                        <div className="text-truncate">
                          <span className="fw-semibold text-dark small d-block text-truncate">
                            {m.ten}
                          </span>
                          {m.tenDanhMuc && (
                            <span className="text-muted d-block text-truncate" style={{ fontSize: '0.72rem' }}>
                              <i className="bi bi-tag me-1"></i>
                              {m.tenDanhMuc}
                            </span>
                          )}
                        </div>
                      </div>
                      <span
                        className="badge rounded-pill px-2 py-1 fw-medium flex-shrink-0"
                        style={{
                          backgroundColor: m.laMonDongDoi ? '#eff6ff' : '#f5f3ff',
                          color: m.laMonDongDoi ? '#1d4ed8' : '#6d28d9',
                          fontSize: '0.7rem',
                        }}
                      >
                        {m.laMonDongDoi ? 'Đồng đội' : 'Cá nhân'}
                      </span>
                    </div>
                  ))}
                </div>
              )}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
