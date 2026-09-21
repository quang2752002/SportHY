'use client';

import React, { useEffect, useState } from 'react';
import { Row, Col, Card, CardBody, CardTitle, Table, Badge, Button, Spinner } from 'reactstrap';
import Link from 'next/link';
import { giaiDauService } from '@/services';
import { GiaiDau } from '@/types';

export default function AdminDashboardPage() {
  const [loading, setLoading] = useState(true);
  const [giaiDaus, setGiaiDaus] = useState<GiaiDau[]>([]);
  const [stats, setStats] = useState({
    totalGiaiDau: 0,
  });

  useEffect(() => {
    const fetchData = async () => {
      try {
        setLoading(true);
        const paged = await giaiDauService.getPaged({ pageIndex: 1, pageSize: 5 });
        setGiaiDaus(paged.items || []);
        setStats({
          totalGiaiDau: paged.totalCount || 0,
        });
      } catch (err) {
        console.error('Lỗi khi tải dữ liệu tổng quan admin:', err);
      } finally {
        setLoading(false);
      }
    };

    fetchData();
  }, []);

  return (
    <div className="d-flex flex-column gap-4">
      {/* Banner Chào mừng Quản trị viên */}
      <div
        className="rounded-4 p-4 p-md-5 text-white position-relative overflow-hidden shadow-sm"
        style={{
          background: 'linear-gradient(135deg, #1e40af 0%, #1e3a8a 50%, #0f172a 100%)',
        }}
      >
        <div
          className="position-absolute end-0 top-0 bottom-0 d-none d-md-flex align-items-center justify-content-end pe-5 opacity-10"
          style={{ pointerEvents: 'none' }}
        >
          <i className="bi bi-shield-shaded" style={{ fontSize: '240px' }}></i>
        </div>

        <div className="position-relative" style={{ zIndex: 2 }}>
          <div
            className="d-inline-flex align-items-center gap-2 px-3 py-1 rounded-pill mb-3 border"
            style={{ backgroundColor: 'rgba(255, 255, 255, 0.15)', borderColor: 'rgba(255, 255, 255, 0.2)', fontSize: '12px' }}
          >
            <i className="bi bi-shield-check text-warning"></i>
            <span className="fw-semibold text-white">Bảng Điều Khiển Trung Tâm Hệ Thống</span>
          </div>

          <h2 className="fw-bold mb-2 fs-3 fs-md-2">
            Hệ Thống Quản Trị Thể Thao DMS
          </h2>
          <p className="text-white-50 mb-0 small" style={{ maxWidth: '680px', lineHeight: 1.6 }}>
            Tổng quan toàn bộ dữ liệu giải đấu, phân quyền tài khoản đa vai trò (Admin, Ban tổ chức, Trọng tài, Thư ký, Đoàn tham gia) và theo dõi dữ liệu thể thao thời gian thực.
          </p>
        </div>
      </div>

      {/* 4 Thẻ Thống Kê Tổng Quan */}
      <Row className="g-3">
        <Col xs={6} md={3}>
          <div className="bg-white rounded-4 p-3.5 border shadow-sm d-flex align-items-center gap-3">
            <div
              className="rounded-3 d-flex align-items-center justify-content-center flex-shrink-0"
              style={{ width: '48px', height: '48px', backgroundColor: '#eff6ff', color: '#2563eb' }}
            >
              <i className="bi bi-trophy-fill fs-4"></i>
            </div>
            <div>
              <span className="text-secondary small d-block" style={{ fontSize: '11px', fontWeight: 500 }}>
                Tổng Giải đấu
              </span>
              <span className="fw-bold fs-4 text-dark">
                {loading ? <Spinner size="sm" /> : stats.totalGiaiDau}
              </span>
            </div>
          </div>
        </Col>

        <Col xs={6} md={3}>
          <div className="bg-white rounded-4 p-3.5 border shadow-sm d-flex align-items-center gap-3">
            <div
              className="rounded-3 d-flex align-items-center justify-content-center flex-shrink-0"
              style={{ width: '48px', height: '48px', backgroundColor: '#ecfdf5', color: '#059669' }}
            >
              <i className="bi bi-building fs-4"></i>
            </div>
            <div>
              <span className="text-secondary small d-block" style={{ fontSize: '11px', fontWeight: 500 }}>
                Đoàn tham gia
              </span>
              <span className="fw-bold fs-4 text-success">18</span>
            </div>
          </div>
        </Col>

        <Col xs={6} md={3}>
          <div className="bg-white rounded-4 p-3.5 border shadow-sm d-flex align-items-center gap-3">
            <div
              className="rounded-3 d-flex align-items-center justify-content-center flex-shrink-0"
              style={{ width: '48px', height: '48px', backgroundColor: '#faf5ff', color: '#9333ea' }}
            >
              <i className="bi bi-people-fill fs-4"></i>
            </div>
            <div>
              <span className="text-secondary small d-block" style={{ fontSize: '11px', fontWeight: 500 }}>
                Vận động viên
              </span>
              <span className="fw-bold fs-4 text-dark">264</span>
            </div>
          </div>
        </Col>

        <Col xs={6} md={3}>
          <div className="bg-white rounded-4 p-3.5 border shadow-sm d-flex align-items-center gap-3">
            <div
              className="rounded-3 d-flex align-items-center justify-content-center flex-shrink-0"
              style={{ width: '48px', height: '48px', backgroundColor: '#fffbeb', color: '#d97706' }}
            >
              <i className="bi bi-whistle-fill fs-4"></i>
            </div>
            <div>
              <span className="text-secondary small d-block" style={{ fontSize: '11px', fontWeight: 500 }}>
                Lực lượng Trọng tài
              </span>
              <span className="fw-bold fs-4 text-warning">32</span>
            </div>
          </div>
        </Col>
      </Row>

      {/* Bảng dữ liệu giải đấu gần đây */}
      <Row className="g-4">
        <Col lg={12}>
          <Card className="border-0 shadow-sm h-100">
            <CardBody className="p-4">
              <div className="d-flex justify-content-between align-items-center mb-3">
                <CardTitle tag="h5" className="fw-bold mb-0">Giải đấu gần đây</CardTitle>
                <Link href="/admin/giai-dau" className="small text-primary text-decoration-none">
                  Xem tất cả ({stats.totalGiaiDau}) <i className="bi bi-arrow-right"></i>
                </Link>
              </div>

              <div className="table-responsive">
                <Table hover className="align-middle mb-0 text-nowrap">
                  <thead className="table-light">
                    <tr>
                      <th>Mã giải</th>
                      <th>Tên giải</th>
                      <th>Phạm vi</th>
                      <th>Thời gian</th>
                      <th>Trạng thái</th>
                    </tr>
                  </thead>
                  <tbody>
                    {loading ? (
                      <tr>
                        <td colSpan={5} className="text-center py-4 text-muted">
                          <Spinner size="sm" className="me-2" /> Đang tải...
                        </td>
                      </tr>
                    ) : giaiDaus.length === 0 ? (
                      <tr>
                        <td colSpan={5} className="text-center py-4 text-muted">
                          Chưa có giải đấu nào.
                        </td>
                      </tr>
                    ) : (
                      giaiDaus.map((g) => (
                        <tr key={g.id}>
                          <td><span className="badge bg-light text-dark border">{g.ma}</span></td>
                          <td><strong className="text-dark">{g.ten}</strong></td>
                          <td><small className="text-muted">{g.phamViText || g.phamVi}</small></td>
                          <td className="small text-muted">
                            {g.ngayBatDau ? new Date(g.ngayBatDau).toLocaleDateString('vi-VN') : '---'}
                            {g.ngayKetThuc ? ` - ${new Date(g.ngayKetThuc).toLocaleDateString('vi-VN')}` : ''}
                          </td>
                          <td>
                            <Badge color="primary">
                              {g.trangThaiText || g.trangThai}
                            </Badge>
                          </td>
                        </tr>
                      ))
                    )}
                  </tbody>
                </Table>
              </div>
            </CardBody>
          </Card>
        </Col>
      </Row>
    </div>
  );
}
