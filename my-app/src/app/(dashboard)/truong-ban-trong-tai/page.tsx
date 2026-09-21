'use client';

import React from 'react';
import { Award, UserCheck, ShieldCheck, Calendar, Clock, CheckCircle2 } from 'lucide-react';
import { Card, CardBody, Row, Col, Table, Badge, Button } from 'reactstrap';

export default function TruongBanTrongTaiPage() {
  const referees = [
    { id: 1, name: 'Nguyễn Văn A', grade: 'Trọng tài Quốc gia', sport: 'Bóng đá', status: 'Sẵn sàng' },
    { id: 2, name: 'Trần Văn B', grade: 'Trọng tài Cấp 1', sport: 'Cầu lông', status: 'Đang làm nhiệm vụ' },
    { id: 3, name: 'Lê Thị C', grade: 'Trọng tài Cấp 1', sport: 'Bơi lội', status: 'Sẵn sàng' },
    { id: 4, name: 'Phạm Văn D', grade: 'Trọng tài Quốc gia', sport: 'Điền kinh', status: 'Sẵn sàng' },
  ];

  const matchesNeedAssign = [
    { id: 101, match: 'Chung kết Bóng đá Nam: Đội A vs Đội B', time: '15:30 20/09', stadium: 'Sân vận động 1' },
    { id: 102, match: 'Bán kết Cầu lông Đơn nữ: VĐV X vs VĐV Y', time: '09:00 21/09', stadium: 'Nhà thi đấu A' },
  ];

  return (
    <div className="d-flex flex-column gap-4">
      {/* Banner Trưởng Ban Trọng Tài */}
      <div
        className="rounded-4 p-4 p-md-5 text-white position-relative overflow-hidden shadow-sm"
        style={{
          background: 'linear-gradient(135deg, #b45309 0%, #78350f 50%, #0f172a 100%)',
        }}
      >
        <div
          className="position-absolute end-0 top-0 bottom-0 d-none d-md-flex align-items-center justify-content-end pe-5 opacity-10"
          style={{ pointerEvents: 'none' }}
        >
          <Award size={240} />
        </div>

        <div className="position-relative" style={{ zIndex: 2 }}>
          <div
            className="d-inline-flex align-items-center gap-2 px-3 py-1 rounded-pill mb-3 border"
            style={{ backgroundColor: 'rgba(255, 255, 255, 0.15)', borderColor: 'rgba(255, 255, 255, 0.2)', fontSize: '12px' }}
          >
            <ShieldCheck size={15} className="text-warning" />
            <span className="fw-semibold text-white">Chỉ Đạo Chuyên Môn Trọng Tài</span>
          </div>

          <h2 className="fw-bold mb-2 fs-3 fs-md-2">
            Phân Hệ Trưởng Ban Trọng Tài
          </h2>
          <p className="text-white-50 mb-0 small" style={{ maxWidth: '680px', lineHeight: 1.6 }}>
            Điều động, phân công lực lượng trọng tài chính và trợ lý cho từng trận đấu, kiểm soát chất lượng chuyên môn và xử lý các khiếu nại luật thi đấu.
          </p>
        </div>
      </div>

      <Row className="g-4">
        {/* Trận đấu cần phân công */}
        <Col lg={6}>
          <Card className="border-0 shadow-sm rounded-4 h-100">
            <CardBody className="p-4">
              <div className="d-flex align-items-center justify-content-between mb-4 pb-3 border-bottom">
                <div className="d-flex align-items-center gap-2.5">
                  <div
                    className="rounded-3 d-flex align-items-center justify-content-center"
                    style={{ width: '38px', height: '38px', backgroundColor: '#fef3c7', color: '#d97706' }}
                  >
                    <Calendar size={20} />
                  </div>
                  <div>
                    <h6 className="fw-bold text-dark mb-0">Trận Đấu Cần Phân Công</h6>
                    <small className="text-muted" style={{ fontSize: '11px' }}>
                      Các trận đấu chuẩn bị diễn ra
                    </small>
                  </div>
                </div>
              </div>

              <div className="table-responsive">
                <Table hover className="align-middle mb-0" style={{ fontSize: '13px' }}>
                  <thead className="table-light">
                    <tr>
                      <th className="py-2.5 px-3">Trận Đấu</th>
                      <th className="py-2.5 px-3">Thời Gian</th>
                      <th className="py-2.5 px-3">Địa Điểm</th>
                      <th className="py-2.5 px-3 text-end">Thao Tác</th>
                    </tr>
                  </thead>
                  <tbody>
                    {matchesNeedAssign.map((m) => (
                      <tr key={m.id}>
                        <td className="py-3 px-3 fw-semibold text-dark">{m.match}</td>
                        <td className="py-3 px-3 text-muted">
                          <Clock size={12} className="me-1" />
                          {m.time}
                        </td>
                        <td className="py-3 px-3">{m.stadium}</td>
                        <td className="py-3 px-3 text-end">
                          <Button
                            color="warning"
                            size="sm"
                            className="rounded-pill px-3 py-1 fw-semibold text-dark"
                            style={{ fontSize: '11px' }}
                          >
                            Phân công
                          </Button>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </Table>
              </div>
            </CardBody>
          </Card>
        </Col>

        {/* Lực lượng trọng tài */}
        <Col lg={6}>
          <Card className="border-0 shadow-sm rounded-4 h-100">
            <CardBody className="p-4">
              <div className="d-flex align-items-center justify-content-between mb-4 pb-3 border-bottom">
                <div className="d-flex align-items-center gap-2.5">
                  <div
                    className="rounded-3 d-flex align-items-center justify-content-center"
                    style={{ width: '38px', height: '38px', backgroundColor: '#ecfdf5', color: '#059669' }}
                  >
                    <UserCheck size={20} />
                  </div>
                  <div>
                    <h6 className="fw-bold text-dark mb-0">Lực Lượng Trọng Tài ({referees.length})</h6>
                    <small className="text-muted" style={{ fontSize: '11px' }}>
                      Trạng thái sẵn sàng làm nhiệm vụ
                    </small>
                  </div>
                </div>
              </div>

              <div className="table-responsive">
                <Table hover className="align-middle mb-0" style={{ fontSize: '13px' }}>
                  <thead className="table-light">
                    <tr>
                      <th className="py-2.5 px-3">Họ và Tên</th>
                      <th className="py-2.5 px-3">Môn</th>
                      <th className="py-2.5 px-3">Cấp Bậc</th>
                      <th className="py-2.5 px-3 text-center">Trạng Thái</th>
                    </tr>
                  </thead>
                  <tbody>
                    {referees.map((r) => (
                      <tr key={r.id}>
                        <td className="py-3 px-3 fw-semibold text-dark">{r.name}</td>
                        <td className="py-3 px-3 text-primary">{r.sport}</td>
                        <td className="py-3 px-3 text-muted">{r.grade}</td>
                        <td className="py-3 px-3 text-center">
                          <Badge
                            color={r.status === 'Sẵn sàng' ? 'success' : 'info'}
                            pill
                            className="px-2.5 py-1"
                          >
                            {r.status}
                          </Badge>
                        </td>
                      </tr>
                    ))}
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

