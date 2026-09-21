'use client';

import React, { useState, useEffect, useMemo } from 'react';
import { useRouter } from 'next/navigation';
import { useAuth, useToast } from '@/context/AuthContext';
import { giaiDauService } from '@/services/giaiDauService';
import { GiaiDau, TrangThaiGiaiDau, TrangThaiGiaiDauLabels } from '@/types/giaiDau';
import {
  Trophy,
  Calendar,
  MapPin,
  ChevronRight,
  Search,
  Filter,
  Layers,
  Clock,
  CheckCircle2,
  PlayCircle,
  AlertCircle,
  FileCheck2,
  Dumbbell,
  ArrowRight,
} from 'lucide-react';
import { Input, Spinner, Badge } from 'reactstrap';

// Màu sắc và icon cho từng trạng thái giải đấu
const trangThaiConfig: Record<
  number,
  { label: string; color: string; bg: string; border: string; icon: React.ReactNode }
> = {
  1: {
    label: 'Bản nháp',
    color: '#6c757d',
    bg: '#f8f9fa',
    border: '#dee2e6',
    icon: <AlertCircle size={13} />,
  },
  2: {
    label: 'Sắp diễn ra',
    color: '#0d6efd',
    bg: '#e7f0ff',
    border: '#b6d0ff',
    icon: <Clock size={13} />,
  },
  3: {
    label: 'Đang diễn ra',
    color: '#198754',
    bg: '#e6f4ea',
    border: '#a3d9b1',
    icon: <PlayCircle size={13} />,
  },
  4: {
    label: 'Đã kết thúc',
    color: '#6c757d',
    bg: '#f0f0f0',
    border: '#ccc',
    icon: <CheckCircle2 size={13} />,
  },
  5: {
    label: 'Đã hủy',
    color: '#dc3545',
    bg: '#fdecea',
    border: '#f5c6cb',
    icon: <AlertCircle size={13} />,
  },
};

const gradients = [
  'linear-gradient(135deg, #059669 0%, #047857 100%)',
  'linear-gradient(135deg, #2563eb 0%, #1d4ed8 100%)',
  'linear-gradient(135deg, #7c3aed 0%, #6d28d9 100%)',
  'linear-gradient(135deg, #dc2626 0%, #b91c1c 100%)',
  'linear-gradient(135deg, #d97706 0%, #b45309 100%)',
  'linear-gradient(135deg, #0891b2 0%, #0e7490 100%)',
];

function formatDate(dateStr?: string) {
  if (!dateStr) return '--';
  const d = new Date(dateStr);
  return d.toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' });
}

