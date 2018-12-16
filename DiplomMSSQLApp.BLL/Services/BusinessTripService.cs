using AutoMapper;
using DiplomMSSQLApp.BLL.BusinessModels;
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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

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
                    .ForMember(e => e.AnnualLeaves, opt => opt.Ignore())
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
                    .ForMember(e => e.AnnualLeaves, opt => opt.Ignore())
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

        // Получение списка командировок по фильтру
        public virtual IEnumerable<BusinessTripDTO> Get(BusinessTripFilter filter) {
            Func<BusinessTrip, bool> predicate = CreatePredicate(filter);
            IEnumerable<BusinessTrip> filteredSortedCollection = FilterAndSortBusinessTrips(filter, predicate);
            InitializeMapper();
            IEnumerable<BusinessTripDTO> collection = Mapper.Map<IEnumerable<BusinessTrip>, IEnumerable<BusinessTripDTO>>(filteredSortedCollection);
            return collection;
        }

        private Func<BusinessTrip, bool> CreatePredicate(BusinessTripFilter filter) {
            bool predicate(BusinessTrip businessTrip) {
                bool returnValue = false;
                if (filter.IsAntiFilter)            // Если флаг установлен, то выбираются записи несоответствующие фильтру
                    returnValue = true;

                // Фильтр по коду командировки
                if (filter.Code != null) {
                    string[] сodeArray = filter.Code.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();   // Удаляем пустые элементы из массива
                    if (сodeArray.Length > 0) {
                        bool flag = true;
                        foreach (string code in сodeArray) {
                            if (Regex.IsMatch(businessTrip.Name, code, RegexOptions.IgnoreCase)) {
                                flag = false;
                                break;
                            }
                        }
                        if (flag)
                            return returnValue;     // Проверяемая запись не соответствует фильтру по коду командировки
                    }
                }

                // Фильтр по дате начала командировки
                if (!string.IsNullOrWhiteSpace(filter.DateStart) && DateTime.TryParse(filter.DateStart, out DateTime date_start) && businessTrip.DateStart != date_start)
                    return returnValue;             // Проверяемая запись не соответствует фильтру по дате начала командировки

                // Фильтр по дате окончания командировки
                if (!string.IsNullOrWhiteSpace(filter.DateEnd) && DateTime.TryParse(filter.DateEnd, out DateTime date_end) && businessTrip.DateEnd != date_end)
                    return returnValue;             // Проверяемая запись не соответствует фильтру по дате окончания командировки

                // Фильтр по месту назначения
                if (filter.Destination != null) {
                    string[] destinationArray = filter.Destination.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();   // Удаляем пустые элементы из массива
                    if (destinationArray.Length > 0) {
                        bool flag = true;
                        foreach (string destination in destinationArray) {
                            if (Regex.IsMatch(businessTrip.Destination, destination, RegexOptions.IgnoreCase)) {
                                flag = false;
                                break;
                            }
                        }
                        if (flag)
                            return returnValue;     // Проверяемая запись не соответствует фильтру по месту назначения
                    }
                }
                return !returnValue;                // Если дошли до сюда, значит проверяемая запись соответствует фильтру
            }
            return predicate;
        }

        private IEnumerable<BusinessTrip> FilterAndSortBusinessTrips(BusinessTripFilter filter, Func<BusinessTrip, bool> predicate) {
            IEnumerable<BusinessTrip> filteredSortedCollection;
            // Параметры сортировки
            string sortField = filter.SortField ?? "Default";
            string order = filter.SortOrder ?? "Asc";
            // Компараторы сортировки по возрастанию или по убыванию
            IComparer<string> stringComparer = Comparer<string>.Create((x, y) => order.Equals("Asc") ? (x ?? "").CompareTo(y ?? "") : (y ?? "").CompareTo(x ?? ""));
            IComparer<DateTime> dateTimeComparer = Comparer<DateTime>.Create((x, y) => order.Equals("Asc") ? x.CompareTo(y) : y.CompareTo(x));
            switch (sortField) {
                case "Code": filteredSortedCollection = Database.BusinessTrips.Get(predicate).OrderBy(bt => bt.Name, stringComparer); break;
                case "DateStart": filteredSortedCollection = Database.BusinessTrips.Get(predicate).OrderBy(bt => bt.DateStart, dateTimeComparer); break;
                case "DateEnd": filteredSortedCollection = Database.BusinessTrips.Get(predicate).OrderBy(bt => bt.DateEnd, dateTimeComparer); break;
                case "Destination": filteredSortedCollection = Database.BusinessTrips.Get(predicate).OrderBy(bt => bt.Destination, stringComparer); break;
                default: filteredSortedCollection = Database.BusinessTrips.Get(predicate).OrderBy(bt => bt.Name); break;
            }
            return filteredSortedCollection;
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

        // Запись информации о командировках в JSON-файл
        public override async Task ExportJsonAsync(string fullPath) {
            IEnumerable<BusinessTrip> businessTrips = await Database.BusinessTrips.GetAllAsync();
            var transformBusinessTrips = businessTrips.Select(bt => new {
                bt.DateEnd,
                bt.DateStart,
                bt.Destination,
                bt.Name,
                bt.Purpose
            }).ToArray();
            using (StreamWriter sw = new StreamWriter(fullPath, true, Encoding.UTF8)) {
                sw.WriteLine("{\"BusinessTrips\":");
                sw.WriteLine(new JavaScriptSerializer().Serialize(transformBusinessTrips));
                sw.WriteLine("}");
            }
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

        // Добавление командировок для тестирования
        public async Task TestCreateAsync() {
            int[] employeeIds = (await Database.Employees.GetAllAsync()).Select(e => e.Id).ToArray();
            if (employeeIds.Length == 0)
                throw new ValidationException("Сначала нужно добавить сотрудников", "");
            string[] destinations = { "Москва", "Санкт-Петербург", "Екатеринбург", "Казань", "Омск", "Тюмень", "Краснодар" };
            for (int i = 0; i < 100; i++) {
                BusinessTrip businessTrip = new BusinessTrip {
                    Name = (new Random().Next(100000, 999999) + i).ToString(),
                    DateStart = new DateTime(2017, (new Random().Next(1000, 2000) + i) % 12 + 1, (new Random().Next(2000, 3000) + i) % 28 + 1),
                    DateEnd = new DateTime(2018, (new Random().Next(3000, 4000) + i) % 12 + 1, (new Random().Next(5000, 6000) + i) % 28 + 1),
                    Destination = destinations[(new Random().Next(7000, 8000) + i) % destinations.Length],
                    Employees = new Employee[] { await Database.Employees.FindByIdAsync(employeeIds[i % employeeIds.Length]) }
                };
                Database.BusinessTrips.Create(businessTrip);
            }
            await Database.SaveAsync();
        }

        public override void Dispose() {
            Database.Dispose();
        }
    }
}
