'use client';

import React, { useState, useEffect, useMemo, useRef } from 'react';
import Link from 'next/link';
import { useRouter, useSearchParams } from 'next/navigation';
import { useAuth, useToast } from '@/context/AuthContext';
import {
  FileText,
  Printer,
  ChevronLeft,
  CheckCircle2,
  Calendar,
  Clock,
  MapPin,
  Users,
  ShieldCheck,
  Award,
  AlertCircle,
  Flag,
  Flame,
  Check,
  Edit,
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
} from 'reactstrap';
import { giaiDauService } from '@/services/giaiDauService';
import { tranDauService } from '@/services/tranDauService';
import { GiaiDau, TrangThaiGiaiDau } from '@/types/giaiDau';

import { TranDau, MatchScoreDetails } from '@/types/tranDau';
import { refereeScoreStorage } from '@/utils/refereeScoreStorage';

export default function MatchReportOfficialPage() {
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
  const [selectedTranDauId, setSelectedTranDauId] = useState<number | ''>('');
  const [currentMatch, setCurrentMatch] = useState<TranDau | null>(null);

  const [loading, setLoading] = useState(true);
  const [loadingMatches, setLoadingMatches] = useState(false);

  // Dữ liệu biên bản chi tiết
  const [scoreDetails, setScoreDetails] = useState<MatchScoreDetails>({
    score1: 0,
    score2: 0,
    setScores: [],
    events: [],
  });

  // Chữ ký xác nhận
  const [signedReferee, setSignedReferee] = useState<{ name: string; signedAt: string } | null>(
    null
  );
  const [signedSecretary, setSignedSecretary] = useState<{ name: string; signedAt: string } | null>(
    null
  );
  const [signedTeam1, setSignedTeam1] = useState<{ name: string; signedAt: string } | null>(null);
  const [signedTeam2, setSignedTeam2] = useState<{ name: string; signedAt: string } | null>(null);

  // Modal ký tên
  const [signModalOpen, setSignModalOpen] = useState(false);
  const [signingRole, setSigningRole] = useState<'referee' | 'secretary' | 'team1' | 'team2'>('referee');
  const [signerName, setSignerName] = useState('');

  // 1. Tải danh mục giải đấu
  useEffect(() => {
    const fetchGiaiDaus = async () => {
      setLoading(true);
      try {
        let list: GiaiDau[] = [];
        try {
          list = (await giaiDauService.getAll()) || [];
        } catch {}
        if (!list || list.length === 0) {
          try {
            const paged = await giaiDauService.getPaged({ pageSize: 100 });
            if (paged?.items) list = paged.items;
          } catch {}
        }
        setGiaiDaus(list || []);

        if (queryGiaiId && list.some((g) => g.id === Number(queryGiaiId))) {
          setSelectedGiaiDauId(Number(queryGiaiId));
        } else if (list.length > 0) {
          const active = list.find((g) => g.trangThai === TrangThaiGiaiDau.DangDienRa || (g.trangThai as any) === 'DangDienRa') || list[0];
          setSelectedGiaiDauId(active.id);
        }

      } catch (err) {
        console.error(err);
        toast.error('Không thể tải danh sách giải đấu');
      } finally {
        setLoading(false);
      }
    };

    fetchGiaiDaus();
  }, [queryGiaiId, toast]);

  // 2. Tải danh sách trận đấu
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

        if (queryTranId && data.some((m) => m.id === Number(queryTranId))) {
          setSelectedTranDauId(Number(queryTranId));
        } else if (data.length > 0) {
          // Ưu tiên trận đã kết thúc hoặc đang diễn ra
          const finished = data.find((m) => m.trangThai === 'DaKetThuc') || data[0];
          setSelectedTranDauId(finished.id);
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

  // 3. Khi chọn trận, tải chi tiết biên bản & chữ ký
  useEffect(() => {
    if (!selectedTranDauId) {
      setCurrentMatch(null);
      return;
    }

    const found = matches.find((m) => m.id === Number(selectedTranDauId));
    if (found) {
      setCurrentMatch(found);
      const details = refereeScoreStorage.getScoreDetails(found);
      setScoreDetails(details);

      // Nạp chữ ký nếu đã có
      setSignedReferee(details.signedReferee || null);
      setSignedSecretary(details.signedSecretary || null);
      setSignedTeam1(details.signedTeam1 || null);
      setSignedTeam2(details.signedTeam2 || null);
    }
  }, [selectedTranDauId, matches]);

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
        year: 'numeric',
      });
    } catch {
      return val;
    }
  };

  // Mở modal ký
  const handleOpenSignModal = (role: 'referee' | 'secretary' | 'team1' | 'team2') => {
    setSigningRole(role);
    if (role === 'referee') {
      const refName =
        currentMatch?.danhSachTrongTai?.find((r) => r.vaiTro === 'TrongTaiChinh')?.tenTrongTai ||
        user?.fullName ||
        '';
      setSignerName(refName);
    } else if (role === 'secretary') {
      const secName =
        currentMatch?.danhSachTrongTai?.find((r) => r.vaiTro === 'TrongTaiBan' || r.vaiTro === 'ThuKy')
          ?.tenTrongTai ||
        user?.fullName ||
        '';
      setSignerName(secName);
    } else if (role === 'team1') {
      setSignerName(`Đại diện: ${currentMatch?.tenDoi1 || 'Đội 1'}`);
    } else {
      setSignerName(`Đại diện: ${currentMatch?.tenDoi2 || 'Đội 2'}`);
    }
    setSignModalOpen(true);
  };

  // Lưu chữ ký điện tử
  const handleConfirmSign = (e: React.FormEvent) => {
    e.preventDefault();
    if (!currentMatch) return;

    const signInfo = {
      name: signerName.trim() || 'Đã xác nhận',
      signedAt: new Date().toISOString(),
    };

    const updatedDetails = { ...scoreDetails };

    if (signingRole === 'referee') {
      setSignedReferee(signInfo);
      updatedDetails.signedReferee = signInfo;
    } else if (signingRole === 'secretary') {
      setSignedSecretary(signInfo);
      updatedDetails.signedSecretary = signInfo;
    } else if (signingRole === 'team1') {
      setSignedTeam1(signInfo);
      updatedDetails.signedTeam1 = signInfo;
    } else if (signingRole === 'team2') {
      setSignedTeam2(signInfo);
      updatedDetails.signedTeam2 = signInfo;
    }

    setScoreDetails(updatedDetails);
    refereeScoreStorage.saveScoreDetails(currentMatch.id, updatedDetails);
    setSignModalOpen(false);
    toast.success('Đã ký xác nhận biên bản trận đấu thành công!');
  };

  // In biên bản
  const handlePrintReport = () => {
    window.print();
  };

  // Phân tích tổ trọng tài
  const mainRef = currentMatch?.danhSachTrongTai?.find((r) => r.vaiTro === 'TrongTaiChinh');
  const as1 = currentMatch?.danhSachTrongTai?.find((r) => r.vaiTro === 'TroLy1' || r.vaiTro === 'TrongTaiPhu');
  const as2 = currentMatch?.danhSachTrongTai?.find((r) => r.vaiTro === 'TroLy2');
  const tableRef = currentMatch?.danhSachTrongTai?.find((r) => r.vaiTro === 'TrongTaiBan' || r.vaiTro === 'ThuKy');

  // Đội thắng cuộc
  const winnerText = useMemo(() => {
    if (scoreDetails.winner === 1) return currentMatch?.tenDoi1 || 'Đội 1';
    if (scoreDetails.winner === 2) return currentMatch?.tenDoi2 || 'Đội 2';
    if (scoreDetails.winner === 'draw') return 'HÒA';
    if (scoreDetails.score1 > scoreDetails.score2) return currentMatch?.tenDoi1 || 'Đội 1';
    if (scoreDetails.score2 > scoreDetails.score1) return currentMatch?.tenDoi2 || 'Đội 2';
    return 'HÒA';
  }, [scoreDetails, currentMatch]);

  return (
    <div className="d-flex flex-column gap-4 pb-5">
      {/* ── CSS TỐI ƯU HÓA CHO IN ẤN KHỔ A4 (PRINT STYLES) ── */}
      <style jsx global>{`
        @media print {
          /* Ẩn sidebar, topbar, navbar và các nút thao tác */
          .donvi-sidebar,
          .donvi-topbar,
          .no-print,
          .btn,
          header,
          aside,
          footer {
            display: none !important;
          }

          body,
          .donvi-layout-wrapper,
          .donvi-main-area,
          .donvi-content-container {
            margin: 0 !important;
            padding: 0 !important;
            background: #fff !important;
            color: #000 !important;
          }

          .print-container {
            width: 100% !important;
            max-width: 100% !important;
            box-shadow: none !important;
            border: none !important;
            padding: 0 !important;
          }

          .print-border {
            border: 2px solid #000 !important;
          }

          @page {
            size: A4 portrait;
            margin: 15mm 15mm 15mm 15mm;
          }
        }
      `}</style>

      {/* ── Header Navigation (Hidden on print) ── */}
      <div className="d-flex flex-wrap align-items-center justify-content-between gap-3 no-print">
        <div className="d-flex align-items-center gap-3">
          <Link
            href="/trong-tai"
            className="btn btn-outline-secondary btn-sm rounded-circle p-2 d-flex align-items-center justify-content-center"
            title="Quay lại danh sách phân công"
            style={{ width: '36px', height: '36px' }}
          >
            <ChevronLeft size={18} />
          </Link>
          <div>
            <h4 className="fw-bold text-dark mb-0 d-flex align-items-center gap-2">
              <FileText size={22} className="text-primary" />
              <span>Biên Bản Thi Đấu & Ký Xác Nhận Điện Tử</span>
            </h4>
            <small className="text-muted">
              Lập biên bản kết quả chính thức, ký số 4 bên và in ấn theo chuẩn mẫu quy định
            </small>
          </div>
        </div>

        {currentMatch && (
          <div className="d-flex align-items-center gap-2">
            <Link
              href={`/trong-tai/ket-qua?tranDauId=${currentMatch.id}&giaiDauId=${selectedGiaiDauId}`}
              className="btn btn-outline-danger btn-sm rounded-pill px-3 py-1.5 d-flex align-items-center gap-1.5 fw-semibold"
            >
              <Flame size={15} />
              <span>Ghi nhận lại tỷ số</span>
            </Link>

            <Button
              color="primary"
              size="sm"
              className="rounded-pill px-3.5 py-1.5 d-flex align-items-center gap-1.5 fw-bold shadow-sm"
              onClick={handlePrintReport}
            >
              <Printer size={15} />
              <span>In Biên Bản (A4)</span>
            </Button>
          </div>
        )}
      </div>

      {/* ── Bộ chọn giải & trận đấu (Hidden on print) ── */}
      <Card className="border-0 shadow-sm rounded-4 no-print">
        <CardBody className="p-3 p-md-4">
          <Row className="g-3">
            <Col xs={12} md={5}>
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

            <Col xs={12} md={7}>
              <FormGroup className="mb-0">
                <Label className="fw-semibold small text-secondary mb-1">
                  2. Chọn Trận Đấu Để Lập Biên Bản
                </Label>
                <Input
                  type="select"
                  className="rounded-3"
                  value={selectedTranDauId}
                  onChange={(e) => setSelectedTranDauId(e.target.value ? Number(e.target.value) : '')}
                  disabled={loadingMatches || matches.length === 0}
                >
                  {matches.length === 0 ? (
                    <option value="">-- Không có trận đấu nào --</option>
                  ) : (
                    matches.map((m) => (
                      <option key={m.id} value={m.id}>
                        [Trận #{m.soTran || m.id}] {m.tenMonTheThao} • {m.tenDoi1 || 'Đội 1'} vs{' '}
                        {m.tenDoi2 || 'Đội 2'} (
                        {m.trangThai === 'DaKetThuc'
                          ? 'Đã kết thúc'
                          : m.trangThai === 'DangDienRa'
                          ? 'Đang đấu'
                          : 'Chưa đấu'}
                        )
                      </option>
                    ))
                  )}
                </Input>
              </FormGroup>
            </Col>
          </Row>
        </CardBody>
      </Card>

      {!currentMatch ? (
        <Card className="border-0 shadow-sm rounded-4 text-center py-5 no-print">
          <CardBody>
            <FileText size={48} className="text-muted opacity-50 mb-3" />
            <h6 className="fw-bold text-dark">Chưa chọn trận đấu</h6>
            <p className="text-muted small mb-0">
              Vui lòng chọn giải đấu và trận đấu để hiển thị biên bản.
            </p>
          </CardBody>
        </Card>
      ) : (
        /* ── TỜ BIÊN BẢN CHÍNH THỨC CHUẨN IN ẤN A4 ── */
        <Card className="border-0 shadow-sm rounded-4 print-container bg-white">
          <CardBody className="p-4 p-md-5">
            {/* Header Quốc hiệu & Tiêu ngữ */}
            <div className="row align-items-start border-bottom pb-4 mb-4 text-dark">
              <div className="col-5 text-center">
                <div className="fw-bold text-uppercase" style={{ fontSize: '13px', letterSpacing: '0.5px' }}>
                  BAN TỔ CHỨC GIẢI THỂ THAO
                </div>
                <div className="fw-bold text-uppercase text-primary" style={{ fontSize: '12px' }}>
                  TỔ TRỌNG TÀI ĐIỀU HÀNH
                </div>
                <div className="small text-muted mt-1 font-monospace">
                  Mã trận: #{currentMatch.soTran || currentMatch.id}
                </div>
              </div>

              <div className="col-7 text-center">
                <div className="fw-bold text-uppercase" style={{ fontSize: '13px' }}>
                  CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM
                </div>
                <div className="fw-bold text-dark" style={{ fontSize: '13px' }}>
                  Độc lập - Tự do - Hạnh phúc
                </div>
                <div className="text-muted small">──────── • ────────</div>
              </div>
            </div>

            {/* Tiêu đề biên bản */}
            <div className="text-center mb-4">
              <h3 className="fw-black text-uppercase text-dark mb-1" style={{ letterSpacing: '1px' }}>
                BIÊN BẢN THI ĐẤU CHÍNH THỨC
              </h3>
              <div className="fw-bold text-primary fs-6 text-uppercase">
                GIẢI ĐẤU: {currentMatch.tenGiaiDau || 'GIẢI THỂ THAO'}
              </div>
              <div className="text-muted small mt-1">
                (Ban hành kèm theo Luật thi đấu & Điều lệ giải)
              </div>
            </div>

            {/* I. THÔNG TIN CHUNG */}
            <div className="mb-4">
              <h6 className="fw-bold text-dark text-uppercase mb-2 pb-1 border-bottom d-flex align-items-center gap-2">
                <Award size={16} className="text-primary" />
                <span>I. THÔNG TIN CHUNG TRẬN ĐẤU</span>
              </h6>

              <Table bordered size="sm" className="mb-0" style={{ fontSize: '13px' }}>
                <tbody>
                  <tr>
                    <td className="bg-light fw-semibold" style={{ width: '22%' }}>
                      Môn thi đấu:
                    </td>
                    <td className="fw-bold text-primary" style={{ width: '28%' }}>
                      {currentMatch.tenMonTheThao || '—'}
                    </td>
                    <td className="bg-light fw-semibold" style={{ width: '22%' }}>
                      Vòng thi / Bảng đấu:
                    </td>
                    <td style={{ width: '28%' }}>
                      {currentMatch.tenVongDau || 'Vòng bảng'}
                      {currentMatch.tenBangDau ? ` • ${currentMatch.tenBangDau}` : ''}
                    </td>
                  </tr>
                  <tr>
                    <td className="bg-light fw-semibold">Địa điểm / Sân:</td>
                    <td>
                      {currentMatch.tenSanDau || 'Sân tiêu chuẩn'}
                      {currentMatch.tenCumSan ? ` (${currentMatch.tenCumSan})` : ''}
                    </td>
                    <td className="bg-light fw-semibold">Lịch thi đấu dự kiến:</td>
                    <td>
                      {formatDateTime(currentMatch.thoiGianBatDau || currentMatch.thoiGianDuKien)}
                    </td>
                  </tr>
                  <tr>
                    <td className="bg-light fw-semibold">Đội 1 (Vị trí 1):</td>
                    <td className="fw-bold">
                      {currentMatch.tenDoi1 || 'Đội 1'}{' '}
                      {currentMatch.donViDoi1 ? `(${currentMatch.donViDoi1})` : ''}
                    </td>
                    <td className="bg-light fw-semibold">Đội 2 (Vị trí 2):</td>
                    <td className="fw-bold">
                      {currentMatch.tenDoi2 || 'Đội 2'}{' '}
                      {currentMatch.donViDoi2 ? `(${currentMatch.donViDoi2})` : ''}
                    </td>
                  </tr>
                  {/* Thông tin hoàn thiện thực tế */}
                  {(scoreDetails.actualStartTime || scoreDetails.weatherCondition || scoreDetails.durationMinutes) && (
                    <>
                      <tr>
                        <td className="bg-light fw-semibold">Giờ thi đấu thực tế:</td>
                        <td>
                          {scoreDetails.actualStartTime ? formatDateTime(scoreDetails.actualStartTime) : '—'}
                          {scoreDetails.actualEndTime ? ` đến ${formatDateTime(scoreDetails.actualEndTime)}` : ''}
                        </td>
                        <td className="bg-light fw-semibold">Thời lượng & Bù giờ:</td>
                        <td>
                          {scoreDetails.durationMinutes ? `${scoreDetails.durationMinutes} phút` : 'Theo điều lệ'}
                          {scoreDetails.extraTimeMinutes ? ` (Bù giờ: ${scoreDetails.extraTimeMinutes} phút)` : ''}
                        </td>
                      </tr>
                      <tr>
                        <td className="bg-light fw-semibold">Thời tiết & Mặt sân:</td>
                        <td>
                          {scoreDetails.weatherCondition || 'Thuận lợi'} • {scoreDetails.pitchCondition || 'Mặt sân đạt chuẩn'}
                        </td>
                        <td className="bg-light fw-semibold">Khán giả dự khán:</td>
                        <td>
                          {scoreDetails.spectatorCount ? `${scoreDetails.spectatorCount.toLocaleString('vi-VN')} người` : '—'}
                        </td>
                      </tr>
                    </>
                  )}
                </tbody>
              </Table>
            </div>

            {/* II. TỔ TRỌNG TÀI ĐIỀU HÀNH */}
            <div className="mb-4">
              <h6 className="fw-bold text-dark text-uppercase mb-2 pb-1 border-bottom d-flex align-items-center gap-2">
                <Users size={16} className="text-success" />
                <span>II. THÀNH PHẦN TỔ TRỌNG TÀI ĐIỀU HÀNH</span>
              </h6>

              <Table bordered size="sm" className="mb-0" style={{ fontSize: '13px' }}>
                <thead className="table-light text-secondary">
                  <tr>
                    <th style={{ width: '25%' }}>VAI TRÒ</th>
                    <th style={{ width: '40%' }}>HỌ VÀ TÊN TRỌNG TÀI</th>
                    <th style={{ width: '20%' }}>CẤP BẬC / CHỨNG CHỈ</th>
                    <th style={{ width: '15%' }}>SỐ ĐIỆN THOẠI</th>
                  </tr>
                </thead>
                <tbody>
                  <tr>
                    <td className="fw-bold text-dark">Trọng tài chính:</td>
                    <td className="fw-semibold text-primary">
                      {mainRef?.tenTrongTai || '—'}
                    </td>
                    <td>{mainRef?.capBac || 'Quốc gia'}</td>
                    <td>{mainRef?.soDienThoai || '—'}</td>
                  </tr>
                  <tr>
                    <td className="fw-bold text-dark">Trợ lý trọng tài 1:</td>
                    <td>{as1?.tenTrongTai || '—'}</td>
                    <td>{as1?.capBac || 'Cấp tỉnh'}</td>
                    <td>{as1?.soDienThoai || '—'}</td>
                  </tr>
                  <tr>
                    <td className="fw-bold text-dark">Trợ lý trọng tài 2:</td>
                    <td>{as2?.tenTrongTai || '—'}</td>
                    <td>{as2?.capBac || 'Cấp tỉnh'}</td>
                    <td>{as2?.soDienThoai || '—'}</td>
                  </tr>
                  <tr>
                    <td className="fw-bold text-dark">Trọng tài bàn / Thư ký:</td>
                    <td>{tableRef?.tenTrongTai || '—'}</td>
                    <td>{tableRef?.capBac || 'Thư ký'}</td>
                    <td>{tableRef?.soDienThoai || '—'}</td>
                  </tr>
                </tbody>
              </Table>
            </div>

            {/* III. KẾT QUẢ THI ĐẤU CHÍNH THỨC */}
            <div className="mb-4">
              <h6 className="fw-bold text-dark text-uppercase mb-2 pb-1 border-bottom d-flex align-items-center gap-2">
                <Award size={16} className="text-danger" />
                <span>III. KẾT QUẢ THI ĐẤU CHÍNH THỨC</span>
              </h6>

              {/* Tỷ số Box */}
              <div
                className="p-3 rounded-3 mb-3 text-center border"
                style={{ backgroundColor: '#f8fafc' }}
              >
                <div className="row align-items-center">
                  <div className="col-5 text-end">
                    <span className="fw-bold fs-5 text-dark">
                      {currentMatch.tenDoi1 || 'Đội 1'}
                    </span>
                    {currentMatch.donViDoi1 && (
                      <small className="text-muted d-block">{currentMatch.donViDoi1}</small>
                    )}
                  </div>

                  <div className="col-2">
                    <div className="fw-black fs-2 text-danger font-monospace">
                      {scoreDetails.score1} - {scoreDetails.score2}
                    </div>
                    <small className="text-muted fw-bold">CHUNG CUỘC</small>
                  </div>

                  <div className="col-5 text-start">
                    <span className="fw-bold fs-5 text-dark">
                      {currentMatch.tenDoi2 || 'Đội 2'}
                    </span>
                    {currentMatch.donViDoi2 && (
                      <small className="text-muted d-block">{currentMatch.donViDoi2}</small>
                    )}
                  </div>
                </div>
              </div>

              {/* Tỷ số từng hiệp */}
              {scoreDetails.setScores && scoreDetails.setScores.length > 0 && (
                <div className="table-responsive mb-3">
                  <Table bordered size="sm" className="text-center mb-0" style={{ fontSize: '13px' }}>
                    <thead className="table-light text-secondary">
                      <tr>
                        <th>ĐỘI THI ĐẤU</th>
                        {scoreDetails.setScores.map((s) => (
                          <th key={s.setNumber}>HIỆP {s.setNumber}</th>
                        ))}
                        <th className="table-warning">TỔNG</th>
                      </tr>
                    </thead>
                    <tbody>
                      <tr>
                        <td className="fw-bold text-start ps-3">{currentMatch.tenDoi1 || 'Đội 1'}</td>
                        {scoreDetails.setScores.map((s) => (
                          <td key={s.setNumber} className="fw-semibold">
                            {s.score1}
                          </td>
                        ))}
                        <td className="table-warning fw-bold text-danger">{scoreDetails.score1}</td>
                      </tr>
                      <tr>
                        <td className="fw-bold text-start ps-3">{currentMatch.tenDoi2 || 'Đội 2'}</td>
                        {scoreDetails.setScores.map((s) => (
                          <td key={s.setNumber} className="fw-semibold">
                            {s.score2}
                          </td>
                        ))}
                        <td className="table-warning fw-bold text-danger">{scoreDetails.score2}</td>
                      </tr>
                    </tbody>
                  </Table>
                </div>
              )}

              {/* Đội thắng cuộc & MVP */}
              <div className="alert alert-success d-flex flex-wrap align-items-center justify-content-between gap-2 mb-0 py-2.5 px-3">
                <div className="d-flex align-items-center gap-2">
                  <CheckCircle2 size={18} className="text-success flex-shrink-0" />
                  <span style={{ fontSize: '13.5px' }}>
                    KẾT LUẬN THẮNG CUỘC:{' '}
                    <strong className="text-uppercase text-success-emphasis fs-6">
                      {winnerText}
                    </strong>{' '}
                    <span>
                      (Hình thức:{' '}
                      {scoreDetails.winMethod === 'extra_time'
                        ? 'Hiệp phụ'
                        : scoreDetails.winMethod === 'penalties'
                        ? 'Luân lưu / Phạt đền 11m'
                        : scoreDetails.winMethod === 'walkover'
                        ? 'Đối thủ bỏ cuộc (Walkover)'
                        : scoreDetails.winMethod === 'disqualification'
                        ? 'Truất quyền thi đấu'
                        : 'Thời gian thi đấu chính thức'}
                      ).
                    </span>
                  </span>
                </div>
                {scoreDetails.mvpAthlete && (
                  <div className="d-flex align-items-center gap-1.5 bg-white px-2.5 py-1 rounded-pill border border-success-subtle shadow-sm">
                    <Award size={14} className="text-warning" />
                    <span className="small fw-semibold text-dark">
                      MVP: <span className="text-primary">{scoreDetails.mvpAthlete}</span>
                    </span>
                  </div>
                )}
              </div>
            </div>

            {/* IV. TÌNH HÌNH KỶ LUẬT & SỰ KIỆN TRẬN ĐẤU */}
            <div className="mb-4">
              <h6 className="fw-bold text-dark text-uppercase mb-2 pb-1 border-bottom d-flex align-items-center gap-2">
                <Flag size={16} className="text-warning" />
                <span>IV. TÌNH HÌNH KỶ LUẬT & DIỄN BIẾN SỰ KIỆN TRẬN ĐẤU</span>
              </h6>

              {scoreDetails.events && scoreDetails.events.length > 0 ? (
                <Table bordered size="sm" className="mb-0" style={{ fontSize: '12.5px' }}>
                  <thead className="table-light text-secondary">
                    <tr>
                      <th style={{ width: '70px' }}>PHÚT</th>
                      <th style={{ width: '130px' }}>HÌNH THỨC</th>
                      <th>ĐỘI / ĐOÀN</th>
                      <th>VẬN ĐỘNG VIÊN</th>
                      <th>HÀNH VI / GHI CHÚ DIỄN BIẾN</th>
                    </tr>
                  </thead>
                  <tbody>
                    {scoreDetails.events.map((ev) => (
                      <tr key={ev.id}>
                        <td className="text-center font-monospace fw-bold">{ev.minute}&apos;</td>
                        <td>
                          {ev.type === 'yellow_card' ? (
                            <span className="text-warning fw-bold">🟨 Thẻ vàng</span>
                          ) : ev.type === 'red_card' ? (
                            <span className="text-danger fw-bold">🟥 Thẻ đỏ</span>
                          ) : ev.type === 'goal' ? (
                            <span className="text-success fw-bold">⚽ Bàn thắng</span>
                          ) : ev.type === 'point' ? (
                            <span className="text-primary fw-bold">🎯 Ghi điểm</span>
                          ) : ev.type === 'substitution' ? (
                            <span className="text-info fw-bold">🔄 Thay người</span>
                          ) : ev.type === 'penalty' ? (
                            <span className="text-danger fw-bold">⚡ Phạt đền</span>
                          ) : ev.type === 'injury' ? (
                            <span className="text-secondary fw-bold">🚑 Chấn thương</span>
                          ) : ev.type === 'incident' ? (
                            <span className="text-danger fw-bold">⚠️ Sự cố sân</span>
                          ) : (
                            <span className="fw-semibold">{ev.type}</span>
                          )}
                        </td>
                        <td className="fw-semibold">
                          {ev.team === 1
                            ? currentMatch.tenDoi1 || 'Đội 1'
                            : currentMatch.tenDoi2 || 'Đội 2'}
                        </td>
                        <td>
                          {ev.athleteName || '—'}
                          {ev.assistName ? <span className="text-muted small d-block">KT: {ev.assistName}</span> : ''}
                        </td>
                        <td>{ev.details || 'Theo quy chế điều hành giải'}</td>
                      </tr>
                    ))}
                  </tbody>
                </Table>
              ) : (
                <div className="p-2 border rounded-2 text-muted small fst-italic">
                  Trận đấu diễn ra an toàn, đúng điều lệ, không có trường hợp thẻ phạt hoặc vi phạm kỷ luật nghiêm trọng.
                </div>
              )}
            </div>

            {/* V. BÁO CÁO & ĐÁNH GIÁ CHUYÊN MÔN */}
            <div className="mb-4">
              <h6 className="fw-bold text-dark text-uppercase mb-2 pb-1 border-bottom d-flex align-items-center gap-2">
                <ShieldCheck size={16} className="text-primary" />
                <span>V. BÁO CÁO & ĐÁNH GIÁ CHUYÊN MÔN CỦA TỔ TRỌNG TÀI & GIÁM SÁT</span>
              </h6>

              <div className="row g-3">
                <div className="col-12 col-md-6">
                  <div className="p-3 border rounded-3 text-dark bg-light h-100" style={{ fontSize: '13px' }}>
                    <div className="fw-bold text-primary mb-1 small text-uppercase">1. Nhận xét của Tổ trọng tài điều hành:</div>
                    <div>
                      {scoreDetails.refereeNotes ||
                        scoreDetails.notes ||
                        currentMatch.ghiChu ||
                        'Hai đội chấp hành nghiêm chỉnh luật thi đấu và điều lệ của Ban Tổ Chức. Không có khiếu nại phát sinh.'}
                    </div>
                  </div>
                </div>

                <div className="col-12 col-md-6">
                  <div className="p-3 border rounded-3 text-dark bg-light h-100" style={{ fontSize: '13px' }}>
                    <div className="fw-bold text-success mb-1 small text-uppercase">2. Đánh giá của Giám sát trận đấu:</div>
                    <div>
                      {scoreDetails.supervisorNotes ||
                        'Công tác tổ chức, sân bãi, an ninh và y tế được bảo đảm đúng tiêu chuẩn của giải đấu.'}
                    </div>
                  </div>
                </div>
              </div>

              {scoreDetails.isFinalized && (
                <div className="alert alert-primary d-flex align-items-center justify-content-between mt-3 mb-0 py-2 px-3 small border-primary-subtle">
                  <div className="d-flex align-items-center gap-2">
                    <CheckCircle2 size={16} className="text-primary" />
                    <span>
                      Hồ sơ trận đấu đã được <strong>Chốt & Khóa hoàn thiện chính thức</strong> lúc{' '}
                      {formatDateTime(scoreDetails.finalizedAt)}.
                    </span>
                  </div>
                  <span className="badge bg-primary text-white font-monospace">OFFICIAL FINALIZED</span>
                </div>
              )}
            </div>

            {/* VI. CHỮ KÝ XÁC NHẬN 4 BÊN */}
            <div className="pt-2 border-top">
              <div className="text-end text-muted small fst-italic mb-4">
                Biên bản lập xong lúc{' '}
                {new Date().toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' })}, ngày{' '}
                {new Date().toLocaleDateString('vi-VN')}
              </div>

              <div className="row text-center g-3 text-dark">
                {/* Đại diện Đội 1 */}
                <div className="col-3">
                  <div className="fw-bold text-uppercase small">ĐẠI DIỆN ĐỘI 1</div>
                  <div className="text-muted" style={{ fontSize: '11px' }}>
                    (Ký, ghi rõ họ tên)
                  </div>
                  <div
                    className="d-flex flex-column align-items-center justify-content-center my-3"
                    style={{ minHeight: '75px' }}
                  >
                    {signedTeam1 ? (
                      <div className="text-success">
                        <CheckCircle2 size={24} className="mb-1" />
                        <div className="fw-bold small">{signedTeam1.name}</div>
                        <div className="text-muted" style={{ fontSize: '10px' }}>
                          {formatDateTime(signedTeam1.signedAt)}
                        </div>
                      </div>
                    ) : (
                      <Button
                        color="outline-secondary"
                        size="sm"
                        className="rounded-pill px-3 py-1 no-print"
                        style={{ fontSize: '11px' }}
                        onClick={() => handleOpenSignModal('team1')}
                      >
                        Ký xác nhận
                      </Button>
                    )}
                  </div>
                  <div className="fw-semibold small border-top pt-1 text-truncate">
                    {currentMatch.tenDoi1 || 'Đội 1'}
                  </div>
                </div>

                {/* Đại diện Đội 2 */}
                <div className="col-3">
                  <div className="fw-bold text-uppercase small">ĐẠI DIỆN ĐỘI 2</div>
                  <div className="text-muted" style={{ fontSize: '11px' }}>
                    (Ký, ghi rõ họ tên)
                  </div>
                  <div
                    className="d-flex flex-column align-items-center justify-content-center my-3"
                    style={{ minHeight: '75px' }}
                  >
                    {signedTeam2 ? (
                      <div className="text-success">
                        <CheckCircle2 size={24} className="mb-1" />
                        <div className="fw-bold small">{signedTeam2.name}</div>
                        <div className="text-muted" style={{ fontSize: '10px' }}>
                          {formatDateTime(signedTeam2.signedAt)}
                        </div>
                      </div>
                    ) : (
                      <Button
                        color="outline-secondary"
                        size="sm"
                        className="rounded-pill px-3 py-1 no-print"
                        style={{ fontSize: '11px' }}
                        onClick={() => handleOpenSignModal('team2')}
                      >
                        Ký xác nhận
                      </Button>
                    )}
                  </div>
                  <div className="fw-semibold small border-top pt-1 text-truncate">
                    {currentMatch.tenDoi2 || 'Đội 2'}
                  </div>
                </div>

                {/* Thư ký trận đấu */}
                <div className="col-3">
                  <div className="fw-bold text-uppercase small">THƯ KÝ TRẬN ĐẤU</div>
                  <div className="text-muted" style={{ fontSize: '11px' }}>
                    (Ký, ghi rõ họ tên)
                  </div>
                  <div
                    className="d-flex flex-column align-items-center justify-content-center my-3"
                    style={{ minHeight: '75px' }}
                  >
                    {signedSecretary ? (
                      <div className="text-success">
                        <CheckCircle2 size={24} className="mb-1" />
                        <div className="fw-bold small">{signedSecretary.name}</div>
                        <div className="text-muted" style={{ fontSize: '10px' }}>
                          {formatDateTime(signedSecretary.signedAt)}
                        </div>
                      </div>
                    ) : (
                      <Button
                        color="outline-secondary"
                        size="sm"
                        className="rounded-pill px-3 py-1 no-print"
                        style={{ fontSize: '11px' }}
                        onClick={() => handleOpenSignModal('secretary')}
                      >
                        Ký xác nhận
                      </Button>
                    )}
                  </div>
                  <div className="fw-semibold small border-top pt-1 text-truncate">
                    {tableRef?.tenTrongTai || user?.fullName || 'Thư ký'}
                  </div>
                </div>

                {/* Trọng tài chính */}
                <div className="col-3">
                  <div className="fw-bold text-uppercase small text-danger">TRỌNG TÀI CHÍNH</div>
                  <div className="text-muted" style={{ fontSize: '11px' }}>
                    (Ký, ghi rõ họ tên)
                  </div>
                  <div
                    className="d-flex flex-column align-items-center justify-content-center my-3"
                    style={{ minHeight: '75px' }}
                  >
                    {signedReferee ? (
                      <div className="text-success">
                        <CheckCircle2 size={24} className="mb-1" />
                        <div className="fw-bold small">{signedReferee.name}</div>
                        <div className="text-muted" style={{ fontSize: '10px' }}>
                          {formatDateTime(signedReferee.signedAt)}
                        </div>
                      </div>
                    ) : (
                      <Button
                        color="danger"
                        size="sm"
                        className="rounded-pill px-3 py-1 fw-bold no-print shadow-sm"
                        style={{ fontSize: '11px' }}
                        onClick={() => handleOpenSignModal('referee')}
                      >
                        Ký xác nhận
                      </Button>
                    )}
                  </div>
                  <div className="fw-bold small border-top pt-1 text-truncate text-primary">
                    {mainRef?.tenTrongTai || user?.fullName || 'Trọng tài chính'}
                  </div>
                </div>
              </div>
            </div>
          </CardBody>
        </Card>
      )}

      {/* ── Modal Ký Xác Nhận Điện Tử ── */}
      <Modal isOpen={signModalOpen} toggle={() => setSignModalOpen(false)} centered>
        <form onSubmit={handleConfirmSign}>
          <ModalHeader toggle={() => setSignModalOpen(false)}>
            <div className="d-flex align-items-center gap-2">
              <CheckCircle2 size={18} className="text-success" />
              <span>Xác Nhận Ký Số Biên Bản Trận Đấu</span>
            </div>
          </ModalHeader>
          <ModalBody className="p-4">
            <div className="alert alert-info py-2 px-3 small mb-3">
              Bạn đang thực hiện ký xác nhận với vai trò:{' '}
              <strong className="text-uppercase">
                {signingRole === 'referee'
                  ? 'Trọng tài chính'
                  : signingRole === 'secretary'
                  ? 'Thư ký trận đấu'
                  : signingRole === 'team1'
                  ? `Đại diện ${currentMatch?.tenDoi1 || 'Đội 1'}`
                  : `Đại diện ${currentMatch?.tenDoi2 || 'Đội 2'}`}
              </strong>
            </div>

            <FormGroup className="mb-0">
              <Label className="fw-semibold small">Họ và tên người ký xác nhận</Label>
              <Input
                type="text"
                className="rounded-3"
                value={signerName}
                onChange={(e) => setSignerName(e.target.value)}
                placeholder="Nhập họ và tên..."
                required
              />
            </FormGroup>
          </ModalBody>
          <ModalFooter>
            <Button color="light" className="rounded-3" onClick={() => setSignModalOpen(false)}>
              Hủy
            </Button>
            <Button color="success" type="submit" className="rounded-3 fw-bold shadow-sm">
              Xác Nhận Ký Biên Bản
            </Button>
          </ModalFooter>
        </form>
      </Modal>
    </div>
  );
}
