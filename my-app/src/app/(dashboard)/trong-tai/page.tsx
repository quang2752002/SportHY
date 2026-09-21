'use client';

import React, { useState, useEffect, useMemo, useCallback } from 'react';
import Link from 'next/link';
import { useRouter, useSearchParams } from 'next/navigation';
import { useAuth, useToast } from '@/context/AuthContext';
import {
  Users,
  UserCheck,
  ShieldCheck,
  CheckCircle2,
  Clock,
  Calendar,
  Search,
  Filter,
  Trophy,
  Play,
  Pause,
  RotateCcw,
  FileText,
  MapPin,
  Flame,
  AlertCircle,
  ChevronRight,
  ExternalLink,
  SlidersHorizontal,
  LayoutGrid,
  Table as TableIcon,
  Plus,
  Edit,
  Edit2,
  Phone,
  Award,
  Sparkles,
  Lock,
  Unlock,
  Save,
  Trash2,
  CloudSun,
  Flag,
  Activity,
  Check,
  AlertTriangle,
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
import { sanDauService } from '@/services/sanDauService';
import { monTheThaoService } from '@/services/monTheThaoService';
import { GiaiDau, TrangThaiGiaiDau } from '@/types/giaiDau';

import {
  TranDau,
  PhanCongTrongTaiItem,
  AssignTrongTai,
  MatchScoreDetails,
  MatchEvent,
  SetScore,
} from '@/types/tranDau';
import { TrongTai } from '@/types/trongTai';
import { SanDau } from '@/types/sanDau';
import { MonTheThao } from '@/types/monTheThao';
import { refereeScoreStorage } from '@/utils/refereeScoreStorage';

export default function RefereeDashboardPage() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const { user } = useAuth();
  const toast = useToast();

  // Danh mục dữ liệu
  const [giaiDaus, setGiaiDaus] = useState<GiaiDau[]>([]);
  const [selectedGiaiDauId, setSelectedGiaiDauId] = useState<number | 'ALL'>('ALL');
  const [matches, setMatches] = useState<TranDau[]>([]);
  const [allReferees, setAllReferees] = useState<TrongTai[]>([]);
  const [venues, setVenues] = useState<SanDau[]>([]);
  const [sports, setSports] = useState<MonTheThao[]>([]);
  const [loading, setLoading] = useState(true);
  const [loadingMatches, setLoadingMatches] = useState(false);

  // Tab chính: 'my_matches' (Trận tôi được phân công) | 'all_matches' (Tất cả trận đấu)
  const [mainTab, setMainTab] = useState<'my_matches' | 'all_matches'>('my_matches');

  // Trọng tài đang thao tác (mặc định lấy theo tài khoản đăng nhập hoặc cho phép chuyển đổi)
  const [activeRefereeId, setActiveRefereeId] = useState<number | ''>('');

  // Bộ lọc
  const [selectedSportId, setSelectedSportId] = useState<string>('ALL');
  const [selectedVenueId, setSelectedVenueId] = useState<string>('ALL');
  const [selectedStatus, setSelectedStatus] = useState<string>('ALL');
  const [selectedRoleFilter, setSelectedRoleFilter] = useState<string>('ALL'); // Lọc theo vai trò: TT Chính, Trợ lý, TT Bàn
  const [filterDate, setFilterDate] = useState<string>('');
  const [searchKeyword, setSearchKeyword] = useState<string>('');
  const [viewMode, setViewMode] = useState<'grid' | 'table'>('grid');

  // Modal phân công trọng tài cho 1 trận
  const [assignModalOpen, setAssignModalOpen] = useState(false);
  const [targetMatch, setTargetMatch] = useState<TranDau | null>(null);
  const [mainRefId, setMainRefId] = useState<number | ''>('');
  const [assistant1Id, setAssistant1Id] = useState<number | ''>('');
  const [assistant2Id, setAssistant2Id] = useState<number | ''>('');
  const [tableRefId, setTableRefId] = useState<number | ''>('');
  const [submittingAssign, setSubmittingAssign] = useState(false);

  // Modal Cập nhật nhanh kết quả & diễn biến
  const [quickScoreModalOpen, setQuickScoreModalOpen] = useState(false);
  const [quickScoreMatch, setQuickScoreMatch] = useState<TranDau | null>(null);
  const [qScore1, setQScore1] = useState<number>(0);
  const [qScore2, setQScore2] = useState<number>(0);
  const [qSetScores, setQSetScores] = useState<SetScore[]>([]);
  const [qWinner, setQWinner] = useState<1 | 2 | 'draw' | undefined>(undefined);
  const [qStatus, setQStatus] = useState<string>('ChuaDau');
  const [qEvents, setQEvents] = useState<MatchEvent[]>([]);
  const [qNotes, setQNotes] = useState<string>('');
  const [savingQuickScore, setSavingQuickScore] = useState(false);

  // Modal thêm sự kiện nhanh
  const [quickEventModalOpen, setQuickEventModalOpen] = useState(false);
  const [qEventType, setQEventType] = useState<MatchEvent['type']>('goal');
  const [qEventTeam, setQEventTeam] = useState<1 | 2>(1);
  const [qEventMinute, setQEventMinute] = useState<number>(1);
  const [qEventAthlete, setQEventAthlete] = useState<string>('');
  const [qEventAssist, setQEventAssist] = useState<string>('');
  const [qEventDetails, setQEventDetails] = useState<string>('');

  // Modal Hoàn thiện thông tin trận đấu (Finalize Match Modal)
  const [finalizeModalOpen, setFinalizeModalOpen] = useState(false);
  const [finalizeMatch, setFinalizeMatch] = useState<TranDau | null>(null);
  const [fActualStartTime, setFActualStartTime] = useState<string>('');
  const [fActualEndTime, setFActualEndTime] = useState<string>('');
  const [fDurationMinutes, setFDurationMinutes] = useState<number>(90);
  const [fExtraTimeMinutes, setFExtraTimeMinutes] = useState<number>(0);
  const [fWeatherCondition, setFWeatherCondition] = useState<string>('Nắng ráo, điều kiện thi đấu tốt');
  const [fPitchCondition, setFPitchCondition] = useState<string>('Mặt sân đạt chuẩn, an toàn');
  const [fSpectatorCount, setFSpectatorCount] = useState<number>(0);
  const [fMvpAthlete, setFMvpAthlete] = useState<string>('');
  const [fWinMethod, setFWinMethod] = useState<MatchScoreDetails['winMethod']>('normal');
  const [fRefereeNotes, setFRefereeNotes] = useState<string>('');
  const [fSupervisorNotes, setFSupervisorNotes] = useState<string>('');
  const [fIsFinalized, setFIsFinalized] = useState<boolean>(false);
  const [fFinalizedAt, setFFinalizedAt] = useState<string>('');
  const [fFinalizedBy, setFFinalizedBy] = useState<string>('');
  const [savingFinalize, setSavingFinalize] = useState(false);

  // 1. Tải danh mục khởi tạo
  useEffect(() => {
    const initData = async () => {
      setLoading(true);
      try {
        const [gdRes, ttRes, sdRes, sportRes] = await Promise.allSettled([
          giaiDauService.getAll(),
          trongTaiService.getAll(),
          sanDauService.getAll(),
          monTheThaoService.getAll(),
        ]);

        let list: GiaiDau[] = [];
        if (gdRes.status === 'fulfilled' && gdRes.value && gdRes.value.length > 0) {
          list = gdRes.value;
        } else {
          try {
            const paged = await giaiDauService.getPaged({ pageSize: 100 });
            if (paged?.items) list = paged.items;
          } catch {}
        }
        setGiaiDaus(list);

        const paramGiai = searchParams.get('giaiDauId');
        if (paramGiai && (paramGiai === 'ALL' || list.some((g) => g.id === Number(paramGiai)))) {
          setSelectedGiaiDauId(paramGiai === 'ALL' ? 'ALL' : Number(paramGiai));
        } else {
          // Mặc định chọn ALL để trọng tài thấy tất cả các trận phân công của mình
          setSelectedGiaiDauId('ALL');
        }

        let refs: TrongTai[] = [];
        if (ttRes.status === 'fulfilled' && ttRes.value) {
          refs = ttRes.value;
          setAllReferees(refs);
        }

        // Nhận diện ban đầu trọng tài
        if (user?.trongTaiId) {
          setActiveRefereeId(user.trongTaiId);
        } else if (user) {
          const foundRef = refs.find(
            (r) =>
              r.hoTen?.toLowerCase() === user.fullName?.toLowerCase() ||
              r.ma?.toLowerCase() === user.username?.toLowerCase()
          );
          if (foundRef) setActiveRefereeId(foundRef.id);
        }

        if (sdRes.status === 'fulfilled' && sdRes.value) {
          setVenues(sdRes.value);
        }
        if (sportRes.status === 'fulfilled' && sportRes.value) {
          setSports(sportRes.value);
        }
      } catch (err) {
        console.error('Lỗi khi tải dữ liệu khởi tạo', err);
        toast.error('Không thể tải danh sách dữ liệu');
      } finally {
        setLoading(false);
      }
    };

    initData();
  }, [searchParams, user, toast]);

  // 2. Tải danh sách trận đấu khi đổi giải đấu
  const loadMatches = useCallback(async () => {
    setLoadingMatches(true);
    try {
      const params: any = {};
      if (selectedGiaiDauId && selectedGiaiDauId !== 'ALL') {
        params.giaiDauId = Number(selectedGiaiDauId);
      }
      const data = (await tranDauService.getAll(params)) || [];
      setMatches(data);

      // Tự động bổ sung giải đấu từ danh sách trận nếu chưa có
      setGiaiDaus((prev) => {
        const map = new Map<number, GiaiDau>();
        prev.forEach((g) => map.set(g.id, g));
        data.forEach((m) => {
          if (m.giaiDauId && !map.has(m.giaiDauId)) {
            map.set(m.giaiDauId, {
              id: m.giaiDauId,
              ten: m.tenGiaiDau || `Giải đấu #${m.giaiDauId}`,
              trangThai: TrangThaiGiaiDau.DangDienRa,
            } as any);
          }
        });
        return Array.from(map.values());
      });
    } catch (err: any) {
      console.error('Lỗi tải trận đấu', err);
      toast.error('Không thể tải lịch thi đấu');
    } finally {
      setLoadingMatches(false);
    }
  }, [selectedGiaiDauId, toast]);

  useEffect(() => {
    loadMatches();
  }, [loadMatches]);

  // Nhận diện Trọng tài hiện tại (theo activeRefereeId hoặc user)
  const currentReferee = useMemo(() => {
    const targetId = activeRefereeId ? Number(activeRefereeId) : user?.trongTaiId;
    if (targetId) {
      const found = allReferees.find((r) => r.id === targetId);
      if (found) return found;
      return { id: targetId, hoTen: user?.fullName || user?.username || `Trọng tài #${targetId}` } as TrongTai;
    }
    if (user) {
      const found = allReferees.find(
        (r) =>
          r.hoTen?.toLowerCase() === user.fullName?.toLowerCase() ||
          r.ma?.toLowerCase() === user.username?.toLowerCase()
      );
      if (found) return found;
    }
    return null;
  }, [activeRefereeId, user, allReferees]);

  // Kiểm tra xem trận đấu có trọng tài hiện tại tham gia hay không
  const isMatchAssignedToMe = useCallback(
    (m: TranDau) => {
      const targetId = activeRefereeId ? Number(activeRefereeId) : user?.trongTaiId;
      const refList = m.danhSachTrongTai || [];
      if (targetId && refList.some((r) => r.trongTaiId === targetId)) {
        return true;
      }
      if (currentReferee && refList.some((r) => r.trongTaiId === currentReferee.id)) {
        return true;
      }
      if (user?.fullName) {
        const uName = user.fullName.trim().toLowerCase();
        if (refList.some((r) => r.tenTrongTai && r.tenTrongTai.trim().toLowerCase() === uName)) {
          return true;
        }
      }
      return false;
    },
    [activeRefereeId, user, currentReferee]
  );

  // Lấy vai trò của trọng tài hiện tại trong trận đấu
  const getRefereeRoleInMatch = useCallback(
    (m: TranDau) => {
      const targetId = activeRefereeId ? Number(activeRefereeId) : user?.trongTaiId;
      const refList = m.danhSachTrongTai || [];
      let item = targetId ? refList.find((r) => r.trongTaiId === targetId) : null;
      if (!item && currentReferee) {
        item = refList.find((r) => r.trongTaiId === currentReferee.id);
      }
      if (!item && user?.fullName) {
        const uName = user.fullName.trim().toLowerCase();
        item = refList.find((r) => r.tenTrongTai && r.tenTrongTai.trim().toLowerCase() === uName);
      }
      return item?.vaiTro || null;
    },
    [activeRefereeId, user, currentReferee]
  );

  // Chuẩn hóa tên vai trò
  const normalizeRole = (roleStr?: string | null) => {
    if (!roleStr) return '';
    const r = roleStr.toLowerCase().replace(/[^a-z0-9]/g, '');
    if (r.includes('chinh')) return 'TrongTaiChinh';
    if (r.includes('troly1') || r.includes('phu1')) return 'TroLy1';
    if (r.includes('troly2') || r.includes('phu2')) return 'TroLy2';
    if (r.includes('ban') || r.includes('thuky')) return 'TrongTaiBan';
    return roleStr;
  };

  // Lọc danh sách trận đấu
  const filteredMatches = useMemo(() => {
    return matches.filter((m) => {
      // 1. Tab Lọc "Trận của tôi" vs "Tất cả trận"
      if (mainTab === 'my_matches') {
        if (!isMatchAssignedToMe(m)) return false;
      }

      // 2. Lọc theo vai trò của tôi trong trận
      if (selectedRoleFilter !== 'ALL') {
        const myRole = getRefereeRoleInMatch(m);
        if (normalizeRole(myRole) !== normalizeRole(selectedRoleFilter)) return false;
      }

      // 3. Lọc theo môn
      if (selectedSportId !== 'ALL') {
        const sId = Number(selectedSportId);
        if (m.monTheThaoId !== sId && m.giaiDauMonTheThaoId !== sId) return false;
      }

      // 4. Lọc theo sân
      if (selectedVenueId !== 'ALL') {
        if (m.sanDauId !== Number(selectedVenueId)) return false;
      }

      // 5. Lọc theo trạng thái
      if (selectedStatus !== 'ALL') {
        if (m.trangThai !== selectedStatus) return false;
      }

      // 6. Lọc theo ngày
      if (filterDate) {
        const matchDate = m.thoiGianDuKien || m.thoiGianBatDau;
        if (!matchDate || !matchDate.startsWith(filterDate)) return false;
      }

      // 7. Tìm kiếm từ khóa
      if (searchKeyword.trim()) {
        const q = searchKeyword.toLowerCase();
        const t1 = (m.tenDoi1 || '').toLowerCase();
        const t2 = (m.tenDoi2 || '').toLowerCase();
        const name = (m.tenTran || '').toLowerCase();
        const sport = (m.tenMonTheThao || '').toLowerCase();
        const venue = (m.tenSanDau || '').toLowerCase();
        const refs = (m.danhSachTrongTai || [])
          .map((r) => (r.tenTrongTai || '').toLowerCase())
          .join(' ');

        if (
          !t1.includes(q) &&
          !t2.includes(q) &&
          !name.includes(q) &&
          !sport.includes(q) &&
          !venue.includes(q) &&
          !refs.includes(q)
        ) {
          return false;
        }
      }

      return true;
    });
  }, [
    matches,
    mainTab,
    isMatchAssignedToMe,
    selectedRoleFilter,
    getRefereeRoleInMatch,
    selectedSportId,
    selectedVenueId,
    selectedStatus,
    filterDate,
    searchKeyword,
  ]);

  // Thống kê nhanh
  const stats = useMemo(() => {
    const total = matches.length;
    const myTotal = matches.filter(isMatchAssignedToMe).length;
    const live = matches.filter((m) => m.trangThai === 'DangDienRa').length;
    const today = new Date().toISOString().slice(0, 10);
    const scheduledToday = matches.filter(
      (m) => (m.thoiGianDuKien || m.thoiGianBatDau || '').startsWith(today)
    ).length;
    const finished = matches.filter((m) => m.trangThai === 'DaKetThuc').length;
    const finalized = matches.filter((m) => {
      const d = refereeScoreStorage.getScoreDetails(m);
      return !!d.isFinalized;
    }).length;

    return { total, myTotal, live, scheduledToday, finished, finalized };
  }, [matches, isMatchAssignedToMe]);

  // Bắt đầu trận đấu (Chuyển sang DangDienRa)
  const handleStartMatch = async (m: TranDau) => {
    try {
      const nowIso = new Date().toISOString();
      const details = refereeScoreStorage.getScoreDetails(m);
      details.actualStartTime = details.actualStartTime || nowIso;
      refereeScoreStorage.saveScoreDetails(m.id, details);
      const serializedJson = refereeScoreStorage.serializeDetails(details);

      await tranDauService.update(m.id, {
        giaiDauMonTheThaoId: m.giaiDauMonTheThaoId,
        vongDauId: m.vongDauId,
        bangDauId: m.bangDauId,
        sanDauId: m.sanDauId,
        soTran: m.soTran,
        tenTran: m.tenTran,
        thoiGianDuKien: m.thoiGianDuKien,
        thoiGianBatDau: nowIso,
        trangThai: 'DangDienRa',
        ghiChu: serializedJson,
        doi1DangKyId: m.doi1DangKyId,
        doi2DangKyId: m.doi2DangKyId,
        danhSachTrongTai: (m.danhSachTrongTai || []).map((r) => ({
          trongTaiId: r.trongTaiId,
          vaiTro: r.vaiTro,
          ghiChu: r.ghiChu,
        })),
      });

      toast.success(`Đã bắt đầu trận: ${m.tenTran || `Trận ${m.soTran}`}!`);
      loadMatches();
    } catch (err: any) {
      console.error(err);
      toast.error('Không thể cập nhật trạng thái trận đấu');
    }
  };

  // Mở modal Cập nhật nhanh kết quả & diễn biến
  const handleOpenQuickScore = (m: TranDau) => {
    setQuickScoreMatch(m);
    const details = refereeScoreStorage.getScoreDetails(m);
    setQScore1(details.score1);
    setQScore2(details.score2);
    setQSetScores(
      details.setScores && details.setScores.length > 0
        ? details.setScores
        : [{ setNumber: 1, score1: details.score1, score2: details.score2 }]
    );
    setQWinner(details.winner);
    setQStatus(m.trangThai || 'ChuaDau');
    setQEvents(details.events || []);
    setQNotes(details.notes || '');
    setQuickScoreModalOpen(true);
  };

  // Lưu nhanh kết quả
  const handleSaveQuickScore = async (finishMatch: boolean = false) => {
    if (!quickScoreMatch) return;
    setSavingQuickScore(true);
    try {
      const nextStatus = finishMatch ? 'DaKetThuc' : qStatus;
      const calcWinner = qScore1 > qScore2 ? 1 : qScore2 > qScore1 ? 2 : 'draw';
      const effectiveWinner = qWinner !== undefined ? qWinner : calcWinner;
      const nowIso = new Date().toISOString();

      const existingStored = refereeScoreStorage.getScoreDetails(quickScoreMatch);

      const scoreDetails: MatchScoreDetails = {
        ...existingStored,
        score1: qScore1,
        score2: qScore2,
        winner: effectiveWinner,
        setScores: qSetScores,
        events: qEvents,
        notes: qNotes,
        actualStartTime:
          existingStored.actualStartTime ||
          quickScoreMatch.thoiGianBatDau ||
          (nextStatus === 'DangDienRa' ? nowIso : undefined),
        actualEndTime: finishMatch
          ? existingStored.actualEndTime || nowIso
          : existingStored.actualEndTime,
      };

      refereeScoreStorage.saveScoreDetails(quickScoreMatch.id, scoreDetails);
      const serializedJson = refereeScoreStorage.serializeDetails(scoreDetails);

      await tranDauService.update(quickScoreMatch.id, {
        giaiDauMonTheThaoId: quickScoreMatch.giaiDauMonTheThaoId,
        vongDauId: quickScoreMatch.vongDauId,
        bangDauId: quickScoreMatch.bangDauId,
        sanDauId: quickScoreMatch.sanDauId,
        soTran: quickScoreMatch.soTran,
        tenTran: quickScoreMatch.tenTran,
        thoiGianDuKien: quickScoreMatch.thoiGianDuKien,
        thoiGianBatDau: scoreDetails.actualStartTime || quickScoreMatch.thoiGianBatDau,
        thoiGianKetThuc: scoreDetails.actualEndTime || quickScoreMatch.thoiGianKetThuc,
        trangThai: nextStatus,
        ghiChu: serializedJson,
        doi1DangKyId: quickScoreMatch.doi1DangKyId,
        doi2DangKyId: quickScoreMatch.doi2DangKyId,
        danhSachTrongTai: (quickScoreMatch.danhSachTrongTai || []).map((r) => ({
          trongTaiId: r.trongTaiId,
          vaiTro: r.vaiTro,
          ghiChu: r.ghiChu,
        })),
      });

      toast.success(
        finishMatch
          ? 'Đã kết thúc trận và cập nhật kết quả thành công!'
          : 'Đã cập nhật tỷ số & diễn biến thành công!'
      );
      setQuickScoreModalOpen(false);
      loadMatches();
    } catch (err: any) {
      console.error(err);
      toast.error('Lỗi khi lưu kết quả trận đấu');
    } finally {
      setSavingQuickScore(false);
    }
  };

  // Mở modal Hoàn thiện thông tin trận đấu
  const handleOpenFinalize = (m: TranDau) => {
    setFinalizeMatch(m);
    const details = refereeScoreStorage.getScoreDetails(m);
    setFActualStartTime(details.actualStartTime || m.thoiGianBatDau || '');
    setFActualEndTime(details.actualEndTime || m.thoiGianKetThuc || '');
    setFDurationMinutes(details.durationMinutes || 90);
    setFExtraTimeMinutes(details.extraTimeMinutes || 0);
    setFWeatherCondition(details.weatherCondition || 'Nắng ráo, điều kiện thi đấu tốt');
    setFPitchCondition(details.pitchCondition || 'Mặt sân đạt chuẩn, an toàn');
    setFSpectatorCount(details.spectatorCount || 0);
    setFMvpAthlete(details.mvpAthlete || '');
    setFWinMethod(details.winMethod || 'normal');
    setFRefereeNotes(details.refereeNotes || '');
    setFSupervisorNotes(details.supervisorNotes || '');
    setFIsFinalized(!!details.isFinalized);
    setFFinalizedAt(details.finalizedAt || '');
    setFFinalizedBy(details.finalizedBy || '');
    setFinalizeModalOpen(true);
  };

  // Lưu hoàn thiện thông tin trận đấu
  const handleSaveFinalize = async (markFinalized: boolean = false) => {
    if (!finalizeMatch) return;
    setSavingFinalize(true);
    try {
      const nowIso = new Date().toISOString();
      const existingStored = refereeScoreStorage.getScoreDetails(finalizeMatch);

      const scoreDetails: MatchScoreDetails = {
        ...existingStored,
        actualStartTime: fActualStartTime || finalizeMatch.thoiGianBatDau || nowIso,
        actualEndTime:
          fActualEndTime || (markFinalized ? nowIso : finalizeMatch.thoiGianKetThuc),
        durationMinutes: Number(fDurationMinutes) || 90,
        extraTimeMinutes: Number(fExtraTimeMinutes) || 0,
        weatherCondition: fWeatherCondition,
        pitchCondition: fPitchCondition,
        spectatorCount: Number(fSpectatorCount) || 0,
        mvpAthlete: fMvpAthlete,
        winMethod: fWinMethod,
        refereeNotes: fRefereeNotes,
        supervisorNotes: fSupervisorNotes,
        isFinalized: markFinalized ? true : fIsFinalized,
        finalizedAt: markFinalized ? nowIso : fFinalizedAt,
        finalizedBy: markFinalized
          ? user?.fullName || user?.username || 'Trọng tài'
          : fFinalizedBy,
      };

      refereeScoreStorage.saveScoreDetails(finalizeMatch.id, scoreDetails);
      const serializedJson = refereeScoreStorage.serializeDetails(scoreDetails);

      const nextStatus = markFinalized ? 'DaKetThuc' : finalizeMatch.trangThai;

      await tranDauService.update(finalizeMatch.id, {
        giaiDauMonTheThaoId: finalizeMatch.giaiDauMonTheThaoId,
        vongDauId: finalizeMatch.vongDauId,
        bangDauId: finalizeMatch.bangDauId,
        sanDauId: finalizeMatch.sanDauId,
        soTran: finalizeMatch.soTran,
        tenTran: finalizeMatch.tenTran,
        thoiGianDuKien: finalizeMatch.thoiGianDuKien,
        thoiGianBatDau: scoreDetails.actualStartTime,
        thoiGianKetThuc: scoreDetails.actualEndTime,
        trangThai: nextStatus,
        ghiChu: serializedJson,
        doi1DangKyId: finalizeMatch.doi1DangKyId,
        doi2DangKyId: finalizeMatch.doi2DangKyId,
        danhSachTrongTai: (finalizeMatch.danhSachTrongTai || []).map((r) => ({
          trongTaiId: r.trongTaiId,
          vaiTro: r.vaiTro,
          ghiChu: r.ghiChu,
        })),
      });

      toast.success(
        markFinalized
          ? 'Đã hoàn thiện hồ sơ và chốt kết quả trận đấu thành công!'
          : 'Đã lưu thông tin hoàn thiện trận đấu!'
      );
      setFinalizeModalOpen(false);
      loadMatches();
    } catch (err: any) {
      console.error(err);
      toast.error('Lỗi khi lưu thông tin hoàn thiện trận đấu');
    } finally {
      setSavingFinalize(false);
    }
  };

  // Mở modal phân công trọng tài
  const handleOpenAssignModal = (m: TranDau) => {
    setTargetMatch(m);
    const list = m.danhSachTrongTai || [];
    const main = list.find((x) => x.vaiTro === 'TrongTaiChinh') || list[0];
    const as1 = list.find((x) => x.vaiTro === 'TroLy1' || x.vaiTro === 'TrongTaiPhu') || list[1];
    const as2 = list.find((x) => x.vaiTro === 'TroLy2') || list[2];
    const tb = list.find((x) => x.vaiTro === 'TrongTaiBan' || x.vaiTro === 'ThuKy') || list[3];

    setMainRefId(main ? main.trongTaiId : '');
    setAssistant1Id(as1 ? as1.trongTaiId : '');
    setAssistant2Id(as2 ? as2.trongTaiId : '');
    setTableRefId(tb ? tb.trongTaiId : '');
    setAssignModalOpen(true);
  };

  // Lưu phân công trọng tài
  const handleSaveAssign = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!targetMatch) return;

    const assignList: AssignTrongTai[] = [];
    if (mainRefId) {
      assignList.push({ trongTaiId: Number(mainRefId), vaiTro: 'TrongTaiChinh' });
    }
    if (assistant1Id) {
      assignList.push({ trongTaiId: Number(assistant1Id), vaiTro: 'TroLy1' });
    }
    if (assistant2Id) {
      assignList.push({ trongTaiId: Number(assistant2Id), vaiTro: 'TroLy2' });
    }
    if (tableRefId) {
      assignList.push({ trongTaiId: Number(tableRefId), vaiTro: 'TrongTaiBan' });
    }

    setSubmittingAssign(true);
    try {
      await tranDauService.update(targetMatch.id, {
        giaiDauMonTheThaoId: targetMatch.giaiDauMonTheThaoId,
        vongDauId: targetMatch.vongDauId,
        bangDauId: targetMatch.bangDauId,
        sanDauId: targetMatch.sanDauId,
        soTran: targetMatch.soTran,
        tenTran: targetMatch.tenTran,
        thoiGianDuKien: targetMatch.thoiGianDuKien,
        thoiGianBatDau: targetMatch.thoiGianBatDau,
        thoiGianKetThuc: targetMatch.thoiGianKetThuc,
        trangThai: targetMatch.trangThai,
        ghiChu: targetMatch.ghiChu,
        doi1DangKyId: targetMatch.doi1DangKyId,
        doi2DangKyId: targetMatch.doi2DangKyId,
        danhSachTrongTai: assignList,
      });

      toast.success('Đã cập nhật tổ trọng tài điều hành trận đấu thành công!');
      setAssignModalOpen(false);
      loadMatches();
    } catch (err: any) {
      console.error(err);
      toast.error('Lỗi khi lưu phân công trọng tài');
    } finally {
      setSubmittingAssign(false);
    }
  };

  // Helper format thời gian
  const formatDateTime = (val?: string) => {
    if (!val) return '—';
    try {
      const d = new Date(val);
      if (isNaN(d.getTime())) return val;
      return d.toLocaleString('vi-VN', {
        hour: '2-digit',
        minute: '2-digit',
        day: '2-digit',
        month: '2-digit',
      });
    } catch {
      return val;
    }
  };

  // Helper render Badge trạng thái
  const renderStatusBadge = (status: string) => {
    switch (status) {
      case 'DangDienRa':
        return (
          <span
            className="badge rounded-pill px-2.5 py-1 d-inline-flex align-items-center gap-1.5"
            style={{ backgroundColor: '#fee2e2', color: '#dc2626', border: '1px solid #fecaca' }}
          >
            <span
              className="rounded-circle bg-danger"
              style={{ width: '6px', height: '6px', animation: 'pulse 1.5s infinite' }}
            />
            ĐANG ĐẤU
          </span>
        );
      case 'DaKetThuc':
        return (
          <span
            className="badge rounded-pill px-2.5 py-1"
            style={{ backgroundColor: '#f3f4f6', color: '#4b5563', border: '1px solid #e5e7eb' }}
          >
            ĐÃ KẾT THÚC
          </span>
        );
      case 'Hoan':
        return (
          <span
            className="badge rounded-pill px-2.5 py-1"
            style={{ backgroundColor: '#fef3c7', color: '#b45309', border: '1px solid #fde68a' }}
          >
            TẠM HOÃN
          </span>
        );
      case 'ChuaDau':
      default:
        return (
          <span
            className="badge rounded-pill px-2.5 py-1"
            style={{ backgroundColor: '#eff6ff', color: '#1d4ed8', border: '1px solid #bfdbfe' }}
          >
            CHƯA ĐẤU
          </span>
        );
    }
  };

  return (
    <div className="d-flex flex-column gap-4 pb-5">
      {/* ── Banner Trọng Tài Hiện Đại ── */}
      <div
        className="rounded-4 p-4 p-md-5 text-white position-relative overflow-hidden shadow-sm"
        style={{
          background: 'linear-gradient(135deg, #065f46 0%, #0d9488 60%, #1e293b 100%)',
        }}
      >
        <div
          className="position-absolute end-0 top-0 bottom-0 d-none d-md-flex align-items-center justify-content-end pe-5 opacity-10"
          style={{ pointerEvents: 'none' }}
        >
          <Award size={220} />
        </div>

        <div className="position-relative" style={{ zIndex: 2 }}>
          <div className="d-flex flex-wrap align-items-center justify-content-between gap-3 mb-3">
            <div
              className="d-inline-flex align-items-center gap-2 px-3 py-1 rounded-pill border"
              style={{
                backgroundColor: 'rgba(255, 255, 255, 0.15)',
                borderColor: 'rgba(255, 255, 255, 0.25)',
                fontSize: '12px',
              }}
            >
              <ShieldCheck size={16} className="text-warning" />
              <span className="fw-semibold text-white">Ban Trọng Tài Điều Hành Giải Đấu</span>
            </div>

            {/* Trọng tài selector (cho phép xem theo bất kỳ trọng tài nào) */}
            <div className="d-flex align-items-center gap-2 bg-white text-dark px-3 py-1.5 rounded-pill shadow-sm">
              <UserCheck size={15} className="text-success" />
              <span className="small fw-semibold text-secondary">Tư cách trọng tài:</span>
              <select
                className="form-select form-select-sm border-0 bg-transparent fw-bold text-dark p-0 ps-1 pe-4"
                style={{ fontSize: '13px', width: 'auto', cursor: 'pointer' }}
                value={activeRefereeId}
                onChange={(e) => setActiveRefereeId(e.target.value ? Number(e.target.value) : '')}
              >
                {allReferees.length === 0 && user && (
                  <option value={user.trongTaiId || ''}>
                    {user.fullName || user.username} (Trọng tài của bạn)
                  </option>
                )}
                {allReferees.map((r) => (
                  <option key={r.id} value={r.id}>
                    {r.hoTen} ({r.capBac || 'Trọng tài'})
                  </option>
                ))}
              </select>
            </div>
          </div>

          <h2 className="fw-bold mb-2 fs-3 fs-md-2">
            Nhiệm Vụ Điều Hành & Phân Công Trận Đấu
          </h2>
          <p className="text-white-50 mb-0 small" style={{ maxWidth: '750px', lineHeight: 1.6 }}>
            Xem các trận đấu bạn được phân công, chủ động bắt đầu trận, cập nhật kết quả & diễn biến trực tiếp, hoàn thiện thông tin hồ sơ và xuất biên bản thi đấu chính thức.
          </p>
        </div>
      </div>

      {/* ── Quick KPI Stat Cards ── */}
      <Row className="g-3">
        <Col xs={6} md={3}>
          <Card className="border-0 shadow-sm rounded-4 h-100 bg-white">
            <CardBody className="p-3 d-flex align-items-center gap-3">
              <div
                className="rounded-3 p-2.5 d-flex align-items-center justify-content-center text-success"
                style={{ backgroundColor: '#ecfdf5' }}
              >
                <UserCheck size={22} />
              </div>
              <div>
                <small className="text-muted d-block fw-medium" style={{ fontSize: '12px' }}>
                  Trận tôi được phân công
                </small>
                <div className="fs-4 fw-bold text-success lh-1">{stats.myTotal}</div>
              </div>
            </CardBody>
          </Card>
        </Col>

        <Col xs={6} md={3}>
          <Card className="border-0 shadow-sm rounded-4 h-100 bg-white">
            <CardBody className="p-3 d-flex align-items-center gap-3">
              <div
                className="rounded-3 p-2.5 d-flex align-items-center justify-content-center text-danger"
                style={{ backgroundColor: '#fef2f2' }}
              >
                <Flame size={22} />
              </div>
              <div>
                <small className="text-muted d-block fw-medium" style={{ fontSize: '12px' }}>
                  Đang diễn ra (Live)
                </small>
                <div className="fs-4 fw-bold text-danger lh-1">{stats.live}</div>
              </div>
            </CardBody>
          </Card>
        </Col>

        <Col xs={6} md={3}>
          <Card className="border-0 shadow-sm rounded-4 h-100 bg-white">
            <CardBody className="p-3 d-flex align-items-center gap-3">
              <div
                className="rounded-3 p-2.5 d-flex align-items-center justify-content-center text-warning"
                style={{ backgroundColor: '#fffbeb' }}
              >
                <Clock size={22} />
              </div>
              <div>
                <small className="text-muted d-block fw-medium" style={{ fontSize: '12px' }}>
                  Trận diễn ra hôm nay
                </small>
                <div className="fs-4 fw-bold text-dark lh-1">{stats.scheduledToday}</div>
              </div>
            </CardBody>
          </Card>
        </Col>

        <Col xs={6} md={3}>
          <Card className="border-0 shadow-sm rounded-4 h-100 bg-white">
            <CardBody className="p-3 d-flex align-items-center gap-3">
              <div
                className="rounded-3 p-2.5 d-flex align-items-center justify-content-center text-primary"
                style={{ backgroundColor: '#eff6ff' }}
              >
                <Sparkles size={22} />
              </div>
              <div>
                <small className="text-muted d-block fw-medium" style={{ fontSize: '12px' }}>
                  Đã hoàn thiện hồ sơ
                </small>
                <div className="fs-4 fw-bold text-primary lh-1">{stats.finalized}</div>
              </div>
            </CardBody>
          </Card>
        </Col>
      </Row>

      {/* ── TAB CHUYỂN ĐỔI CHÍNH: TRẬN PHÂN CÔNG vs TẤT CẢ TRẬN ── */}
      <div className="d-flex flex-wrap align-items-center justify-content-between gap-3 border-bottom pb-2">
        <Nav tabs className="border-bottom-0 gap-2 mb-0">
          <NavItem>
            <NavLink
              className={`rounded-pill px-4 py-2 fw-bold cursor-pointer border ${
                mainTab === 'my_matches'
                  ? 'bg-success text-white shadow-sm border-success'
                  : 'bg-white text-secondary border-light-subtle'
              }`}
              style={{ cursor: 'pointer' }}
              onClick={() => setMainTab('my_matches')}
            >
              <div className="d-flex align-items-center gap-2">
                <UserCheck size={16} />
                <span>Trận Đấu Tôi Được Phân Công</span>
                <Badge
                  color={mainTab === 'my_matches' ? 'light' : 'success'}
                  className={mainTab === 'my_matches' ? 'text-success' : 'text-white'}
                  pill
                >
                  {stats.myTotal}
                </Badge>
              </div>
            </NavLink>
          </NavItem>

          <NavItem>
            <NavLink
              className={`rounded-pill px-4 py-2 fw-bold cursor-pointer border ${
                mainTab === 'all_matches'
                  ? 'bg-primary text-white shadow-sm border-primary'
                  : 'bg-white text-secondary border-light-subtle'
              }`}
              style={{ cursor: 'pointer' }}
              onClick={() => setMainTab('all_matches')}
            >
              <div className="d-flex align-items-center gap-2">
                <Calendar size={16} />
                <span>Tất Cả Lượt Trận Trong Giải</span>
                <Badge
                  color={mainTab === 'all_matches' ? 'light' : 'secondary'}
                  className={mainTab === 'all_matches' ? 'text-primary' : 'text-white'}
                  pill
                >
                  {stats.total}
                </Badge>
              </div>
            </NavLink>
          </NavItem>
        </Nav>

        {/* View mode toggle & Refresh */}
        <div className="d-flex align-items-center gap-2">
          <div className="btn-group btn-group-sm">
            <button
              type="button"
              onClick={() => setViewMode('grid')}
              className={`btn ${viewMode === 'grid' ? 'btn-dark' : 'btn-outline-secondary'}`}
              title="Dạng thẻ"
            >
              <LayoutGrid size={15} />
            </button>
            <button
              type="button"
              onClick={() => setViewMode('table')}
              className={`btn ${viewMode === 'table' ? 'btn-dark' : 'btn-outline-secondary'}`}
              title="Dạng bảng"
            >
              <TableIcon size={15} />
            </button>
          </div>

          <Button
            color="light"
            size="sm"
            className="border p-1.5 rounded-2 text-secondary shadow-sm"
            onClick={loadMatches}
            disabled={loadingMatches}
            title="Tải lại dữ liệu"
          >
            <RotateCcw size={15} className={loadingMatches ? 'spin' : ''} />
          </Button>
        </div>
      </div>

      {/* ── Bộ Lọc & Tìm Kiếm Trận Đấu ── */}
      <Card className="border-0 shadow-sm rounded-4">
        <CardBody className="p-3 p-md-4">
          <Row className="g-3">
            {/* Chọn giải đấu */}
            <Col xs={12} md={4} lg={3}>
              <FormGroup className="mb-0">
                <Label className="fw-semibold text-secondary small mb-1">
                  Giải đấu đang xem <span className="text-danger">*</span>
                </Label>
                <Input
                  type="select"
                  className="rounded-3"
                  value={selectedGiaiDauId}
                  onChange={(e) =>
                    setSelectedGiaiDauId(e.target.value === 'ALL' ? 'ALL' : Number(e.target.value))
                  }
                >
                  <option value="ALL">-- Tất cả giải đấu --</option>
                  {giaiDaus.map((g) => (
                    <option key={g.id} value={g.id}>
                      {g.ten}{' '}
                      {g.trangThai === TrangThaiGiaiDau.DangDienRa ||
                      (g.trangThai as any) === 'DangDienRa'
                        ? '(Đang diễn ra)'
                        : ''}
                    </option>
                  ))}
                </Input>
              </FormGroup>
            </Col>

            {/* Lọc Vai trò của tôi (chỉ có khi ở tab trận của tôi) */}
            {mainTab === 'my_matches' && (
              <Col xs={6} md={4} lg={2}>
                <FormGroup className="mb-0">
                  <Label className="fw-semibold text-secondary small mb-1">Vai trò của bạn</Label>
                  <Input
                    type="select"
                    className="rounded-3"
                    value={selectedRoleFilter}
                    onChange={(e) => setSelectedRoleFilter(e.target.value)}
                  >
                    <option value="ALL">-- Tất cả vai trò --</option>
                    <option value="TrongTaiChinh">Trọng tài chính</option>
                    <option value="TroLy1">Trợ lý 1</option>
                    <option value="TroLy2">Trợ lý 2</option>
                    <option value="TrongTaiBan">Trọng tài bàn / Thư ký</option>
                  </Input>
                </FormGroup>
              </Col>
            )}

            {/* Lọc Môn thi đấu */}
            <Col xs={6} md={4} lg={mainTab === 'my_matches' ? 2 : 3}>
              <FormGroup className="mb-0">
                <Label className="fw-semibold text-secondary small mb-1">Môn thi đấu</Label>
                <Input
                  type="select"
                  className="rounded-3"
                  value={selectedSportId}
                  onChange={(e) => setSelectedSportId(e.target.value)}
                >
                  <option value="ALL">-- Tất cả môn --</option>
                  {sports.map((sp) => (
                    <option key={sp.id} value={sp.id}>
                      {sp.ten}
                    </option>
                  ))}
                </Input>
              </FormGroup>
            </Col>

            {/* Lọc Sân thi đấu */}
            <Col xs={6} md={4} lg={2}>
              <FormGroup className="mb-0">
                <Label className="fw-semibold text-secondary small mb-1">Địa điểm / Sân</Label>
                <Input
                  type="select"
                  className="rounded-3"
                  value={selectedVenueId}
                  onChange={(e) => setSelectedVenueId(e.target.value)}
                >
                  <option value="ALL">-- Tất cả sân --</option>
                  {venues.map((v) => (
                    <option key={v.id} value={v.id}>
                      {v.ten}
                    </option>
                  ))}
                </Input>
              </FormGroup>
            </Col>

            {/* Lọc Trạng thái */}
            <Col xs={6} md={4} lg={2}>
              <FormGroup className="mb-0">
                <Label className="fw-semibold text-secondary small mb-1">Trạng thái</Label>
                <Input
                  type="select"
                  className="rounded-3"
                  value={selectedStatus}
                  onChange={(e) => setSelectedStatus(e.target.value)}
                >
                  <option value="ALL">-- Tất cả trạng thái --</option>
                  <option value="DangDienRa">Đang diễn ra (Live)</option>
                  <option value="ChuaDau">Chưa đấu</option>
                  <option value="DaKetThuc">Đã kết thúc</option>
                  <option value="Hoan">Tạm hoãn</option>
                </Input>
              </FormGroup>
            </Col>

            {/* Lọc Ngày */}
            <Col xs={12} md={4} lg={mainTab === 'my_matches' ? 3 : 2}>
              <FormGroup className="mb-0">
                <Label className="fw-semibold text-secondary small mb-1">Ngày thi đấu</Label>
                <div className="d-flex gap-1.5">
                  <Input
                    type="date"
                    className="rounded-3"
                    value={filterDate}
                    onChange={(e) => setFilterDate(e.target.value)}
                  />
                  {filterDate && (
                    <Button
                      color="light"
                      size="sm"
                      className="border"
                      onClick={() => setFilterDate('')}
                      title="Xóa lọc ngày"
                    >
                      X
                    </Button>
                  )}
                </div>
              </FormGroup>
            </Col>

            {/* Tìm kiếm từ khóa */}
            <Col xs={12}>
              <div className="position-relative">
                <Search
                  size={16}
                  className="position-absolute text-muted"
                  style={{ top: '11px', left: '12px' }}
                />
                <Input
                  type="text"
                  placeholder="Tìm theo tên đội, VĐV, tên trận, môn thi, sân bãi..."
                  className="rounded-3 ps-5"
                  value={searchKeyword}
                  onChange={(e) => setSearchKeyword(e.target.value)}
                />
              </div>
            </Col>
          </Row>
        </CardBody>
      </Card>

      {/* ── Danh Sách Trận Đấu ── */}
      <div>
        <div className="d-flex align-items-center justify-content-between mb-3">
          <div className="d-flex align-items-center gap-2">
            <h5 className="fw-bold text-dark mb-0">
              {mainTab === 'my_matches'
                ? 'Lượt Trận Bạn Được Phân Công Điều Hành'
                : 'Toàn Bộ Lượt Trận Trong Giải Đấu'}
            </h5>
            <Badge color="secondary" pill className="px-2.5">
              {filteredMatches.length} trận
            </Badge>
          </div>

          <div className="text-muted small">
            Hiển thị {filteredMatches.length} trên tổng số {matches.length} trận
          </div>
        </div>

        {loadingMatches ? (
          <div className="text-center py-5">
            <Spinner color="primary" />
            <p className="text-muted small mt-2">Đang tải lịch thi đấu...</p>
          </div>
        ) : filteredMatches.length === 0 ? (
          <Card className="border-0 shadow-sm rounded-4 text-center py-5">
            <CardBody>
              <Calendar size={48} className="text-muted opacity-50 mb-3" />
              <h6 className="fw-bold text-dark mb-1">
                {mainTab === 'my_matches'
                  ? 'Chưa có trận đấu nào được phân công cho trọng tài này'
                  : 'Không tìm thấy trận đấu nào phù hợp'}
              </h6>
              <p className="text-muted small mb-3">
                {mainTab === 'my_matches'
                  ? 'Vui lòng chọn trọng tài khác ở thanh phía trên, hoặc chuyển sang tab "Tất cả trận đấu" để phân công.'
                  : 'Vui lòng thay đổi tiêu chí lọc hoặc chọn giải đấu khác để xem lịch thi đấu.'}
              </p>
              <Button
                color="outline-primary"
                size="sm"
                className="rounded-pill px-3"
                onClick={() => {
                  setMainTab('all_matches');
                  setSelectedSportId('ALL');
                  setSelectedVenueId('ALL');
                  setSelectedStatus('ALL');
                  setSelectedRoleFilter('ALL');
                  setFilterDate('');
                  setSearchKeyword('');
                }}
              >
                Xem tất cả trận đấu
              </Button>
            </CardBody>
          </Card>
        ) : viewMode === 'grid' ? (
          /* ── GRID CARD VIEW ── */
          <Row className="g-3">
            {filteredMatches.map((m) => {
              const myRole = getRefereeRoleInMatch(m);
              const scoreDetails = refereeScoreStorage.getScoreDetails(m);

              return (
                <Col xs={12} lg={6} key={m.id}>
                  <Card
                    className={`border-0 shadow-sm rounded-4 h-100 transition-all ${
                      m.trangThai === 'DangDienRa'
                        ? 'border border-2 border-danger'
                        : myRole
                        ? 'border border-2 border-success-subtle'
                        : ''
                    }`}
                  >
                    <CardBody className="p-4 d-flex flex-column justify-content-between gap-3">
                      {/* Top Header Card */}
                      <div>
                        <div className="d-flex align-items-center justify-content-between gap-2 mb-2 flex-wrap">
                          <div className="d-flex align-items-center gap-2 flex-wrap">
                            <span className="badge bg-light text-dark border fw-medium px-2 py-1">
                              Trận #{m.soTran || m.id}
                            </span>
                            <span className="fw-bold text-primary" style={{ fontSize: '13.5px' }}>
                              {m.tenMonTheThao || 'Môn thi đấu'}
                            </span>
                            {m.tenVongDau && (
                              <span className="text-muted small">• {m.tenVongDau}</span>
                            )}
                            {m.tenBangDau && (
                              <span className="badge bg-secondary-subtle text-secondary small">
                                {m.tenBangDau}
                              </span>
                            )}
                          </div>

                          <div className="d-flex align-items-center gap-1.5 flex-wrap">
                            {renderStatusBadge(m.trangThai)}
                            {scoreDetails.isFinalized && (
                              <span
                                className="badge rounded-pill px-2 py-1 small"
                                style={{ backgroundColor: '#ecfdf5', color: '#059669', border: '1px solid #a7f3d0' }}
                                title="Hồ sơ đã được hoàn thiện & chốt"
                              >
                                <Lock size={11} className="me-1" />
                                Đã chốt
                              </span>
                            )}
                          </div>
                        </div>

                        {/* Huy hiệu vai trò nổi bật của tôi */}
                        {myRole && (
                          <div className="mb-2.5">
                            <span
                              className={`badge rounded-pill px-2.5 py-1.5 small fw-bold d-inline-flex align-items-center gap-1 ${
                                myRole === 'TrongTaiChinh'
                                  ? 'bg-success text-white'
                                  : myRole === 'TroLy1' || myRole === 'TroLy2'
                                  ? 'bg-primary text-white'
                                  : 'bg-purple text-white'
                              }`}
                              style={{
                                backgroundColor:
                                  myRole === 'TrongTaiBan' ? '#7c3aed' : undefined,
                              }}
                            >
                              <UserCheck size={13} />
                              <span>
                                Bạn là:{' '}
                                {myRole === 'TrongTaiChinh'
                                  ? 'TRỌNG TÀI CHÍNH'
                                  : myRole === 'TroLy1'
                                  ? 'TRỢ LÝ 1'
                                  : myRole === 'TroLy2'
                                  ? 'TRỢ LÝ 2'
                                  : 'TRỌNG TÀI BÀN / THƯ KÝ'}
                              </span>
                            </span>
                          </div>
                        )}

                        {/* Tên trận / Sân đấu / Thời gian */}
                        <div className="d-flex flex-wrap align-items-center gap-3 text-secondary small mb-3 pb-2 border-bottom">
                          <div className="d-flex align-items-center gap-1">
                            <Clock size={13} className="text-muted" />
                            <span>
                              {formatDateTime(m.thoiGianBatDau || m.thoiGianDuKien)}
                            </span>
                          </div>
                          <div className="d-flex align-items-center gap-1">
                            <MapPin size={13} className="text-danger" />
                            <span>
                              {m.tenSanDau || 'Chưa xếp sân'}
                              {m.tenCumSan ? ` (${m.tenCumSan})` : ''}
                            </span>
                          </div>
                        </div>

                        {/* Cặp đấu VERSUS & Tỷ số */}
                        <div
                          className="rounded-3 p-3 mb-3"
                          style={{
                            background: 'linear-gradient(135deg, #f8fafc 0%, #f1f5f9 100%)',
                            border: '1px solid #e2e8f0',
                          }}
                        >
                          <div className="row align-items-center text-center">
                            {/* Đội 1 */}
                            <div className="col-5 text-start">
                              <div className="fw-bold text-dark fs-6 text-truncate">
                                {m.tenDoi1 || 'Đội 1 (Chờ xác định)'}
                              </div>
                              {m.donViDoi1 && (
                                <small className="text-muted text-truncate d-block" style={{ fontSize: '11px' }}>
                                  {m.donViDoi1}
                                </small>
                              )}
                            </div>

                            {/* Tỷ số hoặc VS */}
                            <div className="col-2 px-0">
                              {m.trangThai === 'DangDienRa' || m.trangThai === 'DaKetThuc' ? (
                                <div className="fw-bold fs-5 text-danger font-monospace">
                                  {scoreDetails.score1} - {scoreDetails.score2}
                                </div>
                              ) : (
                                <span className="badge rounded-pill bg-light text-muted border px-2 py-1">
                                  VS
                                </span>
                              )}
                            </div>

                            {/* Đội 2 */}
                            <div className="col-5 text-end">
                              <div className="fw-bold text-dark fs-6 text-truncate">
                                {m.tenDoi2 || 'Đội 2 (Chờ xác định)'}
                              </div>
                              {m.donViDoi2 && (
                                <small className="text-muted text-truncate d-block" style={{ fontSize: '11px' }}>
                                  {m.donViDoi2}
                                </small>
                              )}
                            </div>
                          </div>
                        </div>

                        {/* Tổ Trọng Tài Phân Công */}
                        <div className="bg-light p-2.5 rounded-3 mb-2">
                          <div className="d-flex align-items-center justify-content-between mb-1.5">
                            <span className="fw-semibold text-dark small d-flex align-items-center gap-1">
                              <Users size={13} className="text-success" />
                              Tổ trọng tài điều hành:
                            </span>
                            <button
                              type="button"
                              onClick={() => handleOpenAssignModal(m)}
                              className="btn btn-link btn-sm p-0 text-primary text-decoration-none small"
                              style={{ fontSize: '11.5px' }}
                            >
                              <Edit size={12} className="me-1" />
                              Đổi tổ trọng tài
                            </button>
                          </div>

                          <div className="d-flex flex-wrap gap-1.5">
                            {m.danhSachTrongTai && m.danhSachTrongTai.length > 0 ? (
                              m.danhSachTrongTai.map((r, idx) => (
                                <span
                                  key={idx}
                                  className={`badge rounded-pill px-2 py-1 small border ${
                                    currentReferee && r.trongTaiId === currentReferee.id
                                      ? 'bg-success text-white border-success'
                                      : 'bg-white text-dark'
                                  }`}
                                  style={{ fontSize: '11px' }}
                                >
                                  <strong>
                                    {r.vaiTro === 'TrongTaiChinh'
                                      ? 'TT Chính: '
                                      : r.vaiTro === 'TroLy1'
                                      ? 'Trợ lý 1: '
                                      : r.vaiTro === 'TroLy2'
                                      ? 'Trợ lý 2: '
                                      : r.vaiTro === 'TrongTaiBan'
                                      ? 'TT Bàn: '
                                      : ''}
                                  </strong>
                                  {r.tenTrongTai || `Trọng tài #${r.trongTaiId}`}
                                </span>
                              ))
                            ) : (
                              <span className="text-muted small fst-italic">
                                Chưa phân công trọng tài cho trận này
                              </span>
                            )}
                          </div>
                        </div>

                        {/* Tóm tắt diễn biến / MVP nếu có */}
                        {scoreDetails.mvpAthlete && (
                          <div className="small text-muted d-flex align-items-center gap-1 mb-1">
                            <Sparkles size={12} className="text-warning" />
                            <span>VĐV xuất sắc (MVP): <strong>{scoreDetails.mvpAthlete}</strong></span>
                          </div>
                        )}
                      </div>

                      {/* Bottom Action Buttons: Đầy đủ 4 chức năng */}
                      <div className="d-flex flex-wrap align-items-center justify-content-between gap-2 pt-2 border-top">
                        {/* Nút Bắt đầu trận nếu chưa đấu */}
                        {m.trangThai === 'ChuaDau' && (
                          <Button
                            color="success"
                            size="sm"
                            className="rounded-3 d-flex align-items-center gap-1 fw-bold shadow-sm"
                            onClick={() => handleStartMatch(m)}
                          >
                            <Play size={13} fill="currentColor" />
                            <span>Bắt đầu trận</span>
                          </Button>
                        )}

                        <div className="d-flex flex-wrap align-items-center gap-1.5 ms-auto">
                          {/* 1. Nút Cập nhật nhanh kết quả & diễn biến */}
                          <Button
                            color="outline-danger"
                            size="sm"
                            className="rounded-3 d-flex align-items-center gap-1 fw-semibold"
                            onClick={() => handleOpenQuickScore(m)}
                            title="Cập nhật tỷ số và diễn biến trận đấu"
                          >
                            <Flame size={13} />
                            <span>Cập nhật kết quả</span>
                          </Button>

                          {/* 2. Nút Hoàn thiện thông tin trận đấu */}
                          <Button
                            color="outline-success"
                            size="sm"
                            className="rounded-3 d-flex align-items-center gap-1 fw-semibold"
                            onClick={() => handleOpenFinalize(m)}
                            title="Hoàn thiện giờ thi đấu, thời tiết, MVP, nhận xét trọng tài"
                          >
                            <Sparkles size={13} />
                            <span>Hoàn thiện hồ sơ</span>
                          </Button>

                          {/* 3. Nút Xem & Lập biên bản */}
                          <Link
                            href={`/trong-tai/bien-ban?tranDauId=${m.id}&giaiDauId=${selectedGiaiDauId}`}
                            className="btn btn-light border btn-sm rounded-3 d-flex align-items-center gap-1 fw-semibold text-secondary"
                            title="Biên bản thi đấu A4"
                          >
                            <FileText size={13} />
                            <span>Biên bản</span>
                          </Link>
                        </div>
                      </div>
                    </CardBody>
                  </Card>
                </Col>
              );
            })}
          </Row>
        ) : (
          /* ── TABLE VIEW ── */
          <Card className="border-0 shadow-sm rounded-4 overflow-hidden">
            <div className="table-responsive">
              <Table hover className="align-middle mb-0" style={{ fontSize: '13.5px' }}>
                <thead className="table-light text-secondary" style={{ fontSize: '12px' }}>
                  <tr>
                    <th className="ps-3">TRẬN</th>
                    <th>MÔN / VÒNG</th>
                    <th>THỜI GIAN & SÂN</th>
                    <th>CẶP ĐẤU (ĐỘI 1 vs ĐỘI 2)</th>
                    <th>TỔ TRỌNG TÀI</th>
                    <th>TRẠNG THÁI</th>
                    <th className="text-end pe-3">THAO TÁC</th>
                  </tr>
                </thead>
                <tbody>
                  {filteredMatches.map((m) => {
                    const scoreDetails = refereeScoreStorage.getScoreDetails(m);
                    const myRole = getRefereeRoleInMatch(m);

                    return (
                      <tr key={m.id}>
                        <td className="ps-3">
                          <span className="badge bg-light text-dark border font-monospace px-2 py-1">
                            #{m.soTran || m.id}
                          </span>
                        </td>
                        <td>
                          <div className="fw-semibold text-dark">{m.tenMonTheThao || 'Môn thi'}</div>
                          <small className="text-muted d-block">
                            {m.tenVongDau} {m.tenBangDau ? `• ${m.tenBangDau}` : ''}
                          </small>
                        </td>
                        <td>
                          <div className="fw-medium text-dark">
                            {formatDateTime(m.thoiGianBatDau || m.thoiGianDuKien)}
                          </div>
                          <small className="text-secondary d-flex align-items-center gap-1">
                            <MapPin size={11} className="text-danger" />
                            {m.tenSanDau || 'Chưa xếp sân'}
                          </small>
                        </td>
                        <td>
                          <div className="d-flex align-items-center gap-2">
                            <span className="fw-semibold text-dark">{m.tenDoi1 || 'Đội 1'}</span>
                            <span className="badge bg-danger-subtle text-danger px-1.5 py-0.5 font-monospace">
                              {scoreDetails.score1} - {scoreDetails.score2}
                            </span>
                            <span className="fw-semibold text-dark">{m.tenDoi2 || 'Đội 2'}</span>
                          </div>
                        </td>
                        <td>
                          <div className="d-flex flex-wrap gap-1" style={{ maxWidth: '240px' }}>
                            {myRole && (
                              <span className="badge bg-success text-white mb-1" style={{ fontSize: '10px' }}>
                                ★ Bạn là: {myRole === 'TrongTaiChinh' ? 'TT Chính' : myRole}
                              </span>
                            )}
                            {(m.danhSachTrongTai || []).map((r, i) => (
                              <span
                                key={i}
                                className="badge bg-light text-dark border"
                                style={{ fontSize: '10.5px' }}
                              >
                                {r.vaiTro === 'TrongTaiChinh' ? 'TT Chính: ' : ''}
                                {r.tenTrongTai}
                              </span>
                            ))}
                          </div>
                        </td>
                        <td>
                          <div className="d-flex flex-column gap-1">
                            {renderStatusBadge(m.trangThai)}
                            {scoreDetails.isFinalized && (
                              <span className="badge bg-success-subtle text-success border border-success-subtle px-1.5 py-0.5 small">
                                Đã chốt hồ sơ
                              </span>
                            )}
                          </div>
                        </td>
                        <td className="text-end pe-3">
                          <div className="d-flex align-items-center justify-content-end gap-1.5">
                            {m.trangThai === 'ChuaDau' && (
                              <Button
                                color="success"
                                size="sm"
                                className="p-1 px-2 rounded-2"
                                title="Bắt đầu trận"
                                onClick={() => handleStartMatch(m)}
                              >
                                <Play size={13} />
                              </Button>
                            )}

                            {/* Cập nhật kết quả nhanh */}
                            <Button
                              color="outline-danger"
                              size="sm"
                              className="p-1 px-2 rounded-2"
                              title="Cập nhật kết quả & diễn biến nhanh"
                              onClick={() => handleOpenQuickScore(m)}
                            >
                              <Flame size={13} />
                            </Button>

                            {/* Hoàn thiện thông tin */}
                            <Button
                              color="outline-success"
                              size="sm"
                              className="p-1 px-2 rounded-2"
                              title="Hoàn thiện thông tin trận đấu"
                              onClick={() => handleOpenFinalize(m)}
                            >
                              <Sparkles size={13} />
                            </Button>

                            {/* Đổi trọng tài */}
                            <Button
                              color="light"
                              size="sm"
                              className="p-1 px-2 border rounded-2"
                              title="Đổi tổ trọng tài"
                              onClick={() => handleOpenAssignModal(m)}
                            >
                              <Users size={13} />
                            </Button>

                            {/* Biên bản */}
                            <Link
                              href={`/trong-tai/bien-ban?tranDauId=${m.id}&giaiDauId=${selectedGiaiDauId}`}
                              className="btn btn-sm btn-light border p-1 px-2 rounded-2"
                              title="Biên bản trận đấu"
                            >
                              <FileText size={13} />
                            </Link>
                          </div>
                        </td>
                      </tr>
                    );
                  })}
                </tbody>
              </Table>
            </div>
          </Card>
        )}
      </div>

      {/* ── MODAL 1: CẬP NHẬT NHANH KẾT QUẢ & DIỄN BIẾN ── */}
      <Modal isOpen={quickScoreModalOpen} toggle={() => setQuickScoreModalOpen(false)} centered size="lg">
        <ModalHeader toggle={() => setQuickScoreModalOpen(false)}>
          <div className="d-flex align-items-center gap-2">
            <Flame size={18} className="text-danger" />
            <span>Cập Nhật Kết Quả & Diễn Biến Trực Tiếp</span>
          </div>
        </ModalHeader>
        <ModalBody className="p-4">
          {quickScoreMatch && (
            <>
              {/* Match Header Info */}
              <div className="p-3 bg-light rounded-3 border mb-3">
                <div className="fw-bold text-dark">
                  {quickScoreMatch.tenMonTheThao} - Trận #{quickScoreMatch.soTran || quickScoreMatch.id}
                </div>
                <div className="small text-muted d-flex align-items-center gap-3 mt-1">
                  <span>Sân: {quickScoreMatch.tenSanDau || 'Chưa xếp'}</span>
                  <span>•</span>
                  <span>Lịch: {formatDateTime(quickScoreMatch.thoiGianBatDau || quickScoreMatch.thoiGianDuKien)}</span>
                </div>
              </div>

              {/* Score Input Versus */}
              <div className="p-4 rounded-3 text-white mb-3" style={{ background: '#0f172a' }}>
                <Row className="align-items-center text-center g-3">
                  <Col xs={5}>
                    <h5 className="text-white fw-bold mb-2 text-truncate">
                      {quickScoreMatch.tenDoi1 || 'Đội 1'}
                    </h5>
                    <div className="d-flex align-items-center justify-content-center gap-2">
                      <Button
                        color="outline-light"
                        size="sm"
                        className="rounded-circle"
                        style={{ width: '36px', height: '36px' }}
                        onClick={() => setQScore1(Math.max(0, qScore1 - 1))}
                      >
                        -
                      </Button>
                      <Input
                        type="number"
                        min={0}
                        className="text-center font-monospace fw-bold fs-4 rounded-3"
                        style={{ width: '70px' }}
                        value={qScore1}
                        onChange={(e) => setQScore1(parseInt(e.target.value, 10) || 0)}
                      />
                      <Button
                        color="success"
                        size="sm"
                        className="rounded-circle"
                        style={{ width: '36px', height: '36px' }}
                        onClick={() => setQScore1(qScore1 + 1)}
                      >
                        +
                      </Button>
                    </div>
                  </Col>

                  <Col xs={2}>
                    <div className="text-warning fw-black fs-4">VS</div>
                    <small className="text-white-50">TỶ SỐ</small>
                  </Col>

                  <Col xs={5}>
                    <h5 className="text-white fw-bold mb-2 text-truncate">
                      {quickScoreMatch.tenDoi2 || 'Đội 2'}
                    </h5>
                    <div className="d-flex align-items-center justify-content-center gap-2">
                      <Button
                        color="outline-light"
                        size="sm"
                        className="rounded-circle"
                        style={{ width: '36px', height: '36px' }}
                        onClick={() => setQScore2(Math.max(0, qScore2 - 1))}
                      >
                        -
                      </Button>
                      <Input
                        type="number"
                        min={0}
                        className="text-center font-monospace fw-bold fs-4 rounded-3"
                        style={{ width: '70px' }}
                        value={qScore2}
                        onChange={(e) => setQScore2(parseInt(e.target.value, 10) || 0)}
                      />
                      <Button
                        color="success"
                        size="sm"
                        className="rounded-circle"
                        style={{ width: '36px', height: '36px' }}
                        onClick={() => setQScore2(qScore2 + 1)}
                      >
                        +
                      </Button>
                    </div>
                  </Col>
                </Row>
              </div>

              <Row className="g-3 mb-3">
                <Col md={6}>
                  <FormGroup className="mb-0">
                    <Label className="fw-semibold small">Trạng thái trận đấu</Label>
                    <Input
                      type="select"
                      className="rounded-3"
                      value={qStatus}
                      onChange={(e) => setQStatus(e.target.value)}
                    >
                      <option value="ChuaDau">Chưa đấu</option>
                      <option value="DangDienRa">Đang diễn ra (Live)</option>
                      <option value="DaKetThuc">Đã kết thúc</option>
                      <option value="Hoan">Tạm hoãn</option>
                    </Input>
                  </FormGroup>
                </Col>

                <Col md={6}>
                  <FormGroup className="mb-0">
                    <Label className="fw-semibold small">Đội thắng cuộc</Label>
                    <Input
                      type="select"
                      className="rounded-3"
                      value={qWinner !== undefined ? String(qWinner) : (qScore1 > qScore2 ? '1' : qScore2 > qScore1 ? '2' : 'draw')}
                      onChange={(e) => setQWinner(e.target.value === '1' ? 1 : e.target.value === '2' ? 2 : 'draw')}
                    >
                      <option value="1">{quickScoreMatch.tenDoi1 || 'Đội 1'} thắng</option>
                      <option value="2">{quickScoreMatch.tenDoi2 || 'Đội 2'} thắng</option>
                      <option value="draw">Hòa</option>
                    </Input>
                  </FormGroup>
                </Col>
              </Row>

              {/* Diễn biến sự kiện */}
              <div className="border rounded-3 p-3 mb-2">
                <div className="d-flex align-items-center justify-content-between mb-2 pb-2 border-bottom">
                  <span className="fw-bold small text-dark d-flex align-items-center gap-1.5">
                    <Flag size={14} className="text-warning" />
                    Diễn biến & thẻ phạt ({qEvents.length})
                  </span>
                  <Button
                    color="light"
                    size="sm"
                    className="border rounded-pill px-2.5 py-0.5 small"
                    onClick={() => {
                      setQEventType('goal');
                      setQEventTeam(1);
                      setQEventMinute(1);
                      setQEventAthlete('');
                      setQEventAssist('');
                      setQEventDetails('');
                      setQuickEventModalOpen(true);
                    }}
                  >
                    + Thêm sự kiện
                  </Button>
                </div>

                {qEvents.length === 0 ? (
                  <small className="text-muted fst-italic d-block text-center py-2">
                    Chưa có sự kiện nào
                  </small>
                ) : (
                  <div className="d-flex flex-column gap-1.5" style={{ maxHeight: '180px', overflowY: 'auto' }}>
                    {qEvents.map((ev) => (
                      <div
                        key={ev.id}
                        className="p-1.5 px-2.5 rounded-2 bg-light border d-flex align-items-center justify-content-between small"
                      >
                        <div>
                          <span className="badge bg-dark me-1">{ev.minute}&apos;</span>
                          <span className="fw-semibold me-1">
                            [{ev.team === 1 ? quickScoreMatch.tenDoi1 || 'Đội 1' : quickScoreMatch.tenDoi2 || 'Đội 2'}]
                          </span>
                          <span>{ev.type === 'goal' ? '⚽ Ghi bàn' : ev.type === 'yellow_card' ? '🟨 Thẻ vàng' : ev.type === 'red_card' ? '🟥 Thẻ đỏ' : ev.type}</span>
                          {ev.athleteName && <span className="ms-1 text-primary">({ev.athleteName})</span>}
                        </div>
                        <button
                          type="button"
                          className="btn btn-sm btn-link text-danger p-0"
                          onClick={() => setQEvents(qEvents.filter((x) => x.id !== ev.id))}
                        >
                          <Trash2 size={12} />
                        </button>
                      </div>
                    ))}
                  </div>
                )}
              </div>
            </>
          )}
        </ModalBody>
        <ModalFooter className="d-flex justify-content-between">
          <Link
            href={`/trong-tai/ket-qua?tranDauId=${quickScoreMatch?.id}&giaiDauId=${selectedGiaiDauId}`}
            className="btn btn-outline-primary btn-sm rounded-3 d-flex align-items-center gap-1 fw-semibold"
          >
            <span>Mở trang ghi nhận chi tiết</span>
            <ChevronRight size={14} />
          </Link>

          <div className="d-flex align-items-center gap-2">
            <Button color="light" size="sm" className="rounded-3" onClick={() => setQuickScoreModalOpen(false)}>
              Hủy
            </Button>
            <Button
              color="danger"
              size="sm"
              className="rounded-3 fw-bold"
              onClick={() => handleSaveQuickScore(true)}
              disabled={savingQuickScore}
            >
              Hết trận & Khóa tỷ số
            </Button>
            <Button
              color="success"
              size="sm"
              className="rounded-3 fw-bold shadow-sm"
              onClick={() => handleSaveQuickScore(false)}
              disabled={savingQuickScore}
            >
              {savingQuickScore ? <Spinner size="sm" /> : <Save size={14} className="me-1" />}
              <span>Lưu Tỷ Số</span>
            </Button>
          </div>
        </ModalFooter>
      </Modal>

      {/* ── MODAL 2: HOÀN THIỆN THÔNG TIN TRẬN ĐẤU (FINALIZE MODAL) ── */}
      <Modal isOpen={finalizeModalOpen} toggle={() => setFinalizeModalOpen(false)} centered size="lg">
        <ModalHeader toggle={() => setFinalizeModalOpen(false)}>
          <div className="d-flex align-items-center gap-2">
            <Sparkles size={18} className="text-success" />
            <span>Hoàn Thiện Hồ Sơ Trận Đấu & Báo Cáo Trọng Tài</span>
          </div>
        </ModalHeader>
        <ModalBody className="p-4">
          {finalizeMatch && (
            <>
              <div className="alert alert-light border rounded-3 p-3 mb-3 d-flex align-items-center justify-content-between">
                <div>
                  <div className="fw-bold text-dark">
                    Trận #{finalizeMatch.soTran || finalizeMatch.id}: {finalizeMatch.tenDoi1 || 'Đội 1'} vs {finalizeMatch.tenDoi2 || 'Đội 2'}
                  </div>
                  <small className="text-muted">
                    {finalizeMatch.tenMonTheThao} • {finalizeMatch.tenSanDau || 'Chưa xếp sân'}
                  </small>
                </div>
                {fIsFinalized && (
                  <span className="badge bg-success text-white px-2.5 py-1 rounded-pill">
                    ✓ Đã chốt hoàn thiện
                  </span>
                )}
              </div>

              <Row className="g-3">
                {/* 1. Thời gian thực tế */}
                <Col md={6}>
                  <FormGroup>
                    <div className="d-flex align-items-center justify-content-between mb-1">
                      <Label className="fw-semibold small mb-0">Giờ bắt đầu thực tế</Label>
                      <button
                        type="button"
                        onClick={() => setFActualStartTime(new Date().toISOString())}
                        className="btn btn-link btn-sm p-0 text-primary small text-decoration-none"
                        style={{ fontSize: '11px' }}
                      >
                        Giờ hiện tại
                      </button>
                    </div>
                    <Input
                      type="datetime-local"
                      className="rounded-3"
                      value={fActualStartTime ? fActualStartTime.slice(0, 16) : ''}
                      onChange={(e) => setFActualStartTime(e.target.value)}
                    />
                  </FormGroup>
                </Col>

                <Col md={6}>
                  <FormGroup>
                    <div className="d-flex align-items-center justify-content-between mb-1">
                      <Label className="fw-semibold small mb-0">Giờ kết thúc thực tế</Label>
                      <button
                        type="button"
                        onClick={() => setFActualEndTime(new Date().toISOString())}
                        className="btn btn-link btn-sm p-0 text-primary small text-decoration-none"
                        style={{ fontSize: '11px' }}
                      >
                        Giờ hiện tại
                      </button>
                    </div>
                    <Input
                      type="datetime-local"
                      className="rounded-3"
                      value={fActualEndTime ? fActualEndTime.slice(0, 16) : ''}
                      onChange={(e) => setFActualEndTime(e.target.value)}
                    />
                  </FormGroup>
                </Col>

                <Col xs={6} md={3}>
                  <FormGroup>
                    <Label className="fw-semibold small">Thời lượng (Phút)</Label>
                    <Input
                      type="number"
                      min={1}
                      className="rounded-3"
                      value={fDurationMinutes}
                      onChange={(e) => setFDurationMinutes(parseInt(e.target.value, 10) || 0)}
                    />
                  </FormGroup>
                </Col>

                <Col xs={6} md={3}>
                  <FormGroup>
                    <Label className="fw-semibold small">Bù giờ (Phút)</Label>
                    <Input
                      type="number"
                      min={0}
                      className="rounded-3"
                      value={fExtraTimeMinutes}
                      onChange={(e) => setFExtraTimeMinutes(parseInt(e.target.value, 10) || 0)}
                    />
                  </FormGroup>
                </Col>

                <Col md={6}>
                  <FormGroup>
                    <Label className="fw-semibold small">VĐV xuất sắc nhất trận (MVP)</Label>
                    <Input
                      type="text"
                      placeholder="Ví dụ: Nguyễn Văn A (#10)"
                      className="rounded-3"
                      value={fMvpAthlete}
                      onChange={(e) => setFMvpAthlete(e.target.value)}
                    />
                  </FormGroup>
                </Col>

                {/* 2. Điều kiện tổ chức */}
                <Col md={6}>
                  <FormGroup>
                    <Label className="fw-semibold small">Thời tiết / Khí hậu</Label>
                    <Input
                      type="text"
                      className="rounded-3"
                      value={fWeatherCondition}
                      onChange={(e) => setFWeatherCondition(e.target.value)}
                    />
                  </FormGroup>
                </Col>

                <Col md={6}>
                  <FormGroup>
                    <Label className="fw-semibold small">Tình trạng mặt sân / thảm đấu</Label>
                    <Input
                      type="text"
                      className="rounded-3"
                      value={fPitchCondition}
                      onChange={(e) => setFPitchCondition(e.target.value)}
                    />
                  </FormGroup>
                </Col>

                <Col md={6}>
                  <FormGroup>
                    <Label className="fw-semibold small">Hình thức phân định thắng thua</Label>
                    <Input
                      type="select"
                      className="rounded-3"
                      value={fWinMethod}
                      onChange={(e) => setFWinMethod(e.target.value as any)}
                    >
                      <option value="normal">Thời gian thi đấu chính thức</option>
                      <option value="extra_time">Hiệp phụ</option>
                      <option value="penalties">Luân lưu / Penalties</option>
                      <option value="walkover">Đối thủ bỏ cuộc (Walkover)</option>
                      <option value="disqualification">Truất quyền thi đấu</option>
                    </Input>
                  </FormGroup>
                </Col>

                <Col md={6}>
                  <FormGroup>
                    <Label className="fw-semibold small">Số lượng khán giả (Ước tính)</Label>
                    <Input
                      type="number"
                      min={0}
                      className="rounded-3"
                      value={fSpectatorCount}
                      onChange={(e) => setFSpectatorCount(parseInt(e.target.value, 10) || 0)}
                    />
                  </FormGroup>
                </Col>

                {/* 3. Nhận xét trọng tài & giám sát */}
                <Col xs={12}>
                  <FormGroup>
                    <Label className="fw-semibold small">Nhận xét của Tổ trọng tài điều hành</Label>
                    <Input
                      type="textarea"
                      rows={2}
                      placeholder="Ghi nhận về tinh thần thể thao, tính kỷ luật, sự cố nếu có..."
                      className="rounded-3"
                      value={fRefereeNotes}
                      onChange={(e) => setFRefereeNotes(e.target.value)}
                    />
                  </FormGroup>
                </Col>

                <Col xs={12}>
                  <FormGroup className="mb-0">
                    <Label className="fw-semibold small">Đánh giá của Giám sát trận đấu</Label>
                    <Input
                      type="textarea"
                      rows={2}
                      placeholder="Đánh giá công tác tổ chức, an ninh, y tế và chất lượng điều hành..."
                      className="rounded-3"
                      value={fSupervisorNotes}
                      onChange={(e) => setFSupervisorNotes(e.target.value)}
                    />
                  </FormGroup>
                </Col>
              </Row>
            </>
          )}
        </ModalBody>
        <ModalFooter className="d-flex justify-content-between">
          <Link
            href={`/trong-tai/bien-ban?tranDauId=${finalizeMatch?.id}&giaiDauId=${selectedGiaiDauId}`}
            className="btn btn-outline-primary btn-sm rounded-3 d-flex align-items-center gap-1 fw-semibold"
          >
            <FileText size={14} />
            <span>Ký số biên bản A4</span>
          </Link>

          <div className="d-flex align-items-center gap-2">
            <Button color="light" size="sm" className="rounded-3" onClick={() => setFinalizeModalOpen(false)}>
              Hủy
            </Button>
            <Button
              color="outline-secondary"
              size="sm"
              className="rounded-3 fw-semibold"
              onClick={() => handleSaveFinalize(false)}
              disabled={savingFinalize}
            >
              Lưu nháp
            </Button>
            <Button
              color="success"
              size="sm"
              className="rounded-3 fw-bold shadow-sm d-flex align-items-center gap-1"
              onClick={() => handleSaveFinalize(true)}
              disabled={savingFinalize}
            >
              {savingFinalize ? <Spinner size="sm" /> : <CheckCircle2 size={15} />}
              <span>Chốt Hoàn Thiện Hồ Sơ</span>
            </Button>
          </div>
        </ModalFooter>
      </Modal>

      {/* ── MODAL PHỤ: THÊM SỰ KIỆN NHANH TRONG QUICK SCORE ── */}
      <Modal isOpen={quickEventModalOpen} toggle={() => setQuickEventModalOpen(false)} centered size="sm">
        <ModalHeader toggle={() => setQuickEventModalOpen(false)}>
          <span className="small fw-bold">Thêm Diễn Biến Nhanh</span>
        </ModalHeader>
        <ModalBody className="p-3">
          <FormGroup className="mb-2">
            <Label className="small fw-semibold mb-1">Đội áp dụng</Label>
            <Input
              type="select"
              className="form-select-sm"
              value={qEventTeam}
              onChange={(e) => setQEventTeam(Number(e.target.value) as 1 | 2)}
            >
              <option value={1}>{quickScoreMatch?.tenDoi1 || 'Đội 1'}</option>
              <option value={2}>{quickScoreMatch?.tenDoi2 || 'Đội 2'}</option>
            </Input>
          </FormGroup>

          <FormGroup className="mb-2">
            <Label className="small fw-semibold mb-1">Loại sự kiện</Label>
            <Input
              type="select"
              className="form-select-sm"
              value={qEventType}
              onChange={(e) => setQEventType(e.target.value as MatchEvent['type'])}
            >
              <option value="goal">⚽ Bàn thắng / Điểm</option>
              <option value="yellow_card">🟨 Thẻ vàng</option>
              <option value="red_card">🟥 Thẻ đỏ</option>
              <option value="substitution">🔄 Thay người</option>
              <option value="foul">⚠️ Phạm lỗi / Kỹ thuật</option>
            </Input>
          </FormGroup>

          <FormGroup className="mb-2">
            <Label className="small fw-semibold mb-1">Phút</Label>
            <Input
              type="number"
              min={1}
              className="form-control-sm"
              value={qEventMinute}
              onChange={(e) => setQEventMinute(parseInt(e.target.value, 10) || 1)}
            />
          </FormGroup>

          <FormGroup className="mb-0">
            <Label className="small fw-semibold mb-1">VĐV liên quan</Label>
            <Input
              type="text"
              placeholder="Nguyễn Văn A (#10)"
              className="form-control-sm"
              value={qEventAthlete}
              onChange={(e) => setQEventAthlete(e.target.value)}
            />
          </FormGroup>
        </ModalBody>
        <ModalFooter className="p-2">
          <Button color="light" size="sm" onClick={() => setQuickEventModalOpen(false)}>
            Hủy
          </Button>
          <Button
            color="primary"
            size="sm"
            onClick={() => {
              const newEv: MatchEvent = {
                id: `${Date.now()}`,
                minute: qEventMinute,
                type: qEventType,
                team: qEventTeam,
                athleteName: qEventAthlete.trim() || undefined,
                timestamp: new Date().toISOString(),
              };
              if (qEventType === 'goal' || qEventType === 'point') {
                if (qEventTeam === 1) setQScore1(qScore1 + 1);
                else setQScore2(qScore2 + 1);
              }
              setQEvents([...qEvents, newEv].sort((a, b) => a.minute - b.minute));
              setQuickEventModalOpen(false);
              toast.success('Đã thêm sự kiện');
            }}
          >
            Thêm
          </Button>
        </ModalFooter>
      </Modal>

      {/* ── Modal Phân Công & Đổi Tổ Trọng Tài ── */}
      <Modal isOpen={assignModalOpen} toggle={() => setAssignModalOpen(false)} centered size="lg">
        <form onSubmit={handleSaveAssign}>
          <ModalHeader toggle={() => setAssignModalOpen(false)}>
            <div className="d-flex align-items-center gap-2">
              <Users size={18} className="text-success" />
              <span>Phân Công & Xác Nhận Tổ Trọng Tài</span>
            </div>
          </ModalHeader>
          <ModalBody className="p-4">
            {targetMatch && (
              <div className="alert alert-light border rounded-3 p-3 mb-3">
                <div className="fw-bold text-dark mb-1">
                  {targetMatch.tenMonTheThao} - Trận #{targetMatch.soTran || targetMatch.id}:{' '}
                  {targetMatch.tenDoi1 || 'Đội 1'} vs {targetMatch.tenDoi2 || 'Đội 2'}
                </div>
                <div className="small text-muted d-flex align-items-center gap-3">
                  <span>
                    <Clock size={12} className="me-1" />
                    {formatDateTime(targetMatch.thoiGianBatDau || targetMatch.thoiGianDuKien)}
                  </span>
                  <span>
                    <MapPin size={12} className="me-1 text-danger" />
                    {targetMatch.tenSanDau || 'Chưa xếp sân'}
                  </span>
                </div>
              </div>
            )}

            <Row className="g-3">
              {/* Trọng tài chính */}
              <Col md={6}>
                <FormGroup>
                  <Label className="fw-semibold small">
                    Trọng tài chính <span className="text-danger">*</span>
                  </Label>
                  <Input
                    type="select"
                    className="rounded-3"
                    value={mainRefId}
                    onChange={(e) => setMainRefId(e.target.value ? Number(e.target.value) : '')}
                    required
                  >
                    <option value="">-- Chọn Trọng tài chính --</option>
                    {allReferees.map((r) => (
                      <option key={r.id} value={r.id}>
                        {r.hoTen} ({r.capBac || 'Trọng tài'})
                      </option>
                    ))}
                  </Input>
                </FormGroup>
              </Col>

              {/* Trợ lý 1 */}
              <Col md={6}>
                <FormGroup>
                  <Label className="fw-semibold small">Trợ lý trọng tài 1</Label>
                  <Input
                    type="select"
                    className="rounded-3"
                    value={assistant1Id}
                    onChange={(e) => setAssistant1Id(e.target.value ? Number(e.target.value) : '')}
                  >
                    <option value="">-- Chọn Trợ lý 1 --</option>
                    {allReferees.map((r) => (
                      <option key={r.id} value={r.id}>
                        {r.hoTen} ({r.capBac || 'Trọng tài'})
                      </option>
                    ))}
                  </Input>
                </FormGroup>
              </Col>

              {/* Trợ lý 2 */}
              <Col md={6}>
                <FormGroup>
                  <Label className="fw-semibold small">Trợ lý trọng tài 2</Label>
                  <Input
                    type="select"
                    className="rounded-3"
                    value={assistant2Id}
                    onChange={(e) => setAssistant2Id(e.target.value ? Number(e.target.value) : '')}
                  >
                    <option value="">-- Chọn Trợ lý 2 --</option>
                    {allReferees.map((r) => (
                      <option key={r.id} value={r.id}>
                        {r.hoTen} ({r.capBac || 'Trọng tài'})
                      </option>
                    ))}
                  </Input>
                </FormGroup>
              </Col>

              {/* Trọng tài bàn / Thư ký */}
              <Col md={6}>
                <FormGroup>
                  <Label className="fw-semibold small">Trọng tài bàn / Thư ký</Label>
                  <Input
                    type="select"
                    className="rounded-3"
                    value={tableRefId}
                    onChange={(e) => setTableRefId(e.target.value ? Number(e.target.value) : '')}
                  >
                    <option value="">-- Chọn Trọng tài bàn --</option>
                    {allReferees.map((r) => (
                      <option key={r.id} value={r.id}>
                        {r.hoTen} ({r.capBac || 'Trọng tài'})
                      </option>
                    ))}
                  </Input>
                </FormGroup>
              </Col>
            </Row>
          </ModalBody>
          <ModalFooter>
            <Button
              color="light"
              className="rounded-3"
              onClick={() => setAssignModalOpen(false)}
              disabled={submittingAssign}
            >
              Hủy
            </Button>
            <Button
              type="submit"
              color="success"
              className="rounded-3 fw-bold shadow-sm d-flex align-items-center gap-1"
              disabled={submittingAssign}
            >
              {submittingAssign ? <Spinner size="sm" /> : <CheckCircle2 size={16} />}
              <span>Lưu Phân Công Tổ Trọng Tài</span>
            </Button>
          </ModalFooter>
        </form>
      </Modal>
    </div>
  );
}

