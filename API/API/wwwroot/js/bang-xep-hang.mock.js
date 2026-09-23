(function (window) {
    'use strict';

    function createRankingRows(entries) {
        return entries.map(function (entry, index) {
            return {
                donViId: 900 + index,
                tenDonVi: entry[0],
                maDonVi: entry[1],
                soHuyChuongVang: entry[2],
                soHuyChuongBac: entry[3],
                soHuyChuongDong: entry[4],
                tongSoHuyChuong: entry[2] + entry[3] + entry[4],
                tongDiem: entry[2] * 3 + entry[3] * 2 + entry[4],
                xepHang: index + 1
            };
        });
    }

    function createResultRows(entries, isTeamSport) {
        return entries.map(function (entry, index) {
            return {
                dangKyThiDauId: 950 + index,
                xepHang: entry[0],
                tenDoi: isTeamSport ? entry[1] : null,
                maDoi: isTeamSport ? entry[2] : null,
                tenVanDongVien: isTeamSport ? null : entry[1],
                maVanDongVien: isTeamSport ? null : entry[2],
                tenDonVi: entry[3],
                maDonVi: entry[4],
                tenLoaiHuyChuong: entry[5]
            };
        });
    }

    function createGroup(id, code, name, categoryName, totalSports, entries, isTeamSport, resultEntries) {
        var rankings = createRankingRows(entries);
        var eventResults = createResultRows(resultEntries || [], isTeamSport !== false);
        var totals = eventResults.length > 0
            ? eventResults.reduce(function (result, row) {
                if (row.xepHang === 1) result.gold++;
                else if (row.xepHang === 2) result.silver++;
                else result.bronze++;
                return result;
            }, { gold: 0, silver: 0, bronze: 0 })
            : rankings.reduce(function (result, row) {
                result.gold += row.soHuyChuongVang;
                result.silver += row.soHuyChuongBac;
                result.bronze += row.soHuyChuongDong;
                return result;
            }, { gold: 0, silver: 0, bronze: 0 });

        return {
            danhMucId: id,
            monTheThaoId: id,
            maDanhMuc: code,
            maMon: code,
            tenDanhMuc: categoryName || name,
            tenMon: name,
            laMonDongDoi: isTeamSport !== false,
            tongSoMon: totalSports,
            tongHuyChuongVang: totals.gold,
            tongHuyChuongBac: totals.silver,
            tongHuyChuongDong: totals.bronze,
            tongHuyChuong: totals.gold + totals.silver + totals.bronze,
            bangXepHang: rankings,
            ketQua: eventResults
        };
    }

    var overallRankings = createRankingRows([
        ['Đại học Bách Khoa', 'DV_BK', 5, 2, 1],
        ['Đại học Kinh tế TP.HCM', 'DV_UEH', 3, 4, 2],
        ['Đại học Quốc gia Hà Nội', 'DHQGHN', 2, 2, 4],
        ['Đại học Sư phạm TDTT', 'SPTDTT', 1, 3, 3],
        ['Đại học Cần Thơ', 'DHCT', 1, 1, 1],
        ['Cao đẳng Y tế Thành phố', 'CDYT', 0, 2, 2],
        ['Đại học Công nghệ', 'DHCN', 0, 1, 2]
    ]);

    var categories = [
        createGroup(101, 'BONG_DA', 'Bóng đá', null, 3, [
            ['Đại học Bách Khoa', 'DV_BK', 2, 1, 0],
            ['Đại học Kinh tế TP.HCM', 'DV_UEH', 1, 1, 1],
            ['Đại học Quốc gia Hà Nội', 'DHQGHN', 0, 1, 1]
        ]),
        createGroup(102, 'VO_THUAT', 'Võ thuật', null, 4, [
            ['Đại học Kinh tế TP.HCM', 'DV_UEH', 3, 1, 2],
            ['Đại học Bách Khoa', 'DV_BK', 2, 2, 1],
            ['Đại học Cần Thơ', 'DHCT', 1, 1, 2],
            ['Đại học Quốc gia Hà Nội', 'DHQGHN', 0, 2, 1]
        ]),
        createGroup(103, 'CAU_LONG', 'Cầu lông', null, 2, [
            ['Đại học Bách Khoa', 'DV_BK', 1, 2, 1],
            ['Đại học Sư phạm TDTT', 'SPTDTT', 1, 1, 2],
            ['Đại học Kinh tế TP.HCM', 'DV_UEH', 0, 1, 1]
        ]),
        createGroup(104, 'DIEN_KINH', 'Điền kinh', null, 5, [
            ['Đại học Quốc gia Hà Nội', 'DHQGHN', 4, 2, 1],
            ['Đại học Bách Khoa', 'DV_BK', 2, 3, 2],
            ['Đại học Cần Thơ', 'DHCT', 1, 2, 3]
        ])
    ];

    var sports = [
        createGroup(201, 'BONG_DA_NAM', 'Bóng đá nam', 'Bóng đá', 1, [
            ['Đại học Bách Khoa', 'DV_BK', 1, 0, 0],
            ['Đại học Kinh tế TP.HCM', 'DV_UEH', 0, 1, 0],
            ['Đại học Quốc gia Hà Nội', 'DHQGHN', 0, 0, 1]
        ], true, [
            [1, 'Bách Khoa FC', 'BK_FC', 'Đại học Bách Khoa', 'DV_BK', 'Vàng'],
            [2, 'UEH United', 'UEH_U', 'Đại học Kinh tế TP.HCM', 'DV_UEH', 'Bạc'],
            [3, 'ĐHQG Hà Nội FC', 'DHQG_FC', 'Đại học Quốc gia Hà Nội', 'DHQGHN', 'Đồng']
        ]),
        createGroup(202, 'VO_THUAT_KATA', 'Karate Kata', 'Võ thuật', 1, [
            ['Đại học Kinh tế TP.HCM', 'DV_UEH', 1, 0, 0],
            ['Đại học Bách Khoa', 'DV_BK', 0, 1, 0],
            ['Đại học Cần Thơ', 'DHCT', 0, 0, 1]
        ], false, [
            [1, 'Lê Hoàng Anh', 'VDV_KA_01', 'Đại học Kinh tế TP.HCM', 'DV_UEH', 'Vàng'],
            [2, 'Trần Minh Quân', 'VDV_KA_03', 'Đại học Bách Khoa', 'DV_BK', 'Bạc'],
            [3, 'Phạm Thu Trang', 'VDV_KA_04', 'Đại học Cần Thơ', 'DHCT', 'Đồng']
        ]),
        createGroup(203, 'CAU_LONG_DON_NAM', 'Cầu lông đơn nam', 'Cầu lông', 1, [
            ['Đại học Sư phạm TDTT', 'SPTDTT', 1, 0, 0],
            ['Đại học Bách Khoa', 'DV_BK', 0, 1, 0],
            ['Đại học Cần Thơ', 'DHCT', 0, 0, 1]
        ], false, [
            [1, 'Nguyễn Thành Đạt', 'VDV_CL_01', 'Đại học Sư phạm TDTT', 'SPTDTT', 'Vàng'],
            [2, 'Đỗ Minh Long', 'VDV_CL_02', 'Đại học Bách Khoa', 'DV_BK', 'Bạc'],
            [3, 'Lê Quốc Huy', 'VDV_CL_03', 'Đại học Cần Thơ', 'DHCT', 'Đồng']
        ]),
        createGroup(204, 'CHAY_100M', 'Điền kinh 100m', 'Điền kinh', 1, [
            ['Đại học Quốc gia Hà Nội', 'DHQGHN', 1, 0, 0],
            ['Đại học Bách Khoa', 'DV_BK', 0, 1, 0],
            ['Đại học Cần Thơ', 'DHCT', 0, 0, 1]
        ], false, [
            [1, 'Nguyễn Minh Phúc', 'VDV_DK_01', 'Đại học Quốc gia Hà Nội', 'DHQGHN', 'Vàng'],
            [2, 'Lê Hoàng Nam', 'VDV_DK_03', 'Đại học Bách Khoa', 'DV_BK', 'Bạc'],
            [3, 'Phạm Gia Huy', 'VDV_DK_04', 'Đại học Cần Thơ', 'DHCT', 'Đồng']
        ]),
        createGroup(205, 'BONG_RO_NAM', 'Bóng rổ nam', 'Bóng rổ', 1, [
            ['Đại học Kinh tế TP.HCM', 'DV_UEH', 1, 0, 0],
            ['Đại học Bách Khoa', 'DV_BK', 0, 1, 0],
            ['Đại học Quốc gia Hà Nội', 'DHQGHN', 0, 0, 1]
        ], true, [
            [1, 'UEH Basketball', 'UEH_BB', 'Đại học Kinh tế TP.HCM', 'DV_UEH', 'Vàng'],
            [2, 'BK Warriors', 'BK_W', 'Đại học Bách Khoa', 'DV_BK', 'Bạc'],
            [3, 'Hanoi Eagles', 'HN_E', 'Đại học Quốc gia Hà Nội', 'DHQGHN', 'Đồng']
        ])
    ];

    function normalize(value) {
        return (value || '')
            .toString()
            .normalize('NFD')
            .replace(/[\u0300-\u036f]/g, '')
            .toLowerCase()
            .replace(/[^a-z0-9]+/g, ' ')
            .trim();
    }

    function findFixture(item, fixtures, index, isSport) {
        var itemCode = normalize(isSport ? item.maMon : item.maDanhMuc);
        var itemName = normalize(isSport ? item.tenMon : item.tenDanhMuc);
        var fixture = fixtures.find(function (candidate) {
            var candidateCode = normalize(isSport ? candidate.maMon : candidate.maDanhMuc);
            var candidateName = normalize(isSport ? candidate.tenMon : candidate.tenDanhMuc);
            return (itemCode && candidateCode && (itemCode === candidateCode || itemCode.includes(candidateCode))) ||
                (itemName && candidateName && (itemName === candidateName || itemName.includes(candidateName)));
        });

        return fixture || fixtures[index % fixtures.length];
    }

    function withFallbackRows(item, fixture) {
        var rows = fixture.bangXepHang.map(function (row, index) {
            return Object.assign({}, row, { donViId: 9000 + index });
        });
        var resultRows = (fixture.ketQua || []).map(function (row, index) {
            return Object.assign({}, row, { dangKyThiDauId: 9500 + index });
        });
        var totals = resultRows.length > 0
            ? resultRows.reduce(function (result, row) {
                if (row.xepHang === 1) result.gold++;
                else if (row.xepHang === 2) result.silver++;
                else result.bronze++;
                return result;
            }, { gold: 0, silver: 0, bronze: 0 })
            : rows.reduce(function (result, row) {
                result.gold += row.soHuyChuongVang;
                result.silver += row.soHuyChuongBac;
                result.bronze += row.soHuyChuongDong;
                return result;
            }, { gold: 0, silver: 0, bronze: 0 });

        return Object.assign({}, fixture, item, {
            _isMockFallback: true,
            bangXepHang: rows,
            ketQua: resultRows,
            tongHuyChuongVang: totals.gold,
            tongHuyChuongBac: totals.silver,
            tongHuyChuongDong: totals.bronze,
            tongHuyChuong: totals.gold + totals.silver + totals.bronze
        });
    }

    function fillEmptyGroups(items, fixtures, isSport) {
        return items.map(function (item, index) {
            var rankingRows = isSport && item ? item.ketQua : item && item.bangXepHang;
            if (Array.isArray(rankingRows) && rankingRows.length > 0) {
                return Object.assign({}, item, { _isMockFallback: false });
            }

            return withFallbackRows(item || {}, findFixture(item || {}, fixtures, index, isSport));
        });
    }

    function markAllAsFallback(fixtures) {
        return fixtures.map(function (fixture) {
            return withFallbackRows(fixture, fixture);
        });
    }

    window.BangXepHangMockData = {
        overall: {
            bangXepHang: overallRankings,
            tongSoHuyChuongVang: overallRankings.reduce(function (sum, row) { return sum + row.soHuyChuongVang; }, 0),
            tongSoHuyChuongBac: overallRankings.reduce(function (sum, row) { return sum + row.soHuyChuongBac; }, 0),
            tongSoHuyChuongDong: overallRankings.reduce(function (sum, row) { return sum + row.soHuyChuongDong; }, 0),
            tongSoHuyChuongDaTrao: overallRankings.reduce(function (sum, row) { return sum + row.tongSoHuyChuong; }, 0)
        },
        categories: categories,
        sports: sports,
        fillEmptyCategories: function (items) { return fillEmptyGroups(items, categories, false); },
        fillEmptySports: function (items) { return fillEmptyGroups(items, sports, true); },
        getFallbackCategories: function () { return markAllAsFallback(categories); },
        getFallbackSports: function () { return markAllAsFallback(sports); }
    };
})(window);
