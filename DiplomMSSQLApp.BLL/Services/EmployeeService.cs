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
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace DiplomMSSQLApp.BLL.Services {
    public class EmployeeService : BaseService<EmployeeDTO> {
        public IUnitOfWork Database { get; set; }

        public EmployeeService(IUnitOfWork uow) {
            Database = uow;
        }

        public EmployeeService() { }

        // Добавление нового сотрудника
        public override async Task CreateAsync(EmployeeDTO eDto) {
            Employee employee = await MapDTOAndDomainModelWithValidationAsync(eDto);
            Database.Employees.Create(employee);
            await Database.SaveAsync();
        }

        private async Task<Employee> MapDTOAndDomainModelWithValidationAsync(EmployeeDTO eDto) {
            eDto = SetAgeProperty(eDto);
            await ValidationEmployeeAsync(eDto);
            Employee employee = MapDTOAndDomainModel(eDto);
            return employee;
        }

        private EmployeeDTO SetAgeProperty(EmployeeDTO eDto) {
            // Рассчитываем свойство Age по дате рождения
            DateTime birthdate = eDto.Birth.BirthDate;
            DateTime today = DateTime.Today;
            int age = today.Year - birthdate.Year;
            if (birthdate > today.AddYears(-age)) age--;
            eDto.Age = age;
            return eDto;
        }

        private async Task ValidationEmployeeAsync(EmployeeDTO item) {
            if (item.LastName == null)
                throw new ValidationException("Требуется ввести фамилию", "LastName");
            if (item.FirstName == null)
                throw new ValidationException("Требуется ввести имя", "FirstName");
            if (item.Contacts.Email != null) {
                try {
                    MailAddress m = new MailAddress(item.Contacts.Email);
                }
                catch (FormatException) {
                    throw new ValidationException("Некорректный email", "Email");
                }
            }
            if (item.Birth.BirthDate > DateTime.Today)
                throw new ValidationException("Некорректная дата рождения", "Birth.BirthDate");
            if (item.Age < 14)
                throw new ValidationException("Нельзя принять сотрудника моложе 14 лет", "Birth.BirthDate");
            Post post = await Database.Posts.FindByIdAsync(item.PostId ?? 0);
            if (post == null)
                throw new ValidationException("Должность не найдена", "");
            int employeesCount = await Database.Employees.CountAsync(e => e.PostId == item.PostId && e.Id != item.Id);
            if (employeesCount >= post.NumberOfUnits)
                throw new ValidationException("В данной должности нет свободных штатных единиц", "PostId");
        }

        private Employee MapDTOAndDomainModel(EmployeeDTO eDto) {
            Mapper.Initialize(cfg => {
                cfg.CreateMap<EmployeeDTO, Employee>();
                cfg.CreateMap<BaseBusinessTripDTO, BusinessTrip>();
                cfg.CreateMap<DepartmentDTO, Department>()
                   .ForMember(d => d.Posts, opt => opt.Ignore());
                cfg.CreateMap<PostDTO, Post>()
                   .ForMember(p => p.Employees, opt => opt.Ignore());

                cfg.CreateMap<DTO.Birth, DAL.Entities.Birth>();
                cfg.CreateMap<DTO.Passport, DAL.Entities.Passport>();
                cfg.CreateMap<DTO.Contacts, DAL.Entities.Contacts>();
                cfg.CreateMap<DTO.Education, DAL.Entities.Education>();
            });
            Employee employee = Mapper.Map<EmployeeDTO, Employee>(eDto);
            return employee;
        }

        // Обновление информации о сотруднике
        public override async Task EditAsync(EmployeeDTO eDto) {
            Employee employee = await MapDTOAndDomainModelWithValidationAsync(eDto);
            Database.Employees.Update(employee);
            await Database.SaveAsync();
        }

        // Получение сотрудника по id
        public override async Task<EmployeeDTO> FindByIdAsync(int? id) {
            if (id == null)
                throw new ValidationException("Не установлено id сотрудника", "");
            Employee employee = await Database.Employees.FindByIdAsync(id.Value);
            if (employee == null)
                throw new ValidationException("Сотрудник не найден", "");
            InitializeMapper();
            EmployeeDTO eDto = Mapper.Map<Employee, EmployeeDTO>(employee);
            return eDto;
        }

        private void InitializeMapper() {
            Mapper.Initialize(cfg => {
                cfg.CreateMap<Employee, EmployeeDTO>();
                cfg.CreateMap<BusinessTrip, BaseBusinessTripDTO>();
                cfg.CreateMap<Department, DepartmentDTO>()
                    .ForMember(d => d.Posts, opt => opt.Ignore());
                cfg.CreateMap<Post, PostDTO>()
                    .ForMember(p => p.Employees, opt => opt.Ignore());

                cfg.CreateMap<DAL.Entities.Birth, DTO.Birth>();
                cfg.CreateMap<DAL.Entities.Passport, DTO.Passport>();
                cfg.CreateMap<DAL.Entities.Contacts, DTO.Contacts>();
                cfg.CreateMap<DAL.Entities.Education, DTO.Education>();
            });
        }

        // Получение списка всех сотрудников
        public override async Task<IEnumerable<EmployeeDTO>> GetAllAsync() {
            IEnumerable<Employee> employees = await Database.Employees.GetAllAsync();
            InitializeMapper();
            IEnumerable<EmployeeDTO> collection = Mapper.Map<IEnumerable<Employee>, IEnumerable<EmployeeDTO>>(employees);
            return collection;
        }

        // Получение списка сотрудников по фильтру
        public virtual IEnumerable<EmployeeDTO> Get(EmployeeFilter filter) {
            Func<Employee, bool> predicate = CreatePredicate(filter);
            IEnumerable<Employee> filteredSortedCollection = FilterAndSortEmployees(filter, predicate);
            InitializeMapper();
            IEnumerable<EmployeeDTO> collection = Mapper.Map<IEnumerable<Employee>, IEnumerable<EmployeeDTO>>(filteredSortedCollection);
            return collection;
        }

        private Func<Employee, bool> CreatePredicate(EmployeeFilter filter) {
            bool predicate(Employee employee) {
                bool returnValue = false;
                if (filter.IsAntiFilter)    // Если флаг установлен, то выбираются записи несоответствующие фильтру
                    returnValue = true;
                // Фильтр по фамилии
                if (filter.LastName != null) {
                    string[] lastNameArray = filter.LastName.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();   // Удаляем пустые элементы из массива
                    if (lastNameArray.Length == 1)          // Если заполнено только одно поле LastName
                    {
                        if (!Regex.IsMatch(employee.LastName, lastNameArray[0], RegexOptions.IgnoreCase))
                            return returnValue;             // Проверяемая запись не соответствует фильтру по фамилии
                    }
                    else if (lastNameArray.Length > 1)      // Если заполнено несколько полей LastName
                    {
                        if (filter.IsMatchAnyLastName)      // Если установлена галка "Минимум одно соответствие"
                        {
                            bool flag = true;
                            foreach (string lastName in lastNameArray)
                                if (Regex.IsMatch(employee.LastName, lastName, RegexOptions.IgnoreCase)) {
                                    flag = false;
                                    break;
                                }
                            if (flag) return returnValue;   // Проверяемая запись не соответствует фильтру по фамилии
                        }
                        else    // Если не установлена галка "Минимум одно соответствие" (т.е. фамилия должна соответствовать всем шаблонам)
                        {
                            foreach (string lastName in lastNameArray)
                                if (!Regex.IsMatch(employee.LastName, lastName, RegexOptions.IgnoreCase))
                                    return returnValue;     // Проверяемая запись не соответствует фильтру по фамилии
                        }
                    }
                }
                // Фильтр по email
                if (!string.IsNullOrWhiteSpace(filter.Email) && !Regex.IsMatch(employee.Contacts.Email, filter.Email, RegexOptions.IgnoreCase))
                    return returnValue;             // Проверяемая запись не соответствует фильтру по email
                // Фильтр по номеру мобильного телефона
                if (filter.IsPhoneNumber && string.IsNullOrWhiteSpace(employee.Contacts.MobilePhone))
                    return returnValue;             // Проверяемая запись не соответствует фильтру по наличию номера телефона
                // Фильтр по дате найма на работу
                if (!string.IsNullOrWhiteSpace(filter.HireDate) && DateTime.TryParse(filter.HireDate, out DateTime date) && employee.HireDate != date)
                    return returnValue;             // Проверяемая запись не соответствует фильтру по дате найма на работу
                // Фильтры по зарплате
                if (filter.MinSalary.HasValue && employee.Post.Salary < filter.MinSalary.Value)
                    return returnValue;             // Проверяемая запись не соответствует фильтру по минимальной зарплате
                if (filter.MaxSalary.HasValue && employee.Post.Salary > filter.MaxSalary.Value)
                    return returnValue;             // Проверяемая запись не соответствует фильтру по максимальной зарплате
                // Фильтр по премии
                if (filter.Bonus != null) {
                    double?[] bonuses = filter.Bonus.Where(b => b != null).ToArray();
                    if (bonuses.Length > 0 && !bonuses.Contains(employee.Post.Premium))
                        return returnValue;         // Проверяемая запись не соответствует фильтру по премии
                }
                if (filter.IsBonus && employee.Post.Premium == 0)
                    return returnValue;             // Проверяемая запись не соответствует фильтру по наличию премии
                // Фильтр по должности
                if (!string.IsNullOrWhiteSpace(filter.PostTitle) && !Regex.IsMatch(employee.Post.Title, filter.PostTitle, RegexOptions.IgnoreCase))
                    return returnValue;             // Проверяемая запись не соответствует фильтру по должности
                // Фильтр по названию отдела
                if (!string.IsNullOrWhiteSpace(filter.DepartmentName) && !Regex.IsMatch(employee.Post.Department.DepartmentName, filter.DepartmentName, RegexOptions.IgnoreCase))
                    return returnValue;             // Проверяемая запись не соответствует фильтру по названию отдела
                return !returnValue;                // Если дошли до сюда, значит проверяемая запись соответствует фильтру
            }
            return predicate;
        }

        private IEnumerable<Employee> FilterAndSortEmployees(EmployeeFilter filter, Func<Employee, bool> predicate) {
            IEnumerable<Employee> filteredSortedCollection;
            // Параметры сортировки
            string sortField = filter.SortField ?? "Default";
            string order = filter.SortOrder ?? "Asc";
            // Компараторы сортировки по возрастанию или по убыванию
            IComparer<string> stringComparer = Comparer<string>.Create((x, y) => order.Equals("Asc") ? (x ?? "").CompareTo(y ?? "") : (y ?? "").CompareTo(x ?? ""));
            IComparer<double?> doubleNullableComparer = Comparer<double?>.Create((x, y) => order.Equals("Asc") ? (x ?? 0).CompareTo(y ?? 0) : (y ?? 0).CompareTo(x ?? 0));
            IComparer<DateTime> dateTimeComparer = Comparer<DateTime>.Create((x, y) => order.Equals("Asc") ? x.CompareTo(y) : y.CompareTo(x));
            switch (sortField) {
                case "LastName": filteredSortedCollection = Database.Employees.Get(predicate).OrderBy(e => e.LastName, stringComparer); break;
                case "Email": filteredSortedCollection = Database.Employees.Get(predicate).OrderBy(e => e.Contacts.Email, stringComparer); break;
                case "HireDate": filteredSortedCollection = Database.Employees.Get(predicate).OrderBy(e => e.HireDate, dateTimeComparer); break;
                case "Salary": filteredSortedCollection = Database.Employees.Get(predicate).OrderBy(e => e.Post.Salary, doubleNullableComparer); break;
                case "Bonus": filteredSortedCollection = Database.Employees.Get(predicate).OrderBy(e => e.Post.Premium, doubleNullableComparer); break;
                case "PostTitle": filteredSortedCollection = Database.Employees.Get(predicate).OrderBy(e => e.Post.Title, stringComparer); break;
                case "DepartmentName": filteredSortedCollection = Database.Employees.Get(predicate).OrderBy(e => e.Post.Department.DepartmentName, stringComparer); break;
                default: filteredSortedCollection = Database.Employees.Get(predicate).OrderBy(e => e.LastName); break;
            }
            return filteredSortedCollection;
        }

        // Удаление сотрудника
        public override async Task DeleteAsync(int id) {
            Employee employee = await Database.Employees.FindByIdAsync(id);
            if (employee == null) return;
            Database.Employees.Remove(employee);
            await Database.SaveAsync();
        }

        // Удаление всех сотрудников
        public override async Task DeleteAllAsync() {
            await Database.Employees.RemoveAllAsync();
            await Database.SaveAsync();
        }

        // Запись информации о сотрудниках в JSON-файл
        public override async Task ExportJsonAsync(string fullPath) {
            IEnumerable<Employee> employees = await Database.Employees.GetAllAsync();
            var transformEmployees = employees.Select(e => new {
                e.Age,
                e.Birth,
                e.Contacts,
                e.Education,
                e.FirstName,
                e.Gender,
                e.HireDate,
                e.LastName,
                e.Passport,
                e.Patronymic,
                e.PersonnelNumber,
                e.PostId
            }).ToArray();
            using (StreamWriter sw = new StreamWriter(fullPath, true, Encoding.UTF8)) {
                sw.WriteLine("{\"Employees\":");
                sw.WriteLine(new JavaScriptSerializer().Serialize(transformEmployees));
                sw.WriteLine("}");
            }
        }

        // Количество сотрудников
        public override async Task<int> CountAsync() {
            return await Database.Employees.CountAsync();
        }

        // Количество сотрудников, удовлетворяющих предикату
        public override async Task<int> CountAsync(Expression<Func<EmployeeDTO, bool>> predicateDTO) {
            Mapper.Initialize(cfg => cfg.CreateMap<EmployeeDTO, Employee>());
            var predicate = Mapper.Map<Expression<Func<EmployeeDTO, bool>>, Expression<Func<Employee, bool>>>(predicateDTO);
            return await Database.Employees.CountAsync(predicate);
        }

        // Добавление сотрудников для тестирования
        public async Task TestCreateAsync() {
            int[] postIds = (await Database.Posts.GetAllAsync()).Select(p => p.Id).ToArray();
            if (postIds.Length == 0)
                throw new ValidationException("Сначала нужно добавить хотя бы одну должность", "");
            string[] lastNames = { "Smith", "Johnson", "Williams", "Jones", "Brown", "Davis", "Miller", "Wilson", "Moore", "Taylor", "Anderson", "Thomas", "Jackson",
                "White", "Harris", "Martin", "Thompson", "Wood", "Lewis", "Scott", "Cooper", "King", "Green", "Walker", "Edwards", "Turner", "Morgan", "Baker", "Hill" };
            string[] firstNames = { "James", "Alex", "Ben", "Daniel", "Tom", "Ryan", "Sam", "Lewis", "Joe", "David", "Harry", "George", "Jamie", "Dan", "Matt", "Robert" };
            string[] emails = { "@mail.ru", "@yandex.ru", "@gmail.com" };
            string[] phones = { "+7-935-456-31-01", "+7-935-742-25-02", "+7-935-788-08-03", "+7-935-412-77-04", "+7-935-224-39-05", "+7-935-941-85-06", "+7-935-254-56-07" };
            for (int i = 0; i < 100; i++) {
                Employee employee = new Employee {
                    LastName = lastNames[i % lastNames.Length],
                    FirstName = firstNames[i % firstNames.Length],
                    Contacts = new DAL.Entities.Contacts {
                        Email = lastNames[i % lastNames.Length] + emails[i % emails.Length],
                        MobilePhone = phones[i % phones.Length]
                    },
                    HireDate = new DateTime(i % 18 + 2000, i % 12 + 1, i % 28 + 1),
                    PostId = postIds[i % postIds.Length],
                    Birth = new DAL.Entities.Birth { BirthDate = new DateTime(i % 18 + 1970, i % 12 + 1, i % 28 + 1) },
                    Passport = new DAL.Entities.Passport(),
                    Education = new DAL.Entities.Education()
                };
                Database.Employees.Create(employee);
            }
            await Database.SaveAsync();
        }

        public override void Dispose() {
            Database.Dispose();
        }
    }
}
