using AutoMapper;
using DiplomMSSQLApp.BLL.BusinessModels;
using DiplomMSSQLApp.BLL.DTO;
using DiplomMSSQLApp.BLL.Infrastructure;
using DiplomMSSQLApp.DAL.Entities;
using DiplomMSSQLApp.DAL.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiplomMSSQLApp.BLL.Services {
    public class BusinessTripService : BaseService<BusinessTripDTO> {
        public IUnitOfWork Database { get; set; }

        public BusinessTripService(IUnitOfWork uow) {
            Database = uow;
        }

        public BusinessTripService() { }

        // Добавление новой командировки (с валидацией)
        public virtual async Task CreateAsync(BusinessTripDTO btDto, int[] ids) {
            ValidationBusinessTrip(btDto);
            InitializeMapperDTO();
            BusinessTrip bt = Mapper.Map<BusinessTripDTO, BusinessTrip>(btDto);
            await AddEmployeesOnBusinessTripAsync(bt, ids);
            Database.BusinessTrips.Create(bt);
            await Database.SaveAsync();
        }

        private void ValidationBusinessTrip(BusinessTripDTO item) {
            if (item.Name == null)
                throw new ValidationException("Требуется ввести код командировки", "Name");
            if (item.DateStart > item.DateEnd)
                throw new ValidationException("Дата окончания командировки не должна быть до даты начала", "DateEnd");
            if (item.Destination == null)
                throw new ValidationException("Требуется ввести место назначения", "Destination");
            if (item.Purpose == null)
                throw new ValidationException("Требуется ввести цель командировки", "Purpose");
        }

        private void InitializeMapperDTO() {
            Mapper.Initialize(cfg => {
                cfg.CreateMap<BusinessTripDTO, BusinessTrip>();
                cfg.CreateMap<EmployeeDTO, Employee>()
                   .ForMember(e => e.BusinessTrips, opt => opt.Ignore())
                   .ForMember(e => e.Department, opt => opt.Ignore())
                   .ForMember(e => e.Post, opt => opt.Ignore());
            });
        }

        private async Task AddEmployeesOnBusinessTripAsync(BusinessTrip bt, int[] ids) {
            foreach (int id in ids.Distinct()) {
                Employee employee = await Database.Employees.FindByIdAsync(id);
                if (employee != null)
                    bt.Employees.Add(employee);
            }
        }

        // Обновление информации о командировке
        public virtual async Task EditAsync(BusinessTripDTO btDto, int[] ids) {
            ValidationBusinessTrip(btDto);
            BusinessTrip bt = await Database.BusinessTrips.FindByIdAsync(btDto.Id);
            if (bt == null) return;
            InitializeMapperDTO();
            Mapper.Map(btDto, bt);
            bt.Employees.Clear();
            await AddEmployeesOnBusinessTripAsync(bt, ids);
            Database.BusinessTrips.Update(bt);
            await Database.SaveAsync();
        }

        // Получение командировки по id
        public override async Task<BusinessTripDTO> FindByIdAsync(int? id) {
            if (id == null)
                throw new ValidationException("Не установлено id командировки", "");
            BusinessTrip bt = await Database.BusinessTrips.FindByIdAsync(id.Value);
            if (bt == null)
                throw new ValidationException("Командировка не найдена", "");
            InitializeMapper();
            BusinessTripDTO btDto = Mapper.Map<BusinessTrip, BusinessTripDTO>(bt);
            return btDto;
        }

        private void InitializeMapper() {
            Mapper.Initialize(cfg => {
                cfg.CreateMap<BusinessTrip, BusinessTripDTO>();
                cfg.CreateMap<Employee, EmployeeDTO>()
                    .ForMember(e => e.BusinessTrips, opt => opt.Ignore())
                    .ForMember(e => e.Department, opt => opt.Ignore())
                    .ForMember(e => e.Post, opt => opt.Ignore());
            });
        }

        // Получение списка всех командировок
        public override async Task<IEnumerable<BusinessTripDTO>> GetAllAsync() {
            IEnumerable<BusinessTrip> businessTrips = await Database.BusinessTrips.GetAsync();
            InitializeMapper();
            IEnumerable<BusinessTripDTO> collection = Mapper.Map<IEnumerable<BusinessTrip>, IEnumerable<BusinessTripDTO>>(businessTrips);
            return collection;
        }

        // Удаление командировки
        public override async Task DeleteAsync(int id) {
            BusinessTrip bt = await Database.BusinessTrips.FindByIdAsync(id);
            if (bt == null) return;
            Database.BusinessTrips.Remove(bt);
            await Database.SaveAsync();
        }

        // Удаление всех командировок
        public override async Task DeleteAllAsync() {
            await Database.BusinessTrips.RemoveAllAsync();
            await Database.SaveAsync();
        }

        public override void Dispose() {
            Database.Dispose();
        }

        // Нереализованные методы
        public override Task CreateAsync(BusinessTripDTO item) {
            throw new System.NotImplementedException();
        }

        public override Task EditAsync(BusinessTripDTO item) {
            throw new System.NotImplementedException();
        }

        public override IEnumerable<BusinessTripDTO> Get(EmployeeFilter f) {
            throw new System.NotImplementedException();
        }

        public override Task TestCreateAsync(int num) {
            throw new System.NotImplementedException();
        }

        public override Task TestReadAsync(int num, int salary) {
            throw new System.NotImplementedException();
        }

        public override Task TestUpdateAsync(int num) {
            throw new System.NotImplementedException();
        }

        public override Task TestDeleteAsync(int num) {
            throw new System.NotImplementedException();
        }

        public override Task<BusinessTripDTO> GetFirstAsync() {
            throw new System.NotImplementedException();
        }
    }
}
