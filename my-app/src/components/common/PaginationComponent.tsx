'use client';

import React from 'react';

interface PaginationComponentProps {
  pageIndex: number;
  totalPages: number;
  totalCount: number;
  pageSize: number;
  onPageChange: (page: number) => void;
  onPageSizeChange?: (size: number) => void;
}

export const PaginationComponent: React.FC<PaginationComponentProps> = ({
  pageIndex,
  totalPages,
  totalCount,
  pageSize,
  onPageChange,
  onPageSizeChange,
}) => {
  if (totalCount === 0) return null;

  const startItem = (pageIndex - 1) * pageSize + 1;
  const endItem = Math.min(pageIndex * pageSize, totalCount);

  // Hiển thị tối đa 5 số trang xung quanh pageIndex
  const getPageNumbers = () => {
    const pages: number[] = [];
    const maxVisible = 5;
    let start = Math.max(1, pageIndex - Math.floor(maxVisible / 2));
    let end = Math.min(totalPages, start + maxVisible - 1);

    if (end - start + 1 < maxVisible) {
      start = Math.max(1, end - maxVisible + 1);
    }

    for (let i = start; i <= end; i++) {
      pages.push(i);
    }
    return pages;
  };

  const pages = getPageNumbers();

  const handlePageClick = (e: React.MouseEvent, page: number) => {
    e.preventDefault();
    if (page >= 1 && page <= totalPages && page !== pageIndex) {
      onPageChange(page);
    }
  };

  return (
    <div className="d-flex flex-wrap justify-content-between align-items-center pt-3 mt-2 border-top gap-3">
      {/* Thông tin số lượng & Chọn page size */}
      <div className="d-flex flex-wrap align-items-center text-secondary small gap-3">
        <div className="d-flex align-items-center gap-2">
          <span className="badge bg-light text-dark border px-2 py-1 rounded-2 fw-semibold">
            {totalCount}
          </span>
          <span className="text-muted">
            Hiển thị từ <strong className="text-dark">{startItem}</strong> đến <strong className="text-dark">{endItem}</strong> mục
          </span>
        </div>

        {onPageSizeChange && (
          <div className="d-flex align-items-center gap-2 ps-2 border-start">
            <span className="text-muted">Mỗi trang:</span>
            <select
              className="form-select form-select-sm bg-white border rounded-3 shadow-none fw-medium text-dark cursor-pointer"
              style={{ width: '80px', height: '34px' }}
              value={pageSize}
              onChange={(e) => onPageSizeChange(Number(e.target.value))}
            >
              <option value={5}>5 mục</option>
              <option value={10}>10 mục</option>
              <option value={20}>20 mục</option>
              <option value={50}>50 mục</option>
            </select>
          </div>
        )}
      </div>

      {/* Pagination Controls với phong cách Modern Pill / Rounded Buttons */}
      {totalPages > 1 && (
        <nav aria-label="Phân trang danh sách" className="d-flex align-items-center">
          <ul className="pagination pagination-sm mb-0 align-items-center gap-1">
            {/* Trang đầu */}
            <li className={`page-item ${pageIndex <= 1 ? 'disabled' : ''}`}>
              <button
                type="button"
                className="btn btn-sm btn-light border rounded-2 px-2 py-1 text-secondary d-flex align-items-center justify-content-center"
                style={{ minWidth: '34px', height: '34px' }}
                disabled={pageIndex <= 1}
                onClick={(e) => handlePageClick(e, 1)}
                title="Trang đầu"
              >
                <i className="bi bi-chevron-double-left small"></i>
              </button>
            </li>

            {/* Trang trước */}
            <li className={`page-item ${pageIndex <= 1 ? 'disabled' : ''}`}>
              <button
                type="button"
                className="btn btn-sm btn-light border rounded-2 px-2 py-1 text-secondary d-flex align-items-center justify-content-center"
                style={{ minWidth: '34px', height: '34px' }}
                disabled={pageIndex <= 1}
                onClick={(e) => handlePageClick(e, pageIndex - 1)}
                title="Trang trước"
              >
                <i className="bi bi-chevron-left small"></i>
              </button>
            </li>

            {/* Số 1 nếu bị rút gọn */}
            {pages[0] > 1 && (
              <>
                <li className="page-item">
                  <button
                    type="button"
                    className="btn btn-sm btn-light border rounded-2 px-2 py-1 text-dark fw-medium"
                    style={{ minWidth: '34px', height: '34px' }}
                    onClick={(e) => handlePageClick(e, 1)}
                  >
                    1
                  </button>
                </li>
                {pages[0] > 2 && (
                  <li className="page-item disabled px-1 text-muted">
                    <span>...</span>
                  </li>
                )}
              </>
            )}

            {/* Các số trang xung quanh */}
            {pages.map((p) => {
              const isActive = p === pageIndex;
              return (
                <li className="page-item" key={p}>
                  <button
                    type="button"
                    className={`btn btn-sm rounded-2 px-2 py-1 fw-semibold transition-all ${
                      isActive
                        ? 'btn-primary text-white shadow-sm border-primary'
                        : 'btn-light border text-dark'
                    }`}
                    style={{
                      minWidth: '34px',
                      height: '34px',
                      transform: isActive ? 'scale(1.05)' : 'none',
                    }}
                    onClick={(e) => handlePageClick(e, p)}
                  >
                    {p}
                  </button>
                </li>
              );
            })}

            {/* Trang cuối nếu bị rút gọn */}
            {pages[pages.length - 1] < totalPages && (
              <>
                {pages[pages.length - 1] < totalPages - 1 && (
                  <li className="page-item disabled px-1 text-muted">
                    <span>...</span>
                  </li>
                )}
                <li className="page-item">
                  <button
                    type="button"
                    className="btn btn-sm btn-light border rounded-2 px-2 py-1 text-dark fw-medium"
                    style={{ minWidth: '34px', height: '34px' }}
                    onClick={(e) => handlePageClick(e, totalPages)}
                  >
                    {totalPages}
                  </button>
                </li>
              </>
            )}

            {/* Trang tiếp theo */}
            <li className={`page-item ${pageIndex >= totalPages ? 'disabled' : ''}`}>
              <button
                type="button"
                className="btn btn-sm btn-light border rounded-2 px-2 py-1 text-secondary d-flex align-items-center justify-content-center"
                style={{ minWidth: '34px', height: '34px' }}
                disabled={pageIndex >= totalPages}
                onClick={(e) => handlePageClick(e, pageIndex + 1)}
                title="Trang sau"
              >
                <i className="bi bi-chevron-right small"></i>
              </button>
            </li>

            {/* Trang cuối */}
            <li className={`page-item ${pageIndex >= totalPages ? 'disabled' : ''}`}>
              <button
                type="button"
                className="btn btn-sm btn-light border rounded-2 px-2 py-1 text-secondary d-flex align-items-center justify-content-center"
                style={{ minWidth: '34px', height: '34px' }}
                disabled={pageIndex >= totalPages}
                onClick={(e) => handlePageClick(e, totalPages)}
                title="Trang cuối"
              >
                <i className="bi bi-chevron-double-right small"></i>
              </button>
            </li>
          </ul>
        </nav>
      )}
    </div>
  );
};
