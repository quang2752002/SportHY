'use client';

import React, { useState } from 'react';
import { useAuth } from '@/context/AuthContext';
import {
  Trophy,
  Calendar,
  Layers,
  Users,
  CheckCircle2,
  ShieldCheck,
  PlusCircle,
  PlayCircle,
  Clock,
  MapPin,
} from 'lucide-react';
import { Row, Col, Card, CardBody, Badge, Button, Table } from 'reactstrap';

export default function QuanLyGiaiPage() {
  const { user } = useAuth();
  const [tournaments] = useState([
    { id: 1, name: 'Giải Thể Thao Mở Rộng 2026', sport: 'Nhiều môn', date: '15/09 - 30/09/2026', status: 'Đang diễn ra', teams: 18, matches: 42 },
    { id: 2, name: 'Giải Bóng Đá Cúp Tỉnh 2026', sport: 'Bóng Đá Nam Sân 7', date: '01/10 - 15/10/2026', status: 'Sắp diễn ra', teams: 12, matches: 24 },
    { id: 3, name: 'Giải Pickleball & Cầu Lông Đôi', sport: 'Pickleball, Cầu Lông', date: '10/08 - 15/08/2026', status: 'Đã bế mạc', teams: 24, matches: 58 },
  ]);

  return (
    <div className="d-flex flex-column gap-4">
      {/* Banner Ban Tổ Chức */}
      <div
        className="rounded-4 p-4 p-md-5 text-white position-relative overflow-hidden shadow-sm"
        style={{
          background: 'linear-gradient(135deg, #1e3a8a 0%, #312e81 50%, #0f172a 100%)',
        }}
      >
        <div
          className="position-absolute end-0 top-0 bottom-0 d-none d-md-flex align-items-center justify-content-end pe-5 opacity-10"
          style={{ pointerEvents: 'none' }}
        >
          <Trophy size={240} />
        </div>

        <div className="position-relative" style={{ zIndex: 2 }}>
          <div
            className="d-inline-flex align-items-center gap-2 px-3 py-1 rounded-pill mb-3 border"
            style={{ backgroundColor: 'rgba(255, 255, 255, 0.15)', borderColor: 'rgba(255, 255, 255, 0.2)', fontSize: '12px' }}
          >
            <ShieldCheck size={15} className="text-warning" />
            <span className="fw-semibold text-white">Trung Tâm Điều Hành Ban Tổ Chức Giải</span>
          </div>

          <h2 className="fw-bold mb-2 fs-3 fs-md-2">
            Hệ Thống Quản Lý & Điều Hành Giải Đấu
          </h2>
          <p className="text-white-50 mb-0 small" style={{ maxWidth: '680px', lineHeight: 1.6 }}>
            Điều hành toàn bộ tiến độ tổ chức giải, quản lý bốc thăm chia bảng thi đấu, sắp xếp lịch trình sân bãi và tiếp nhận hồ sơ các đoàn thể thao tham gia.
          </p>
        </div>
      </div>

      {/* 4 Thẻ KPI Điều Hành */}
      <Row className="g-3">
        <Col xs={6} md={3}>
          <div className="bg-white rounded-4 p-3.5 border shadow-sm d-flex align-items-center gap-3">
            <div
              className="rounded-3 d-flex align-items-center justify-content-center flex-shrink-0"
              style={{ width: '48px', height: '48px', backgroundColor: '#eef2ff', color: '#4f46e5' }}
            >
              <Trophy size={24} />
            </div>
            <div>
              <span className="text-secondary small d-block" style={{ fontSize: '11px', fontWeight: 500 }}>
                Giải đấu điều hành
              </span>
              <span className="fw-bold fs-4 text-dark">3</span>
            </div>
          </div>
        </Col>

        <Col xs={6} md={3}>
          <div className="bg-white rounded-4 p-3.5 border shadow-sm d-flex align-items-center gap-3">
            <div
              className="rounded-3 d-flex align-items-center justify-content-center flex-shrink-0"
              style={{ width: '48px', height: '48px', backgroundColor: '#ecfdf5', color: '#059669' }}
            >
              <Users size={24} />
            </div>
            <div>
              <span className="text-secondary small d-block" style={{ fontSize: '11px', fontWeight: 500 }}>
                Đoàn tham gia
              </span>
              <span className="fw-bold fs-4 text-success">54</span>
            </div>
          </div>
        </Col>

        <Col xs={6} md={3}>
          <div className="bg-white rounded-4 p-3.5 border shadow-sm d-flex align-items-center gap-3">
            <div
              className="rounded-3 d-flex align-items-center justify-content-center flex-shrink-0"
              style={{ width: '48px', height: '48px', backgroundColor: '#fffbeb', color: '#d97706' }}
            >
              <PlayCircle size={24} />
            </div>
            <div>
              <span className="text-secondary small d-block" style={{ fontSize: '11px', fontWeight: 500 }}>
                Trận đang diễn ra
              </span>
              <span className="fw-bold fs-4 text-warning">8</span>
            </div>
          </div>
        </Col>

        <Col xs={6} md={3}>
          <div className="bg-white rounded-4 p-3.5 border shadow-sm d-flex align-items-center gap-3">
            <div
              className="rounded-3 d-flex align-items-center justify-content-center flex-shrink-0"
              style={{ width: '48px', height: '48px', backgroundColor: '#f0fdfa', color: '#0d9488' }}
            >
              <MapPin size={24} />
            </div>
            <div>
              <span className="text-secondary small d-block" style={{ fontSize: '11px', fontWeight: 500 }}>
                Cụm sân thi đấu
              </span>
              <span className="fw-bold fs-4 text-dark">6</span>
            </div>
          </div>
        </Col>
      </Row>

      {/* Danh sách giải đấu quản lý */}
      <Card className="border-0 shadow-sm rounded-4">
        <CardBody className="p-4">
          <div className="d-flex flex-column flex-sm-row align-items-sm-center justify-content-between gap-3 mb-4 pb-3 border-bottom">
            <div>
              <h6 className="fw-bold text-dark mb-1">Danh Sách Các Giải Đấu Đang Điều Hành</h6>
              <small className="text-muted" style={{ fontSize: '12px' }}>
                Tổng quan trạng thái, quy mô và tiến độ của các giải
              </small>
            </div>

            <Button
              color="primary"
              size="sm"
              className="rounded-pill px-3 py-1.5 fw-semibold d-flex align-items-center gap-1.5 shadow-sm"
              style={{ fontSize: '12px' }}
            >
              <PlusCircle size={15} />
              <span>Thêm Giải Mới</span>
            </Button>
          </div>

          <div className="table-responsive">
            <Table hover className="align-middle mb-0" style={{ fontSize: '13px' }}>
              <thead className="table-light">
                <tr>
                  <th className="py-2.5 px-3">Tên Giải Đấu</th>
                  <th className="py-2.5 px-3">Nội Dung / Môn</th>
                  <th className="py-2.5 px-3">Thời Gian Diễn Ra</th>
                  <th className="py-2.5 px-3 text-center">Số Đoàn</th>
                  <th className="py-2.5 px-3 text-center">Số Trận</th>
                  <th className="py-2.5 px-3 text-center">Trạng Thái</th>
                </tr>
              </thead>
              <tbody>
                {tournaments.map((t) => (
                  <tr key={t.id}>
                    <td className="py-3 px-3">
                      <div className="fw-semibold text-dark">{t.name}</div>
                    </td>
                    <td className="py-3 px-3 text-primary fw-medium">
                      {t.sport}
                    </td>
                    <td className="py-3 px-3 text-muted">
                      <Clock size={12} className="me-1" />
                      {t.date}
                    </td>
                    <td className="py-3 px-3 text-center fw-semibold text-dark">
                      {t.teams}
                    </td>
                    <td className="py-3 px-3 text-center font-monospace">
                      {t.matches}
                    </td>
                    <td className="py-3 px-3 text-center">
                      <Badge
                        color={
                          t.status === 'Đang diễn ra'
                            ? 'success'
                            : t.status === 'Sắp diễn ra'
                            ? 'warning'
                            : 'secondary'
                        }
                        pill
                        className="px-2.5 py-1 fw-semibold"
                      >
                        {t.status}
                      </Badge>
                    </td>
                  </tr>
                ))}
              </tbody>
            </Table>
          </div>
        </CardBody>
      </Card>
    </div>
  );
}

