using AutoMapper;
using DiplomMSSQLApp.BLL.DTO;
using DiplomMSSQLApp.BLL.Infrastructure;
using DiplomMSSQLApp.DAL.Entities;
using DiplomMSSQLApp.DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace DiplomMSSQLApp.BLL.Services {
    public class BusinessTripService : BaseService<BusinessTripDTO> {
        public IUnitOfWork Database { get; set; }

        public BusinessTripService(IUnitOfWork uow) {
            Database = uow;
        }

        public BusinessTripService() { }

        // Добавление новой командировки (с валидацией)
        public override async Task CreateAsync(BusinessTripDTO btDto) {
            ValidationBusinessTrip(btDto);
            InitializeMapperDTO();
            BusinessTrip bt = Mapper.Map<BusinessTripDTO, BusinessTrip>(btDto);
            await AddEmployeesOnBusinessTripAsync(bt);
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
                    .ForMember(e => e.Birth, opt => opt.Ignore())
                    .ForMember(e => e.BusinessTrips, opt => opt.Ignore())
                    .ForMember(e => e.Contacts, opt => opt.Ignore())
                    .ForMember(e => e.Education, opt => opt.Ignore())
                    .ForMember(e => e.Passport, opt => opt.Ignore())
                    .ForMember(e => e.Post, opt => opt.Ignore());
            });
        }

        private async Task AddEmployeesOnBusinessTripAsync(BusinessTrip bt) {
            IEnumerable<int> ids = bt.Employees.Select(e => e.Id).Distinct().ToList();
            bt.Employees.Clear();
            foreach (int id in ids) {
                Employee employee = await Database.Employees.FindByIdAsync(id);
                if (employee != null)
                    bt.Employees.Add(employee);
            }
        }

        // Обновление информации о командировке
        public override async Task EditAsync(BusinessTripDTO btDto) {
            ValidationBusinessTrip(btDto);
            BusinessTrip bt = await Database.BusinessTrips.FindByIdAsync(btDto.Id);
            if (bt == null) return;
            InitializeMapperDTO();
            Mapper.Map(btDto, bt);
            await AddEmployeesOnBusinessTripAsync(bt);
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
                    .ForMember(e => e.Birth, opt => opt.Ignore())
                    .ForMember(e => e.BusinessTrips, opt => opt.Ignore())
                    .ForMember(e => e.Contacts, opt => opt.Ignore())
                    .ForMember(e => e.Education, opt => opt.Ignore())
                    .ForMember(e => e.Passport, opt => opt.Ignore())
                    .ForMember(e => e.Post, opt => opt.Ignore());
            });
        }

        // Получение списка всех командировок
        public override async Task<IEnumerable<BusinessTripDTO>> GetAllAsync() {
            IEnumerable<BusinessTrip> businessTrips = await Database.BusinessTrips.GetAllAsync();
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

        // Количество командировок
        public override async Task<int> CountAsync() {
            return await Database.BusinessTrips.CountAsync();
        }

        // Количество командировок, удовлетворяющих предикату
        public override async Task<int> CountAsync(Expression<Func<BusinessTripDTO, bool>> predicateDTO) {
            Mapper.Initialize(cfg => cfg.CreateMap<BusinessTripDTO, BusinessTrip>());
            var predicate = Mapper.Map<Expression<Func<BusinessTripDTO, bool>>, Expression<Func<BusinessTrip, bool>>>(predicateDTO);
            return await Database.BusinessTrips.CountAsync(predicate);
        }

        public override void Dispose() {
            Database.Dispose();
        }
    }
}
