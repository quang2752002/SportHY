'use client';

import React, { useState } from 'react';
import Link from 'next/link';
import { usePathname } from 'next/navigation';
import { useAuth } from '@/context/AuthContext';
import {
  Whistle,
  Calendar,
  CheckCircle2,
  Trophy,
  LogOut,
  ChevronLeft,
  ChevronRight,
  Menu,
  X,
  Home,
  UserCheck,
} from 'lucide-react';

const refereeNavItems = [
  {
    title: 'Nhiệm vụ Phân công',
    href: '/trong-tai',
    icon: Calendar,
  },
  {
    title: 'Ghi nhận Kết quả & Thẻ',
    href: '/trong-tai/ket-qua',
    icon: CheckCircle2,
  },
  {
    title: 'Biên bản Trận đấu',
    href: '/trong-tai/bien-ban',
    icon: Trophy,
  },
];

export default function TrongTaiLayout({ children }: { children: React.ReactNode }) {
  const pathname = usePathname();
  const { user, logout } = useAuth();
  const [collapsed, setCollapsed] = useState(false);
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);

  const isActive = (href: string) => {
    if (href === '/trong-tai') return pathname === '/trong-tai';
    return pathname.startsWith(href);
  };

  return (
    <div className="donvi-layout-wrapper portal-theme-referee">
      {mobileMenuOpen && (
        <div className="donvi-backdrop d-lg-none" onClick={() => setMobileMenuOpen(false)} />
      )}

      {/* Sidebar Trọng Tài */}
      <aside
        className={`donvi-sidebar ${collapsed ? 'collapsed' : ''} ${
          mobileMenuOpen ? 'mobile-open' : ''
        }`}
      >
        <div className="donvi-brand">
          <Link href="/trong-tai" className="d-flex align-items-center gap-3 text-decoration-none text-white overflow-hidden">
            <div className="donvi-brand-logo">
              <Calendar size={22} />
            </div>
            {!collapsed && (
              <div className="lh-sm">
                <span className="fw-bold fs-6 text-white d-block">TRỌNG TÀI</span>
                <small className="text-success text-truncate d-block" style={{ maxWidth: '140px', fontSize: '11px' }}>
                  Điều hành thi đấu
                </small>
              </div>
            )}
          </Link>

          <button
            type="button"
            onClick={() => setCollapsed(!collapsed)}
            className="d-none d-lg-flex btn btn-sm btn-dark p-1 rounded-2 text-secondary border-0"
            title={collapsed ? 'Mở rộng' : 'Thu gọn'}
            style={{ width: '28px', height: '28px' }}
          >
            {collapsed ? <ChevronRight size={16} /> : <ChevronLeft size={16} />}
          </button>

          <button
            type="button"
            onClick={() => setMobileMenuOpen(false)}
            className="d-lg-none btn btn-sm btn-dark p-1 text-secondary"
          >
            <X size={18} />
          </button>
        </div>

        <div className="donvi-nav">
          <div className="donvi-nav-header">
            {!collapsed ? 'Nhiệm Vụ Điều Hành' : '•••'}
          </div>

          {refereeNavItems.map((item) => {
            const Icon = item.icon;
            const active = isActive(item.href);
            return (
              <Link
                key={item.href}
                href={item.href}
                onClick={() => setMobileMenuOpen(false)}
                className={`donvi-nav-link ${active ? 'active' : ''}`}
                title={collapsed ? item.title : undefined}
              >
                <Icon size={20} className="flex-shrink-0" />
                {!collapsed && <span className="text-truncate">{item.title}</span>}
              </Link>
            );
          })}
        </div>

        <div className="donvi-user-footer">
          <div className={`d-flex align-items-center gap-2.5 overflow-hidden ${collapsed ? 'd-none' : ''}`}>
            <div
              className="rounded-circle bg-success bg-opacity-25 text-success d-flex align-items-center justify-content-center fw-bold"
              style={{ width: '34px', height: '34px', fontSize: '12px' }}
            >
              {(user?.username || 'TT').slice(0, 2).toUpperCase()}
            </div>
            <div className="lh-1 overflow-hidden">
              <p className="text-light fw-semibold text-truncate mb-1" style={{ fontSize: '13px' }}>
                {user?.username}
              </p>
              <small className="text-secondary" style={{ fontSize: '11px' }}>
                Trọng tài giải
              </small>
            </div>
          </div>

          <button
            type="button"
            onClick={() => logout()}
            className="btn btn-sm btn-dark text-danger border-0 p-1.5 rounded-2 ms-auto"
            title="Đăng xuất"
          >
            <LogOut size={16} />
          </button>
        </div>
      </aside>

      {/* Main Content */}
      <div className="donvi-main-area">
        <header className="donvi-topbar">
          <div className="d-flex align-items-center gap-3">
            <button
              type="button"
              onClick={() => setMobileMenuOpen(true)}
              className="d-lg-none btn btn-light btn-sm p-1.5 rounded-2 border"
            >
              <Menu size={20} />
            </button>
            <div className="d-flex align-items-center gap-2">
              <span className="d-none d-sm-inline fw-semibold text-dark" style={{ fontSize: '13px' }}>
                Phân Hệ Trọng Tài Điều Hành
              </span>
              <span className="d-none d-sm-inline text-muted">•</span>
              <span className="badge bg-success-subtle text-success border border-success-subtle px-2.5 py-1 rounded-pill fw-semibold">
                {user?.fullName || 'Trọng Tài'}
              </span>
            </div>
          </div>

          <div className="d-flex align-items-center gap-3">
            <Link
              href="/"
              className="btn btn-outline-secondary btn-sm rounded-pill px-3 py-1.5 d-flex align-items-center gap-1.5 text-decoration-none"
              style={{ fontSize: '12px' }}
            >
              <Home size={14} />
              <span className="d-none d-md-inline">Trang Chủ Giải</span>
            </Link>

            <div className="vr d-none d-sm-block my-1" />

            <div className="d-flex align-items-center gap-2">
              <div
                className="rounded-circle bg-light border d-flex align-items-center justify-content-center text-success"
                style={{ width: '34px', height: '34px' }}
              >
                <UserCheck size={18} />
              </div>
              <div className="d-none d-md-block text-start lh-1">
                <p className="fw-semibold text-dark mb-0" style={{ fontSize: '13px' }}>
                  {user?.fullName || user?.username}
                </p>
                <small className="text-muted" style={{ fontSize: '11px' }}>
                  Tài khoản trọng tài
                </small>
              </div>
            </div>
          </div>
        </header>

        <main className="donvi-content-container">
          <div className="container-fluid p-0" style={{ maxWidth: '1400px' }}>
            {children}
          </div>
        </main>
      </div>
    </div>
  );
}
