'use client';

import React, { useState, useEffect, useMemo, useCallback } from 'react';
import Link from 'next/link';
import {
  Calendar,
  Clock,
  MapPin,
  Users,
  Shield,
  Plus,
  Trash2,
  Edit2,
  RefreshCw,
  Zap,
  Layers,
  AlertTriangle,
  CheckCircle2,
  Search,
  Filter,
  Trophy,
  ChevronRight,
  UserCheck,
  Flag,
  Shuffle,
  Eye,
  Info,
  Sliders,
  CalendarDays,
  ShieldAlert,
  UserX,
  CheckCircle,
  Building,
  User,
  Award,
  ExternalLink,
} from 'lucide-react';
import {
  Row,
  Col,
  Card,
  CardBody,
  Badge,
  Button,
  Table,
  Modal,
  ModalHeader,
  ModalBody,
  ModalFooter,
  Form,
  FormGroup,
  Label,
  Input,
  Nav,
  NavItem,
  NavLink,
  Spinner,
  Alert,
} from 'reactstrap';
import { useToast } from '@/context/AuthContext';
import {
  giaiDauService,
  noiDungThiDauService,
  sanDauService,
  trongTaiService,
  dangKyThiDauService,
  tranDauService,
  bangDauService,
  vongDauService,
  cauHinhLichThiDauService,
} from '@/services';
import {
  GiaiDau,
  NoiDungThiDau,
  SanDau,
  TrongTai,
  DangKyThiDau,
  TranDau,
  BangDau,
  VongDau,
  AutoScheduleRequest,
  CreateUpdateTranDau,
  AssignTrongTai,
  ConflictDetail,
  TournamentConflictReport,
  CauHinhLichThiDau,
} from '@/types';
import { HinhThucThiDauLabels } from '@/types/hinhThucThiDau';

