'use client';

import React, { useState, useEffect, useMemo, useCallback } from 'react';
import Link from 'next/link';
import { useRouter, useSearchParams } from 'next/navigation';
import { useAuth, useToast } from '@/context/AuthContext';
import {
  Flame,
  CheckCircle2,
  Clock,
  Calendar,
  AlertTriangle,
  Play,
  Pause,
  RotateCcw,
  FileText,
  MapPin,
  Users,
  Shield,
  Plus,
  Trash2,
  Save,
  ChevronLeft,
  Award,
  AlertCircle,
  Flag,
  UserCheck,
  Check,
  Edit2,
  CloudSun,
  Activity,
  Sparkles,
  Lock,
  Unlock,
} from 'lucide-react';
import {
  Row,
  Col,
  Card,
  CardBody,
  Badge,
  Button,
  Input,
  FormGroup,
  Label,
  Table,
  Modal,
  ModalHeader,
  ModalBody,
  ModalFooter,
  Spinner,
  Alert,
  Nav,
  NavItem,
  NavLink,
} from 'reactstrap';
import { giaiDauService } from '@/services/giaiDauService';
import { tranDauService } from '@/services/tranDauService';
import { trongTaiService } from '@/services/trongTaiService';
import { GiaiDau, TrangThaiGiaiDau } from '@/types/giaiDau';

import { TranDau, MatchEvent, SetScore, MatchScoreDetails } from '@/types/tranDau';
import { TrongTai } from '@/types/trongTai';
import { refereeScoreStorage } from '@/utils/refereeScoreStorage';

