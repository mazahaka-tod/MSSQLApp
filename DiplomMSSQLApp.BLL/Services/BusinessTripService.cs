using AutoMapper;
using DiplomMSSQLApp.BLL.DTO;
using DiplomMSSQLApp.BLL.Infrastructure;
using DiplomMSSQLApp.DAL.Entities;
using DiplomMSSQLApp.DAL.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiplomMSSQLApp.BLL.Services
{
    public class BusinessTripService : BaseService<BusinessTripDTO>
    {
        IUnitOfWork Database { get; set; }

        public BusinessTripService(IUnitOfWork uow)
        {
            Database = uow;
        }

        // Добавление новой командировки (с валидацией)
        public async Task CreateAsync(BusinessTripDTO item, int[] ids)
        {
            ValidationBusinessTrip(item);
            BusinessTrip newbt = new BusinessTrip
            {
                Name = item.Name,
                DateStart = item.DateStart,
                DateEnd = item.DateEnd,
                Destination = item.Destination,
                Purpose = item.Purpose
            };
            foreach (var id in ids.Distinct())
            {
                newbt.Employees.Add(Database.Employees.FindById(id));
            }
            Database.BusinessTrips.Create(newbt);
            await Database.SaveAsync();
        }

        // Получение списка всех командировок
        public override IEnumerable<BusinessTripDTO> GetAll()
        {
            Mapper.Initialize(cfg => {
                cfg.CreateMap<BusinessTrip, BusinessTripDTO>();
                cfg.CreateMap<Employee, EmployeeDTO>()
                    .ForMember(p => p.Post, opt => opt.Ignore())
                    .ForMember(d => d.Department, opt => opt.Ignore());
            });
            return Mapper.Map<IEnumerable<BusinessTrip>, List<BusinessTripDTO>>(Database.BusinessTrips.Get());
        }

        // Удаление командировки
        public override async Task DeleteAsync(int id)
        {
            BusinessTrip item = Database.BusinessTrips.FindById(id);
            if (item == null) return;
            Database.BusinessTrips.Remove(item);
            await Database.SaveAsync();
        }

        // Удаление всех командировок
        public override async Task DeleteAllAsync()
        {
            Database.BusinessTrips.RemoveAll();
            await Database.SaveAsync();
        }

        // Обновление информации о командировке
        public async Task EditAsync(BusinessTripDTO item, int[] ids)
        {
            ValidationBusinessTrip(item);
            BusinessTrip newbt = Database.BusinessTrips.FindById(item.Id);
            newbt.Name = item.Name;
            newbt.DateStart = item.DateStart;
            newbt.DateEnd = item.DateEnd;
            newbt.Destination = item.Destination;
            newbt.Purpose = item.Purpose;
            newbt.Employees.Clear();
            foreach (var id in ids.Distinct())
            {
                newbt.Employees.Add(Database.Employees.FindById(id));
            }
            Database.BusinessTrips.Update(newbt);
            await Database.SaveAsync();
        }

        public override void Dispose()
        {
            Database.Dispose();
        }

        // Получение командировки по id
        public override BusinessTripDTO FindById(int? id)
        {
            if (id == null)
                throw new ValidationException("Не установлено id командировки", "");
            BusinessTrip bt = Database.BusinessTrips.FindById(id.Value);
            if (bt == null)
                throw new ValidationException("Командировка не найдена", "");
            Mapper.Initialize(cfg => {
                cfg.CreateMap<BusinessTrip, BusinessTripDTO>();
                cfg.CreateMap<Employee, EmployeeDTO>();
                cfg.CreateMap<Post, PostDTO>();
                cfg.CreateMap<Department, DepartmentDTO>();
            });
            return Mapper.Map<BusinessTrip, BusinessTripDTO>(bt);
        }

        // Валидация модели
        private void ValidationBusinessTrip(BusinessTripDTO item)
        {
            if (item.Name == null)
                throw new ValidationException("Требуется ввести код командировки", "Name");
            if (item.DateStart == null)
                throw new ValidationException("Требуется ввести дату начала командировки", "DateStart");
            if (item.DateEnd == null)
                throw new ValidationException("Требуется ввести дату окончания командировки", "DateEnd");
            if (item.DateStart > item.DateEnd)
                throw new ValidationException("Дата окончания командировки не должна быть до даты начала", "DateEnd");
            if (item.Destination == null)
                throw new ValidationException("Требуется ввести место назначения", "Destination");
        }

        public override Task CreateAsync(BusinessTripDTO item)
        {
            throw new System.NotImplementedException();
        }

        public override Task EditAsync(BusinessTripDTO item)
        {
            throw new System.NotImplementedException();
        }

        public override Task TestCreateAsync(int num, string path)
        {
            throw new System.NotImplementedException();
        }

        public override void TestRead(int num, string path, int val)
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