export default function LichThiDauPage() {
  const toast = useToast();
  // 1. Data Selection States
  const [tournaments, setTournaments] = useState<GiaiDau[]>([]);
  const [selectedTournamentId, setSelectedTournamentId] = useState<number | ''>('');
  const [events, setEvents] = useState<NoiDungThiDau[]>([]);
  const [selectedEventId, setSelectedEventId] = useState<number | ''>('');

  // 2. Resource Data States
  const [matches, setMatches] = useState<TranDau[]>([]);
  const [groups, setGroups] = useState<BangDau[]>([]);
  const [rounds, setRounds] = useState<VongDau[]>([]);
  const [venues, setVenues] = useState<SanDau[]>([]);
  const [referees, setReferees] = useState<TrongTai[]>([]);
  const [registeredTeams, setRegisteredTeams] = useState<DangKyThiDau[]>([]);

  // Tournament-wide data states
  const [tournamentTeams, setTournamentTeams] = useState<DangKyThiDau[]>([]);
  const [loadingTournamentTeams, setLoadingTournamentTeams] = useState<boolean>(false);
  const [allTournamentMatches, setAllTournamentMatches] = useState<TranDau[]>([]);

  // 3. UI States
  const [loading, setLoading] = useState<boolean>(false);
  const [activeTab, setActiveTab] = useState<'list' | 'court' | 'referee' | 'groups' | 'teams'>('list');
  const [filterDate, setFilterDate] = useState<string>('');
  const [filterRoundId, setFilterRoundId] = useState<string>('');
  const [filterGroupId, setFilterGroupId] = useState<string>('');
  const [filterVenueId, setFilterVenueId] = useState<string>('');
  const [filterStatus, setFilterStatus] = useState<string>('');
  const [searchKeyword, setSearchKeyword] = useState<string>('');

  // Teams Tab UI states
  const [teamScope, setTeamScope] = useState<'current_event' | 'tournament'>('current_event');
  const [teamSearch, setTeamSearch] = useState<string>('');
  const [teamFilterEventId, setTeamFilterEventId] = useState<string>('');
  const [teamFilterDonVi, setTeamFilterDonVi] = useState<string>('');
  const [teamFilterStatus, setTeamFilterStatus] = useState<string>('');
  const [teamFilterScheduled, setTeamFilterScheduled] = useState<string>('');
  const [selectedTeamDetail, setSelectedTeamDetail] = useState<DangKyThiDau | null>(null);
  const [teamDetailModalOpen, setTeamDetailModalOpen] = useState<boolean>(false);

  // 4. Modal States
  const [autoScheduleModalOpen, setAutoScheduleModalOpen] = useState(false);
  const [autoScheduleSubmitting, setAutoScheduleSubmitting] = useState(false);
  const [autoScheduleConfig, setAutoScheduleConfig] = useState<{
    startDate: string;
    startTime: string;
    endTime: string;
    matchDuration: number;
    breakDuration: number;
    soHiepDau: number;
    thoiGianMoiHiepPhut: number;
    selectedVenueIds: number[];
    selectedRefereeIds: number[];
    refereesPerMatch: number;
    autoCreateGroups: boolean;
    teamsPerGroup: number;
    clearOldMatches: boolean;
    avoidAthleteConflict: boolean;
    thoiGianNghiToiThieuVdvPhut: number;
    canBangTaiSanDau: boolean;
    canBangTaiTrongTai: boolean;
    soDoiMoiBangVaoVongTrong: number;
    layDoiThu3TotNhat: boolean;
    soDoiThu3TotNhat: number;
    // Leaderboard (TinhDiemXepHang) specific
    soVdvMoiLuotThi: number;
    phuongThucPhanNhom: 'random' | 'registration_order' | 'performance_seed';
    soVongThi: number;
    // Round Robin (VongBang = 2) specific
    soLuotDau: number;
    cheDoVongBang: 'single_group' | 'multi_groups';
    heThongDiem: string;
    diemThang: number;
    diemHoa: number;
    diemThua: number;
  }>({
    startDate: new Date().toISOString().split('T')[0],
    startTime: '08:00',
    endTime: '17:30',
    matchDuration: 90,
    breakDuration: 15,
    soHiepDau: 2,
    thoiGianMoiHiepPhut: 45,
    selectedVenueIds: [],
    selectedRefereeIds: [],
    refereesPerMatch: 1,
    autoCreateGroups: true,
    teamsPerGroup: 4,
    clearOldMatches: true,
    avoidAthleteConflict: true,
    thoiGianNghiToiThieuVdvPhut: 60,
    canBangTaiSanDau: true,
    canBangTaiTrongTai: true,
    soDoiMoiBangVaoVongTrong: 2,
    layDoiThu3TotNhat: false,
    soDoiThu3TotNhat: 0,
    // Leaderboard defaults
    soVdvMoiLuotThi: 8,
    phuongThucPhanNhom: 'random',
    soVongThi: 1,
    // Round Robin (VongBang = 2) defaults
    soLuotDau: 1,
    cheDoVongBang: 'single_group',
    heThongDiem: '3_1_0',
    diemThang: 3,
    diemHoa: 1,
    diemThua: 0,
  });

  // Manual Match Modal
  const [manualMatchModalOpen, setManualMatchModalOpen] = useState(false);
  const [editingMatchId, setEditingMatchId] = useState<number | null>(null);
  const [matchForm, setMatchForm] = useState<{
    roundId: number | '';
    groupId: number | '';
    doi1Id: number | '';
    doi2Id: number | '';
    venueId: number | '';
    matchDate: string;
    startTime: string;
    endTime: string;
    matchNumber: number;
    matchName: string;
    status: string;
    notes: string;
    refereeAssignments: AssignTrongTai[];
  }>({
    roundId: '',
    groupId: '',
    doi1Id: '',
    doi2Id: '',
    venueId: '',
    matchDate: new Date().toISOString().split('T')[0],
    startTime: '08:30',
    endTime: '09:30',
    matchNumber: 1,
    matchName: '',
    status: 'ChuaDau',
    notes: '',
    refereeAssignments: [],
  });

  // Conflict state
  const [conflictWarning, setConflictWarning] = useState<string[]>([]);
  const [chiTietConflictWarning, setChiTietConflictWarning] = useState<ConflictDetail[]>([]);
  const [checkingConflict, setCheckingConflict] = useState(false);

  // Tournament Conflict Audit Modal
  const [tournamentConflictModalOpen, setTournamentConflictModalOpen] = useState(false);
  const [tournamentConflictReport, setTournamentConflictReport] = useState<TournamentConflictReport | null>(null);
  const [checkingTournamentConflict, setCheckingTournamentConflict] = useState(false);

  // Group Distribution Modal
  const [groupModalOpen, setGroupModalOpen] = useState(false);
  const [distributeGroupCount, setDistributeGroupCount] = useState<number>(2);

  // Current selected tournament object
  const currentTournament = useMemo(() => {
    return tournaments.find((t) => t.id === Number(selectedTournamentId));
  }, [tournaments, selectedTournamentId]);

  // Current sport schedule configuration from CauHinhLichThiDau
  const [sportScheduleConfig, setSportScheduleConfig] = useState<CauHinhLichThiDau | null>(null);
  const [loadingSportConfig, setLoadingSportConfig] = useState<boolean>(false);

  // Ưu tiên Khoảng cách giữa các vòng (giờ) trước rồi mới Thời gian nghỉ tối thiểu giữa 2 trận (phút)
  const effectiveRestInfo = useMemo(() => {
    if (sportScheduleConfig?.khoangCachGiuaCacVongGio && sportScheduleConfig.khoangCachGiuaCacVongGio > 0) {
      const minutes = sportScheduleConfig.khoangCachGiuaCacVongGio * 60;
      return {
        minutes,
        source: 'khoangCachVong' as const,
        label: `${minutes} phút (${sportScheduleConfig.khoangCachGiuaCacVongGio} giờ - Khoảng cách giữa các vòng)`,
      };
    }
    const minutes = sportScheduleConfig?.nghiToiThieuGiua2TranPhut || autoScheduleConfig.thoiGianNghiToiThieuVdvPhut || 60;
    return {
      minutes,
      source: 'nghiToiThieu2Tran' as const,
      label: `${minutes} phút (Nghỉ tối thiểu giữa 2 trận)`,
    };
  }, [sportScheduleConfig, autoScheduleConfig.thoiGianNghiToiThieuVdvPhut]);

  // Sync tournament start date with autoScheduleConfig.startDate
  useEffect(() => {
    if (currentTournament?.ngayBatDau) {
      const d = new Date(currentTournament.ngayBatDau).toISOString().split('T')[0];
      setAutoScheduleConfig((prev) => ({
        ...prev,
        startDate: d,
      }));
    }
  }, [currentTournament]);

  // Current selected event object
  const currentEvent = useMemo(() => {
    return events.find((e) => e.id === Number(selectedEventId));
  }, [events, selectedEventId]);

  // Check if current event has group stage
  const hasGroupStage = useMemo(() => {
    if (!currentEvent) return false;
    const ht = currentEvent.hinhThucThiDau;
    // Nếu là Loại trực tiếp thì TUYỆT ĐỐI KHÔNG chia bảng và KHÔNG có vòng bảng
    if (ht === 'LoaiTrucTiep' || ht === 'NhanhThangNhanhThua') return false;
    // Chỉ chia bảng với Vòng Bảng hoặc Kết Hợp Vòng Bảng & Loại Trực Tiếp
    return ht === 'VongBang' || ht === 'KetHopVongBangVaLoaiTrucTiep';
  }, [currentEvent]);

  const isKnockout = useMemo(() => {
    if (!currentEvent) return false;
    const ht = currentEvent.hinhThucThiDau;
    return ht === 'LoaiTrucTiep' || ht === 'NhanhThangNhanhThua' || !ht;
  }, [currentEvent]);

  // Thể thức tính điểm xếp hạng / tính giờ - thành tích (bơi lội, điền kinh...)
  const isLeaderboard = useMemo(() => {
    return currentEvent?.hinhThucThiDau === 'TinhDiemXepHang';
  }, [currentEvent]);

  // Dự kiến số đội và sơ đồ vòng loại trực tiếp khi ở thể thức Kết hợp vòng bảng & loại trực tiếp
  const predictedAdvancingInfo = useMemo(() => {
    if (currentEvent?.hinhThucThiDau !== 'KetHopVongBangVaLoaiTrucTiep') return null;

    let estNumGroups = groups.length;
    if (estNumGroups === 0 && autoScheduleConfig.autoCreateGroups) {
      const totalTeams = registeredTeams.length;
      const perGroup = autoScheduleConfig.teamsPerGroup || 4;
      estNumGroups = Math.max(1, Math.ceil(totalTeams / perGroup));
    }
    if (estNumGroups === 0) estNumGroups = 2;

    const soDoiMoiBang = autoScheduleConfig.soDoiMoiBangVaoVongTrong || 2;
    const soDoiThu3 = (soDoiMoiBang >= 2 && autoScheduleConfig.layDoiThu3TotNhat) ? (autoScheduleConfig.soDoiThu3TotNhat || 0) : 0;
    const totalAdvancing = (estNumGroups * soDoiMoiBang) + soDoiThu3;

    let bracketDesc = '';
    if (totalAdvancing <= 2) {
      bracketDesc = 'Trận Chung kết (2 đội)';
    } else if (totalAdvancing <= 4) {
      bracketDesc = 'Bán kết (2 trận) ➔ Tranh hạng 3-4 ➔ Chung kết';
    } else if (totalAdvancing <= 8) {
      bracketDesc = 'Tứ kết (4 trận) ➔ Bán kết ➔ Tranh hạng 3-4 ➔ Chung kết';
    } else {
      bracketDesc = 'Vòng 1/8 (8 trận) ➔ Tứ kết ➔ Bán kết ➔ Tranh hạng 3-4 ➔ Chung kết';
    }

    return {
      estNumGroups,
      soDoiMoiBang,
      soDoiThu3,
      totalAdvancing,
      bracketDesc,
    };
  }, [
    currentEvent,
    groups.length,
    registeredTeams.length,
    autoScheduleConfig.autoCreateGroups,
    autoScheduleConfig.teamsPerGroup,
    autoScheduleConfig.soDoiMoiBangVaoVongTrong,
    autoScheduleConfig.layDoiThu3TotNhat,
    autoScheduleConfig.soDoiThu3TotNhat,
  ]);

  // Live preview cho Leaderboard (tính số lượt thi, số ngày, tổng thời gian dự kiến)
  const leaderboardPreview = useMemo(() => {
    if (!isLeaderboard) return null;
    const totalVdv = registeredTeams.length;
    const heatSize = Math.max(1, autoScheduleConfig.soVdvMoiLuotThi);
    const heatsPerVong = Math.ceil(totalVdv / heatSize);
    const totalHeats = heatsPerVong * autoScheduleConfig.soVongThi;
    const minutesPerHeat = (sportScheduleConfig?.thoiLuongTranMacDinhPhut || autoScheduleConfig.matchDuration || 5) +
      (sportScheduleConfig?.thoiGianDemDonSanPhut ?? autoScheduleConfig.breakDuration ?? 10);
    const totalMinutes = totalHeats * minutesPerHeat;
    const startH = (sportScheduleConfig?.caSangBatDau || autoScheduleConfig.startTime || '08:00').split(':').map(Number);
    const endH = (sportScheduleConfig?.caChieuKetThuc || autoScheduleConfig.endTime || '17:30').split(':').map(Number);
    const dailyMinutes = (endH[0] * 60 + endH[1]) - (startH[0] * 60 + startH[1]);
    const estimatedDays = dailyMinutes > 0 ? Math.ceil(totalMinutes / dailyMinutes) : 1;
    return { totalVdv, heatSize, heatsPerVong, totalHeats, minutesPerHeat, totalMinutes, estimatedDays };
  }, [
    isLeaderboard,
    registeredTeams.length,
    autoScheduleConfig.soVdvMoiLuotThi,
    autoScheduleConfig.soVongThi,
    autoScheduleConfig.matchDuration,
    autoScheduleConfig.breakDuration,
    autoScheduleConfig.startTime,
    autoScheduleConfig.endTime,
    sportScheduleConfig,
  ]);

  // Thể thức vòng tròn tính điểm (VongBang = 2)
  const isRoundRobin = useMemo(() => {
    return currentEvent?.hinhThucThiDau === 'VongBang';
  }, [currentEvent]);

  // Live preview cho Vòng tròn tính điểm (Round Robin)
  const roundRobinPreview = useMemo(() => {
    if (!isRoundRobin) return null;
    const totalTeams = registeredTeams.length;
    const isSingleGroup = autoScheduleConfig.cheDoVongBang === 'single_group';
    const teamsPerGroup = autoScheduleConfig.teamsPerGroup || 4;
    const numGroups = isSingleGroup ? 1 : Math.max(1, Math.ceil(totalTeams / teamsPerGroup));
    const teamsInGroup = isSingleGroup ? totalTeams : Math.min(totalTeams, teamsPerGroup);
    // Trong Round Robin: nếu N chẵn thì N-1 vòng; nếu N lẻ thì N vòng
    const roundsPerLuot = teamsInGroup > 1 ? (teamsInGroup % 2 === 0 ? teamsInGroup - 1 : teamsInGroup) : 0;
    const soLuot = autoScheduleConfig.soLuotDau || 1;
    const totalRounds = roundsPerLuot * soLuot;
    const matchesPerGroup = teamsInGroup > 1
      ? Math.floor((teamsInGroup * (teamsInGroup - 1)) / 2) * soLuot
      : 0;
    const totalMatches = isSingleGroup ? matchesPerGroup : matchesPerGroup * numGroups;
    const matchMinutes = (sportScheduleConfig?.thoiLuongTranMacDinhPhut || autoScheduleConfig.matchDuration || 60) +
      (sportScheduleConfig?.thoiGianDemDonSanPhut ?? autoScheduleConfig.breakDuration ?? 15);
    const courtsCount = Math.max(1, venues.length);
    const startH = (sportScheduleConfig?.caSangBatDau || autoScheduleConfig.startTime || '08:00').split(':').map(Number);
    const endH = (sportScheduleConfig?.caChieuKetThuc || autoScheduleConfig.endTime || '17:30').split(':').map(Number);
    const dailyMinutes = Math.max(60, (endH[0] * 60 + endH[1]) - (startH[0] * 60 + startH[1]));
    const matchesPerCourtPerDay = Math.max(1, Math.floor(dailyMinutes / matchMinutes));
    const totalMatchesCapacityPerDay = matchesPerCourtPerDay * courtsCount;

    const estimatedDays = (sportScheduleConfig?.moiVongMotNgay ?? true)
      ? Math.max(1, totalRounds)
      : Math.max(1, Math.ceil(totalMatches / totalMatchesCapacityPerDay));

    return {
      totalTeams,
      isSingleGroup,
      numGroups,
      teamsInGroup,
      roundsPerLuot,
      soLuot,
      totalRounds,
      matchesPerGroup,
      totalMatches,
      estimatedDays,
    };
  }, [
    isRoundRobin,
    registeredTeams.length,
    autoScheduleConfig.cheDoVongBang,
    autoScheduleConfig.teamsPerGroup,
    autoScheduleConfig.soLuotDau,
    autoScheduleConfig.matchDuration,
    autoScheduleConfig.breakDuration,
    autoScheduleConfig.startTime,
    autoScheduleConfig.endTime,
    sportScheduleConfig,
    venues.length,
  ]);

  // Initial Load: Tournaments
  useEffect(() => {
    const loadTournaments = async () => {
      try {
        const data = await giaiDauService.getAll();
        setTournaments(data);
        if (data.length > 0) {
          setSelectedTournamentId(data[0].id);
        }
      } catch (err) {
        console.error('Lỗi khi tải danh sách giải đấu:', err);
      }
    };
    loadTournaments();
  }, []);

  // Load tournament-wide teams & matches
  const loadTournamentData = useCallback(async () => {
    if (!selectedTournamentId) {
      setTournamentTeams([]);
      setAllTournamentMatches([]);
      return;
    }
    setLoadingTournamentTeams(true);
    try {
      const tId = Number(selectedTournamentId);
      const [teamsRes, matchesRes] = await Promise.all([
        dangKyThiDauService.getAll({ giaiDauId: tId }),
        tranDauService.getAll({ giaiDauId: tId }).catch(() => []),
      ]);
      setTournamentTeams(teamsRes || []);
      setAllTournamentMatches(matchesRes || []);
    } catch (err) {
      console.error('Lỗi khi tải danh sách đội toàn giải:', err);
    } finally {
      setLoadingTournamentTeams(false);
    }
  }, [selectedTournamentId]);

  useEffect(() => {
    loadTournamentData();
  }, [loadTournamentData]);

  // When selected tournament changes -> Load events / sports
  useEffect(() => {
    if (!selectedTournamentId) {
      setEvents([]);
      setSelectedEventId('');
      return;
    }
    const loadEvents = async () => {
      try {
        let sports: any[] = [];
        const tourDetail = await giaiDauService.getById(Number(selectedTournamentId));
        if (tourDetail?.monTheThaos && tourDetail.monTheThaos.length > 0) {
          sports = tourDetail.monTheThaos.map((m) => ({
            id: m.id,
            giaiDauMonTheThaoId: m.id,
            monTheThaoId: m.monTheThaoId,
            ten: m.ten,
            tenMonTheThao: m.ten,
            ma: m.ma,
            laMonDongDoi: m.laMonDongDoi,
            hinhThucThiDau: m.hinhThucThiDau || 'LoaiTrucTiep',
          }));
        } else {
          const data = await noiDungThiDauService.getAll({ giaiDauId: Number(selectedTournamentId) }).catch(() => []);
          sports = data || [];
        }

        setEvents(sports);
        if (sports.length > 0) {
          setSelectedEventId(sports[0].id);
        } else {
          setSelectedEventId('');
        }
      } catch (err) {
        console.error('Lỗi khi tải danh sách môn thi đấu:', err);
      }
    };
    loadEvents();
  }, [selectedTournamentId]);

  // When selected event changes -> Load all related data (Matches, Groups, Rounds, Venues, Referees, Registrations)
  const refreshEventData = useCallback(async () => {
    if (!selectedEventId) {
      setMatches([]);
      setGroups([]);
      setRounds([]);
      setRegisteredTeams([]);
      return;
    }

    setLoading(true);
    try {
      const eId = Number(selectedEventId);
      const [matchesRes, groupsRes, roundsRes, teamsRes, venuesRes, refereesRes] = await Promise.all([
        tranDauService.getAll({ giaiDauMonTheThaoId: eId }),
        bangDauService.getAll(eId),
        vongDauService.getAll(eId),
        dangKyThiDauService.getAll({ giaiDauMonTheThaoId: eId }),
        sanDauService.getAll(),
        trongTaiService.getAll(),
      ]);

      setMatches(matchesRes);
      setGroups(groupsRes);
      setRounds(roundsRes);
      setRegisteredTeams(teamsRes);
      setVenues(venuesRes);
      setReferees(refereesRes);

      // Pre-select venues compatible with sport if available
      const curEv = events.find((e) => e.id === eId);
      const monId = curEv?.monTheThaoId;
      const compatibleVenues = monId
        ? venuesRes.filter((v) => !v.monTheThaoId || v.monTheThaoId === monId)
        : venuesRes;

      if (monId) {
        setLoadingSportConfig(true);
        cauHinhLichThiDauService.getByMonTheThao(monId).then((config) => {
          setSportScheduleConfig(config || null);
          if (config) {
            setAutoScheduleConfig((prev) => {
              const calculatedRest = (config.khoangCachGiuaCacVongGio && config.khoangCachGiuaCacVongGio > 0)
                ? config.khoangCachGiuaCacVongGio * 60
                : (config.nghiToiThieuGiua2TranPhut || prev.thoiGianNghiToiThieuVdvPhut || 60);

              return {
                ...prev,
                startTime: config.caSangBatDau || prev.startTime,
                endTime: config.caChieuKetThuc || prev.endTime,
                matchDuration: config.thoiLuongTranMacDinhPhut || prev.matchDuration,
                breakDuration: config.thoiGianDemDonSanPhut ?? prev.breakDuration,
                soHiepDau: config.soHiepDauMacDinh || prev.soHiepDau,
                thoiGianMoiHiepPhut: config.thoiGianMoiHiepPhut || prev.thoiGianMoiHiepPhut,
                thoiGianNghiToiThieuVdvPhut: calculatedRest,
                selectedVenueIds: compatibleVenues.map((v) => v.id),
                selectedRefereeIds: refereesRes.map((r) => r.id),
              };
            });
          } else {
            setAutoScheduleConfig((prev) => ({
              ...prev,
              selectedVenueIds: compatibleVenues.map((v) => v.id),
              selectedRefereeIds: refereesRes.map((r) => r.id),
            }));
          }
        }).catch((err) => {
          console.error('Lỗi khi tải cấu hình lịch môn thể thao:', err);
          setSportScheduleConfig(null);
        }).finally(() => {
          setLoadingSportConfig(false);
        });
      } else {
        setSportScheduleConfig(null);
        setAutoScheduleConfig((prev) => ({
          ...prev,
          selectedVenueIds: compatibleVenues.map((v) => v.id),
          selectedRefereeIds: refereesRes.map((r) => r.id),
        }));
      }

      loadTournamentData();
    } catch (err) {
      console.error('Lỗi khi tải dữ liệu chi tiết của nội dung thi đấu:', err);
      toast.error('Không thể tải dữ liệu thi đấu.');
    } finally {
      setLoading(false);
    }
  }, [selectedEventId, events, loadTournamentData]);

  useEffect(() => {
    refreshEventData();
  }, [refreshEventData]);

  // KPI Calculations
  const totalMatches = matches.length;
  const scheduledVenues = matches.filter((m) => m.sanDauId).length;
  const assignedReferees = matches.filter((m) => m.danhSachTrongTai && m.danhSachTrongTai.length > 0).length;
  const uniqueVenuesUsed = new Set(matches.map((m) => m.sanDauId).filter(Boolean)).size;
  const uniqueRefereesUsed = new Set(
    matches.flatMap((m) => m.danhSachTrongTai?.map((r) => r.trongTaiId) || []).filter(Boolean)
  ).size;

  // Filtered Matches
  const filteredMatches = useMemo(() => {
    return matches.filter((m) => {
      if (filterDate && m.thoiGianDuKien) {
        const matchDay = new Date(m.thoiGianDuKien).toISOString().split('T')[0];
        if (matchDay !== filterDate) return false;
      }
      if (filterRoundId && m.vongDauId !== Number(filterRoundId)) return false;
      if (filterGroupId && m.bangDauId !== Number(filterGroupId)) return false;
      if (filterVenueId && m.sanDauId !== Number(filterVenueId)) return false;
      if (filterStatus && m.trangThai !== filterStatus) return false;
      if (searchKeyword) {
        const kw = searchKeyword.toLowerCase();
        const matchName = (m.tenTran || '').toLowerCase();
        const team1 = (m.tenDoi1 || '').toLowerCase();
        const team2 = (m.tenDoi2 || '').toLowerCase();
        const venue = (m.tenSanDau || '').toLowerCase();
        if (!matchName.includes(kw) && !team1.includes(kw) && !team2.includes(kw) && !venue.includes(kw)) {
          return false;
        }
      }
      return true;
    });
  }, [matches, filterDate, filterRoundId, filterGroupId, filterVenueId, filterStatus, searchKeyword]);

  // Unique match dates for filters and Court Matrix
  const uniqueDates = useMemo(() => {
    const dates = new Set<string>();
    matches.forEach((m) => {
      if (m.thoiGianDuKien) {
        dates.add(new Date(m.thoiGianDuKien).toISOString().split('T')[0]);
      }
    });
    return Array.from(dates).sort();
  }, [matches]);

  // Handle Auto-Schedule Submission
  const handleAutoSchedule = async () => {
    if (!selectedEventId) {
      toast.warning('Vui lòng chọn một nội dung thi đấu.');
      return;
    }
    if (registeredTeams.length < 2) {
      toast.warning('Cần có ít nhất 2 đội/VĐV đã duyệt đăng ký để xếp lịch.');
      return;
    }
    if (autoScheduleConfig.selectedVenueIds.length === 0) {
      toast.warning('Vui lòng chọn ít nhất 1 sân đấu.');
      return;
    }

    setAutoScheduleSubmitting(true);
    try {
      const tStart = currentTournament?.ngayBatDau
        ? new Date(currentTournament.ngayBatDau).toISOString().split('T')[0]
        : (autoScheduleConfig.startDate || new Date().toISOString().split('T')[0]);

      const effectiveMatchDuration = sportScheduleConfig?.thoiLuongTranMacDinhPhut
        ? sportScheduleConfig.thoiLuongTranMacDinhPhut
        : (sportScheduleConfig?.soHiepDauMacDinh && sportScheduleConfig?.thoiGianMoiHiepPhut
            ? sportScheduleConfig.soHiepDauMacDinh * sportScheduleConfig.thoiGianMoiHiepPhut
            : (autoScheduleConfig.matchDuration || 60));

      const payload: AutoScheduleRequest = {
        giaiDauMonTheThaoId: Number(selectedEventId),
        noiDungThiDauId: Number(selectedEventId),
        ngayBatDau: tStart,
        gioBatDauMoiNgay: sportScheduleConfig?.caSangBatDau || autoScheduleConfig.startTime || '08:00',
        gioKetThucMoiNgay: sportScheduleConfig?.caChieuKetThuc || autoScheduleConfig.endTime || '17:30',
        thoiLuongTranPhut: Number(effectiveMatchDuration),
        nghiGiuaTranPhut: Number(sportScheduleConfig?.thoiGianDemDonSanPhut ?? autoScheduleConfig.breakDuration ?? 15),
        sanDauIds: autoScheduleConfig.selectedVenueIds,
        trongTaiIds: autoScheduleConfig.selectedRefereeIds,
        soTrongTaiMoiTran: Number(autoScheduleConfig.refereesPerMatch),
        taoBangDauNeuChuaCo: hasGroupStage ? autoScheduleConfig.autoCreateGroups : false,
        soDoiMoiBang: Number(autoScheduleConfig.teamsPerGroup),
        xoaLichCu: autoScheduleConfig.clearOldMatches,
        tranhTrungLichVdv: autoScheduleConfig.avoidAthleteConflict,
        thoiGianNghiToiThieuVdvPhut: Number(effectiveRestInfo.minutes),
        khoangCachGiuaCacVongGio: Number(sportScheduleConfig?.khoangCachGiuaCacVongGio ?? 0),
        canBangTaiSanDau: autoScheduleConfig.canBangTaiSanDau,
        canBangTaiTrongTai: autoScheduleConfig.canBangTaiTrongTai,
        soHiepDau: Number(sportScheduleConfig?.soHiepDauMacDinh ?? autoScheduleConfig.soHiepDau ?? 0),
        thoiGianMoiHiepPhut: Number(sportScheduleConfig?.thoiGianMoiHiepPhut ?? autoScheduleConfig.thoiGianMoiHiepPhut ?? 0),
        soDoiMoiBangVaoVongTrong: currentEvent?.hinhThucThiDau === 'KetHopVongBangVaLoaiTrucTiep'
          ? Number(autoScheduleConfig.soDoiMoiBangVaoVongTrong || 2)
          : 2,
        soDoiThu3TotNhat: (currentEvent?.hinhThucThiDau === 'KetHopVongBangVaLoaiTrucTiep' && autoScheduleConfig.layDoiThu3TotNhat)
          ? Number(autoScheduleConfig.soDoiThu3TotNhat || 0)
          : 0,
        // Leaderboard parameters (TinhDiemXepHang)
        soVdvMoiLuotThi: isLeaderboard ? Number(autoScheduleConfig.soVdvMoiLuotThi || 8) : undefined,
        soVongThi: isLeaderboard ? Number(autoScheduleConfig.soVongThi || 1) : undefined,
        phuongThucPhanNhom: isLeaderboard ? autoScheduleConfig.phuongThucPhanNhom : undefined,
        // Round Robin parameters (VongBang = 2)
        soLuotDau: isRoundRobin ? Number(autoScheduleConfig.soLuotDau || 1) : 1,
        cheDoVongBang: isRoundRobin ? autoScheduleConfig.cheDoVongBang : undefined,
        diemThang: isRoundRobin ? Number(autoScheduleConfig.diemThang ?? 3) : undefined,
        diemHoa: isRoundRobin ? Number(autoScheduleConfig.diemHoa ?? 1) : undefined,
        diemThua: isRoundRobin ? Number(autoScheduleConfig.diemThua ?? 0) : undefined,
      };

      const res = await tranDauService.autoSchedule(payload);
      if (res.success) {
        toast.success(res.message || `Đã xếp lịch thành công ${res.totalMatchesCreated} trận!`);
        if (res.warnings && res.warnings.length > 0) {
          toast.warning(`Lưu ý xếp lịch: ${res.warnings.join(' | ')}`);
        }
        setAutoScheduleModalOpen(false);
        refreshEventData();
      } else {
        toast.error(res.message || 'Xếp lịch thất bại.');
      }
    } catch (err: any) {
      console.error('Lỗi khi xếp lịch tự động:', err);
      toast.error(err?.response?.data?.message || 'Không thể thực hiện xếp lịch tự động.');
    } finally {
      setAutoScheduleSubmitting(false);
    }
  };

  // Open Create Manual Match Modal
  const handleOpenCreateMatch = () => {
    setEditingMatchId(null);
    setConflictWarning([]);
    setChiTietConflictWarning([]);
    setMatchForm({
      roundId: rounds.length > 0 ? rounds[0].id : '',
      groupId: groups.length > 0 ? groups[0].id : '',
      doi1Id: '',
      doi2Id: '',
      venueId: venues.length > 0 ? venues[0].id : '',
      matchDate: uniqueDates[0] || new Date().toISOString().split('T')[0],
      startTime: '08:30',
      endTime: '09:30',
      matchNumber: matches.length + 1,
      matchName: `Trận ${matches.length + 1}`,
      status: 'ChuaDau',
      notes: '',
      refereeAssignments: referees.length > 0 ? [{ trongTaiId: referees[0].id, vaiTro: 'TrongTaiChinh' }] : [],
    });
    setManualMatchModalOpen(true);
  };

  // Open Edit Manual Match Modal
  const handleOpenEditMatch = (m: TranDau) => {
    setEditingMatchId(m.id);
    setConflictWarning([]);
    setChiTietConflictWarning([]);

    const matchDateStr = m.thoiGianBatDau
      ? new Date(m.thoiGianBatDau).toISOString().split('T')[0]
      : new Date().toISOString().split('T')[0];
    const startTimeStr = m.thoiGianBatDau
      ? new Date(m.thoiGianBatDau).toTimeString().substring(0, 5)
      : '08:30';
    const endTimeStr = m.thoiGianKetThuc
      ? new Date(m.thoiGianKetThuc).toTimeString().substring(0, 5)
      : '09:30';

    setMatchForm({
      roundId: m.vongDauId,
      groupId: m.bangDauId || '',
      doi1Id: m.doi1DangKyId || '',
      doi2Id: m.doi2DangKyId || '',
      venueId: m.sanDauId || '',
      matchDate: matchDateStr,
      startTime: startTimeStr,
      endTime: endTimeStr,
      matchNumber: m.soTran,
      matchName: m.tenTran || '',
      status: m.trangThai,
      notes: m.ghiChu || '',
      refereeAssignments: m.danhSachTrongTai?.map((r) => ({
        trongTaiId: r.trongTaiId,
        vaiTro: r.vaiTro,
        ghiChu: r.ghiChu,
      })) || [],
    });
    setManualMatchModalOpen(true);
  };

  // Check Conflict in Manual Match
  const handleCheckConflict = async (showSuccessToast = true): Promise<boolean> => {
    if (!matchForm.venueId || !matchForm.matchDate || !matchForm.startTime || !matchForm.endTime) {
      if (showSuccessToast) toast.warning('Vui lòng chọn Sân đấu, Ngày và Giờ thi đấu để kiểm tra.');
      return false;
    }
    setCheckingConflict(true);
    try {
      const startDateTime = `${matchForm.matchDate}T${matchForm.startTime}:00`;
      const endDateTime = `${matchForm.matchDate}T${matchForm.endTime}:00`;

      const res = await tranDauService.checkConflict({
        tranDauId: editingMatchId || undefined,
        sanDauId: Number(matchForm.venueId),
        thoiGianBatDau: startDateTime,
        thoiGianKetThuc: endDateTime,
        trongTaiIds: matchForm.refereeAssignments.map((r) => r.trongTaiId),
        dangKyThiDauIds: [Number(matchForm.doi1Id), Number(matchForm.doi2Id)].filter(Boolean),
      });

      if (res.hasConflict) {
        setConflictWarning(res.conflicts || []);
        setChiTietConflictWarning(res.chiTietXungDot || []);
        return true;
      } else {
        setConflictWarning([]);
        setChiTietConflictWarning([]);
        if (showSuccessToast) {
          toast.success('Không có xung đột nào về Sân đấu, Trọng tài hoặc Vận động viên!');
        }
        return false;
      }
    } catch (err) {
      console.error('Lỗi khi kiểm tra xung đột:', err);
      return false;
    } finally {
      setCheckingConflict(false);
    }
  };

  // Save Manual Match
  const handleSaveManualMatch = async () => {
    if (!selectedEventId) return;
    if (!matchForm.roundId) {
      toast.warning('Vui lòng chọn Vòng đấu.');
      return;
    }
    if (!matchForm.doi1Id || !matchForm.doi2Id) {
      toast.warning('Vui lòng chọn đủ 2 đội tham gia trận đấu.');
      return;
    }
    if (matchForm.doi1Id === matchForm.doi2Id) {
      toast.warning('Hai đội thi đấu phải khác nhau.');
      return;
    }

    // Tự động kiểm tra xung đột VĐV / Sân / Trọng tài trước khi lưu
    if (matchForm.venueId && matchForm.matchDate && matchForm.startTime && matchForm.endTime) {
      const hasConflict = await handleCheckConflict(false);
      if (hasConflict) {
        const confirmSave = window.confirm(
          'CẢNH BÁO: Phát hiện xung đột lịch thi đấu (Sân đấu, Trọng tài hoặc VĐV thi đấu nhiều môn)!\nBạn có chắc chắn vẫn muốn lưu trận đấu này không?'
        );
        if (!confirmSave) return;
      }
    }

    try {
      const startDateTime = `${matchForm.matchDate}T${matchForm.startTime}:00`;
      const endDateTime = `${matchForm.matchDate}T${matchForm.endTime}:00`;

      const payload: CreateUpdateTranDau = {
        giaiDauMonTheThaoId: Number(selectedEventId),
        noiDungThiDauId: Number(selectedEventId),
        vongDauId: Number(matchForm.roundId),
        bangDauId: matchForm.groupId ? Number(matchForm.groupId) : null,
        sanDauId: matchForm.venueId ? Number(matchForm.venueId) : null,
        soTran: Number(matchForm.matchNumber),
        tenTran: matchForm.matchName,
        thoiGianDuKien: startDateTime,
        thoiGianBatDau: startDateTime,
        thoiGianKetThuc: endDateTime,
        trangThai: matchForm.status,
        ghiChu: matchForm.notes,
        doi1DangKyId: Number(matchForm.doi1Id),
        doi2DangKyId: Number(matchForm.doi2Id),
        danhSachTrongTai: matchForm.refereeAssignments,
      };

      if (editingMatchId) {
        await tranDauService.update(editingMatchId, payload);
        toast.success('Cập nhật trận đấu thành công!');
      } else {
        await tranDauService.create(payload);
        toast.success('Tạo trận đấu mới thành công!');
      }

      setManualMatchModalOpen(false);
      refreshEventData();
    } catch (err: any) {
      console.error('Lỗi lưu trận đấu:', err);
      toast.error(err?.response?.data?.message || 'Không thể lưu trận đấu.');
    }
  };

  // Tournament Conflict Audit Handler
  const handleCheckTournamentConflicts = async () => {
    if (!selectedTournamentId) {
      toast.warning('Vui lòng chọn một giải đấu để rà soát.');
      return;
    }
    setCheckingTournamentConflict(true);
    setTournamentConflictModalOpen(true);
    try {
      const report = await tranDauService.checkAllConflicts(Number(selectedTournamentId));
      setTournamentConflictReport(report);
    } catch (err: any) {
      console.error('Lỗi khi kiểm tra xung đột giải đấu:', err);
      toast.error('Không thể kiểm tra xung đột toàn giải.');
    } finally {
      setCheckingTournamentConflict(false);
    }
  };

  // Delete Match
  const handleDeleteMatch = async (id: number) => {
    if (!window.confirm('Bạn có chắc chắn muốn xóa trận đấu này?')) return;
    try {
      await tranDauService.delete(id);
      toast.success('Đã xóa trận đấu.');
      refreshEventData();
    } catch (err) {
      toast.error('Không thể xóa trận đấu.');
    }
  };

  // Clear All Matches
  const handleClearAllMatches = async () => {
    if (!selectedEventId) return;
    if (!window.confirm('CẢNH BÁO: Thao tác này sẽ xóa toàn bộ lịch thi đấu và phân công trọng tài đã tạo của nội dung này! Bạn có chắc chắn không?')) {
      return;
    }
    try {
      await tranDauService.clearByNoiDung(Number(selectedEventId));
      toast.success('Đã xóa toàn bộ lịch thi đấu của nội dung.');
      refreshEventData();
    } catch (err) {
      toast.error('Không thể xóa lịch.');
    }
  };

  // Auto Distribute Groups
  const handleAutoDistributeGroups = async () => {
    if (!selectedEventId) return;
    try {
      await bangDauService.autoDistribute({
        noiDungThiDauId: Number(selectedEventId),
        soBang: Number(distributeGroupCount),
        tienToBang: 'Bảng ',
      });
      toast.success(`Đã bốc thăm chia ${distributeGroupCount} bảng đấu thành công!`);
      setGroupModalOpen(false);
      refreshEventData();
    } catch (err: any) {
      toast.error(err?.response?.data?.message || 'Không thể chia bảng.');
    }
  };

  // Available Teams for match form (filtered by group if group selected)
  const availableTeamsForMatch = useMemo(() => {
    if (matchForm.groupId) {
      const grp = groups.find((g) => g.id === Number(matchForm.groupId));
      if (grp && grp.thanhViens && grp.thanhViens.length > 0) {
        const teamIds = grp.thanhViens.map((m) => m.dangKyThiDauId);
        return registeredTeams.filter((t) => teamIds.includes(t.id));
      }
    }
    return registeredTeams;
  }, [matchForm.groupId, groups, registeredTeams]);

  // Event teams count map (number of teams per event)
  const eventTeamsCountMap = useMemo(() => {
    const map = new Map<number, number>();
    tournamentTeams.forEach((t) => {
      if (t.noiDungThiDauId) {
        map.set(t.noiDungThiDauId, (map.get(t.noiDungThiDauId) || 0) + 1);
      }
    });
    return map;
  }, [tournamentTeams]);

  // Team match count helper
  const getTeamMatchCount = useCallback(
    (teamId: number, scope: 'current_event' | 'tournament') => {
      const sourceMatches = scope === 'current_event' ? matches : allTournamentMatches;
      return sourceMatches.filter((m) => m.doi1DangKyId === teamId || m.doi2DangKyId === teamId).length;
    },
    [matches, allTournamentMatches]
  );

  // Team matches list helper
  const getTeamMatchesList = useCallback(
    (teamId: number, scope: 'current_event' | 'tournament') => {
      const sourceMatches = scope === 'current_event' ? matches : allTournamentMatches;
      return sourceMatches.filter((m) => m.doi1DangKyId === teamId || m.doi2DangKyId === teamId);
    },
    [matches, allTournamentMatches]
  );

  // Team metrics for Team Tab
  const teamMetrics = useMemo(() => {
    const source = teamScope === 'current_event' ? registeredTeams : tournamentTeams;
    const totalTeams = source.length;
    let totalVdv = 0;
    let teamsWithMatches = 0;
    let teamsWithoutMatches = 0;

    source.forEach((t) => {
      totalVdv += t.soVdv || t.vanDongVienNames?.length || 0;
      const matchCount = getTeamMatchCount(t.id, teamScope);
      if (matchCount > 0) {
        teamsWithMatches++;
      } else {
        teamsWithoutMatches++;
      }
    });

    return { totalTeams, totalVdv, teamsWithMatches, teamsWithoutMatches };
  }, [teamScope, registeredTeams, tournamentTeams, getTeamMatchCount]);

  // Distinct units for filter
  const distinctUnits = useMemo(() => {
    const source = teamScope === 'current_event' ? registeredTeams : tournamentTeams;
    const units = new Set<string>();
    source.forEach((t) => {
      if (t.tenDonVi) units.add(t.tenDonVi);
    });
    return Array.from(units).sort();
  }, [teamScope, registeredTeams, tournamentTeams]);

  // Displayed teams with filtering & search
  const displayedTeams = useMemo(() => {
    const source = teamScope === 'current_event' ? registeredTeams : tournamentTeams;
    return source.filter((t) => {
      if (teamFilterEventId && t.noiDungThiDauId !== Number(teamFilterEventId)) return false;
      if (teamFilterDonVi && t.tenDonVi !== teamFilterDonVi) return false;
      if (teamFilterStatus && t.trangThai !== teamFilterStatus) return false;
      if (teamSearch) {
        const kw = teamSearch.toLowerCase();
        const teamName = (t.tenDoi || t.tenDangKy || '').toLowerCase();
        const regNo = (t.soDangKy || '').toLowerCase();
        const unit = (t.tenDonVi || '').toLowerCase();
        const athletes = (t.vanDongVienNames || []).join(' ').toLowerCase();
        const eventName = (t.tenNoiDung || '').toLowerCase();
        const sportName = (t.tenMonTheThao || '').toLowerCase();
        if (
          !teamName.includes(kw) &&
          !regNo.includes(kw) &&
          !unit.includes(kw) &&
          !athletes.includes(kw) &&
          !eventName.includes(kw) &&
          !sportName.includes(kw)
        ) {
          return false;
        }
      }
      const matchCount = getTeamMatchCount(t.id, teamScope);
      if (teamFilterScheduled === 'has_matches' && matchCount === 0) return false;
      if (teamFilterScheduled === 'no_matches' && matchCount > 0) return false;
      return true;
    });
  }, [
    teamScope,
    registeredTeams,
    tournamentTeams,
    teamFilterEventId,
    teamFilterDonVi,
    teamFilterStatus,
    teamFilterScheduled,
    teamSearch,
    getTeamMatchCount,
  ]);

  // Group lookup map for current event teams
  const teamGroupMap = useMemo(() => {
    const map = new Map<number, string>();
    groups.forEach((g) => {
      g.thanhViens?.forEach((tv) => {
        if (tv.dangKyThiDauId) {
          map.set(tv.dangKyThiDauId, g.ten);
        }
      });
    });
    return map;
  }, [groups]);

  // Handlers for teams
  const handleViewTeamMatches = (team: DangKyThiDau) => {
    const targetEventId = team.giaiDauMonTheThaoId ?? team.noiDungThiDauId ?? '';
    if (targetEventId && targetEventId !== Number(selectedEventId)) {
      setSelectedEventId(targetEventId);
    }
    setSearchKeyword(team.tenDoi || team.tenDangKy || '');
    setActiveTab('list');
  };

  const handleQuickScheduleMatchForTeam = (team: DangKyThiDau) => {
    const targetEventId = team.giaiDauMonTheThaoId ?? team.noiDungThiDauId ?? '';
    if (targetEventId && targetEventId !== Number(selectedEventId)) {
      setSelectedEventId(targetEventId);
    }
    setEditingMatchId(null);
    setConflictWarning([]);
    setChiTietConflictWarning([]);
    setMatchForm({
      roundId: rounds.length > 0 ? rounds[0].id : '',
      groupId: '',
      doi1Id: team.id,
      doi2Id: '',
      venueId: venues.length > 0 ? venues[0].id : '',
      matchDate: uniqueDates[0] || new Date().toISOString().split('T')[0],
      startTime: '08:30',
      endTime: '09:30',
      matchNumber: matches.length + 1,
      matchName: `Trận ${matches.length + 1}`,
      status: 'ChuaDau',
      notes: '',
      refereeAssignments: referees.length > 0 ? [{ trongTaiId: referees[0].id, vaiTro: 'TrongTaiChinh' }] : [],
    });
    setManualMatchModalOpen(true);
  };

  const handleOpenTeamDetail = (team: DangKyThiDau) => {
    setSelectedTeamDetail(team);
    setTeamDetailModalOpen(true);
  };

  return (
    <div className="d-flex flex-column gap-4 pb-5">
      {/* 1. Header Banner & Selectors */}
      <div
        className="rounded-4 p-4 text-white position-relative overflow-hidden shadow-sm"
        style={{
          background: 'linear-gradient(135deg, #0f172a 0%, #1e293b 40%, #0369a1 100%)',
        }}
      >
        <div className="d-flex flex-column flex-lg-row align-items-lg-center justify-content-between gap-3 position-relative" style={{ zIndex: 2 }}>
          <div>
            <div className="d-inline-flex align-items-center gap-2 px-3 py-1 rounded-pill mb-2 border" style={{ backgroundColor: 'rgba(255,255,255,0.12)', fontSize: '12px' }}>
              <Calendar size={14} className="text-warning" />
              <span className="fw-semibold">Trung Tâm Xếp Lịch & Điều Hành Thi Đấu</span>
            </div>
            <h3 className="fw-bold mb-1 fs-4 text-white">
              Xếp Lịch Thi Đấu & Phân Công Trọng Tài
            </h3>
            <p className="text-white-50 mb-0 small" style={{ maxWidth: '640px' }}>
              Hệ thống xếp lịch thi đấu thông minh tự động tối ưu theo Sân đấu, Trọng tài điều hành, Vòng đấu và Bảng đấu theo thể thức thi đấu.
            </p>
          </div>

          {/* Tournament & Event Selectors */}
          <div className="bg-dark bg-opacity-50 p-3 rounded-4 border border-secondary border-opacity-25 d-flex flex-column flex-sm-row gap-3">
            <div style={{ minWidth: '220px' }}>
              <Label className="small text-white-50 mb-1 fw-medium">Chọn Giải Đấu</Label>
              <Input
                type="select"
                className="form-select form-select-sm bg-dark text-white border-secondary rounded-3"
                value={selectedTournamentId}
                onChange={(e) => setSelectedTournamentId(e.target.value ? Number(e.target.value) : '')}
              >
                {tournaments.map((t) => (
                  <option key={t.id} value={t.id}>
                    {t.ten}
                  </option>
                ))}
              </Input>
            </div>

            <div style={{ minWidth: '240px' }}>
              <Label className="small text-white-50 mb-1 fw-medium">Nội Dung / Môn Thi Đấu</Label>
              <Input
                type="select"
                className="form-select form-select-sm bg-dark text-white border-secondary rounded-3"
                value={selectedEventId}
                onChange={(e) => setSelectedEventId(e.target.value ? Number(e.target.value) : '')}
                disabled={events.length === 0}
              >
                {events.map((ev) => (
                  <option key={ev.id} value={ev.id}>
                    {ev.ten} ({ev.tenMonTheThao || 'Môn'})
                  </option>
                ))}
              </Input>
            </div>
          </div>
        </div>

        {/* Selected Event Details Strip */}
        {currentEvent && (
          <div className="mt-3 pt-3 border-top border-white border-opacity-10 d-flex flex-wrap align-items-center gap-3" style={{ fontSize: '13px' }}>
            <div className="d-flex align-items-center gap-1.5 text-white-50">
              <Trophy size={14} className="text-warning" />
              <span>Hình thức:</span>
              <Badge color="info" pill className="px-2.5 py-1 text-white">
                {HinhThucThiDauLabels[currentEvent.hinhThucThiDau || ''] || currentEvent.hinhThucThiDau || 'Loại trực tiếp'}
              </Badge>
            </div>

            <div className="d-flex align-items-center gap-1.5 text-white-50">
              <Users size={14} className="text-success" />
              <span>Đội đã duyệt:</span>
              <strong className="text-white">{registeredTeams.length}</strong>
            </div>

            {hasGroupStage && (
              <div className="d-flex align-items-center gap-1.5 text-white-50">
                <Layers size={14} className="text-info" />
                <span>Số bảng đấu:</span>
                <strong className="text-white">{groups.length} bảng</strong>
              </div>
            )}

            {isKnockout && (
              <div className="d-flex align-items-center gap-1.5 text-white-50">
                <Award size={14} className="text-warning" />
                <span>Thể thức:</span>
                <strong className="text-warning">Loại trực tiếp (Knockout)</strong>
              </div>
            )}

            {isRoundRobin && (
              <div className="d-flex align-items-center gap-1.5 text-white-50">
                <Award size={14} className="text-success" />
                <span>Thể thức:</span>
                <strong className="text-success">🔄 Vòng Tròn Tính Điểm (Round Robin)</strong>
              </div>
            )}

            {currentEvent?.hinhThucThiDau === 'KetHopVongBangVaLoaiTrucTiep' && (
              <div className="d-flex align-items-center gap-1.5 text-white-50">
                <Award size={14} className="text-info" />
                <span>Thể thức:</span>
                <strong className="text-info">🏆 Vòng Bảng + Knockout</strong>
              </div>
            )}

            {isLeaderboard && (
              <div className="d-flex align-items-center gap-1.5 text-white-50">
                <Award size={14} className="text-info" />
                <span>Thể thức:</span>
                <strong className="text-info">🏊 Thi Thành Tích (Leaderboard)</strong>
              </div>
            )}

            <div className="d-flex align-items-center gap-1.5 text-white-50">
              <MapPin size={14} className="text-danger" />
              <span>Sân khả dụng:</span>
              <strong className="text-white">{venues.length} sân</strong>
            </div>

            <div className="d-flex align-items-center gap-1.5 text-white-50 ms-auto">
              <Button
                color="light"
                size="sm"
                className="rounded-pill px-3 py-1 d-flex align-items-center gap-1.5 fw-semibold"
                style={{ fontSize: '12px' }}
                onClick={refreshEventData}
              >
                <RefreshCw size={13} className={loading ? 'spin' : ''} />
                Làm mới
              </Button>
            </div>
          </div>
        )}
      </div>

      {/* 1.5. Events Overview & Selector Strip */}
      {events.length > 0 && (
        <Card className="border-0 shadow-sm rounded-4 overflow-hidden">
          <CardBody className="p-3">
            <div className="d-flex flex-column flex-md-row align-items-md-center justify-content-between gap-2 mb-3">
              <div className="d-flex align-items-center gap-2">
                <div className="p-2 rounded-3 bg-warning bg-opacity-10 text-warning d-flex align-items-center justify-content-center">
                  <Trophy size={18} />
                </div>
                <div>
                  <h6 className="fw-bold mb-0 text-dark">Môn Thi Đấu Trong Giải ({events.length})</h6>
                  <small className="text-muted">Chọn môn thi đấu để xem lịch thi đấu, phân công sân và danh sách đội đăng ký</small>
                </div>
              </div>

              <div className="d-flex align-items-center gap-2">
                <Badge color="light" className="text-secondary border px-3 py-1.5 rounded-pill fw-medium">
                  Tổng cộng: <strong className="text-dark">{tournamentTeams.length}</strong> đội đăng ký
                </Badge>
                <Button
                  color="outline-primary"
                  size="sm"
                  className="rounded-pill px-3 py-1 fw-semibold d-flex align-items-center gap-1.5"
                  style={{ fontSize: '12px' }}
                  onClick={() => {
                    setTeamScope('tournament');
                    setActiveTab('teams');
                  }}
                >
                  <Users size={14} />
                  <span>Xem Tất Cả Đội ({tournamentTeams.length})</span>
                </Button>
              </div>
            </div>

            {/* Horizontal responsive list of event cards */}
            <div className="d-flex gap-2.5 overflow-auto pb-1" style={{ scrollbarWidth: 'thin' }}>
              {events.map((ev) => {
                const isSelected = ev.id === Number(selectedEventId);
                const teamCount = eventTeamsCountMap.get(ev.id) ?? 0;
                return (
                  <div
                    key={ev.id}
                    onClick={() => setSelectedEventId(ev.id)}
                    className={`p-3 rounded-3 cursor-pointer transition-all flex-shrink-0 d-flex flex-column justify-content-between ${
                      isSelected
                        ? 'border-2 border-primary bg-primary bg-opacity-10 shadow-sm'
                        : 'border bg-white hover-shadow'
                    }`}
                    style={{
                      minWidth: '220px',
                      maxWidth: '280px',
                      borderColor: isSelected ? '#2563eb' : '#e2e8f0',
                      transition: 'all 0.2s ease',
                    }}
                  >
                    <div>
                      <div className="d-flex align-items-center justify-content-between gap-1 mb-1.5">
                        <Badge color={isSelected ? 'primary' : 'secondary'} pill className="px-2 py-0.5" style={{ fontSize: '11px' }}>
                          {ev.tenMonTheThao || 'Môn thi'}
                        </Badge>
                        {isSelected && (
                          <Badge color="primary" pill className="p-1 d-flex align-items-center justify-content-center">
                            <CheckCircle2 size={12} />
                          </Badge>
                        )}
                      </div>
                      <div className="fw-bold text-dark text-truncate mb-1" title={ev.ten} style={{ fontSize: '13.5px' }}>
                        {ev.ten}
                      </div>
                      <div className="text-muted small text-truncate mb-2" style={{ fontSize: '11.5px' }}>
                        {HinhThucThiDauLabels[ev.hinhThucThiDau || ''] || ev.hinhThucThiDau || 'Loại trực tiếp'}
                      </div>
                    </div>

                    <div className="d-flex align-items-center justify-content-between pt-2 border-top border-secondary border-opacity-10 mt-auto">
                      <span className="d-flex align-items-center gap-1 small text-secondary" style={{ fontSize: '12px' }}>
                        <Users size={13} className={teamCount > 0 ? 'text-primary' : 'text-muted'} />
                        <strong className={teamCount > 0 ? 'text-dark' : 'text-muted'}>{teamCount}</strong> đội
                      </span>
                      <span className="small text-primary fw-medium" style={{ fontSize: '11px' }}>
                        {isSelected ? 'Đang chọn' : 'Chọn xem'}
                      </span>
                    </div>
                  </div>
                );
              })}
            </div>
          </CardBody>
        </Card>
      )}

      {/* 2. 4 KPI Overview Cards */}
      <Row className="g-3">
        <Col xs={6} md={3}>
          <div className="bg-white rounded-4 p-3 border shadow-sm d-flex align-items-center gap-3">
            <div className="rounded-3 d-flex align-items-center justify-content-center flex-shrink-0" style={{ width: '44px', height: '44px', backgroundColor: '#eef2ff', color: '#4f46e5' }}>
              <CalendarDays size={22} />
            </div>
            <div>
              <span className="text-secondary small d-block" style={{ fontSize: '11px', fontWeight: 500 }}>
                Tổng số trận đấu
              </span>
              <span className="fw-bold fs-4 text-dark">{totalMatches}</span>
            </div>
          </div>
        </Col>

        <Col xs={6} md={3}>
          <div className="bg-white rounded-4 p-3 border shadow-sm d-flex align-items-center gap-3">
            <div className="rounded-3 d-flex align-items-center justify-content-center flex-shrink-0" style={{ width: '44px', height: '44px', backgroundColor: '#ecfdf5', color: '#059669' }}>
              <MapPin size={22} />
            </div>
            <div>
              <span className="text-secondary small d-block" style={{ fontSize: '11px', fontWeight: 500 }}>
                Sân đã bố trí
              </span>
              <span className="fw-bold fs-4 text-success">
                {scheduledVenues} / {totalMatches}
              </span>
            </div>
          </div>
        </Col>

        <Col xs={6} md={3}>
          <div className="bg-white rounded-4 p-3 border shadow-sm d-flex align-items-center gap-3">
            <div className="rounded-3 d-flex align-items-center justify-content-center flex-shrink-0" style={{ width: '44px', height: '44px', backgroundColor: '#fffbeb', color: '#d97706' }}>
              <Shield size={22} />
            </div>
            <div>
              <span className="text-secondary small d-block" style={{ fontSize: '11px', fontWeight: 500 }}>
                Trận có trọng tài
              </span>
              <span className="fw-bold fs-4 text-warning">
                {assignedReferees} / {totalMatches}
              </span>
            </div>
          </div>
        </Col>

        <Col xs={6} md={3}>
          <div className="bg-white rounded-4 p-3 border shadow-sm d-flex align-items-center gap-3">
            <div className="rounded-3 d-flex align-items-center justify-content-center flex-shrink-0" style={{ width: '44px', height: '44px', backgroundColor: '#f0fdfa', color: '#0d9488' }}>
              <UserCheck size={22} />
            </div>
            <div>
              <span className="text-secondary small d-block" style={{ fontSize: '11px', fontWeight: 500 }}>
                Trọng tài tham gia
              </span>
              <span className="fw-bold fs-4 text-dark">
                {uniqueRefereesUsed} người
              </span>
            </div>
          </div>
        </Col>
      </Row>

      {/* 3. Action Toolbar & Filter Bar */}
      <Card className="border-0 shadow-sm rounded-4">
        <CardBody className="p-3">
          <div className="d-flex flex-column flex-md-row align-items-md-center justify-content-between gap-3">
            {/* Action buttons */}
            <div className="d-flex flex-wrap align-items-center gap-2">
              <Button
                color="primary"
                className="rounded-pill px-3 py-2 fw-semibold d-flex align-items-center gap-2 shadow-sm"
                style={{ fontSize: '13px' }}
                onClick={() => setAutoScheduleModalOpen(true)}
                disabled={!selectedEventId}
              >
                <Zap size={16} />
                <span>Xếp Lịch Tự Động (Auto-Schedule)</span>
              </Button>

              <Button
                color="success"
                className="rounded-pill px-3 py-2 fw-semibold d-flex align-items-center gap-2 text-white shadow-sm"
                style={{ fontSize: '13px' }}
                onClick={handleOpenCreateMatch}
                disabled={!selectedEventId}
              >
                <Plus size={16} />
                <span>Thêm Trận Đấu</span>
              </Button>

              {hasGroupStage && (
                <Button
                  color="info"
                  className="rounded-pill px-3 py-2 fw-semibold d-flex align-items-center gap-2 text-white shadow-sm"
                  style={{ fontSize: '13px' }}
                  onClick={() => setGroupModalOpen(true)}
                  disabled={!selectedEventId}
                >
                  <Shuffle size={16} />
                  <span>Chia Bảng / Bốc Thăm</span>
                </Button>
              )}

              {totalMatches > 0 && (
                <Button
                  color="outline-danger"
                  className="rounded-pill px-3 py-2 fw-semibold d-flex align-items-center gap-1.5"
                  style={{ fontSize: '13px' }}
                  onClick={handleClearAllMatches}
                >
                  <Trash2 size={15} />
                  <span>Xóa Hết Lịch</span>
                </Button>
              )}

              <Button
                color="warning"
                className="rounded-pill px-3 py-2 fw-semibold d-flex align-items-center gap-1.5 text-dark shadow-sm ms-md-auto"
                style={{ fontSize: '13px' }}
                onClick={handleCheckTournamentConflicts}
                disabled={!selectedTournamentId || checkingTournamentConflict}
                title="Rà soát toàn bộ xung đột lịch thi đấu giữa các môn, sân và VĐV trong giải"
              >
                {checkingTournamentConflict ? <Spinner size="sm" /> : <ShieldAlert size={16} className="text-danger" />}
                <span>Rà Soát Xung Đột Toàn Giải</span>
              </Button>
            </div>

            {/* Quick search input */}
            <div className="position-relative" style={{ minWidth: '240px' }}>
              <Search size={15} className="position-absolute text-muted" style={{ left: '12px', top: '50%', transform: 'translateY(-50%)' }} />
              <Input
                type="text"
                placeholder="Tìm trận, đội, sân..."
                className="form-control form-control-sm rounded-pill ps-5"
                style={{ fontSize: '13px' }}
                value={searchKeyword}
                onChange={(e) => setSearchKeyword(e.target.value)}
              />
            </div>
          </div>

          {/* Filter Row */}
          <div className="d-flex flex-wrap align-items-center gap-2 mt-3 pt-3 border-top">
            <span className="small text-muted fw-semibold d-flex align-items-center gap-1 me-1">
              <Filter size={13} /> Lọc:
            </span>

            {/* Date filter */}
            <Input
              type="select"
              bsSize="sm"
              className="rounded-pill"
              style={{ width: 'auto', fontSize: '12px' }}
              value={filterDate}
              onChange={(e) => setFilterDate(e.target.value)}
            >
              <option value="">Tất cả ngày</option>
              {uniqueDates.map((d) => (
                <option key={d} value={d}>
                  Ngày {d}
                </option>
              ))}
            </Input>

            {/* Round filter */}
            <Input
              type="select"
              bsSize="sm"
              className="rounded-pill"
              style={{ width: 'auto', fontSize: '12px' }}
              value={filterRoundId}
              onChange={(e) => setFilterRoundId(e.target.value)}
            >
              <option value="">Tất cả vòng đấu</option>
              {rounds.map((r) => (
                <option key={r.id} value={r.id}>
                  {r.ten}
                </option>
              ))}
            </Input>

            {/* Group filter (if group stage exists) */}
            {hasGroupStage && groups.length > 0 && (
              <Input
                type="select"
                bsSize="sm"
                className="rounded-pill"
                style={{ width: 'auto', fontSize: '12px' }}
                value={filterGroupId}
                onChange={(e) => setFilterGroupId(e.target.value)}
              >
                <option value="">Tất cả bảng đấu</option>
                {groups.map((g) => (
                  <option key={g.id} value={g.id}>
                    {g.ten}
                  </option>
                ))}
              </Input>
            )}

            {/* Venue filter */}
            <Input
              type="select"
              bsSize="sm"
              className="rounded-pill"
              style={{ width: 'auto', fontSize: '12px' }}
              value={filterVenueId}
              onChange={(e) => setFilterVenueId(e.target.value)}
            >
              <option value="">Tất cả sân đấu</option>
              {venues.map((v) => (
                <option key={v.id} value={v.id}>
                  {v.ten}
                </option>
              ))}
            </Input>

            {/* Status filter */}
            <Input
              type="select"
              bsSize="sm"
              className="rounded-pill"
              style={{ width: 'auto', fontSize: '12px' }}
              value={filterStatus}
              onChange={(e) => setFilterStatus(e.target.value)}
            >
              <option value="">Tất cả trạng thái</option>
              <option value="ChuaDau">Chưa đấu</option>
              <option value="DangDienRa">Đang diễn ra</option>
              <option value="DaKetThuc">Đã kết thúc</option>
              <option value="Hoan">Hoãn</option>
            </Input>

            {(filterDate || filterRoundId || filterGroupId || filterVenueId || filterStatus || searchKeyword) && (
              <Button
                color="link"
                size="sm"
                className="text-danger p-0 ms-auto text-decoration-none small"
                onClick={() => {
                  setFilterDate('');
                  setFilterRoundId('');
                  setFilterGroupId('');
                  setFilterVenueId('');
                  setFilterStatus('');
                  setSearchKeyword('');
                }}
              >
                Xóa bộ lọc
              </Button>
            )}
          </div>
        </CardBody>
      </Card>

      {/* 4. Tab Navigation for Different Views */}
      <div className="d-flex align-items-center justify-content-between border-bottom pb-2">
        <Nav pills className="custom-pills gap-2">
          <NavItem>
            <NavLink
              className={`rounded-pill px-3 py-1.5 fw-semibold d-flex align-items-center gap-2 cursor-pointer ${
                activeTab === 'list' ? 'active bg-primary text-white' : 'text-secondary bg-white border'
              }`}
              onClick={() => setActiveTab('list')}
              style={{ fontSize: '13px' }}
            >
              <Calendar size={15} />
              <span>Danh Sách Trận Đấu ({filteredMatches.length})</span>
            </NavLink>
          </NavItem>

          <NavItem>
            <NavLink
              className={`rounded-pill px-3 py-1.5 fw-semibold d-flex align-items-center gap-2 cursor-pointer ${
                activeTab === 'court' ? 'active bg-primary text-white' : 'text-secondary bg-white border'
              }`}
              onClick={() => setActiveTab('court')}
              style={{ fontSize: '13px' }}
            >
              <MapPin size={15} />
              <span>Ma Trận Sân Đấu (Court View)</span>
            </NavLink>
          </NavItem>

          <NavItem>
            <NavLink
              className={`rounded-pill px-3 py-1.5 fw-semibold d-flex align-items-center gap-2 cursor-pointer ${
                activeTab === 'referee' ? 'active bg-primary text-white' : 'text-secondary bg-white border'
              }`}
              onClick={() => setActiveTab('referee')}
              style={{ fontSize: '13px' }}
            >
              <Shield size={15} />
              <span>Phân Công Trọng Tài</span>
            </NavLink>
          </NavItem>

          {hasGroupStage && (
            <NavItem>
              <NavLink
                className={`rounded-pill px-3 py-1.5 fw-semibold d-flex align-items-center gap-2 cursor-pointer ${
                  activeTab === 'groups' ? 'active bg-primary text-white' : 'text-secondary bg-white border'
                }`}
                onClick={() => setActiveTab('groups')}
                style={{ fontSize: '13px' }}
              >
                <Layers size={15} />
                <span>Bảng Đấu & BXH ({groups.length})</span>
              </NavLink>
            </NavItem>
          )}

          <NavItem>
            <NavLink
              className={`rounded-pill px-3 py-1.5 fw-semibold d-flex align-items-center gap-2 cursor-pointer ${
                activeTab === 'teams' ? 'active bg-primary text-white' : 'text-secondary bg-white border'
              }`}
              onClick={() => setActiveTab('teams')}
              style={{ fontSize: '13px' }}
            >
              <Users size={15} />
              <span>Đội Đăng Ký & VĐV ({teamScope === 'tournament' ? tournamentTeams.length : registeredTeams.length})</span>
            </NavLink>
          </NavItem>
        </Nav>
      </div>

      {/* 5. Tab Content */}
      {loading ? (
        <div className="text-center py-5">
          <Spinner color="primary" />
          <p className="text-muted mt-2 small">Đang tải lịch thi đấu...</p>
        </div>
      ) : (
        <>
          {/* TAB 1: LIST VIEW */}
          {activeTab === 'list' && (
            <Card className="border-0 shadow-sm rounded-4 overflow-hidden">
              {/* Quick Registered Teams Preview strip */}
              <div className="bg-light bg-opacity-75 border-bottom p-3">
                <div className="d-flex flex-column flex-md-row align-items-md-center justify-content-between gap-2 mb-2">
                  <div className="d-flex align-items-center gap-2">
                    <Users size={16} className="text-primary" />
                    <span className="fw-bold text-dark small">
                      Các Đội Đăng Ký Trong Nội Dung Này ({registeredTeams.length} đội)
                    </span>
                    {searchKeyword && (
                      <Badge color="info" pill className="cursor-pointer" onClick={() => setSearchKeyword('')}>
                        Đang lọc: &quot;{searchKeyword}&quot; ✕
                      </Badge>
                    )}
                  </div>

                  <div className="d-flex align-items-center gap-2">
                    <Button
                      color="link"
                      size="sm"
                      className="p-0 text-primary text-decoration-none fw-semibold small d-flex align-items-center gap-1"
                      onClick={() => {
                        setTeamScope('current_event');
                        setActiveTab('teams');
                      }}
                    >
                      <span>Xem danh sách chi tiết & VĐV</span>
                      <ChevronRight size={14} />
                    </Button>
                  </div>
                </div>

                {registeredTeams.length === 0 ? (
                  <div className="text-muted small fst-italic py-1">
                    Chưa có đội nào đăng ký vào nội dung này.
                  </div>
                ) : (
                  <div className="d-flex flex-wrap gap-2 pt-1">
                    {registeredTeams.map((team) => {
                      const teamName = team.tenDoi || team.tenDangKy || `Đội #${team.id}`;
                      const matchCount = matches.filter((m) => m.doi1DangKyId === team.id || m.doi2DangKyId === team.id).length;
                      const isFiltered = searchKeyword && teamName.toLowerCase().includes(searchKeyword.toLowerCase());
                      const athleteNamesStr = team.vanDongVienNames && team.vanDongVienNames.length > 0
                        ? team.vanDongVienNames.join(', ')
                        : 'Chưa có VĐV';

                      return (
                        <div
                          key={team.id}
                          onClick={() => {
                            if (searchKeyword === teamName) {
                              setSearchKeyword('');
                            } else {
                              setSearchKeyword(teamName);
                            }
                          }}
                          className={`d-inline-flex align-items-center gap-2 px-2.5 py-1 rounded-pill cursor-pointer border transition-all ${
                            isFiltered
                              ? 'bg-primary text-white border-primary shadow-sm'
                              : 'bg-white text-dark hover-shadow'
                          }`}
                          style={{ fontSize: '12px' }}
                          title={`VĐV: ${athleteNamesStr} | Đơn vị: ${team.tenDonVi || '--'} | Nhấn để lọc trận`}
                        >
                          <span className="fw-semibold">{teamName}</span>
                          {team.tenDonVi && (
                            <span className={isFiltered ? 'text-white-50' : 'text-muted'} style={{ fontSize: '11px' }}>
                              ({team.tenDonVi})
                            </span>
                          )}
                          <Badge
                            color={isFiltered ? 'light' : matchCount > 0 ? 'success' : 'warning'}
                            pill
                            className={`px-1.5 py-0.5 ${isFiltered ? 'text-primary' : ''}`}
                            style={{ fontSize: '10px' }}
                          >
                            {matchCount > 0 ? `${matchCount} trận` : 'Chưa đấu'}
                          </Badge>
                        </div>
                      );
                    })}
                  </div>
                )}
              </div>

              <div className="table-responsive">
                <Table hover className="align-middle mb-0" style={{ fontSize: '13px' }}>
                  <thead className="table-light">
                    <tr>
                      <th className="py-3 px-3 text-center" style={{ width: '70px' }}>STT</th>
                      <th className="py-3 px-3" style={{ minWidth: '160px' }}>Trận Đấu & Vòng</th>
                      <th className="py-3 px-3" style={{ minWidth: '280px' }}>Cặp Đấu (Đội 1 vs Đội 2)</th>
                      <th className="py-3 px-3" style={{ minWidth: '170px' }}>Thời Gian</th>
                      <th className="py-3 px-3" style={{ minWidth: '150px' }}>Sân Đấu</th>
                      <th className="py-3 px-3" style={{ minWidth: '180px' }}>Trọng Tài Điều Hành</th>
                      <th className="py-3 px-3 text-center" style={{ width: '120px' }}>Trạng Thái</th>
                      <th className="py-3 px-3 text-center" style={{ width: '100px' }}>Thao Tác</th>
                    </tr>
                  </thead>
                  <tbody>
                    {filteredMatches.length === 0 ? (
                      <tr>
                        <td colSpan={8} className="text-center py-5 text-muted">
                          <div className="d-flex flex-column align-items-center justify-content-center">
                            <Calendar size={44} className="text-secondary opacity-25 mb-2" />
                            <p className="mb-1 fw-semibold text-dark">Chưa có trận đấu nào được xếp</p>
                            <small className="text-secondary mb-3">
                              Hãy nhấn nút <strong>"Xếp Lịch Tự Động"</strong> để hệ thống tự động sinh lịch theo sân và trọng tài, hoặc thêm từng trận thủ công.
                            </small>
                            <Button
                              color="primary"
                              size="sm"
                              className="rounded-pill px-3 py-1.5 fw-semibold"
                              onClick={() => setAutoScheduleModalOpen(true)}
                            >
                              <Zap size={14} className="me-1" />
                              Bắt Đầu Xếp Lịch Tự Động
                            </Button>
                          </div>
                        </td>
                      </tr>
                    ) : (
                      filteredMatches.map((m, idx) => {
                        const dateFormatted = m.thoiGianBatDau
                          ? new Date(m.thoiGianBatDau).toLocaleDateString('vi-VN', {
                              day: '2-digit',
                              month: '2-digit',
                              year: 'numeric',
                            })
                          : 'Chưa xếp';
                        const timeFormatted = m.thoiGianBatDau && m.thoiGianKetThuc
                          ? `${new Date(m.thoiGianBatDau).toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' })} - ${new Date(m.thoiGianKetThuc).toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' })}`
                          : '--:--';

                        return (
                          <tr key={m.id}>
                            <td className="py-3 px-3 text-center fw-bold text-muted">
                              #{m.soTran || idx + 1}
                            </td>

                            <td className="py-3 px-3">
                              <div className="fw-bold text-dark">{m.tenTran || `Trận ${m.soTran}`}</div>
                              <div className="d-flex align-items-center gap-1.5 mt-0.5">
                                <Badge color="light" className="text-secondary border fw-medium px-2 py-0.5" style={{ fontSize: '11px' }}>
                                  {m.tenVongDau || 'Vòng đấu'}
                                </Badge>
                                {m.tenBangDau && (
                                  <Badge color="warning" pill className="px-2 py-0.5 text-dark fw-bold" style={{ fontSize: '11px' }}>
                                    {m.tenBangDau}
                                  </Badge>
                                )}
                              </div>
                            </td>

                            {/* Teams display */}
                            <td className="py-3 px-3">
                              <div className="p-2 rounded-3 bg-light border d-flex flex-column gap-1">
                                <div className="d-flex align-items-center justify-content-between">
                                  <div className="d-flex align-items-center gap-2">
                                    <span className="badge rounded-circle bg-primary text-white p-1" style={{ width: '18px', height: '18px', fontSize: '10px' }}>1</span>
                                    <span className={`fw-semibold ${!m.doi1DangKyId ? 'text-primary fst-italic' : 'text-dark'}`}>
                                      {m.tenDoi1 || 'Chờ xác định'}
                                    </span>
                                    {!m.doi1DangKyId && m.tenDoi1 && (
                                      <Badge color="info" pill className="px-1.5 py-0.5 fw-normal" style={{ fontSize: '9px' }}>Nhánh đấu</Badge>
                                    )}
                                  </div>
                                  {m.donViDoi1 && <small className="text-muted" style={{ fontSize: '11px' }}>({m.donViDoi1})</small>}
                                </div>

                                <div className="text-center my-0.5">
                                  <span className="badge bg-secondary-subtle text-secondary px-2 py-0.5 rounded-pill fw-bold" style={{ fontSize: '10px' }}>VS</span>
                                </div>

                                <div className="d-flex align-items-center justify-content-between">
                                  <div className="d-flex align-items-center gap-2">
                                    <span className="badge rounded-circle bg-danger text-white p-1" style={{ width: '18px', height: '18px', fontSize: '10px' }}>2</span>
                                    <span className={`fw-semibold ${!m.doi2DangKyId ? 'text-danger fst-italic' : 'text-dark'}`}>
                                      {m.tenDoi2 || 'Chờ xác định'}
                                    </span>
                                    {!m.doi2DangKyId && m.tenDoi2 && (
                                      <Badge color="info" pill className="px-1.5 py-0.5 fw-normal" style={{ fontSize: '9px' }}>Nhánh đấu</Badge>
                                    )}
                                  </div>
                                  {m.donViDoi2 && <small className="text-muted" style={{ fontSize: '11px' }}>({m.donViDoi2})</small>}
                                </div>
                              </div>
                            </td>

                            {/* Match Time */}
                            <td className="py-3 px-3">
                              <div className="fw-semibold text-dark d-flex align-items-center gap-1.5">
                                <Calendar size={13} className="text-primary" />
                                {dateFormatted}
                              </div>
                              <div className="small text-muted d-flex align-items-center gap-1.5 mt-0.5">
                                <Clock size={12} />
                                {timeFormatted}
                              </div>
                            </td>

                            {/* Venue */}
                            <td className="py-3 px-3">
                              {m.tenSanDau ? (
                                <div>
                                  <div className="fw-semibold text-dark d-flex align-items-center gap-1">
                                    <MapPin size={13} className="text-danger" />
                                    {m.tenSanDau}
                                  </div>
                                  {m.tenCumSan && (
                                    <small className="text-muted d-block" style={{ fontSize: '11px' }}>
                                      {m.tenCumSan}
                                    </small>
                                  )}
                                </div>
                              ) : (
                                <Badge color="secondary" pill className="px-2 py-1">Chưa xếp sân</Badge>
                              )}
                            </td>

                            {/* Referees */}
                            <td className="py-3 px-3">
                              {m.danhSachTrongTai && m.danhSachTrongTai.length > 0 ? (
                                <div className="d-flex flex-column gap-1">
                                  {m.danhSachTrongTai.map((r) => (
                                    <div key={r.id} className="d-flex align-items-center gap-1.5">
                                      <Shield size={12} className={r.vaiTro === 'TrongTaiChinh' ? 'text-warning' : 'text-secondary'} />
                                      <span className="fw-medium text-dark" style={{ fontSize: '12px' }}>{r.tenTrongTai}</span>
                                      <span className="text-muted small" style={{ fontSize: '10px' }}>
                                        ({r.vaiTro === 'TrongTaiChinh' ? 'Chính' : r.vaiTro === 'TrongTaiPhu' ? 'Phụ' : 'Bàn'})
                                      </span>
                                    </div>
                                  ))}
                                </div>
                              ) : (
                                <Badge color="warning-subtle" className="text-warning border border-warning px-2 py-0.5" style={{ fontSize: '11px' }}>
                                  Chưa phân công
                                </Badge>
                              )}
                            </td>

                            {/* Status */}
                            <td className="py-3 px-3 text-center">
                              <Badge
                                color={
                                  m.trangThai === 'DangDienRa'
                                    ? 'success'
                                    : m.trangThai === 'DaKetThuc'
                                    ? 'secondary'
                                    : m.trangThai === 'Hoan'
                                    ? 'danger'
                                    : 'primary'
                                }
                                pill
                                className="px-2.5 py-1 fw-semibold"
                                style={{ fontSize: '11px' }}
                              >
                                {m.trangThai === 'ChuaDau'
                                  ? 'Chưa đấu'
                                  : m.trangThai === 'DangDienRa'
                                  ? 'Đang đá'
                                  : m.trangThai === 'DaKetThuc'
                                  ? 'Kết thúc'
                                  : m.trangThai === 'Hoan'
                                  ? 'Hoãn'
                                  : m.trangThai}
                              </Badge>
                            </td>

                            {/* Actions */}
                            <td className="py-3 px-3 text-center">
                              <div className="d-flex align-items-center justify-content-center gap-1">
                                <Button
                                  color="light"
                                  size="sm"
                                  className="p-1 text-primary rounded-2 border"
                                  title="Chỉnh sửa trận"
                                  onClick={() => handleOpenEditMatch(m)}
                                >
                                  <Edit2 size={14} />
                                </Button>
                                <Button
                                  color="light"
                                  size="sm"
                                  className="p-1 text-danger rounded-2 border"
                                  title="Xóa trận"
                                  onClick={() => handleDeleteMatch(m.id)}
                                >
                                  <Trash2 size={14} />
                                </Button>
                              </div>
                            </td>
                          </tr>
                        );
                      })
                    )}
                  </tbody>
                </Table>
              </div>
            </Card>
          )}

          {/* TAB 2: COURT MATRIX VIEW (Ma trận theo sân) */}
          {activeTab === 'court' && (
            <Card className="border-0 shadow-sm rounded-4">
              <CardBody className="p-4">
                <div className="d-flex align-items-center justify-content-between mb-4">
                  <div>
                    <h6 className="fw-bold mb-1">Ma Trận Phân Bổ Sân Đấu Theo Khung Giờ</h6>
                    <small className="text-muted">Trực quan hóa từng sân đấu theo thời gian, chống trùng lịch và nhận diện sân trống</small>
                  </div>
                  <div className="d-flex align-items-center gap-2">
                    <Label className="small text-muted mb-0 fw-semibold">Chọn ngày:</Label>
                    <Input
                      type="select"
                      bsSize="sm"
                      className="rounded-pill"
                      style={{ width: '160px' }}
                      value={filterDate || (uniqueDates[0] || '')}
                      onChange={(e) => setFilterDate(e.target.value)}
                    >
                      {uniqueDates.map((d) => (
                        <option key={d} value={d}>
                          Ngày {d}
                        </option>
                      ))}
                    </Input>
                  </div>
                </div>

                <div className="table-responsive">
                  <Table bordered className="align-middle text-center mb-0" style={{ minWidth: '800px' }}>
                    <thead className="table-light">
                      <tr>
                        <th style={{ width: '130px' }} className="py-3">Khung Giờ</th>
                        {venues.map((v) => (
                          <th key={v.id} className="py-3">
                            <div className="fw-bold text-dark">{v.ten}</div>
                            <small className="text-muted fw-normal" style={{ fontSize: '11px' }}>{v.loaiSan || 'Sân tiêu chuẩn'}</small>
                          </th>
                        ))}
                      </tr>
                    </thead>
                    <tbody>
                      {/* Unique start times for the chosen date */}
                      {(() => {
                        const targetDate = filterDate || uniqueDates[0];
                        const dateMatches = matches.filter((m) => {
                          if (!m.thoiGianBatDau) return false;
                          return new Date(m.thoiGianBatDau).toISOString().split('T')[0] === targetDate;
                        });

                        const times = Array.from(
                          new Set(
                            dateMatches.map((m) =>
                              new Date(m.thoiGianBatDau!).toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' })
                            )
                          )
                        ).sort();

                        if (times.length === 0) {
                          return (
                            <tr>
                              <td colSpan={venues.length + 1} className="py-5 text-muted">
                                Không có trận đấu nào trong ngày được chọn.
                              </td>
                            </tr>
                          );
                        }

                        return times.map((tStr) => (
                          <tr key={tStr}>
                            <td className="fw-bold text-primary bg-light">
                              <Clock size={12} className="me-1" />
                              {tStr}
                            </td>
                            {venues.map((v) => {
                              const matchAtCourt = dateMatches.find((m) => {
                                if (m.sanDauId !== v.id) return false;
                                const mTime = new Date(m.thoiGianBatDau!).toLocaleTimeString('vi-VN', {
                                  hour: '2-digit',
                                  minute: '2-digit',
                                });
                                return mTime === tStr;
                              });

                              return (
                                <td key={v.id} className="p-2" style={{ verticalAlign: 'top', height: '90px' }}>
                                  {matchAtCourt ? (
                                    <div
                                      className="p-2 rounded-3 text-start shadow-sm position-relative cursor-pointer"
                                      style={{
                                        backgroundColor: '#eff6ff',
                                        border: '1px solid #bfdbfe',
                                      }}
                                      onClick={() => handleOpenEditMatch(matchAtCourt)}
                                    >
                                      <div className="d-flex align-items-center justify-content-between mb-1">
                                        <Badge color="primary" pill style={{ fontSize: '9px' }}>
                                          #{matchAtCourt.soTran} {matchAtCourt.tenVongDau}
                                        </Badge>
                                        {matchAtCourt.tenBangDau && (
                                          <Badge color="warning" pill style={{ fontSize: '9px' }}>
                                            {matchAtCourt.tenBangDau}
                                          </Badge>
                                        )}
                                      </div>
                                      <div className="fw-bold text-dark text-truncate" style={{ fontSize: '12px' }}>
                                        {matchAtCourt.tenDoi1}
                                      </div>
                                      <div className="text-muted fw-bold text-center" style={{ fontSize: '9px' }}>VS</div>
                                      <div className="fw-bold text-dark text-truncate" style={{ fontSize: '12px' }}>
                                        {matchAtCourt.tenDoi2}
                                      </div>

                                      {matchAtCourt.danhSachTrongTai && matchAtCourt.danhSachTrongTai.length > 0 && (
                                        <div className="mt-1 pt-1 border-top d-flex align-items-center gap-1 text-muted" style={{ fontSize: '10px' }}>
                                          <Shield size={10} className="text-warning" />
                                          <span className="text-truncate">{matchAtCourt.danhSachTrongTai[0]?.tenTrongTai}</span>
                                        </div>
                                      )}
                                    </div>
                                  ) : (
                                    <div className="h-100 d-flex align-items-center justify-content-center text-muted opacity-25 small">
                                      Trống
                                    </div>
                                  )}
                                </td>
                              );
                            })}
                          </tr>
                        ));
                      })()}
                    </tbody>
                  </Table>
                </div>
              </CardBody>
            </Card>
          )}

          {/* TAB 3: REFEREE ASSIGNMENT VIEW */}
          {activeTab === 'referee' && (
            <Card className="border-0 shadow-sm rounded-4">
              <CardBody className="p-4">
                <div className="d-flex align-items-center justify-content-between mb-4">
                  <div>
                    <h6 className="fw-bold mb-1">Bảng Phân Công & Giám Sát Trọng Tài</h6>
                    <small className="text-muted">Tổng hợp danh sách phân công, số lượng trận điều hành và đảm bảo công bằng khối lượng ca trực</small>
                  </div>
                </div>

                <Row className="g-3">
                  {referees.map((ref) => {
                    // Matches assigned to this referee
                    const refMatches = matches.filter((m) =>
                      m.danhSachTrongTai?.some((r) => r.trongTaiId === ref.id)
                    );

                    return (
                      <Col key={ref.id} xs={12} md={6} lg={4}>
                        <div className="p-3.5 rounded-4 border bg-white shadow-sm h-100 d-flex flex-column">
                          <div className="d-flex align-items-center gap-3 mb-3">
                            <div
                              className="rounded-circle bg-warning bg-opacity-25 text-dark d-flex align-items-center justify-content-center fw-bold"
                              style={{ width: '46px', height: '46px', fontSize: '16px' }}
                            >
                              {ref.hoTen.slice(0, 2).toUpperCase()}
                            </div>
                            <div className="lh-sm">
                              <h6 className="fw-bold text-dark mb-0.5">{ref.hoTen}</h6>
                              <div className="d-flex align-items-center gap-2 text-muted small" style={{ fontSize: '11px' }}>
                                <span>{ref.capBac || 'Trọng tài'}</span>
                                <span>•</span>
                                <span>{ref.soDienThoai || 'N/A'}</span>
                              </div>
                            </div>
                            <Badge color="primary" pill className="ms-auto px-2.5 py-1 fw-bold">
                              {refMatches.length} trận
                            </Badge>
                          </div>

                          <div className="border-top pt-2 mt-auto">
                            <span className="small text-secondary fw-semibold d-block mb-2" style={{ fontSize: '11px' }}>
                              Lịch các trận được phân công:
                            </span>
                            {refMatches.length === 0 ? (
                              <p className="text-muted small mb-0 fst-italic" style={{ fontSize: '11px' }}>
                                Chưa có trận đấu nào được phân công.
                              </p>
                            ) : (
                              <div className="d-flex flex-column gap-1.5" style={{ maxHeight: '180px', overflowY: 'auto' }}>
                                {refMatches.map((rm) => (
                                  <div
                                    key={rm.id}
                                    className="p-2 rounded-2 bg-light border d-flex align-items-center justify-content-between"
                                    style={{ fontSize: '11px' }}
                                  >
                                    <div>
                                      <span className="fw-bold text-dark">Trận #{rm.soTran}</span>: {rm.tenDoi1} vs {rm.tenDoi2}
                                      <div className="text-muted" style={{ fontSize: '10px' }}>
                                        {rm.tenSanDau} • {rm.thoiGianBatDau ? new Date(rm.thoiGianBatDau).toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' }) : ''}
                                      </div>
                                    </div>
                                    <Badge color="info" pill style={{ fontSize: '9px' }}>
                                      {rm.danhSachTrongTai?.find((x) => x.trongTaiId === ref.id)?.vaiTro === 'TrongTaiChinh' ? 'Chính' : 'Phụ'}
                                    </Badge>
                                  </div>
                                ))}
                              </div>
                            )}
                          </div>
                        </div>
                      </Col>
                    );
                  })}
                </Row>
              </CardBody>
            </Card>
          )}

          {/* TAB 4: GROUPS & STANDINGS (Bảng Đấu) */}
          {activeTab === 'groups' && (
            <div className="d-flex flex-column gap-4">
              <div className="d-flex align-items-center justify-content-between">
                <div>
                  <h6 className="fw-bold mb-1">Danh Sách Bảng Đấu & Các Đội Tham Gia</h6>
                  <small className="text-muted">Bốc thăm chia bảng các đội đăng ký thi đấu của nội dung</small>
                </div>
                <Button
                  color="primary"
                  size="sm"
                  className="rounded-pill px-3 py-1.5 fw-semibold d-flex align-items-center gap-1.5"
                  onClick={() => setGroupModalOpen(true)}
                >
                  <Shuffle size={14} />
                  <span>Bốc Thăm Lại Bảng Đấu</span>
                </Button>
              </div>

              <Row className="g-3">
                {groups.length === 0 ? (
                  <Col xs={12}>
                    <div className="bg-white rounded-4 p-5 text-center border">
                      <Layers size={44} className="text-muted opacity-25 mb-2" />
                      <p className="fw-semibold text-dark mb-1">Chưa có bảng đấu nào được tạo</p>
                      <small className="text-muted mb-3 d-block">
                        Nhấn nút bên dưới để tự động bốc thăm chia các đội đã duyệt vào các bảng đấu.
                      </small>
                      <Button color="primary" size="sm" className="rounded-pill px-3 py-1.5" onClick={() => setGroupModalOpen(true)}>
                        <Shuffle size={14} className="me-1" />
                        Chia Bảng Đấu Ngay
                      </Button>
                    </div>
                  </Col>
                ) : (
                  groups.map((g) => (
                    <Col key={g.id} xs={12} md={6}>
                      <Card className="border-0 shadow-sm rounded-4 h-100">
                        <CardBody className="p-3.5">
                          <div className="d-flex align-items-center justify-content-between mb-3 pb-2 border-bottom">
                            <div className="d-flex align-items-center gap-2">
                              <Badge color="warning" pill className="px-2.5 py-1 text-dark fw-bold">
                                {g.ten}
                              </Badge>
                              <span className="text-muted small">({g.thanhViens?.length || 0} đội)</span>
                            </div>
                          </div>

                          <div className="table-responsive">
                            <Table size="sm" className="align-middle mb-0" style={{ fontSize: '12px' }}>
                              <thead className="table-light">
                                <tr>
                                  <th style={{ width: '40px' }} className="text-center">#</th>
                                  <th>Tên Đội / VĐV</th>
                                  <th>Đơn Vị</th>
                                  <th className="text-center" title="Số trận đã đấu" style={{ width: '50px' }}>ST</th>
                                  <th className="text-center text-success" title="Số trận thắng" style={{ width: '45px' }}>T</th>
                                  <th className="text-center text-secondary" title="Số trận hòa" style={{ width: '45px' }}>H</th>
                                  <th className="text-center text-danger" title="Số trận bại" style={{ width: '45px' }}>B</th>
                                  <th className="text-center text-muted" title="Hiệu số điểm/bàn thắng" style={{ width: '55px' }}>+/-</th>
                                  <th className="text-center" title="Tổng điểm" style={{ width: '60px' }}>Điểm</th>
                                </tr>
                              </thead>
                              <tbody>
                                {g.thanhViens && g.thanhViens.length > 0 ? (
                                  g.thanhViens.map((tv, idx) => {
                                    const hieuSo = (tv.diemGhiDuoc ?? 0) - (tv.diemBiGhi ?? 0);
                                    return (
                                      <tr key={tv.id} className={idx === 0 ? 'table-warning bg-opacity-25' : ''}>
                                        <td className="text-center fw-bold">
                                          {idx === 0 ? '🥇' : idx === 1 ? '🥈' : idx === 2 ? '🥉' : idx + 1}
                                        </td>
                                        <td className="fw-semibold text-dark">
                                          {tv.tenDoi || tv.tenDangKy}
                                          {idx === 0 && (
                                            <Badge color="warning" pill className="ms-1 px-1.5 py-0.5 text-dark" style={{ fontSize: '9px' }}>
                                              Dẫn đầu
                                            </Badge>
                                          )}
                                        </td>
                                        <td className="text-muted small">{tv.tenDonVi || '--'}</td>
                                        <td className="text-center font-monospace">{tv.soTran}</td>
                                        <td className="text-center font-monospace fw-semibold text-success">{tv.soThang ?? 0}</td>
                                        <td className="text-center font-monospace text-secondary">{tv.soHoa ?? 0}</td>
                                        <td className="text-center font-monospace text-danger">{tv.soThua ?? 0}</td>
                                        <td className={`text-center font-monospace small ${hieuSo > 0 ? 'text-success' : hieuSo < 0 ? 'text-danger' : 'text-muted'}`}>
                                          {hieuSo > 0 ? `+${hieuSo}` : hieuSo}
                                        </td>
                                        <td className="text-center font-monospace">
                                          <Badge color="primary" pill className="px-2 py-1 fw-bold fs-7">
                                            {tv.diem}
                                          </Badge>
                                        </td>
                                      </tr>
                                    );
                                  })
                                ) : (
                                  <tr>
                                    <td colSpan={9} className="text-center py-3 text-muted">
                                      Bảng đấu chưa có đội
                                    </td>
                                  </tr>
                                )}
                              </tbody>
                            </Table>
                          </div>
                        </CardBody>
                      </Card>
                    </Col>
                  ))
                )}
              </Row>
            </div>
          )}

          {/* TAB 5: REGISTERED TEAMS & ATHLETES (Đội Đăng Ký & VĐV) */}
          {activeTab === 'teams' && (
            <div className="d-flex flex-column gap-3">
              {/* Scope Switcher & Filter Toolbar */}
              <Card className="border-0 shadow-sm rounded-4">
                <CardBody className="p-3">
                  <div className="d-flex flex-column flex-lg-row align-items-lg-center justify-content-between gap-3">
                    {/* Scope buttons */}
                    <div className="d-flex align-items-center gap-2">
                      <Button
                        color={teamScope === 'current_event' ? 'primary' : 'light'}
                        size="sm"
                        className={`rounded-pill px-3 py-1.5 fw-semibold d-flex align-items-center gap-2 ${
                          teamScope === 'current_event' ? 'text-white' : 'text-secondary border'
                        }`}
                        onClick={() => {
                          setTeamScope('current_event');
                          setTeamFilterEventId('');
                        }}
                      >
                        <Users size={14} />
                        <span>Nội dung đang chọn: {currentEvent?.ten || 'Chưa chọn'} ({registeredTeams.length})</span>
                      </Button>

                      <Button
                        color={teamScope === 'tournament' ? 'primary' : 'light'}
                        size="sm"
                        className={`rounded-pill px-3 py-1.5 fw-semibold d-flex align-items-center gap-2 ${
                          teamScope === 'tournament' ? 'text-white' : 'text-secondary border'
                        }`}
                        onClick={() => setTeamScope('tournament')}
                      >
                        <Trophy size={14} />
                        <span>Toàn bộ giải đấu ({tournamentTeams.length})</span>
                      </Button>
                    </div>

                    {/* Search bar */}
                    <div className="position-relative" style={{ minWidth: '280px' }}>
                      <Search size={15} className="position-absolute text-muted" style={{ left: '12px', top: '50%', transform: 'translateY(-50%)' }} />
                      <Input
                        type="text"
                        placeholder="Tìm theo đội, VĐV, đơn vị, mã..."
                        className="form-control form-control-sm rounded-pill ps-5"
                        style={{ fontSize: '13px' }}
                        value={teamSearch}
                        onChange={(e) => setTeamSearch(e.target.value)}
                      />
                    </div>
                  </div>

                  {/* Filter row */}
                  <div className="d-flex flex-wrap align-items-center gap-2 mt-3 pt-3 border-top">
                    <span className="small text-muted fw-semibold d-flex align-items-center gap-1 me-1">
                      <Filter size={13} /> Lọc đội:
                    </span>

                    {/* Event filter (active when in tournament view) */}
                    {teamScope === 'tournament' && (
                      <Input
                        type="select"
                        bsSize="sm"
                        className="rounded-pill"
                        style={{ width: 'auto', fontSize: '12px' }}
                        value={teamFilterEventId}
                        onChange={(e) => setTeamFilterEventId(e.target.value)}
                      >
                        <option value="">Tất cả nội dung ({events.length})</option>
                        {events.map((ev) => (
                          <option key={ev.id} value={ev.id}>
                            {ev.ten} ({ev.tenMonTheThao || 'Môn'})
                          </option>
                        ))}
                      </Input>
                    )}

                    {/* Unit filter */}
                    {distinctUnits.length > 0 && (
                      <Input
                        type="select"
                        bsSize="sm"
                        className="rounded-pill"
                        style={{ width: 'auto', fontSize: '12px' }}
                        value={teamFilterDonVi}
                        onChange={(e) => setTeamFilterDonVi(e.target.value)}
                      >
                        <option value="">Tất cả đơn vị ({distinctUnits.length})</option>
                        {distinctUnits.map((u) => (
                          <option key={u} value={u}>
                            {u}
                          </option>
                        ))}
                      </Input>
                    )}

                    {/* Status filter */}
                    <Input
                      type="select"
                      bsSize="sm"
                      className="rounded-pill"
                      style={{ width: 'auto', fontSize: '12px' }}
                      value={teamFilterStatus}
                      onChange={(e) => setTeamFilterStatus(e.target.value)}
                    >
                      <option value="">Tất cả trạng thái duyệt</option>
                      <option value="DaDuyet">Đã duyệt</option>
                      <option value="ChoDuyet">Chờ duyệt</option>
                      <option value="TuChoi">Từ chối</option>
                    </Input>

                    {/* Scheduling filter */}
                    <Input
                      type="select"
                      bsSize="sm"
                      className="rounded-pill"
                      style={{ width: 'auto', fontSize: '12px' }}
                      value={teamFilterScheduled}
                      onChange={(e) => setTeamFilterScheduled(e.target.value)}
                    >
                      <option value="">Tất cả tình trạng xếp lịch</option>
                      <option value="has_matches">Đã có lịch thi đấu</option>
                      <option value="no_matches">Chưa có lịch thi đấu</option>
                    </Input>

                    {(teamSearch || teamFilterEventId || teamFilterDonVi || teamFilterStatus || teamFilterScheduled) && (
                      <Button
                        color="link"
                        size="sm"
                        className="text-danger p-0 ms-auto text-decoration-none small"
                        onClick={() => {
                          setTeamSearch('');
                          setTeamFilterEventId('');
                          setTeamFilterDonVi('');
                          setTeamFilterStatus('');
                          setTeamFilterScheduled('');
                        }}
                      >
                        Xóa bộ lọc
                      </Button>
                    )}
                  </div>
                </CardBody>
              </Card>

              {/* Team Metric Cards */}
              <Row className="g-3">
                <Col xs={6} md={3}>
                  <div className="bg-white rounded-4 p-3 border shadow-sm d-flex align-items-center gap-3">
                    <div className="rounded-3 d-flex align-items-center justify-content-center flex-shrink-0" style={{ width: '42px', height: '42px', backgroundColor: '#eef2ff', color: '#4f46e5' }}>
                      <Users size={20} />
                    </div>
                    <div>
                      <span className="text-secondary small d-block" style={{ fontSize: '11px', fontWeight: 500 }}>
                        Tổng số đội đăng ký
                      </span>
                      <span className="fw-bold fs-5 text-dark">{teamMetrics.totalTeams}</span>
                    </div>
                  </div>
                </Col>

                <Col xs={6} md={3}>
                  <div className="bg-white rounded-4 p-3 border shadow-sm d-flex align-items-center gap-3">
                    <div className="rounded-3 d-flex align-items-center justify-content-center flex-shrink-0" style={{ width: '42px', height: '42px', backgroundColor: '#ecfdf5', color: '#059669' }}>
                      <UserCheck size={20} />
                    </div>
                    <div>
                      <span className="text-secondary small d-block" style={{ fontSize: '11px', fontWeight: 500 }}>
                        Tổng VĐV tham gia
                      </span>
                      <span className="fw-bold fs-5 text-success">{teamMetrics.totalVdv}</span>
                    </div>
                  </div>
                </Col>

                <Col xs={6} md={3}>
                  <div className="bg-white rounded-4 p-3 border shadow-sm d-flex align-items-center gap-3">
                    <div className="rounded-3 d-flex align-items-center justify-content-center flex-shrink-0" style={{ width: '42px', height: '42px', backgroundColor: '#f0fdfa', color: '#0d9488' }}>
                      <CalendarDays size={20} />
                    </div>
                    <div>
                      <span className="text-secondary small d-block" style={{ fontSize: '11px', fontWeight: 500 }}>
                        Đội đã xếp lịch
                      </span>
                      <span className="fw-bold fs-5 text-primary">{teamMetrics.teamsWithMatches}</span>
                    </div>
                  </div>
                </Col>

                <Col xs={6} md={3}>
                  <div className="bg-white rounded-4 p-3 border shadow-sm d-flex align-items-center gap-3">
                    <div className="rounded-3 d-flex align-items-center justify-content-center flex-shrink-0" style={{ width: '42px', height: '42px', backgroundColor: '#fffbeb', color: '#d97706' }}>
                      <AlertTriangle size={20} />
                    </div>
                    <div>
                      <span className="text-secondary small d-block" style={{ fontSize: '11px', fontWeight: 500 }}>
                        Đội chưa có lịch
                      </span>
                      <span className="fw-bold fs-5 text-warning">{teamMetrics.teamsWithoutMatches}</span>
                    </div>
                  </div>
                </Col>
              </Row>

              {/* Data Table */}
              <Card className="border-0 shadow-sm rounded-4 overflow-hidden">
                <div className="table-responsive">
                  <Table hover className="align-middle mb-0" style={{ fontSize: '13px' }}>
                    <thead className="table-light">
                      <tr>
                        <th className="py-3 px-3 text-center" style={{ width: '50px' }}>STT</th>
                        <th className="py-3 px-3" style={{ minWidth: '100px' }}>Số ĐK</th>
                        <th className="py-3 px-3" style={{ minWidth: '220px' }}>Tên Đội / Đăng Ký</th>
                        <th className="py-3 px-3" style={{ minWidth: '160px' }}>Đơn Vị Chủ Quản</th>
                        <th className="py-3 px-3" style={{ minWidth: '180px' }}>Môn & Nội Dung Thi Đấu</th>
                        <th className="py-3 px-3" style={{ minWidth: '240px' }}>Vận Động Viên</th>
                        {teamScope === 'current_event' && hasGroupStage && (
                          <th className="py-3 px-3 text-center" style={{ minWidth: '110px' }}>Bảng Đấu</th>
                        )}
                        <th className="py-3 px-3 text-center" style={{ width: '120px' }}>Hồ Sơ</th>
                        <th className="py-3 px-3 text-center" style={{ minWidth: '130px' }}>Lịch Đấu</th>
                        <th className="py-3 px-3 text-center" style={{ width: '130px' }}>Thao Tác</th>
                      </tr>
                    </thead>
                    <tbody>
                      {displayedTeams.length === 0 ? (
                        <tr>
                          <td colSpan={10} className="text-center py-5 text-muted">
                            <div className="d-flex flex-column align-items-center justify-content-center">
                              <Users size={44} className="text-secondary opacity-25 mb-2" />
                              <p className="mb-1 fw-semibold text-dark">Không tìm thấy đội đăng ký nào</p>
                              <small className="text-secondary">
                                {teamSearch || teamFilterDonVi || teamFilterStatus || teamFilterScheduled
                                  ? 'Thử thay đổi hoặc xóa các bộ lọc bên trên.'
                                  : 'Chưa có hồ sơ đăng ký thi đấu nào trong phạm vi này.'}
                              </small>
                            </div>
                          </td>
                        </tr>
                      ) : (
                        displayedTeams.map((team, idx) => {
                          const teamName = team.tenDoi || team.tenDangKy || `Đội #${team.id}`;
                          const matchCount = getTeamMatchCount(team.id, teamScope);
                          const athletes = team.vanDongVienNames || [];
                          const groupName = teamGroupMap.get(team.id);

                          return (
                            <tr key={team.id}>
                              <td className="text-center text-muted fw-bold">{idx + 1}</td>
                              <td>
                                <Badge color="light" className="text-dark border font-monospace px-2 py-1">
                                  {team.soDangKy || `DK-${team.id}`}
                                </Badge>
                              </td>
                              <td>
                                <div className="d-flex flex-column">
                                  <span className="fw-bold text-dark">{teamName}</span>
                                  {team.ngayDangKy && (
                                    <span className="text-muted" style={{ fontSize: '11px' }}>
                                      ĐK: {new Date(team.ngayDangKy).toLocaleDateString('vi-VN')}
                                    </span>
                                  )}
                                </div>
                              </td>
                              <td>
                                <span className="fw-medium text-dark">{team.tenDonVi || '--'}</span>
                              </td>
                              <td>
                                <div className="d-flex flex-column gap-1">
                                  <span className="fw-semibold text-dark">{team.tenNoiDung || '--'}</span>
                                  {team.tenMonTheThao && (
                                    <Badge color="primary" pill className="align-self-start px-2 py-0.5" style={{ fontSize: '10.5px' }}>
                                      {team.tenMonTheThao}
                                    </Badge>
                                  )}
                                </div>
                              </td>
                              <td>
                                {athletes.length === 0 ? (
                                  <span className="text-muted fst-italic small">Chưa có VĐV</span>
                                ) : (
                                  <div className="d-flex flex-wrap gap-1 align-items-center">
                                    {athletes.slice(0, 3).map((athName, aIdx) => (
                                      <Badge
                                        key={aIdx}
                                        color="light"
                                        className="text-dark border px-2 py-1 d-inline-flex align-items-center gap-1 rounded-pill"
                                        style={{ fontSize: '11.5px', fontWeight: 500 }}
                                      >
                                        <User size={10} className="text-primary" />
                                        <span>{athName}</span>
                                      </Badge>
                                    ))}
                                    {athletes.length > 3 && (
                                      <Badge
                                        color="secondary"
                                        pill
                                        className="px-2 py-0.5 cursor-pointer"
                                        style={{ fontSize: '10.5px' }}
                                        onClick={() => handleOpenTeamDetail(team)}
                                        title="Xem tất cả VĐV"
                                      >
                                        +{athletes.length - 3} VĐV
                                      </Badge>
                                    )}
                                  </div>
                                )}
                              </td>
                              {teamScope === 'current_event' && hasGroupStage && (
                                <td className="text-center">
                                  {groupName ? (
                                    <Badge color="warning" pill className="px-2.5 py-1 text-dark fw-bold">
                                      {groupName}
                                    </Badge>
                                  ) : (
                                    <span className="text-muted small">Chưa chia bảng</span>
                                  )}
                                </td>
                              )}
                              <td className="text-center">
                                <Badge
                                  color={team.trangThai === 'DaDuyet' ? 'success' : team.trangThai === 'ChoDuyet' ? 'warning' : 'secondary'}
                                  pill
                                  className="px-2.5 py-1"
                                  style={{ fontSize: '11px' }}
                                >
                                  {team.trangThai === 'DaDuyet' ? 'Đã duyệt' : team.trangThai === 'ChoDuyet' ? 'Chờ duyệt' : team.trangThai}
                                </Badge>
                              </td>
                              <td className="text-center">
                                {matchCount > 0 ? (
                                  <Button
                                    color="link"
                                    size="sm"
                                    className="p-0 text-success text-decoration-none fw-semibold d-inline-flex align-items-center gap-1"
                                    onClick={() => handleViewTeamMatches(team)}
                                    title="Nhấn để xem các trận đấu của đội này"
                                  >
                                    <Badge color="success" pill className="px-2 py-1">
                                      {matchCount} trận đấu
                                    </Badge>
                                  </Button>
                                ) : (
                                  <Badge color="light" className="text-secondary border px-2 py-1" style={{ fontSize: '11px' }}>
                                    Chưa xếp lịch
                                  </Badge>
                                )}
                              </td>
                              <td className="text-center">
                                <div className="d-flex align-items-center justify-content-center gap-1">
                                  <Button
                                    color="light"
                                    size="sm"
                                    className="p-1 rounded-circle border"
                                    title="Xem chi tiết đội & trận đấu"
                                    onClick={() => handleOpenTeamDetail(team)}
                                  >
                                    <Eye size={14} className="text-primary" />
                                  </Button>
                                  <Button
                                    color="light"
                                    size="sm"
                                    className="p-1 rounded-circle border"
                                    title="Thêm trận đấu cho đội này"
                                    onClick={() => handleQuickScheduleMatchForTeam(team)}
                                  >
                                    <Plus size={14} className="text-success" />
                                  </Button>
                                </div>
                              </td>
                            </tr>
                          );
                        })
                      )}
                    </tbody>
                  </Table>
                </div>
              </Card>
            </div>
          )}
        </>
      )}

      {/* ======================================================== */}
      {/* MODAL 1: AUTO SCHEDULE WIZARD                            */}
      {/* ======================================================== */}
      <Modal isOpen={autoScheduleModalOpen} toggle={() => setAutoScheduleModalOpen(!autoScheduleModalOpen)} size="lg" centered>
        <ModalHeader toggle={() => setAutoScheduleModalOpen(!autoScheduleModalOpen)} className="border-bottom">
          <div className="d-flex align-items-center gap-2">
            <Zap size={18} className="text-warning" />
            <span className="fw-bold fs-6">Trình Xếp Lịch Thi Đấu Tự Động Thông Minh</span>
          </div>
        </ModalHeader>
        <ModalBody className="p-4">
          <Alert
            color={
              isLeaderboard ? 'success' :
              isRoundRobin ? 'success' :
              isKnockout ? 'warning' :
              currentEvent?.hinhThucThiDau === 'KetHopVongBangVaLoaiTrucTiep' ? 'info' : 'primary'
            }
            className="d-flex align-items-center gap-2 mb-3 py-2 px-3 rounded-3"
            style={{ fontSize: '12px' }}
          >
            <Info size={16} className="flex-shrink-0" />
            <div>
              {isLeaderboard ? (
                <>
                  Thể thức <strong>Tính điểm xếp hạng / Tính giờ - Thành tích (Leaderboard)</strong>:{' '}
                  Hệ thống phân VĐV thành các <strong>Lượt thi (Heat)</strong> theo số làn / vị trí khả dụng.
                  Kết quả xếp hạng theo <strong>thành tích tốt nhất</strong> (thời gian, điểm số).
                  Phù hợp: <strong>Bơi lội, Điền kinh, Cử tạ, Bắn súng, Thể dục nghệ thuật...</strong>
                </>
              ) : isRoundRobin ? (
                <>
                  Thể thức <strong>Vòng Tròn Tính Điểm (Round Robin)</strong>:{' '}
                  Tất cả các đội thi đấu vòng tròn tính điểm (xếp hạng: <strong>Điểm số &gt; Đối đầu &gt; Hiệu số (+/-)</strong>).
                  Đội có tổng điểm cao nhất khi kết thúc các vòng sẽ vô địch (<strong>không qua loại trực tiếp</strong>).
                  Hệ thống hỗ trợ <strong>1 lượt (Lượt đi)</strong> hoặc <strong>2 lượt (Lượt đi - Lượt về)</strong> và tối ưu hóa sân bãi, thời gian nghỉ giữa các vòng.
                </>
              ) : isKnockout ? (
                <>
                  Thể thức <strong>{HinhThucThiDauLabels[currentEvent?.hinhThucThiDau || ''] || 'Loại trực tiếp (Knockout)'}</strong>:{' '}
                  Hệ thống <strong>không chia bảng đấu</strong>, mà bắt cặp đối đầu loại trực tiếp:
                  Xếp cặp lấy đội thắng vào vòng trong (<strong>Vòng loại → Bán kết 2 cặp (khi còn 4 đội) → Chung kết (Tranh HCV và Tranh HCĐ)</strong>).
                  Các nhánh sau tự động bảo đảm khoảng nghỉ và phân bổ sân bãi, trọng tài tối ưu.
                </>
              ) : currentEvent?.hinhThucThiDau === 'KetHopVongBangVaLoaiTrucTiep' ? (
                <>
                  Thể thức <strong>Kết hợp vòng bảng & loại trực tiếp</strong>:{' '}
                  Hệ thống sẽ <strong>chia bảng và áp dụng Round-Robin vòng bảng</strong>, đồng thời tự động xếp lịch sẵn toàn bộ các vòng sau (<strong>Tứ kết, Bán kết, Trận tranh hạng 3 - 4 và Chung kết</strong>) theo nhánh đấu chuẩn.
                </>
              ) : (
                <>
                  Thể thức <strong>{HinhThucThiDauLabels[currentEvent?.hinhThucThiDau || ''] || 'Vòng bảng'}</strong>:{' '}
                  Hệ thống sẽ <strong>chia bảng đấu và áp dụng Round-Robin</strong> trong từng bảng,
                  phân bổ tối ưu Sân đấu không bị trùng và xoay vòng Trọng tài điều hành.
                </>
              )}
            </div>
          </Alert>

          {/* ===== LEADERBOARD (TinhDiemXepHang) SPECIFIC CONFIG PANEL ===== */}
          {isLeaderboard && (
            <div
              className="rounded-4 border mb-3 overflow-hidden"
              style={{
                background: 'linear-gradient(135deg, #f0fdf4 0%, #ecfdf5 100%)',
                borderColor: '#86efac',
              }}
            >
              {/* Header */}
              <div
                className="px-3 py-2 d-flex align-items-center justify-content-between"
                style={{ background: 'linear-gradient(90deg, #16a34a 0%, #059669 100%)' }}
              >
                <div className="d-flex align-items-center gap-2 text-white">
                  <Zap size={15} />
                  <span className="fw-bold" style={{ fontSize: '13px' }}>Cấu Hình Lượt Thi (Heat) — Thi Thành Tích</span>
                </div>
                <Badge color="light" className="text-success px-2 py-1 fw-bold" style={{ fontSize: '11px' }}>
                  {registeredTeams.length} VĐV đăng ký
                </Badge>
              </div>

              <div className="p-3">
                <Row className="g-3">
                  {/* Số VĐV / Làn mỗi lượt thi */}
                  <Col xs={12} md={4}>
                    <div className="p-3 rounded-3 bg-white border h-100 d-flex flex-column">
                      <div className="d-flex align-items-center gap-2 mb-2">
                        <div
                          className="rounded-2 d-flex align-items-center justify-content-center flex-shrink-0"
                          style={{ width: '32px', height: '32px', background: '#dcfce7', color: '#16a34a' }}
                        >
                          <Users size={15} />
                        </div>
                        <div>
                          <div className="fw-bold text-dark" style={{ fontSize: '12.5px' }}>Số VĐV / Làn / Lượt thi</div>
                          <div className="text-muted" style={{ fontSize: '11px' }}>Số làn bơi, đường chạy, v.v.</div>
                        </div>
                      </div>
                      <div className="d-flex align-items-center gap-2 mt-auto">
                        <Input
                          type="number"
                          min={1}
                          max={30}
                          className="form-control form-control-sm text-center fw-bold fs-5 border-success"
                          style={{ maxWidth: '80px' }}
                          value={autoScheduleConfig.soVdvMoiLuotThi}
                          onChange={(e) =>
                            setAutoScheduleConfig({ ...autoScheduleConfig, soVdvMoiLuotThi: Math.max(1, Number(e.target.value)) })
                          }
                        />
                        <div className="small text-muted">
                          <div>VĐV / lượt thi</div>
                          {leaderboardPreview && (
                            <div className="text-success fw-semibold">
                              → {leaderboardPreview.heatsPerVong} lượt / vòng
                            </div>
                          )}
                        </div>
                      </div>
                      {/* Quick preset buttons */}
                      <div className="d-flex flex-wrap gap-1 mt-2">
                        {[4, 6, 8, 10].map((n) => (
                          <Badge
                            key={n}
                            color={autoScheduleConfig.soVdvMoiLuotThi === n ? 'success' : 'light'}
                            className={`cursor-pointer px-2 py-1 border ${autoScheduleConfig.soVdvMoiLuotThi === n ? 'text-white' : 'text-secondary'}`}
                            style={{ fontSize: '11px' }}
                            onClick={() => setAutoScheduleConfig({ ...autoScheduleConfig, soVdvMoiLuotThi: n })}
                          >
                            {n} làn
                          </Badge>
                        ))}
                      </div>
                    </div>
                  </Col>

                  {/* Số vòng thi */}
                  <Col xs={12} md={4}>
                    <div className="p-3 rounded-3 bg-white border h-100 d-flex flex-column">
                      <div className="d-flex align-items-center gap-2 mb-2">
                        <div
                          className="rounded-2 d-flex align-items-center justify-content-center flex-shrink-0"
                          style={{ width: '32px', height: '32px', background: '#dbeafe', color: '#2563eb' }}
                        >
                          <Layers size={15} />
                        </div>
                        <div>
                          <div className="fw-bold text-dark" style={{ fontSize: '12.5px' }}>Số Vòng Thi</div>
                          <div className="text-muted" style={{ fontSize: '11px' }}>Sơ loại → Chung kết</div>
                        </div>
                      </div>
                      <div className="d-flex flex-column gap-1.5 mt-auto">
                        {[
                          { val: 1, label: '1 vòng', sub: 'Chung kết trực tiếp (1 lần thi)' },
                          { val: 2, label: '2 vòng', sub: 'Vòng loại → Chung kết' },
                          { val: 3, label: '3 vòng', sub: 'Sơ loại → Bán kết → CK' },
                        ].map((opt) => (
                          <div
                            key={opt.val}
                            className={`p-2 rounded-2 border cursor-pointer d-flex align-items-center gap-2 ${
                              autoScheduleConfig.soVongThi === opt.val
                                ? 'border-primary bg-primary bg-opacity-10'
                                : 'bg-white'
                            }`}
                            onClick={() => setAutoScheduleConfig({ ...autoScheduleConfig, soVongThi: opt.val })}
                          >
                            <input
                              type="radio"
                              readOnly
                              checked={autoScheduleConfig.soVongThi === opt.val}
                              className="form-check-input flex-shrink-0"
                              style={{ marginTop: 0 }}
                            />
                            <div>
                              <div className="fw-semibold" style={{ fontSize: '12px' }}>{opt.label}</div>
                              <div className="text-muted" style={{ fontSize: '10.5px' }}>{opt.sub}</div>
                            </div>
                          </div>
                        ))}
                      </div>
                    </div>
                  </Col>

                  {/* Phương thức phân nhóm */}
                  <Col xs={12} md={4}>
                    <div className="p-3 rounded-3 bg-white border h-100 d-flex flex-column">
                      <div className="d-flex align-items-center gap-2 mb-2">
                        <div
                          className="rounded-2 d-flex align-items-center justify-content-center flex-shrink-0"
                          style={{ width: '32px', height: '32px', background: '#fef3c7', color: '#d97706' }}
                        >
                          <Shuffle size={15} />
                        </div>
                        <div>
                          <div className="fw-bold text-dark" style={{ fontSize: '12.5px' }}>Phương Thức Phân Nhóm</div>
                          <div className="text-muted" style={{ fontSize: '11px' }}>Cách chia VĐV vào lượt thi</div>
                        </div>
                      </div>
                      <div className="d-flex flex-column gap-1.5 mt-auto">
                        {[
                          { val: 'random', label: '🎲 Bốc thăm ngẫu nhiên', sub: 'Chia đều ngẫu nhiên vào từng lượt' },
                          { val: 'registration_order', label: '📋 Theo thứ tự đăng ký', sub: 'Đăng ký trước → lượt thi trước' },
                          { val: 'performance_seed', label: '🏆 Xếp hạt giống thành tích', sub: 'VĐV mạnh nhất vào lượt cuối' },
                        ].map((opt) => (
                          <div
                            key={opt.val}
                            className={`p-2 rounded-2 border cursor-pointer d-flex align-items-center gap-2 ${
                              autoScheduleConfig.phuongThucPhanNhom === opt.val
                                ? 'border-warning bg-warning bg-opacity-10'
                                : 'bg-white'
                            }`}
                            onClick={() =>
                              setAutoScheduleConfig({
                                ...autoScheduleConfig,
                                phuongThucPhanNhom: opt.val as 'random' | 'registration_order' | 'performance_seed',
                              })
                            }
                          >
                            <input
                              type="radio"
                              readOnly
                              checked={autoScheduleConfig.phuongThucPhanNhom === opt.val}
                              className="form-check-input flex-shrink-0"
                              style={{ marginTop: 0 }}
                            />
                            <div>
                              <div className="fw-semibold" style={{ fontSize: '11.5px' }}>{opt.label}</div>
                              <div className="text-muted" style={{ fontSize: '10.5px' }}>{opt.sub}</div>
                            </div>
                          </div>
                        ))}
                      </div>
                    </div>
                  </Col>
                </Row>

                {/* Live Preview Panel */}
                {leaderboardPreview && leaderboardPreview.totalVdv > 0 && (
                  <div
                    className="mt-3 p-3 rounded-3 border d-flex flex-wrap gap-3 align-items-start"
                    style={{ background: 'linear-gradient(135deg, #1e293b 0%, #0f172a 100%)', borderColor: '#334155' }}
                  >
                    <div className="d-flex align-items-center gap-2 text-white mb-1 w-100">
                      <CalendarDays size={15} className="text-green-400" style={{ color: '#4ade80' }} />
                      <span className="fw-bold" style={{ fontSize: '12.5px' }}>📊 Dự kiến Lịch Thi Đấu</span>
                    </div>
                    {[
                      {
                        icon: <Users size={14} />,
                        label: 'Tổng VĐV',
                        value: `${leaderboardPreview.totalVdv} người`,
                        color: '#4ade80',
                      },
                      {
                        icon: <Layers size={14} />,
                        label: `Lượt thi / vòng (⌈${leaderboardPreview.totalVdv}÷${leaderboardPreview.heatSize}⌉)`,
                        value: `${leaderboardPreview.heatsPerVong} lượt`,
                        color: '#60a5fa',
                      },
                      {
                        icon: <Flag size={14} />,
                        label: `Tổng lượt thi (×${autoScheduleConfig.soVongThi} vòng)`,
                        value: `${leaderboardPreview.totalHeats} lượt`,
                        color: '#f59e0b',
                      },
                      {
                        icon: <Clock size={14} />,
                        label: 'Thời gian / lượt',
                        value: `${leaderboardPreview.minutesPerHeat} phút`,
                        color: '#a78bfa',
                      },
                      {
                        icon: <CalendarDays size={14} />,
                        label: 'Dự kiến số ngày',
                        value: `${leaderboardPreview.estimatedDays} ngày`,
                        color: '#fb7185',
                      },
                    ].map((stat, i) => (
                      <div key={i} className="d-flex flex-column align-items-start" style={{ minWidth: '120px' }}>
                        <div className="d-flex align-items-center gap-1 mb-0.5" style={{ color: '#94a3b8', fontSize: '11px' }}>
                          <span style={{ color: stat.color }}>{stat.icon}</span>
                          <span>{stat.label}</span>
                        </div>
                        <div className="fw-bold" style={{ fontSize: '15px', color: stat.color }}>
                          {stat.value}
                        </div>
                      </div>
                    ))}
                  </div>
                )}
                {leaderboardPreview && leaderboardPreview.totalVdv === 0 && (
                  <div className="mt-3 p-2 rounded-3 border border-warning bg-warning bg-opacity-10 d-flex align-items-center gap-2 small text-warning">
                    <AlertTriangle size={14} />
                    <span>Chưa có VĐV đã duyệt đăng ký trong nội dung này. Vui lòng duyệt đăng ký trước khi xếp lịch.</span>
                  </div>
                )}
              </div>
            </div>
          )}



          <Row className="g-3">
            {/* Cấu hình tự động theo môn từ CauHinhLichThiDau & Giải đấu */}
            <Col xs={12}>
              <div className="p-3 rounded-3 border bg-light shadow-sm">
                <div className="d-flex align-items-center justify-content-between mb-2 pb-2 border-bottom">
                  <div className="d-flex align-items-center gap-2">
                    <Sliders size={16} className="text-primary" />
                    <span className="fw-bold text-dark small">
                      Thông số xếp lịch theo Môn thi đấu & Giải đấu
                    </span>
                  </div>
                  <Badge color={sportScheduleConfig ? 'success' : 'secondary'} pill className="px-2.5 py-1">
                    {sportScheduleConfig ? 'Đã liên kết CauHinhLichThiDau' : 'Cấu hình mặc định hệ thống'}
                  </Badge>
                </div>

                <Row className="g-2 text-muted" style={{ fontSize: '12.5px' }}>
                  <Col xs={12} md={4}>
                    <div className="p-2 rounded border bg-white h-100">
                      <div className="text-secondary small d-flex align-items-center gap-1">
                        <Calendar size={12} className="text-primary" />
                        <span>Ngày bắt đầu giải:</span>
                      </div>
                      <div className="fw-bold text-dark mt-1">
                        {currentTournament?.ngayBatDau
                          ? new Date(currentTournament.ngayBatDau).toLocaleDateString('vi-VN')
                          : autoScheduleConfig.startDate}
                      </div>
                    </div>
                  </Col>
                  <Col xs={6} md={4}>
                    <div className="p-2 rounded border bg-white h-100">
                      <div className="text-secondary small d-flex align-items-center gap-1">
                        <Clock size={12} className="text-info" />
                        <span>Khung giờ thi đấu:</span>
                      </div>
                      <div className="fw-bold text-dark mt-1">
                        {sportScheduleConfig?.caSangBatDau || autoScheduleConfig.startTime || '08:00'} - {sportScheduleConfig?.caChieuKetThuc || autoScheduleConfig.endTime || '17:30'}
                      </div>
                      <div className="text-muted" style={{ fontSize: '10.5px' }}>
                        (Sáng: {sportScheduleConfig?.caSangBatDau || '08:00'}-{sportScheduleConfig?.caSangKetThuc || '11:30'} | Chiều: {sportScheduleConfig?.caChieuBatDau || '14:00'}-{sportScheduleConfig?.caChieuKetThuc || '17:30'})
                      </div>
                    </div>
                  </Col>
                  <Col xs={6} md={4}>
                    <div className="p-2 rounded border bg-white h-100">
                      <div className="text-secondary small d-flex align-items-center gap-1">
                        <Zap size={12} className="text-warning" />
                        <span>Thời lượng & Nghỉ:</span>
                      </div>
                      <div className="fw-bold text-dark mt-1">
                        {(sportScheduleConfig?.thoiLuongTranMacDinhPhut || autoScheduleConfig.matchDuration || 60)} phút / trận
                      </div>
                      <div className="text-muted" style={{ fontSize: '10.5px' }}>
                        Nghỉ đệm giữa 2 trận: {sportScheduleConfig?.thoiGianDemDonSanPhut ?? autoScheduleConfig.breakDuration ?? 15} phút
                      </div>
                    </div>
                  </Col>
                  <Col xs={12} md={6}>
                    <div className="p-2 rounded border bg-white h-100">
                      <div className="text-secondary small d-flex align-items-center gap-1">
                        <Award size={12} className="text-danger" />
                        <span>Quy cách hiệp đấu:</span>
                      </div>
                      <div className="fw-bold text-dark mt-1">
                        {sportScheduleConfig && sportScheduleConfig.soHiepDauMacDinh > 0
                          ? `${sportScheduleConfig.soHiepDauMacDinh} hiệp (${sportScheduleConfig.thoiGianMoiHiepPhut} phút/hiệp)`
                          : autoScheduleConfig.soHiepDau > 0
                            ? `${autoScheduleConfig.soHiepDau} hiệp (${autoScheduleConfig.thoiGianMoiHiepPhut} phút/hiệp)`
                            : 'Không phân hiệp riêng (theo thời lượng trận)'}
                      </div>
                    </div>
                  </Col>
                  <Col xs={12} md={6}>
                    <div className="p-2 rounded border bg-white h-100">
                      <div className="text-secondary small d-flex align-items-center gap-1">
                        <Users size={12} className="text-success" />
                        <span>Nghỉ tối thiểu giữa 2 trận của VĐV:</span>
                      </div>
                      <div className="fw-bold text-dark mt-1">
                        {effectiveRestInfo.minutes} phút
                        {effectiveRestInfo.source === 'khoangCachVong' ? (
                          <span className="text-primary fw-normal ms-1" style={{ fontSize: '11px' }}>
                            ({sportScheduleConfig?.khoangCachGiuaCacVongGio} giờ - ưu tiên Khoảng cách giữa các vòng)
                          </span>
                        ) : (
                          <span className="text-muted fw-normal ms-1" style={{ fontSize: '11px' }}>
                            (theo Nghỉ tối thiểu 2 trận)
                          </span>
                        )}
                      </div>
                    </div>
                  </Col>
                </Row>

                <div className="mt-2 pt-2 border-top d-flex justify-content-between align-items-center text-secondary" style={{ fontSize: '11px' }}>
                  <span>
                    <Info size={12} className="me-1 text-info" />
                    Các thông số ngày, giờ và hiệp đấu được áp dụng tự động từ bảng cấu hình <strong>CauHinhLichThiDau</strong> của môn và ngày giải đấu.
                  </span>
                  <Link href="/quan-ly-giai/mon-the-thao" target="_blank" className="text-primary text-decoration-none d-flex align-items-center gap-1">
                    <span>Cấu hình môn</span>
                    <ExternalLink size={12} />
                  </Link>
                </div>
              </div>
            </Col>

            {/* Cấu hình chuyên biệt cho Vòng Tròn Tính Điểm (Round Robin) */}
            {isRoundRobin && (
              <Col xs={12}>
                <div className="p-3.5 rounded-3 border bg-light bg-opacity-75">
                  <div className="d-flex align-items-center justify-content-between mb-3 pb-2 border-bottom">
                    <div className="d-flex align-items-center gap-2">
                      <Layers size={16} className="text-success" />
                      <span className="fw-bold small text-dark">Cấu Hình Thể Thức Vòng Tròn Tính Điểm (Round Robin)</span>
                    </div>
                    <Badge color="success" pill className="px-2.5 py-1">
                      {autoScheduleConfig.soLuotDau === 2 ? 'Lượt đi & Lượt về' : '1 Lượt (Single Round Robin)'}
                    </Badge>
                  </div>

                  {/* 1. Chế độ bảng đấu */}
                  <div className="mb-3">
                    <Label className="small fw-semibold mb-1 text-secondary">Mô hình phân bảng:</Label>
                    <div className="d-flex flex-column flex-sm-row gap-2">
                      <div
                        className={`p-2.5 rounded-3 border cursor-pointer flex-fill d-flex align-items-start gap-2.5 transition-all ${
                          autoScheduleConfig.cheDoVongBang === 'single_group'
                            ? 'border-success bg-success bg-opacity-10 shadow-sm'
                            : 'bg-white'
                        }`}
                        onClick={() => setAutoScheduleConfig({ ...autoScheduleConfig, cheDoVongBang: 'single_group' })}
                      >
                        <input
                          type="radio"
                          name="cheDoVongBang"
                          checked={autoScheduleConfig.cheDoVongBang === 'single_group'}
                          onChange={() => {}}
                          className="form-check-input mt-1"
                        />
                        <div>
                          <div className="fw-bold small text-dark">1 Bảng duy nhất (Tất cả gặp nhau)</div>
                          <div className="text-muted" style={{ fontSize: '11px' }}>
                            Tất cả {registeredTeams.length} đội/VĐV gom chung 1 bảng xoay vòng, đội điểm cao nhất vô địch.
                          </div>
                        </div>
                      </div>

                      <div
                        className={`p-2.5 rounded-3 border cursor-pointer flex-fill d-flex align-items-start gap-2.5 transition-all ${
                          autoScheduleConfig.cheDoVongBang === 'multi_groups'
                            ? 'border-success bg-success bg-opacity-10 shadow-sm'
                            : 'bg-white'
                        }`}
                        onClick={() => setAutoScheduleConfig({ ...autoScheduleConfig, cheDoVongBang: 'multi_groups' })}
                      >
                        <input
                          type="radio"
                          name="cheDoVongBang"
                          checked={autoScheduleConfig.cheDoVongBang === 'multi_groups'}
                          onChange={() => {}}
                          className="form-check-input mt-1"
                        />
                        <div>
                          <div className="fw-bold small text-dark">Chia thành nhiều bảng đấu</div>
                          <div className="text-muted" style={{ fontSize: '11px' }}>
                            Chia thành Bảng A, B, C... thi đấu vòng tròn tính điểm trong từng bảng.
                          </div>
                        </div>
                      </div>
                    </div>

                    {autoScheduleConfig.cheDoVongBang === 'multi_groups' && (
                      <div className="d-flex align-items-center gap-2 mt-2.5 ps-1">
                        <Label className="small mb-0 text-secondary">Số đội mỗi bảng:</Label>
                        <Input
                          type="select"
                          bsSize="sm"
                          style={{ width: '130px' }}
                          value={autoScheduleConfig.teamsPerGroup}
                          onChange={(e) => setAutoScheduleConfig({ ...autoScheduleConfig, teamsPerGroup: Number(e.target.value) })}
                        >
                          <option value={3}>3 đội / bảng</option>
                          <option value={4}>4 đội / bảng</option>
                          <option value={5}>5 đội / bảng</option>
                          <option value={6}>6 đội / bảng</option>
                        </Input>
                        <span className="text-muted small">
                          (Dự kiến: {Math.max(1, Math.ceil(registeredTeams.length / (autoScheduleConfig.teamsPerGroup || 4)))} bảng)
                        </span>
                      </div>
                    )}
                  </div>

                  {/* 2. Số lượt thi đấu */}
                  <div className="mb-3">
                    <Label className="small fw-semibold mb-1 text-secondary">Số lượt thi đấu (Vòng xoay):</Label>
                    <div className="d-flex flex-column flex-sm-row gap-2">
                      <div
                        className={`p-2.5 rounded-3 border cursor-pointer flex-fill d-flex align-items-start gap-2.5 transition-all ${
                          autoScheduleConfig.soLuotDau === 1
                            ? 'border-success bg-success bg-opacity-10 shadow-sm'
                            : 'bg-white'
                        }`}
                        onClick={() => setAutoScheduleConfig({ ...autoScheduleConfig, soLuotDau: 1 })}
                      >
                        <input
                          type="radio"
                          name="soLuotDau"
                          checked={autoScheduleConfig.soLuotDau === 1}
                          onChange={() => {}}
                          className="form-check-input mt-1"
                        />
                        <div>
                          <div className="fw-bold small text-dark">1 Lượt (Lượt đi - Single Round Robin)</div>
                          <div className="text-muted" style={{ fontSize: '11px' }}>
                            Mỗi cặp đấu chỉ gặp nhau 1 lần duy nhất trong giải.
                          </div>
                        </div>
                      </div>

                      <div
                        className={`p-2.5 rounded-3 border cursor-pointer flex-fill d-flex align-items-start gap-2.5 transition-all ${
                          autoScheduleConfig.soLuotDau === 2
                            ? 'border-success bg-success bg-opacity-10 shadow-sm'
                            : 'bg-white'
                        }`}
                        onClick={() => setAutoScheduleConfig({ ...autoScheduleConfig, soLuotDau: 2 })}
                      >
                        <input
                          type="radio"
                          name="soLuotDau"
                          checked={autoScheduleConfig.soLuotDau === 2}
                          onChange={() => {}}
                          className="form-check-input mt-1"
                        />
                        <div>
                          <div className="fw-bold small text-dark">2 Lượt (Lượt đi & Lượt về - Double Round Robin)</div>
                          <div className="text-muted" style={{ fontSize: '11px' }}>
                            Mỗi cặp đội gặp nhau 2 lần, đảo vai trò Đội 1 (Home) và Đội 2 (Away).
                          </div>
                        </div>
                      </div>
                    </div>
                  </div>

                  {/* 3. Quy chuẩn tính điểm */}
                  <div className="mb-2">
                    <Label className="small fw-semibold mb-1 text-secondary">Quy chuẩn điểm số xếp hạng:</Label>
                    <div className="d-flex flex-wrap gap-2 mb-2">
                      {[
                        { key: '3_1_0', label: 'Bóng đá / Futsal (3 - 1 - 0)', win: 3, draw: 1, loss: 0 },
                        { key: '2_1_0', label: 'Bóng chuyền / Bóng rổ (2 - 1 - 0)', win: 2, draw: 1, loss: 0 },
                        { key: '1_0.5_0', label: 'Cờ vua / Cờ tướng (1 - 0.5 - 0)', win: 1, draw: 0.5, loss: 0 },
                        { key: '1_0', label: 'Bóng bàn / Cầu lông (1 - 0)', win: 1, draw: 0, loss: 0 },
                        { key: 'custom', label: 'Tùy chỉnh...', win: autoScheduleConfig.diemThang, draw: autoScheduleConfig.diemHoa, loss: autoScheduleConfig.diemThua },
                      ].map((p) => (
                        <Button
                          key={p.key}
                          size="sm"
                          color={autoScheduleConfig.heThongDiem === p.key ? 'success' : 'light'}
                          className={`rounded-pill px-3 py-1 ${autoScheduleConfig.heThongDiem === p.key ? 'text-white' : 'text-secondary border'}`}
                          style={{ fontSize: '11px' }}
                          onClick={() => {
                            setAutoScheduleConfig({
                              ...autoScheduleConfig,
                              heThongDiem: p.key,
                              diemThang: p.win,
                              diemHoa: p.draw,
                              diemThua: p.loss,
                            });
                          }}
                        >
                          {p.label}
                        </Button>
                      ))}
                    </div>

                    <div className="d-flex align-items-center gap-3 bg-white p-2.5 rounded-3 border">
                      <div className="d-flex align-items-center gap-1.5">
                        <span className="small fw-semibold text-success">Thắng:</span>
                        <Input
                          type="number"
                          step="0.5"
                          bsSize="sm"
                          style={{ width: '65px' }}
                          value={autoScheduleConfig.diemThang}
                          disabled={autoScheduleConfig.heThongDiem !== 'custom'}
                          onChange={(e) => setAutoScheduleConfig({ ...autoScheduleConfig, diemThang: Number(e.target.value) })}
                        />
                        <span className="small text-muted">điểm</span>
                      </div>
                      <div className="d-flex align-items-center gap-1.5">
                        <span className="small fw-semibold text-secondary">Hòa:</span>
                        <Input
                          type="number"
                          step="0.5"
                          bsSize="sm"
                          style={{ width: '65px' }}
                          value={autoScheduleConfig.diemHoa}
                          disabled={autoScheduleConfig.heThongDiem !== 'custom'}
                          onChange={(e) => setAutoScheduleConfig({ ...autoScheduleConfig, diemHoa: Number(e.target.value) })}
                        />
                        <span className="small text-muted">điểm</span>
                      </div>
                      <div className="d-flex align-items-center gap-1.5">
                        <span className="small fw-semibold text-danger">Thua:</span>
                        <Input
                          type="number"
                          step="0.5"
                          bsSize="sm"
                          style={{ width: '65px' }}
                          value={autoScheduleConfig.diemThua}
                          disabled={autoScheduleConfig.heThongDiem !== 'custom'}
                          onChange={(e) => setAutoScheduleConfig({ ...autoScheduleConfig, diemThua: Number(e.target.value) })}
                        />
                        <span className="small text-muted">điểm</span>
                      </div>
                    </div>
                  </div>

                  <div className="text-muted mt-2" style={{ fontSize: '11px' }}>
                    ℹ️ <em>Tiêu chí xếp hạng khi bằng điểm: <strong>Điểm số ➔ Đối đầu trực tiếp ➔ Hiệu số (+/-) ➔ Tổng điểm/bàn thắng ghi được</strong>.</em>
                  </div>
                </div>
              </Col>
            )}

            {/* Live preview cho Round Robin */}
            {isRoundRobin && roundRobinPreview && (
              <Col xs={12}>
                <div
                  className="p-3 rounded-3 text-white shadow-sm"
                  style={{ background: 'linear-gradient(135deg, #064e3b 0%, #047857 100%)' }}
                >
                  <div className="d-flex align-items-center justify-content-between mb-2 pb-1 border-bottom border-white border-opacity-25">
                    <div className="d-flex align-items-center gap-1.5">
                      <Zap size={14} className="text-warning" />
                      <span className="fw-bold small">Dự Báo Lịch Thi Đấu Vòng Tròn (Live Preview)</span>
                    </div>
                    <Badge color="light" pill className="text-dark fw-bold px-2.5 py-1" style={{ fontSize: '10px' }}>
                      Cập nhật tức thời
                    </Badge>
                  </div>
                  <Row className="g-2 text-center" style={{ fontSize: '12px' }}>
                    <Col xs={4} md={2}>
                      <div className="text-white-50" style={{ fontSize: '10px' }}>ĐỘI THAM GIA</div>
                      <div className="fw-bold fs-6 text-white">{roundRobinPreview.totalTeams}</div>
                      <div className="text-white-50" style={{ fontSize: '9.5px' }}>đội đăng ký</div>
                    </Col>
                    <Col xs={4} md={2}>
                      <div className="text-white-50" style={{ fontSize: '10px' }}>BẢNG ĐẤU</div>
                      <div className="fw-bold fs-6 text-warning">
                        {roundRobinPreview.isSingleGroup ? '1 Bảng' : `${roundRobinPreview.numGroups} Bảng`}
                      </div>
                      <div className="text-white-50" style={{ fontSize: '9.5px' }}>
                        {roundRobinPreview.teamsInGroup} đội/bảng
                      </div>
                    </Col>
                    <Col xs={4} md={2}>
                      <div className="text-white-50" style={{ fontSize: '10px' }}>LƯỢT ĐẤU</div>
                      <div className="fw-bold fs-6 text-info">
                        {roundRobinPreview.soLuot === 2 ? '2 lượt' : '1 lượt'}
                      </div>
                      <div className="text-white-50" style={{ fontSize: '9.5px' }}>
                        {roundRobinPreview.soLuot === 2 ? 'Lượt đi + về' : 'Lượt đi'}
                      </div>
                    </Col>
                    <Col xs={4} md={2}>
                      <div className="text-white-50" style={{ fontSize: '10px' }}>TỔNG SỐ VÒNG</div>
                      <div className="fw-bold fs-6 text-white">{roundRobinPreview.totalRounds}</div>
                      <div className="text-white-50" style={{ fontSize: '9.5px' }}>vòng đấu xoay</div>
                    </Col>
                    <Col xs={4} md={2}>
                      <div className="text-white-50" style={{ fontSize: '10px' }}>TỔNG SỐ TRẬN</div>
                      <div className="fw-bold fs-6 text-warning">{roundRobinPreview.totalMatches}</div>
                      <div className="text-white-50" style={{ fontSize: '9.5px' }}>trận thi đấu</div>
                    </Col>
                    <Col xs={4} md={2}>
                      <div className="text-white-50" style={{ fontSize: '10px' }}>DỰ KIẾN</div>
                      <div className="fw-bold fs-6 text-success bg-white px-1.5 py-0.5 rounded-pill d-inline-block">
                        ~{roundRobinPreview.estimatedDays} ngày
                      </div>
                      <div className="text-white-50" style={{ fontSize: '9.5px' }}>thời gian thi đấu</div>
                    </Col>
                  </Row>
                </div>
              </Col>
            )}

            {/* Group stage configuration if applicable — chỉ hiện cho Kết hợp Vòng bảng & Loại trực tiếp */}
            {hasGroupStage && !isLeaderboard && !isRoundRobin && (
              <Col xs={12}>
                <div className="p-3 rounded-3 bg-light border">
                  <div className="form-check mb-2">
                    <Input
                      type="checkbox"
                      id="autoCreateGroups"
                      checked={autoScheduleConfig.autoCreateGroups}
                      onChange={(e) => setAutoScheduleConfig({ ...autoScheduleConfig, autoCreateGroups: e.target.checked })}
                    />
                    <Label check htmlFor="autoCreateGroups" className="fw-semibold small">
                      Tự động bốc thăm chia Bảng đấu nếu chưa có bảng
                    </Label>
                  </div>

                  {autoScheduleConfig.autoCreateGroups && (
                    <div className="d-flex align-items-center gap-2 mt-2">
                      <Label className="small mb-0">Số đội mỗi bảng:</Label>
                      <Input
                        type="select"
                        bsSize="sm"
                        style={{ width: '120px' }}
                        value={autoScheduleConfig.teamsPerGroup}
                        onChange={(e) => setAutoScheduleConfig({ ...autoScheduleConfig, teamsPerGroup: Number(e.target.value) })}
                      >
                        <option value={3}>3 đội / bảng</option>
                        <option value={4}>4 đội / bảng</option>
                        <option value={5}>5 đội / bảng</option>
                        <option value={6}>6 đội / bảng</option>
                      </Input>
                    </div>
                  )}
                </div>
              </Col>
            )}

            {/* Advancing criteria to Knockout Stage if KetHopVongBangVaLoaiTrucTiep — ẩn với Leaderboard */}
            {currentEvent?.hinhThucThiDau === 'KetHopVongBangVaLoaiTrucTiep' && !isLeaderboard && (
              <Col xs={12}>
                <div className="p-3 rounded-3 border bg-light">
                  <div className="d-flex align-items-center justify-content-between mb-2 pb-2 border-bottom">
                    <div className="d-flex align-items-center gap-2">
                      <Trophy size={16} className="text-warning" />
                      <span className="fw-bold small text-dark">Tiêu chí Đội vượt qua Vòng Bảng vào Vòng Loại Trực Tiếp</span>
                    </div>
                    {predictedAdvancingInfo && (
                      <Badge color="primary" pill className="px-2.5 py-1">
                        {predictedAdvancingInfo.totalAdvancing} đội vào vòng trong
                      </Badge>
                    )}
                  </div>

                  {/* Option 1: Top N per group */}
                  <div className="mb-3">
                    <Label className="small fw-semibold mb-1 text-secondary">Số lượng đội đi tiếp từ mỗi bảng:</Label>
                    <div className="d-flex flex-column flex-sm-row gap-2">
                      <div
                        className={`p-2.5 rounded-3 border cursor-pointer flex-fill d-flex align-items-start gap-2.5 transition-all ${
                          autoScheduleConfig.soDoiMoiBangVaoVongTrong === 2
                            ? 'border-primary bg-primary bg-opacity-10 shadow-sm'
                            : 'bg-white'
                        }`}
                        onClick={() => setAutoScheduleConfig({ ...autoScheduleConfig, soDoiMoiBangVaoVongTrong: 2 })}
                      >
                        <input
                          type="radio"
                          name="soDoiMoiBang"
                          checked={autoScheduleConfig.soDoiMoiBangVaoVongTrong === 2}
                          onChange={() => {}}
                          className="form-check-input mt-1"
                        />
                        <div>
                          <div className="fw-bold small text-dark">Lấy Đội Nhất & Nhì mỗi bảng (2 đội / bảng)</div>
                          <div className="text-muted" style={{ fontSize: '11px' }}>
                            Mô hình chuẩn: các đội Nhất và Nhì bảng phân nhánh chéo vào Tứ kết / Bán kết / Chung kết.
                          </div>
                        </div>
                      </div>

                      <div
                        className={`p-2.5 rounded-3 border cursor-pointer flex-fill d-flex align-items-start gap-2.5 transition-all ${
                          autoScheduleConfig.soDoiMoiBangVaoVongTrong === 1
                            ? 'border-primary bg-primary bg-opacity-10 shadow-sm'
                            : 'bg-white'
                        }`}
                        onClick={() =>
                          setAutoScheduleConfig({
                            ...autoScheduleConfig,
                            soDoiMoiBangVaoVongTrong: 1,
                            layDoiThu3TotNhat: false,
                            soDoiThu3TotNhat: 0,
                          })
                        }
                      >
                        <input
                          type="radio"
                          name="soDoiMoiBang"
                          checked={autoScheduleConfig.soDoiMoiBangVaoVongTrong === 1}
                          onChange={() => {}}
                          className="form-check-input mt-1"
                        />
                        <div>
                          <div className="fw-bold small text-dark">Chỉ lấy Đội Nhất mỗi bảng (1 đội / bảng)</div>
                          <div className="text-muted" style={{ fontSize: '11px' }}>
                            Chỉ các đội đầu bảng mới giành vé vào thẳng Bán kết / Chung kết.
                          </div>
                        </div>
                      </div>
                    </div>
                  </div>

                  {/* Option 2: Wildcard 3rd-placed teams */}
                  {autoScheduleConfig.soDoiMoiBangVaoVongTrong >= 2 && (
                    <div className="pt-2 border-top">
                      <div className="d-flex align-items-center justify-content-between flex-wrap gap-2">
                        <div className="form-check mb-0">
                          <Input
                            type="checkbox"
                            id="layDoiThu3"
                            checked={autoScheduleConfig.layDoiThu3TotNhat}
                            onChange={(e) =>
                              setAutoScheduleConfig({
                                ...autoScheduleConfig,
                                layDoiThu3TotNhat: e.target.checked,
                                soDoiThu3TotNhat: e.target.checked ? (autoScheduleConfig.soDoiThu3TotNhat || 4) : 0,
                              })
                            }
                          />
                          <Label check htmlFor="layDoiThu3" className="small fw-semibold cursor-pointer mb-0">
                            Lấy thêm các đội thứ 3 có thành tích tốt nhất (Vé vớt - Wildcard)
                          </Label>
                        </div>

                        {autoScheduleConfig.layDoiThu3TotNhat && (
                          <div className="d-flex align-items-center gap-2">
                            <span className="small text-muted">Số đội thứ 3 lấy thêm:</span>
                            <Input
                              type="select"
                              bsSize="sm"
                              style={{ width: '120px' }}
                              value={autoScheduleConfig.soDoiThu3TotNhat}
                              onChange={(e) =>
                                setAutoScheduleConfig({ ...autoScheduleConfig, soDoiThu3TotNhat: Number(e.target.value) })
                              }
                            >
                              <option value={1}>1 đội thứ 3</option>
                              <option value={2}>2 đội thứ 3</option>
                              <option value={4}>4 đội thứ 3 (EURO)</option>
                            </Input>
                          </div>
                        )}
                      </div>
                      <div className="text-muted ms-4 mt-1" style={{ fontSize: '11px' }}>
                        Ví dụ mô hình EURO: 6 bảng lấy 12 đội (Nhất + Nhì) + 4 đội thứ 3 tốt nhất = 16 đội vào Vòng 1/8.
                      </div>
                    </div>
                  )}

                  {/* Live preview banner */}
                  {predictedAdvancingInfo && (
                    <div className="mt-3 p-2 bg-white rounded-2 border d-flex align-items-center justify-content-between flex-wrap gap-2">
                      <div className="d-flex align-items-center gap-2 small">
                        <Layers size={14} className="text-primary flex-shrink-0" />
                        <span className="text-muted">Sơ đồ Knock-out sinh tự động:</span>
                        <span className="fw-semibold text-primary">{predictedAdvancingInfo.bracketDesc}</span>
                      </div>
                      <div className="text-muted font-monospace" style={{ fontSize: '11px' }}>
                        ({predictedAdvancingInfo.estNumGroups} bảng × {predictedAdvancingInfo.soDoiMoiBang}
                        {predictedAdvancingInfo.soDoiThu3 > 0 ? ` + ${predictedAdvancingInfo.soDoiThu3} thứ 3` : ''}) = {predictedAdvancingInfo.totalAdvancing} đội
                      </div>
                    </div>
                  )}
                </div>
              </Col>
            )}

            {/* Venue selection */}
            <Col xs={12}>
              <Label className="small fw-semibold d-flex align-items-center justify-content-between">
                <span>Chọn các Sân đấu áp dụng:</span>
                <span className="text-primary cursor-pointer" onClick={() => setAutoScheduleConfig({ ...autoScheduleConfig, selectedVenueIds: venues.map((v) => v.id) })}>
                  Chọn tất cả ({venues.length})
                </span>
              </Label>
              <div className="d-flex flex-wrap gap-2 p-2.5 rounded-3 border bg-light" style={{ maxHeight: '120px', overflowY: 'auto' }}>
                {venues.map((v) => {
                  const isChecked = autoScheduleConfig.selectedVenueIds.includes(v.id);
                  return (
                    <Badge
                      key={v.id}
                      color={isChecked ? 'primary' : 'light'}
                      className={`cursor-pointer px-3 py-1.5 border ${isChecked ? 'text-white' : 'text-dark'}`}
                      onClick={() => {
                        const newIds = isChecked
                          ? autoScheduleConfig.selectedVenueIds.filter((id) => id !== v.id)
                          : [...autoScheduleConfig.selectedVenueIds, v.id];
                        setAutoScheduleConfig({ ...autoScheduleConfig, selectedVenueIds: newIds });
                      }}
                    >
                      <MapPin size={11} className="me-1" />
                      {v.ten}
                    </Badge>
                  );
                })}
              </div>
            </Col>

            {/* Referee selection */}
            <Col xs={12}>
              <div className="d-flex align-items-center justify-content-between mb-1">
                <Label className="small fw-semibold mb-0">Chọn các Trọng tài điều hành:</Label>
                <div className="d-flex align-items-center gap-2">
                  <span className="small text-muted">Số TT / trận:</span>
                  <Input
                    type="select"
                    bsSize="sm"
                    style={{ width: '80px' }}
                    value={autoScheduleConfig.refereesPerMatch}
                    onChange={(e) => setAutoScheduleConfig({ ...autoScheduleConfig, refereesPerMatch: Number(e.target.value) })}
                  >
                    <option value={1}>1 người</option>
                    <option value={2}>2 người</option>
                    <option value={3}>3 người</option>
                  </Input>
                </div>
              </div>
              <div className="d-flex flex-wrap gap-2 p-2.5 rounded-3 border bg-light" style={{ maxHeight: '120px', overflowY: 'auto' }}>
                {referees.map((r) => {
                  const isChecked = autoScheduleConfig.selectedRefereeIds.includes(r.id);
                  return (
                    <Badge
                      key={r.id}
                      color={isChecked ? 'warning' : 'light'}
                      className={`cursor-pointer px-3 py-1.5 border ${isChecked ? 'text-dark fw-bold' : 'text-secondary'}`}
                      onClick={() => {
                        const newIds = isChecked
                          ? autoScheduleConfig.selectedRefereeIds.filter((id) => id !== r.id)
                          : [...autoScheduleConfig.selectedRefereeIds, r.id];
                        setAutoScheduleConfig({ ...autoScheduleConfig, selectedRefereeIds: newIds });
                      }}
                    >
                      <Shield size={11} className="me-1" />
                      {r.hoTen}
                    </Badge>
                  );
                })}
              </div>
            </Col>

            {/* Avoid athlete conflict checkbox & Rest time — ẩn với Leaderboard (VĐV không thi trùng giờ cùng lúc) */}
            {!isLeaderboard && (
              <Col xs={12}>
                <div className="p-2.5 rounded-3 border bg-light">
                  <div className="form-check mb-1">
                    <Input
                      type="checkbox"
                      id="avoidAthleteConflict"
                      className="ms-0 me-2"
                      checked={autoScheduleConfig.avoidAthleteConflict}
                      onChange={(e) => setAutoScheduleConfig({ ...autoScheduleConfig, avoidAthleteConflict: e.target.checked })}
                    />
                    <Label check htmlFor="avoidAthleteConflict" className="small text-primary fw-bold cursor-pointer">
                      Tự động kiểm tra & tránh trùng khung giờ cho VĐV (Smart CSP)
                    </Label>
                  </div>
                  <div className="text-muted small ps-4" style={{ fontSize: '11px', lineHeight: 1.4 }}>
                    Hệ thống nhận diện cùng một VĐV khi trùng số CCCD (hoặc cùng mã VĐV), đối soát lịch toàn bộ các môn khác và tự động dời ca giờ tránh xung đột.
                  </div>

                  {autoScheduleConfig.avoidAthleteConflict && (
                    <div className="d-flex align-items-center gap-2 mt-2 pt-2 border-top ps-4">
                      <span className="small text-secondary fw-semibold">
                        Thời gian nghỉ tối thiểu giữa 2 trận:
                      </span>
                      <Badge color="primary" className="px-2.5 py-1 fw-bold">
                        {effectiveRestInfo.minutes} phút
                      </Badge>
                      <span className="small text-muted" style={{ fontSize: '11px' }}>
                        {effectiveRestInfo.source === 'khoangCachVong'
                          ? `(Ưu tiên theo ${sportScheduleConfig?.khoangCachGiuaCacVongGio} giờ Khoảng cách giữa các vòng)`
                          : `(Theo ${effectiveRestInfo.minutes} phút Nghỉ tối thiểu giữa 2 trận của môn)`}
                      </span>
                    </div>
                  )}
                </div>
              </Col>
            )}

            {/* Leaderboard: thông tin thay thế cho mục tránh trùng VĐV */}
            {isLeaderboard && (
              <Col xs={12}>
                <div
                  className="p-2.5 rounded-3 border d-flex align-items-start gap-2"
                  style={{ background: '#f0fdf4', borderColor: '#86efac', fontSize: '12px' }}
                >
                  <CheckCircle size={15} className="text-success flex-shrink-0 mt-0.5" />
                  <div>
                    <div className="fw-semibold text-success mb-0.5">Kiểm tra xung đột lịch thi đấu</div>
                    <div className="text-muted">
                      Với thể thức Thi Thành Tích, mỗi VĐV tham gia <strong>một lượt thi riêng biệt</strong>.
                      Hệ thống tự động đảm bảo VĐV thi đa môn không bị trùng khung giờ giữa các nội dung khác nhau trong cùng giải.
                    </div>
                  </div>
                </div>
              </Col>
            )}

            {/* Load balancing options */}
            <Col xs={12}>
              <div className="p-2.5 rounded-3 border bg-light">
                <div className="fw-semibold small text-dark mb-2">Tối ưu hóa phân bổ (Heuristic Constraints):</div>
                <div className="d-flex flex-column gap-1.5">
                  <div className="form-check">
                    <Input
                      type="checkbox"
                      id="canBangTaiSanDau"
                      checked={autoScheduleConfig.canBangTaiSanDau}
                      onChange={(e) => setAutoScheduleConfig({ ...autoScheduleConfig, canBangTaiSanDau: e.target.checked })}
                    />
                    <Label check htmlFor="canBangTaiSanDau" className="small text-secondary cursor-pointer">
                      Cân bằng tải các sân đấu (chia đều mật độ trận đấu trên các sân được chọn)
                    </Label>
                  </div>
                  <div className="form-check">
                    <Input
                      type="checkbox"
                      id="canBangTaiTrongTai"
                      checked={autoScheduleConfig.canBangTaiTrongTai}
                      onChange={(e) => setAutoScheduleConfig({ ...autoScheduleConfig, canBangTaiTrongTai: e.target.checked })}
                    />
                    <Label check htmlFor="canBangTaiTrongTai" className="small text-secondary cursor-pointer">
                      Xoay tua phân công trọng tài (chia đều số trận cho các trọng tài làm nhiệm vụ)
                    </Label>
                  </div>
                </div>
              </div>
            </Col>

            {/* Clear old matches checkbox */}
            <Col xs={12}>
              <div className="form-check">
                <Input
                  type="checkbox"
                  id="clearOldMatches"
                  checked={autoScheduleConfig.clearOldMatches}
                  onChange={(e) => setAutoScheduleConfig({ ...autoScheduleConfig, clearOldMatches: e.target.checked })}
                />
                <Label check htmlFor="clearOldMatches" className="small text-danger fw-semibold">
                  Xóa lịch thi đấu cũ của nội dung này trước khi sinh lịch mới
                </Label>
              </div>
            </Col>
          </Row>
        </ModalBody>
        <ModalFooter className="border-top">
          <Button color="secondary" size="sm" className="rounded-pill px-3" onClick={() => setAutoScheduleModalOpen(false)}>
            Hủy
          </Button>
          <Button
            color="primary"
            size="sm"
            className="rounded-pill px-4 fw-semibold d-flex align-items-center gap-1.5"
            onClick={handleAutoSchedule}
            disabled={autoScheduleSubmitting}
          >
            {autoScheduleSubmitting ? <Spinner size="sm" /> : <Zap size={15} />}
            <span>Bắt Đầu Xếp Lịch</span>
          </Button>
        </ModalFooter>
      </Modal>

      {/* ======================================================== */}
      {/* MODAL 2: CREATE / EDIT MANUAL MATCH                      */}
      {/* ======================================================== */}
      <Modal isOpen={manualMatchModalOpen} toggle={() => setManualMatchModalOpen(!manualMatchModalOpen)} size="lg" centered>
        <ModalHeader toggle={() => setManualMatchModalOpen(!manualMatchModalOpen)} className="border-bottom">
          <div className="d-flex align-items-center gap-2">
            <Edit2 size={18} className="text-primary" />
            <span className="fw-bold fs-6">
              {editingMatchId ? 'Chỉnh Sửa Trận Đấu & Phân Công' : 'Thêm Mới Trận Đấu Thủ Công'}
            </span>
          </div>
        </ModalHeader>
        <ModalBody className="p-4">
          {conflictWarning.length > 0 && (
            <Alert color="danger" className="py-2.5 px-3 rounded-3 mb-3 shadow-sm border-danger" style={{ fontSize: '12px' }}>
              <div className="fw-bold d-flex align-items-center gap-2 mb-2 text-danger fs-6">
                <AlertTriangle size={18} />
                <span>Phát hiện {conflictWarning.length} xung đột lịch thi đấu:</span>
              </div>

              {chiTietConflictWarning.length > 0 ? (
                <div className="d-flex flex-column gap-2 mb-1">
                  {chiTietConflictWarning.map((c, i) => (
                    <div key={i} className="p-2.5 bg-white rounded-2 border border-danger-subtle shadow-xs">
                      <div className="d-flex flex-wrap align-items-center gap-1.5 mb-1">
                        {c.loaiXungDot === 'VanDongVien' ? (
                          <Badge color="danger" className="px-2 py-1 d-inline-flex align-items-center gap-1">
                            <UserX size={12} /> Trùng Lịch VĐV
                          </Badge>
                        ) : c.loaiXungDot === 'SanDau' ? (
                          <Badge color="warning" className="text-dark px-2 py-1 d-inline-flex align-items-center gap-1">
                            <MapPin size={12} /> Trùng Sân Đấu
                          </Badge>
                        ) : c.loaiXungDot === 'TrongTai' ? (
                          <Badge color="info" className="px-2 py-1 d-inline-flex align-items-center gap-1">
                            <Shield size={12} /> Trùng Trọng Tài
                          </Badge>
                        ) : (
                          <Badge color="secondary" className="px-2 py-1">Trùng Đội</Badge>
                        )}

                        {c.tenVanDongVien && (
                          <span className="fw-bold text-dark">{c.tenVanDongVien} ({c.tenDoiHienTai || 'Đội'})</span>
                        )}
                      </div>
                      <div className="text-dark ps-0.5" style={{ fontSize: '11.5px' }}>
                        {c.thongBao}
                      </div>
                    </div>
                  ))}
                </div>
              ) : (
                <ul className="mb-0 ps-3">
                  {conflictWarning.map((c, i) => (
                    <li key={i}>{c}</li>
                  ))}
                </ul>
              )}
            </Alert>
          )}

          <Row className="g-3">
            <Col xs={12} md={4}>
              <Label className="small fw-semibold">Vòng đấu *</Label>
              <Input
                type="select"
                bsSize="sm"
                value={matchForm.roundId}
                onChange={(e) => setMatchForm({ ...matchForm, roundId: Number(e.target.value) })}
              >
                <option value="">-- Chọn vòng đấu --</option>
                {rounds.map((r) => (
                  <option key={r.id} value={r.id}>
                    {r.ten}
                  </option>
                ))}
              </Input>
            </Col>

            {hasGroupStage && (
              <Col xs={12} md={4}>
                <Label className="small fw-semibold">Bảng đấu (nếu có)</Label>
                <Input
                  type="select"
                  bsSize="sm"
                  value={matchForm.groupId}
                  onChange={(e) => setMatchForm({ ...matchForm, groupId: e.target.value ? Number(e.target.value) : '' })}
                >
                  <option value="">-- Không thuộc bảng (Knockout) --</option>
                  {groups.map((g) => (
                    <option key={g.id} value={g.id}>
                      {g.ten}
                    </option>
                  ))}
                </Input>
              </Col>
            )}

            <Col xs={12} md={hasGroupStage ? 4 : 8}>
              <Label className="small fw-semibold">Tên trận đấu</Label>
              <Input
                type="text"
                bsSize="sm"
                value={matchForm.matchName}
                placeholder="Ví dụ: Trận 1 - Bảng A"
                onChange={(e) => setMatchForm({ ...matchForm, matchName: e.target.value })}
              />
            </Col>

            {/* Teams 1 & 2 */}
            <Col xs={12} md={6}>
              <Label className="small fw-semibold text-primary">Đội 1 (Vị trí 1 / Nhà) *</Label>
              <Input
                type="select"
                bsSize="sm"
                value={matchForm.doi1Id}
                onChange={(e) => setMatchForm({ ...matchForm, doi1Id: Number(e.target.value) })}
              >
                <option value="">-- Chọn Đội 1 --</option>
                {availableTeamsForMatch.map((t) => (
                  <option key={t.id} value={t.id}>
                    {t.tenDoi || t.tenDangKy} ({t.tenDonVi || 'Đơn vị'})
                  </option>
                ))}
              </Input>
            </Col>

            <Col xs={12} md={6}>
              <Label className="small fw-semibold text-danger">Đội 2 (Vị trí 2 / Khách) *</Label>
              <Input
                type="select"
                bsSize="sm"
                value={matchForm.doi2Id}
                onChange={(e) => setMatchForm({ ...matchForm, doi2Id: Number(e.target.value) })}
              >
                <option value="">-- Chọn Đội 2 --</option>
                {availableTeamsForMatch.map((t) => (
                  <option key={t.id} value={t.id}>
                    {t.tenDoi || t.tenDangKy} ({t.tenDonVi || 'Đơn vị'})
                  </option>
                ))}
              </Input>
            </Col>

            {/* Venue & Timing */}
            <Col xs={12} md={4}>
              <Label className="small fw-semibold">Sân thi đấu</Label>
              <Input
                type="select"
                bsSize="sm"
                value={matchForm.venueId}
                onChange={(e) => setMatchForm({ ...matchForm, venueId: e.target.value ? Number(e.target.value) : '' })}
              >
                <option value="">-- Chọn sân đấu --</option>
                {venues.map((v) => (
                  <option key={v.id} value={v.id}>
                    {v.ten} ({v.loaiSan || 'Sân'})
                  </option>
                ))}
              </Input>
            </Col>

            <Col xs={6} md={3}>
              <Label className="small fw-semibold">Ngày thi đấu</Label>
              <Input
                type="date"
                bsSize="sm"
                value={matchForm.matchDate}
                onChange={(e) => setMatchForm({ ...matchForm, matchDate: e.target.value })}
              />
            </Col>

            <Col xs={3} md={2.5}>
              <Label className="small fw-semibold">Giờ bắt đầu</Label>
              <Input
                type="time"
                bsSize="sm"
                value={matchForm.startTime}
                onChange={(e) => setMatchForm({ ...matchForm, startTime: e.target.value })}
              />
            </Col>

            <Col xs={3} md={2.5}>
              <Label className="small fw-semibold">Giờ kết thúc</Label>
              <Input
                type="time"
                bsSize="sm"
                value={matchForm.endTime}
                onChange={(e) => setMatchForm({ ...matchForm, endTime: e.target.value })}
              />
            </Col>

            {/* Referees Assignment Section */}
            <Col xs={12}>
              <div className="p-3 rounded-3 border bg-light">
                <div className="d-flex align-items-center justify-content-between mb-2">
                  <span className="small fw-bold text-dark d-flex align-items-center gap-1.5">
                    <Shield size={14} className="text-warning" />
                    Phân công Trọng tài điều hành:
                  </span>
                  <Button
                    color="outline-primary"
                    size="sm"
                    className="p-1 px-2 rounded-pill"
                    style={{ fontSize: '11px' }}
                    onClick={() => {
                      const firstAvailable = referees.find(
                        (r) => !matchForm.refereeAssignments.some((a) => a.trongTaiId === r.id)
                      );
                      if (firstAvailable) {
                        setMatchForm({
                          ...matchForm,
                          refereeAssignments: [
                            ...matchForm.refereeAssignments,
                            { trongTaiId: firstAvailable.id, vaiTro: 'TrongTaiPhu' },
                          ],
                        });
                      }
                    }}
                  >
                    + Thêm Trọng Tài
                  </Button>
                </div>

                {matchForm.refereeAssignments.length === 0 ? (
                  <p className="text-muted small mb-0 fst-italic" style={{ fontSize: '11px' }}>
                    Chưa phân công trọng tài nào. Nhấn "+ Thêm Trọng Tài" ở trên.
                  </p>
                ) : (
                  <div className="d-flex flex-column gap-2">
                    {matchForm.refereeAssignments.map((ra, idx) => (
                      <div key={idx} className="d-flex align-items-center gap-2">
                        <Input
                          type="select"
                          bsSize="sm"
                          value={ra.trongTaiId}
                          onChange={(e) => {
                            const newAssigns = [...matchForm.refereeAssignments];
                            newAssigns[idx].trongTaiId = Number(e.target.value);
                            setMatchForm({ ...matchForm, refereeAssignments: newAssigns });
                          }}
                          style={{ flex: 2 }}
                        >
                          {referees.map((r) => (
                            <option key={r.id} value={r.id}>
                              {r.hoTen} ({r.capBac || 'Trọng tài'})
                            </option>
                          ))}
                        </Input>

                        <Input
                          type="select"
                          bsSize="sm"
                          value={ra.vaiTro}
                          onChange={(e) => {
                            const newAssigns = [...matchForm.refereeAssignments];
                            newAssigns[idx].vaiTro = e.target.value;
                            setMatchForm({ ...matchForm, refereeAssignments: newAssigns });
                          }}
                          style={{ flex: 1.5 }}
                        >
                          <option value="TrongTaiChinh">Trọng tài chính</option>
                          <option value="TrongTaiPhu">Trọng tài phụ</option>
                          <option value="TrongTaiBan">Trọng tài bàn</option>
                          <option value="GiamSat">Giám sát</option>
                        </Input>

                        <Button
                          color="light"
                          size="sm"
                          className="p-1 text-danger rounded-2 border"
                          onClick={() => {
                            const newAssigns = matchForm.refereeAssignments.filter((_, i) => i !== idx);
                            setMatchForm({ ...matchForm, refereeAssignments: newAssigns });
                          }}
                        >
                          <Trash2 size={14} />
                        </Button>
                      </div>
                    ))}
                  </div>
                )}
              </div>
            </Col>

            {/* Conflict Check Trigger Button */}
            <Col xs={12} className="d-flex align-items-center justify-content-between pt-2">
              <Button
                color="outline-secondary"
                size="sm"
                className="rounded-pill px-3 py-1.5 d-flex align-items-center gap-1.5"
                style={{ fontSize: '12px' }}
                onClick={() => handleCheckConflict(true)}
                disabled={checkingConflict}
              >
                {checkingConflict ? <Spinner size="sm" /> : <AlertTriangle size={13} className="text-warning" />}
                <span>Kiểm tra trùng lịch (Sân / Trọng tài / VĐV thi đấu nhiều môn)</span>
              </Button>
            </Col>
          </Row>
        </ModalBody>
        <ModalFooter className="border-top">
          <Button color="secondary" size="sm" className="rounded-pill px-3" onClick={() => setManualMatchModalOpen(false)}>
            Hủy
          </Button>
          <Button color="primary" size="sm" className="rounded-pill px-4 fw-semibold" onClick={handleSaveManualMatch}>
            Lưu Trận Đấu
          </Button>
        </ModalFooter>
      </Modal>

      {/* ======================================================== */}
      {/* MODAL 3: AUTO DISTRIBUTE GROUPS (Chia Bảng / Bốc Thăm)   */}
      {/* ======================================================== */}
      <Modal isOpen={groupModalOpen} toggle={() => setGroupModalOpen(!groupModalOpen)} centered>
        <ModalHeader toggle={() => setGroupModalOpen(!groupModalOpen)} className="border-bottom">
          <div className="d-flex align-items-center gap-2">
            <Shuffle size={18} className="text-info" />
            <span className="fw-bold fs-6">Bốc Thăm & Chia Bảng Đấu</span>
          </div>
        </ModalHeader>
        <ModalBody className="p-4">
          <p className="small text-muted mb-3">
            Hệ thống sẽ tự động tạo các bảng đấu và chia đều ngẫu nhiên {registeredTeams.length} đội đã duyệt vào từng bảng đấu.
          </p>

          <FormGroup>
            <Label className="small fw-semibold">Số lượng bảng đấu cần chia:</Label>
            <Input
              type="select"
              bsSize="sm"
              value={distributeGroupCount}
              onChange={(e) => setDistributeGroupCount(Number(e.target.value))}
            >
              <option value={2}>2 bảng (Bảng A, B)</option>
              <option value={3}>3 bảng (Bảng A, B, C)</option>
              <option value={4}>4 bảng (Bảng A, B, C, D)</option>
              <option value={6}>6 bảng (Bảng A - F)</option>
              <option value={8}>8 bảng (Bảng A - H)</option>
            </Input>
          </FormGroup>
        </ModalBody>
        <ModalFooter className="border-top">
          <Button color="secondary" size="sm" className="rounded-pill px-3" onClick={() => setGroupModalOpen(false)}>
            Hủy
          </Button>
          <Button color="info" size="sm" className="rounded-pill px-4 fw-semibold text-white" onClick={handleAutoDistributeGroups}>
            Bắt Đầu Bốc Thăm
          </Button>
        </ModalFooter>
      </Modal>

      {/* ======================================================== */}
      {/* MODAL 4: TOURNAMENT CONFLICT AUDIT MODAL (Rà Soát Giải)  */}
      {/* ======================================================== */}
      <Modal isOpen={tournamentConflictModalOpen} toggle={() => setTournamentConflictModalOpen(!tournamentConflictModalOpen)} size="lg" centered>
        <ModalHeader toggle={() => setTournamentConflictModalOpen(!tournamentConflictModalOpen)} className="border-bottom">
          <div className="d-flex align-items-center gap-2">
            <ShieldAlert size={20} className="text-warning" />
            <span className="fw-bold fs-6">Rà Soát Toàn Bộ Xung Đột Lịch Thi Đấu Trong Giải</span>
          </div>
        </ModalHeader>
        <ModalBody className="p-4" style={{ maxHeight: '70vh', overflowY: 'auto' }}>
          {checkingTournamentConflict ? (
            <div className="text-center py-5">
              <Spinner color="primary" />
              <p className="small text-muted mt-3 mb-0">Đang quét toàn bộ các trận đấu, sân thi đấu và đối soát danh sách VĐV giữa các môn...</p>
            </div>
          ) : tournamentConflictReport ? (
            <div>
              {/* Summary stat cards */}
              <div className="row g-3 mb-4">
                <div className="col-6 col-md-4">
                  <div className="p-3 bg-light rounded-3 border text-center">
                    <span className="small text-muted d-block mb-1">Tổng số trận đã rà soát</span>
                    <span className="fw-bold fs-4 text-dark">{tournamentConflictReport.totalMatchesChecked}</span>
                  </div>
                </div>
                <div className="col-6 col-md-4">
                  <div className={`p-3 rounded-3 border text-center ${tournamentConflictReport.totalConflicts > 0 ? 'bg-danger-subtle border-danger' : 'bg-success-subtle border-success'}`}>
                    <span className="small text-muted d-block mb-1">Số xung đột phát hiện</span>
                    <span className={`fw-bold fs-4 ${tournamentConflictReport.totalConflicts > 0 ? 'text-danger' : 'text-success'}`}>
                      {tournamentConflictReport.totalConflicts}
                    </span>
                  </div>
                </div>
                <div className="col-12 col-md-4">
                  <div className="p-3 bg-light rounded-3 border text-center">
                    <span className="small text-muted d-block mb-1">Trạng thái tổng quan</span>
                    <Badge color={tournamentConflictReport.hasConflict ? 'danger' : 'success'} className="px-3 py-1.5 mt-1">
                      {tournamentConflictReport.hasConflict ? 'Cần xử lý trùng lịch' : 'Hoàn toàn hợp lệ'}
                    </Badge>
                  </div>
                </div>
              </div>

              {/* Status details */}
              {!tournamentConflictReport.hasConflict ? (
                <Alert color="success" className="d-flex align-items-center gap-3 p-3 rounded-3 mb-0">
                  <CheckCircle size={28} className="text-success flex-shrink-0" />
                  <div>
                    <div className="fw-bold fs-6">Lịch thi đấu hoàn hảo!</div>
                    <div className="small">
                      Không phát hiện bất kỳ sự trùng lặp nào về thời gian thi đấu của các Vận động viên (kể cả VĐV thi đấu nhiều môn), Sân thi đấu hoặc Trọng tài trong giải.
                    </div>
                  </div>
                </Alert>
              ) : (
                <div className="d-flex flex-column gap-3">
                  <div className="small fw-bold text-danger text-uppercase tracking-wider">
                    Danh sách các xung đột cần khắc phục ({tournamentConflictReport.chiTietXungDot?.length || tournamentConflictReport.conflicts.length}):
                  </div>

                  {tournamentConflictReport.chiTietXungDot && tournamentConflictReport.chiTietXungDot.length > 0 ? (
                    tournamentConflictReport.chiTietXungDot.map((item, idx) => (
                      <div key={idx} className="p-3 rounded-3 border border-danger-subtle bg-white shadow-xs">
                        <div className="d-flex flex-wrap align-items-center justify-content-between gap-2 mb-2 pb-2 border-bottom">
                          <div className="d-flex align-items-center gap-2">
                            {item.loaiXungDot === 'VanDongVien' ? (
                              <Badge color="danger" className="px-2 py-1 d-inline-flex align-items-center gap-1">
                                <UserX size={12} /> Trùng Lịch VĐV
                              </Badge>
                            ) : item.loaiXungDot === 'SanDau' ? (
                              <Badge color="warning" className="text-dark px-2 py-1 d-inline-flex align-items-center gap-1">
                                <MapPin size={12} /> Trùng Sân Đấu
                              </Badge>
                            ) : (
                              <Badge color="info" className="px-2 py-1 d-inline-flex align-items-center gap-1">
                                <Shield size={12} /> Trùng Trọng Tài
                              </Badge>
                            )}

                            {item.tenVanDongVien && (
                              <span className="fw-bold text-dark fs-6">
                                {item.tenVanDongVien}
                                {item.tenDoiHienTai ? <span className="text-muted fw-normal ms-1">({item.tenDoiHienTai})</span> : null}
                              </span>
                            )}
                          </div>

                          {item.thoiGianBatDau && item.thoiGianKetThuc && (
                            <span className="badge bg-light text-secondary border">
                              <Calendar size={11} className="me-1" />
                              {new Date(item.thoiGianBatDau).toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' })} - {new Date(item.thoiGianKetThuc).toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' })}
                            </span>
                          )}
                        </div>

                        <p className="small text-dark mb-0">
                          {item.thongBao}
                        </p>
                      </div>
                    ))
                  ) : (
                    <ul className="list-group list-group-flush">
                      {tournamentConflictReport.conflicts.map((c, idx) => (
                        <li key={idx} className="list-group-item list-group-item-danger small py-2">
                          {c}
                        </li>
                      ))}
                    </ul>
                  )}
                </div>
              )}
            </div>
          ) : (
            <p className="text-center text-muted py-4">Chưa có báo cáo rà soát. Nhấn nút bên dưới để tiến hành kiểm tra.</p>
          )}
        </ModalBody>
        <ModalFooter className="border-top">
          <Button color="secondary" size="sm" className="rounded-pill px-3" onClick={() => setTournamentConflictModalOpen(false)}>
            Đóng
          </Button>
          <Button
            color="warning"
            size="sm"
            className="rounded-pill px-4 fw-semibold text-dark d-flex align-items-center gap-1.5"
            onClick={handleCheckTournamentConflicts}
            disabled={checkingTournamentConflict}
          >
            {checkingTournamentConflict ? <Spinner size="sm" /> : <RefreshCw size={14} />}
            <span>Quét Lại Xung Đột</span>
          </Button>
        </ModalFooter>
      </Modal>

      {/* MODAL 5: TEAM DETAIL & SCHEDULE MODAL */}
      <Modal isOpen={teamDetailModalOpen} toggle={() => setTeamDetailModalOpen(!teamDetailModalOpen)} size="lg" centered>
        <ModalHeader toggle={() => setTeamDetailModalOpen(!teamDetailModalOpen)} className="border-bottom">
          <div className="d-flex align-items-center gap-2">
            <Users size={18} className="text-primary" />
            <span className="fw-bold fs-6">
              Chi Tiết Đội Đăng Ký: {selectedTeamDetail?.tenDoi || selectedTeamDetail?.tenDangKy || ''}
            </span>
          </div>
        </ModalHeader>
        <ModalBody className="p-4">
          {selectedTeamDetail && (
            <div className="d-flex flex-column gap-4">
              {/* Team Overview Card */}
              <div className="p-3 rounded-3 bg-light border">
                <Row className="g-3">
                  <Col xs={12} sm={6} md={3}>
                    <small className="text-muted d-block mb-1">Mã đăng ký</small>
                    <span className="fw-bold font-monospace text-dark">
                      {selectedTeamDetail.soDangKy || `DK-${selectedTeamDetail.id}`}
                    </span>
                  </Col>
                  <Col xs={12} sm={6} md={3}>
                    <small className="text-muted d-block mb-1">Đơn vị chủ quản</small>
                    <span className="fw-semibold text-dark">{selectedTeamDetail.tenDonVi || '--'}</span>
                  </Col>
                  <Col xs={12} sm={6} md={3}>
                    <small className="text-muted d-block mb-1">Môn thi đấu</small>
                    <Badge color="primary" pill className="px-2 py-0.5">
                      {selectedTeamDetail.tenMonTheThao || 'Môn thi'}
                    </Badge>
                  </Col>
                  <Col xs={12} sm={6} md={3}>
                    <small className="text-muted d-block mb-1">Trạng thái hồ sơ</small>
                    <Badge
                      color={selectedTeamDetail.trangThai === 'DaDuyet' ? 'success' : 'warning'}
                      pill
                      className="px-2 py-0.5"
                    >
                      {selectedTeamDetail.trangThai === 'DaDuyet' ? 'Đã duyệt' : 'Chờ duyệt'}
                    </Badge>
                  </Col>
                  <Col xs={12} sm={6} md={6}>
                    <small className="text-muted d-block mb-1">Nội dung thi đấu</small>
                    <span className="fw-bold text-dark">{selectedTeamDetail.tenNoiDung || '--'}</span>
                  </Col>
                  <Col xs={12} sm={6} md={6}>
                    <small className="text-muted d-block mb-1">Ngày đăng ký</small>
                    <span className="text-dark">
                      {selectedTeamDetail.ngayDangKy
                        ? new Date(selectedTeamDetail.ngayDangKy).toLocaleDateString('vi-VN')
                        : '--'}
                    </span>
                  </Col>
                </Row>
              </div>

              {/* Athletes List */}
              <div>
                <h6 className="fw-bold text-dark mb-2 d-flex align-items-center gap-1.5">
                  <UserCheck size={16} className="text-success" />
                  <span>Danh Sách Vận Động Viên ({selectedTeamDetail.vanDongVienNames?.length || 0} VĐV)</span>
                </h6>
                {selectedTeamDetail.vanDongVienNames && selectedTeamDetail.vanDongVienNames.length > 0 ? (
                  <div className="d-flex flex-wrap gap-2">
                    {selectedTeamDetail.vanDongVienNames.map((name, i) => (
                      <div
                        key={i}
                        className="p-2.5 rounded-3 bg-white border d-flex align-items-center gap-2 shadow-sm"
                        style={{ minWidth: '180px' }}
                      >
                        <div
                          className="rounded-circle d-flex align-items-center justify-content-center flex-shrink-0"
                          style={{ width: '32px', height: '32px', backgroundColor: '#e0e7ff', color: '#4338ca', fontWeight: 'bold', fontSize: '12px' }}
                        >
                          {name.charAt(0)}
                        </div>
                        <div>
                          <div className="fw-bold text-dark small">{name}</div>
                          <div className="text-muted" style={{ fontSize: '11px' }}>Vận động viên thi đấu</div>
                        </div>
                      </div>
                    ))}
                  </div>
                ) : (
                  <p className="text-muted small fst-italic">Chưa có thông tin VĐV cụ thể cho đội này.</p>
                )}
              </div>

              {/* Scheduled Matches for this team */}
              <div>
                <h6 className="fw-bold text-dark mb-2 d-flex align-items-center gap-1.5">
                  <Calendar size={16} className="text-primary" />
                  <span>Lịch Thi Đấu Đã Bố Trí ({getTeamMatchesList(selectedTeamDetail.id, 'tournament').length} trận)</span>
                </h6>
                {getTeamMatchesList(selectedTeamDetail.id, 'tournament').length === 0 ? (
                  <div className="p-4 rounded-3 bg-light text-center border">
                    <Calendar size={32} className="text-muted opacity-50 mb-1" />
                    <p className="mb-0 text-muted small">Đội này chưa có trận đấu nào được xếp lịch.</p>
                  </div>
                ) : (
                  <div className="table-responsive">
                    <Table size="sm" className="align-middle border rounded-3 mb-0" style={{ fontSize: '12px' }}>
                      <thead className="table-light">
                        <tr>
                          <th>Trận / Vòng</th>
                          <th>Cặp Đấu</th>
                          <th>Sân Đấu</th>
                          <th>Thời Gian</th>
                          <th className="text-center">Trạng Thái</th>
                        </tr>
                      </thead>
                      <tbody>
                        {getTeamMatchesList(selectedTeamDetail.id, 'tournament').map((m) => {
                          const isTeam1 = m.doi1DangKyId === selectedTeamDetail.id;
                          const opponentName = isTeam1 ? (m.tenDoi2 || 'Chờ xác định') : (m.tenDoi1 || 'Chờ xác định');
                          const matchDateStr = m.thoiGianBatDau ? new Date(m.thoiGianBatDau).toLocaleDateString('vi-VN') : '--';
                          const matchTimeStr = m.thoiGianBatDau ? new Date(m.thoiGianBatDau).toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' }) : '--';

                          return (
                            <tr key={m.id}>
                              <td>
                                <span className="fw-bold text-dark">{m.tenTran || `Trận #${m.soTran}`}</span>
                                <span className="text-muted d-block" style={{ fontSize: '11px' }}>{m.tenVongDau || '--'}</span>
                              </td>
                              <td>
                                <span className="fw-semibold text-primary">{selectedTeamDetail.tenDoi || selectedTeamDetail.tenDangKy}</span>
                                <span className="text-muted px-1.5">vs</span>
                                <span className="fw-semibold text-dark">{opponentName}</span>
                              </td>
                              <td>{m.tenSanDau || 'Chưa xếp sân'}</td>
                              <td>
                                <span>{matchDateStr}</span>
                                <span className="text-muted d-block" style={{ fontSize: '11px' }}>{matchTimeStr}</span>
                              </td>
                              <td className="text-center">
                                <Badge color={m.trangThai === 'DaKetThuc' ? 'secondary' : m.trangThai === 'DangDienRa' ? 'danger' : 'primary'} pill>
                                  {m.trangThai === 'DaKetThuc' ? 'Kết thúc' : m.trangThai === 'DangDienRa' ? 'Đang đấu' : 'Chưa đấu'}
                                </Badge>
                              </td>
                            </tr>
                          );
                        })}
                      </tbody>
                    </Table>
                  </div>
                )}
              </div>
            </div>
          )}
        </ModalBody>
        <ModalFooter className="border-top">
          <Button color="secondary" size="sm" className="rounded-pill px-3" onClick={() => setTeamDetailModalOpen(false)}>
            Đóng
          </Button>
          {selectedTeamDetail && (
            <Button
              color="primary"
              size="sm"
              className="rounded-pill px-3 fw-semibold d-flex align-items-center gap-1.5"
              onClick={() => {
                setTeamDetailModalOpen(false);
                handleQuickScheduleMatchForTeam(selectedTeamDetail);
              }}
            >
              <Plus size={14} />
              <span>Thêm Trận Đấu Nhanh</span>
            </Button>
          )}
        </ModalFooter>
      </Modal>
    </div>
  );
}