export default function ChonGiaiDauPage() {
  const { user } = useAuth();
  const toast = useToast();
  const router = useRouter();

  const [tournaments, setTournaments] = useState<GiaiDau[]>([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [filterStatus, setFilterStatus] = useState<string>('all');

  useEffect(() => {
    (async () => {
      try {
        setLoading(true);
        const data = await giaiDauService.getAll();
        setTournaments(data || []);
      } catch (err: any) {
        console.error('Lỗi tải danh sách giải đấu:', err);
        toast.error('Lỗi tải danh sách giải đấu: ' + (err?.response?.data?.message || err?.message || 'Lỗi server'));
      } finally {
        setLoading(false);
      }
    })();
  }, [toast]);

  // Lọc giải: ưu tiên hiển thị các giải đang/sắp diễn ra
  const filtered = useMemo(() => {
    return tournaments
      .filter((t) => {
        const matchSearch =
          search === '' ||
          t.ten.toLowerCase().includes(search.toLowerCase()) ||
          t.ma.toLowerCase().includes(search.toLowerCase()) ||
          (t.diaDiem || '').toLowerCase().includes(search.toLowerCase());
        const matchStatus =
          filterStatus === 'all' || t.trangThai.toString() === filterStatus;
        return matchSearch && matchStatus;
      })
      .sort((a, b) => {
        // Sắp xếp: đang diễn ra > sắp diễn ra > còn lại
        const order: Record<number, number> = { 3: 0, 2: 1, 4: 2, 1: 3, 5: 4 };
        return (order[a.trangThai] ?? 5) - (order[b.trangThai] ?? 5);
      });
  }, [tournaments, search, filterStatus]);

  const isExpired = (t: GiaiDau) => {
    const deadline = t.hanDangKy || t.ngayBatDau;
    if (!deadline) return false;
    return new Date() > new Date(deadline);
  };

  const canRegister = (t: GiaiDau) =>
    (t.trangThai === TrangThaiGiaiDau.SapDienRa || t.trangThai === TrangThaiGiaiDau.DangDienRa) && !isExpired(t);

  return (
    <div className="d-flex flex-column gap-4">
      {/* ── Header Banner ── */}
      <div
        className="rounded-4 p-4 p-md-5 text-white position-relative overflow-hidden shadow-sm"
        style={{ background: 'linear-gradient(135deg, #059669 0%, #047857 50%, #064e3b 100%)' }}
      >
        {/* background icon */}
        <div
          className="position-absolute end-0 top-0 bottom-0 d-none d-md-flex align-items-center justify-content-end pe-5 opacity-10"
          style={{ pointerEvents: 'none' }}
        >
          <Trophy size={220} />
        </div>

        <div className="position-relative" style={{ zIndex: 2 }}>
          <div
            className="d-inline-flex align-items-center gap-2 px-3 py-1 rounded-pill mb-3 border"
            style={{
              backgroundColor: 'rgba(255,255,255,0.15)',
              borderColor: 'rgba(255,255,255,0.25)',
              fontSize: '12px',
            }}
          >
            <FileCheck2 size={15} className="text-warning" />
            <span className="fw-semibold text-white">Cổng Đăng Ký Thi Đấu Trực Tuyến</span>
          </div>

          <h2 className="fw-bold mb-1 fs-3">
            Chọn Giải Đấu Để Đăng Ký
          </h2>
          <p className="text-white-50 mb-0 small" style={{ maxWidth: '620px' }}>
            Chọn một giải đấu bên dưới để xem danh sách vận động viên và đăng ký tham gia thi đấu.
            <br />
            Đơn vị: <strong className="text-white">{user?.fullName || 'Đoàn Thể Thao'}</strong>
          </p>
        </div>
      </div>

      {/* ── Bộ lọc & tìm kiếm ── */}
      <div className="d-flex flex-column flex-sm-row gap-3 align-items-stretch align-items-sm-center">
        {/* Search */}
        <div className="position-relative flex-grow-1" style={{ maxWidth: '400px' }}>
          <Search
            size={16}
            className="position-absolute top-50 translate-middle-y ms-3 text-secondary"
            style={{ left: 0, zIndex: 2 }}
          />
          <Input
            type="text"
            placeholder="Tìm kiếm tên giải, địa điểm..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            className="ps-5 rounded-pill border-secondary-subtle"
          />
        </div>

        {/* Filter trạng thái */}
        <div className="d-flex align-items-center gap-2" style={{ minWidth: '220px' }}>
          <Filter size={15} className="text-secondary flex-shrink-0" />
          <Input
            type="select"
            value={filterStatus}
            onChange={(e) => setFilterStatus(e.target.value)}
            className="rounded-pill border-secondary-subtle"
          >
            <option value="all">Tất cả trạng thái</option>
            <option value="2">Sắp diễn ra</option>
            <option value="3">Đang diễn ra</option>
            <option value="4">Đã kết thúc</option>
            <option value="1">Bản nháp</option>
            <option value="5">Đã hủy</option>
          </Input>
        </div>
      </div>

      {/* ── Nội dung chính ── */}
      {loading ? (
        <div className="d-flex justify-content-center align-items-center py-5">
          <Spinner color="success" />
          <span className="ms-3 text-secondary">Đang tải danh sách giải đấu...</span>
        </div>
      ) : filtered.length === 0 ? (
        <div className="text-center py-5">
          <Trophy size={56} className="text-muted mb-3 opacity-30" />
          <h5 className="text-secondary fw-semibold">Không tìm thấy giải đấu phù hợp</h5>
          <p className="text-muted small mb-0">Thử thay đổi từ khóa hoặc bộ lọc trạng thái.</p>
        </div>
      ) : (
        <>
          <p className="text-secondary small mb-0">
            Hiển thị <strong>{filtered.length}</strong> giải đấu
          </p>
          <div className="row g-4">
            {filtered.map((t, idx) => {
              const cfg = trangThaiConfig[t.trangThai] ?? trangThaiConfig[1];
              const gradient = gradients[idx % gradients.length];
              const monCount = t.monTheThaos?.length ?? t.monTheThaoIds?.length ?? 0;
              const registrable = canRegister(t);
              const apiBase = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000';

              const expired = isExpired(t);
              const canEnter = registrable || expired;

              return (
                <div className="col-12 col-md-6 col-xl-4" key={t.id}>
                  <div
                    className="card border-0 shadow-sm h-100 rounded-4 overflow-hidden"
                    style={{
                      transition: 'transform 0.22s ease, box-shadow 0.22s ease',
                      cursor: canEnter ? 'pointer' : 'default',
                    }}
                    onMouseEnter={(e) => {
                      if (canEnter) {
                        (e.currentTarget as HTMLElement).style.transform = 'translateY(-4px)';
                        (e.currentTarget as HTMLElement).style.boxShadow =
                          '0 12px 32px rgba(0,0,0,0.12)';
                      }
                    }}
                    onMouseLeave={(e) => {
                      (e.currentTarget as HTMLElement).style.transform = 'translateY(0)';
                      (e.currentTarget as HTMLElement).style.boxShadow = '';
                    }}
                    onClick={() => {
                      if (canEnter) router.push(`/don-vi/dang-ky/${t.id}`);
                    }}
                  >
                    {/* Banner image hoặc gradient placeholder */}
                    <div
                      className="position-relative overflow-hidden"
                      style={{ height: '140px' }}
                    >
                      {t.hinhAnh ? (
                        // eslint-disable-next-line @next/next/no-img-element
                        <img
                          src={`${apiBase}${t.hinhAnh}`}
                          alt={t.ten}
                          style={{
                            width: '100%',
                            height: '100%',
                            objectFit: 'cover',
                          }}
                          onError={(e) => {
                            (e.currentTarget as HTMLImageElement).style.display = 'none';
                            (e.currentTarget.parentElement as HTMLElement).style.background =
                              gradient;
                          }}
                        />
                      ) : (
                        <div
                          className="w-100 h-100 d-flex align-items-center justify-content-center"
                          style={{ background: gradient }}
                        >
                          <Trophy size={52} color="rgba(255,255,255,0.35)" />
                        </div>
                      )}

                      {/* Badge trạng thái */}
                      <div className="position-absolute top-0 start-0 m-2">
                        <span
                          className="badge rounded-pill px-2 py-1 d-flex align-items-center gap-1 fw-semibold"
                          style={{
                            backgroundColor: cfg.bg,
                            color: cfg.color,
                            border: `1px solid ${cfg.border}`,
                            fontSize: '11px',
                            backdropFilter: 'blur(4px)',
                          }}
                        >
                          {cfg.icon}
                          {cfg.label}
                        </span>
                      </div>

                      {/* Badge mã giải */}
                      <div className="position-absolute top-0 end-0 m-2">
                        <span
                          className="badge bg-dark bg-opacity-60 text-white rounded-pill px-2 py-1"
                          style={{ fontSize: '10px' }}
                        >
                          {t.ma}
                        </span>
                      </div>
                    </div>

                    {/* Card body */}
                    <div className="card-body p-3 d-flex flex-column gap-2">
                      <h6
                        className="fw-bold text-dark mb-0 lh-sm"
                        style={{
                          display: '-webkit-box',
                          WebkitLineClamp: 2,
                          WebkitBoxOrient: 'vertical',
                          overflow: 'hidden',
                        }}
                      >
                        {t.ten}
                      </h6>

                      {/* Ngày diễn ra */}
                      <div className="d-flex align-items-center gap-1 text-secondary small">
                        <Calendar size={13} />
                        <span>
                          {formatDate(t.ngayBatDau)} – {formatDate(t.ngayKetThuc)}
                        </span>
                      </div>

                      {/* Hạn chót đăng ký */}
                      <div className={`d-flex align-items-center gap-1 small ${expired ? 'text-danger fw-semibold' : 'text-secondary'}`}>
                        <Clock size={13} />
                        <span>
                          Hạn ĐK: {formatDate(t.hanDangKy || t.ngayBatDau)}
                          {expired && ' (Đã hết hạn)'}
                        </span>
                      </div>

                      {/* Địa điểm */}
                      {t.diaDiem && (
                        <div className="d-flex align-items-center gap-1 text-secondary small">
                          <MapPin size={13} />
                          <span className="text-truncate">{t.diaDiem}</span>
                        </div>
                      )}

                      {/* Số môn */}
                      {monCount > 0 && (
                        <div className="d-flex align-items-center gap-1 text-secondary small">
                          <Dumbbell size={13} />
                          <span>{monCount} môn thi đấu</span>
                        </div>
                      )}
                    </div>

                    {/* Card footer – CTA */}
                    <div className="px-3 pb-3">
                      {expired ? (
                        <button
                          className="btn btn-outline-secondary btn-sm w-100 rounded-3 d-flex align-items-center justify-content-center gap-2 fw-medium"
                          style={{ fontSize: '13px' }}
                          onClick={(e) => {
                            e.stopPropagation();
                            router.push(`/don-vi/dang-ky/${t.id}`);
                          }}
                        >
                          <FileCheck2 size={15} />
                          Đã hết hạn (Xem hồ sơ)
                          <ChevronRight size={14} />
                        </button>
                      ) : registrable ? (
                        <button
                          className="btn btn-success btn-sm w-100 rounded-3 d-flex align-items-center justify-content-center gap-2 fw-semibold"
                          style={{ fontSize: '13px' }}
                          onClick={(e) => {
                            e.stopPropagation();
                            router.push(`/don-vi/dang-ky/${t.id}`);
                          }}
                        >
                          <FileCheck2 size={15} />
                          Đăng ký thi đấu
                          <ArrowRight size={14} />
                        </button>
                      ) : (
                        <button
                          className="btn btn-light btn-sm w-100 rounded-3 text-secondary border"
                          style={{ fontSize: '12px', cursor: 'not-allowed' }}
                          disabled
                        >
                          {t.trangThai === TrangThaiGiaiDau.KetThuc
                            ? 'Giải đấu đã kết thúc'
                            : t.trangThai === TrangThaiGiaiDau.Huy
                              ? 'Giải đấu đã hủy'
                              : 'Chưa mở đăng ký'}
                        </button>
                      )}
                    </div>
                  </div>
                </div>
              );
            })}
          </div>
        </>
      )}
    </div>
  );
}
