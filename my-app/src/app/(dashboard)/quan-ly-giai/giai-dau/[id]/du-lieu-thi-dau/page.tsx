'use client';

import React, { useEffect, useState, useMemo, use } from 'react';
import Link from 'next/link';
import {
  Row,
  Col,
  Card,
  CardBody,
  Button,
  Table,
  Badge,
  Spinner,
  Modal,
  ModalHeader,
  ModalBody,
  ModalFooter,
  Input,
  FormGroup,
  Label,
  Nav,
  NavItem,
  NavLink,
  Alert,
} from 'reactstrap';
import {
  giaiDauService,
  dangKyThiDauService,
  tranDauService,
} from '@/services';
import { GiaiDau, DangKyThiDau, TranDau } from '@/types';
import { useToast } from '@/context/AuthContext';

export default function DuLieuThiDauPage({ params }: { params: Promise<{ id: string }> }) {
  const unwrappedParams = use(params);
  const id = Number(unwrappedParams.id);

  const toast = useToast();

  // Data states
  const [tournament, setTournament] = useState<GiaiDau | null>(null);
  const [registrations, setRegistrations] = useState<DangKyThiDau[]>([]);
  const [matches, setMatches] = useState<TranDau[]>([]);
  const [loading, setLoading] = useState(true);

  // Active Tab
  const [activeTab, setActiveTab] = useState<'dang-ky' | 'tran-dau'>('dang-ky');

  // Filters
  const [filterMonId, setFilterMonId] = useState<number | 'ALL'>('ALL');
  const [searchKeyword, setSearchKeyword] = useState('');

  // Delete Modal States (DangKyThiDau)
  const [deleteRegModal, setDeleteRegModal] = useState(false);
  const [regToDelete, setRegToDelete] = useState<DangKyThiDau | null>(null);
  const [deletingReg, setDeletingReg] = useState(false);

  // Delete Modal States (TranDau)
  const [deleteMatchModal, setDeleteMatchModal] = useState(false);
  const [matchToDelete, setMatchToDelete] = useState<TranDau | null>(null);
  const [deletingMatch, setDeletingMatch] = useState(false);

  // Clear All Matches for Sport Modal
  const [clearMatchesModal, setClearMatchesModal] = useState(false);
  const [clearingMatches, setClearingMatches] = useState(false);

  // Fetch all initial data
  const fetchData = async () => {
    setLoading(true);
    try {
      const [tourData, regsData, matchesData] = await Promise.all([
        giaiDauService.getById(id),
        dangKyThiDauService.getAll({ giaiDauId: id }).catch(() => []),
        tranDauService.getAll({ giaiDauId: id }).catch(() => []),
      ]);

      setTournament(tourData);
      setRegistrations(regsData || []);
      setMatches(matchesData || []);
    } catch (err: any) {
      console.error('Lỗi khi tải dữ liệu giải đấu:', err);
      toast.error(err?.message || 'Không thể tải dữ liệu thi đấu.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (id) {
      fetchData();
    }
  }, [id]);

  // List of sports from tournament (deduplicated by sport id)
  const availableMons = useMemo(() => {
    if (!tournament?.monTheThaos) return [];
    const seen = new Set<number>();
    return tournament.monTheThaos.filter((m) => {
      const sportKey = m.monTheThaoId ?? m.id;
      if (seen.has(sportKey)) return false;
      seen.add(sportKey);
      return true;
    });
  }, [tournament]);

  // Filtered Registrations
  const filteredRegistrations = useMemo(() => {
    return registrations.filter((reg) => {
      // Filter by Sport
      if (filterMonId !== 'ALL') {
        const gdmId = reg.giaiDauMonTheThaoId ?? reg.noiDungThiDauId;
        if (gdmId !== filterMonId) return false;
      }
      // Filter by Keyword
      if (searchKeyword.trim()) {
        const kw = searchKeyword.toLowerCase();
        const matchName = (reg.tenDangKy || reg.tenDoi || '').toLowerCase().includes(kw);
        const matchCode = (reg.soDangKy || '').toLowerCase().includes(kw);
        const matchDonVi = (reg.tenDonVi || '').toLowerCase().includes(kw);
        const matchSport = (reg.tenMonTheThao || '').toLowerCase().includes(kw);
        if (!matchName && !matchCode && !matchDonVi && !matchSport) return false;
      }
      return true;
    });
  }, [registrations, filterMonId, searchKeyword]);

  // Filtered Matches
  const filteredMatches = useMemo(() => {
    return matches.filter((m) => {
      // Filter by Sport
      if (filterMonId !== 'ALL') {
        const gdmId = m.giaiDauMonTheThaoId ?? m.noiDungThiDauId;
        if (gdmId !== filterMonId) return false;
      }
      // Filter by Keyword
      if (searchKeyword.trim()) {
        const kw = searchKeyword.toLowerCase();
        const matchCode = (m.tenTran || `Trận ${m.soTran}`).toLowerCase().includes(kw);
        const matchTeam1 = (m.tenDoi1 || '').toLowerCase().includes(kw);
        const matchTeam2 = (m.tenDoi2 || '').toLowerCase().includes(kw);
        const matchSport = (m.tenMonTheThao || '').toLowerCase().includes(kw);
        if (!matchCode && !matchTeam1 && !matchTeam2 && !matchSport) return false;
      }
      return true;
    });
  }, [matches, filterMonId, searchKeyword]);

  // Count stats by sport
  const sportStats = useMemo(() => {
    const map = new Map<number, { regCount: number; matchCount: number }>();
    availableMons.forEach((m) => {
      const regCount = registrations.filter((r) => (r.giaiDauMonTheThaoId ?? r.noiDungThiDauId) === m.id).length;
      const matchCount = matches.filter((x) => (x.giaiDauMonTheThaoId ?? x.noiDungThiDauId) === m.id).length;
      map.set(m.id, { regCount, matchCount });
    });
    return map;
  }, [availableMons, registrations, matches]);

  // Handle Delete Registration
  const confirmDeleteReg = async () => {
    if (!regToDelete) return;
    setDeletingReg(true);
    try {
      await dangKyThiDauService.delete(regToDelete.id);
      toast.success(`Đã xóa hồ sơ "${regToDelete.tenDangKy || regToDelete.soDangKy}" thành công!`);
      setRegistrations((prev) => prev.filter((r) => r.id !== regToDelete.id));
      setDeleteRegModal(false);
      setRegToDelete(null);
    } catch (err: any) {
      console.error('Lỗi xóa hồ sơ:', err);
      toast.error(err?.response?.data?.message || err?.message || 'Không thể xóa hồ sơ này.');
    } finally {
      setDeletingReg(false);
    }
  };

  // Handle Delete Match
  const confirmDeleteMatch = async () => {
    if (!matchToDelete) return;
    setDeletingMatch(true);
    try {
      await tranDauService.delete(matchToDelete.id);
      toast.success(`Đã xóa trận đấu "${matchToDelete.tenTran || `Trận ${matchToDelete.soTran}`}" thành công!`);
      setMatches((prev) => prev.filter((m) => m.id !== matchToDelete.id));
      setDeleteMatchModal(false);
      setMatchToDelete(null);
    } catch (err: any) {
      console.error('Lỗi xóa trận đấu:', err);
      toast.error(err?.response?.data?.message || err?.message || 'Không thể xóa trận đấu này.');
    } finally {
      setDeletingMatch(false);
    }
  };

  // Handle Clear All Matches for Selected Sport
  const confirmClearMatches = async () => {
    if (filterMonId === 'ALL') return;
    setClearingMatches(true);
    try {
      await tranDauService.clearByNoiDung(filterMonId);
      const monName = availableMons.find((m) => m.id === filterMonId)?.ten || '';
      toast.success(`Đã xóa toàn bộ lịch thi đấu của môn "${monName}"!`);
      setMatches((prev) => prev.filter((m) => (m.giaiDauMonTheThaoId ?? m.noiDungThiDauId) !== filterMonId));
      setClearMatchesModal(false);
    } catch (err: any) {
      console.error('Lỗi xóa toàn bộ lịch đấu:', err);
      toast.error(err?.response?.data?.message || err?.message || 'Không thể xóa lịch thi đấu.');
    } finally {
      setClearingMatches(false);
    }
  };

  if (loading) {
    return (
      <div className="text-center py-5">
        <Spinner color="primary" />
        <p className="mt-2 text-muted small">Đang tải dữ liệu thi đấu của giải...</p>
      </div>
    );
  }

  return (
    <div className="pb-5">
      {/* Header Điều Hướng */}
      <div className="d-flex align-items-center justify-content-between mb-4">
        <div className="d-flex align-items-center gap-3">
          <Link
            href={`/quan-ly-giai/giai-dau/${id}`}
            className="btn btn-outline-secondary btn-sm rounded-circle d-flex align-items-center justify-content-center"
            style={{ width: '38px', height: '38px' }}
            title="Quay lại chỉnh sửa giải đấu"
          >
            <i className="bi bi-arrow-left fs-5"></i>
          </Link>
          <div>
            <h4 className="fw-bold mb-0 text-dark">
              Dữ Liệu Đăng Ký &amp; Trận Đấu{' '}
              <span className="text-primary font-monospace">#{tournament?.ma}</span>
            </h4>
            <span className="text-muted small">
              {tournament?.ten} &bull; Xóa các đăng ký hoặc trận đấu để có thể gỡ bỏ môn thể thao khỏi giải đấu
            </span>
          </div>
        </div>
        <div className="d-flex gap-2">
          <Link
            href={`/quan-ly-giai/lich-thi-dau?giaiDauId=${id}`}
            className="btn btn-outline-primary rounded-3 px-3 d-inline-flex align-items-center gap-2"
          >
            <i className="bi bi-calendar3"></i>
            Xếp Lịch Chi Tiết
          </Link>
          <Link href={`/quan-ly-giai/giai-dau/${id}`} className="btn btn-primary rounded-3 px-3">
            <i className="bi bi-pencil-square me-1"></i>
            Quay Lại Sửa Giải Đấu
          </Link>
        </div>
      </div>

      {/* Thẻ Thống Kê Nhanh Theo Môn */}
      <div className="row g-3 mb-4">
        {availableMons.map((m) => {
          const stats = sportStats.get(m.monTheThaoId) || { regCount: 0, matchCount: 0 };
          const isSelected = filterMonId === m.monTheThaoId;
          const hasData = stats.regCount > 0 || stats.matchCount > 0;

          return (
            <div key={m.id} className="col-sm-6 col-md-4 col-lg-3">
              <div
                onClick={() => {
                  setFilterMonId(isSelected ? 'ALL' : m.id);
                }}
                className={`card border-0 shadow-sm rounded-4 cursor-pointer transition-all p-3 h-100 ${
                  isSelected
                    ? 'border-primary border-2 bg-primary-subtle'
                    : 'bg-white'
                }`}
                style={{ cursor: 'pointer' }}
              >
                <div className="d-flex align-items-center justify-content-between mb-2">
                  <span className="fw-bold text-dark small text-truncate">{m.ten}</span>
                  {hasData ? (
                    <Badge color="danger" pill className="small">
                      Có dữ liệu
                    </Badge>
                  ) : (
                    <Badge color="light" text="secondary" pill className="small">
                      Trống
                    </Badge>
                  )}
                </div>
                <div className="d-flex justify-content-between small text-muted">
                  <span>
                    <i className="bi bi-people me-1 text-primary"></i>
                    <strong>{stats.regCount}</strong> đăng ký
                  </span>
                  <span>
                    <i className="bi bi-trophy me-1 text-warning"></i>
                    <strong>{stats.matchCount}</strong> trận
                  </span>
                </div>
              </div>
            </div>
          );
        })}
      </div>

      {/* Card Chính: Bộ lọc và Danh sách */}
      <Card className="border-0 shadow-sm rounded-4">
        <CardBody className="p-4">
          {/* Tabs Navigation */}
          <Nav tabs className="mb-4 border-bottom">
            <NavItem>
              <NavLink
                className={`cursor-pointer fw-semibold py-2.5 px-4 ${
                  activeTab === 'dang-ky'
                    ? 'active text-primary border-primary border-bottom-2 border-top-0 border-start-0 border-end-0'
                    : 'text-muted border-0'
                }`}
                onClick={() => setActiveTab('dang-ky')}
                style={{ cursor: 'pointer' }}
              >
                <i className="bi bi-people-fill me-2"></i>
                Hồ Sơ Đăng Ký Thi Đấu ({registrations.length})
              </NavLink>
            </NavItem>
            <NavItem>
              <NavLink
                className={`cursor-pointer fw-semibold py-2.5 px-4 ${
                  activeTab === 'tran-dau'
                    ? 'active text-primary border-primary border-bottom-2 border-top-0 border-start-0 border-end-0'
                    : 'text-muted border-0'
                }`}
                onClick={() => setActiveTab('tran-dau')}
                style={{ cursor: 'pointer' }}
              >
                <i className="bi bi-calendar2-range me-2"></i>
                Lịch Thi Đấu &amp; Trận Đấu ({matches.length})
              </NavLink>
            </NavItem>
          </Nav>

          {/* Bộ Lọc */}
          <Row className="g-3 mb-4 align-items-end">
            <Col md={5}>
              <FormGroup className="mb-0">
                <Label className="small fw-semibold">Lọc theo Môn thể thao:</Label>
                <Input
                  type="select"
                  className="rounded-3 form-select-sm"
                  value={filterMonId}
                  onChange={(e) => setFilterMonId(e.target.value === 'ALL' ? 'ALL' : Number(e.target.value))}
                >
                  <option value="ALL">-- Tất cả các môn ({availableMons.length}) --</option>
                  {availableMons.map((m) => (
                    <option key={m.id} value={m.id}>
                      {m.ten}
                    </option>
                  ))}
                </Input>
              </FormGroup>
            </Col>
            <Col md={4}>
              <FormGroup className="mb-0">
                <Label className="small fw-semibold">Tìm kiếm từ khóa:</Label>
                <div className="input-group input-group-sm">
                  <span className="input-group-text bg-light border-end-0">
                    <i className="bi bi-search text-muted"></i>
                  </span>
                  <Input
                    type="text"
                    placeholder="Mã, tên đội, đoàn..."
                    className="rounded-end-3"
                    value={searchKeyword}
                    onChange={(e) => setSearchKeyword(e.target.value)}
                  />
                </div>
              </FormGroup>
            </Col>
            <Col md={3} className="text-end">
              {activeTab === 'tran-dau' && filterMonId !== 'ALL' && (
                <Button
                  color="danger"
                  size="sm"
                  outline
                  onClick={() => setClearMatchesModal(true)}
                  className="rounded-3 w-100 d-inline-flex align-items-center justify-content-center gap-1"
                  title="Xóa toàn bộ lịch thi đấu của môn đang chọn"
                >
                  <i className="bi bi-trash3"></i>
                  Xóa Hết Lịch Môn Này
                </Button>
              )}
            </Col>
          </Row>

          {/* TAB 1: DANH SÁCH ĐĂNG KÝ */}
          {activeTab === 'dang-ky' && (
            <div>
              {filteredRegistrations.length === 0 ? (
                <div className="text-center py-5 text-muted">
                  <i className="bi bi-inbox fs-1 d-block mb-2 text-secondary opacity-50"></i>
                  <p className="mb-1 fw-semibold">Không có hồ sơ đăng ký nào phù hợp</p>
                  <span className="small">
                    {filterMonId !== 'ALL'
                      ? 'Môn này hiện chưa có đội/VĐV nào đăng ký, bạn có thể yên tâm gỡ bỏ môn.'
                      : 'Chưa tìm thấy dữ liệu đăng ký thi đấu.'}
                  </span>
                </div>
              ) : (
                <div className="table-responsive">
                  <Table hover className="align-middle mb-0">
                    <thead className="table-light small">
                      <tr>
                        <th style={{ width: '50px' }}>STT</th>
                        <th>Số ĐK / Mã</th>
                        <th>Tên Đội / VĐV</th>
                        <th>Đoàn / Đơn Vị</th>
                        <th>Môn &amp; Nội Dung</th>
                        <th>Trạng Thái</th>
                        <th className="text-center" style={{ width: '100px' }}>
                          Thao Tác
                        </th>
                      </tr>
                    </thead>
                    <tbody>
                      {filteredRegistrations.map((reg, idx) => (
                        <tr key={reg.id}>
                          <td className="small text-muted">{idx + 1}</td>
                          <td>
                            <span className="font-monospace small fw-bold text-dark">
                              {reg.soDangKy || `#${reg.id}`}
                            </span>
                          </td>
                          <td>
                            <div className="fw-semibold text-dark">
                              {reg.tenDangKy || reg.tenDoi || 'Chưa đặt tên'}
                            </div>
                            {reg.soVdv !== undefined && (
                              <small className="text-muted">{reg.soVdv} VĐV</small>
                            )}
                          </td>
                          <td>
                            <span className="small text-muted">{reg.tenDonVi || 'Vãng lai'}</span>
                          </td>
                          <td>
                            <div className="small fw-semibold text-primary">{reg.tenMonTheThao}</div>
                            <small className="text-muted">{reg.tenNoiDung}</small>
                          </td>
                          <td>
                            {reg.trangThai === 'DaDuyet' ? (
                              <Badge color="success-subtle" className="text-success border border-success-subtle">
                                Đã duyệt
                              </Badge>
                            ) : reg.trangThai === 'ChoDuyet' ? (
                              <Badge color="warning-subtle" className="text-warning border border-warning-subtle">
                                Chờ duyệt
                              </Badge>
                            ) : (
                              <Badge color="secondary-subtle" className="text-secondary">
                                {reg.trangThai || 'Khác'}
                              </Badge>
                            )}
                          </td>
                          <td className="text-center">
                            <Button
                              color="outline-danger"
                              size="sm"
                              className="rounded-circle p-0 d-inline-flex align-items-center justify-content-center"
                              style={{ width: '32px', height: '32px' }}
                              onClick={() => {
                                setRegToDelete(reg);
                                setDeleteRegModal(true);
                              }}
                              title="Xóa hồ sơ đăng ký này"
                            >
                              <i className="bi bi-trash"></i>
                            </Button>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </Table>
                </div>
              )}
            </div>
          )}

          {/* TAB 2: DANH SÁCH TRẬN ĐẤU */}
          {activeTab === 'tran-dau' && (
            <div>
              {filteredMatches.length === 0 ? (
                <div className="text-center py-5 text-muted">
                  <i className="bi bi-calendar-x fs-1 d-block mb-2 text-secondary opacity-50"></i>
                  <p className="mb-1 fw-semibold">Không có trận đấu nào phù hợp</p>
                  <span className="small">
                    {filterMonId !== 'ALL'
                      ? 'Môn này hiện chưa có lịch thi đấu hoặc trận đấu nào.'
                      : 'Chưa có trận đấu nào trong danh sách lọc.'}
                  </span>
                </div>
              ) : (
                <div className="table-responsive">
                  <Table hover className="align-middle mb-0">
                    <thead className="table-light small">
                      <tr>
                        <th style={{ width: '50px' }}>STT</th>
                        <th>Trận Đấu</th>
                        <th>Nội Dung</th>
                        <th>Vòng / Bảng</th>
                        <th>Đối Đầu</th>
                        <th>Thời Gian &amp; Sân</th>
                        <th>Trạng Thái</th>
                        <th className="text-center" style={{ width: '100px' }}>
                          Thao Tác
                        </th>
                      </tr>
                    </thead>
                    <tbody>
                      {filteredMatches.map((m, idx) => (
                        <tr key={m.id}>
                          <td className="small text-muted">{idx + 1}</td>
                          <td>
                            <span className="font-monospace small fw-bold text-dark">
                              {m.tenTran || `Trận ${m.soTran || idx + 1}`}
                            </span>
                          </td>
                          <td>
                            <div className="small fw-semibold text-primary">{m.tenMonTheThao}</div>
                            <small className="text-muted">{m.tenNoiDung}</small>
                          </td>
                          <td>
                            <span className="small text-muted">
                              {m.tenVongDau || m.tenBangDau || '-'}
                            </span>
                          </td>
                          <td>
                            <div className="d-flex align-items-center gap-2">
                              <span className="fw-semibold text-dark small">{m.tenDoi1 || 'Đội 1'}</span>
                              <Badge color="light" text="dark" className="border small">
                                VS
                              </Badge>
                              <span className="fw-semibold text-dark small">{m.tenDoi2 || 'Đội 2'}</span>
                            </div>
                          </td>
                          <td>
                            <div className="small text-dark">
                              {m.thoiGianBatDau
                                ? new Date(m.thoiGianBatDau).toLocaleString('vi-VN', {
                                    hour: '2-digit',
                                    minute: '2-digit',
                                    day: '2-digit',
                                    month: '2-digit',
                                  })
                                : 'Chưa xếp giờ'}
                            </div>
                            <small className="text-muted">{m.tenSanDau || 'Chưa xếp sân'}</small>
                          </td>
                          <td>
                            <Badge color="light" text="dark" className="border small">
                              {m.trangThai || 'Chưa diễn ra'}
                            </Badge>
                          </td>
                          <td className="text-center">
                            <Button
                              color="outline-danger"
                              size="sm"
                              className="rounded-circle p-0 d-inline-flex align-items-center justify-content-center"
                              style={{ width: '32px', height: '32px' }}
                              onClick={() => {
                                setMatchToDelete(m);
                                setDeleteMatchModal(true);
                              }}
                              title="Xóa trận đấu này"
                            >
                              <i className="bi bi-trash"></i>
                            </Button>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </Table>
                </div>
              )}
            </div>
          )}
        </CardBody>
      </Card>

      {/* Modal Xác Nhận Xóa Đăng Ký */}
      <Modal isOpen={deleteRegModal} toggle={() => setDeleteRegModal(false)} centered>
        <ModalHeader toggle={() => setDeleteRegModal(false)}>
          <i className="bi bi-exclamation-triangle text-danger me-2"></i>
          Xác Nhận Xóa Hồ Sơ Đăng Ký
        </ModalHeader>
        <ModalBody>
          <p>
            Bạn có chắc chắn muốn xóa hồ sơ đăng ký{' '}
            <strong>"{regToDelete?.tenDangKy || regToDelete?.tenDoi || regToDelete?.soDangKy}"</strong>?
          </p>
          <Alert color="warning" className="small mb-0">
            <i className="bi bi-info-circle me-1"></i>
            Hành động này sẽ hủy đăng ký của đội/VĐV này trong nội dung{' '}
            <strong>{regToDelete?.tenNoiDung}</strong>. Sau khi xóa hết đăng ký và trận đấu của môn, bạn có thể gỡ bỏ môn đó khỏi giải.
          </Alert>
        </ModalBody>
        <ModalFooter>
          <Button color="light" onClick={() => setDeleteRegModal(false)} disabled={deletingReg}>
            Hủy Bỏ
          </Button>
          <Button color="danger" onClick={confirmDeleteReg} disabled={deletingReg}>
            {deletingReg ? <Spinner size="sm" /> : <i className="bi bi-trash me-1"></i>}
            Xóa Hồ Sơ
          </Button>
        </ModalFooter>
      </Modal>

      {/* Modal Xác Nhận Xóa Trận Đấu */}
      <Modal isOpen={deleteMatchModal} toggle={() => setDeleteMatchModal(false)} centered>
        <ModalHeader toggle={() => setDeleteMatchModal(false)}>
          <i className="bi bi-exclamation-triangle text-danger me-2"></i>
          Xác Nhận Xóa Trận Đấu
        </ModalHeader>
        <ModalBody>
          <p>
            Bạn có chắc chắn muốn xóa trận đấu{' '}
            <strong>"{matchToDelete?.tenTran || `Trận ${matchToDelete?.soTran}`}"</strong> ({matchToDelete?.tenDoi1} vs{' '}
            {matchToDelete?.tenDoi2})?
          </p>
          <Alert color="warning" className="small mb-0">
            <i className="bi bi-info-circle me-1"></i>
            Trận đấu sẽ bị gỡ khỏi lịch thi đấu và không còn ràng buộc với môn/nội dung này nữa.
          </Alert>
        </ModalBody>
        <ModalFooter>
          <Button color="light" onClick={() => setDeleteMatchModal(false)} disabled={deletingMatch}>
            Hủy Bỏ
          </Button>
          <Button color="danger" onClick={confirmDeleteMatch} disabled={deletingMatch}>
            {deletingMatch ? <Spinner size="sm" /> : <i className="bi bi-trash me-1"></i>}
            Xóa Trận Đấu
          </Button>
        </ModalFooter>
      </Modal>

      {/* Modal Xác Nhận Xóa Toàn Bộ Trận Của Môn */}
      <Modal isOpen={clearMatchesModal} toggle={() => setClearMatchesModal(false)} centered>
        <ModalHeader toggle={() => setClearMatchesModal(false)}>
          <i className="bi bi-shield-x text-danger me-2"></i>
          Xóa Toàn Bộ Lịch Đấu Của Môn
        </ModalHeader>
        <ModalBody>
          <p>
            Bạn có chắc chắn muốn xóa <strong>toàn bộ các trận đấu</strong> thuộc môn{' '}
            <strong className="text-danger">
              "{availableMons.find((m) => m.id === filterMonId)?.ten}"
            </strong>
            ?
          </p>
          <Alert color="danger" className="small mb-0">
            <i className="bi bi-exclamation-octagon me-1"></i>
            Tất cả các trận đấu, kết quả và phân công sân/trọng tài của môn này sẽ bị xóa bỏ hoàn toàn!
          </Alert>
        </ModalBody>
        <ModalFooter>
          <Button color="light" onClick={() => setClearMatchesModal(false)} disabled={clearingMatches}>
            Hủy Bỏ
          </Button>
          <Button color="danger" onClick={confirmClearMatches} disabled={clearingMatches}>
            {clearingMatches ? <Spinner size="sm" /> : <i className="bi bi-trash3 me-1"></i>}
            Xóa Sạch Lịch Đấu
          </Button>
        </ModalFooter>
      </Modal>
    </div>
  );
}
