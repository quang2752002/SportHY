'use client';

import React from 'react';
import Link from 'next/link';
import { usePathname } from 'next/navigation';

export default function PublicMobileNav() {
  const pathname = usePathname();

  const isNavActive = (href: string) => {
    if (href === '/') {
      return pathname === '/' || pathname.startsWith('/giai-dau');
    }
    return pathname.startsWith(href);
  };

  return (
    <div className="mobile-bottom-nav d-flex d-md-none">
      <Link
        href="/"
        className={`mobile-nav-item ${isNavActive('/') ? 'active' : ''}`}
      >
        <i className="bi bi-trophy"></i>
        <span>Giải đấu</span>
      </Link>

      <Link
        href="/diem-trinh"
        className={`mobile-nav-item ${isNavActive('/diem-trinh') ? 'active' : ''}`}
      >
        <i className="bi bi-people"></i>
        <span>Điểm Trình</span>
      </Link>

      <Link
        href="/cau-lac-bo"
        className={`mobile-nav-item ${isNavActive('/cau-lac-bo') ? 'active' : ''}`}
      >
        <i className="bi bi-building"></i>
        <span>CLB</span>
      </Link>

      <Link
        href="/lich-thi-dau"
        className={`mobile-nav-item ${isNavActive('/lich-thi-dau') ? 'active' : ''}`}
      >
        <i className="bi bi-calendar3"></i>
        <span>Lịch đấu</span>
      </Link>

      <Link
        href="/ty-so-truc-tiep"
        className={`mobile-nav-item ${isNavActive('/ty-so-truc-tiep') ? 'active' : ''}`}
      >
        <i className="bi bi-activity"></i>
        <span>Live</span>
      </Link>

      <Link
        href="/them"
        className={`mobile-nav-item ${isNavActive('/them') ? 'active' : ''}`}
      >
        <i className="bi bi-three-dots"></i>
        <span>Thêm</span>
      </Link>
    </div>
  );
}
