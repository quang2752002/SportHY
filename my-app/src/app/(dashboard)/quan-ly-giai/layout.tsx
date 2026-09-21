'use client';

import React, { useState, useMemo } from 'react';
import Link from 'next/link';
import { usePathname } from 'next/navigation';
import { useAuth } from '@/context/AuthContext';
import { Permissions } from '@/constants/permissions';
import {
  Trophy,
  ChevronLeft,
  ChevronRight,
  ChevronDown,
  Menu,
  X,
  Home,
  UserCheck,
  LogOut,
} from 'lucide-react';

interface SubItem {
  title: string;
  href: string;
  icon: string;
  permission?: string;
}

interface NavItem {
  title: string;
  href?: string;
  icon: string;
  permission?: string;
  children?: SubItem[];
}

const managerNavigation: NavItem[] = [
  {
    title: 'Tổng quan Giải',
    href: '/quan-ly-giai',
    icon: 'bi bi-speedometer2',
  },
  {
    title: 'Điều Hành Thi Đấu',
    icon: 'bi bi-calendar-event',
    children: [
      {
        title: 'Lịch thi đấu & Bốc thăm',
        href: '/quan-ly-giai/lich-thi-dau',
        icon: 'bi bi-calendar3',
      },
      {
        title: 'Bảng thi đấu & Nhánh',
        href: '/quan-ly-giai/bang-dau',
        icon: 'bi bi-diagram-3',
      },
    ],
  },
  {
    title: 'Quản trị Thể thao',
    icon: 'bi bi-trophy',
    children: [
      {
        title: 'Giải đấu',
        href: '/quan-ly-giai/giai-dau',
        icon: 'bi bi-trophy-fill',
        permission: Permissions.GiaiDau.View,
      },
      {
        title: 'Khối tham gia',
        href: '/quan-ly-giai/khoi',
        icon: 'bi bi-diagram-2',
        permission: Permissions.Khoi.View,
      },
      {
        title: 'Đơn vị tham gia',
        href: '/quan-ly-giai/don-vi',
        icon: 'bi bi-building',
        permission: Permissions.DonVi.View,
      },
      {
        title: 'Vận động viên',
        href: '/quan-ly-giai/van-dong-vien',
        icon: 'bi bi-person-walking',
        permission: Permissions.VanDongVien.View,
      },
      {
        title: 'Danh mục môn',
        href: '/quan-ly-giai/danh-muc-mon-the-thao',
        icon: 'bi bi-tags',
        permission: Permissions.DanhMucMonTheThao.View,
      },
      {
        title: 'Môn thi đấu',
        href: '/quan-ly-giai/mon-the-thao',
        icon: 'bi bi-dribbble',
        permission: Permissions.MonTheThao.View,
      },
      {
        title: 'Trọng tài',
        href: '/quan-ly-giai/trong-tai',
        icon: 'bi bi-whistle',
        permission: Permissions.TrongTai.View,
      },
      {
        title: 'Thư ký giải đấu',
        href: '/quan-ly-giai/thu-ky',
        icon: 'bi bi-file-earmark-person',
      },
      {
        title: 'Cụm sân',
        href: '/quan-ly-giai/cum-san',
        icon: 'bi bi-geo-alt',
        permission: Permissions.SanDau.View,
      },
      {
        title: 'Sân đấu',
        href: '/quan-ly-giai/san-dau',
        icon: 'bi bi-grid-3x3-gap',
        permission: Permissions.SanDau.View,
      },
    ],
  },
];

