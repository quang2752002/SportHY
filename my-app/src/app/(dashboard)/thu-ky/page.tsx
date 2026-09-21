'use client';

import React, { useState } from 'react';
import { useAuth } from '@/context/AuthContext';
import { FileCheck, Download, CheckCircle2, ShieldCheck, Printer, Eye } from 'lucide-react';
import { Row, Col, Card, CardBody, Badge, Button, Table } from 'reactstrap';

export default function SecretaryPage() {
  const { hasPermission, user } = useAuth();
  const [exportNotice, setExportNotice] = useState<string | null>(null);

  const handleExport = (type: 'PDF' | 'Excel') => {
    setExportNotice(`Đã xuất biên bản kết quả thi đấu định dạng ${type} thành công!`);
    setTimeout(() => setExportNotice(null), 4000);
  };

  return (
    <div className="d-flex flex-column gap-4">
      {/* Banner Thư Ký */}
      <div
        className="rounded-4 p-4 p-md-5 text-white position-relative overflow-hidden shadow-sm"
        style={{
          background: 'linear-gradient(135deg, #0e7490 0%, #0369a1 50%, #0f172a 100%)',
        }}
      >
        <div
          className="position-absolute end-0 top-0 bottom-0 d-none d-md-flex align-items-center justify-content-end pe-5 opacity-10"
          style={{ pointerEvents: 'none' }}
        >
          <FileCheck size={240} />
        </div>

        <div className="position-relative" style={{ zIndex: 2 }}>
          <div
            className="d-inline-flex align-items-center gap-2 px-3 py-1 rounded-pill mb-3 border"
            style={{ backgroundColor: 'rgba(255, 255, 255, 0.15)', borderColor: 'rgba(255, 255, 255, 0.2)', fontSize: '12px' }}
          >
            <ShieldCheck size={15} className="text-warning" />
            <span className="fw-semibold text-white">Nhiệm Vụ Thư Ký & Biên Bản</span>
          </div>

          <h2 className="fw-bold mb-2 fs-3 fs-md-2">
            Phân Hệ Thư Ký Tổng Hợp & Kết Quả
          </h2>
          <p className="text-white-50 mb-0 small" style={{ maxWidth: '680px', lineHeight: 1.6 }}>
            Tiếp nhận, đối soát và kiểm tra biên bản thi đấu từ các tổ trọng tài; tổng hợp tỷ số và xuất báo cáo kết quả chính thức (PDF / Excel) cho Ban tổ chức.
          </p>
        </div>
      </div>

      {exportNotice && (
        <div className="alert alert-success rounded-4 d-flex align-items-center justify-content-between mb-0 shadow-sm border-0 py-3">
          <div className="d-flex align-items-center gap-2">
            <CheckCircle2 size={18} className="text-success" />
            <span className="fw-medium text-success-emphasis">{exportNotice}</span>
          </div>
          <button
            type="button"
            onClick={() => setExportNotice(null)}
            className="btn-close"
            style={{ fontSize: '10px' }}
          />
        </div>
      )}

      {/* Danh sách biên bản thi đấu */}
      <Card className="border-0 shadow-sm rounded-4">
        <CardBody className="p-4">
          <div className="d-flex flex-column flex-sm-row align-items-sm-center justify-content-between gap-3 mb-4 pb-3 border-bottom">
            <div>
              <h6 className="fw-bold text-dark mb-1">Danh Sách Biên Bản Thi Đấu Cần Duyệt</h6>
              <small className="text-muted" style={{ fontSize: '12px' }}>
                Tổng cộng: 3 biên bản trận đấu
              </small>
            </div>

            <div className="d-flex gap-2">
              <Button
                color="danger"
                outline
                size="sm"
                onClick={() => handleExport('PDF')}
                className="rounded-pill px-3 py-1.5 fw-semibold d-flex align-items-center gap-1.5 shadow-none"
                style={{ fontSize: '12px' }}
              >
                <Download size={14} />
                <span>Xuất PDF</span>
              </Button>
              <Button
                color="success"
                outline
                size="sm"
                onClick={() => handleExport('Excel')}
                className="rounded-pill px-3 py-1.5 fw-semibold d-flex align-items-center gap-1.5 shadow-none"
                style={{ fontSize: '12px' }}
              >
                <Download size={14} />
                <span>Xuất Excel</span>
              </Button>
            </div>
          </div>

          <div className="table-responsive">
            <Table hover className="align-middle mb-0" style={{ fontSize: '13px' }}>
              <thead className="table-light">
                <tr>
                  <th className="py-2.5 px-3">Mã Biên Bản</th>
                  <th className="py-2.5 px-3">Môn Thi Đấu</th>
                  <th className="py-2.5 px-3">Cặp Đấu</th>
                  <th className="py-2.5 px-3 text-center">Tỷ Số</th>
                  <th className="py-2.5 px-3 text-center">Trạng Thái</th>
                  <th className="py-2.5 px-3 text-end">Thao Tác</th>
                </tr>
              </thead>
              <tbody>
                <tr>
                  <td className="py-3 px-3 font-monospace text-muted">#BB-091</td>
                  <td className="py-3 px-3 fw-medium text-dark">Bóng Đá Nam 7 Người</td>
                  <td className="py-3 px-3">Đoàn Sở GD-ĐT vs Đoàn Sở VH-TT</td>
                  <td className="py-3 px-3 text-center font-monospace fw-bold text-warning fs-6">2 - 1</td>
                  <td className="py-3 px-3 text-center">
                    <Badge color="warning" pill className="px-2.5 py-1 text-dark">
                      Chờ Thư ký duyệt
                    </Badge>
                  </td>
                  <td className="py-3 px-3 text-end">
                    <Button
                      color="primary"
                      size="sm"
                      className="rounded-pill px-2.5 py-1 d-inline-flex align-items-center gap-1"
                      style={{ fontSize: '11px' }}
                      onClick={() => alert('Kiểm tra chi tiết biên bản #BB-091')}
                    >
                      <Eye size={13} />
                      <span>Kiểm tra</span>
                    </Button>
                  </td>
                </tr>

                <tr>
                  <td className="py-3 px-3 font-monospace text-muted">#BB-090</td>
                  <td className="py-3 px-3 fw-medium text-dark">Cầu Lông Đơn Nam</td>
                  <td className="py-3 px-3">Nguyễn Văn A vs Trần Văn B</td>
                  <td className="py-3 px-3 text-center font-monospace fw-bold text-success fs-6">2 - 0</td>
                  <td className="py-3 px-3 text-center">
                    <Badge color="success" pill className="px-2.5 py-1">
                      Đã Ký Duyệt
                    </Badge>
                  </td>
                  <td className="py-3 px-3 text-end">
                    <Button
                      color="secondary"
                      outline
                      size="sm"
                      className="rounded-pill px-2.5 py-1 d-inline-flex align-items-center gap-1 shadow-none"
                      style={{ fontSize: '11px' }}
                      onClick={() => handleExport('PDF')}
                    >
                      <Printer size={13} />
                      <span>In biên bản</span>
                    </Button>
                  </td>
                </tr>
              </tbody>
            </Table>
          </div>
        </CardBody>
      </Card>
    </div>
  );
}

