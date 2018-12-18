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
    public class AnnualLeaveService : BaseService<AnnualLeaveDTO> {
        public IUnitOfWork Database { get; set; }

        public AnnualLeaveService(IUnitOfWork uow) {
            Database = uow;
        }

        public AnnualLeaveService() { }

        // Добавление нового отпуска (с валидацией)
        public override async Task CreateAsync(AnnualLeaveDTO annualLeaveDto) {
            await ValidationAnnualLeave(annualLeaveDto);
            InitializeMapperDTO();
            AnnualLeave annualLeave = Mapper.Map<AnnualLeaveDTO, AnnualLeave>(annualLeaveDto);
            await UpdateLeaveSchedule(annualLeave);
            Database.AnnualLeaves.Create(annualLeave);
            await Database.SaveAsync();
        }

        private async Task ValidationAnnualLeave(AnnualLeaveDTO item) {
            if (item.ScheduledDate == null)
                throw new ValidationException("Требуется ввести запланированную дату отпуска", "ScheduledDate");
            int leaveScheduleCount = await Database.LeaveSchedules.CountAsync(ls => ls.Year == item.ScheduledDate.Value.Year);
            if (leaveScheduleCount == 0)
                throw new ValidationException("Не найден график отпусков на " + item.ScheduledDate.Value.Year + " год", "ScheduledDate");

            if (item.ScheduledNumberOfDays == null)
                throw new ValidationException("Требуется ввести запланированное количество дней отпуска", "ScheduledNumberOfDays");
            if (item.ScheduledNumberOfDays.Value < 1 || item.ScheduledNumberOfDays.Value > 1000)
                throw new ValidationException("Некорректное значение", "ScheduledNumberOfDays");

            if (item.ActualDate.HasValue) {
                if (item.ActualDate.Value.Year != item.ScheduledDate.Value.Year)
                    throw new ValidationException("Некорректный год", "ActualDate");
                if (item.ActualNumberOfDays == null)
                    throw new ValidationException("Требуется ввести значение", "ActualNumberOfDays");
            }

            if (item.ActualNumberOfDays.HasValue) {
                if (item.ActualNumberOfDays.Value < 1 || item.ActualNumberOfDays.Value > 1000)
                    throw new ValidationException("Некорректное значение", "ActualNumberOfDays");
                if (item.ActualDate == null)
                    throw new ValidationException("Требуется ввести значение", "ActualDate");
            }
        }

        private async Task UpdateLeaveSchedule(AnnualLeave item) {
            // Добавляем информацию о графике отпусков
            LeaveSchedule leaveSchedule = await Database.LeaveSchedules.GetFirstAsync(ls => ls.Year == item.ScheduledDate.Value.Year);
            item.LeaveScheduleId = leaveSchedule.Id;
        }

        private void InitializeMapperDTO() {
            Mapper.Initialize(cfg => {
                cfg.CreateMap<AnnualLeaveDTO, AnnualLeave>();
                cfg.CreateMap<LeaveScheduleDTO, LeaveSchedule>()
                    .ForMember(ls => ls.AnnualLeaves, opt => opt.Ignore());
                cfg.CreateMap<EmployeeDTO, Employee>()
                    .ForMember(e => e.AnnualLeaves, opt => opt.Ignore())
                    .ForMember(e => e.Birth, opt => opt.Ignore())
                    .ForMember(e => e.BusinessTrips, opt => opt.Ignore())
                    .ForMember(e => e.Contacts, opt => opt.Ignore())
                    .ForMember(e => e.Education, opt => opt.Ignore())
                    .ForMember(e => e.Passport, opt => opt.Ignore());
                cfg.CreateMap<PostDTO, Post>()
                    .ForMember(p => p.Employees, opt => opt.Ignore());
                cfg.CreateMap<DepartmentDTO, Department>()
                    .ForMember(d => d.Organization, opt => opt.Ignore())
                    .ForMember(d => d.Posts, opt => opt.Ignore());
            });
        }

        // Обновление информации об отпуске
        public override async Task EditAsync(AnnualLeaveDTO annualLeaveDto) {
            await ValidationAnnualLeave(annualLeaveDto);
            InitializeMapperDTO();
            AnnualLeave annualLeave = Mapper.Map<AnnualLeaveDTO, AnnualLeave>(annualLeaveDto);
            await UpdateLeaveSchedule(annualLeave);
            Database.AnnualLeaves.Update(annualLeave);
            await Database.SaveAsync();
        }

        // Получение отпуска по id
        public override async Task<AnnualLeaveDTO> FindByIdAsync(int? id) {
            if (id == null)
                throw new ValidationException("Не установлен id отпуска", "");
            AnnualLeave annualLeave = await Database.AnnualLeaves.FindByIdAsync(id.Value);
            if (annualLeave == null)
                throw new ValidationException("Отпуск не найден", "");
            InitializeMapper();
            AnnualLeaveDTO annualLeaveDto = Mapper.Map<AnnualLeave, AnnualLeaveDTO>(annualLeave);
            return annualLeaveDto;
        }

        private void InitializeMapper() {
            Mapper.Initialize(cfg => {
                cfg.CreateMap<AnnualLeave, AnnualLeaveDTO>();
                cfg.CreateMap<LeaveSchedule, LeaveScheduleDTO>()
                    .ForMember(ls => ls.AnnualLeaves, opt => opt.Ignore());
                cfg.CreateMap<Employee, EmployeeDTO>()
                    .ForMember(e => e.AnnualLeaves, opt => opt.Ignore())
                    .ForMember(e => e.Birth, opt => opt.Ignore())
                    .ForMember(e => e.BusinessTrips, opt => opt.Ignore())
                    .ForMember(e => e.Contacts, opt => opt.Ignore())
                    .ForMember(e => e.Education, opt => opt.Ignore())
                    .ForMember(e => e.Passport, opt => opt.Ignore());
                cfg.CreateMap<Post, PostDTO>()
                   .ForMember(p => p.Employees, opt => opt.Ignore());
                cfg.CreateMap<Department, DepartmentDTO>()
                   .ForMember(d => d.Manager, opt => opt.Ignore())
                   .ForMember(d => d.Organization, opt => opt.Ignore())
                   .ForMember(d => d.Posts, opt => opt.Ignore());
            });
        }

        // Получение списка всех отпусков
        public override async Task<IEnumerable<AnnualLeaveDTO>> GetAllAsync() {
            IEnumerable<AnnualLeave> annualLeaves = await Database.AnnualLeaves.GetAllAsync();
            InitializeMapper();
            IEnumerable<AnnualLeaveDTO> collection = Mapper.Map<IEnumerable<AnnualLeave>, IEnumerable<AnnualLeaveDTO>>(annualLeaves);
            return collection;
        }

        // Получение списка отпусков по фильтру
        public virtual IEnumerable<AnnualLeaveDTO> Get(AnnualLeaveFilter filter) {
            Func<AnnualLeave, bool> predicate = CreatePredicate(filter);
            IEnumerable<AnnualLeave> filteredSortedCollection = FilterAndSortAnnualLeaves(filter, predicate);
            InitializeMapper();
            IEnumerable<AnnualLeaveDTO> collection = Mapper.Map<IEnumerable<AnnualLeave>, IEnumerable<AnnualLeaveDTO>>(filteredSortedCollection);
            return collection;
        }

        private Func<AnnualLeave, bool> CreatePredicate(AnnualLeaveFilter filter) {
            bool predicate(AnnualLeave annualLeave) {
                bool returnValue = false;
                if (filter.IsAntiFilter)            // Если флаг установлен, то выбираются записи несоответствующие фильтру
                    returnValue = true;

                // Фильтр по ФИО сотрудника
                if (filter.Name != null) {
                    string[] nameArray = filter.Name.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();   // Удаляем пустые элементы из массива
                    if (nameArray.Length > 0) {
                        bool flag = true;
                        foreach (string name in nameArray) {
                            string fio = annualLeave.Employee.LastName + " " + annualLeave.Employee.FirstName +
                                (!string.IsNullOrWhiteSpace(annualLeave.Employee.Patronymic) ? " " + annualLeave.Employee.Patronymic : "");
                            if (Regex.IsMatch(fio, name, RegexOptions.IgnoreCase)) {
                                flag = false;
                                break;
                            }
                        }
                        if (flag)
                            return returnValue;     // Проверяемая запись не соответствует фильтру по ФИО сотрудника
                    }
                }

                // Фильтр по названию должности
                if (filter.PostTitle != null) {
                    string[] postTitleArray = filter.PostTitle.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();   // Удаляем пустые элементы из массива
                    if (postTitleArray.Length > 0) {
                        bool flag = true;
                        foreach (string postTitle in postTitleArray) {
                            if (Regex.IsMatch(annualLeave.Employee.Post.Title, postTitle, RegexOptions.IgnoreCase)) {
                                flag = false;
                                break;
                            }
                        }
                        if (flag)
                            return returnValue;     // Проверяемая запись не соответствует фильтру по названию должности
                    }
                }

                // Фильтр по названию отдела
                if (filter.DepartmentName != null) {
                    string[] departmentNameArray = filter.DepartmentName.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();   // Удаляем пустые элементы из массива
                    if (departmentNameArray.Length > 0) {
                        bool flag = true;
                        foreach (string name in departmentNameArray) {
                            if (Regex.IsMatch(annualLeave.Employee.Post.Department.DepartmentName, name, RegexOptions.IgnoreCase)) {
                                flag = false;
                                break;
                            }
                        }
                        if (flag)
                            return returnValue;     // Проверяемая запись не соответствует фильтру по названию отдела
                    }
                }

                // Фильтр по запланированной дате отпуска
                if (!string.IsNullOrWhiteSpace(filter.ScheduledDate) && DateTime.TryParse(filter.ScheduledDate, out DateTime scheduledDate) && annualLeave.ScheduledDate != scheduledDate)
                    return returnValue;             // Проверяемая запись не соответствует фильтру по запланированной дате отпуска

                // Фильтр по фактической дате отпуска
                if (!string.IsNullOrWhiteSpace(filter.ActualDate) && DateTime.TryParse(filter.ActualDate, out DateTime actualDate) && annualLeave.ActualDate != actualDate)
                    return returnValue;             // Проверяемая запись не соответствует фильтру по фактической дате отпуска

                // Фильтры по общему количеству дней отпуска за год
                if (filter.MinNumberOfDaysOfLeave.HasValue && annualLeave.Employee.Post.NumberOfDaysOfLeave < filter.MinNumberOfDaysOfLeave.Value)
                    return returnValue;             // Проверяемая запись не соответствует фильтру по общему количеству дней отпуска за год
                if (filter.MaxNumberOfDaysOfLeave.HasValue && annualLeave.Employee.Post.NumberOfDaysOfLeave > filter.MaxNumberOfDaysOfLeave.Value)
                    return returnValue;             // Проверяемая запись не соответствует фильтру по общему количеству дней отпуска за год

                // Фильтры по запланированному количеству дней отпуска
                if (filter.MinScheduledNumberOfDays.HasValue && annualLeave.ScheduledNumberOfDays.HasValue && annualLeave.ScheduledNumberOfDays < filter.MinScheduledNumberOfDays.Value)
                    return returnValue;             // Проверяемая запись не соответствует фильтру по запланированному количеству дней отпуска
                if (filter.MaxScheduledNumberOfDays.HasValue && annualLeave.ScheduledNumberOfDays.HasValue && annualLeave.ScheduledNumberOfDays > filter.MaxScheduledNumberOfDays.Value)
                    return returnValue;             // Проверяемая запись не соответствует фильтру по запланированному количеству дней отпуска

                // Фильтры по фактическому количеству дней отпуска
                if (filter.MinActualNumberOfDays.HasValue && annualLeave.ActualNumberOfDays.HasValue && annualLeave.ActualNumberOfDays < filter.MinActualNumberOfDays.Value)
                    return returnValue;             // Проверяемая запись не соответствует фильтру по фактическому количеству дней отпуска
                if (filter.MaxActualNumberOfDays.HasValue && annualLeave.ActualNumberOfDays.HasValue && annualLeave.ActualNumberOfDays > filter.MaxActualNumberOfDays.Value)
                    return returnValue;             // Проверяемая запись не соответствует фильтру по фактическому количеству дней отпуска

                return !returnValue;                // Если дошли до сюда, значит проверяемая запись соответствует фильтру
            }
            return predicate;
        }

        private IEnumerable<AnnualLeave> FilterAndSortAnnualLeaves(AnnualLeaveFilter filter, Func<AnnualLeave, bool> predicate) {
            IEnumerable<AnnualLeave> filteredSortedCollection;
            // Параметры сортировки
            string sortField = filter.SortField ?? "Default";
            string order = filter.SortOrder ?? "Asc";
            // Компараторы сортировки по возрастанию или по убыванию
            IComparer<string> stringComparer = Comparer<string>.Create((x, y) => order.Equals("Asc") ? (x ?? "").CompareTo(y ?? "") : (y ?? "").CompareTo(x ?? ""));
            IComparer<DateTime> dateTimeComparer = Comparer<DateTime>.Create((x, y) => order.Equals("Asc") ? x.CompareTo(y) : y.CompareTo(x));
            IComparer<int> intComparer = Comparer<int>.Create((x, y) => order.Equals("Asc") ? x.CompareTo(y) : y.CompareTo(x));
            switch (sortField) {
                case "Name":
                    filteredSortedCollection = Database.AnnualLeaves.Get(predicate).OrderBy(al => al.Employee.LastName + " " + al.Employee.FirstName +
                       (!string.IsNullOrWhiteSpace(al.Employee.Patronymic) ? " " + al.Employee.Patronymic : ""), stringComparer); break;
                case "PostTitle": filteredSortedCollection = Database.AnnualLeaves.Get(predicate).OrderBy(al => al.Employee.Post.Title, stringComparer); break;
                case "DepartmentName": filteredSortedCollection = Database.AnnualLeaves.Get(predicate).OrderBy(al => al.Employee.Post.Department.DepartmentName, stringComparer); break;
                case "ScheduledDate": filteredSortedCollection = Database.AnnualLeaves.Get(predicate).OrderBy(al => al.ScheduledDate.Value, dateTimeComparer); break;
                case "ActualDate": filteredSortedCollection = Database.AnnualLeaves.Get(predicate).OrderBy(al => al.ActualDate.Value, dateTimeComparer); break;
                case "NumberOfDaysOfLeave": filteredSortedCollection = Database.AnnualLeaves.Get(predicate).OrderBy(al => al.Employee.Post.NumberOfDaysOfLeave, intComparer); break;
                case "ScheduledNumberOfDays": filteredSortedCollection = Database.AnnualLeaves.Get(predicate).OrderBy(al => al.ScheduledNumberOfDays.Value, intComparer); break;
                case "ActualNumberOfDays": filteredSortedCollection = Database.AnnualLeaves.Get(predicate).OrderBy(al => al.ActualNumberOfDays.Value, intComparer); break;
                default:
                    filteredSortedCollection = Database.AnnualLeaves.Get(predicate).OrderBy(al => al.Employee.LastName + " " + al.Employee.FirstName +
                           (!string.IsNullOrWhiteSpace(al.Employee.Patronymic) ? " " + al.Employee.Patronymic : "")); break;
            }
            return filteredSortedCollection;
        }

        // Удаление отпуска
        public override async Task DeleteAsync(int id) {
            AnnualLeave annualLeave = await Database.AnnualLeaves.FindByIdAsync(id);
            if (annualLeave == null) return;
            Database.AnnualLeaves.Remove(annualLeave);
            await Database.SaveAsync();
        }

        // Удаление всех отпусков
        public override async Task DeleteAllAsync() {
            await Database.AnnualLeaves.RemoveAllAsync();
            await Database.SaveAsync();
        }

        // Запись информации об отпусках в JSON-файл
        public override async Task ExportJsonAsync(string fullPath) {
            IEnumerable<AnnualLeave> annualLeaves = await Database.AnnualLeaves.GetAllAsync();
            var transformAnnualLeaves = annualLeaves.Select(al => new {
                Name = al.Employee.LastName + " " + al.Employee.FirstName + (!string.IsNullOrWhiteSpace(al.Employee.Patronymic) ? " " + al.Employee.Patronymic : ""),
                PostTitle = al.Employee.Post.Title,
                DepartmentName = al.Employee.Post.Department.DepartmentName,
                NumberOfDaysOfLeave = al.Employee.Post.NumberOfDaysOfLeave,
                al.ScheduledDate,
                al.ActualDate,
                al.ScheduledNumberOfDays,
                al.ActualNumberOfDays
            }).ToArray();
            using (StreamWriter sw = new StreamWriter(fullPath, true, Encoding.UTF8)) {
                sw.WriteLine("{\"AnnualLeaves\":");
                sw.WriteLine(new JavaScriptSerializer().Serialize(transformAnnualLeaves));
                sw.WriteLine("}");
            }
        }

        // Количество отпусков
        public override async Task<int> CountAsync() {
            return await Database.AnnualLeaves.CountAsync();
        }

        // Количество отпусков, удовлетворяющих предикату
        public override async Task<int> CountAsync(Expression<Func<AnnualLeaveDTO, bool>> predicateDTO) {
            InitializeMapperDTO();
            var predicate = Mapper.Map<Expression<Func<AnnualLeaveDTO, bool>>, Expression<Func<AnnualLeave, bool>>>(predicateDTO);
            return await Database.AnnualLeaves.CountAsync(predicate);
        }

        // Добавление отпусков для тестирования
        public async Task TestCreateAsync() {
            // Если график отпусков на 2018 год не найден, то добавляем его
            int leaveScheduleCount = await Database.LeaveSchedules.CountAsync(ls => ls.Year == 2018);
            if (leaveScheduleCount == 0) {
                Database.LeaveSchedules.Create(new LeaveSchedule { Number = 1, Year = 2018, DateOfPreparation = new DateTime(2017, 12, 1) });
                await Database.SaveAsync();
            }
            // Получаем Id сотрудников
            int[] employeeIds = (await Database.Employees.GetAllAsync()).Select(e => e.Id).ToArray();
            if (employeeIds.Length == 0)
                throw new ValidationException("Сначала нужно добавить сотрудников", "");
            // Добавляем отпуска
            for (int i = 0; i < 100; i++) {
                AnnualLeave annualLeave = new AnnualLeave {
                    ActualNumberOfDays = new Random().Next(10, 20) + i % 5,
                    ScheduledNumberOfDays = new Random().Next(10, 20) + i % 5,
                    ScheduledDate = new DateTime(2018, (new Random().Next(1000, 2000) + i) % 12 + 1, (new Random().Next(2000, 3000) + i) % 28 + 1),
                    ActualDate = new DateTime(2018, (new Random().Next(3000, 4000) + i) % 12 + 1, (new Random().Next(5000, 6000) + i) % 28 + 1),
                    EmployeeId = employeeIds[i % employeeIds.Length],
                    LeaveScheduleId = (await Database.LeaveSchedules.GetFirstAsync(ls => ls.Year == 2018)).Id
                };
                Database.AnnualLeaves.Create(annualLeave);
            }
            await Database.SaveAsync();
        }

        public override void Dispose() {
            Database.Dispose();
        }
    }
}
