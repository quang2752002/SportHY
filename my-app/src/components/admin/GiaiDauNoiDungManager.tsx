'use client';

import React, { useState, useMemo } from 'react';
import {
  Card,
  CardBody,
  Button,
  Badge,
  Table,
  Modal,
  ModalHeader,
  ModalBody,
  ModalFooter,
  FormGroup,
  Label,
  Input,
  Row,
  Col,
  Spinner,
} from 'reactstrap';
import { MonTheThao } from '@/types';
import { NoiDungThiDau } from '@/types/noiDungThiDau';
import { HinhThucThiDau, HinhThucThiDauLabels } from '@/types/hinhThucThiDau';
import { noiDungThiDauService } from '@/services';
import { useToast } from '@/context/AuthContext';

export interface NoiDungThiDauFormItem {
  id?: number; // > 0 nếu đã lưu trong DB, undefined nếu là bản nháp
  tempId?: string; // Khóa tạm thời cho draft
  monTheThaoId: number;
  giaiDauMonTheThaoId?: number;
  tenMonTheThao?: string;
  ma: string;
  ten: string;
  gioiTinh: string; // 'Nam' | 'Nu' | 'HonHop'
  loaiThiDau: string; // 'CaNhan' | 'DongDoi'
  hinhThucThiDau?: string; // HinhThucThiDau
  soLuongToiThieu: number;
  soLuongToiDa: number;
  moTa?: string;
  trangThai: boolean;
}

interface Props {
  giaiDauId?: number;
  selectedMonIds: number[];
  availableMons: MonTheThao[];
  items: NoiDungThiDauFormItem[];
  onChange: (items: NoiDungThiDauFormItem[]) => void;
  readOnly?: boolean;
}

