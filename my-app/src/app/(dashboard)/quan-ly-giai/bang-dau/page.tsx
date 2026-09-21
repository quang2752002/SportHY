'use client';

import React, { useState, useEffect } from 'react';
import Link from 'next/link';
import {
  Layers,
  Calendar,
  Shuffle,
  Plus,
  Trash2,
  Users,
  Trophy,
  ArrowRight,
  Shield,
  RefreshCw,
  Info,
} from 'lucide-react';
import {
  Row,
  Col,
  Card,
  CardBody,
  Badge,
  Button,
  Table,
  Input,
  Label,
  Modal,
  ModalHeader,
  ModalBody,
  ModalFooter,
  FormGroup,
  Spinner,
  Alert,
} from 'reactstrap';
import { useToast } from '@/context/AuthContext';
import {
  giaiDauService,
  noiDungThiDauService,
  dangKyThiDauService,
  bangDauService,
} from '@/services';
import { GiaiDau, NoiDungThiDau, DangKyThiDau, BangDau } from '@/types';
import { HinhThucThiDauLabels } from '@/types/hinhThucThiDau';

export default function BangDauPage() {
  const toast = useToast();
  const [tournaments, setTournaments] = useState<GiaiDau[]>([]);
  const [selectedTournamentId, setSelectedTournamentId] = useState<number | ''>('');
  const [events, setEvents] = useState<NoiDungThiDau[]>([]);
  const [selectedEventId, setSelectedEventId] = useState<number | ''>('');

  const [groups, setGroups] = useState<BangDau[]>([]);
  const [registeredTeams, setRegisteredTeams] = useState<DangKyThiDau[]>([]);
  const [loading, setLoading] = useState<boolean>(false);

  // Modals
  const [createGroupModalOpen, setCreateGroupModalOpen] = useState(false);
  const [newGroupName, setNewGroupName] = useState('');
  const [distributeModalOpen, setDistributeModalOpen] = useState(false);
  const [distributeCount, setDistributeCount] = useState(2);

  // Load Tournaments
  useEffect(() => {
    const load = async () => {
      try {
        const data = await giaiDauService.getAll();
        setTournaments(data);
        if (data.length > 0) setSelectedTournamentId(data[0].id);
      } catch (err) {
        console.error(err);
      }
    };
    load();
  }, []);

  // Load Events
  useEffect(() => {
    if (!selectedTournamentId) return;
    const load = async () => {
      try {
        const data = await noiDungThiDauService.getAll({ giaiDauId: Number(selectedTournamentId) });
        setEvents(data);
        if (data.length > 0) setSelectedEventId(data[0].id);
        else setSelectedEventId('');
      } catch (err) {
        console.error(err);
      }
    };
    load();
  }, [selectedTournamentId]);

  // Load Groups & Teams
  const refreshData = async () => {
    if (!selectedEventId) {
      setGroups([]);
      setRegisteredTeams([]);
      return;
    }
    setLoading(true);
    try {
      const eId = Number(selectedEventId);
      const [groupsRes, teamsRes] = await Promise.all([
        bangDauService.getAll(eId),
        dangKyThiDauService.getAll({ noiDungThiDauId: eId }),
      ]);
      setGroups(groupsRes);
      setRegisteredTeams(teamsRes);
    } catch (err) {
      toast.error('Không thể tải dữ liệu bảng đấu.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    refreshData();
  }, [selectedEventId]);

  const currentEvent = events.find((e) => e.id === Number(selectedEventId));

  // Create Single Group
  const handleCreateGroup = async () => {
    if (!selectedEventId || !newGroupName.trim()) return;
    try {
      await bangDauService.create({
        noiDungThiDauId: Number(selectedEventId),
        ten: newGroupName.trim(),
        thuTu: groups.length + 1,
      });
      toast.success('Tạo bảng đấu thành công!');
      setNewGroupName('');
      setCreateGroupModalOpen(false);
      refreshData();
    } catch (err) {
      toast.error('Lỗi khi tạo bảng đấu.');
    }
  };

  // Delete Group
  const handleDeleteGroup = async (id: number) => {
    if (!window.confirm('Bạn có chắc muốn xóa bảng đấu này?')) return;
    try {
      await bangDauService.delete(id);
      toast.success('Đã xóa bảng đấu.');
      refreshData();
    } catch (err) {
      toast.error('Lỗi khi xóa bảng.');
    }
  };

  // Auto Distribute
  const handleAutoDistribute = async () => {
    if (!selectedEventId) return;
    try {
      await bangDauService.autoDistribute({
        noiDungThiDauId: Number(selectedEventId),
        soBang: Number(distributeCount),
        tienToBang: 'Bảng ',
      });
      toast.success(`Đã chia ${distributeCount} bảng đấu ngẫu nhiên!`);
      setDistributeModalOpen(false);
      refreshData();
    } catch (err) {
      toast.error('Lỗi khi bốc thăm chia bảng.');
    }
  };

  return (
    <div className="d-flex flex-column gap-4 pb-5">
      {/* Top Banner */}
      <div
        className="rounded-4 p-4 text-white position-relative overflow-hidden shadow-sm"
        style={{ background: 'linear-gradient(135deg, #1e1b4b 0%, #312e81 50%, #4338ca 100%)' }}
      >
        <div className="d-flex flex-column flex-md-row align-items-md-center justify-content-between gap-3">
          <div>
            <div className="d-inline-flex align-items-center gap-2 px-3 py-1 rounded-pill mb-2 border" style={{ backgroundColor: 'rgba(255,255,255,0.12)', fontSize: '12px' }}>
              <Layers size={14} className="text-warning" />
              <span className="fw-semibold">Quản Lý Bảng Đấu & Bốc Thăm Thi Đấu</span>
            </div>
            <h3 className="fw-bold mb-1 fs-4 text-white">Bảng Thi Đấu & Nhánh Đấu</h3>
            <p className="text-white-50 mb-0 small">
              Quản lý các bảng đấu, phân bổ đội và xếp thứ tự hạt giống theo thể thức giải đấu.
            </p>
          </div>

          <div className="d-flex gap-2">
            <Link
              href="/quan-ly-giai/lich-thi-dau"
              className="btn btn-warning rounded-pill px-3 py-2 fw-semibold d-flex align-items-center gap-1.5 shadow-sm text-dark"
              style={{ fontSize: '13px' }}
            >
              <Calendar size={15} />
              <span>Xếp Lịch Thi Đấu</span>
              <ArrowRight size={14} />
            </Link>
          </div>
        </div>

        {/* Selectors */}
        <div className="mt-3 pt-3 border-top border-white border-opacity-10 d-flex flex-wrap gap-3">
          <div style={{ minWidth: '220px' }}>
            <Label className="small text-white-50 mb-1 fw-medium">Chọn Giải Đấu</Label>
            <Input
              type="select"
              bsSize="sm"
              className="bg-dark text-white border-secondary rounded-3"
              value={selectedTournamentId}
              onChange={(e) => setSelectedTournamentId(e.target.value ? Number(e.target.value) : '')}
            >
              {tournaments.map((t) => (
                <option key={t.id} value={t.id}>{t.ten}</option>
              ))}
            </Input>
          </div>

          <div style={{ minWidth: '240px' }}>
            <Label className="small text-white-50 mb-1 fw-medium">Chọn Nội Dung</Label>
            <Input
              type="select"
              bsSize="sm"
              className="bg-dark text-white border-secondary rounded-3"
              value={selectedEventId}
              onChange={(e) => setSelectedEventId(e.target.value ? Number(e.target.value) : '')}
            >
              {events.map((ev) => (
                <option key={ev.id} value={ev.id}>{ev.ten} ({ev.tenMonTheThao || 'Môn'})</option>
              ))}
            </Input>
          </div>

          {currentEvent && (
            <div className="d-flex align-items-center gap-2 ms-auto mt-auto mb-1">
              <Badge color="info" pill className="px-3 py-1.5 text-white">
                {HinhThucThiDauLabels[currentEvent.hinhThucThiDau || ''] || currentEvent.hinhThucThiDau || 'Loại trực tiếp'}
              </Badge>
              <Button color="light" size="sm" className="rounded-pill px-3" onClick={refreshData}>
                <RefreshCw size={13} className={loading ? 'spin' : ''} /> Làm mới
              </Button>
            </div>
          )}
        </div>
      </div>

      {/* Toolbar */}
      <div className="d-flex align-items-center justify-content-between">
        <div className="d-flex align-items-center gap-2">
          <Button
            color="primary"
            className="rounded-pill px-3 py-2 fw-semibold d-flex align-items-center gap-1.5 shadow-sm"
            style={{ fontSize: '13px' }}
            onClick={() => setDistributeModalOpen(true)}
            disabled={!selectedEventId}
          >
            <Shuffle size={15} />
            <span>Bốc Thăm Tự Động</span>
          </Button>

          <Button
            color="outline-primary"
            className="rounded-pill px-3 py-2 fw-semibold d-flex align-items-center gap-1.5 bg-white"
            style={{ fontSize: '13px' }}
            onClick={() => {
              setNewGroupName(`Bảng ${(char => String.fromCharCode(char))('A'.charCodeAt(0) + groups.length)}`);
              setCreateGroupModalOpen(true);
            }}
            disabled={!selectedEventId}
          >
            <Plus size={15} />
            <span>Thêm Bảng Mới</span>
          </Button>
        </div>

        <div className="text-muted small">
          Tổng số đội tham gia: <strong className="text-dark">{registeredTeams.length}</strong> | Số bảng: <strong className="text-primary">{groups.length}</strong>
        </div>
      </div>

      {/* Group Cards Grid */}
      {loading ? (
        <div className="text-center py-5">
          <Spinner color="primary" />
        </div>
      ) : groups.length === 0 ? (
        <Card className="border-0 shadow-sm rounded-4 p-5 text-center">
          <Layers size={48} className="text-secondary opacity-25 mx-auto mb-2" />
          <h6 className="fw-bold text-dark">Chưa có bảng đấu nào cho nội dung này</h6>
          <p className="text-muted small mb-3">Nhấn "Bốc Thăm Tự Động" để chia các đội vào các bảng.</p>
          <div>
            <Button color="primary" size="sm" className="rounded-pill px-3 py-1.5" onClick={() => setDistributeModalOpen(true)}>
              <Shuffle size={14} className="me-1" /> Bốc Thăm Chia Bảng
            </Button>
          </div>
        </Card>
      ) : (
        <Row className="g-4">
          {groups.map((g) => (
            <Col key={g.id} xs={12} md={6}>
              <Card className="border-0 shadow-sm rounded-4 h-100">
                <CardBody className="p-4">
                  <div className="d-flex align-items-center justify-content-between mb-3 pb-2 border-bottom">
                    <div className="d-flex align-items-center gap-2">
                      <Badge color="warning" pill className="px-3 py-1 text-dark fw-bold fs-6">
                        {g.ten}
                      </Badge>
                      <span className="text-muted small">({g.thanhViens?.length || 0} đội)</span>
                    </div>

                    <Button
                      color="light"
                      size="sm"
                      className="p-1 text-danger rounded-2 border"
                      title="Xóa bảng này"
                      onClick={() => handleDeleteGroup(g.id)}
                    >
                      <Trash2 size={14} />
                    </Button>
                  </div>

                  <div className="table-responsive">
                    <Table size="sm" className="align-middle mb-0" style={{ fontSize: '13px' }}>
                      <thead className="table-light">
                        <tr>
                          <th style={{ width: '40px' }}>STT</th>
                          <th>Tên Đội / VĐV</th>
                          <th>Đơn Vị</th>
                          <th className="text-center">Số Trận</th>
                          <th className="text-center">Điểm</th>
                        </tr>
                      </thead>
                      <tbody>
                        {g.thanhViens && g.thanhViens.length > 0 ? (
                          g.thanhViens.map((tv, idx) => (
                            <tr key={tv.id}>
                              <td className="fw-bold text-muted">{idx + 1}</td>
                              <td className="fw-semibold text-dark">{tv.tenDoi || tv.tenDangKy}</td>
                              <td className="text-muted small">{tv.tenDonVi || '--'}</td>
                              <td className="text-center font-monospace">{tv.soTran}</td>
                              <td className="text-center fw-bold text-primary font-monospace">{tv.diem}</td>
                            </tr>
                          ))
                        ) : (
                          <tr>
                            <td colSpan={5} className="text-center py-3 text-muted">
                              Chưa có đội trong bảng này
                            </td>
                          </tr>
                        )}
                      </tbody>
                    </Table>
                  </div>
                </CardBody>
              </Card>
            </Col>
          ))}
        </Row>
      )}

      {/* Modal Auto Distribute */}
      <Modal isOpen={distributeModalOpen} toggle={() => setDistributeModalOpen(!distributeModalOpen)} centered>
        <ModalHeader toggle={() => setDistributeModalOpen(!distributeModalOpen)}>
          <div className="d-flex align-items-center gap-2">
            <Shuffle size={18} className="text-primary" />
            <span className="fw-bold fs-6">Bốc Thăm Chia Bảng Tự Động</span>
          </div>
        </ModalHeader>
        <ModalBody className="p-4">
          <p className="small text-muted mb-3">
            Hệ thống sẽ tạo số lượng bảng đấu theo yêu cầu và chia đều ngẫu nhiên {registeredTeams.length} đội tham gia.
          </p>
          <FormGroup>
            <Label className="small fw-semibold">Số lượng bảng đấu:</Label>
            <Input
              type="select"
              bsSize="sm"
              value={distributeCount}
              onChange={(e) => setDistributeCount(Number(e.target.value))}
            >
              <option value={2}>2 bảng (Bảng A, B)</option>
              <option value={3}>3 bảng (Bảng A, B, C)</option>
              <option value={4}>4 bảng (Bảng A, B, C, D)</option>
              <option value={6}>6 bảng (Bảng A - F)</option>
              <option value={8}>8 bảng (Bảng A - H)</option>
            </Input>
          </FormGroup>
        </ModalBody>
        <ModalFooter>
          <Button color="secondary" size="sm" className="rounded-pill px-3" onClick={() => setDistributeModalOpen(false)}>
            Hủy
          </Button>
          <Button color="primary" size="sm" className="rounded-pill px-4 fw-semibold" onClick={handleAutoDistribute}>
            Bắt Đầu Bốc Thăm
          </Button>
        </ModalFooter>
      </Modal>

      {/* Modal Create Single Group */}
      <Modal isOpen={createGroupModalOpen} toggle={() => setCreateGroupModalOpen(!createGroupModalOpen)} centered>
        <ModalHeader toggle={() => setCreateGroupModalOpen(!createGroupModalOpen)}>
          <span className="fw-bold fs-6">Thêm Bảng Đấu Mới</span>
        </ModalHeader>
        <ModalBody className="p-4">
          <FormGroup>
            <Label className="small fw-semibold">Tên bảng đấu *</Label>
            <Input
              type="text"
              bsSize="sm"
              placeholder="Ví dụ: Bảng A"
              value={newGroupName}
              onChange={(e) => setNewGroupName(e.target.value)}
            />
          </FormGroup>
        </ModalBody>
        <ModalFooter>
          <Button color="secondary" size="sm" className="rounded-pill px-3" onClick={() => setCreateGroupModalOpen(false)}>
            Hủy
          </Button>
          <Button color="primary" size="sm" className="rounded-pill px-4 fw-semibold" onClick={handleCreateGroup}>
            Tạo Bảng
          </Button>
        </ModalFooter>
      </Modal>
    </div>
  );
}
