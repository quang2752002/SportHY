'use client';

import Link from 'next/link';
import { useAuth } from '../../context/AuthContext';
import { ShieldAlert, ArrowLeft, Home } from 'lucide-react';

export default function UnauthorizedPage() {
  const { user } = useAuth();

  return (
    <div className="min-h-screen flex items-center justify-center bg-slate-950 p-4 font-sans text-slate-100">
      <div className="max-w-md w-full bg-slate-900 border border-slate-800 rounded-3xl p-8 shadow-2xl text-center">
        <div className="w-16 h-16 bg-red-500/10 border border-red-500/20 text-red-400 rounded-2xl flex items-center justify-center mx-auto mb-5 shadow-lg shadow-red-500/10">
          <ShieldAlert className="w-8 h-8" />
        </div>

        <span className="px-3 py-1 rounded-full text-[11px] font-bold tracking-wider uppercase bg-red-500/10 text-red-400 border border-red-500/30">
          Lỗi 403 - Forbidden
        </span>

        <h1 className="text-2xl font-bold text-white mt-3 mb-2">
          Không Có Quyền Truy Cập
        </h1>

        <p className="text-xs text-slate-400 mb-6 leading-relaxed">
          Tài khoản của bạn (<span className="text-slate-200 font-semibold">{user?.username}</span> - Vai trò:{' '}
          <span className="text-blue-400 font-semibold">{user?.roles.join(', ') || 'Chưa xác định'}</span>) không có quyền hạn cần thiết để truy cập vào phân hệ này.
        </p>

        <div className="flex gap-3 justify-center text-xs font-medium">
          <Link
            href="/"
            className="px-4 py-2.5 rounded-xl bg-slate-800 hover:bg-slate-700 text-slate-200 border border-slate-700 transition flex items-center gap-1.5"
          >
            <Home className="w-4 h-4" />
            <span>Trang Chủ</span>
          </Link>
          <Link
            href="/login"
            className="px-4 py-2.5 rounded-xl bg-blue-600 hover:bg-blue-500 text-white shadow-lg shadow-blue-500/20 transition flex items-center gap-1.5"
          >
            <ArrowLeft className="w-4 h-4" />
            <span>Đổi Tài Khoản Khác</span>
          </Link>
        </div>
      </div>
    </div>
  );
}