export default function QuanLyGiaiLayout({ children }: { children: React.ReactNode }) {
  const pathname = usePathname();
  const { user, logout, hasPermission } = useAuth();
  const [collapsed, setCollapsed] = useState(false);
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);

  const isItemActive = (href?: string) => {
    if (!href) return false;
    if (href === '/quan-ly-giai') return pathname === '/quan-ly-giai';
    return pathname === href || pathname.startsWith(href + '/');
  };

  const filteredNav = useMemo(() => {
    const userRoles = (user?.roles || []).map((r) => String(r).toLowerCase());
    const isManagerOrAdmin = userRoles.includes('admin') || userRoles.includes('manager');

    const checkAllowed = (item: NavItem | SubItem) => {
      if (isManagerOrAdmin) return true;
      if (item.permission) return hasPermission(item.permission);
      return true;
    };

    return managerNavigation
      .map((item) => {
        if (!checkAllowed(item)) return null;
        if (item.children) {
          const validChildren = item.children.filter(checkAllowed);
          if (validChildren.length === 0) return null;
          return { ...item, children: validChildren };
        }
        return item;
      })
      .filter(Boolean) as NavItem[];
  }, [hasPermission, user]);

  // Quản lý đóng mở submenu
  const [openGroups, setOpenGroups] = useState<Record<number, boolean>>(() => {
    const initial: Record<number, boolean> = { 1: true, 2: true };
    filteredNav.forEach((item, idx) => {
      if (item.children?.some((child) => isItemActive(child.href))) {
        initial[idx] = true;
      }
    });
    return initial;
  });

  const toggleGroup = (idx: number) => {
    setOpenGroups((prev) => ({ ...prev, [idx]: !prev[idx] }));
  };

  return (
    <div className="donvi-layout-wrapper portal-theme-manager">
      {/* Mobile Backdrop */}
      {mobileMenuOpen && (
        <div className="donvi-backdrop d-lg-none" onClick={() => setMobileMenuOpen(false)} />
      )}

      {/* Sidebar Ban Tổ Chức */}
      <aside
        className={`donvi-sidebar ${collapsed ? 'collapsed' : ''} ${
          mobileMenuOpen ? 'mobile-open' : ''
        }`}
      >
        <div className="donvi-brand">
          <Link href="/quan-ly-giai" className="d-flex align-items-center gap-3 text-decoration-none text-white overflow-hidden">
            <div className="donvi-brand-logo">
              <Trophy size={22} />
            </div>
            {!collapsed && (
              <div className="lh-sm">
                <span className="fw-bold fs-6 text-white d-block">BAN TỔ CHỨC</span>
                <small className="text-info text-truncate d-block" style={{ maxWidth: '140px', fontSize: '11px' }}>
                  Quản lý điều hành giải
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
            {!collapsed ? 'Hệ Thống Ban Tổ Chức' : '•••'}
          </div>

          {filteredNav.map((item, idx) => {
            if (item.children) {
              const isOpen = !!openGroups[idx];
              const isChildActive = item.children.some((c) => isItemActive(c.href));

              return (
                <div key={idx} className="mb-1">
                  <div
                    onClick={() => toggleGroup(idx)}
                    role="button"
                    className={`donvi-nav-link justify-content-between ${
                      isChildActive ? 'bg-primary bg-opacity-25 text-white fw-semibold' : ''
                    }`}
                    title={collapsed ? item.title : undefined}
                    style={{ cursor: 'pointer' }}
                  >
                    <div className="d-flex align-items-center gap-2 overflow-hidden">
                      <i className={`${item.icon} fs-5 text-secondary`}></i>
                      {!collapsed && <span className="text-truncate">{item.title}</span>}
                    </div>
                    {!collapsed && (
                      <ChevronDown
                        size={14}
                        className="text-secondary transition-all"
                        style={{
                          transform: isOpen ? 'rotate(180deg)' : 'rotate(0deg)',
                          transition: 'transform 0.2s ease',
                        }}
                      />
                    )}
                  </div>

                  {!collapsed && isOpen && (
                    <div className="donvi-submenu">
                      {item.children.map((child, cIdx) => {
                        const isCurrent = isItemActive(child.href);
                        return (
                          <Link
                            key={cIdx}
                            href={child.href}
                            onClick={() => setMobileMenuOpen(false)}
                            className={`donvi-sub-link ${isCurrent ? 'active' : ''}`}
                          >
                            <i className={`${child.icon} fs-6 flex-shrink-0`}></i>
                            <span className="text-truncate">{child.title}</span>
                          </Link>
                        );
                      })}
                    </div>
                  )}
                </div>
              );
            }

            const active = isItemActive(item.href);
            return (
              <Link
                key={idx}
                href={item.href || '#'}
                onClick={() => setMobileMenuOpen(false)}
                className={`donvi-nav-link ${active ? 'active' : ''}`}
                title={collapsed ? item.title : undefined}
              >
                <i className={`${item.icon} fs-6 flex-shrink-0`}></i>
                {!collapsed && <span className="text-truncate">{item.title}</span>}
              </Link>
            );
          })}
        </div>

        <div className="donvi-user-footer">
          <div className={`d-flex align-items-center gap-2.5 overflow-hidden ${collapsed ? 'd-none' : ''}`}>
            <div
              className="rounded-circle bg-primary bg-opacity-25 text-primary d-flex align-items-center justify-content-center fw-bold"
              style={{ width: '34px', height: '34px', fontSize: '12px' }}
            >
              {(user?.username || 'BTC').slice(0, 2).toUpperCase()}
            </div>
            <div className="lh-1 overflow-hidden">
              <p className="text-light fw-semibold text-truncate mb-1" style={{ fontSize: '13px' }}>
                {user?.username}
              </p>
              <small className="text-secondary" style={{ fontSize: '11px' }}>
                Ban tổ chức
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

      {/* Main Content Area */}
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
                Phân Hệ Ban Tổ Chức &amp; Quản Lý Giải Đấu
              </span>
              <span className="d-none d-sm-inline text-muted">•</span>
              <span className="badge bg-primary-subtle text-primary border border-primary-subtle px-2.5 py-1 rounded-pill fw-semibold">
                {user?.fullName || 'Ban Tổ Chức'}
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
                className="rounded-circle bg-light border d-flex align-items-center justify-content-center text-primary"
                style={{ width: '34px', height: '34px' }}
              >
                <UserCheck size={18} />
              </div>
              <div className="d-none d-md-block text-start lh-1">
                <p className="fw-semibold text-dark mb-0" style={{ fontSize: '13px' }}>
                  {user?.fullName || user?.username}
                </p>
                <small className="text-muted" style={{ fontSize: '11px' }}>
                  Quản lý giải
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
