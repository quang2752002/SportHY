'use client';

import React, { useEffect, useState, useCallback, Suspense } from 'react';
import {
  Row,
  Col,
  Table,
  Card,
  CardBody,
  Button,
  Input,
  Spinner,
  Modal,
  ModalHeader,
  ModalBody,
  ModalFooter,
  Form,
  FormGroup,
  Label,
  Badge,
} from 'reactstrap';
import { sanDauService, cumSanService, monTheThaoService } from '@/services';
import { SanDau, CreateUpdateSanDau, CumSan, MonTheThao } from '@/types';
import { PaginationComponent } from '@/components/common/PaginationComponent';
import { useAuth } from '@/context/AuthContext';
import { Permissions } from '@/constants/permissions';
import { useSearchParams } from 'next/navigation';
import Link from 'next/link';

function AdminSanDauContent() {
  const searchParams = useSearchParams();
  const initialCumSanId = searchParams.get('cumSanId') || '';

  const { hasPermission } = useAuth();
  const canCreate = hasPermission(Permissions.SanDau.Create);
  const canEdit = hasPermission(Permissions.SanDau.Edit);
  const canDelete = hasPermission(Permissions.SanDau.Delete);

  const [sanDaus, setSanDaus] = useState<SanDau[]>([]);
  const [cumSans, setCumSans] = useState<CumSan[]>([]);
  const [monTheThaos, setMonTheThaos] = useState<MonTheThao[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(1);
  const [pageIndex, setPageIndex] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [keyword, setKeyword] = useState('');
  const [cumSanFilter, setCumSanFilter] = useState<string>(initialCumSanId);
  const [monTheThaoFilter, setMonTheThaoFilter] = useState<string>('');
  const [trangThaiFilter, setTrangThaiFilter] = useState<string>('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Modal Thêm / Sửa
  const [modalOpen, setModalOpen] = useState(false);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [formData, setFormData] = useState<CreateUpdateSanDau>({
    cumSanId: initialCumSanId ? Number(initialCumSanId) : 0,
    monTheThaoId: undefined,
    ma: '',
    ten: '',
    loaiSan: 'Sân cỏ nhân tạo',
    soSan: 1,
    sucChua: 500,
    moTa: '',
    trangThai: true,
  });
  const [submitting, setSubmitting] = useState(false);

  // Tải danh sách cụm sân & môn thể thao cho dropdown
  useEffect(() => {
    cumSanService.getAll()
      .then((data) => {
        setCumSans(data || []);
        if (!formData.cumSanId && data && data.length > 0) {
          setFormData((prev) => ({ ...prev, cumSanId: data[0].id }));
        }
      })
      .catch((err) => console.error('Lỗi khi tải danh sách cụm sân:', err));

    monTheThaoService.getAll()
      .then((data) => setMonTheThaos(data || []))
      .catch((err) => console.error('Lỗi khi tải danh sách môn thể thao:', err));
  }, []);

  // Fetch dữ liệu từ backend
  const loadSanDaus = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await sanDauService.getPaged({
        pageIndex,
        pageSize,
        keyword: keyword.trim() || undefined,
        cumSanId: cumSanFilter ? Number(cumSanFilter) : undefined,
        monTheThaoId: monTheThaoFilter ? Number(monTheThaoFilter) : undefined,
        trangThai: trangThaiFilter !== '' ? trangThaiFilter === 'true' : undefined,
      });
      setSanDaus(data.items || []);
      setTotalCount(data.totalCount || 0);
      setTotalPages(data.totalPages || 1);
    } catch (err: any) {
      console.error('Lỗi khi tải danh sách sân đấu:', err);
      setError(err?.message || 'Không thể kết nối đến máy chủ Backend.');
    } finally {
      setLoading(false);
    }
  }, [pageIndex, pageSize, keyword, cumSanFilter, monTheThaoFilter, trangThaiFilter]);

  useEffect(() => {
    loadSanDaus();
  }, [loadSanDaus]);

  const handleSearchSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setPageIndex(1);
    loadSanDaus();
  };

  const handleOpenCreateModal = () => {
    setEditingId(null);
    setFormData({
      cumSanId: cumSanFilter ? Number(cumSanFilter) : (cumSans[0]?.id || 0),
      monTheThaoId: monTheThaoFilter ? Number(monTheThaoFilter) : undefined,
      ma: '',
      ten: '',
      loaiSan: 'Sân cỏ nhân tạo',
      soSan: 1,
      sucChua: 200,
      moTa: '',
      trangThai: true,
    });
    setModalOpen(true);
  };

  const handleOpenEditModal = (item: SanDau) => {
    setEditingId(item.id);
    setFormData({
      cumSanId: item.cumSanId,
      monTheThaoId: item.monTheThaoId,
      ma: item.ma,
      ten: item.ten,
      loaiSan: item.loaiSan || '',
      soSan: item.soSan ?? 1,
      sucChua: item.sucChua ?? 0,
      moTa: item.moTa || '',
      trangThai: item.trangThai ?? true,
    });
    setModalOpen(true);
  };

  const handleDelete = async (id: number) => {
    if (!confirm('Bạn có chắc chắn muốn xóa sân đấu này?')) return;
    try {
      await sanDauService.delete(id);
      loadSanDaus();
    } catch (err: any) {
      alert('Không thể xóa sân đấu: ' + (err?.message || 'Lỗi server'));
    }
  };

  const handleSubmitForm = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!formData.cumSanId) {
      alert('Vui lòng chọn Cụm sân trực thuộc!');
      return;
    }
    setSubmitting(true);
    try {
      if (editingId) {
        await sanDauService.update(editingId, formData);
      } else {
        await sanDauService.create(formData);
      }
      setModalOpen(false);
      loadSanDaus();
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
                    <i className="bi bi-grid-3x3-gap-fill fs-4"></i>
                  </div>
                  <div>
                    <h4 className="fw-bold mb-0 text-dark">Quản lý Sân Đấu </h4>
                    <p className="text-muted small mb-0">
                      Danh mục các sân đấu cụ thể thuộc từng cụm sân ({totalCount} sân đấu)
                    </p>
                  </div>
                </div>
              </div>
              <div className="d-flex gap-2">
                <Link href="/admin/cum-san" className="btn btn-outline-secondary px-3 py-2 fw-semibold rounded-3 d-inline-flex align-items-center gap-2">
                  <i className="bi bi-geo-alt"></i> Quản lý Cụm sân
                </Link>
                {canCreate && (
                  <Button
                    color="primary"
                    onClick={handleOpenCreateModal}
                    className="px-3 py-2 fw-semibold rounded-3 d-inline-flex align-items-center gap-2 shadow-sm"
                  >
                    <i className="bi bi-plus-lg"></i> Thêm sân đấu mới
                  </Button>
                )}
              </div>
            </div>

            {/* Thanh tìm kiếm và bộ lọc */}
            <Form onSubmit={handleSearchSubmit} className="mb-4">
              <div className="p-3 bg-light rounded-3">
                <Row className="g-2 align-items-center">
                  <Col md={6} lg={3}>
                    <div className="input-group bg-white rounded-3 overflow-hidden border">
                      <span className="input-group-text bg-white border-0 text-muted ps-3">
                        <i className="bi bi-search"></i>
                      </span>
                      <Input
                        type="text"
                        className="border-0 shadow-none ps-2"
                        placeholder="Tìm kiếm theo mã, tên sân, loại sân..."
                        value={keyword}
                        onChange={(e) => setKeyword(e.target.value)}
                      />
                    </div>
                  </Col>
                  <Col md={6} lg={3}>
                    <Input
                      type="select"
                      className="bg-white border rounded-3 shadow-none"
                      value={cumSanFilter}
                      onChange={(e) => {
                        setCumSanFilter(e.target.value);
                        setPageIndex(1);
                      }}
                    >
                      <option value="">-- Tất cả cụm sân --</option>
                      {cumSans.map((cs) => (
                        <option key={cs.id} value={cs.id}>
                          {cs.ten}
                        </option>
                      ))}
                    </Input>
                  </Col>
                  <Col md={6} lg={2}>
                    <Input
                      type="select"
                      className="bg-white border rounded-3 shadow-none"
                      value={monTheThaoFilter}
                      onChange={(e) => {
                        setMonTheThaoFilter(e.target.value);
                        setPageIndex(1);
                      }}
                    >
                      <option value="">-- Tất cả môn thi đấu --</option>
                      {monTheThaos.map((m) => (
                        <option key={m.id} value={m.id}>
                          {m.ten}
                        </option>
                      ))}
                    </Input>
                  </Col>
                  <Col md={6} lg={2}>
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
                      <option value="true">Sẵn sàng thi đấu</option>
                      <option value="false">Tạm dừng bảo trì</option>
                    </Input>
                  </Col>
                  <Col md={12} lg={2}>
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

            {/* Bảng dữ liệu sân đấu */}
            {loading ? (
              <div className="text-center py-5">
                <Spinner color="primary" />
                <p className="mt-2 text-muted small">Đang tải danh sách sân đấu...</p>
              </div>
            ) : sanDaus.length === 0 ? (
              <div className="text-center py-5 text-muted border rounded-3 bg-light">
                <i className="bi bi-inbox fs-1 d-block mb-2 text-secondary"></i>
                Chưa có sân đấu nào được ghi nhận.
              </div>
            ) : (
              <div className="table-responsive">
                <Table hover className="align-middle mb-0">
                  <thead className="table-light text-uppercase fs-7 text-muted">
                    <tr>
                      <th style={{ width: '50px' }}>#</th>
                      <th>Mã Sân</th>
                      <th>Tên Sân Đấu</th>
                      <th>Môn Thi Đấu</th>
                      <th>Cụm Sân Trực Thuộc</th>
                      <th>Loại Sân</th>
                      <th>Sức Chứa</th>
                      <th>Trạng Thái</th>
                      <th className="text-end" style={{ width: '130px' }}>
                        Thao tác
                      </th>
                    </tr>
                  </thead>
                  <tbody>
                    {sanDaus.map((item, index) => (
                      <tr key={item.id}>
                        <td className="text-muted">{(pageIndex - 1) * pageSize + index + 1}</td>
                        <td>
                          <span className="badge bg-light text-primary border font-monospace">
                            {item.ma}
                          </span>
                        </td>
                        <td>
                          <div className="fw-semibold text-dark">{item.ten}</div>
                          {item.soSan && (
                            <small className="text-muted">Số hiệu: #{item.soSan}</small>
                          )}
                        </td>
                        <td>
                          {item.tenMonTheThao ? (
                            <Badge color="primary" pill className="px-2 py-1 fw-medium d-inline-flex align-items-center gap-1">
                              <i className="bi bi-trophy"></i>
                              {item.tenMonTheThao}
                            </Badge>
                          ) : (
                            <span className="badge bg-light text-secondary border">Chưa gán môn</span>
                          )}
                        </td>
                        <td>
                          <span className="fw-medium text-dark">
                            <i className="bi bi-geo-alt me-1 text-primary"></i>
                            {item.tenCumSan || '---'}
                          </span>
                        </td>
                        <td>
                          {item.loaiSan ? (
                            <Badge color="info" pill>
                              {item.loaiSan}
                            </Badge>
                          ) : (
                            <span className="text-muted small">---</span>
                          )}
                        </td>
                        <td>
                          <span className="small text-muted">
                            {item.sucChua ? `${item.sucChua.toLocaleString()} người` : '---'}
                          </span>
                        </td>
                        <td>
                          {item.trangThai ? (
                            <span className="badge rounded-pill bg-success-subtle text-success border border-success-subtle px-3 py-2 fw-medium d-inline-flex align-items-center gap-1">
                              <span className="p-1 bg-success rounded-circle d-inline-block" />
                              Sẵn sàng
                            </span>
                          ) : (
                            <span className="badge rounded-pill bg-secondary-subtle text-secondary border border-secondary-subtle px-3 py-2 fw-medium d-inline-flex align-items-center gap-1">
                              <span className="p-1 bg-secondary rounded-circle d-inline-block" />
                              Bảo trì
                            </span>
                          )}
                        </td>
                        <td className="text-end">
                          <div className="d-flex justify-content-end gap-1">
                            {canEdit && (
                              <Button
                                size="sm"
                                color="light"
                                className="btn-icon text-primary"
                                title="Chỉnh sửa sân đấu"
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
                                title="Xóa sân đấu"
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

      {/* Modal Thêm/Sửa Sân Đấu */}
      <Modal isOpen={modalOpen} toggle={() => setModalOpen(!modalOpen)} size="lg" centered>
        <ModalHeader toggle={() => setModalOpen(!modalOpen)} className="border-bottom">
          <div className="d-flex align-items-center gap-2">
            <i className={`bi ${editingId ? 'bi-pencil-square text-primary' : 'bi-plus-circle-fill text-success'}`}></i>
            <span className="fw-bold">{editingId ? 'Cập Nhật Sân Đấu' : 'Thêm Sân Đấu Mới'}</span>
          </div>
        </ModalHeader>
        <Form onSubmit={handleSubmitForm}>
          <ModalBody className="p-4">
            <Row className="g-3">
              <Col md={6}>
                <FormGroup>
                  <Label className="fw-semibold small">
                    Cụm sân trực thuộc <span className="text-danger">*</span>
                  </Label>
                  <Input
                    type="select"
                    required
                    value={formData.cumSanId}
                    onChange={(e) => setFormData({ ...formData, cumSanId: Number(e.target.value) })}
                  >
                    <option value="">-- Chọn Cụm sân --</option>
                    {cumSans.map((cs) => (
                      <option key={cs.id} value={cs.id}>
                        {cs.ten}
                      </option>
                    ))}
                  </Input>
                </FormGroup>
              </Col>
              <Col md={6}>
                <FormGroup>
                  <Label className="fw-semibold small">
                    Môn thể thao thi đấu (1 sân chỉ 1 môn)
                  </Label>
                  <Input
                    type="select"
                    value={formData.monTheThaoId || ''}
                    onChange={(e) =>
                      setFormData({
                        ...formData,
                        monTheThaoId: e.target.value ? Number(e.target.value) : undefined,
                      })
                    }
                  >
                    <option value="">-- Chưa gán môn thi đấu --</option>
                    {monTheThaos.map((m) => (
                      <option key={m.id} value={m.id}>
                        {m.ten} ({m.ma})
                      </option>
                    ))}
                  </Input>
                </FormGroup>
              </Col>

              <Col md={4}>
                <FormGroup>
                  <Label className="fw-semibold small">
                    Mã sân đấu <span className="text-danger">*</span>
                  </Label>
                  <Input
                    type="text"
                    required
                    placeholder="VD: SD_001"
                    value={formData.ma}
                    onChange={(e) => setFormData({ ...formData, ma: e.target.value })}
                  />
                </FormGroup>
              </Col>

              <Col md={5}>
                <FormGroup>
                  <Label className="fw-semibold small">
                    Tên sân đấu <span className="text-danger">*</span>
                  </Label>
                  <Input
                    type="text"
                    required
                    placeholder="VD: Sân bóng số 1 (Sân trung tâm)"
                    value={formData.ten}
                    onChange={(e) => setFormData({ ...formData, ten: e.target.value })}
                  />
                </FormGroup>
              </Col>

              <Col md={3}>
                <FormGroup>
                  <Label className="fw-semibold small">Số thứ tự sân</Label>
                  <Input
                    type="number"
                    min={1}
                    value={formData.soSan || 1}
                    onChange={(e) => setFormData({ ...formData, soSan: Number(e.target.value) })}
                  />
                </FormGroup>
              </Col>

              <Col md={6}>
                <FormGroup>
                  <Label className="fw-semibold small">Loại sân / Mặt sân</Label>
                  <Input
                    type="text"
                    placeholder="VD: Cỏ nhân tạo, Sàn gỗ, Mặt nỉ, Thảm PVC..."
                    value={formData.loaiSan}
                    onChange={(e) => setFormData({ ...formData, loaiSan: e.target.value })}
                  />
                </FormGroup>
              </Col>

              <Col md={6}>
                <FormGroup>
                  <Label className="fw-semibold small">Sức chứa khán giả</Label>
                  <Input
                    type="number"
                    placeholder="VD: 500"
                    value={formData.sucChua || ''}
                    onChange={(e) => setFormData({ ...formData, sucChua: Number(e.target.value) })}
                  />
                </FormGroup>
              </Col>

              <Col md={12}>
                <FormGroup>
                  <Label className="fw-semibold small">Mô tả / Trang thiết bị</Label>
                  <Input
                    type="textarea"
                    rows={3}
                    placeholder="Ghi chú về kích thước tiêu chuẩn, bảng điểm điện tử, đèn chiếu sáng..."
                    value={formData.moTa}
                    onChange={(e) => setFormData({ ...formData, moTa: e.target.value })}
                  />
                </FormGroup>
              </Col>

              <Col md={12}>
                <FormGroup check className="mt-2">
                  <Input
                    type="checkbox"
                    id="trangThaiSanDau"
                    checked={formData.trangThai}
                    onChange={(e) => setFormData({ ...formData, trangThai: e.target.checked })}
                  />
                  <Label check htmlFor="trangThaiSanDau" className="fw-semibold small">
                    Sẵn sàng thi đấu
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
              {editingId ? 'Cập nhật' : 'Tạo sân đấu'}
            </Button>
          </ModalFooter>
        </Form>
      </Modal>
    </Row>
  );
}

export default function AdminSanDauPage() {
  return (
    <Suspense fallback={<div className="text-center py-5"><Spinner color="primary" /></div>}>
      <AdminSanDauContent />
    </Suspense>
  );
}
