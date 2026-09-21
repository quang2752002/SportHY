'use client';

import React, { useEffect, useState, useCallback } from 'react';
import {Row,Col,Table,Card,CardBody,Button,Input,Spinner,
  Modal,
  ModalHeader,
  ModalBody,
  ModalFooter,
  Form,
  FormGroup,
  Label,
  Badge,
} from 'reactstrap';
import { cumSanService } from '@/services';
import { CumSan, CreateUpdateCumSan } from '@/types';
import { PaginationComponent } from '@/components/common/PaginationComponent';
import { useAuth } from '@/context/AuthContext';
import { Permissions } from '@/constants/permissions';
import Link from 'next/link';

export default function AdminCumSanPage() {
  const { hasPermission } = useAuth();
  const canCreate = hasPermission(Permissions.SanDau.Create);
  const canEdit = hasPermission(Permissions.SanDau.Edit);
  const canDelete = hasPermission(Permissions.SanDau.Delete);

  const [cumSans, setCumSans] = useState<CumSan[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(1);
  const [pageIndex, setPageIndex] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [keyword, setKeyword] = useState('');
  const [trangThaiFilter, setTrangThaiFilter] = useState<string>('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Modal Thêm / Sửa
  const [modalOpen, setModalOpen] = useState(false);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [formData, setFormData] = useState<CreateUpdateCumSan>({
    ma: '',
    ten: '',
    diaChi: '',
    soLuongSan: 1,
    moTa: '',
    trangThai: true,
  });
  const [submitting, setSubmitting] = useState(false);

  // Fetch dữ liệu từ backend
  const loadCumSans = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await cumSanService.getPaged({
        pageIndex,
        pageSize,
        keyword: keyword.trim() || undefined,
        trangThai: trangThaiFilter !== '' ? trangThaiFilter === 'true' : undefined,
      });
      setCumSans(data.items || []);
      setTotalCount(data.totalCount || 0);
      setTotalPages(data.totalPages || 1);
    } catch (err: any) {
      console.error('Lỗi khi tải danh sách cụm sân:', err);
      setError(err?.message || 'Không thể kết nối đến máy chủ Backend.');
    } finally {
      setLoading(false);
    }
  }, [pageIndex, pageSize, keyword, trangThaiFilter]);

  useEffect(() => {
    loadCumSans();
  }, [loadCumSans]);

  const handleSearchSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setPageIndex(1);
    loadCumSans();
  };

  const handleOpenCreateModal = () => {
    setEditingId(null);
    setFormData({
      ma: '',
      ten: '',
      diaChi: '',
      soLuongSan: 1,
      moTa: '',
      trangThai: true,
    });
    setModalOpen(true);
  };

  const handleOpenEditModal = (item: CumSan) => {
    setEditingId(item.id);
    setFormData({
      ma: item.ma,
      ten: item.ten,
      diaChi: item.diaChi || '',
      soLuongSan: item.soLuongSan ?? 1,
      moTa: item.moTa || '',
      trangThai: item.trangThai ?? true,
    });
    setModalOpen(true);
  };

  const handleDelete = async (id: number) => {
    if (!confirm('Bạn có chắc chắn muốn xóa cụm sân này?')) return;
    try {
      await cumSanService.delete(id);
      loadCumSans();
    } catch (err: any) {
      alert('Không thể xóa cụm sân: ' + (err?.message || 'Lỗi server'));
    }
  };

  const handleSubmitForm = async (e: React.FormEvent) => {
    e.preventDefault();
    setSubmitting(true);
    try {
      if (editingId) {
        await cumSanService.update(editingId, formData);
      } else {
        await cumSanService.create(formData);
      }
      setModalOpen(false);
      loadCumSans();
    } catch (err: any) {
      alert('Lỗi lưu thông tin: ' + (err?.message || 'Lỗi server'));
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <Row>
      <Col lg="12">
        <Card className="border-0 shadow-sm rounded-4 overflow-hidden mb-4">
          <CardBody className="p-4">
            {/* Header */}
            <div className="d-flex flex-wrap justify-content-between align-items-center gap-3 pb-3 mb-4 border-bottom">
              <div>
                <div className="d-flex align-items-center gap-2">
                  <div className="p-2 bg-primary-subtle text-primary rounded-3 d-inline-flex">
                    <i className="bi bi-geo-alt-fill fs-4"></i>
                  </div>
                  <div>
                    <h4 className="fw-bold mb-0 text-dark">Quản lý Cụm Sân </h4>
                    <p className="text-muted small mb-0">
                      Khu liên hợp thể thao, nhà thi đấu, cụm sân bãi ({totalCount} cụm sân)
                    </p>
                  </div>
                </div>
              </div>
              <div className="d-flex gap-2">
                <Link href="/admin/san-dau" className="btn btn-outline-secondary px-3 py-2 fw-semibold rounded-3 d-inline-flex align-items-center gap-2">
                  <i className="bi bi-grid-3x3-gap"></i> Xem tất cả sân đấu
                </Link>
                {canCreate && (
                  <Button
                    color="primary"
                    onClick={handleOpenCreateModal}
                    className="px-3 py-2 fw-semibold rounded-3 d-inline-flex align-items-center gap-2 shadow-sm"
                  >
                    <i className="bi bi-plus-lg"></i> Thêm cụm sân mới
                  </Button>
                )}
              </div>
            </div>

            {/* Thanh tìm kiếm và bộ lọc */}
            <Form onSubmit={handleSearchSubmit} className="mb-4">
              <div className="p-3 bg-light rounded-3">
                <Row className="g-2 align-items-center">
                  <Col md={6} lg={5}>
                    <div className="input-group bg-white rounded-3 overflow-hidden border">
                      <span className="input-group-text bg-white border-0 text-muted ps-3">
                        <i className="bi bi-search"></i>
                      </span>
                      <Input
                        type="text"
                        className="border-0 shadow-none ps-2"
                        placeholder="Tìm kiếm theo mã, tên cụm sân, địa chỉ..."
                        value={keyword}
                        onChange={(e) => setKeyword(e.target.value)}
                      />
                    </div>
                  </Col>
                  <Col md={3} lg={4}>
                    <Input
                      type="select"
                      className="bg-white border rounded-3 shadow-none"
                      value={trangThaiFilter}
                      onChange={(e) => {
                        setTrangThaiFilter(e.target.value);
                        setPageIndex(1);
                      }}
                    >
                      <option value="">-- Tất cả trạng thái --</option>
                      <option value="true">Đang hoạt động</option>
                      <option value="false">Tạm dừng bảo trì</option>
                    </Input>
                  </Col>
                  <Col md={3} lg={3}>
                    <Button color="dark" type="submit" className="w-100 rounded-3">
                      Tìm kiếm
                    </Button>
                  </Col>
                </Row>
              </div>
            </Form>

            {/* Thông báo lỗi nếu có */}
            {error && (
              <div className="alert alert-danger d-flex align-items-center mb-4" role="alert">
                <i className="bi bi-exclamation-triangle-fill me-2 fs-5"></i>
                <div>{error}</div>
              </div>
            )}

            {/* Bảng dữ liệu cụm sân */}
            {loading ? (
              <div className="text-center py-5">
                <Spinner color="primary" />
                <p className="mt-2 text-muted small">Đang tải danh sách cụm sân...</p>
              </div>
            ) : cumSans.length === 0 ? (
              <div className="text-center py-5 text-muted border rounded-3 bg-light">
                <i className="bi bi-inbox fs-1 d-block mb-2 text-secondary"></i>
                Chưa có cụm sân nào được ghi nhận.
              </div>
            ) : (
              <div className="table-responsive">
                <Table hover className="align-middle mb-0">
                  <thead className="table-light text-uppercase fs-7 text-muted">
                    <tr>
                      <th style={{ width: '50px' }}>#</th>
                      <th>Mã Cụm</th>
                      <th>Tên Cụm Sân</th>
                      <th>Địa Chỉ</th>
                      <th>Quy Mô</th>
                      <th>Trạng Thái</th>
                      <th className="text-end" style={{ width: '170px' }}>
                        Thao tác
                      </th>
                    </tr>
                  </thead>
                  <tbody>
                    {cumSans.map((item, index) => (
                      <tr key={item.id}>
                        <td className="text-muted">{(pageIndex - 1) * pageSize + index + 1}</td>
                        <td>
                          <span className="badge bg-light text-primary border font-monospace">
                            {item.ma}
                          </span>
                        </td>
                        <td>
                          <div className="fw-semibold text-dark">{item.ten}</div>
                          {item.moTa && (
                            <small className="text-muted text-truncate d-block" style={{ maxWidth: '280px' }}>
                              {item.moTa}
                            </small>
                          )}
                        </td>
                        <td>
                          <div className="small text-muted d-flex align-items-center">
                            <i className="bi bi-geo-alt me-1 text-secondary"></i>
                            {item.diaChi || 'Chưa cập nhật'}
                          </div>
                        </td>
                        <td>
                          <Badge color="info" pill className="me-1">
                            {item.soSanHienCo ?? 0} sân đấu
                          </Badge>
                          {item.soLuongSan && (
                            <span className="small text-muted">/ quy mô {item.soLuongSan}</span>
                          )}
                        </td>
                        <td>
                          {item.trangThai ? (
                            <span className="badge rounded-pill bg-success-subtle text-success border border-success-subtle px-3 py-2 fw-medium d-inline-flex align-items-center gap-1">
                              <span className="p-1 bg-success rounded-circle d-inline-block" />
                              Hoạt động
                            </span>
                          ) : (
                            <span className="badge rounded-pill bg-secondary-subtle text-secondary border border-secondary-subtle px-3 py-2 fw-medium d-inline-flex align-items-center gap-1">
                              <span className="p-1 bg-secondary rounded-circle d-inline-block" />
                              Tạm dừng
                            </span>
                          )}
                        </td>
                        <td className="text-end">
                          <div className="d-flex justify-content-end gap-1">
                            <Link
                              href={`/admin/san-dau?cumSanId=${item.id}`}
                              className="btn btn-sm btn-light btn-icon text-info"
                              title="Xem danh sách sân đấu thuộc cụm"
                            >
                              <i className="bi bi-grid-fill"></i>
                            </Link>
                            {canEdit && (
                              <Button
                                size="sm"
                                color="light"
                                className="btn-icon text-primary"
                                title="Chỉnh sửa cụm sân"
                                onClick={() => handleOpenEditModal(item)}
                              >
                                <i className="bi bi-pencil-square"></i>
                              </Button>
                            )}
                            {canDelete && (
                              <Button
                                size="sm"
                                color="light"
                                className="btn-icon text-danger"
                                title="Xóa cụm sân"
                                onClick={() => handleDelete(item.id)}
                              >
                                <i className="bi bi-trash"></i>
                              </Button>
                            )}
                          </div>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </Table>
              </div>
            )}

            {/* Phân trang */}
            {!loading && totalCount > 0 && (
              <div className="mt-4 pt-3 border-top">
                <PaginationComponent
                  pageIndex={pageIndex}
                  totalPages={totalPages}
                  totalCount={totalCount}
                  pageSize={pageSize}
                  onPageChange={(page) => setPageIndex(page)}
                  onPageSizeChange={(size) => {
                    setPageSize(size);
                    setPageIndex(1);
                  }}
                />
              </div>
            )}
          </CardBody>
        </Card>
      </Col>

      {/* Modal Thêm/Sửa Cụm Sân */}
      <Modal isOpen={modalOpen} toggle={() => setModalOpen(!modalOpen)} size="lg" centered>
        <ModalHeader toggle={() => setModalOpen(!modalOpen)} className="border-bottom">
          <div className="d-flex align-items-center gap-2">
            <i className={`bi ${editingId ? 'bi-pencil-square text-primary' : 'bi-plus-circle-fill text-success'}`}></i>
            <span className="fw-bold">{editingId ? 'Cập Nhật Cụm Sân' : 'Thêm Cụm Sân Mới'}</span>
          </div>
        </ModalHeader>
        <Form onSubmit={handleSubmitForm}>
          <ModalBody className="p-4">
            <Row className="g-3">
              <Col md={4}>
                <FormGroup>
                  <Label className="fw-semibold small">
                    Mã cụm sân <span className="text-danger">*</span>
                  </Label>
                  <Input
                    type="text"
                    required
                    placeholder="VD: CS_001"
                    value={formData.ma}
                    onChange={(e) => setFormData({ ...formData, ma: e.target.value })}
                  />
                </FormGroup>
              </Col>
              <Col md={8}>
                <FormGroup>
                  <Label className="fw-semibold small">
                    Tên cụm sân <span className="text-danger">*</span>
                  </Label>
                  <Input
                    type="text"
                    required
                    placeholder="VD: Cụm Sân Đa Năng Tỉnh / Nhà Thi Đấu A"
                    value={formData.ten}
                    onChange={(e) => setFormData({ ...formData, ten: e.target.value })}
                  />
                </FormGroup>
              </Col>

              <Col md={8}>
                <FormGroup>
                  <Label className="fw-semibold small">Địa chỉ</Label>
                  <Input
                    type="text"
                    placeholder="VD: Số 123 Đường Trần Phú, TP..."
                    value={formData.diaChi}
                    onChange={(e) => setFormData({ ...formData, diaChi: e.target.value })}
                  />
                </FormGroup>
              </Col>

              <Col md={4}>
                <FormGroup>
                  <Label className="fw-semibold small">Quy mô thiết kế (Số sân)</Label>
                  <Input
                    type="number"
                    min={1}
                    value={formData.soLuongSan || 1}
                    onChange={(e) => setFormData({ ...formData, soLuongSan: Number(e.target.value) })}
                  />
                </FormGroup>
              </Col>

              <Col md={12}>
                <FormGroup>
                  <Label className="fw-semibold small">Mô tả / Tiện ích</Label>
                  <Input
                    type="textarea"
                    rows={3}
                    placeholder="Ghi chú về cơ sở vật chất, hệ thống chiếu sáng, khán đài..."
                    value={formData.moTa}
                    onChange={(e) => setFormData({ ...formData, moTa: e.target.value })}
                  />
                </FormGroup>
              </Col>

              <Col md={12}>
                <FormGroup check className="mt-2">
                  <Input
                    type="checkbox"
                    id="trangThaiCumSan"
                    checked={formData.trangThai}
                    onChange={(e) => setFormData({ ...formData, trangThai: e.target.checked })}
                  />
                  <Label check htmlFor="trangThaiCumSan" className="fw-semibold small">
                    Đang hoạt động (sẵn sàng phục vụ giải đấu)
                  </Label>
                </FormGroup>
              </Col>
            </Row>
          </ModalBody>
          <ModalFooter className="border-top">
            <Button color="light" onClick={() => setModalOpen(false)} disabled={submitting}>
              Hủy
            </Button>
            <Button color="primary" type="submit" disabled={submitting} className="d-inline-flex align-items-center gap-2">
              {submitting && <Spinner size="sm" />}
              {editingId ? 'Cập nhật' : 'Tạo cụm sân'}
            </Button>
          </ModalFooter>
        </Form>
      </Modal>
    </Row>
  );
}
