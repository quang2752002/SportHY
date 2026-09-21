'use client';

import React, { useEffect, useState, useMemo } from 'react';
import Link from 'next/link';
import Image from 'next/image';
import { giaiDauService } from '@/services/giaiDauService';
import { GiaiDau, TrangThaiGiaiDau } from '@/types/giaiDau';
import {
  Container,
  Row,
  Col,
  Button,
  Spinner,
  Badge,
} from 'reactstrap';

export default function Home() {
  // Danh sách giải đấu lấy từ API hoặc fallback mẫu
  const [tournaments, setTournaments] = useState<GiaiDau[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [activeTab, setActiveTab] = useState<'tatCa' | 'sapDienRa' | 'dangDienRa' | 'daKetThuc'>('tatCa');
  const [searchQuery, setSearchQuery] = useState<string>('');

  // Load danh sách giải đấu
  const fetchTournaments = async () => {
    setLoading(true);
    try {
      // Gọi service giải đấu
      const res = await giaiDauService.getPaged({ pageIndex: 1, pageSize: 50 });
      if (res && res.items && res.items.length > 0) {
        setTournaments(res.items);
      } else {
        setTournaments([]);
      }
    } catch (err) {
      console.warn('Không thể kết nối API giải đấu, sử dụng dữ liệu hiển thị mẫu:', err);
    } finally {
      setLoading(false);
    }
  };



  useEffect(() => {
    fetchTournaments();
  }, []);

  // Tính số lượng cho từng tab
  const counts = useMemo(() => {
    let sapDienRaCount = 0;
    let dangDienRaCount = 0;
    let daKetThucCount = 0;

    tournaments.forEach((t) => {
      if (t.trangThai === TrangThaiGiaiDau.SapDienRa) sapDienRaCount++;
      else if (t.trangThai === TrangThaiGiaiDau.DangDienRa) dangDienRaCount++;
      else if (t.trangThai === TrangThaiGiaiDau.KetThuc) daKetThucCount++;
      else sapDienRaCount++; // Mặc định vào sắp diễn ra nếu là nháp
    });

    return {
      tatCa: tournaments.length,
      sapDienRa: sapDienRaCount,
      dangDienRa: dangDienRaCount,
      daKetThuc: daKetThucCount,
    };
  }, [tournaments]);

  // Lọc giải đấu theo tab và từ khoá tìm kiếm
  const filteredTournaments = useMemo(() => {
    return tournaments.filter((t) => {
      // Lọc trạng thái
      let matchStatus = true;
      if (activeTab === 'sapDienRa') {
        matchStatus = t.trangThai === TrangThaiGiaiDau.SapDienRa || t.trangThai === TrangThaiGiaiDau.Nhap;
      } else if (activeTab === 'dangDienRa') {
        matchStatus = t.trangThai === TrangThaiGiaiDau.DangDienRa;
      } else if (activeTab === 'daKetThuc') {
        matchStatus = t.trangThai === TrangThaiGiaiDau.KetThuc;
      }

      // Lọc keyword
      const matchKeyword = searchQuery.trim() === '' ||
        t.ten.toLowerCase().includes(searchQuery.toLowerCase()) ||
        (t.diaDiem && t.diaDiem.toLowerCase().includes(searchQuery.toLowerCase())) ||
        (t.ma && t.ma.toLowerCase().includes(searchQuery.toLowerCase()));

      return matchStatus && matchKeyword;
    });
  }, [tournaments, activeTab, searchQuery]);

  const formatDate = (dateStr?: string) => {
    if (!dateStr) return '';
    try {
      const d = new Date(dateStr);
      if (isNaN(d.getTime())) return dateStr;
      const day = String(d.getDate()).padStart(2, '0');
      const month = String(d.getMonth() + 1).padStart(2, '0');
      const year = d.getFullYear();
      return `${day}/${month}/${year}`;
    } catch {
      return dateStr;
    }
  };

  return (
    <div className="py-3 py-md-4">
      <Container fluid="lg">
        {/* Box Header Thanh Điều Hướng & Tìm kiếm */}
        <div className="bg-white rounded-4 border p-3 p-md-4 shadow-sm mb-4">
          <div className="d-flex flex-column flex-md-row justify-content-between align-items-md-center gap-3">
            {/* Tiêu đề & Bộ Tab trạng thái */}
            <div className="d-flex flex-column flex-sm-row align-items-sm-center gap-3">
              <h4 className="fw-bold mb-0 text-dark me-2">Giải đấu</h4>

              <div className="d-flex align-items-center gap-2 flex-wrap">
                {/* Tab Tất cả */}
                <button
                  type="button"
                  onClick={() => setActiveTab('tatCa')}
                  className={`status-pill-btn ${activeTab === 'tatCa' ? 'active-orange' : ''}`}
                >
                  <i className="bi bi-grid"></i>
                  <span>Tất cả</span>
                  <span className="badge-pill-count">{counts.tatCa}</span>
                </button>

                {/* Tab Sắp diễn ra */}
                <button
                  type="button"
                  onClick={() => setActiveTab('sapDienRa')}
                  className={`status-pill-btn ${activeTab === 'sapDienRa' ? 'active-orange' : ''}`}
                >
                  <i className="bi bi-calendar-event"></i>
                  <span>Sắp diễn ra</span>
                  <span className="badge-pill-count">{counts.sapDienRa}</span>
                </button>

                {/* Tab Đang diễn ra */}
                <button
                  type="button"
                  onClick={() => setActiveTab('dangDienRa')}
                  className={`status-pill-btn ${activeTab === 'dangDienRa' ? 'active-orange' : ''}`}
                >
                  <i className="bi bi-trophy"></i>
                  <span>Đang diễn ra</span>
                  <span className="badge-pill-count">{counts.dangDienRa}</span>
                </button>

                {/* Tab Đã kết thúc */}
                <button
                  type="button"
                  onClick={() => setActiveTab('daKetThuc')}
                  className={`status-pill-btn ${activeTab === 'daKetThuc' ? 'active-orange' : ''}`}
                >
                  <i className="bi bi-check2-circle"></i>
                  <span>Đã kết thúc</span>
                  <span className="badge-pill-count">{counts.daKetThuc}</span>
                </button>
              </div>
            </div>

            {/* Ô Tìm Kiếm & Nút Tải lại */}
            <div className="d-flex align-items-center gap-2">
              <div className="search-box-wrap flex-grow-1 flex-md-grow-0" style={{ minWidth: '220px' }}>
                <i className="bi bi-search search-icon-pos"></i>
                <input
                  type="text"
                  className="form-control search-box-input"
                  placeholder="Tìm giải đấu..."
                  value={searchQuery}
                  onChange={(e) => setSearchQuery(e.target.value)}
                />
              </div>
              <button
                type="button"
                className="btn btn-outline-secondary border d-flex align-items-center justify-content-center"
                style={{ width: '40px', height: '40px', borderRadius: '0.6rem' }}
                onClick={fetchTournaments}
                title="Tải lại danh sách"
              >
                <i className="bi bi-arrow-clockwise"></i>
              </button>
            </div>
          </div>
        </div>

        {/* 3. Danh sách Giải Đấu */}
        {loading ? (
          <div className="text-center py-5">
            <Spinner color="warning" />
            <p className="text-muted mt-2 small">Đang tải danh sách giải đấu...</p>
          </div>
        ) : filteredTournaments.length === 0 ? (
          <div className="bg-white rounded-4 border p-5 text-center shadow-sm">
            <i className="bi bi-calendar-x text-muted" style={{ fontSize: '3rem' }}></i>
            <h5 className="fw-bold mt-3 text-secondary">Không có giải đấu nào phù hợp</h5>
            <p className="text-muted small">Hãy thử tìm kiếm với từ khóa khác hoặc chuyển sang tab trạng thái khác.</p>
            <Button
              color="warning"
              outline
              size="sm"
              className="rounded-pill px-3"
              onClick={() => {
                setSearchQuery('');
                setActiveTab('sapDienRa');
              }}
            >
              Xem tất cả giải đấu
            </Button>
          </div>
        ) : (
          <div className="d-flex flex-column gap-4">
            {filteredTournaments.map((tournament) => (
              <TournamentCard
                key={tournament.id}
                tournament={tournament}
                formatDate={formatDate}
              />
            ))}
          </div>
        )}
      </Container>
    </div>
  );
}

// Sub-component hiển thị Card giải đấu với danh sách môn thi đấu phân trang 4 môn / trang
function TournamentCard({
  tournament,
  formatDate,
}: {
  tournament: GiaiDau;
  formatDate: (dateStr?: string) => string;
}) {
  const sports = tournament.monTheThaos || [];
  const pageSize = 4;
  const [currentPage, setCurrentPage] = useState(1);

  const totalPages = Math.ceil(sports.length / pageSize) || 1;
  const startIndex = (currentPage - 1) * pageSize;
  const currentSports = sports.slice(startIndex, startIndex + pageSize);

  // Chuyển trang an toàn
  const handlePageChange = (newPage: number) => {
    if (newPage >= 1 && newPage <= totalPages) {
      setCurrentPage(newPage);
    }
  };

  return (
    <div className="tournament-card overflow-hidden">
      <Row className="g-0">
        {/* Cột Trái: Banner Poster Giải Đấu - Bấm vào sang trang chi tiết theo slug */}
        <Col lg={4} className="p-3 p-md-4">
          <Link
            href={tournament.slug ? `/giai-dau/${tournament.slug}` : `/giai-dau/${tournament.id}`}
            className="d-block text-decoration-none h-100 banner-click-hover"
            title={`Xem chi tiết giải đấu ${tournament.ten}`}
          >
            <div
              className="tournament-banner-container h-100 rounded-4 overflow-hidden position-relative shadow-sm d-flex flex-column justify-content-between"
              style={{
                minHeight: '340px',
                background: tournament.hinhAnh
                  ? '#0f172a'
                  : 'linear-gradient(145deg, #0f172a 0%, #1e293b 50%, #0369a1 100%)',
                cursor: 'pointer',
              }}
            >
              {tournament.hinhAnh ? (
                <>
                  <img
                    src={
                      tournament.hinhAnh.startsWith('http')
                        ? tournament.hinhAnh
                        : `${process.env.NEXT_PUBLIC_API_URL || 'https://localhost:7015'}${tournament.hinhAnh.startsWith('/') ? '' : '/'
                        }${tournament.hinhAnh}`
                    }
                    alt={tournament.ten}
                    className="w-100 h-100 position-absolute top-0 start-0"
                    style={{ objectFit: 'cover', objectPosition: 'center' }}
                  />
                  <div
                    className="position-absolute top-0 start-0 w-100 h-100"
                    style={{
                      background: 'linear-gradient(180deg, rgba(0,0,0,0.3) 0%, rgba(0,0,0,0.1) 40%, rgba(0,0,0,0.85) 100%)',
                    }}
                  />
                  <div className="position-relative z-1 p-3 d-flex justify-content-between align-items-center">
                    <span className="badge bg-warning text-dark fw-bold px-2.5 py-1.5 shadow-sm">
                      {tournament.ma}
                    </span>
                    <span className="badge bg-danger px-2.5 py-1.5 shadow-sm">
                      {tournament.trangThaiText}
                    </span>
                  </div>
                  <div className="position-relative z-1 p-3 text-white mt-auto">
                    <div className="badge bg-primary bg-opacity-75 text-white mb-2 small">
                      {tournament.phamViText || 'Giải Đấu Thể Thao'}
                    </div>
                    <h5 className="fw-bold text-white text-uppercase mb-1 text-shadow line-clamp-2">
                      {tournament.ten}
                    </h5>
                    <div className="small text-light text-opacity-90 d-flex align-items-center gap-1">
                      <i className="bi bi-geo-alt-fill text-warning"></i>
                      <span className="text-truncate">{tournament.diaDiem || 'Địa điểm đang cập nhật'}</span>
                    </div>
                  </div>
                </>
              ) : (
                <div className="d-flex flex-column justify-content-between h-100 p-4 text-white">
                  {/* Banner Header */}
                  <div className="d-flex justify-content-between align-items-center border-bottom border-secondary border-opacity-25 pb-3">
                    <div className="d-flex align-items-center gap-2">
                      <span className="badge bg-warning text-dark fw-bold px-2 py-1">{tournament.ma}</span>
                      <span className="small text-light text-opacity-75">{tournament.phamViText || 'Giải Đấu'}</span>
                    </div>
                    <span className="badge bg-danger px-2 py-1 small">{tournament.trangThaiText}</span>
                  </div>

                  {/* Banner Center Title */}
                  <div className="py-4 text-center my-auto">
                    <div className="text-uppercase text-warning fw-bold small tracking-widest mb-1">
                      {tournament.phamViText || 'HỆ THỐNG THỂ THAO'}
                    </div>
                    <h4 className="fw-black text-white text-uppercase mb-2" style={{ letterSpacing: '0.5px' }}>
                      {tournament.ten}
                    </h4>
                    <div className="small text-light text-opacity-80">
                      <i className="bi bi-geo-alt-fill text-warning me-1"></i>
                      {tournament.diaDiem || 'Địa điểm đang cập nhật'}
                    </div>
                  </div>

                  {/* Banner Footer Info */}
                  <div className="pt-3 border-top border-secondary border-opacity-25 d-flex justify-content-between align-items-center small text-light text-opacity-75">
                    <div>
                      <i className="bi bi-clock me-1 text-warning"></i>
                      {formatDate(tournament.ngayBatDau)}
                    </div>
                    <div className="text-warning fw-semibold">
                      {tournament.ngayKetThuc ? `Đến ${formatDate(tournament.ngayKetThuc)}` : ''}
                    </div>
                  </div>
                </div>
              )}
            </div>
          </Link>
        </Col>

        {/* Cột Phải: Thông tin chi tiết & Danh sách Môn thi đấu / Nội dung */}
        <Col lg={8} className="p-3 p-md-4 d-flex flex-column justify-content-between">
          <div>
            {/* Tiêu đề giải và Trạng thái */}
            <div className="d-flex flex-wrap align-items-start justify-content-between gap-2 mb-2">
              <h4 className="fw-bold text-dark mb-0 text-uppercase flex-grow-1" style={{ fontSize: '1.25rem' }}>
                {tournament.ten}
              </h4>
              <span
                className="badge rounded-pill px-3 py-1.5 fw-semibold"
                style={{ backgroundColor: '#e0f2fe', color: '#0369a1', fontSize: '0.78rem' }}
              >
                {tournament.trangThaiText || 'Đang mở đăng ký'}
              </span>
            </div>

            {/* Thông tin Địa điểm, Thời gian, Hạn ĐK */}
            <div className="d-flex flex-wrap align-items-center gap-3 text-secondary small mb-3">
              <div className="d-flex align-items-center gap-1 text-primary">
                <i className="bi bi-geo-alt"></i>
                <span>{tournament.diaDiem || 'Địa điểm đang cập nhật'}</span>
              </div>
              <div className="d-flex align-items-center gap-1 text-success">
                <i className="bi bi-calendar-check"></i>
                <span>
                  {formatDate(tournament.ngayBatDau)} – {formatDate(tournament.ngayKetThuc)}
                </span>
              </div>
              <div className="d-flex align-items-center gap-1 text-danger fw-medium">
                <i className="bi bi-clock-history"></i>
                <span>Hạn ĐK: {formatDate(tournament.ngayBatDau)}</span>
              </div>
            </div>

            {/* Danh sách Môn / Nội Dung Thi Đấu của giải */}
            <div className="mt-3">
              <div className="d-flex justify-content-between align-items-center mb-2">
                <div className="fw-bold text-secondary text-uppercase small" style={{ letterSpacing: '0.5px' }}>
                  MÔN THI ĐẤU ({sports.length})
                </div>
              </div>

              {sports.length === 0 ? (
                <div className="text-muted small py-3 px-3 bg-light rounded-3 text-center border">
                  Chưa có môn thi đấu nào được gán cho giải đấu này.
                </div>
              ) : (
                <>
                  <div className="d-flex flex-column gap-2.5">
                    {currentSports.map((mon, idx) => (
                      <div
                        key={mon.id || mon.monTheThaoId}
                        className="category-item d-flex flex-column flex-sm-row justify-content-between align-items-sm-center gap-2 py-2.5 px-3"
                      >
                        <div className="d-flex align-items-center gap-2.5 flex-grow-1 min-w-0">
                          <div className="category-index-badge flex-shrink-0">
                            {startIndex + idx + 1}
                          </div>
                          <div className="text-truncate">
                            <span className="fw-bold text-dark d-block text-truncate" style={{ fontSize: '0.95rem' }}>
                              {mon.ten}
                            </span>
                            {mon.moTa && (
                              <span className="text-muted small d-block text-truncate" style={{ fontSize: '0.78rem' }}>
                                {mon.moTa}
                              </span>
                            )}
                          </div>
                        </div>

                        <div className="d-flex flex-wrap align-items-center gap-2 flex-shrink-0">
                          {/* Danh mục môn thể thao */}
                          {mon.tenDanhMuc && (
                            <span
                              className="badge rounded-pill px-2.5 py-1.5 fw-medium d-flex align-items-center gap-1"
                              style={{ backgroundColor: '#f1f5f9', color: '#334155', border: '1px solid #e2e8f0' }}
                            >
                              <i className="bi bi-tag text-muted"></i>
                              <span>{mon.tenDanhMuc}</span>
                            </span>
                          )}

                          {/* Huy hiệu Đồng đội / Cá nhân */}
                          <span
                            className="badge rounded-pill px-2.5 py-1.5 fw-medium d-flex align-items-center gap-1"
                            style={{
                              backgroundColor: mon.laMonDongDoi ? '#eff6ff' : '#f5f3ff',
                              color: mon.laMonDongDoi ? '#1d4ed8' : '#6d28d9',
                            }}
                          >
                            <i className={mon.laMonDongDoi ? 'bi bi-people-fill' : 'bi bi-person-fill'}></i>
                            {mon.laMonDongDoi ? 'Đồng đội' : 'Cá nhân'}
                          </span>
                        </div>
                      </div>
                    ))}
                  </div>

                  {/* Phân trang đặt ở phía dưới danh sách môn thi đấu */}
                  {totalPages > 1 && (
                    <div className="d-flex justify-content-between align-items-center pt-3 mt-3 border-top border-light-subtle">
                      <span className="small text-muted" style={{ fontSize: '0.78rem' }}>
                        Hiển thị {startIndex + 1} - {Math.min(startIndex + pageSize, sports.length)} trong tổng số {sports.length} môn
                      </span>

                      <div className="d-flex align-items-center gap-1">
                        <button
                          type="button"
                          disabled={currentPage === 1}
                          onClick={() => handlePageChange(currentPage - 1)}
                          className="btn btn-sm btn-outline-secondary p-0 d-inline-flex align-items-center justify-content-center rounded-circle"
                          style={{ width: '26px', height: '26px', fontSize: '0.75rem' }}
                          title="Trang trước"
                        >
                          <i className="bi bi-chevron-left"></i>
                        </button>
                        {Array.from({ length: totalPages }).map((_, i) => {
                          const p = i + 1;
                          return (
                            <button
                              key={p}
                              type="button"
                              onClick={() => handlePageChange(p)}
                              className={`btn btn-sm rounded-circle p-0 d-inline-flex align-items-center justify-content-center ${p === currentPage ? 'btn-warning text-white fw-bold' : 'btn-light border text-secondary'
                                }`}
                              style={{
                                width: '26px',
                                height: '26px',
                                fontSize: '0.75rem',
                                backgroundColor: p === currentPage ? '#ea580c' : undefined,
                                borderColor: p === currentPage ? '#ea580c' : undefined,
                              }}
                            >
                              {p}
                            </button>
                          );
                        })}
                        <button
                          type="button"
                          disabled={currentPage === totalPages}
                          onClick={() => handlePageChange(currentPage + 1)}
                          className="btn btn-sm btn-outline-secondary p-0 d-inline-flex align-items-center justify-content-center rounded-circle"
                          style={{ width: '26px', height: '26px', fontSize: '0.75rem' }}
                          title="Trang sau"
                        >
                          <i className="bi bi-chevron-right"></i>
                        </button>
                      </div>
                    </div>
                  )}
                </>
              )}
            </div>
          </div>

          {/* Footer Card: Xem chi tiết điều lệ giải đấu */}
          <div className="mt-3 pt-3 border-top d-flex align-items-center justify-content-end">
            <Link
              href={tournament.slug ? `/giai-dau/${tournament.slug}` : `/giai-dau/${tournament.id}`}
              className="btn btn-outline-warning rounded-3 px-3.5 py-2 fw-semibold d-inline-flex align-items-center gap-1.5 text-decoration-none shadow-sm"
              style={{ borderColor: '#ea580c', color: '#ea580c', fontSize: '0.88rem' }}
            >
              <span>Xem chi tiết điều lệ giải đấu</span>
              <i className="bi bi-arrow-right"></i>
            </Link>
          </div>
        </Col>
      </Row>
    </div>
  );
}
