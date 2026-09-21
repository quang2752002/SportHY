'use client';

import React, { useEffect, useState, useCallback } from 'react';
import Link from 'next/link';
import {
  Row,
  Col,
  Table,
  Card,
  CardBody,
  Button,
  Input,
  Spinner,
  Form,
  Badge,
  Modal,
  ModalHeader,
  ModalBody,
  ModalFooter,
} from 'reactstrap';
import { giaiDauService } from '@/services';
import {
  GiaiDau,
  PhamViGiaiDau,
  PhamViGiaiDauLabels,
  TrangThaiGiaiDau,
  TrangThaiGiaiDauLabels,
} from '@/types';
import { PaginationComponent } from '@/components/common/PaginationComponent';
import { useAuth, useToast } from '@/context/AuthContext';
import { Permissions } from '@/constants/permissions';

export default function AdminGiaiDauPage() {
  const { hasPermission, user } = useAuth();
  const toast = useToast();
  const isAdmin = (user?.roles || []).some((r) => String(r).toLowerCase() === 'admin');
  const canCreate = isAdmin || hasPermission(Permissions.GiaiDau.Create);
  const canEdit = isAdmin || hasPermission(Permissions.GiaiDau.Edit);
  const canDelete = isAdmin || hasPermission(Permissions.GiaiDau.Delete);

  const [giaiDaus, setGiaiDaus] = useState<GiaiDau[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(1);
  const [pageIndex, setPageIndex] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [keyword, setKeyword] = useState('');
  const [trangThaiFilter, setTrangThaiFilter] = useState<string>('');
  const [phamViFilter, setPhamViFilter] = useState<string>('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Modal Xóa
  const [deleteModalOpen, setDeleteModalOpen] = useState(false);
  const [deletingItem, setDeletingItem] = useState<GiaiDau | null>(null);
  const [submittingDelete, setSubmittingDelete] = useState(false);

  // Fetch dữ liệu từ backend
  const loadGiaiDaus = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await giaiDauService.getPaged({
        pageIndex,
        pageSize,
        keyword: keyword.trim() || undefined,
        trangThai: trangThaiFilter ? (Number(trangThaiFilter) as TrangThaiGiaiDau) : undefined,
        phamVi: phamViFilter ? (Number(phamViFilter) as PhamViGiaiDau) : undefined,
      });
      setGiaiDaus(data.items || []);
      setTotalCount(data.totalCount || 0);
      setTotalPages(data.totalPages || 1);
    } catch (err: any) {
      console.error('Lỗi khi tải danh sách giải đấu:', err);
      setError(err?.message || 'Không thể kết nối đến máy chủ Backend.');
    } finally {
      setLoading(false);
    }
  }, [pageIndex, pageSize, keyword, trangThaiFilter, phamViFilter]);

  useEffect(() => {
    loadGiaiDaus();
  }, [loadGiaiDaus]);

  const handleSearchSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setPageIndex(1);
    loadGiaiDaus();
  };

  // Mở modal xác nhận xóa
  const handleOpenDeleteModal = (item: GiaiDau) => {
    setDeletingItem(item);
    setDeleteModalOpen(true);
  };

  // Xác nhận xóa giải đấu
  const handleConfirmDelete = async () => {
    if (!deletingItem) return;
    setSubmittingDelete(true);
    try {
      await giaiDauService.delete(deletingItem.id);
      toast.success(`Đã xóa giải đấu "${deletingItem.ten}" thành công!`);
      setDeleteModalOpen(false);
      setDeletingItem(null);
      loadGiaiDaus();
    } catch (err: any) {
      toast.error('Không thể xóa giải đấu: ' + (err?.response?.data?.message || err?.message || 'Lỗi server'));
    } finally {
      setSubmittingDelete(false);
    }
  };

  // Badge hiển thị trạng thái theo Enum & Text
  const renderTrangThai = (trangThai?: TrangThaiGiaiDau, text?: string) => {
    const label = text || (trangThai ? TrangThaiGiaiDauLabels[trangThai] : 'Bản nháp');
    switch (trangThai) {
      case TrangThaiGiaiDau.DangDienRa:
        return (
          <span className="badge rounded-pill bg-success-subtle text-success border border-success-subtle px-3 py-2 fw-medium d-inline-flex align-items-center gap-1">
            <span className="p-1 bg-success rounded-circle d-inline-block" />
            {label}
          </span>
        );
      case TrangThaiGiaiDau.SapDienRa:
        return (
          <span className="badge rounded-pill bg-warning-subtle text-warning-emphasis border border-warning-subtle px-3 py-2 fw-medium d-inline-flex align-items-center gap-1">
            <span className="p-1 bg-warning rounded-circle d-inline-block" />
            {label}
          </span>
        );
      case TrangThaiGiaiDau.KetThuc:
        return (
          <span className="badge rounded-pill bg-secondary-subtle text-secondary border border-secondary-subtle px-3 py-2 fw-medium d-inline-flex align-items-center gap-1">
            <span className="p-1 bg-secondary rounded-circle d-inline-block" />
            {label}
          </span>
        );
      case TrangThaiGiaiDau.Huy:
        return (
          <span className="badge rounded-pill bg-danger-subtle text-danger border border-danger-subtle px-3 py-2 fw-medium d-inline-flex align-items-center gap-1">
            <span className="p-1 bg-danger rounded-circle d-inline-block" />
            {label}
          </span>
        );
      case TrangThaiGiaiDau.Nhap:
      default:
        return (
          <span className="badge rounded-pill bg-light text-dark border px-3 py-2 fw-medium d-inline-flex align-items-center gap-1">
            <span className="p-1 bg-secondary rounded-circle d-inline-block" />
            {label}
          </span>
        );
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
                    <i className="bi bi-trophy-fill fs-4"></i>
                  </div>
                  <div>
                    <h4 className="fw-bold mb-0 text-dark">Quản lý Giải đấu </h4>
                    <p className="text-muted small mb-0">
                      Danh mục giải đấu thể thao, điều lệ &amp; các môn thi đấu ({totalCount} giải đấu)
                    </p>
                  </div>
                </div>
              </div>
              {canCreate && (
                <div>
                  <Link
                    href="/quan-ly-giai/giai-dau/create"
                    className="btn btn-primary px-3 py-2 fw-semibold rounded-3 d-inline-flex align-items-center gap-2 shadow-sm"
                  >
                    <i className="bi bi-plus-lg"></i> Thêm giải đấu mới
                  </Link>
                </div>
              )}
            </div>

            {/* Thanh tìm kiếm và bộ lọc */}
            <Form onSubmit={handleSearchSubmit} className="mb-4">
              <div className="p-3 bg-light rounded-3">
                <Row className="g-2 align-items-center">
                  <Col md={5} lg={4}>
                    <div className="input-group bg-white rounded-3 overflow-hidden border">
                      <span className="input-group-text bg-white border-0 text-muted ps-3">
                        <i className="bi bi-search"></i>
                      </span>
                      <Input
                        type="text"
                        className="border-0 shadow-none ps-2"
                        placeholder="Tìm kiếm theo mã, tên giải đấu..."
                        value={keyword}
                        onChange={(e) => setKeyword(e.target.value)}
                      />
                    </div>
                  </Col>
                  <Col md={3} lg={3}>
                    <Input
                      type="select"
                      className="bg-white border rounded-3 shadow-none"
                      value={phamViFilter}
                      onChange={(e) => {
                        setPhamViFilter(e.target.value);
                        setPageIndex(1);
                      }}
                    >
                      <option value="">-- Tất cả phạm vi --</option>
                      <option value={PhamViGiaiDau.TatCa}>{PhamViGiaiDauLabels[PhamViGiaiDau.TatCa]}</option>
                      <option value={PhamViGiaiDau.TheoKhoi}>{PhamViGiaiDauLabels[PhamViGiaiDau.TheoKhoi]}</option>
                    </Input>
                  </Col>
                  <Col md={3} lg={3}>
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
                      <option value={TrangThaiGiaiDau.Nhap}>{TrangThaiGiaiDauLabels[TrangThaiGiaiDau.Nhap]}</option>
                      <option value={TrangThaiGiaiDau.SapDienRa}>{TrangThaiGiaiDauLabels[TrangThaiGiaiDau.SapDienRa]}</option>
                      <option value={TrangThaiGiaiDau.DangDienRa}>{TrangThaiGiaiDauLabels[TrangThaiGiaiDau.DangDienRa]}</option>
                      <option value={TrangThaiGiaiDau.KetThuc}>{TrangThaiGiaiDauLabels[TrangThaiGiaiDau.KetThuc]}</option>
                      <option value={TrangThaiGiaiDau.Huy}>{TrangThaiGiaiDauLabels[TrangThaiGiaiDau.Huy]}</option>
                    </Input>
                  </Col>
                  <Col md={1} lg={2}>
                    <Button color="dark" type="submit" className="w-100 rounded-3">
                      Tìm kiếm
                    </Button>
                  </Col>
                </Row>
              </div>
            </Form>

            {/* Thông báo lỗi */}
            {error && (
              <div className="alert alert-danger d-flex align-items-center mb-4" role="alert">
                <i className="bi bi-exclamation-triangle-fill me-2 fs-5"></i>
                <div>{error}</div>
              </div>
            )}

            {/* Bảng dữ liệu giải đấu */}
            {loading ? (
              <div className="text-center py-5">
                <Spinner color="primary" />
                <p className="mt-2 text-muted small">Đang tải danh sách giải đấu...</p>
              </div>
            ) : giaiDaus.length === 0 ? (
              <div className="text-center py-5 text-muted border rounded-3 bg-light">
                <i className="bi bi-inbox fs-1 d-block mb-2 text-secondary"></i>
                Chưa có giải đấu nào được ghi nhận.
              </div>
            ) : (
              <div className="table-responsive">
                <Table hover className="align-middle mb-0">
                  <thead className="table-light text-uppercase fs-7 text-muted">
                    <tr>
                      <th style={{ width: '50px' }}>#</th>
                      <th>Mã Giải</th>
                      <th>Tên Giải Đấu</th>
                      <th>Phạm Vi</th>
                      <th>Thời Gian Thi Đấu</th>
                      <th>Địa Điểm</th>
                      <th>Trạng Thái</th>
                      <th className="text-end" style={{ width: '130px' }}>
                        Thao tác
                      </th>
                    </tr>
                  </thead>
                  <tbody>
                    {giaiDaus.map((item, index) => (
                      <tr key={item.id}>
                        <td className="text-muted">{(pageIndex - 1) * pageSize + index + 1}</td>
                        <td>
                          <span className="badge bg-light text-primary border font-monospace">
                            {item.ma}
                          </span>
                        </td>
                        <td>
                          <div className="d-flex align-items-center gap-2.5">
                            {item.hinhAnh ? (
                              <div
                                className="rounded-2 overflow-hidden border flex-shrink-0"
                                style={{ width: '48px', height: '36px', backgroundColor: '#f8f9fa' }}
                              >
                                <img
                                  src={
                                    item.hinhAnh.startsWith('http')
                                      ? item.hinhAnh
                                      : `${process.env.NEXT_PUBLIC_API_URL || 'https://localhost:7085'}${item.hinhAnh}`
                                  }
                                  alt={item.ten}
                                  className="w-100 h-100 object-fit-cover"
                                />
                              </div>
                            ) : (
                              <div
                                className="rounded-2 border flex-shrink-0 d-flex align-items-center justify-content-center bg-light text-muted"
                                style={{ width: '48px', height: '36px' }}
                              >
                                <i className="bi bi-image" style={{ fontSize: '14px' }}></i>
                              </div>
                            )}
                            <div>
                              <div className="fw-semibold text-dark">{item.ten}</div>
                              {item.slug && (
                                <div className="mt-0.5">
                                  <Link
                                    href={`/giai-dau/${item.slug}`}
                                    target="_blank"
                                    className="text-decoration-none small d-inline-flex align-items-center gap-1 text-primary"
                                    title="Xem trang giải đấu chuẩn SEO"
                                  >
                                    <i className="bi bi-link-45deg"></i>
                                    <span className="font-monospace text-muted" style={{ fontSize: '11px' }}>
                                      /giai-dau/{item.slug}
                                    </span>
                                    <i className="bi bi-box-arrow-up-right" style={{ fontSize: '10px' }}></i>
                                  </Link>
                                </div>
                              )}
                            </div>
                          </div>
                          {item.moTa && !item.slug && (
                            <small className="text-muted text-truncate d-block" style={{ maxWidth: '280px' }}>
                              {item.moTa}
                            </small>
                          )}
                        </td>
                        <td>
                          {item.phamVi === PhamViGiaiDau.TheoKhoi ? (
                            <Badge color="info" pill>
                              {item.phamViText || PhamViGiaiDauLabels[PhamViGiaiDau.TheoKhoi]}
                            </Badge>
                          ) : (
                            <Badge color="secondary" pill>
                              {item.phamViText || PhamViGiaiDauLabels[PhamViGiaiDau.TatCa]}
                            </Badge>
                          )}
                        </td>
                        <td>
                          <div className="small">
                            <div>
                              <i className="bi bi-calendar-check me-1 text-success"></i>
                              {item.ngayBatDau ? new Date(item.ngayBatDau).toLocaleDateString('vi-VN') : '---'}
                            </div>
                            <div className="text-muted">
                              <i className="bi bi-calendar-x me-1 text-danger"></i>
                              {item.ngayKetThuc ? new Date(item.ngayKetThuc).toLocaleDateString('vi-VN') : '---'}
                            </div>
                          </div>
                        </td>
                        <td>
                          <div className="small text-muted d-flex align-items-center">
                            <i className="bi bi-geo-alt me-1 text-secondary"></i>
                            {item.diaDiem || 'Chưa cập nhật'}
                          </div>
                        </td>
                        <td>{renderTrangThai(item.trangThai, item.trangThaiText)}</td>
                        <td className="text-end">
                          <div className="d-flex justify-content-end gap-1">
                            {canEdit && (
                              <Link
                                href={`/quan-ly-giai/giai-dau/${item.id}`}
                                className="btn btn-sm btn-light btn-icon text-primary d-inline-flex align-items-center justify-content-center"
                                title="Chỉnh sửa giải đấu"
                              >
                                <i className="bi bi-pencil-square"></i>
                              </Link>
                            )}
                            {canDelete && (
                              <Button
                                size="sm"
                                color="light"
                                className="btn-icon text-danger"
                                title="Xóa giải đấu"
                                onClick={() => handleOpenDeleteModal(item)}
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

        {/* MODAL XÁC NHẬN XÓA GIẢI ĐẤU (GIỐNG TRANG KHỐI) */}
        <Modal
          isOpen={deleteModalOpen}
          toggle={() => setDeleteModalOpen(!deleteModalOpen)}
          centered
          size="sm"
        >
          <ModalHeader
            toggle={() => setDeleteModalOpen(!deleteModalOpen)}
            className="text-danger border-bottom-0 pb-0"
          >
            <div className="d-flex align-items-center gap-2">
              <div className="p-2 bg-danger-subtle text-danger rounded-circle d-inline-flex">
                <i className="bi bi-exclamation-triangle-fill fs-5"></i>
              </div>
              <span className="fw-bold">Xác nhận xóa</span>
            </div>
          </ModalHeader>
          <ModalBody className="py-3">
            Bạn có chắc chắn muốn xóa giải đấu:
            <div className="p-2.5 my-2 bg-light rounded-3 border">
              <div className="fw-bold text-dark">{deletingItem?.ten}</div>
              <div className="text-muted small font-monospace">Mã giải: #{deletingItem?.ma}</div>
            </div>
            <p className="text-muted small mb-0">
              Hành động này sẽ chuyển trạng thái giải đấu sang đã xóa và ẩn khỏi hệ thống.
            </p>
          </ModalBody>
          <ModalFooter className="border-top-0 pt-0">
            <Button
              color="secondary"
              outline
              onClick={() => setDeleteModalOpen(false)}
              disabled={submittingDelete}
              className="rounded-3 px-3"
            >
              Hủy bỏ
            </Button>
            <Button
              color="danger"
              onClick={handleConfirmDelete}
              disabled={submittingDelete}
              className="rounded-3 px-3 fw-semibold d-inline-flex align-items-center gap-1.5"
            >
              {submittingDelete ? <Spinner size="sm" /> : <i className="bi bi-trash-fill"></i>}
              Đồng Ý Xóa
            </Button>
          </ModalFooter>
        </Modal>
      </Col>
    </Row>
  );
}
