import React from 'react';
import PublicHeader from '@/components/portal/PublicHeader';
import PublicFooter from '@/components/portal/PublicFooter';
import PublicMobileNav from '@/components/portal/PublicMobileNav';

export default function PublicLayout({ children }: { children: React.ReactNode }) {
  return (
    <div className="bg-light min-vh-100 d-flex flex-column font-sans position-relative">
      {/* 1. Header portal công khai đồng bộ */}
      <PublicHeader />

      {/* 2. Phần nội dung chính của các trang công khai */}
      <main className="flex-grow-1">{children}</main>

      {/* 3. Footer chung đồng bộ */}
      <PublicFooter />

      {/* 4. Bottom bar dành cho điện thoại */}
      <PublicMobileNav />
    </div>
  );
}