export default function RecordMatchResultPage() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const { user } = useAuth();
  const toast = useToast();

  const queryTranId = searchParams.get('tranDauId');
  const queryGiaiId = searchParams.get('giaiDauId');

  // Danh mục dữ liệu
  const [giaiDaus, setGiaiDaus] = useState<GiaiDau[]>([]);
  const [selectedGiaiDauId, setSelectedGiaiDauId] = useState<number | ''>('');
  const [matches, setMatches] = useState<TranDau[]>([]);
  const [allReferees, setAllReferees] = useState<TrongTai[]>([]);
  const [selectedTranDauId, setSelectedTranDauId] = useState<number | ''>('');
  const [currentMatch, setCurrentMatch] = useState<TranDau | null>(null);

  const [loading, setLoading] = useState(true);
  const [loadingMatches, setLoadingMatches] = useState(false);
  const [saving, setSaving] = useState(false);

  // Tab điều hành: 'score_events' (Tỷ số & Diễn biến) | 'finalize' (Hoàn thiện thông tin)
  const [activeTab, setActiveTab] = useState<'score_events' | 'finalize'>('score_events');
  const [onlyMyMatches, setOnlyMyMatches] = useState<boolean>(false);

  // Trọng tài đang thao tác (nhận diện từ user hoặc cho phép chọn nếu là admin/test)
  const [activeRefereeId, setActiveRefereeId] = useState<number | ''>('');

  // Trạng thái tỷ số & sự kiện trận đấu
  const [score1, setScore1] = useState<number>(0);
  const [score2, setScore2] = useState<number>(0);
  const [matchStatus, setMatchStatus] = useState<string>('ChuaDau');
  const [setScores, setSetScores] = useState<SetScore[]>([
    { setNumber: 1, score1: 0, score2: 0 },
  ]);
  const [events, setEvents] = useState<MatchEvent[]>([]);
  const [winner, setWinner] = useState<1 | 2 | 'draw' | undefined>(undefined);
  const [matchNotes, setMatchNotes] = useState<string>('');

  // Thông tin hoàn thiện trận đấu (Finalize match)
  const [actualStartTime, setActualStartTime] = useState<string>('');
  const [actualEndTime, setActualEndTime] = useState<string>('');
  const [durationMinutes, setDurationMinutes] = useState<number>(90);
  const [extraTimeMinutes, setExtraTimeMinutes] = useState<number>(0);
  const [weatherCondition, setWeatherCondition] = useState<string>('Nắng ráo, điều kiện thi đấu tốt');
  const [pitchCondition, setPitchCondition] = useState<string>('Mặt sân đạt chuẩn, an toàn');
  const [spectatorCount, setSpectatorCount] = useState<number>(0);
  const [mvpAthlete, setMvpAthlete] = useState<string>('');
  const [winMethod, setWinMethod] = useState<MatchScoreDetails['winMethod']>('normal');
  const [refereeNotes, setRefereeNotes] = useState<string>('');
  const [supervisorNotes, setSupervisorNotes] = useState<string>('');
  const [isFinalized, setIsFinalized] = useState<boolean>(false);
  const [finalizedAt, setFinalizedAt] = useState<string>('');
  const [finalizedBy, setFinalizedBy] = useState<string>('');

  // Modal thêm / sửa sự kiện
  const [eventModalOpen, setEventModalOpen] = useState(false);
  const [editingEventId, setEditingEventId] = useState<string | null>(null);
  const [eventTeam, setEventTeam] = useState<1 | 2>(1);
  const [eventType, setEventType] = useState<MatchEvent['type']>('goal');
  const [eventMinute, setEventMinute] = useState<number>(1);
  const [eventAthlete, setEventAthlete] = useState<string>('');
  const [eventAssist, setEventAssist] = useState<string>('');
  const [eventDetails, setEventDetails] = useState<string>('');

  // 1. Tải danh mục giải đấu & trọng tài
  useEffect(() => {
    const fetchInitData = async () => {
      setLoading(true);
      try {
        const [giaiRes, refRes] = await Promise.allSettled([
          giaiDauService.getAll(),
          trongTaiService.getAll(),
        ]);

        let list: GiaiDau[] = [];
        if (giaiRes.status === 'fulfilled' && giaiRes.value && giaiRes.value.length > 0) {
          list = giaiRes.value;
        } else {
          try {
            const paged = await giaiDauService.getPaged({ pageSize: 100 });
            if (paged?.items) list = paged.items;
          } catch {}
        }
        setGiaiDaus(list);

        if (queryGiaiId && list.some((g) => g.id === Number(queryGiaiId))) {
          setSelectedGiaiDauId(Number(queryGiaiId));
        } else if (list.length > 0) {
          const active =
            list.find(
              (g) =>
                g.trangThai === TrangThaiGiaiDau.DangDienRa ||
                (g.trangThai as any) === 'DangDienRa'
            ) || list[0];
          setSelectedGiaiDauId(active.id);
        }

        if (refRes.status === 'fulfilled' && refRes.value) {
          const refs = refRes.value;
          setAllReferees(refs);

          // Nhận diện trọng tài hiện tại
          if (user?.trongTaiId) {
            setActiveRefereeId(user.trongTaiId);
          } else if (user) {
            const matchRef = refs.find(
              (r) =>
                r.hoTen?.toLowerCase() === user.fullName?.toLowerCase() ||
                r.ma?.toLowerCase() === user.username?.toLowerCase()
            );
            if (matchRef) setActiveRefereeId(matchRef.id);
          }
        }
      } catch (err) {
        console.error(err);
        toast.error('Không thể tải danh sách giải đấu');
      } finally {
        setLoading(false);
      }
    };

    fetchInitData();
  }, [queryGiaiId, user, toast]);

  // 2. Tải danh sách trận đấu khi đổi giải đấu
  useEffect(() => {
    const fetchMatches = async () => {
      if (!selectedGiaiDauId) {
        setMatches([]);
        return;
      }
      setLoadingMatches(true);
      try {
        const data = (await tranDauService.getAll({ giaiDauId: Number(selectedGiaiDauId) })) || [];
        setMatches(data);

        // Nếu có queryTranId, chọn trận đó
        if (queryTranId && data.some((m) => m.id === Number(queryTranId))) {
          setSelectedTranDauId(Number(queryTranId));
        } else if (data.length > 0) {
          // Ưu tiên chọn trận đang diễn ra
          const live = data.find((m) => m.trangThai === 'DangDienRa');
          setSelectedTranDauId(live ? live.id : data[0].id);
        } else {
          setSelectedTranDauId('');
          setCurrentMatch(null);
        }
      } catch (err) {
        console.error(err);
        toast.error('Không thể tải lịch thi đấu');
      } finally {
        setLoadingMatches(false);
      }
    };

    fetchMatches();
  }, [selectedGiaiDauId, queryTranId, toast]);

  // Trọng tài hiện tại
  const currentReferee = useMemo(() => {
    if (!activeRefereeId) return null;
    return allReferees.find((r) => r.id === activeRefereeId) || null;
  }, [activeRefereeId, allReferees]);

  // Kiểm tra trận có được phân công cho trọng tài đang thao tác
  const isMatchAssignedToMe = useCallback(
    (m: TranDau) => {
      if (!currentReferee) return true; // Nếu chưa nhận diện thì không loại trừ
      const refList = m.danhSachTrongTai || [];
      return refList.some((r) => r.trongTaiId === currentReferee.id);
    },
    [currentReferee]
  );

  // Danh sách trận đấu hiển thị (có hỗ trợ lọc chỉ trận của tôi)
  const displayMatches = useMemo(() => {
    if (!onlyMyMatches) return matches;
    return matches.filter(isMatchAssignedToMe);
  }, [matches, onlyMyMatches, isMatchAssignedToMe]);

  // 3. Khi chọn trận đấu cụ thể, nạp dữ liệu tỷ số, sự kiện & thông tin hoàn thiện
  useEffect(() => {
    if (!selectedTranDauId) {
      setCurrentMatch(null);
      return;
    }

    const found = matches.find((m) => m.id === Number(selectedTranDauId));
    if (found) {
      setCurrentMatch(found);
      setMatchStatus(found.trangThai || 'ChuaDau');

      // Đọc từ storage / ghiChu
      const details = refereeScoreStorage.getScoreDetails(found);
      setScore1(details.score1);
      setScore2(details.score2);
      setSetScores(
        details.setScores && details.setScores.length > 0
          ? details.setScores
          : [{ setNumber: 1, score1: details.score1, score2: details.score2 }]
      );
      setEvents(details.events || []);
      setWinner(details.winner);
      setMatchNotes(details.notes || '');

      // Thông tin hoàn thiện trận đấu
      setActualStartTime(details.actualStartTime || found.thoiGianBatDau || '');
      setActualEndTime(details.actualEndTime || found.thoiGianKetThuc || '');
      setDurationMinutes(details.durationMinutes || 90);
      setExtraTimeMinutes(details.extraTimeMinutes || 0);
      setWeatherCondition(details.weatherCondition || 'Nắng ráo, điều kiện thi đấu tốt');
      setPitchCondition(details.pitchCondition || 'Mặt sân đạt chuẩn, an toàn');
      setSpectatorCount(details.spectatorCount || 0);
      setMvpAthlete(details.mvpAthlete || '');
      setWinMethod(details.winMethod || 'normal');
      setRefereeNotes(details.refereeNotes || '');
      setSupervisorNotes(details.supervisorNotes || '');
      setIsFinalized(!!details.isFinalized);
      setFinalizedAt(details.finalizedAt || '');
      setFinalizedBy(details.finalizedBy || '');
    }
  }, [selectedTranDauId, matches]);

  // Vai trò của trọng tài hiện tại trong trận đang chọn
  const myRoleInCurrentMatch = useMemo(() => {
    if (!currentMatch || !currentReferee) return null;
    return (currentMatch.danhSachTrongTai || []).find(
      (r) => r.trongTaiId === currentReferee.id
    );
  }, [currentMatch, currentReferee]);

  // Tự động tính toán người thắng theo tỷ số chung cuộc
  const calculatedWinner = useMemo(() => {
    if (score1 > score2) return 1;
    if (score2 > score1) return 2;
    return 'draw';
  }, [score1, score2]);

  // Điều chỉnh tỷ số chung cuộc
  const handleScoreChange = (team: 1 | 2, delta: number) => {
    if (team === 1) {
      const next = Math.max(0, score1 + delta);
      setScore1(next);
      // Cập nhật hiệp cuối
      setSetScores((prev) => {
        if (!prev.length) return [{ setNumber: 1, score1: next, score2 }];
        const updated = [...prev];
        const lastIdx = updated.length - 1;
        updated[lastIdx] = {
          ...updated[lastIdx],
          score1: Math.max(0, updated[lastIdx].score1 + delta),
        };
        return updated;
      });
    } else {
      const next = Math.max(0, score2 + delta);
      setScore2(next);
      // Cập nhật hiệp cuối
      setSetScores((prev) => {
        if (!prev.length) return [{ setNumber: 1, score1, score2: next }];
        const updated = [...prev];
        const lastIdx = updated.length - 1;
        updated[lastIdx] = {
          ...updated[lastIdx],
          score2: Math.max(0, updated[lastIdx].score2 + delta),
        };
        return updated;
      });
    }
  };

  // Thêm hiệp mới
  const handleAddSet = () => {
    setSetScores((prev) => [
      ...prev,
      { setNumber: prev.length + 1, score1: 0, score2: 0 },
    ]);
  };

  // Xóa hiệp
  const handleRemoveSet = (index: number) => {
    if (setScores.length <= 1) return;
    setSetScores((prev) => prev.filter((_, i) => i !== index));
  };

  // Cập nhật tỷ số từng hiệp
  const handleSetScoreChange = (index: number, team: 1 | 2, val: number) => {
    setSetScores((prev) => {
      const updated = [...prev];
      updated[index] = {
        ...updated[index],
        [team === 1 ? 'score1' : 'score2']: Math.max(0, val),
      };
      // Tự động tính tổng điểm nếu môn tính tổng điểm các hiệp
      const total1 = updated.reduce((sum, s) => sum + s.score1, 0);
      const total2 = updated.reduce((sum, s) => sum + s.score2, 0);
      setScore1(total1);
      setScore2(total2);
      return updated;
    });
  };

  // Mở modal thêm sự kiện
  const handleOpenAddEvent = (type: MatchEvent['type'], team: 1 | 2 = 1) => {
    setEditingEventId(null);
    setEventType(type);
    setEventTeam(team);
    setEventAthlete('');
    setEventAssist('');
    setEventDetails('');
    setEventMinute(1);
    setEventModalOpen(true);
  };

  // Mở modal sửa sự kiện
  const handleOpenEditEvent = (ev: MatchEvent) => {
    setEditingEventId(ev.id);
    setEventType(ev.type);
    setEventTeam(ev.team);
    setEventAthlete(ev.athleteName || '');
    setEventAssist(ev.assistName || '');
    setEventDetails(ev.details || '');
    setEventMinute(ev.minute || 1);
    setEventModalOpen(true);
  };

  // Lưu sự kiện mới hoặc sửa
  const handleSaveEvent = (e: React.FormEvent) => {
    e.preventDefault();

    if (editingEventId) {
      // Cập nhật sự kiện cũ
      setEvents((prev) =>
        prev
          .map((ev) =>
            ev.id === editingEventId
              ? {
                  ...ev,
                  minute: Number(eventMinute) || 1,
                  type: eventType,
                  team: eventTeam,
                  athleteName: eventAthlete.trim() || undefined,
                  assistName: eventAssist.trim() || undefined,
                  details: eventDetails.trim() || undefined,
                }
              : ev
          )
          .sort((a, b) => a.minute - b.minute)
      );
      toast.success('Đã cập nhật sự kiện trận đấu');
    } else {
      // Thêm sự kiện mới
      const newEv: MatchEvent = {
        id: `${Date.now()}_${Math.random().toString(36).substr(2, 5)}`,
        minute: Number(eventMinute) || 1,
        type: eventType,
        team: eventTeam,
        athleteName: eventAthlete.trim() || undefined,
        assistName: eventAssist.trim() || undefined,
        details: eventDetails.trim() || undefined,
        timestamp: new Date().toISOString(),
      };

      // Nếu là sự kiện ghi bàn/điểm thì tự động cộng 1 điểm cho đội
      if (eventType === 'goal' || eventType === 'point') {
        handleScoreChange(eventTeam, 1);
      }

      setEvents((prev) => [...prev, newEv].sort((a, b) => a.minute - b.minute));
      toast.success('Đã thêm sự kiện vào diễn biến trận đấu');
    }

    setEventModalOpen(false);
  };

  // Xóa sự kiện
  const handleDeleteEvent = (id: string) => {
    setEvents((prev) => prev.filter((ev) => ev.id !== id));
    toast.info('Đã xóa sự kiện');
  };

  // Lưu toàn bộ kết quả lên hệ thống & API
  const handleSaveAllResults = async (
    finishMatch: boolean = false,
    markFinalized: boolean = false
  ) => {
    if (!currentMatch) return;

    setSaving(true);
    try {
      const nextStatus = finishMatch ? 'DaKetThuc' : matchStatus;
      const effectiveWinner = winner !== undefined ? winner : calculatedWinner;
      const nowIso = new Date().toISOString();

      const existingStored = refereeScoreStorage.getScoreDetails(currentMatch);

      const scoreDetails: MatchScoreDetails = {
        score1,
        score2,
        winner: effectiveWinner,
        setScores,
        events,
        notes: matchNotes,

        // Thông tin hoàn thiện trận đấu
        actualStartTime:
          actualStartTime || currentMatch.thoiGianBatDau || (nextStatus === 'DangDienRa' ? nowIso : undefined),
        actualEndTime: finishMatch ? (actualEndTime || nowIso) : actualEndTime,
        durationMinutes: Number(durationMinutes) || 90,
        extraTimeMinutes: Number(extraTimeMinutes) || 0,
        weatherCondition,
        pitchCondition,
        spectatorCount: Number(spectatorCount) || 0,
        mvpAthlete,
        winMethod,
        refereeNotes,
        supervisorNotes,
        isFinalized: markFinalized ? true : isFinalized,
        finalizedAt: markFinalized ? nowIso : finalizedAt,
        finalizedBy: markFinalized
          ? user?.fullName || user?.username || 'Trọng tài điều hành'
          : finalizedBy,

        // Bảo toàn chữ ký nếu có
        signedReferee: existingStored.signedReferee,
        signedSecretary: existingStored.signedSecretary,
        signedTeam1: existingStored.signedTeam1,
        signedTeam2: existingStored.signedTeam2,
      };

      // 1. Lưu vào storage client
      refereeScoreStorage.saveScoreDetails(currentMatch.id, scoreDetails);

      // 2. Serialize JSON bền vững để lưu vào database qua field tran.ghiChu
      const serializedJson = refereeScoreStorage.serializeDetails(scoreDetails);

      // 3. Gửi cập nhật API backend
      await tranDauService.update(currentMatch.id, {
        giaiDauMonTheThaoId: currentMatch.giaiDauMonTheThaoId,
        vongDauId: currentMatch.vongDauId,
        bangDauId: currentMatch.bangDauId,
        sanDauId: currentMatch.sanDauId,
        soTran: currentMatch.soTran,
        tenTran: currentMatch.tenTran,
        thoiGianDuKien: currentMatch.thoiGianDuKien,
        thoiGianBatDau: scoreDetails.actualStartTime || currentMatch.thoiGianBatDau,
        thoiGianKetThuc: scoreDetails.actualEndTime || currentMatch.thoiGianKetThuc,
        trangThai: nextStatus,
        ghiChu: serializedJson, // lưu cấu trúc đầy đủ
        doi1DangKyId: currentMatch.doi1DangKyId,
        doi2DangKyId: currentMatch.doi2DangKyId,
        danhSachTrongTai: (currentMatch.danhSachTrongTai || []).map((r) => ({
          trongTaiId: r.trongTaiId,
          vaiTro: r.vaiTro,
          ghiChu: r.ghiChu,
        })),
      });

      setMatchStatus(nextStatus);
      if (markFinalized) {
        setIsFinalized(true);
        setFinalizedAt(nowIso);
        setFinalizedBy(user?.fullName || user?.username || 'Trọng tài');
      }

      toast.success(
        markFinalized
          ? 'Đã hoàn thiện thông tin và chốt kết quả trận đấu thành công!'
          : finishMatch
          ? 'Đã kết thúc trận đấu và lưu kết quả chính thức thành công!'
          : 'Đã lưu cập nhật tỷ số & diễn biến trận đấu thành công!'
      );

      // Cập nhật lại danh sách trận trong state
      setMatches((prev) =>
        prev.map((m) =>
          m.id === currentMatch.id
            ? {
                ...m,
                trangThai: nextStatus,
                ghiChu: serializedJson,
                thoiGianBatDau: scoreDetails.actualStartTime,
                thoiGianKetThuc: scoreDetails.actualEndTime,
              }
            : m
        )
      );
    } catch (err: any) {
      console.error(err);
      toast.error('Lỗi khi lưu kết quả trận đấu');
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="d-flex flex-column gap-4 pb-5">
      {/* ── Header Navigation ── */}
      <div className="d-flex flex-wrap align-items-center justify-content-between gap-3">
        <div className="d-flex align-items-center gap-3">
          <Link
            href="/trong-tai"
            className="btn btn-outline-secondary btn-sm rounded-circle p-2 d-flex align-items-center justify-content-center shadow-sm"
            title="Quay lại danh sách phân công"
            style={{ width: '38px', height: '38px' }}
          >
            <ChevronLeft size={20} />
          </Link>
          <div>
            <div className="d-flex align-items-center gap-2 mb-1">
              <h4 className="fw-bold text-dark mb-0 d-flex align-items-center gap-2">
                <Flame size={24} className="text-danger" />
                <span>Điều Hành & Ghi Nhận Trận Đấu</span>
              </h4>
              {isFinalized && (
                <span className="badge bg-success-subtle text-success border border-success-subtle px-2.5 py-1 rounded-pill d-flex align-items-center gap-1 small fw-semibold">
                  <Lock size={12} />
                  Đã hoàn thiện
                </span>
              )}
            </div>
            <div className="d-flex flex-wrap align-items-center gap-2 text-muted small">
              <span>Cập nhật tỷ số, diễn biến thời gian thực và hoàn thiện hồ sơ trận đấu</span>
              {currentReferee && (
                <>
                  <span>•</span>
                  <span className="badge bg-light text-dark border">
                    <UserCheck size={12} className="text-success me-1" />
                    Trọng tài: <strong>{currentReferee.hoTen}</strong>
                  </span>
                </>
              )}
            </div>
          </div>
        </div>

        {currentMatch && (
          <div className="d-flex flex-wrap align-items-center gap-2">
            <Link
              href={`/trong-tai/bien-ban?tranDauId=${currentMatch.id}&giaiDauId=${selectedGiaiDauId}`}
              className="btn btn-light border btn-sm rounded-pill px-3 py-1.5 d-flex align-items-center gap-1.5 fw-semibold text-secondary shadow-sm"
            >
              <FileText size={15} />
              <span>Xem & Ký Biên Bản</span>
            </Link>

            <Button
              color="primary"
              size="sm"
              className="rounded-pill px-3.5 py-1.5 d-flex align-items-center gap-1.5 fw-bold shadow-sm"
              onClick={() => handleSaveAllResults(false, false)}
              disabled={saving}
            >
              {saving ? <Spinner size="sm" /> : <Save size={15} />}
              <span>Lưu Dữ Liệu</span>
            </Button>
          </div>
        )}
      </div>

      {/* ── Bộ chọn giải & trận đấu ── */}
      <Card className="border-0 shadow-sm rounded-4">
        <CardBody className="p-3 p-md-4">
          <Row className="g-3 align-items-end">
            <Col xs={12} md={4}>
              <FormGroup className="mb-0">
                <Label className="fw-semibold small text-secondary mb-1">
                  1. Chọn Giải Đấu
                </Label>
                <Input
                  type="select"
                  className="rounded-3"
                  value={selectedGiaiDauId}
                  onChange={(e) => setSelectedGiaiDauId(Number(e.target.value))}
                >
                  {giaiDaus.map((g) => (
                    <option key={g.id} value={g.id}>
                      {g.ten} {g.trangThai === TrangThaiGiaiDau.DangDienRa || (g.trangThai as any) === 'DangDienRa' ? '(Đang diễn ra)' : ''}
                    </option>
                  ))}
                </Input>
              </FormGroup>
            </Col>

            <Col xs={12} md={6}>
              <FormGroup className="mb-0">
                <div className="d-flex align-items-center justify-content-between mb-1">
                  <Label className="fw-semibold small text-secondary mb-0">
                    2. Chọn Trận Đấu Cần Điều Hành & Ghi Điểm
                  </Label>
                  <button
                    type="button"
                    onClick={() => setOnlyMyMatches(!onlyMyMatches)}
                    className={`btn btn-link btn-sm p-0 text-decoration-none small ${
                      onlyMyMatches ? 'fw-bold text-success' : 'text-muted'
                    }`}
                    style={{ fontSize: '11.5px' }}
                  >
                    <UserCheck size={12} className="me-1" />
                    {onlyMyMatches ? 'Đang lọc: Trận của tôi' : 'Lọc: Trận của tôi'}
                  </button>
                </div>
                <Input
                  type="select"
                  className="rounded-3"
                  value={selectedTranDauId}
                  onChange={(e) => setSelectedTranDauId(e.target.value ? Number(e.target.value) : '')}
                  disabled={loadingMatches || displayMatches.length === 0}
                >
                  {displayMatches.length === 0 ? (
                    <option value="">
                      {onlyMyMatches
                        ? '-- Không có trận nào được phân công cho bạn --'
                        : '-- Giải đấu này chưa có trận đấu nào --'}
                    </option>
                  ) : (
                    displayMatches.map((m) => {
                      const isMine = isMatchAssignedToMe(m);
                      return (
                        <option key={m.id} value={m.id}>
                          {isMine ? '★ ' : ''}[Trận #{m.soTran || m.id}] {m.tenMonTheThao} • {m.tenDoi1 || 'Đội 1'} vs{' '}
                          {m.tenDoi2 || 'Đội 2'} (
                          {m.trangThai === 'DangDienRa'
                            ? 'Đang đấu'
                            : m.trangThai === 'DaKetThuc'
                            ? 'Đã kết thúc'
                            : 'Chưa đấu'}
                          )
                        </option>
                      );
                    })
                  )}
                </Input>
              </FormGroup>
            </Col>

            <Col xs={12} md={2}>
              <div className="d-flex align-items-center gap-1.5 h-100 justify-content-md-end">
                {myRoleInCurrentMatch && (
                  <div
                    className="badge bg-success-subtle text-success border border-success-subtle px-3 py-2 rounded-3 w-100 text-center"
                    style={{ fontSize: '12px' }}
                  >
                    <strong>
                      {myRoleInCurrentMatch.vaiTro === 'TrongTaiChinh'
                        ? '★ TT Chính'
                        : myRoleInCurrentMatch.vaiTro === 'TroLy1'
                        ? 'Trợ lý 1'
                        : myRoleInCurrentMatch.vaiTro === 'TroLy2'
                        ? 'Trợ lý 2'
                        : myRoleInCurrentMatch.vaiTro === 'TrongTaiBan'
                        ? 'TT Bàn'
                        : 'Tổ Trọng Tài'}
                    </strong>
                  </div>
                )}
              </div>
            </Col>
          </Row>
        </CardBody>
      </Card>

      {!currentMatch ? (
        <Card className="border-0 shadow-sm rounded-4 text-center py-5">
          <CardBody>
            <Clock size={48} className="text-muted opacity-50 mb-3" />
            <h6 className="fw-bold text-dark">Chưa chọn trận đấu nào</h6>
            <p className="text-muted small mb-0">
              Vui lòng chọn giải đấu và trận đấu ở thanh phía trên để tiến hành ghi điểm và hoàn thiện hồ sơ.
            </p>
          </CardBody>
        </Card>
      ) : (
        <>
          {/* ── BẢNG TỶ SỐ ĐIỆN TỬ (SCOREBOARD) ── */}
          <div
            className="rounded-4 p-4 p-md-5 text-white shadow-sm position-relative overflow-hidden"
            style={{
              background: 'linear-gradient(135deg, #0f172a 0%, #1e293b 50%, #064e3b 100%)',
            }}
          >
            {/* Top Match Info */}
            <div className="d-flex flex-wrap align-items-center justify-content-between gap-3 mb-4 pb-3 border-bottom border-white border-opacity-10">
              <div className="d-flex align-items-center gap-2 flex-wrap">
                <span className="badge bg-warning text-dark fw-bold px-2.5 py-1 rounded-pill">
                  Trận #{currentMatch.soTran || currentMatch.id}
                </span>
                <span className="fw-bold text-white fs-6">
                  {currentMatch.tenMonTheThao}
                </span>
                {currentMatch.tenVongDau && (
                  <span className="text-white-50 small">• {currentMatch.tenVongDau}</span>
                )}
                {currentMatch.tenBangDau && (
                  <span className="badge bg-white bg-opacity-20 text-white small">
                    {currentMatch.tenBangDau}
                  </span>
                )}
              </div>

              {/* Trạng thái trận */}
              <div className="d-flex align-items-center gap-2">
                <span className="text-white-50 small">Trạng thái:</span>
                <span
                  className={`badge rounded-pill px-3 py-1.5 fw-bold ${
                    matchStatus === 'DangDienRa'
                      ? 'bg-danger text-white'
                      : matchStatus === 'DaKetThuc'
                      ? 'bg-secondary text-white'
                      : 'bg-primary text-white'
                  }`}
                >
                  {matchStatus === 'DangDienRa'
                    ? '🔴 ĐANG DIỄN RA'
                    : matchStatus === 'DaKetThuc'
                    ? '✓ ĐÃ KẾT THÚC'
                    : 'CHƯA ĐẤU'}
                </span>
                {isFinalized && (
                  <span className="badge bg-success text-white rounded-pill px-2.5 py-1">
                    ĐÃ CHỐT HỒ SƠ
                  </span>
                )}
              </div>
            </div>

            {/* Score Display */}
            <Row className="align-items-center text-center g-4 my-2">
              {/* Đội 1 */}
              <Col xs={12} md={5}>
                <div className="p-3 rounded-4 bg-white bg-opacity-5 border border-white border-opacity-10">
                  <h4 className="fw-bold text-white mb-1 text-truncate">
                    {currentMatch.tenDoi1 || 'Đội 1'}
                  </h4>
                  <small className="text-white-50 d-block mb-3 text-truncate">
                    {currentMatch.donViDoi1 || 'Đoàn VĐV'}
                  </small>

                  {/* Nút cộng trừ điểm */}
                  <div className="d-flex align-items-center justify-content-center gap-2">
                    <button
                      type="button"
                      onClick={() => handleScoreChange(1, -1)}
                      className="btn btn-outline-light btn-sm rounded-circle fw-bold"
                      style={{ width: '38px', height: '38px' }}
                      title="Trừ 1 điểm"
                    >
                      -1
                    </button>
                    <button
                      type="button"
                      onClick={() => handleScoreChange(1, 1)}
                      className="btn btn-success btn-sm rounded-circle fw-bold shadow-sm"
                      style={{ width: '42px', height: '42px' }}
                      title="Cộng 1 điểm"
                    >
                      +1
                    </button>
                    <button
                      type="button"
                      onClick={() => handleOpenAddEvent('goal', 1)}
                      className="btn btn-warning btn-sm rounded-pill px-3 py-1.5 fw-bold text-dark shadow-sm d-flex align-items-center gap-1 ms-1"
                    >
                      <Plus size={14} />
                      <span>Ghi điểm</span>
                    </button>
                  </div>
                </div>
              </Col>

              {/* Tỷ số trung tâm */}
              <Col xs={12} md={2}>
                <div className="d-flex flex-column align-items-center justify-content-center">
                  <div
                    className="font-monospace fw-black display-3 text-warning lh-1 py-2 px-3 rounded-4"
                    style={{
                      textShadow: '0 0 20px rgba(234, 179, 8, 0.4)',
                      letterSpacing: '2px',
                    }}
                  >
                    {score1} - {score2}
                  </div>
                  <small className="text-white-50 fw-medium">TỔNG TỶ SỐ</small>
                </div>
              </Col>

              {/* Đội 2 */}
              <Col xs={12} md={5}>
                <div className="p-3 rounded-4 bg-white bg-opacity-5 border border-white border-opacity-10">
                  <h4 className="fw-bold text-white mb-1 text-truncate">
                    {currentMatch.tenDoi2 || 'Đội 2'}
                  </h4>
                  <small className="text-white-50 d-block mb-3 text-truncate">
                    {currentMatch.donViDoi2 || 'Đoàn VĐV'}
                  </small>

                  {/* Nút cộng trừ điểm */}
                  <div className="d-flex align-items-center justify-content-center gap-2">
                    <button
                      type="button"
                      onClick={() => handleOpenAddEvent('goal', 2)}
                      className="btn btn-warning btn-sm rounded-pill px-3 py-1.5 fw-bold text-dark shadow-sm d-flex align-items-center gap-1 me-1"
                    >
                      <Plus size={14} />
                      <span>Ghi điểm</span>
                    </button>
                    <button
                      type="button"
                      onClick={() => handleScoreChange(2, 1)}
                      className="btn btn-success btn-sm rounded-circle fw-bold shadow-sm"
                      style={{ width: '42px', height: '42px' }}
                      title="Cộng 1 điểm"
                    >
                      +1
                    </button>
                    <button
                      type="button"
                      onClick={() => handleScoreChange(2, -1)}
                      className="btn btn-outline-light btn-sm rounded-circle fw-bold"
                      style={{ width: '38px', height: '38px' }}
                      title="Trừ 1 điểm"
                    >
                      -1
                    </button>
                  </div>
                </div>
              </Col>
            </Row>

            {/* Quick Match Action Controller */}
            <div className="d-flex flex-wrap align-items-center justify-content-between gap-3 pt-3 mt-3 border-top border-white border-opacity-10">
              <div className="d-flex align-items-center gap-3 text-white-50 small flex-wrap">
                <span className="d-flex align-items-center gap-1">
                  <MapPin size={14} className="text-warning" />
                  Sân: <strong>{currentMatch.tenSanDau || 'Chưa xếp sân'}</strong>
                </span>
                <span>•</span>
                <span className="d-flex align-items-center gap-1">
                  <Clock size={14} className="text-info" />
                  {actualStartTime ? (
                    <span>Bắt đầu lúc: <strong>{new Date(actualStartTime).toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' })}</strong></span>
                  ) : (
                    <span>Lịch: {currentMatch.thoiGianBatDau || currentMatch.thoiGianDuKien || 'Chưa định giờ'}</span>
                  )}
                </span>
              </div>

              <div className="d-flex flex-wrap align-items-center gap-2">
                {matchStatus !== 'DangDienRa' && (
                  <Button
                    color="success"
                    size="sm"
                    className="rounded-pill px-3 py-1.5 fw-bold d-flex align-items-center gap-1.5 shadow-sm"
                    onClick={() => {
                      setMatchStatus('DangDienRa');
                      if (!actualStartTime) setActualStartTime(new Date().toISOString());
                    }}
                  >
                    <Play size={14} fill="currentColor" />
                    <span>Bắt đầu thi đấu</span>
                  </Button>
                )}

                {matchStatus === 'DangDienRa' && (
                  <Button
                    color="warning"
                    size="sm"
                    className="rounded-pill px-3 py-1.5 fw-bold text-dark d-flex align-items-center gap-1.5 shadow-sm"
                    onClick={() => setMatchStatus('Hoan')}
                  >
                    <Pause size={14} />
                    <span>Tạm dừng hiệp</span>
                  </Button>
                )}

                {matchStatus !== 'DaKetThuc' && (
                  <Button
                    color="danger"
                    size="sm"
                    className="rounded-pill px-3.5 py-1.5 fw-bold shadow-sm d-flex align-items-center gap-1.5"
                    onClick={() => handleSaveAllResults(true, false)}
                    disabled={saving}
                  >
                    <CheckCircle2 size={15} />
                    <span>Kết thúc trận</span>
                  </Button>
                )}
              </div>
            </div>
          </div>

          {/* ── TABS CHỨC NĂNG: TỶ SỐ & DIỄN BIẾN vs HOÀN THIỆN THÔNG TIN ── */}
          <Nav tabs className="border-bottom-0 gap-2 mb-0">
            <NavItem>
              <NavLink
                className={`rounded-top-3 px-4 py-2.5 fw-bold cursor-pointer border-0 ${
                  activeTab === 'score_events'
                    ? 'bg-white text-primary shadow-sm'
                    : 'bg-light text-muted'
                }`}
                style={{ cursor: 'pointer' }}
                onClick={() => setActiveTab('score_events')}
              >
                <div className="d-flex align-items-center gap-2">
                  <Flame size={16} />
                  <span>1. Cập Nhật Tỷ Số & Diễn Biến Trực Tiếp</span>
                  <Badge color="danger" pill className="ms-1 px-2">
                    {events.length}
                  </Badge>
                </div>
              </NavLink>
            </NavItem>

            <NavItem>
              <NavLink
                className={`rounded-top-3 px-4 py-2.5 fw-bold cursor-pointer border-0 ${
                  activeTab === 'finalize'
                    ? 'bg-white text-success shadow-sm'
                    : 'bg-light text-muted'
                }`}
                style={{ cursor: 'pointer' }}
                onClick={() => setActiveTab('finalize')}
              >
                <div className="d-flex align-items-center gap-2">
                  <Sparkles size={16} />
                  <span>2. Hoàn Thiện Thông Tin Trận Đấu</span>
                  {isFinalized ? (
                    <Badge color="success" pill className="ms-1">
                      Đã chốt
                    </Badge>
                  ) : (
                    <Badge color="warning" pill className="ms-1 text-dark">
                      Cần hoàn thiện
                    </Badge>
                  )}
                </div>
              </NavLink>
            </NavItem>
          </Nav>

          {/* ── TAB 1: TỶ SỐ & DIỄN BIẾN ── */}
          {activeTab === 'score_events' && (
            <Row className="g-4">
              {/* Cột trái: Tỷ số theo từng hiệp đấu */}
              <Col lg={5}>
                <Card className="border-0 shadow-sm rounded-4 h-100">
                  <CardBody className="p-4">
                    <div className="d-flex align-items-center justify-content-between mb-3 pb-2 border-bottom">
                      <div className="d-flex align-items-center gap-2">
                        <Award size={18} className="text-primary" />
                        <h6 className="fw-bold mb-0 text-dark">Tỷ Số Từng Hiệp / Set Đấu</h6>
                      </div>

                      <Button
                        color="outline-primary"
                        size="sm"
                        className="rounded-pill px-2.5 py-1 d-flex align-items-center gap-1"
                        onClick={handleAddSet}
                        style={{ fontSize: '12px' }}
                      >
                        <Plus size={13} />
                        <span>Thêm hiệp</span>
                      </Button>
                    </div>

                    <div className="table-responsive">
                      <Table bordered hover className="align-middle text-center mb-3">
                        <thead className="table-light text-secondary small">
                          <tr>
                            <th style={{ width: '80px' }}>Hiệp</th>
                            <th>{currentMatch.tenDoi1 || 'Đội 1'}</th>
                            <th>{currentMatch.tenDoi2 || 'Đội 2'}</th>
                            <th style={{ width: '50px' }}>Xóa</th>
                          </tr>
                        </thead>
                        <tbody>
                          {setScores.map((set, idx) => (
                            <tr key={idx}>
                              <td className="fw-bold text-dark bg-light">Hiệp {set.setNumber}</td>
                              <td>
                                <Input
                                  type="number"
                                  min={0}
                                  value={set.score1}
                                  onChange={(e) =>
                                    handleSetScoreChange(idx, 1, parseInt(e.target.value, 10) || 0)
                                  }
                                  className="text-center fw-bold rounded-3 font-monospace"
                                />
                              </td>
                              <td>
                                <Input
                                  type="number"
                                  min={0}
                                  value={set.score2}
                                  onChange={(e) =>
                                    handleSetScoreChange(idx, 2, parseInt(e.target.value, 10) || 0)
                                  }
                                  className="text-center fw-bold rounded-3 font-monospace"
                                />
                              </td>
                              <td>
                                {setScores.length > 1 && (
                                  <button
                                    type="button"
                                    onClick={() => handleRemoveSet(idx)}
                                    className="btn btn-sm btn-link text-danger p-0"
                                    title="Xóa hiệp"
                                  >
                                    <Trash2 size={14} />
                                  </button>
                                )}
                              </td>
                            </tr>
                          ))}
                        </tbody>
                      </Table>
                    </div>

                    {/* Chọn đội thắng cuộc */}
                    <FormGroup className="mb-3">
                      <Label className="fw-semibold small text-dark">
                        Xác định Đội / VĐV Thắng Cuộc
                      </Label>
                      <Input
                        type="select"
                        className="rounded-3"
                        value={winner !== undefined ? String(winner) : String(calculatedWinner)}
                        onChange={(e) => {
                          const val = e.target.value;
                          setWinner(val === '1' ? 1 : val === '2' ? 2 : 'draw');
                        }}
                      >
                        <option value="1">
                          {currentMatch.tenDoi1 || 'Đội 1'} (Thắng cuộc)
                        </option>
                        <option value="2">
                          {currentMatch.tenDoi2 || 'Đội 2'} (Thắng cuộc)
                        </option>
                        <option value="draw">Hòa (Không phân định thắng thua)</option>
                      </Input>
                    </FormGroup>

                    {/* Hình thức thắng */}
                    <FormGroup className="mb-3">
                      <Label className="fw-semibold small text-dark">
                        Hình thức phân định kết quả
                      </Label>
                      <Input
                        type="select"
                        className="rounded-3"
                        value={winMethod || 'normal'}
                        onChange={(e) => setWinMethod(e.target.value as any)}
                      >
                        <option value="normal">Thắng trong thời gian thi đấu chính thức</option>
                        <option value="extra_time">Thắng trong hiệp phụ</option>
                        <option value="penalties">Thắng luân lưu / Loạt bắn Penalty</option>
                        <option value="walkover">Thắng do đối phương bỏ cuộc (Walkover)</option>
                        <option value="disqualification">Thắng do đối phương bị truất quyền thi đấu</option>
                      </Input>
                    </FormGroup>

                    {/* Ghi chú nhanh */}
                    <FormGroup className="mb-0">
                      <Label className="fw-semibold small text-dark">
                        Ghi chú tóm tắt tỷ số
                      </Label>
                      <Input
                        type="textarea"
                        rows={2}
                        placeholder="Ghi chú nhanh về tỷ số, tình huống..."
                        className="rounded-3"
                        value={matchNotes}
                        onChange={(e) => setMatchNotes(e.target.value)}
                      />
                    </FormGroup>
                  </CardBody>
                </Card>
              </Col>

              {/* Cột phải: Diễn biến sự kiện & Kỷ luật thẻ phạt */}
              <Col lg={7}>
                <Card className="border-0 shadow-sm rounded-4 h-100">
                  <CardBody className="p-4 d-flex flex-column justify-content-between">
                    <div>
                      <div className="d-flex flex-wrap align-items-center justify-content-between gap-2 mb-3 pb-2 border-bottom">
                        <div className="d-flex align-items-center gap-2">
                          <Flag size={18} className="text-warning" />
                          <h6 className="fw-bold mb-0 text-dark">
                            Diễn Biến & Sự Kiện Trận Đấu ({events.length})
                          </h6>
                        </div>

                        {/* Quick Add Event Buttons */}
                        <div className="d-flex flex-wrap align-items-center gap-1.5">
                          <Button
                            color="success"
                            size="sm"
                            className="rounded-pill px-2.5 py-1 fw-bold d-flex align-items-center gap-1 shadow-sm"
                            onClick={() => handleOpenAddEvent('goal')}
                            style={{ fontSize: '11.5px' }}
                          >
                            <span>⚽ Điểm/Bàn</span>
                          </Button>

                          <Button
                            color="warning"
                            size="sm"
                            className="rounded-pill px-2.5 py-1 text-dark fw-bold d-flex align-items-center gap-1 shadow-sm"
                            onClick={() => handleOpenAddEvent('yellow_card')}
                            style={{ fontSize: '11.5px' }}
                          >
                            <span
                              className="bg-warning border border-dark rounded-1"
                              style={{ width: '8px', height: '11px', display: 'inline-block' }}
                            />
                            <span>Thẻ vàng</span>
                          </Button>

                          <Button
                            color="danger"
                            size="sm"
                            className="rounded-pill px-2.5 py-1 fw-bold d-flex align-items-center gap-1 shadow-sm"
                            onClick={() => handleOpenAddEvent('red_card')}
                            style={{ fontSize: '11.5px' }}
                          >
                            <span
                              className="bg-danger border border-white rounded-1"
                              style={{ width: '8px', height: '11px', display: 'inline-block' }}
                            />
                            <span>Thẻ đỏ</span>
                          </Button>

                          <Button
                            color="info"
                            size="sm"
                            className="rounded-pill px-2.5 py-1 fw-bold text-white d-flex align-items-center gap-1 shadow-sm"
                            onClick={() => handleOpenAddEvent('substitution')}
                            style={{ fontSize: '11.5px' }}
                          >
                            <Users size={12} />
                            <span>Thay người</span>
                          </Button>

                          <Button
                            color="light"
                            size="sm"
                            className="border rounded-pill px-2.5 py-1 fw-semibold text-secondary d-flex align-items-center gap-1"
                            onClick={() => handleOpenAddEvent('foul')}
                            style={{ fontSize: '11.5px' }}
                          >
                            <AlertTriangle size={12} className="text-warning" />
                            <span>Khác</span>
                          </Button>
                        </div>
                      </div>

                      {/* Danh sách sự kiện timeline 2 bên */}
                      {events.length === 0 ? (
                        <div className="text-center py-5 text-muted small">
                          <Activity size={36} className="text-muted opacity-40 mb-2" />
                          <div className="fw-semibold text-dark">Chưa có sự kiện nào được ghi nhận</div>
                          <div>Sử dụng các nút phía trên để thêm bàn thắng, thẻ phạt, thay người hoặc lỗi kỹ thuật.</div>
                        </div>
                      ) : (
                        <div
                          className="d-flex flex-column gap-2 mb-3 pe-1"
                          style={{ maxHeight: '420px', overflowY: 'auto' }}
                        >
                          {events.map((ev) => {
                            const isTeam1 = ev.team === 1;
                            const teamName = isTeam1
                              ? currentMatch.tenDoi1 || 'Đội 1'
                              : currentMatch.tenDoi2 || 'Đội 2';

                            return (
                              <div
                                key={ev.id}
                                className={`p-2.5 rounded-3 border d-flex align-items-center justify-content-between gap-3 ${
                                  isTeam1 ? 'bg-white border-start border-start-3 border-start-primary' : 'bg-white border-end border-end-3 border-end-danger'
                                } shadow-sm`}
                              >
                                <div className="d-flex align-items-center gap-2.5">
                                  {/* Phút */}
                                  <span className="badge bg-dark font-monospace px-2 py-1">
                                    {ev.minute}&apos;
                                  </span>

                                  {/* Loại sự kiện */}
                                  {ev.type === 'goal' || ev.type === 'point' ? (
                                    <span className="badge bg-success-subtle text-success border border-success-subtle px-2 py-1">
                                      ⚽ Điểm / Bàn
                                    </span>
                                  ) : ev.type === 'yellow_card' ? (
                                    <span className="badge bg-warning-subtle text-warning-emphasis border border-warning-subtle px-2 py-1 d-inline-flex align-items-center gap-1">
                                      <span
                                        className="bg-warning rounded-1 border border-dark"
                                        style={{ width: '8px', height: '11px' }}
                                      />
                                      Thẻ vàng
                                    </span>
                                  ) : ev.type === 'red_card' ? (
                                    <span className="badge bg-danger-subtle text-danger border border-danger-subtle px-2 py-1 d-inline-flex align-items-center gap-1">
                                      <span
                                        className="bg-danger rounded-1 border border-white"
                                        style={{ width: '8px', height: '11px' }}
                                      />
                                      Thẻ đỏ
                                    </span>
                                  ) : ev.type === 'substitution' ? (
                                    <span className="badge bg-info-subtle text-info border border-info-subtle px-2 py-1">
                                      🔄 Thay người
                                    </span>
                                  ) : ev.type === 'penalty' ? (
                                    <span className="badge bg-primary-subtle text-primary border border-primary-subtle px-2 py-1">
                                      🎯 Phạt đền
                                    </span>
                                  ) : ev.type === 'injury' ? (
                                    <span className="badge bg-danger-subtle text-danger px-2 py-1">
                                      🚑 Chấn thương
                                    </span>
                                  ) : ev.type === 'timeout' ? (
                                    <span className="badge bg-secondary-subtle text-secondary px-2 py-1">
                                      ⏱️ Hội ý
                                    </span>
                                  ) : (
                                    <span className="badge bg-warning-subtle text-warning px-2 py-1">
                                      ⚠️ {ev.type === 'foul' ? 'Phạm lỗi' : 'Sự cố'}
                                    </span>
                                  )}

                                  {/* Chi tiết nội dung */}
                                  <div className="lh-sm">
                                    <span className={`fw-bold small me-1 ${isTeam1 ? 'text-primary' : 'text-danger'}`}>
                                      [{teamName}]
                                    </span>
                                    {ev.athleteName && (
                                      <span className="fw-semibold text-dark small">
                                        {ev.athleteName}
                                      </span>
                                    )}
                                    {ev.assistName && (
                                      <span className="text-secondary small ms-1">
                                        (Kiến tạo / Ra sân: {ev.assistName})
                                      </span>
                                    )}
                                    {ev.details && (
                                      <small className="text-muted d-block" style={{ fontSize: '11.5px' }}>
                                        {ev.details}
                                      </small>
                                    )}
                                  </div>
                                </div>

                                <div className="d-flex align-items-center gap-1">
                                  <button
                                    type="button"
                                    onClick={() => handleOpenEditEvent(ev)}
                                    className="btn btn-sm btn-light border p-1 rounded-2 text-secondary"
                                    title="Sửa sự kiện"
                                  >
                                    <Edit2 size={13} />
                                  </button>
                                  <button
                                    type="button"
                                    onClick={() => handleDeleteEvent(ev.id)}
                                    className="btn btn-sm btn-light border p-1 rounded-2 text-danger"
                                    title="Xóa sự kiện"
                                  >
                                    <Trash2 size={13} />
                                  </button>
                                </div>
                              </div>
                            );
                          })}
                        </div>
                      )}
                    </div>

                    {/* Bottom Save Bar */}
                    <div className="pt-3 border-top d-flex align-items-center justify-content-between flex-wrap gap-2">
                      <small className="text-muted">
                        Sau khi ghi nhận, nhấn <strong>Lưu Kết Quả</strong> để đồng bộ hệ thống.
                      </small>

                      <div className="d-flex align-items-center gap-2">
                        <Button
                          color="outline-success"
                          className="rounded-3 fw-bold px-3 d-flex align-items-center gap-1.5"
                          onClick={() => setActiveTab('finalize')}
                        >
                          <Sparkles size={15} />
                          <span>Chuyển sang Hoàn thiện thông tin</span>
                        </Button>

                        <Button
                          color="success"
                          className="rounded-3 fw-bold px-4 shadow-sm d-flex align-items-center gap-1.5"
                          onClick={() => handleSaveAllResults(false, false)}
                          disabled={saving}
                        >
                          {saving ? <Spinner size="sm" /> : <Save size={16} />}
                          <span>Lưu Kết Quả</span>
                        </Button>
                      </div>
                    </div>
                  </CardBody>
                </Card>
              </Col>
            </Row>
          )}

          {/* ── TAB 2: HOÀN THIỆN THÔNG TIN TRẬN ĐẤU ── */}
          {activeTab === 'finalize' && (
            <Card className="border-0 shadow-sm rounded-4">
              <CardBody className="p-4 p-md-5">
                <div className="d-flex flex-wrap align-items-center justify-content-between gap-3 mb-4 pb-3 border-bottom">
                  <div>
                    <h5 className="fw-bold text-dark mb-1 d-flex align-items-center gap-2">
                      <Sparkles size={20} className="text-success" />
                      <span>Hồ Sơ Hoàn Thiện Trận Đấu & Báo Cáo Trọng Tài</span>
                    </h5>
                    <p className="text-muted small mb-0">
                      Ghi nhận thời gian thực tế, điều kiện tổ chức, bầu chọn MVP, đánh giá công tác trọng tài và chốt hồ sơ trận đấu.
                    </p>
                  </div>

                  <div className="d-flex align-items-center gap-2">
                    {isFinalized ? (
                      <span className="badge bg-success-subtle text-success border border-success-subtle px-3 py-2 rounded-pill fw-semibold d-flex align-items-center gap-1.5">
                        <Lock size={14} />
                        Hồ sơ đã được chốt và khóa ({finalizedBy})
                      </span>
                    ) : (
                      <span className="badge bg-warning-subtle text-warning-emphasis border border-warning-subtle px-3 py-2 rounded-pill fw-semibold d-flex align-items-center gap-1.5">
                        <Unlock size={14} />
                        Hồ sơ đang mở (Chưa chốt hoàn thiện)
                      </span>
                    )}
                  </div>
                </div>

                <Row className="g-4">
                  {/* Khối 1: Thời gian thi đấu thực tế */}
                  <Col xs={12} lg={6}>
                    <div className="p-3.5 rounded-3 bg-light border h-100">
                      <h6 className="fw-bold text-dark mb-3 d-flex align-items-center gap-2">
                        <Clock size={16} className="text-primary" />
                        <span>1. Thời Gian Thi Đấu Thực Tế</span>
                      </h6>

                      <Row className="g-3">
                        <Col xs={12} sm={6}>
                          <FormGroup className="mb-2">
                            <div className="d-flex align-items-center justify-content-between mb-1">
                              <Label className="fw-semibold small text-secondary mb-0">
                                Giờ bắt đầu thực tế
                              </Label>
                              <button
                                type="button"
                                onClick={() => setActualStartTime(new Date().toISOString())}
                                className="btn btn-link btn-sm p-0 text-primary small text-decoration-none"
                                style={{ fontSize: '11px' }}
                              >
                                Lấy giờ hiện tại
                              </button>
                            </div>
                            <Input
                              type="datetime-local"
                              className="rounded-3"
                              value={actualStartTime ? actualStartTime.slice(0, 16) : ''}
                              onChange={(e) => setActualStartTime(e.target.value)}
                            />
                          </FormGroup>
                        </Col>

                        <Col xs={12} sm={6}>
                          <FormGroup className="mb-2">
                            <div className="d-flex align-items-center justify-content-between mb-1">
                              <Label className="fw-semibold small text-secondary mb-0">
                                Giờ kết thúc thực tế
                              </Label>
                              <button
                                type="button"
                                onClick={() => setActualEndTime(new Date().toISOString())}
                                className="btn btn-link btn-sm p-0 text-primary small text-decoration-none"
                                style={{ fontSize: '11px' }}
                              >
                                Lấy giờ hiện tại
                              </button>
                            </div>
                            <Input
                              type="datetime-local"
                              className="rounded-3"
                              value={actualEndTime ? actualEndTime.slice(0, 16) : ''}
                              onChange={(e) => setActualEndTime(e.target.value)}
                            />
                          </FormGroup>
                        </Col>

                        <Col xs={6}>
                          <FormGroup className="mb-0">
                            <Label className="fw-semibold small text-secondary mb-1">
                              Tổng thời lượng (Phút)
                            </Label>
                            <Input
                              type="number"
                              min={1}
                              className="rounded-3"
                              value={durationMinutes}
                              onChange={(e) => setDurationMinutes(parseInt(e.target.value, 10) || 0)}
                            />
                          </FormGroup>
                        </Col>

                        <Col xs={6}>
                          <FormGroup className="mb-0">
                            <Label className="fw-semibold small text-secondary mb-1">
                              Bù giờ / Hiệp phụ (Phút)
                            </Label>
                            <Input
                              type="number"
                              min={0}
                              className="rounded-3"
                              value={extraTimeMinutes}
                              onChange={(e) => setExtraTimeMinutes(parseInt(e.target.value, 10) || 0)}
                            />
                          </FormGroup>
                        </Col>
                      </Row>
                    </div>
                  </Col>

                  {/* Khối 2: Điều kiện tổ chức & Sân bãi */}
                  <Col xs={12} lg={6}>
                    <div className="p-3.5 rounded-3 bg-light border h-100">
                      <h6 className="fw-bold text-dark mb-3 d-flex align-items-center gap-2">
                        <CloudSun size={16} className="text-warning" />
                        <span>2. Điều Kiện Thi Đấu & Sân Bãi</span>
                      </h6>

                      <Row className="g-3">
                        <Col xs={12} sm={6}>
                          <FormGroup className="mb-2">
                            <Label className="fw-semibold small text-secondary mb-1">
                              Thời tiết / Khí hậu
                            </Label>
                            <Input
                              type="text"
                              placeholder="Nắng ráo, Mưa nhỏ, Trong nhà thi đấu..."
                              className="rounded-3"
                              value={weatherCondition}
                              onChange={(e) => setWeatherCondition(e.target.value)}
                            />
                          </FormGroup>
                        </Col>

                        <Col xs={12} sm={6}>
                          <FormGroup className="mb-2">
                            <Label className="fw-semibold small text-secondary mb-1">
                              Tình trạng mặt sân / thảm
                            </Label>
                            <Input
                              type="text"
                              placeholder="Đạt chuẩn, Khô ráo, Trơn ướt..."
                              className="rounded-3"
                              value={pitchCondition}
                              onChange={(e) => setPitchCondition(e.target.value)}
                            />
                          </FormGroup>
                        </Col>

                        <Col xs={12} sm={6}>
                          <FormGroup className="mb-0">
                            <Label className="fw-semibold small text-secondary mb-1">
                              Số lượng khán giả (Ước tính)
                            </Label>
                            <Input
                              type="number"
                              min={0}
                              placeholder="Số khán giả"
                              className="rounded-3"
                              value={spectatorCount}
                              onChange={(e) => setSpectatorCount(parseInt(e.target.value, 10) || 0)}
                            />
                          </FormGroup>
                        </Col>

                        <Col xs={12} sm={6}>
                          <FormGroup className="mb-0">
                            <Label className="fw-semibold small text-secondary mb-1">
                              VĐV xuất sắc nhất trận (MVP)
                            </Label>
                            <Input
                              type="text"
                              placeholder="Họ tên VĐV (#Số áo)"
                              className="rounded-3"
                              value={mvpAthlete}
                              onChange={(e) => setMvpAthlete(e.target.value)}
                            />
                          </FormGroup>
                        </Col>
                      </Row>
                    </div>
                  </Col>

                  {/* Khối 3: Nhận xét Tổ Trọng Tài & Giám Sát */}
                  <Col xs={12}>
                    <div className="p-3.5 rounded-3 bg-light border">
                      <h6 className="fw-bold text-dark mb-3 d-flex align-items-center gap-2">
                        <FileText size={16} className="text-success" />
                        <span>3. Báo Cáo Chuyên Môn Của Tổ Trọng Tài & Giám Sát Trận Đấu</span>
                      </h6>

                      <Row className="g-3">
                        <Col xs={12} md={6}>
                          <FormGroup className="mb-0">
                            <Label className="fw-semibold small text-dark mb-1">
                              Nhận xét của Tổ Trọng Tài (Tinh thần thi đấu, kỷ luật, fair-play)
                            </Label>
                            <Input
                              type="textarea"
                              rows={3}
                              placeholder="Hai đội thi đấu quyết tâm, chấp hành nghiêm điều lệ giải và phán quyết của trọng tài. Không có sự cố nghiêm trọng phát sinh..."
                              className="rounded-3"
                              value={refereeNotes}
                              onChange={(e) => setRefereeNotes(e.target.value)}
                            />
                          </FormGroup>
                        </Col>

                        <Col xs={12} md={6}>
                          <FormGroup className="mb-0">
                            <Label className="fw-semibold small text-dark mb-1">
                              Ý kiến / Đánh giá của Giám sát trận đấu
                            </Label>
                            <Input
                              type="textarea"
                              rows={3}
                              placeholder="Tổ trọng tài hoàn thành tốt nhiệm vụ, điều hành công tâm, chính xác. Công tác y tế và an ninh sân bãi đảm bảo tuyệt đối..."
                              className="rounded-3"
                              value={supervisorNotes}
                              onChange={(e) => setSupervisorNotes(e.target.value)}
                            />
                          </FormGroup>
                        </Col>
                      </Row>
                    </div>
                  </Col>
                </Row>

                {/* Chốt hoàn tất & Nộp biên bản Bar */}
                <div className="mt-4 pt-3 border-top d-flex flex-wrap align-items-center justify-content-between gap-3">
                  <div className="text-secondary small">
                    {isFinalized ? (
                      <span className="text-success fw-medium">
                        ✓ Trận đấu đã được chốt hoàn tất vào lúc {new Date(finalizedAt).toLocaleString('vi-VN')} bởi {finalizedBy}.
                      </span>
                    ) : (
                      <span>
                        Vui lòng kiểm tra kỹ mọi thông số trước khi nhấn <strong>Hoàn Thiện & Chốt Hồ Sơ</strong>.
                      </span>
                    )}
                  </div>

                  <div className="d-flex align-items-center gap-2 flex-wrap">
                    <Button
                      color="light"
                      className="border rounded-3 fw-semibold px-3"
                      onClick={() => handleSaveAllResults(false, false)}
                      disabled={saving}
                    >
                      {saving ? <Spinner size="sm" /> : <Save size={15} className="me-1" />}
                      <span>Lưu Nháp Thông Tin</span>
                    </Button>

                    <Button
                      color="success"
                      className="rounded-3 fw-bold px-4 shadow-sm d-flex align-items-center gap-1.5"
                      onClick={() => handleSaveAllResults(true, true)}
                      disabled={saving}
                    >
                      {saving ? <Spinner size="sm" /> : <CheckCircle2 size={16} />}
                      <span>Hoàn Thiện & Chốt Hồ Sơ Trận Đấu</span>
                    </Button>

                    <Link
                      href={`/trong-tai/bien-ban?tranDauId=${currentMatch.id}&giaiDauId=${selectedGiaiDauId}`}
                      className="btn btn-primary rounded-3 fw-bold px-3 d-flex align-items-center gap-1.5 shadow-sm"
                    >
                      <FileText size={15} />
                      <span>Ký Số Biên Bản A4</span>
                    </Link>
                  </div>
                </div>
              </CardBody>
            </Card>
          )}
        </>
      )}

      {/* ── Modal Thêm / Sửa Sự Kiện Trận Đấu ── */}
      <Modal isOpen={eventModalOpen} toggle={() => setEventModalOpen(false)} centered size="md">
        <form onSubmit={handleSaveEvent}>
          <ModalHeader toggle={() => setEventModalOpen(false)}>
            <div className="d-flex align-items-center gap-2">
              <Flag size={18} className="text-warning" />
              <span>{editingEventId ? 'Chỉnh Sửa Sự Kiện Trận Đấu' : 'Ghi Nhận Sự Kiện / Diễn Biến Trận Đấu'}</span>
            </div>
          </ModalHeader>
          <ModalBody className="p-4">
            <Row className="g-3">
              {/* Chọn đội */}
              <Col xs={12}>
                <FormGroup>
                  <Label className="fw-semibold small">Áp dụng cho Đội / VĐV</Label>
                  <Input
                    type="select"
                    className="rounded-3"
                    value={eventTeam}
                    onChange={(e) => setEventTeam(Number(e.target.value) as 1 | 2)}
                  >
                    <option value={1}>{currentMatch?.tenDoi1 || 'Đội 1'}</option>
                    <option value={2}>{currentMatch?.tenDoi2 || 'Đội 2'}</option>
                  </Input>
                </FormGroup>
              </Col>

              {/* Loại sự kiện */}
              <Col xs={7}>
                <FormGroup>
                  <Label className="fw-semibold small">Loại sự kiện</Label>
                  <Input
                    type="select"
                    className="rounded-3"
                    value={eventType}
                    onChange={(e) => setEventType(e.target.value as MatchEvent['type'])}
                  >
                    <option value="goal">⚽ Bàn thắng / Điểm trực tiếp</option>
                    <option value="yellow_card">🟨 Thẻ vàng</option>
                    <option value="red_card">🟥 Thẻ đỏ</option>
                    <option value="substitution">🔄 Thay người</option>
                    <option value="penalty">🎯 Phạt đền / Penalty</option>
                    <option value="foul">⚠️ Lỗi kỹ thuật / Phạm lỗi</option>
                    <option value="timeout">⏱️ Hội ý kỹ thuật</option>
                    <option value="injury">🚑 Chấn thương / Sơ cứu</option>
                    <option value="incident">⚡ Sự cố trận đấu</option>
                  </Input>
                </FormGroup>
              </Col>

              {/* Phút thi đấu */}
              <Col xs={5}>
                <FormGroup>
                  <Label className="fw-semibold small">Phút thi đấu</Label>
                  <Input
                    type="number"
                    min={1}
                    max={150}
                    className="rounded-3 font-monospace"
                    value={eventMinute}
                    onChange={(e) => setEventMinute(parseInt(e.target.value, 10) || 1)}
                    required
                  />
                </FormGroup>
              </Col>

              {/* Họ tên VĐV */}
              <Col xs={12}>
                <FormGroup>
                  <Label className="fw-semibold small">
                    {eventType === 'substitution'
                      ? 'Cầu thủ vào sân'
                      : 'Vận động viên thực hiện / liên quan'}
                  </Label>
                  <Input
                    type="text"
                    placeholder="Ví dụ: Nguyễn Văn A (#10)"
                    className="rounded-3"
                    value={eventAthlete}
                    onChange={(e) => setEventAthlete(e.target.value)}
                  />
                </FormGroup>
              </Col>

              {/* Người hỗ trợ / Cầu thủ ra sân */}
              <Col xs={12}>
                <FormGroup>
                  <Label className="fw-semibold small">
                    {eventType === 'substitution'
                      ? 'Cầu thủ rời sân'
                      : 'Người kiến tạo / Người liên quan khác'}
                  </Label>
                  <Input
                    type="text"
                    placeholder="Ví dụ: Trần Văn B (#7)"
                    className="rounded-3"
                    value={eventAssist}
                    onChange={(e) => setEventAssist(e.target.value)}
                  />
                </FormGroup>
              </Col>

              {/* Chi tiết vi phạm / Diễn biến */}
              <Col xs={12}>
                <FormGroup className="mb-0">
                  <Label className="fw-semibold small">Mô tả chi tiết tình huống</Label>
                  <Input
                    type="text"
                    placeholder="Ví dụ: Sút bóng vào góc cao, Kéo áo cản người, Hội ý chiến thuật..."
                    className="rounded-3"
                    value={eventDetails}
                    onChange={(e) => setEventDetails(e.target.value)}
                  />
                </FormGroup>
              </Col>
            </Row>
          </ModalBody>
          <ModalFooter>
            <Button color="light" className="rounded-3" onClick={() => setEventModalOpen(false)}>
              Hủy
            </Button>
            <Button color="primary" type="submit" className="rounded-3 fw-bold shadow-sm">
              {editingEventId ? 'Cập Nhật Sự Kiện' : 'Thêm Vào Diễn Biến'}
            </Button>
          </ModalFooter>
        </form>
      </Modal>
    </div>
  );
}
