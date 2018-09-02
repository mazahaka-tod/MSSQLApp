using AutoMapper;
using DiplomMSSQLApp.BLL.BusinessModels;
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
        private IUnitOfWork Database { get; set; }

        public BusinessTripService(IUnitOfWork uow)
        {
            Database = uow;
        }

        public BusinessTripService() {}

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
            // Добавляем сотрудников, отправленных в командировку
            foreach (var id in ids.Distinct())
            {
                newbt.Employees.Add(await Database.Employees.FindByIdAsync(id));
            }
            Database.BusinessTrips.Create(newbt);
            await Database.SaveAsync();
        }

        // Удаление командировки
        public override async Task DeleteAsync(int id)
        {
            BusinessTrip item = await Database.BusinessTrips.FindByIdAsync(id);
            if (item == null) return;
            Database.BusinessTrips.Remove(item);
            await Database.SaveAsync();
        }

        // Удаление всех командировок
        public override async Task DeleteAllAsync()
        {
            await Database.BusinessTrips.RemoveAllAsync();
            await Database.SaveAsync();
        }

        public override void Dispose()
        {
            Database.Dispose();
        }

        // Обновление информации о командировке
        public async Task EditAsync(BusinessTripDTO item, int[] ids)
        {
            ValidationBusinessTrip(item);
            BusinessTrip newbt = await Database.BusinessTrips.FindByIdAsync(item.Id);
            newbt.Name = item.Name;
            newbt.DateStart = item.DateStart;
            newbt.DateEnd = item.DateEnd;
            newbt.Destination = item.Destination;
            newbt.Purpose = item.Purpose;
            newbt.Employees.Clear();
            // Добавляем сотрудников, отправленных в командировку
            foreach (var id in ids.Distinct())
            {
                newbt.Employees.Add(await Database.Employees.FindByIdAsync(id));
            }
            Database.BusinessTrips.Update(newbt);
            await Database.SaveAsync();
        }

        // Получение командировки по id
        public override async Task<BusinessTripDTO> FindByIdAsync(int? id)
        {
            if (id == null)
                throw new ValidationException("Не установлено id командировки", "");
            BusinessTrip bt = await Database.BusinessTrips.FindByIdAsync(id.Value);
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

        // Получение списка всех командировок
        public override async Task<IEnumerable<BusinessTripDTO>> GetAllAsync()
        {
            Mapper.Initialize(cfg => {
                cfg.CreateMap<BusinessTrip, BusinessTripDTO>();
                cfg.CreateMap<Employee, EmployeeDTO>()
                    .ForMember(p => p.Post, opt => opt.Ignore())
                    .ForMember(d => d.Department, opt => opt.Ignore());
            });
            return Mapper.Map<IEnumerable<BusinessTrip>, List<BusinessTripDTO>>(await Database.BusinessTrips.GetAsync());
        }

        // Валидация модели
        private void ValidationBusinessTrip(BusinessTripDTO item)
        {
            if (item.DateStart > item.DateEnd)
                throw new ValidationException("Дата окончания командировки не должна быть до даты начала", "");
        }

        // Нереализованные методы
        public override Task CreateAsync(BusinessTripDTO item)
        {
            throw new System.NotImplementedException();
        }

        public override Task EditAsync(BusinessTripDTO item)
        {
            throw new System.NotImplementedException();
        }

        public override IEnumerable<BusinessTripDTO> Get(EmployeeFilter f, string path)
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
