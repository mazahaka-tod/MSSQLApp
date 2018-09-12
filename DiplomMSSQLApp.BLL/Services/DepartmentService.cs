using AutoMapper;
using DiplomMSSQLApp.BLL.BusinessModels;
using DiplomMSSQLApp.BLL.DTO;
using DiplomMSSQLApp.BLL.Infrastructure;
using DiplomMSSQLApp.DAL.Entities;
using DiplomMSSQLApp.DAL.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiplomMSSQLApp.BLL.Services
{
    public class DepartmentService : BaseService<DepartmentDTO>
    {
        IUnitOfWork Database { get; set; }

        public DepartmentService(IUnitOfWork uow)
        {
            Database = uow;
        }

        public DepartmentService() { }

        // Добавление нового отдела (с валидацией)
        public override async Task CreateAsync(DepartmentDTO item)
        {
            ValidationDepartment(item);
            Mapper.Initialize(cfg => cfg.CreateMap<DepartmentDTO, Department>());
            Database.Departments.Create(Mapper.Map<DepartmentDTO, Department>(item));
            await Database.SaveAsync();
        }

        // Удаление отдела
        public override async Task DeleteAsync(int id)
        {
            Department item = await Database.Departments.FindByIdAsync(id);
            if (item == null) return;
            Database.Departments.Remove(item);
            await Database.SaveAsync();
        }

        // Удаление всех отделов
        public override async Task DeleteAllAsync()
        {
            await Database.Departments.RemoveAllAsync();
            await Database.SaveAsync();
        }

        public override void Dispose()
        {
            Database.Dispose();
        }

        // Обновление информации об отделе
        public override async Task EditAsync(DepartmentDTO item)
        {
            ValidationDepartment(item);
            Mapper.Initialize(cfg => cfg.CreateMap<DepartmentDTO, Department>());
            Database.Departments.Update(Mapper.Map<DepartmentDTO, Department>(item));
            await Database.SaveAsync();
        }

        // Получение отдела по id
        public override async Task<DepartmentDTO> FindByIdAsync(int? id)
        {
            if (id == null)
                throw new ValidationException("Не установлено id отдела", "");
            Department d = await Database.Departments.FindByIdAsync(id.Value);
            if (d == null)
                throw new ValidationException("Отдел не найден", "");
            Mapper.Initialize(cfg => {
                cfg.CreateMap<Department, DepartmentDTO>();
                cfg.CreateMap<Employee, EmployeeDTO>()
                    .ForMember(dp => dp.BusinessTrips, opt => opt.Ignore())
                    .ForMember(dp => dp.Department, opt => opt.Ignore())
                    .ForMember(dp => dp.Post, opt => opt.Ignore());
            });
            return Mapper.Map<Department, DepartmentDTO>(d);
        }

        // Получение списка всех отделов
        public override async Task<IEnumerable<DepartmentDTO>> GetAllAsync()
        {
            Mapper.Initialize(cfg => cfg.CreateMap<Department, DepartmentDTO>()
                .ForMember(d => d.Employees, opt => opt.Ignore()));
            List<DepartmentDTO> col = Mapper.Map<IEnumerable<Department>, List<DepartmentDTO>>(await Database.Departments.GetAsync());
            return col;
        }

        // Валидация модели
        private void ValidationDepartment(DepartmentDTO item)
        {
            if (item.DepartmentName == null)
                throw new ValidationException("Требуется ввести название отдела", "DepartmentName");
        }

        // Нереализованные методы
        public override IEnumerable<DepartmentDTO> Get(EmployeeFilter f)
        {
            throw new System.NotImplementedException();
        }

        public override Task TestCreateAsync(int num, string path)
        {
            throw new System.NotImplementedException();
        }

        public override Task TestReadAsync(int num, string path, int val)
        {
            throw new System.NotImplementedException();
        }

        public override Task TestUpdateAsync(int num, string path)
        {
            throw new System.NotImplementedException();
        }

        public override Task TestDeleteAsync(int num, string path)
        {
            throw new System.NotImplementedException();
        }
    }
}
