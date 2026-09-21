'use client';

import React from 'react';
import Link from 'next/link';
import { usePathname } from 'next/navigation';
import { useAuth } from '@/context/AuthContext';
import { Button } from 'reactstrap';

export default function PublicHeader() {
  const { user, isAuthenticated, logout } = useAuth();
  const pathname = usePathname();

  const isNavActive = (href: string) => {
    if (href === '/') {
      return pathname === '/' || pathname.startsWith('/giai-dau');
    }
    return pathname.startsWith(href);
  };

  return (
    <header className="portal-header sticky-top py-2 px-3 shadow-sm">
      <div className="container-fluid px-lg-4">
        <div className="d-flex align-items-center justify-content-between">
          {/* Logo Thương Hiệu */}
          <div className="d-flex align-items-center gap-3">
            <Link href="/" className="d-flex align-items-center text-decoration-none">
              <div
                className="rounded-circle d-flex align-items-center justify-content-center shadow-sm overflow-hidden"
                style={{
                  width: '46px',
                  height: '46px',
                  background: 'radial-gradient(circle, #f97316 0%, #1e3a8a 100%)',
                  border: '2px solid #ea580c',
                }}
              >
                <span
                  className="text-white fw-bold"
                  style={{ fontSize: '11px', textAlign: 'center', lineHeight: 1.1 }}
                >
                  HƯNG YÊN<br />PICK
                </span>
              </div>
            </Link>

            {/* Desktop Menu Navigation Links */}
            <nav className="d-none d-md-flex align-items-center gap-1 ms-2">
              <Link
                href="/"
                className={`nav-link-custom ${isNavActive('/') ? 'active' : ''}`}
              >
                <i className="bi bi-trophy"></i>
                <span>Giải đấu</span>
              </Link>
              <Link
                href="/diem-trinh"
                className={`nav-link-custom ${isNavActive('/diem-trinh') ? 'active' : ''}`}
              >
                <i className="bi bi-people"></i>
                <span>Điểm trình</span>
              </Link>
              <Link
                href="/cau-lac-bo"
                className={`nav-link-custom ${isNavActive('/cau-lac-bo') ? 'active' : ''}`}
              >
                <i className="bi bi-building"></i>
                <span>Câu lạc bộ</span>
              </Link>
              <Link
                href="/lich-thi-dau"
                className={`nav-link-custom ${isNavActive('/lich-thi-dau') ? 'active' : ''}`}
              >
                <i className="bi bi-calendar3"></i>
                <span>Lịch thi đấu</span>
              </Link>
              <Link
                href="/ty-so-truc-tiep"
                className={`nav-link-custom ${isNavActive('/ty-so-truc-tiep') ? 'active' : ''}`}
              >
                <i className="bi bi-activity"></i>
                <span>Tỷ số trực tiếp</span>
              </Link>
              <Link
                href="/xem-truc-tiep"
                className={`nav-link-custom ${isNavActive('/xem-truc-tiep') ? 'active' : ''}`}
              >
                <i className="bi bi-broadcast"></i>
                <span>Xem trực tiếp</span>
              </Link>
            </nav>
          </div>

          {/* User Auth Buttons */}
          <div className="d-flex align-items-center gap-2">
            {isAuthenticated ? (
              <div className="d-flex align-items-center gap-2">
                <Link
                  href="/admin"
                  className="btn btn-sm btn-light border rounded-pill px-3 py-1.5 fw-semibold d-flex align-items-center gap-1.5 text-dark"
                >
                  <i className="bi bi-person-circle text-primary"></i>
                  <span className="d-none d-sm-inline">
                    {user?.fullName || user?.username}
                  </span>
                </Link>
                <Button
                  color="outline-secondary"
                  size="sm"
                  className="rounded-pill px-3"
                  onClick={() => logout()}
                  title="Đăng xuất"
                >
                  <i className="bi bi-box-arrow-right"></i>
                </Button>
              </div>
            ) : (
              <div className="d-flex align-items-center gap-2">
                <Link
                  href="/register"
                  className="btn btn-outline-secondary btn-sm rounded-3 px-3 py-1.5 fw-medium d-flex align-items-center gap-1"
                  style={{ fontSize: '0.85rem' }}
                >
                  <i className="bi bi-person-plus"></i>
                  <span>Đăng ký</span>
                </Link>
                <Link
                  href="/login"
                  className="btn btn-sm rounded-3 px-3 py-1.5 fw-semibold text-white d-flex align-items-center gap-1"
                  style={{
                    backgroundColor: '#ea580c',
                    borderColor: '#ea580c',
                    fontSize: '0.85rem',
                  }}
                >
                  <i className="bi bi-box-arrow-in-right"></i>
                  <span>Đăng nhập</span>
                </Link>
              </div>
            )}
          </div>
        </div>
      </div>
    </header>
  );
}
