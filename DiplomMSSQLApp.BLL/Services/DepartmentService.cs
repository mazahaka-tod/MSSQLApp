using AutoMapper;
using DiplomMSSQLApp.BLL.BusinessModels;
using DiplomMSSQLApp.BLL.DTO;
using DiplomMSSQLApp.BLL.Infrastructure;
using DiplomMSSQLApp.DAL.Entities;
using DiplomMSSQLApp.DAL.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiplomMSSQLApp.BLL.Services {
    public class DepartmentService : BaseService<DepartmentDTO> {
        public IUnitOfWork Database { get; set; }

        public DepartmentService(IUnitOfWork uow) {
            Database = uow;
        }

        public DepartmentService() { }

        // Добавление нового отдела
        public override async Task CreateAsync(DepartmentDTO dDto) {
            Department department = MapDTOAndDomainModelWithValidation(dDto);
            Database.Departments.Create(department);
            await Database.SaveAsync();
        }

        private Department MapDTOAndDomainModelWithValidation(DepartmentDTO dDto) {
            ValidationDepartment(dDto);
            Mapper.Initialize(cfg => cfg.CreateMap<DepartmentDTO, Department>()
                                        .ForMember(d => d.Employees, opt => opt.Ignore()));
            Department department = Mapper.Map<DepartmentDTO, Department>(dDto);
            return department;
        }

        private void ValidationDepartment(DepartmentDTO item) {
            if (item.DepartmentName == null)
                throw new ValidationException("Требуется ввести название отдела", "DepartmentName");
        }

        // Обновление информации об отделе
        public override async Task EditAsync(DepartmentDTO dDto) {
            Department department = MapDTOAndDomainModelWithValidation(dDto);
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
            return dDto;
        }

        private void InitializeMapper() {
            Mapper.Initialize(cfg => {
                cfg.CreateMap<Department, DepartmentDTO>();
                cfg.CreateMap<Employee, EmployeeDTO>()
                    .ForMember(e => e.BusinessTrips, opt => opt.Ignore())
                    .ForMember(e => e.Department, opt => opt.Ignore())
                    .ForMember(e => e.Post, opt => opt.Ignore());
            });
        }

        // Получение списка всех отделов
        public override async Task<IEnumerable<DepartmentDTO>> GetAllAsync() {
            IEnumerable<Department> departments = await Database.Departments.GetAsync();
            InitializeMapper();
            IEnumerable<DepartmentDTO> collection = Mapper.Map<IEnumerable<Department>, IEnumerable<DepartmentDTO>>(departments);
            return collection;
        }

        // Удаление отдела
        public override async Task DeleteAsync(int id) {
            Department item = await Database.Departments.FindByIdAsync(id);
            if (item == null) return;
            Database.Departments.Remove(item);
            await Database.SaveAsync();
        }

        // Удаление всех отделов
        public override async Task DeleteAllAsync() {
            await Database.Departments.RemoveAllAsync();
            await Database.SaveAsync();
        }

        public override void Dispose() {
            Database.Dispose();
        }

        // Нереализованные методы
        public override IEnumerable<DepartmentDTO> Get(EmployeeFilter f) {
            throw new System.NotImplementedException();
        }

        public override Task TestCreateAsync(int num, string path) {
            throw new System.NotImplementedException();
        }

        public override Task TestReadAsync(int num, string path, int val) {
            throw new System.NotImplementedException();
        }

        public override Task TestUpdateAsync(int num, string path) {
            throw new System.NotImplementedException();
        }

        public override Task TestDeleteAsync(int num, string path) {
            throw new System.NotImplementedException();
        }
    }
}