// Gợi ý nội dung thông minh theo tên môn
function getSmartPresets(monTen: string) {
  const t = (monTen || '').toLowerCase();

  if (t.includes('cầu lông') || t.includes('badminton')) {
    return [
      { ten: 'Đơn nam', gioiTinh: 'Nam', loaiThiDau: 'CaNhan', min: 1, max: 1 },
      { ten: 'Đơn nữ', gioiTinh: 'Nu', loaiThiDau: 'CaNhan', min: 1, max: 1 },
      { ten: 'Đôi nam', gioiTinh: 'Nam', loaiThiDau: 'DongDoi', min: 2, max: 2 },
      { ten: 'Đôi nữ', gioiTinh: 'Nu', loaiThiDau: 'DongDoi', min: 2, max: 2 },
      { ten: 'Đôi nam nữ', gioiTinh: 'HonHop', loaiThiDau: 'DongDoi', min: 2, max: 2 },
    ];
  }
  if (t.includes('bóng bàn') || t.includes('table tennis')) {
    return [
      { ten: 'Đơn nam', gioiTinh: 'Nam', loaiThiDau: 'CaNhan', min: 1, max: 1 },
      { ten: 'Đơn nữ', gioiTinh: 'Nu', loaiThiDau: 'CaNhan', min: 1, max: 1 },
      { ten: 'Đôi nam', gioiTinh: 'Nam', loaiThiDau: 'DongDoi', min: 2, max: 2 },
      { ten: 'Đôi nữ', gioiTinh: 'Nu', loaiThiDau: 'DongDoi', min: 2, max: 2 },
      { ten: 'Đôi nam nữ', gioiTinh: 'HonHop', loaiThiDau: 'DongDoi', min: 2, max: 2 },
    ];
  }
  if (t.includes('tennis') || t.includes('quần vợt') || t.includes('pickleball')) {
    return [
      { ten: 'Đơn nam', gioiTinh: 'Nam', loaiThiDau: 'CaNhan', min: 1, max: 1 },
      { ten: 'Đơn nữ', gioiTinh: 'Nu', loaiThiDau: 'CaNhan', min: 1, max: 1 },
      { ten: 'Đôi nam', gioiTinh: 'Nam', loaiThiDau: 'DongDoi', min: 2, max: 2 },
      { ten: 'Đôi nam nữ', gioiTinh: 'HonHop', loaiThiDau: 'DongDoi', min: 2, max: 2 },
    ];
  }
  if (t.includes('bóng đá') || t.includes('football') || t.includes('futsal')) {
    return [
      { ten: 'Bóng đá nam 5 người', gioiTinh: 'Nam', loaiThiDau: 'DongDoi', min: 5, max: 12 },
      { ten: 'Bóng đá nam 7 người', gioiTinh: 'Nam', loaiThiDau: 'DongDoi', min: 7, max: 14 },
      { ten: 'Bóng đá nữ 5 người', gioiTinh: 'Nu', loaiThiDau: 'DongDoi', min: 5, max: 12 },
      { ten: 'Bóng đá nữ 7 người', gioiTinh: 'Nu', loaiThiDau: 'DongDoi', min: 7, max: 14 },
    ];
  }
  if (t.includes('bóng chuyền') || t.includes('volleyball')) {
    return [
      { ten: 'Bóng chuyền nam', gioiTinh: 'Nam', loaiThiDau: 'DongDoi', min: 6, max: 12 },
      { ten: 'Bóng chuyền nữ', gioiTinh: 'Nu', loaiThiDau: 'DongDoi', min: 6, max: 12 },
      { ten: 'Bóng chuyền hơi nam', gioiTinh: 'Nam', loaiThiDau: 'DongDoi', min: 5, max: 10 },
      { ten: 'Bóng chuyền hơi nữ', gioiTinh: 'Nu', loaiThiDau: 'DongDoi', min: 5, max: 10 },
    ];
  }
  if (t.includes('bóng rổ') || t.includes('basketball')) {
    return [
      { ten: 'Bóng rổ nam 5x5', gioiTinh: 'Nam', loaiThiDau: 'DongDoi', min: 5, max: 12 },
      { ten: 'Bóng rổ nam 3x3', gioiTinh: 'Nam', loaiThiDau: 'DongDoi', min: 3, max: 4 },
      { ten: 'Bóng rổ nữ 5x5', gioiTinh: 'Nu', loaiThiDau: 'DongDoi', min: 5, max: 12 },
      { ten: 'Bóng rổ nữ 3x3', gioiTinh: 'Nu', loaiThiDau: 'DongDoi', min: 3, max: 4 },
    ];
  }
  if (t.includes('cờ vua') || t.includes('cờ tướng') || t.includes('chess')) {
    return [
      { ten: 'Cá nhân nam', gioiTinh: 'Nam', loaiThiDau: 'CaNhan', min: 1, max: 1 },
      { ten: 'Cá nhân nữ', gioiTinh: 'Nu', loaiThiDau: 'CaNhan', min: 1, max: 1 },
      { ten: 'Đồng đội nam', gioiTinh: 'Nam', loaiThiDau: 'DongDoi', min: 3, max: 4 },
      { ten: 'Đồng đội nữ', gioiTinh: 'Nu', loaiThiDau: 'DongDoi', min: 3, max: 4 },
    ];
  }
  if (t.includes('kéo co')) {
    return [
      { ten: 'Kéo co nam', gioiTinh: 'Nam', loaiThiDau: 'DongDoi', min: 8, max: 10 },
      { ten: 'Kéo co nữ', gioiTinh: 'Nu', loaiThiDau: 'DongDoi', min: 8, max: 10 },
      { ten: 'Kéo co phối hợp nam nữ', gioiTinh: 'HonHop', loaiThiDau: 'DongDoi', min: 8, max: 10 },
    ];
  }
  if (t.includes('điền kinh') || t.includes('chạy') || t.includes('bơi')) {
    return [
      { ten: 'Cá nhân nam', gioiTinh: 'Nam', loaiThiDau: 'CaNhan', min: 1, max: 1 },
      { ten: 'Cá nhân nữ', gioiTinh: 'Nu', loaiThiDau: 'CaNhan', min: 1, max: 1 },
      { ten: 'Tiếp sức nam', gioiTinh: 'Nam', loaiThiDau: 'DongDoi', min: 4, max: 4 },
      { ten: 'Tiếp sức nữ', gioiTinh: 'Nu', loaiThiDau: 'DongDoi', min: 4, max: 4 },
    ];
  }

  // Mặc định cho môn khác
  return [
    { ten: 'Cá nhân nam', gioiTinh: 'Nam', loaiThiDau: 'CaNhan', min: 1, max: 1 },
    { ten: 'Cá nhân nữ', gioiTinh: 'Nu', loaiThiDau: 'CaNhan', min: 1, max: 1 },
    { ten: 'Đồng đội nam', gioiTinh: 'Nam', loaiThiDau: 'DongDoi', min: 2, max: 10 },
    { ten: 'Đồng đội nữ', gioiTinh: 'Nu', loaiThiDau: 'DongDoi', min: 2, max: 10 },
  ];
}

