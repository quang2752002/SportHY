'use client';

import React, { useState, Suspense } from 'react';
import { useRouter, useSearchParams } from 'next/navigation';
import Link from 'next/link';
import { useAuth } from '../../../context/AuthContext';
import { ROLE_DEFAULT_REDIRECT, AppRoles } from '../../../constants/roles';
import {
  Card,
  CardBody,
  Row,
  Col,
  Form,
  FormGroup,
  Label,
  Input,
  Button,
  Spinner,
  Alert,
} from 'reactstrap';

function LoginForm() {
  const [isRegister, setIsRegister] = useState(false);
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [email, setEmail] = useState('');
  const [fullName, setFullName] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  const { login, register } = useAuth();
  const router = useRouter();
  const searchParams = useSearchParams();
  const redirectParam = searchParams.get('redirect');

  const getRedirectPath = (roles: string[]) => {
    if (redirectParam) return redirectParam;
    for (const [role, path] of Object.entries(ROLE_DEFAULT_REDIRECT)) {
      if (roles.some((r) => r.toLowerCase() === role.toLowerCase())) return path;
    }
    return '/admin';
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setLoading(true);

    try {
      let loggedInUser;
      if (isRegister) {
        loggedInUser = await register({ username, password, email, fullName });
      } else {
        loggedInUser = await login({ username, password });
      }

      const roles = loggedInUser?.roles || [];
      const targetPath = getRedirectPath(roles);
      router.push(targetPath);
    } catch (err: any) {
      setError(err?.message || (Array.isArray(err) ? err.join(', ') : 'Đã có lỗi xảy ra khi xác thực.'));
    } finally {
      setLoading(false);
    }
  };

  const handleQuickLogin = (u: string, p: string) => {
    setUsername(u);
    setPassword(p);
    setIsRegister(false);
  };

  return (
    <div
      className="min-h-100 d-flex align-items-center justify-content-center py-5 px-3"
      style={{
        minHeight: '100vh',
        background: 'linear-gradient(135deg, #0f172a 0%, #1e293b 50%, #0948b3 100%)',
      }}
    >
      <div style={{ maxWidth: '520px', width: '100%' }}>
        <Card className="border-0 shadow-lg rounded-4 overflow-hidden bg-white">
          <CardBody className="p-4 p-md-5">
            {/* Header / Logo thương hiệu */}
            <div className="text-center mb-4">
              <div
                className="rounded-4 bg-primary text-white d-inline-flex align-items-center justify-content-center shadow mb-3"
                style={{ width: '64px', height: '64px', fontSize: '1.8rem' }}
              >
                <i className="bi bi-trophy-fill"></i>
              </div>
              <h3 className="fw-bold text-dark mb-1">
                {isRegister ? 'Đăng Ký Tài Khoản' : 'Hệ Thống Thể Thao DMS'}
              </h3>
              <p className="text-muted small mb-0">
                {isRegister
                  ? 'Tạo tài khoản quản lý giải đấu thể thao'
                  : 'Đăng nhập với Token JWT & Phân quyền RBAC'}
              </p>
            </div>

            {/* Quick Login 6 Roles Pills */}
            {!isRegister && (
              <div className="mb-4 p-3 bg-light rounded-3 border">
                <div className="d-flex align-items-center gap-2 mb-2 text-primary fw-semibold small">
                  <i className="bi bi-key-fill"></i>
                  <span>Đăng nhập nhanh theo 6 vai trò:</span>
                </div>
                <Row className="g-2">
                  <Col xs={6}>
                    <Button
                      color="primary"
                      outline
                      size="sm"
                      className="w-100 text-start rounded-2 py-2 px-2 d-flex flex-column border shadow-none"
                      onClick={() => handleQuickLogin('admin', 'Admin@123')}
                    >
                      <span className="fw-bold text-dark d-flex align-items-center gap-1">
                        Admin <i className="bi bi-check-circle-fill text-primary small"></i>
                      </span>
                      <small className="text-muted" style={{ fontSize: '11px' }}>
                        Toàn quyền hệ thống
                      </small>
                    </Button>
                  </Col>
                  <Col xs={6}>
                    <Button
                      color="secondary"
                      outline
                      size="sm"
                      className="w-100 text-start rounded-2 py-2 px-2 d-flex flex-column border shadow-none"
                      onClick={() => handleQuickLogin('manager', 'Manager@123')}
                    >
                      <span className="fw-bold text-dark">Quản Lý Giải</span>
                      <small className="text-muted" style={{ fontSize: '11px' }}>
                        Ban tổ chức giải
                      </small>
                    </Button>
                  </Col>
                  <Col xs={6}>
                    <Button
                      color="success"
                      outline
                      size="sm"
                      className="w-100 text-start rounded-2 py-2 px-2 d-flex flex-column border shadow-none"
                      onClick={() => handleQuickLogin('head_referee', 'HeadReferee@123')}
                    >
                      <span className="fw-bold text-dark">Trưởng Ban TT</span>
                      <small className="text-muted" style={{ fontSize: '11px' }}>
                        Phân công trọng tài
                      </small>
                    </Button>
                  </Col>
                  <Col xs={6}>
                    <Button
                      color="warning"
                      outline
                      size="sm"
                      className="w-100 text-start rounded-2 py-2 px-2 d-flex flex-column border shadow-none"
                      onClick={() => handleQuickLogin('referee', 'Referee@123')}
                    >
                      <span className="fw-bold text-dark">Trọng Tài</span>
                      <small className="text-muted" style={{ fontSize: '11px' }}>
                        Cập nhật kết quả tỉ số
                      </small>
                    </Button>
                  </Col>
                  <Col xs={6}>
                    <Button
                      color="info"
                      outline
                      size="sm"
                      className="w-100 text-start rounded-2 py-2 px-2 d-flex flex-column border shadow-none"
                      onClick={() => handleQuickLogin('secretary', 'Secretary@123')}
                    >
                      <span className="fw-bold text-dark">Thư Ký Giải</span>
                      <small className="text-muted" style={{ fontSize: '11px' }}>
                        Biên bản & xuất báo cáo
                      </small>
                    </Button>
                  </Col>
                  <Col xs={6}>
                    <Button
                      color="danger"
                      outline
                      size="sm"
                      className="w-100 text-start rounded-2 py-2 px-2 d-flex flex-column border shadow-none"
                      onClick={() => handleQuickLogin('delegation', 'Delegation@123')}
                    >
                      <span className="fw-bold text-dark">Đoàn VĐV</span>
                      <small className="text-muted" style={{ fontSize: '11px' }}>
                        Đăng ký VĐV & đội
                      </small>
                    </Button>
                  </Col>
                </Row>
              </div>
            )}

            {/* Thông báo lỗi */}
            {error && (
              <Alert color="danger" className="py-2 px-3 rounded-3 small d-flex align-items-center gap-2">
                <i className="bi bi-exclamation-triangle-fill fs-5"></i>
                <div>{error}</div>
              </Alert>
            )}

            {/* Form đăng nhập / đăng ký */}
            <Form onSubmit={handleSubmit}>
              <FormGroup className="mb-3">
                <Label className="fw-semibold small text-dark">Tên đăng nhập</Label>
                <div className="input-group">
                  <span className="input-group-text bg-light border-end-0 text-muted">
                    <i className="bi bi-person"></i>
                  </span>
                  <Input
                    type="text"
                    required
                    className="border-start-0 ps-1 rounded-end-3 shadow-none"
                    value={username}
                    onChange={(e) => setUsername(e.target.value)}
                    placeholder="VD: admin hoặc manager"
                  />
                </div>
              </FormGroup>

              {isRegister && (
                <>
                  <FormGroup className="mb-3">
                    <Label className="fw-semibold small text-dark">Họ và tên</Label>
                    <div className="input-group">
                      <span className="input-group-text bg-light border-end-0 text-muted">
                        <i className="bi bi-card-text"></i>
                      </span>
                      <Input
                        type="text"
                        required
                        className="border-start-0 ps-1 rounded-end-3 shadow-none"
                        value={fullName}
                        onChange={(e) => setFullName(e.target.value)}
                        placeholder="VD: Nguyễn Văn An"
                      />
                    </div>
                  </FormGroup>

                  <FormGroup className="mb-3">
                    <Label className="fw-semibold small text-dark">Địa chỉ Email</Label>
                    <div className="input-group">
                      <span className="input-group-text bg-light border-end-0 text-muted">
                        <i className="bi bi-envelope"></i>
                      </span>
                      <Input
                        type="email"
                        required
                        className="border-start-0 ps-1 rounded-end-3 shadow-none"
                        value={email}
                        onChange={(e) => setEmail(e.target.value)}
                        placeholder="email@example.com"
                      />
                    </div>
                  </FormGroup>
                </>
              )}

              <FormGroup className="mb-4">
                <Label className="fw-semibold small text-dark">Mật khẩu</Label>
                <div className="input-group">
                  <span className="input-group-text bg-light border-end-0 text-muted">
                    <i className="bi bi-lock"></i>
                  </span>
                  <Input
                    type="password"
                    required
                    className="border-start-0 ps-1 rounded-end-3 shadow-none"
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                    placeholder="••••••••"
                  />
                </div>
              </FormGroup>

              <Button
                color="primary"
                type="submit"
                size="lg"
                disabled={loading}
                className="w-100 fw-semibold rounded-3 py-2 shadow-sm d-flex align-items-center justify-content-center gap-2"
              >
                {loading ? (
                  <>
                    <Spinner size="sm" />
                    <span>Đang xử lý...</span>
                  </>
                ) : (
                  <>
                    <span>{isRegister ? 'Đăng Ký Tài Khoản' : 'Đăng Nhập Vào Hệ Thống'}</span>
                    <i className="bi bi-arrow-right"></i>
                  </>
                )}
              </Button>
            </Form>

            {/* Chuyển đổi đăng nhập / đăng ký */}
            <div className="mt-4 pt-3 border-top text-center text-muted small">
              {isRegister ? (
                <p className="mb-2">
                  Đã có tài khoản?{' '}
                  <button
                    type="button"
                    onClick={() => setIsRegister(false)}
                    className="btn btn-link p-0 text-decoration-none fw-semibold text-primary small"
                  >
                    Đăng nhập ngay
                  </button>
                </p>
              ) : (
                <p className="mb-2">
                  Chưa có tài khoản?{' '}
                  <button
                    type="button"
                    onClick={() => setIsRegister(true)}
                    className="btn btn-link p-0 text-decoration-none fw-semibold text-primary small"
                  >
                    Đăng ký tài khoản mới
                  </button>
                </p>
              )}

              <div>
                <Link href="/" className="text-muted text-decoration-none small d-inline-flex align-items-center gap-1">
                  <i className="bi bi-house"></i> Quay về Trang chủ
                </Link>
              </div>
            </div>
          </CardBody>
        </Card>
      </div>
    </div>
  );
}

export default function LoginPage() {
  return (
    <Suspense fallback={<div className="d-flex justify-content-center align-items-center vh-100"><Spinner color="primary" /></div>}>
      <LoginForm />
    </Suspense>
  );
}
