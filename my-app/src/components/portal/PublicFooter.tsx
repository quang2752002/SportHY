'use client';

import React from 'react';
import Link from 'next/link';
import { Container, Row, Col } from 'reactstrap';

export default function PublicFooter() {
  return (
    <footer className="portal-footer bg-white border-top mt-auto py-4 py-md-5">
      <Container fluid="lg">
        <Row className="gy-4">
          {/* Cột Trái: Logo & Đơn vị phát triển */}
          <Col md={5} className="d-flex flex-column align-items-start">
            <div className="d-flex align-items-center gap-2 mb-3">
              <div
                className="rounded-circle d-flex align-items-center justify-content-center shadow-sm"
                style={{
                  width: '38px',
                  height: '38px',
                  background: 'radial-gradient(circle, #f97316 0%, #1e3a8a 100%)',
                }}
              >
                <span className="text-white fw-bold" style={{ fontSize: '9px', textAlign: 'center' }}>
                  HY
                </span>
              </div>
              <div
                className="rounded-circle d-flex align-items-center justify-content-center shadow-sm text-white fw-bold"
                style={{
                  width: '38px',
                  height: '38px',
                  backgroundColor: '#0f172a',
                  fontSize: '10px',
                }}
              >
                DALI
              </div>
            </div>
            <div className="fw-bold text-dark mb-1">© 2024–2026 TB Pick</div>
            <div
              className="fw-bold text-uppercase mb-2"
              style={{ color: '#ea580c', fontSize: '0.85rem' }}
            >
              PHÁT TRIỂN BỞI DALI SPORTS
            </div>
            <p
              className="text-muted small mb-0"
              style={{ maxWidth: '340px', fontSize: '0.8rem', lineHeight: '1.5' }}
            >
              Nền tảng quản lý VĐV và vận hành giải đấu chuyên nghiệp cho cộng đồng Hưng Yên &amp; toàn quốc.
            </p>
          </Col>

          {/* Cột Giữa: Khám phá */}
          <Col sm={6} md={3}>
            <div
              className="d-flex align-items-center gap-1 fw-bold text-uppercase mb-3"
              style={{ color: '#ea580c', fontSize: '0.85rem' }}
            >
              <i className="bi bi-trophy"></i>
              <span>KHÁM PHÁ</span>
            </div>
            <ul className="list-unstyled d-flex flex-column gap-2 small text-muted mb-0">
              <li>
                <Link href="/" className="text-decoration-none text-secondary hover-orange">
                  Giải đấu
                </Link>
              </li>
              <li>
                <Link href="/diem-trinh" className="text-decoration-none text-secondary hover-orange">
                  Điểm trình
                </Link>
              </li>
              <li>
                <Link href="/cau-lac-bo" className="text-decoration-none text-secondary hover-orange">
                  Câu lạc bộ
                </Link>
              </li>
              <li>
                <Link href="/lich-thi-dau" className="text-decoration-none text-secondary hover-orange">
                  Lịch thi đấu
                </Link>
              </li>
              <li>
                <Link href="/ty-so-truc-tiep" className="text-decoration-none text-secondary hover-orange">
                  Tỷ số trực tiếp
                </Link>
              </li>
            </ul>
          </Col>

          {/* Cột Phải: Liên hệ & Mạng xã hội */}
          <Col sm={6} md={4}>
            <div
              className="fw-bold text-uppercase mb-3 text-dark"
              style={{ fontSize: '0.85rem' }}
            >
              LIÊN HỆ
            </div>
            <div className="d-flex align-items-center gap-2 text-secondary small mb-3">
              <i className="bi bi-telephone-fill" style={{ color: '#ea580c' }}></i>
              <span className="fw-semibold">098 438 79 99</span>
            </div>
            <div className="d-flex align-items-center gap-2">
              <a
                href="https://facebook.com"
                target="_blank"
                rel="noreferrer"
                className="btn btn-sm btn-light border rounded-circle d-flex align-items-center justify-content-center text-secondary"
                style={{ width: '36px', height: '36px' }}
                aria-label="Facebook"
              >
                <i className="bi bi-facebook"></i>
              </a>
              <a
                href="https://youtube.com"
                target="_blank"
                rel="noreferrer"
                className="btn btn-sm btn-light border rounded-circle d-flex align-items-center justify-content-center text-secondary"
                style={{ width: '36px', height: '36px' }}
                aria-label="YouTube"
              >
                <i className="bi bi-youtube"></i>
              </a>
              <a
                href="https://tiktok.com"
                target="_blank"
                rel="noreferrer"
                className="btn btn-sm btn-light border rounded-circle d-flex align-items-center justify-content-center text-secondary"
                style={{ width: '36px', height: '36px' }}
                aria-label="TikTok"
              >
                <i className="bi bi-tiktok"></i>
              </a>
            </div>
          </Col>
        </Row>
      </Container>
    </footer>
  );
}
