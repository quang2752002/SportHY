using AutoMapper;
using Dms.Application.DTOs;
using Dms.Application.Interfaces;
using Dms.Domain.Entities;
using Dms.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dms.Application.Services
{
    public class MenuService : IMenuService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public MenuService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<MenuDto>> GetAllAsync()
        {
            var menus = await _unitOfWork.Menus.FindAsync(m => m.IsDeleted != true);
            var sortedMenus = menus.OrderBy(m => m.SortOrder);
            return _mapper.Map<IEnumerable<MenuDto>>(sortedMenus);
        }

        public async Task<IEnumerable<MenuDto>> GetTreeAsync()
        {
            var allMenus = await _unitOfWork.Menus.FindAsync(m => m.IsDeleted != true);
            var dtoList = _mapper.Map<List<MenuDto>>(allMenus.OrderBy(m => m.SortOrder));

            // Map thành dạng cây phân cấp (Tree structure)
            var menuLookup = dtoList.ToDictionary(m => m.Id);
            var rootMenus = new List<MenuDto>();

            foreach (var item in dtoList)
            {
                if (item.ParentId.HasValue && menuLookup.TryGetValue(item.ParentId.Value, out var parentMenu))
                {
                    parentMenu.Children.Add(item);
                }
                else
                {
                    rootMenus.Add(item);
                }
            }

            return rootMenus;
        }

        public async Task<MenuDto?> GetByIdAsync(int id)
        {
            var menu = await _unitOfWork.Menus.GetByIdAsync(id);
            if (menu == null || menu.IsDeleted == true)
            {
                return null;
            }

            return _mapper.Map<MenuDto>(menu);
        }

        public async Task<MenuDto> CreateAsync(MenuDto dto)
        {
            var menu = _mapper.Map<Menu>(dto);
            menu.Created = DateTime.UtcNow;
            menu.IsDeleted = false;

            await _unitOfWork.Menus.AddAsync(menu);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<MenuDto>(menu);
        }

        public async Task<MenuDto?> UpdateAsync(int id, MenuDto dto)
        {
            var menu = await _unitOfWork.Menus.GetByIdAsync(id);
            if (menu == null || menu.IsDeleted == true)
            {
                return null;
            }

            // Không cho phép menu tự nhận mình làm cha để tránh vòng lặp vô hạn
            if (dto.ParentId.HasValue && dto.ParentId.Value == id)
            {
                throw new InvalidOperationException("Menu không thể tự làm menu cha của chính nó.");
            }

            menu.Title = dto.Title;
            menu.Url = dto.Url;
            menu.Icon = dto.Icon;
            menu.SortOrder = dto.SortOrder;
            menu.IsActive = dto.IsActive;
            menu.ParentId = dto.ParentId;
            menu.LastModified = DateTime.UtcNow;

            _unitOfWork.Menus.Update(menu);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<MenuDto>(menu);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var menu = await _unitOfWork.Menus.GetByIdAsync(id);
            if (menu == null || menu.IsDeleted == true)
            {
                return false;
            }

            // Xóa mềm các menu con trực tiếp hoặc ngắt liên kết cha
            var children = await _unitOfWork.Menus.FindAsync(m => m.ParentId == id && m.IsDeleted != true);
            foreach (var child in children)
            {
                child.IsDeleted = true;
                child.LastModified = DateTime.UtcNow;
                _unitOfWork.Menus.Update(child);
            }

            menu.IsDeleted = true;
            menu.LastModified = DateTime.UtcNow;
            _unitOfWork.Menus.Update(menu);
            await _unitOfWork.CompleteAsync();

            return true;
        }
    }
}
