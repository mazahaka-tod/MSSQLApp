using AutoMapper;
using DiplomMSSQLApp.BLL.DTO;
using DiplomMSSQLApp.BLL.Infrastructure;
using DiplomMSSQLApp.DAL.Entities;
using DiplomMSSQLApp.DAL.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace DiplomMSSQLApp.BLL.Services
{
    public class DepartmentService : BaseService<DepartmentDTO>
    {
        IUnitOfWork Database { get; set; }

        public DepartmentService(IUnitOfWork uow)
        {
            Database = uow;
        }

        // Добавление нового отдела (с валидацией)
        public override void Create(DepartmentDTO item)
        {
            ValidationDepartment(item);
            Mapper.Initialize(cfg => cfg.CreateMap<DepartmentDTO, Department>());
            Database.Departments.Create(Mapper.Map<DepartmentDTO, Department>(item));
            Database.Save();
        }

        // Получение списка всех отделов
        public override IEnumerable<DepartmentDTO> GetAll()
        {
            Mapper.Initialize(cfg => cfg.CreateMap<Department, DepartmentDTO>()
                .ForMember(d => d.Employees, opt => opt.Ignore())
            );
            return Mapper.Map<IEnumerable<Department>, List<DepartmentDTO>>(Database.Departments.Get());
        }

        // Удаление отдела
        public override void Delete(int id)
        {
            Department item = Database.Departments.FindById(id);
            if (item == null) return;
            Database.Departments.Remove(item);
            Database.Save();
        }

        // Удаление всех отделов
        public override void DeleteAll()
        {
            Database.Departments.RemoveAll();
            Database.Save();
        }

        // Обновление информации об отделе
        public override void Edit(DepartmentDTO item)
        {
            ValidationDepartment(item);
            Mapper.Initialize(cfg => cfg.CreateMap<DepartmentDTO, Department>());
            Database.Departments.Update(Mapper.Map<DepartmentDTO, Department>(item));
            Database.Save();
        }

        public override void Dispose()
        {
            Database.Dispose();
        }

        // Получение отдела по id
        public override DepartmentDTO FindById(int? id)
        {
            if (id == null)
                throw new ValidationException("Не установлено id отдела", "");
            Department d = Database.Departments.FindById(id.Value);
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

        // Валидация модели
        private void ValidationDepartment(DepartmentDTO item)
        {
            if (item.DepartmentName == null)
                throw new ValidationException("Требуется ввести название отдела", "DepartmentName");
        }
    }
}