export default function GiaiDauNoiDungManager({
  giaiDauId,
  selectedMonIds,
  availableMons,
  items,
  onChange,
  readOnly = false,
}: Props) {
  const toast = useToast();

  // Môn thể thao đang chọn để xem/cấu hình nội dung (active tab)
  const [activeMonId, setActiveMonId] = useState<number | null>(null);

  // Modal thêm / sửa nội dung
  const [modalOpen, setModalOpen] = useState(false);
  const [editingItem, setEditingItem] = useState<NoiDungThiDauFormItem | null>(null);
  const [modalMonId, setModalMonId] = useState<number>(0);
  const [formData, setFormData] = useState<NoiDungThiDauFormItem>({
    monTheThaoId: 0,
    ma: '',
    ten: '',
    gioiTinh: 'Nam',
    loaiThiDau: 'CaNhan',
    soLuongToiThieu: 1,
    soLuongToiDa: 1,
    moTa: '',
    trangThai: true,
  });
  const [savingApi, setSavingApi] = useState(false);

  // Modal xác nhận xóa nội dung
  const [deleteModalOpen, setDeleteModalOpen] = useState(false);
  const [itemToDelete, setItemToDelete] = useState<NoiDungThiDauFormItem | null>(null);

  // Danh sách các môn thể thao đã được tích chọn
  const selectedMons = useMemo(() => {
    return availableMons.filter((m) => selectedMonIds.includes(m.id));
  }, [availableMons, selectedMonIds]);

  // Tự động chọn tab đầu tiên nếu tab hiện tại không nằm trong danh sách đã chọn
  React.useEffect(() => {
    if (selectedMons.length > 0) {
      if (!activeMonId || !selectedMonIds.includes(activeMonId)) {
        setActiveMonId(selectedMons[0].id);
      }
    } else {
      setActiveMonId(null);
    }
  }, [selectedMons, selectedMonIds, activeMonId]);

  // Các nội dung của môn đang active
  const activeMonItems = useMemo(() => {
    if (!activeMonId) return [];
    return items.filter((x) => x.monTheThaoId === activeMonId);
  }, [items, activeMonId]);

  // Thông tin môn đang active
  const activeMon = useMemo(() => {
    return availableMons.find((m) => m.id === activeMonId);
  }, [availableMons, activeMonId]);

  // Đếm số lượng nội dung của một môn
  const countForMon = (monId: number) => {
    return items.filter((x) => x.monTheThaoId === monId).length;
  };

  // Mở modal thêm mới nội dung cho 1 môn
  const handleOpenAddModal = (monId: number) => {
    const mon = availableMons.find((m) => m.id === monId);
    setModalMonId(monId);
    setEditingItem(null);
    setFormData({
      monTheThaoId: monId,
      tenMonTheThao: mon?.ten || '',
      ma: '',
      ten: '',
      gioiTinh: 'Nam',
      loaiThiDau: 'CaNhan',
      hinhThucThiDau: mon?.hinhThucThiDau || 'LoaiTrucTiep',
      soLuongToiThieu: 1,
      soLuongToiDa: 1,
      moTa: '',
      trangThai: true,
    });
    setModalOpen(true);
  };

  // Mở modal sửa nội dung
  const handleOpenEditModal = (item: NoiDungThiDauFormItem) => {
    const mon = availableMons.find((m) => m.id === item.monTheThaoId);
    setModalMonId(item.monTheThaoId);
    setEditingItem(item);
    setFormData({
      ...item,
      hinhThucThiDau: item.hinhThucThiDau || mon?.hinhThucThiDau || 'LoaiTrucTiep',
    });
    setModalOpen(true);
  };

  // Thêm nhanh từ Preset
  const handleAddPreset = async (mon: MonTheThao, preset: { ten: string; gioiTinh: string; loaiThiDau: string; min: number; max: number }) => {
    // Kiểm tra đã tồn tại nội dung trùng tên chưa
    const exists = items.some((x) => x.monTheThaoId === mon.id && x.ten.toLowerCase().trim() === preset.ten.toLowerCase().trim());
    if (exists) {
      toast.warning(`Nội dung "${preset.ten}" đã có trong môn ${mon.ten}!`);
      return;
    }

    const defaultHinhThuc = mon.hinhThucThiDau || 'LoaiTrucTiep';

    const newItem: NoiDungThiDauFormItem = {
      tempId: `draft_${Date.now()}_${Math.random().toString(36).substr(2, 5)}`,
      monTheThaoId: mon.id,
      tenMonTheThao: mon.ten,
      ma: '',
      ten: preset.ten,
      gioiTinh: preset.gioiTinh,
      loaiThiDau: preset.loaiThiDau,
      hinhThucThiDau: defaultHinhThuc,
      soLuongToiThieu: preset.min,
      soLuongToiDa: preset.max,
      moTa: '',
      trangThai: true,
    };

    // Nếu đang sửa giải đấu đã có trong DB, lưu trực tiếp qua API
    if (giaiDauId) {
      try {
        setSavingApi(true);
        const created = await noiDungThiDauService.create({
          giaiDauId,
          monTheThaoId: mon.id,
          ma: newItem.ma,
          ten: newItem.ten,
          gioiTinh: newItem.gioiTinh,
          loaiThiDau: newItem.loaiThiDau,
          hinhThucThiDau: newItem.hinhThucThiDau,
          soLuongToiThieu: newItem.soLuongToiThieu,
          soLuongToiDa: newItem.soLuongToiDa,
          trangThai: newItem.trangThai,
        });
        newItem.id = created.id;
        newItem.giaiDauMonTheThaoId = created.giaiDauMonTheThaoId;
        newItem.ma = created.ma;
        toast.success(`Đã thêm nội dung [${preset.ten}] vào môn ${mon.ten}!`);
      } catch (err: any) {
        console.error('Lỗi khi thêm nhanh nội dung:', err);
        toast.error(err?.response?.data?.message || 'Không thể thêm nội dung thi đấu.');
        return;
      } finally {
        setSavingApi(false);
      }
    }

    onChange([...items, newItem]);
  };

  // Lưu Modal Thêm / Sửa
  const handleSaveModal = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!formData.ten.trim()) {
      toast.error('Vui lòng nhập tên nội dung thi đấu.');
      return;
    }

    const mon = availableMons.find((m) => m.id === modalMonId);

    // TH 1: Đang sửa giải đấu và item đã có ID trên DB
    if (giaiDauId && editingItem?.id) {
      try {
        setSavingApi(true);
        const updated = await noiDungThiDauService.update(editingItem.id, {
          giaiDauId,
          monTheThaoId: modalMonId,
          ma: formData.ma,
          ten: formData.ten.trim(),
          gioiTinh: formData.gioiTinh,
          loaiThiDau: formData.loaiThiDau,
          hinhThucThiDau: formData.hinhThucThiDau || mon?.hinhThucThiDau || 'LoaiTrucTiep',
          soLuongToiThieu: formData.soLuongToiThieu,
          soLuongToiDa: formData.soLuongToiDa,
          moTa: formData.moTa,
          trangThai: formData.trangThai,
        });

        const newItems = items.map((x) => (x.id === editingItem.id ? { ...x, ...formData, ma: updated.ma || formData.ma } : x));
        onChange(newItems);
        toast.success(`Đã cập nhật nội dung [${formData.ten}]!`);
        setModalOpen(false);
      } catch (err: any) {
        console.error('Lỗi cập nhật nội dung:', err);
        toast.error(err?.response?.data?.message || 'Không thể cập nhật nội dung thi đấu.');
      } finally {
        setSavingApi(false);
      }
      return;
    }

    // TH 2: Đang sửa giải đấu và thêm item mới vào DB
    if (giaiDauId && !editingItem) {
      try {
        setSavingApi(true);
        const created = await noiDungThiDauService.create({
          giaiDauId,
          monTheThaoId: modalMonId,
          ma: formData.ma,
          ten: formData.ten.trim(),
          gioiTinh: formData.gioiTinh,
          loaiThiDau: formData.loaiThiDau,
          hinhThucThiDau: formData.hinhThucThiDau || mon?.hinhThucThiDau || 'LoaiTrucTiep',
          soLuongToiThieu: formData.soLuongToiThieu,
          soLuongToiDa: formData.soLuongToiDa,
          moTa: formData.moTa,
          trangThai: formData.trangThai,
        });

        const newItem: NoiDungThiDauFormItem = {
          ...formData,
          id: created.id,
          giaiDauMonTheThaoId: created.giaiDauMonTheThaoId,
          ma: created.ma,
          tenMonTheThao: mon?.ten || '',
          hinhThucThiDau: formData.hinhThucThiDau || mon?.hinhThucThiDau || 'LoaiTrucTiep',
        };
        onChange([...items, newItem]);
        toast.success(`Đã thêm nội dung [${formData.ten}]!`);
        setModalOpen(false);
      } catch (err: any) {
        console.error('Lỗi thêm nội dung:', err);
        toast.error(err?.response?.data?.message || 'Không thể thêm nội dung thi đấu.');
      } finally {
        setSavingApi(false);
      }
      return;
    }

    // TH 3: Thêm mới giải đấu (chưa có giải đấu trên DB, lưu vào local draft)
    if (editingItem) {
      // Sửa draft
      const newItems = items.map((x) => {
        if (x === editingItem || (editingItem.tempId && x.tempId === editingItem.tempId)) {
          return {
            ...x,
            ...formData,
            tenMonTheThao: mon?.ten || '',
            hinhThucThiDau: formData.hinhThucThiDau || mon?.hinhThucThiDau || 'LoaiTrucTiep',
          };
        }
        return x;
      });
      onChange(newItems);
      toast.success(`Đã cập nhật nội dung [${formData.ten}]!`);
    } else {
      // Thêm draft mới
      const newItem: NoiDungThiDauFormItem = {
        ...formData,
        hinhThucThiDau: formData.hinhThucThiDau || mon?.hinhThucThiDau || 'LoaiTrucTiep',
        tempId: `draft_${Date.now()}_${Math.random().toString(36).substr(2, 5)}`,
        tenMonTheThao: mon?.ten || '',
      };
      onChange([...items, newItem]);
      toast.success(`Đã thêm nội dung [${formData.ten}]!`);
    }
    setModalOpen(false);
  };

  // So khớp xem 2 nội dung thi đấu có phải cùng một bản ghi hay không
  const isMatchItem = (a: NoiDungThiDauFormItem, b: NoiDungThiDauFormItem) => {
    if (a.id && b.id) return a.id === b.id;
    if (a.tempId && b.tempId) return a.tempId === b.tempId;
    return a.monTheThaoId === b.monTheThaoId && a.ten.trim().toLowerCase() === b.ten.trim().toLowerCase();
  };

  // Mở modal xác nhận xóa
  const handleOpenDeleteModal = (e: React.MouseEvent, item: NoiDungThiDauFormItem) => {
    e.preventDefault();
    e.stopPropagation();
    setItemToDelete(item);
    setDeleteModalOpen(true);
  };

  // Xác nhận xóa nội dung
  const handleConfirmDelete = async () => {
    if (!itemToDelete) return;

    if (giaiDauId && itemToDelete.id) {
      try {
        setSavingApi(true);
        await noiDungThiDauService.delete(itemToDelete.id);
        toast.success(`Đã xóa nội dung [${itemToDelete.ten}]!`);
      } catch (err: any) {
        console.error('Lỗi khi xóa nội dung:', err);
        toast.error(err?.response?.data?.message || 'Không thể xóa nội dung thi đấu này.');
        return;
      } finally {
        setSavingApi(false);
      }
    } else {
      toast.success(`Đã xóa nội dung [${itemToDelete.ten}]!`);
    }

    // Lọc bỏ khỏi danh sách local
    const newItems = items.filter((x) => !isMatchItem(x, itemToDelete));
    onChange(newItems);
    setDeleteModalOpen(false);
    setItemToDelete(null);
  };

  // Thay đổi nhanh hình thức thi đấu trực tiếp từ bảng
  const handleInlineChangeHinhThuc = async (item: NoiDungThiDauFormItem, newHinhThuc: string) => {
    // Nếu đang ở trang sửa giải đấu và nội dung đã được lưu vào DB
    if (giaiDauId && item.id) {
      try {
        await noiDungThiDauService.update(item.id, {
          giaiDauId,
          monTheThaoId: item.monTheThaoId,
          ma: item.ma,
          ten: item.ten,
          gioiTinh: item.gioiTinh,
          loaiThiDau: item.loaiThiDau,
          hinhThucThiDau: newHinhThuc,
          soLuongToiThieu: item.soLuongToiThieu,
          soLuongToiDa: item.soLuongToiDa,
          moTa: item.moTa,
          trangThai: item.trangThai ?? true,
        });
        toast.success(`Đã đổi hình thức thi đấu cho "${item.ten}" sang ${HinhThucThiDauLabels[newHinhThuc] || newHinhThuc}!`);
      } catch (err: any) {
        console.error('Lỗi cập nhật hình thức thi đấu:', err);
        toast.error(err?.response?.data?.message || 'Không thể cập nhật hình thức thi đấu.');
        return;
      }
    }

    const newItems = items.map((x) => {
      if (isMatchItem(x, item)) {
        return { ...x, hinhThucThiDau: newHinhThuc };
      }
      return x;
    });
    onChange(newItems);
  };

  if (selectedMons.length === 0) {
    return (
      <div className="mt-4 p-4 rounded-3 border border-dashed text-center bg-light">
        <i className="bi bi-layers text-muted fs-2 d-block mb-2"></i>
        <h6 className="fw-semibold text-secondary mb-1">Chưa có môn thể thao nào được chọn</h6>
        <p className="text-muted small mb-0">
          Hãy tích chọn các môn thể thao ở trên để tiến hành thiết lập các nội dung thi đấu (như Đơn nam, Đơn nữ, Đôi nam nữ...).
        </p>
      </div>
    );
  }

  const presets = activeMon ? getSmartPresets(activeMon.ten) : [];

  return (
    <div className="mt-4 pt-3 border-top">
      <div className="d-flex flex-wrap align-items-center justify-content-between gap-2 mb-3">
        <div>
          <h6 className="fw-bold text-dark mb-0 d-flex align-items-center gap-2">
            <i className="bi bi-layers-fill text-primary"></i>
            Cấu Hình Nội Dung Thi Đấu Theo Môn
            <Badge color="primary" pill className="ms-1">
              {items.length} nội dung
            </Badge>
          </h6>
          <span className="text-muted small">
            Các nội dung này sẽ hiển thị để các đơn vị lựa chọn khi đăng ký tham gia giải đấu
          </span>
        </div>
      </div>

      {/* Tabs chọn môn thể thao */}
      <div className="d-flex flex-wrap gap-2 mb-3">
        {selectedMons.map((mon) => {
          const count = countForMon(mon.id);
          const isActive = mon.id === activeMonId;
          return (
            <Button
              key={mon.id}
              type="button"
              size="sm"
              color={isActive ? 'primary' : 'light'}
              onClick={(e) => {
                e.preventDefault();
                e.stopPropagation();
                setActiveMonId(mon.id);
              }}
              className={`rounded-pill px-3 d-inline-flex align-items-center gap-2 border transition-all ${
                isActive ? 'fw-bold shadow-sm' : 'text-dark'
              }`}
            >
              <span>{mon.ten}</span>
              <Badge
                color={isActive ? 'light' : count > 0 ? 'success' : 'secondary'}
                className={isActive ? 'text-primary' : 'text-white'}
                pill
              >
                {count}
              </Badge>
            </Button>
          );
        })}
      </div>

      {/* Bảng nội dung thi đấu của môn đang chọn */}
      {activeMon && (
        <Card className="border rounded-3 bg-white shadow-none">
          <CardBody className="p-3">
            {/* Header môn đang chọn */}
            <div className="d-flex flex-wrap align-items-center justify-content-between gap-2 mb-3 pb-2 border-bottom">
              <div className="d-flex align-items-center gap-2">
                <span className="fs-5">🏆</span>
                <div>
                  <span className="fw-bold text-dark">{activeMon.ten}</span>
                  {activeMon.tenDanhMuc && (
                    <Badge color="secondary" className="ms-2 font-monospace" style={{ fontSize: '11px' }}>
                      {activeMon.tenDanhMuc}
                    </Badge>
                  )}
                </div>
              </div>

              {!readOnly && (
                <Button
                  type="button"
                  size="sm"
                  color="primary"
                  onClick={(e) => {
                    e.preventDefault();
                    e.stopPropagation();
                    handleOpenAddModal(activeMon.id);
                  }}
                  disabled={savingApi}
                  className="rounded-3 d-inline-flex align-items-center gap-1.5"
                >
                  <i className="bi bi-plus-lg"></i>
                  Thêm nội dung
                </Button>
              )}
            </div>

            {/* Gợi ý nhanh các nội dung phổ biến */}
            {!readOnly && presets.length > 0 && (
              <div className="mb-3 p-2.5 rounded-3 bg-light border border-light-subtle d-flex flex-wrap align-items-center gap-2">
                <span className="text-muted small fw-semibold d-inline-flex align-items-center gap-1">
                  <i className="bi bi-lightning-charge-fill text-warning"></i>
                  Gợi ý thêm nhanh:
                </span>
                {presets.map((preset, idx) => {
                  const alreadyAdded = activeMonItems.some(
                    (x) => x.ten.toLowerCase().trim() === preset.ten.toLowerCase().trim()
                  );
                  return (
                    <button
                      key={idx}
                      type="button"
                      disabled={alreadyAdded || savingApi}
                      onClick={() => handleAddPreset(activeMon, preset)}
                      className={`btn btn-sm rounded-pill py-0 px-2.5 d-inline-flex align-items-center gap-1 ${
                        alreadyAdded
                          ? 'btn-outline-secondary opacity-50 cursor-not-allowed'
                          : 'btn-outline-primary bg-white hover-bg-primary text-primary'
                      }`}
                      style={{ fontSize: '12px', height: '26px' }}
                      title={alreadyAdded ? 'Đã có trong danh sách' : 'Bấm để thêm nhanh nội dung này'}
                    >
                      <i className={`bi ${alreadyAdded ? 'bi-check-lg' : 'bi-plus'}`}></i>
                      {preset.ten}
                    </button>
                  );
                })}
              </div>
            )}

            {/* Bảng danh sách nội dung */}
            {activeMonItems.length === 0 ? (
              <div className="text-center py-4 bg-light rounded-3 border border-dashed">
                <i className="bi bi-tag text-muted fs-3 d-block mb-1"></i>
                <p className="text-muted small mb-2">
                  Môn <strong>{activeMon.ten}</strong> chưa có nội dung thi đấu nào.
                </p>
                {!readOnly && (
                  <Button
                    type="button"
                    size="sm"
                    color="outline-primary"
                    onClick={(e) => {
                      e.preventDefault();
                      e.stopPropagation();
                      handleOpenAddModal(activeMon.id);
                    }}
                    className="rounded-pill"
                  >
                    + Thêm nội dung đầu tiên
                  </Button>
                )}
              </div>
            ) : (
              <div className="table-responsive">
                <Table hover className="align-middle mb-0 text-nowrap">
                  <thead className="table-light text-muted small">
                    <tr>
                      <th>#</th>
                      <th>Tên Nội Dung Thi Đấu</th>
                      <th>Giới Tính</th>
                      <th>Thể Thức</th>
                      <th style={{ minWidth: '220px' }}>Hình Thức Thi Đấu</th>
                      <th>Số VĐV</th>
                      {!readOnly && <th className="text-end">Thao Tác</th>}
                    </tr>
                  </thead>
                  <tbody>
                    {activeMonItems.map((item, idx) => {
                      const effectiveHinhThuc = item.hinhThucThiDau || activeMon?.hinhThucThiDau || 'LoaiTrucTiep';
                      return (
                        <tr key={item.id || item.tempId || idx}>
                          <td className="text-muted small">{idx + 1}</td>
                          <td>
                            <span className="fw-bold text-dark">{item.ten}</span>
                            {item.ma && (
                              <span className="text-muted font-monospace small ms-2">
                                ({item.ma})
                              </span>
                            )}
                            {item.moTa && (
                              <div className="text-muted small text-truncate" style={{ maxWidth: '280px' }}>
                                {item.moTa}
                              </div>
                            )}
                          </td>
                          <td>
                            <Badge
                              color={
                                item.gioiTinh === 'Nam'
                                  ? 'primary'
                                  : item.gioiTinh === 'Nu'
                                  ? 'danger'
                                  : 'info'
                              }
                              pill
                            >
                              {item.gioiTinh === 'Nam' ? '♂ Nam' : item.gioiTinh === 'Nu' ? '♀ Nữ' : '⚥ Hỗn hợp'}
                            </Badge>
                          </td>
                          <td>
                            <Badge color={item.loaiThiDau === 'CaNhan' ? 'secondary' : 'dark'} pill>
                              {item.loaiThiDau === 'CaNhan' ? 'Cá nhân' : 'Đồng đội'}
                            </Badge>
                          </td>
                          <td style={{ minWidth: '220px' }}>
                          {!readOnly ? (
                            <Input
                              type="select"
                              bsSize="sm"
                              className="form-select form-select-sm border-primary-subtle fw-medium"
                              style={{ fontSize: '12px', padding: '4px 8px' }}
                              value={effectiveHinhThuc}
                              onChange={(e) => handleInlineChangeHinhThuc(item, e.target.value)}
                              title={`Hình thức thi đấu của ${item.ten} (mặc định theo môn: ${HinhThucThiDauLabels[activeMon?.hinhThucThiDau || 'LoaiTrucTiep']})`}
                            >
                              {Object.entries(HinhThucThiDauLabels).map(([key, label]) => {
                                const isDefaultOfMon = (activeMon?.hinhThucThiDau || 'LoaiTrucTiep') === key;
                                return (
                                  <option key={key} value={key}>
                                    {label} {isDefaultOfMon ? ' (Mặc định môn)' : ''}
                                  </option>
                                );
                              })}
                            </Input>
                          ) : (
                            <span
                              className="badge bg-light text-dark border font-monospace small"
                              style={{ fontSize: '11px' }}
                              title={HinhThucThiDauLabels[effectiveHinhThuc] || effectiveHinhThuc}
                            >
                              {HinhThucThiDauLabels[effectiveHinhThuc] || effectiveHinhThuc}
                            </span>
                          )}
                        </td>
                        <td className="small">
                          {item.loaiThiDau === 'CaNhan' ? (
                            <span className="text-secondary">1 VĐV</span>
                          ) : (
                            <span>
                              {item.soLuongToiThieu} - {item.soLuongToiDa} VĐV
                            </span>
                          )}
                        </td>
                        {!readOnly && (
                          <td className="text-end">
                            <div className="d-inline-flex gap-1">
                              <Button
                                type="button"
                                size="sm"
                                color="light"
                                className="btn-icon py-1 px-2 text-primary"
                                onClick={(e) => {
                                  e.preventDefault();
                                  e.stopPropagation();
                                  handleOpenEditModal(item);
                                }}
                                title="Chỉnh sửa nội dung"
                              >
                                <i className="bi bi-pencil-square"></i>
                              </Button>
                              <Button
                                type="button"
                                size="sm"
                                color="light"
                                className="btn-icon py-1 px-2 text-danger"
                                onClick={(e) => handleOpenDeleteModal(e, item)}
                                title="Xóa nội dung"
                              >
                                <i className="bi bi-trash"></i>
                              </Button>
                            </div>
                          </td>
                        )}
                        </tr>
                      );
                    })}
                  </tbody>
                </Table>
              </div>
            )}
          </CardBody>
        </Card>
      )}

      {/* MODAL THÊM / SỬA NỘI DUNG */}
      <Modal isOpen={modalOpen} toggle={() => setModalOpen(false)} centered backdrop="static">
        <ModalHeader toggle={() => setModalOpen(false)} className="border-bottom">
          <span className="fw-bold">
            {editingItem ? 'Chỉnh Sửa Nội Dung Thi Đấu' : 'Thêm Mới Nội Dung Thi Đấu'}
          </span>
          <div className="small text-muted font-normal">
            Môn: {availableMons.find((m) => m.id === modalMonId)?.ten}
          </div>
        </ModalHeader>
        <form onSubmit={handleSaveModal}>
          <ModalBody className="p-4">
            <FormGroup className="mb-3">
              <Label className="small fw-semibold">
                Tên nội dung thi đấu <span className="text-danger">*</span>
              </Label>
              <Input
                type="text"
                placeholder="VD: Đơn nam U18, Bóng đá nam 7 người, Đôi nam nữ..."
                value={formData.ten}
                onChange={(e) => {
                  const val = e.target.value;
                  setFormData((prev) => ({
                    ...prev,
                    ten: val,
                  }));
                }}
                required
                autoFocus
              />
            </FormGroup>

            <Row className="g-3 mb-3">
              <Col md={6}>
                <FormGroup className="mb-0">
                  <Label className="small fw-semibold">Giới tính</Label>
                  <Input
                    type="select"
                    value={formData.gioiTinh}
                    onChange={(e) => setFormData({ ...formData, gioiTinh: e.target.value })}
                  >
                    <option value="Nam">Nam</option>
                    <option value="Nu">Nữ</option>
                    <option value="HonHop">Hỗn hợp (Nam &amp; Nữ)</option>
                  </Input>
                </FormGroup>
              </Col>
              <Col md={6}>
                <FormGroup className="mb-0">
                  <Label className="small fw-semibold">Thể thức thi đấu</Label>
                  <Input
                    type="select"
                    value={formData.loaiThiDau}
                    onChange={(e) => {
                      const newLoai = e.target.value;
                      setFormData({
                        ...formData,
                        loaiThiDau: newLoai,
                        soLuongToiThieu: newLoai === 'CaNhan' ? 1 : 2,
                        soLuongToiDa: newLoai === 'CaNhan' ? 1 : 15,
                      });
                    }}
                  >
                    <option value="CaNhan">Cá nhân (1 người)</option>
                    <option value="DongDoi">Đồng đội / Đôi (Nhiều người)</option>
                  </Input>
                </FormGroup>
              </Col>
            </Row>

            {/* Hình thức thi đấu (Kế thừa mặc định từ môn thể thao nếu không chọn khác) */}
            <FormGroup className="mb-3">
              <div className="d-flex justify-content-between align-items-center mb-1">
                <Label className="small fw-semibold mb-0">Hình thức thi đấu</Label>
                {(() => {
                  const mon = availableMons.find((m) => m.id === modalMonId);
                  const monHt = mon?.hinhThucThiDau || 'LoaiTrucTiep';
                  return (
                    <span className="text-primary small fw-semibold" style={{ fontSize: '11px' }}>
                      Môn {mon?.ten}: {HinhThucThiDauLabels[monHt] || monHt}
                    </span>
                  );
                })()}
              </div>
              <Input
                type="select"
                value={
                  formData.hinhThucThiDau ||
                  availableMons.find((m) => m.id === modalMonId)?.hinhThucThiDau ||
                  'LoaiTrucTiep'
                }
                onChange={(e) => setFormData({ ...formData, hinhThucThiDau: e.target.value })}
              >
                {Object.entries(HinhThucThiDauLabels).map(([key, label]) => {
                  const mon = availableMons.find((m) => m.id === modalMonId);
                  const isDefaultOfMon = (mon?.hinhThucThiDau || 'LoaiTrucTiep') === key;
                  return (
                    <option key={key} value={key}>
                      {label} {isDefaultOfMon ? '★ (Mặc định theo môn)' : ''}
                    </option>
                  );
                })}
              </Input>
              <small className="text-muted" style={{ fontSize: '11.5px' }}>
                Nếu không chọn khác, nội dung sẽ mặc định kế thừa theo hình thức thi đấu của môn thể thao này.
              </small>
            </FormGroup>

            {formData.loaiThiDau === 'DongDoi' && (
              <Row className="g-3 mb-3 p-3 bg-light rounded-3">
                <Col md={6}>
                  <FormGroup className="mb-0">
                    <Label className="small fw-semibold">Số VĐV tối thiểu / đội</Label>
                    <Input
                      type="number"
                      min={1}
                      value={formData.soLuongToiThieu}
                      onChange={(e) =>
                        setFormData({ ...formData, soLuongToiThieu: Number(e.target.value) })
                      }
                    />
                  </FormGroup>
                </Col>
                <Col md={6}>
                  <FormGroup className="mb-0">
                    <Label className="small fw-semibold">Số VĐV tối đa / đội</Label>
                    <Input
                      type="number"
                      min={formData.soLuongToiThieu}
                      value={formData.soLuongToiDa}
                      onChange={(e) =>
                        setFormData({ ...formData, soLuongToiDa: Number(e.target.value) })
                      }
                    />
                  </FormGroup>
                </Col>
              </Row>
            )}

            <FormGroup className="mb-3">
              <Label className="small fw-semibold">Ghi chú / Quy định thể lệ riêng (tuỳ chọn)</Label>
              <Input
                type="textarea"
                rows={2}
                placeholder="VD: Thi đấu theo luật cầu lông hiện hành..."
                value={formData.moTa || ''}
                onChange={(e) => setFormData({ ...formData, moTa: e.target.value })}
              />
            </FormGroup>

          </ModalBody>
          <ModalFooter className="border-top">
            <Button color="light" type="button" onClick={() => setModalOpen(false)}>
              Hủy bỏ
            </Button>
            <Button
              color="primary"
              type="submit"
              disabled={savingApi}
              className="d-inline-flex align-items-center gap-1.5"
            >
              {savingApi ? <Spinner size="sm" /> : <i className="bi bi-check-lg"></i>}
              {editingItem ? 'Lưu Thay Đổi' : 'Thêm Nội Dung'}
            </Button>
          </ModalFooter>
        </form>
      </Modal>

      {/* MODAL XÁC NHẬN XÓA NỘI DUNG */}
      <Modal isOpen={deleteModalOpen} toggle={() => setDeleteModalOpen(false)} centered size="sm">
        <ModalHeader toggle={() => setDeleteModalOpen(false)} className="border-bottom text-danger">
          <i className="bi bi-exclamation-triangle-fill me-2"></i>
          Xác nhận xóa
        </ModalHeader>
        <ModalBody className="p-4 text-center">
          <div className="mb-3">
            <i className="bi bi-trash text-danger" style={{ fontSize: '2.5rem' }}></i>
          </div>
          <p className="mb-1 text-dark">
            Bạn có chắc chắn muốn xóa nội dung:
          </p>
          <p className="fw-bold fs-6 text-primary mb-2">
            {itemToDelete?.ten}
          </p>
          <small className="text-muted">
            Nội dung này sẽ được xóa khỏi môn thi đấu.
          </small>
        </ModalBody>
        <ModalFooter className="border-top justify-content-center">
          <Button type="button" color="light" onClick={() => setDeleteModalOpen(false)} disabled={savingApi}>
            Hủy bỏ
          </Button>
          <Button
            type="button"
            color="danger"
            onClick={handleConfirmDelete}
            disabled={savingApi}
            className="d-inline-flex align-items-center gap-1.5"
          >
            {savingApi ? <Spinner size="sm" /> : <i className="bi bi-trash"></i>}
            Xóa nội dung
          </Button>
        </ModalFooter>
      </Modal>
    </div>
  );
}
