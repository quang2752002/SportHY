'use client';

import React, { useState } from 'react';
import { useAuth } from '@/context/AuthContext';
import {
  Building2,
  UserPlus,
  Users,
  CheckCircle2,
  Clock,
  Trophy,
  Search,
  ShieldCheck,
} from 'lucide-react';
import { Row, Col, Card, CardBody, Badge, Button, Input, Form, FormGroup, Label, Table } from 'reactstrap';

export default function DonViPage() {
  const { user } = useAuth();
  const [athletes, setAthletes] = useState([
    { id: 1, name: 'Nguyễn Văn Hoàng', sport: 'Bóng Đá Nam', dob: '1998', unit: 'Đoàn Sở VH-TT Tỉnh', status: 'Đã duyệt', bib: 'BD-07' },
    { id: 2, name: 'Trần Thị Thu Hà', sport: 'Cầu Lông Đơn Nữ', dob: '2001', unit: 'Đoàn Sở VH-TT Tỉnh', status: 'Chờ duyệt', bib: 'CL-12' },
    { id: 3, name: 'Lê Minh Tuấn', sport: 'Điền Kinh 100m', dob: '2002', unit: 'Đoàn Sở VH-TT Tỉnh', status: 'Đã duyệt', bib: 'DK-03' },
    { id: 4, name: 'Phạm Hồng Nhung', sport: 'Bơi Tự Do 50m', dob: '2003', unit: 'Đoàn Sở VH-TT Tỉnh', status: 'Đã duyệt', bib: 'BO-09' },
    { id: 5, name: 'Hoàng Quốc Việt', sport: 'Bóng Bàn Đơn Nam', dob: '1999', unit: 'Đoàn Sở VH-TT Tỉnh', status: 'Chờ duyệt', bib: 'BB-04' },
  ]);

  const [name, setName] = useState('');
  const [sport, setSport] = useState('Bóng Đá Nam');
  const [dob, setDob] = useState('2001');
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState('ALL');
  const [notice, setNotice] = useState<string | null>(null);

  const handleAddAthlete = (e: React.FormEvent) => {
    e.preventDefault();
    if (!name.trim()) return;

    const newA = {
      id: Date.now(),
      name,
      sport,
      dob,
      unit: user?.fullName || 'Đoàn Sở VH-TT Tỉnh',
      status: 'Chờ duyệt',
      bib: `DV-${Math.floor(10 + Math.random() * 90)}`,
    };

    setAthletes([newA, ...athletes]);
    setName('');
    setNotice(`Đã thêm VĐV [${name}] vào danh sách đăng ký thi đấu của đoàn thành công!`);
    setTimeout(() => setNotice(null), 4000);
  };

  const filteredAthletes = athletes.filter((a) => {
    const matchName = a.name.toLowerCase().includes(search.toLowerCase()) || a.sport.toLowerCase().includes(search.toLowerCase());
    const matchStatus = statusFilter === 'ALL' || a.status === statusFilter;
    return matchName && matchStatus;
  });

  const countApproved = athletes.filter((a) => a.status === 'Đã duyệt').length;
  const countPending = athletes.filter((a) => a.status === 'Chờ duyệt').length;

  return (
    <div className="d-flex flex-column gap-4">
      {/* Banner Chào Mừng */}
      <div
        className="rounded-4 p-4 p-md-5 text-white position-relative overflow-hidden shadow-sm"
        style={{
          background: 'linear-gradient(135deg, #065f46 0%, #0f766e 50%, #0f172a 100%)',
        }}
      >
        <div
          className="position-absolute end-0 top-0 bottom-0 d-none d-md-flex align-items-center justify-content-end pe-5 opacity-10"
          style={{ pointerEvents: 'none' }}
        >
          <Building2 size={240} />
        </div>

        <div className="position-relative" style={{ zIndex: 2 }}>
          <div
            className="d-inline-flex align-items-center gap-2 px-3 py-1 rounded-pill mb-3 border"
            style={{ backgroundColor: 'rgba(255, 255, 255, 0.15)', borderColor: 'rgba(255, 255, 255, 0.2)', fontSize: '12px' }}
          >
            <ShieldCheck size={15} className="text-warning" />
            <span className="fw-semibold text-white">Phân hệ Quản Trị Đoàn Thi Đấu</span>
          </div>

          <h2 className="fw-bold mb-2 fs-3 fs-md-2">
            Xin chào, {user?.fullName || user?.username}!
          </h2>
          <p className="text-white-50 mb-0 small" style={{ maxWidth: '680px', lineHeight: 1.6 }}>
            Chào mừng bạn đến với cổng quản trị đoàn tham gia thi đấu. Tại đây bạn có thể quản lý danh sách vận động viên, gửi hồ sơ đăng ký nội dung, và theo dõi tiến độ duyệt của Ban tổ chức.
          </p>
        </div>
      </div>

      {/* 4 Thẻ Thống Kê Nhanh */}
      <Row className="g-3">
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
                Tổng số VĐV
              </span>
              <span className="fw-bold fs-4 text-dark">{athletes.length}</span>
            </div>
          </div>
        </Col>

        <Col xs={6} md={3}>
          <div className="bg-white rounded-4 p-3.5 border shadow-sm d-flex align-items-center gap-3">
            <div
              className="rounded-3 d-flex align-items-center justify-content-center flex-shrink-0"
              style={{ width: '48px', height: '48px', backgroundColor: '#eff6ff', color: '#2563eb' }}
            >
              <CheckCircle2 size={24} />
            </div>
            <div>
              <span className="text-secondary small d-block" style={{ fontSize: '11px', fontWeight: 500 }}>
                Hồ sơ đã duyệt
              </span>
              <span className="fw-bold fs-4 text-primary">{countApproved}</span>
            </div>
          </div>
        </Col>

        <Col xs={6} md={3}>
          <div className="bg-white rounded-4 p-3.5 border shadow-sm d-flex align-items-center gap-3">
            <div
              className="rounded-3 d-flex align-items-center justify-content-center flex-shrink-0"
              style={{ width: '48px', height: '48px', backgroundColor: '#fffbeb', color: '#d97706' }}
            >
              <Clock size={24} />
            </div>
            <div>
              <span className="text-secondary small d-block" style={{ fontSize: '11px', fontWeight: 500 }}>
                Chờ BTC duyệt
              </span>
              <span className="fw-bold fs-4 text-warning">{countPending}</span>
            </div>
          </div>
        </Col>

        <Col xs={6} md={3}>
          <div className="bg-white rounded-4 p-3.5 border shadow-sm d-flex align-items-center gap-3">
            <div
              className="rounded-3 d-flex align-items-center justify-content-center flex-shrink-0"
              style={{ width: '48px', height: '48px', backgroundColor: '#faf5ff', color: '#9333ea' }}
            >
              <Trophy size={24} />
            </div>
            <div>
              <span className="text-secondary small d-block" style={{ fontSize: '11px', fontWeight: 500 }}>
                Nội dung tham gia
              </span>
              <span className="fw-bold fs-4 text-dark">5</span>
            </div>
          </div>
        </Col>
      </Row>

      {/* Thông báo cập nhật */}
      {notice && (
        <div className="alert alert-success rounded-4 d-flex align-items-center justify-content-between mb-0 shadow-sm border-0 py-3">
          <div className="d-flex align-items-center gap-2">
            <CheckCircle2 size={18} className="text-success" />
            <span className="fw-medium text-success-emphasis">{notice}</span>
          </div>
          <button
            type="button"
            onClick={() => setNotice(null)}
            className="btn-close"
            style={{ fontSize: '10px' }}
          />
        </div>
      )}

      {/* Main Row: Form Đăng ký + Danh sách VĐV */}
      <Row className="g-4">
        {/* Form Đăng ký bên trái */}
        <Col lg={4}>
          <Card className="border-0 shadow-sm rounded-4">
            <CardBody className="p-4">
              <div className="d-flex align-items-center gap-2.5 mb-4 pb-3 border-bottom">
                <div
                  className="rounded-3 d-flex align-items-center justify-content-center flex-shrink-0"
                  style={{ width: '38px', height: '38px', backgroundColor: '#ecfdf5', color: '#059669' }}
                >
                  <UserPlus size={20} />
                </div>
                <div>
                  <h6 className="fw-bold text-dark mb-0">Đăng Ký Vận Động Viên</h6>
                  <small className="text-muted" style={{ fontSize: '11px' }}>
                    Thêm VĐV vào danh sách đoàn
                  </small>
                </div>
              </div>

              <Form onSubmit={handleAddAthlete} className="d-flex flex-column gap-3">
                <FormGroup className="mb-0">
                  <Label className="fw-semibold text-dark small mb-1">Họ và tên VĐV *</Label>
                  <Input
                    className="rounded-3 py-2 text-sm"
                    placeholder="VD: Nguyễn Văn Nam"
                    value={name}
                    onChange={(e) => setName(e.target.value)}
                    required
                  />
                </FormGroup>

                <FormGroup className="mb-0">
                  <Label className="fw-semibold text-dark small mb-1">Môn thi đấu / Nội dung *</Label>
                  <Input
                    type="select"
                    className="rounded-3 py-2 text-sm"
                    value={sport}
                    onChange={(e) => setSport(e.target.value)}
                  >
                    <option value="Bóng Đá Nam">Bóng Đá Nam (Sân 7)</option>
                    <option value="Cầu Lông Đơn Nam">Cầu Lông Đơn Nam</option>
                    <option value="Cầu Lông Đơn Nữ">Cầu Lông Đơn Nữ</option>
                    <option value="Cầu Lông Đôi Nam Nữ">Cầu Lông Đôi Nam Nữ</option>
                    <option value="Điền Kinh 100m">Điền Kinh 100m Nam</option>
                    <option value="Bơi Tự Do 50m">Bơi Tự Do 50m Nam</option>
                    <option value="Bóng Bàn Đơn Nam">Bóng Bàn Đơn Nam</option>
                    <option value="Pickleball Đôi Nam">Pickleball Đôi Nam</option>
                  </Input>
                </FormGroup>

                <FormGroup className="mb-0">
                  <Label className="fw-semibold text-dark small mb-1">Năm sinh *</Label>
                  <Input
                    type="number"
                    className="rounded-3 py-2 text-sm"
                    placeholder="2001"
                    value={dob}
                    onChange={(e) => setDob(e.target.value)}
                    required
                  />
                </FormGroup>

                <FormGroup className="mb-2">
                  <Label className="fw-semibold text-dark small mb-1">Đơn vị chủ quản</Label>
                  <Input
                    disabled
                    className="rounded-3 py-2 text-sm bg-light text-muted"
                    value={user?.fullName || 'Đoàn Sở VH-TT Tỉnh'}
                  />
                </FormGroup>

                <Button
                  type="submit"
                  color="success"
                  className="w-100 py-2.5 rounded-3 fw-bold shadow-sm d-flex align-items-center justify-content-center gap-2 mt-2"
                >
                  <UserPlus size={16} />
                  <span>Xác Nhận Thêm VĐV</span>
                </Button>
              </Form>
            </CardBody>
          </Card>
        </Col>

        {/* Bảng Danh sách bên phải */}
        <Col lg={8}>
          <Card className="border-0 shadow-sm rounded-4">
            <CardBody className="p-4">
              {/* Header và Search */}
              <div className="d-flex flex-column flex-sm-row align-items-sm-center justify-content-between gap-3 mb-4 pb-3 border-bottom">
                <div>
                  <h6 className="fw-bold text-dark mb-1">Danh Sách VĐV Đoàn Tham Gia</h6>
                  <small className="text-muted" style={{ fontSize: '12px' }}>
                    Đã đăng ký {filteredAthletes.length} vận động viên
                  </small>
                </div>

                <div className="d-flex align-items-center gap-2">
                  <div className="position-relative">
                    <Search size={14} className="position-absolute text-muted" style={{ left: '12px', top: '12px' }} />
                    <Input
                      className="rounded-pill ps-4 py-1.5 text-xs"
                      placeholder="Tìm tên, môn..."
                      value={search}
                      onChange={(e) => setSearch(e.target.value)}
                      style={{ width: '170px', fontSize: '12px' }}
                    />
                  </div>

                  <Input
                    type="select"
                    className="rounded-pill py-1.5 text-xs"
                    value={statusFilter}
                    onChange={(e) => setStatusFilter(e.target.value)}
                    style={{ width: '130px', fontSize: '12px' }}
                  >
                    <option value="ALL">Tất cả trạng thái</option>
                    <option value="Đã duyệt">Đã duyệt</option>
                    <option value="Chờ duyệt">Chờ duyệt</option>
                  </Input>
                </div>
              </div>

              {/* Bảng dữ liệu */}
              <div className="table-responsive">
                <Table hover className="align-middle mb-0" style={{ fontSize: '13px' }}>
                  <thead className="table-light">
                    <tr>
                      <th className="py-2.5 px-3">Số BIB</th>
                      <th className="py-2.5 px-3">Vận Động Viên</th>
                      <th className="py-2.5 px-3">Nội Dung Thi Đấu</th>
                      <th className="py-2.5 px-3 text-center">Năm Sinh</th>
                      <th className="py-2.5 px-3 text-center">Trạng Thái</th>
                    </tr>
                  </thead>
                  <tbody>
                    {filteredAthletes.length === 0 ? (
                      <tr>
                        <td colSpan={5} className="py-4 text-center text-muted small">
                          Không tìm thấy vận động viên nào phù hợp.
                        </td>
                      </tr>
                    ) : (
                      filteredAthletes.map((item) => (
                        <tr key={item.id}>
                          <td className="py-2.5 px-3">
                            <span className="badge bg-light text-dark border font-monospace px-2 py-1">
                              {item.bib}
                            </span>
                          </td>
                          <td className="py-2.5 px-3">
                            <div className="fw-semibold text-dark">{item.name}</div>
                            <small className="text-muted" style={{ fontSize: '11px' }}>
                              {item.unit}
                            </small>
                          </td>
                          <td className="py-2.5 px-3 fw-medium text-success">
                            {item.sport}
                          </td>
                          <td className="py-2.5 px-3 text-center font-monospace text-muted">
                            {item.dob}
                          </td>
                          <td className="py-2.5 px-3 text-center">
                            {item.status === 'Đã duyệt' ? (
                              <Badge color="success" pill className="px-2.5 py-1 fw-semibold">
                                <CheckCircle2 size={11} className="me-1" />
                                Đã duyệt
                              </Badge>
                            ) : (
                              <Badge color="warning" pill className="px-2.5 py-1 fw-semibold text-dark">
                                <Clock size={11} className="me-1" />
                                Chờ duyệt
                              </Badge>
                            )}
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

