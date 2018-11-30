using AutoMapper;
using DiplomMSSQLApp.BLL.DTO;
using DiplomMSSQLApp.BLL.Infrastructure;
using DiplomMSSQLApp.DAL.Entities;
using DiplomMSSQLApp.DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace DiplomMSSQLApp.BLL.Services {
    public class DepartmentService : BaseService<DepartmentDTO> {
        public IUnitOfWork Database { get; set; }

        public DepartmentService(IUnitOfWork uow) {
            Database = uow;
        }

        public DepartmentService() { }

        // Добавление нового отдела
        public override async Task CreateAsync(DepartmentDTO dDto) {
            Department department = await MapDTOAndDomainModelWithValidation(dDto);
            Database.Departments.Create(department);
            await Database.SaveAsync();
        }

        private async Task<Department> MapDTOAndDomainModelWithValidation(DepartmentDTO dDto) {
            await ValidationDepartment(dDto);
            Mapper.Initialize(cfg => cfg.CreateMap<DepartmentDTO, Department>()
                                        .ForMember(d => d.Posts, opt => opt.Ignore()));
            Department department = Mapper.Map<DepartmentDTO, Department>(dDto);
            return department;
        }

        private async Task ValidationDepartment(DepartmentDTO department) {
            if (department.DepartmentName == null)
                throw new ValidationException("Требуется ввести название отдела", "DepartmentName");
            int departmentsCount = await Database.Departments.CountAsync(d => d.Code == department.Code && d.Id != department.Id);
            if (departmentsCount > 0)
                throw new ValidationException("Отдел с таким кодом уже существует", "Code");
            departmentsCount = await Database.Departments.CountAsync(d => d.DepartmentName == department.DepartmentName && d.Id != department.Id);
            if (departmentsCount > 0)
                throw new ValidationException("Отдел с таким названием уже существует", "DepartmentName");
            if (department.ManagerId != null) {
                departmentsCount = await Database.Departments.CountAsync(d => d.ManagerId == department.ManagerId && d.Id != department.Id);
                if (departmentsCount > 0)
                    throw new ValidationException("Сотрудник уже является начальником другого отдела", "ManagerId");
            }
        }

        // Обновление информации об отделе
        public override async Task EditAsync(DepartmentDTO dDto) {
            Department department = await MapDTOAndDomainModelWithValidation(dDto);
            Database.Departments.Update(department);
            await Database.SaveAsync();
        }

        // Получение отдела по id
        public override async Task<DepartmentDTO> FindByIdAsync(int? id) {
            if (id == null)
                throw new ValidationException("Не установлено id отдела", "");
            Department department = await Database.Departments.FindByIdAsync(id.Value);
            if (department == null)
                throw new ValidationException("Отдел не найден", "");
            InitializeMapper();
            DepartmentDTO dDto = Mapper.Map<Department, DepartmentDTO>(department);
            if (dDto.ManagerId != null) {
                Employee manager = await Database.Employees.FindByIdAsync(dDto.ManagerId.Value);
                dDto.Manager = Mapper.Map<Employee, EmployeeDTO>(manager);
            }
            return dDto;
        }

        private void InitializeMapper() {
            Mapper.Initialize(cfg => {
                cfg.CreateMap<Department, DepartmentDTO>();
                cfg.CreateMap<Employee, EmployeeDTO>()
                    .ForMember(e => e.BusinessTrips, opt => opt.Ignore())
                    .ForMember(e => e.Birth, opt => opt.Ignore())
                    .ForMember(e => e.Contacts, opt => opt.Ignore())
                    .ForMember(e => e.Education, opt => opt.Ignore())
                    .ForMember(e => e.Passport, opt => opt.Ignore());
                cfg.CreateMap<Organization, OrganizationDTO>()
                    .ForMember(o => o.Requisites, opt => opt.Ignore())
                    .ForMember(o => o.Bank, opt => opt.Ignore());
                cfg.CreateMap<Post, PostDTO>()
                    .ForMember(p => p.Employees, opt => opt.Ignore());
            });
        }

        // Получение списка всех отделов
        public override async Task<IEnumerable<DepartmentDTO>> GetAllAsync() {
            IEnumerable<Department> departments = await Database.Departments.GetAllAsync();
            InitializeMapper();
            IEnumerable<DepartmentDTO> collectionDTO = Mapper.Map<IEnumerable<Department>, IEnumerable<DepartmentDTO>>(departments);
            // С помощью свойства ManagerId находим начальника отдела и присваиваем его свойству Manager
            foreach (DepartmentDTO dDto in collectionDTO) {
                if (dDto.ManagerId != null) {
                    Employee manager = await Database.Employees.FindByIdAsync(dDto.ManagerId.Value);
                    dDto.Manager = Mapper.Map<Employee, EmployeeDTO>(manager);
                }
            }
            return collectionDTO;
        }

        // Удаление отдела
        public override async Task DeleteAsync(int id) {
            Department department = await Database.Departments.FindByIdAsync(id);
            if (department == null) return;
            if (department.Posts.Count > 0)
                throw new ValidationException("Нельзя удалить отдел, пока в нем есть хотя бы одна должность", "");
            Database.Departments.Remove(department);
            await Database.SaveAsync();
        }

        // Удаление всех отделов
        public override async Task DeleteAllAsync() {
            await Database.Departments.RemoveAllAsync();
            await Database.SaveAsync();
        }

        // Запись информации об отделах в JSON-файл
        public override async Task ExportJsonAsync(string fullPath) {
            IEnumerable<Department> departments = await Database.Departments.GetAllAsync();
            var transformDepartments = departments.Select(d => new {
                d.Code,
                d.DepartmentName,
                d.ManagerId,
                d.OrganizationId
            }).ToArray();
            using (StreamWriter sw = new StreamWriter(fullPath, true, Encoding.UTF8)) {
                sw.WriteLine("{\"Departments\":");
                sw.WriteLine(new JavaScriptSerializer().Serialize(transformDepartments));
                sw.WriteLine("}");
            }
        }

        // Количество отделов
        public override async Task<int> CountAsync() {
            return await Database.Departments.CountAsync();
        }

        // Количество отделов, удовлетворяющих предикату
        public override async Task<int> CountAsync(Expression<Func<DepartmentDTO, bool>> predicateDTO) {
            Mapper.Initialize(cfg => cfg.CreateMap<DepartmentDTO, Department>());
            var predicate = Mapper.Map<Expression<Func<DepartmentDTO, bool>>, Expression<Func<Department, bool>>>(predicateDTO);
            return await Database.Departments.CountAsync(predicate);
        }

        public override void Dispose() {
            Database.Dispose();
        }
    }
}
