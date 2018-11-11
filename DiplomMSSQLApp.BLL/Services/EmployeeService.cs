using AutoMapper;
using DiplomMSSQLApp.BLL.BusinessModels;
using DiplomMSSQLApp.BLL.DTO;
using DiplomMSSQLApp.BLL.Infrastructure;
using DiplomMSSQLApp.DAL.Entities;
using DiplomMSSQLApp.DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace DiplomMSSQLApp.BLL.Services {
    public class EmployeeService : BaseService<EmployeeDTO> {
        public IUnitOfWork Database { get; set; }
        public string MessageAboutFilterParametersUsed { get; set; }   // Сообщение записывается в файл WEB/Results/Employee/Filter.txt
        public string ElapsedTime { get; set; }
        public string PathToFileForTests { get; set; }

        public EmployeeService(IUnitOfWork uow) {
            Database = uow;
        }

        public EmployeeService() { }

        // Добавление нового сотрудника
        public override async Task CreateAsync(EmployeeDTO eDto) {
            Employee Employee = await MapDTOAndDomainModelWithValidationAsync(eDto);
            Database.Employees.Create(Employee);
            await Database.SaveAsync();
        }

        private async Task<Employee> MapDTOAndDomainModelWithValidationAsync(EmployeeDTO eDto) {
            await ValidationEmployeeAsync(eDto);
            Mapper.Initialize(cfg => {
                cfg.CreateMap<EmployeeDTO, Employee>();
                cfg.CreateMap<BaseBusinessTripDTO, BusinessTrip>();
                cfg.CreateMap<DepartmentDTO, Department>()
                   .ForMember(d => d.Employees, opt => opt.Ignore());
                cfg.CreateMap<PostDTO, Post>()
                   .ForMember(p => p.Employees, opt => opt.Ignore());
            });
            Employee Employee = Mapper.Map<EmployeeDTO, Employee>(eDto);
            return Employee;
        }

        private async Task ValidationEmployeeAsync(EmployeeDTO item) {
            if (item.LastName == null)
                throw new ValidationException("Требуется ввести фамилию", "LastName");
            if (item.FirstName == null)
                throw new ValidationException("Требуется ввести имя", "FirstName");
            if (item.Email != null) {
                try {
                    MailAddress m = new MailAddress(item.Email);
                }
                catch (FormatException) {
                    throw new ValidationException("Некорректный email", "Email");
                }
            }
            Post post = await Database.Posts.FindByIdAsync(item.PostId ?? 0);
            if (post == null)
                throw new ValidationException("Должность не найдена", "");
            Department department = await Database.Departments.FindByIdAsync(item.DepartmentId ?? 0);
            if (department == null)
                throw new ValidationException("Отдел не найден", "");
        }

        // Обновление информации о сотруднике
        public override async Task EditAsync(EmployeeDTO eDto) {
            Employee Employee = await MapDTOAndDomainModelWithValidationAsync(eDto);
            Database.Employees.Update(Employee);
            await Database.SaveAsync();
        }

        // Получение сотрудника по id
        public override async Task<EmployeeDTO> FindByIdAsync(int? id) {
            if (id == null)
                throw new ValidationException("Не установлено id сотрудника", "");
            Employee Employee = await Database.Employees.FindByIdAsync(id.Value);
            if (Employee == null)
                throw new ValidationException("Сотрудник не найден", "");
            InitializeMapper();
            EmployeeDTO eDto = Mapper.Map<Employee, EmployeeDTO>(Employee);
            return eDto;
        }

        private void InitializeMapper() {
            Mapper.Initialize(cfg => {
                cfg.CreateMap<Employee, EmployeeDTO>();
                cfg.CreateMap<BusinessTrip, BaseBusinessTripDTO>();
                cfg.CreateMap<Department, DepartmentDTO>()
                    .ForMember(d => d.Employees, opt => opt.Ignore());
                cfg.CreateMap<Post, PostDTO>()
                    .ForMember(p => p.Employees, opt => opt.Ignore());
            });
        }

        // Получение списка всех сотрудников
        public override async Task<IEnumerable<EmployeeDTO>> GetAllAsync() {
            IEnumerable<Employee> Employees = await Database.Employees.GetAsync();
            InitializeMapper();
            IEnumerable<EmployeeDTO> collection = Mapper.Map<IEnumerable<Employee>, IEnumerable<EmployeeDTO>>(Employees);
            return collection;
        }

        // Получение списка сотрудников по фильтру
        public override IEnumerable<EmployeeDTO> Get(EmployeeFilter filter) {
            Func<Employee, bool> predicate = CreatePredicate(filter);
            CreateMessageAboutFilterParametersUsed(filter);
            IEnumerable<Employee> filteredSortedCollection = FilterAndSortEmployees(filter, predicate);
            InitializeMapper();
            IEnumerable<EmployeeDTO> collection = Mapper.Map<IEnumerable<Employee>, IEnumerable<EmployeeDTO>>(filteredSortedCollection);
            return collection;
        }

        private Func<Employee, bool> CreatePredicate(EmployeeFilter filter) {
            bool predicate(Employee Employee) {
                bool returnValue = false;
                if (filter.IsAntiFilter)    // Если флаг установлен, то выбираются записи несоответствующие фильтру
                    returnValue = true;
                // Фильтр по фамилии
                if (filter.LastName != null) {
                    string[] lastNameArray = filter.LastName.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();   // Удаляем пустые элементы из массива
                    if (lastNameArray.Length == 1)          // Если заполнено только одно поле LastName
                    {
                        if (!Regex.IsMatch(Employee.LastName, lastNameArray[0], RegexOptions.IgnoreCase))
                            return returnValue;             // Проверяемая запись не соответствует фильтру по фамилии
                    }
                    else if (lastNameArray.Length > 1)      // Если заполнено несколько полей LastName
                    {
                        if (filter.IsMatchAnyLastName)      // Если установлена галка "Минимум одно соответствие"
                        {
                            bool flag = true;
                            foreach (string lastName in lastNameArray)
                                if (Regex.IsMatch(Employee.LastName, lastName, RegexOptions.IgnoreCase)) {
                                    flag = false;
                                    break;
                                }
                            if (flag) return returnValue;   // Проверяемая запись не соответствует фильтру по фамилии
                        }
                        else    // Если не установлена галка "Минимум одно соответствие" (т.е. фамилия должна соответствовать всем шаблонам)
                        {
                            foreach (string lastName in lastNameArray)
                                if (!Regex.IsMatch(Employee.LastName, lastName, RegexOptions.IgnoreCase))
                                    return returnValue;     // Проверяемая запись не соответствует фильтру по фамилии
                        }
                    }
                }
                // Фильтр по email
                if (!string.IsNullOrWhiteSpace(filter.Email) && !Regex.IsMatch(Employee.Email, filter.Email, RegexOptions.IgnoreCase))
                    return returnValue;             // Проверяемая запись не соответствует фильтру по email
                // Фильтр по номеру телефона
                if (filter.IsPhoneNumber && string.IsNullOrWhiteSpace(Employee.PhoneNumber))
                    return returnValue;             // Проверяемая запись не соответствует фильтру по наличию номера телефона
                // Фильтр по дате найма на работу
                if (!string.IsNullOrWhiteSpace(filter.HireDate) && DateTime.TryParse(filter.HireDate, out DateTime date) && Employee.HireDate != date)
                    return returnValue;             // Проверяемая запись не соответствует фильтру по дате найма на работу
                // Фильтры по зарплате
                if (filter.MinSalary.HasValue && Employee.Salary.HasValue && Employee.Salary.Value < filter.MinSalary.Value)
                    return returnValue;             // Проверяемая запись не соответствует фильтру по минимальной зарплате
                if (filter.MaxSalary.HasValue && Employee.Salary.HasValue && Employee.Salary.Value > filter.MaxSalary.Value)
                    return returnValue;             // Проверяемая запись не соответствует фильтру по максимальной зарплате
                // Фильтр по премии
                if (filter.Bonus != null) {
                    double?[] bonuses = filter.Bonus.Where(b => b != null).ToArray();
                    if (bonuses.Length > 0 && !bonuses.Contains(Employee.Bonus))
                        return returnValue;         // Проверяемая запись не соответствует фильтру по премии
                }
                if (filter.IsBonus && !Employee.Bonus.HasValue)
                    return returnValue;             // Проверяемая запись не соответствует фильтру по наличию премии
                // Фильтр по должности
                if (!string.IsNullOrWhiteSpace(filter.PostTitle) && !Regex.IsMatch(Employee.Post.Title, filter.PostTitle, RegexOptions.IgnoreCase))
                    return returnValue;             // Проверяемая запись не соответствует фильтру по должности
                // Фильтр по названию отдела
                if (!string.IsNullOrWhiteSpace(filter.DepartmentName) && !Regex.IsMatch(Employee.Department.DepartmentName, filter.DepartmentName, RegexOptions.IgnoreCase))
                    return returnValue;             // Проверяемая запись не соответствует фильтру по названию отдела
                return !returnValue;                // Если дошли до сюда, значит проверяемая запись соответствует фильтру
            }
            return predicate;
        }

        private void CreateMessageAboutFilterParametersUsed(EmployeeFilter filter) {
            MessageAboutFilterParametersUsed = "";
            // Фильтр по фамилии
            if (filter.LastName != null)
                foreach (string lastName in filter.LastName)
                    if (!string.IsNullOrWhiteSpace(lastName))
                        MessageAboutFilterParametersUsed += "Фамилия = " + lastName + "; ";
            // Фильтр по email
            if (!string.IsNullOrWhiteSpace(filter.Email))
                MessageAboutFilterParametersUsed += "Email = " + filter.Email + "; ";
            // Фильтр по номеру телефона
            if (filter.IsPhoneNumber)
                MessageAboutFilterParametersUsed += "Есть телефон; ";
            // Фильтр по дате найма на работу
            if (!string.IsNullOrWhiteSpace(filter.HireDate))
                MessageAboutFilterParametersUsed += "Дата приема на работу = " + filter.HireDate + "; ";
            // Фильтры по зарплате
            if (filter.MinSalary.HasValue)
                MessageAboutFilterParametersUsed += "Зарплата >= " + filter.MinSalary.Value + "; ";
            if (filter.MaxSalary.HasValue)
                MessageAboutFilterParametersUsed += "Зарплата <= " + filter.MaxSalary.Value + "; ";
            // Фильтры по премии
            if (filter.Bonus != null)
                foreach (var bonus in filter.Bonus)
                    if (bonus.HasValue)
                        MessageAboutFilterParametersUsed += "Премия = " + bonus + "; ";
            if (filter.IsBonus)
                MessageAboutFilterParametersUsed += "Есть премия; ";
            // Фильтр по должности
            if (!string.IsNullOrWhiteSpace(filter.PostTitle))
                MessageAboutFilterParametersUsed += "Должность = " + filter.PostTitle + "; ";
            // Фильтр по названию отдела
            if (!string.IsNullOrWhiteSpace(filter.DepartmentName))
                MessageAboutFilterParametersUsed += "Название отдела = " + filter.DepartmentName + "; ";
            if (filter.IsAntiFilter)
                MessageAboutFilterParametersUsed += "Используется отрицание; ";
            if (MessageAboutFilterParametersUsed == "")
                MessageAboutFilterParametersUsed = "Фильтр не задан; ";
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
                case "LastName": filteredSortedCollection = SortByLastName(predicate, stringComparer); break;
                case "Email": filteredSortedCollection = Database.Employees.Get(predicate).OrderBy(e => e.Email, stringComparer); break;
                case "HireDate": filteredSortedCollection = Database.Employees.Get(predicate).OrderBy(e => e.HireDate, dateTimeComparer); break;
                case "Salary": filteredSortedCollection = Database.Employees.Get(predicate).OrderBy(e => e.Salary, doubleNullableComparer); break;
                case "Bonus": filteredSortedCollection = Database.Employees.Get(predicate).OrderBy(e => e.Bonus, doubleNullableComparer); break;
                case "PostTitle": filteredSortedCollection = Database.Employees.Get(predicate).OrderBy(e => e.Post.Title, stringComparer); break;
                case "DepartmentName": filteredSortedCollection = Database.Employees.Get(predicate).OrderBy(e => e.Department.DepartmentName, stringComparer); break;
                default: filteredSortedCollection = Database.Employees.Get(predicate).OrderBy(e => e.LastName); break;
            }
            return filteredSortedCollection;
        }

        private IEnumerable<Employee> SortByLastName(Func<Employee, bool> predicate, IComparer<string> stringComparer) {
            IEnumerable<Employee> filteredSortedCollection = SortByLastNameWithTimeMeasurement(predicate, stringComparer);
            WriteMessageAndElapsedTimeInFile();
            return filteredSortedCollection;
        }

        private IEnumerable<Employee> SortByLastNameWithTimeMeasurement(Func<Employee, bool> predicate, IComparer<string> stringComparer) {
            IEnumerable<Employee> sortedCollection;
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            sortedCollection = Database.Employees.Get(predicate).OrderBy(e => e.LastName, stringComparer).ToList();
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            ElapsedTime = $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds:000}";
            return sortedCollection;
        }

        private void WriteMessageAndElapsedTimeInFile() {
            StringBuilder sb = new StringBuilder();
            sb.Append("Используемые параметры фильтра: ");
            sb.Append(MessageAboutFilterParametersUsed);
            sb.Append("Время: ");
            sb.Append(ElapsedTime);
            using (StreamWriter sw = new StreamWriter(PathToFileForTests, true, Encoding.UTF8)) {
                sw.WriteLine(sb.ToString());
            }
        }

        // Удаление сотрудника
        public override async Task DeleteAsync(int id) {
            Employee Employee = await Database.Employees.FindByIdAsync(id);
            if (Employee == null) return;
            Database.Employees.Remove(Employee);
            await Database.SaveAsync();
        }

        // Удаление всех сотрудников
        public override async Task DeleteAllAsync() {
            await Database.Employees.RemoveAllAsync();
            await Database.SaveAsync();
        }

        // Запись информации о должностях в файл
        public virtual async Task ExportJsonAsync(string fullPath) {
            IEnumerable<Employee> Employees = await Database.Employees.GetAsync();
            var transformEmployees = Employees.Select(e => new {
                e.LastName,
                e.FirstName,
                e.Email,
                e.PhoneNumber,
                e.Address,
                HireDate = e.HireDate.ToShortDateString(),
                e.Salary,
                e.Bonus,
                Post = e.Post.Title,
                Department = e.Department.DepartmentName,
                BusinessTrips = e.BusinessTrips.Select(bt => bt.Name)
            }).ToArray();
            using (StreamWriter sw = new StreamWriter(fullPath, true, Encoding.UTF8)) {
                sw.WriteLine("{\"Employees\":[");
                int EmployeesLength = transformEmployees.Length;
                for (int i = 1; i < EmployeesLength; i++) {
                    sw.Write(new JavaScriptSerializer().Serialize(transformEmployees[i]));
                    if (i != EmployeesLength - 1) sw.WriteLine(",");
                }
                sw.WriteLine("]}");
            }
        }

        // Тест добавления сотрудников
        public override async Task TestCreateAsync(int num) {
            IEnumerable<Employee> Employees = await CreateEmployeesCollectionForTestAsync(num);
            await RunTestCreateAsync(Employees);
            WriteResultTestCreateInFile(num);
        }

        private async Task<IEnumerable<Employee>> CreateEmployeesCollectionForTestAsync(int num) {
            List<Employee> Employees = new List<Employee>();
            string[] lastNames = { "Smith", "Johnson", "Williams", "Jones", "Brown", "Davis", "Miller", "Wilson", "Moore", "Taylor", "Anderson", "Thomas", "Jackson",
                "White", "Harris", "Martin", "Thompson", "Wood", "Lewis", "Scott", "Cooper", "King", "Green", "Walker", "Edwards", "Turner", "Morgan", "Baker", "Hill" };
            string[] firstNames = { "James", "Alex", "Ben", "Daniel", "Tom", "Ryan", "Sam", "Lewis", "Joe", "David", "Harry", "George", "Jamie", "Dan", "Matt", "Robert" };
            string[] emails = { "@mail.ru", "@yandex.ru", "@gmail.com" };
            double[] bonuses = { 0.05, 0.1, 0.15, 0.2 };
            Post firstPost = (await Database.Posts.GetFirstAsync()) ?? new Post { Id = 1, Title = "unknown" };
            Department firstDepartment = (await Database.Departments.GetFirstAsync()) ?? new Department { Id = 1, DepartmentName = "unknown" };
            for (int i = 0; i < num; i++) {
                Employee Employee = new Employee {
                    LastName = lastNames[i % lastNames.Length],
                    FirstName = firstNames[i % firstNames.Length],
                    Email = lastNames[i % lastNames.Length] + emails[i % emails.Length],
                    HireDate = new DateTime(2015, 3, 14),
                    Salary = new double?(10000.0 + Math.Round(new Random(i).NextDouble() * 100) * 500),
                    PhoneNumber = "+7-935-728-35-07",
                    Bonus = bonuses[i % bonuses.Length],
                    Address = "Moskow, Kutuzovsky Avenue 57",
                    Post = firstPost,
                    PostId = firstPost.Id,
                    Department = firstDepartment,
                    DepartmentId = firstDepartment.Id
                };
                Employees.Add(Employee);
            }
            return Employees;
        }

        private async Task RunTestCreateAsync(IEnumerable<Employee> Employees) {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            Database.Employees.Create(Employees);
            await Database.SaveAsync();
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            ElapsedTime = $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds:000}";
        }

        private void WriteResultTestCreateInFile(int num) {
            StringBuilder sb = new StringBuilder();
            sb.Append("Количество добавленных сотрудников: ");
            sb.Append(num);
            sb.Append("; Время: ");
            sb.Append(ElapsedTime);
            using (StreamWriter sw = new StreamWriter(PathToFileForTests, true, Encoding.UTF8)) {
                sw.WriteLine(sb.ToString());
            }
        }

        // Тест выборки сотрудников
        public override async Task TestReadAsync(int num, int salary) {
            int EmployeesCount = (await Database.Employees.GetAsync()).Count();
            int resultCount = Database.Employees.Get(salary).Count();
            RunTestRead(num, salary);
            WriteResultTestReadInFile(EmployeesCount,  resultCount, num, salary);
        }

        private void RunTestRead(int num, int salary) {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            for (int i = 0; i < num; i++) {
                Database.Employees.Get(salary);
            }
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            ElapsedTime = $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds:000}";
        }

        private void WriteResultTestReadInFile(int EmployeesCount, int resultCount, int num, int salary) {
            StringBuilder sb = new StringBuilder();
            sb.Append("Общее количество сотрудников: ");
            sb.Append(EmployeesCount);
            sb.Append("; Количество найденных сотрудников: ");
            sb.Append(resultCount);
            sb.Append("; Количество выборок: ");
            sb.Append(num);
            sb.Append("; Условие выборки: Salary >= ");
            sb.Append(salary);
            sb.Append("; Время: ");
            sb.Append(ElapsedTime);
            using (StreamWriter sw = new StreamWriter(PathToFileForTests, true, Encoding.UTF8)) {
                sw.WriteLine(sb.ToString());
            }
        }

        // Тест обновления сотрудников
        public override async Task TestUpdateAsync(int num) {
            Employee[] Employees = (await Database.Employees.GetAsync()).Take(num).ToArray();
            int EmployeesLength = Employees.Length;
            await RunTestUpdateAsync(Employees);
            WriteResultTestUpdateInFile(EmployeesLength);
        }

        private async Task RunTestUpdateAsync(Employee[] Employees) {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            for (int i = 0; i < Employees.Length; i++) {
                Employees[i].LastName = "Martin";
                Employees[i].FirstName = "Max";
                Employees[i].Email = "Martin@mail.ru";
                Employees[i].HireDate = new DateTime(2018, 8, 8);
                Employees[i].Salary = 100000;
                Employees[i].PhoneNumber = "+7-944-569-85-36";
                Employees[i].Bonus = 0.2;
                Employees[i].Address = "Moskow Mira Avenue 105";
                Database.Employees.Update(Employees[i]);
                await Database.SaveAsync();
            }
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            ElapsedTime = $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds:000}";
        }

        private void WriteResultTestUpdateInFile(int EmployeesLength) {
            StringBuilder sb = new StringBuilder();
            sb.Append("Количество обновленных сотрудников: ");
            sb.Append(EmployeesLength);
            sb.Append("; Время: ");
            sb.Append(ElapsedTime);
            using (StreamWriter sw = new StreamWriter(PathToFileForTests, true, Encoding.UTF8)) {
                sw.WriteLine(sb.ToString());
            }
        }

        // Тест удаления сотрудников
        public override async Task TestDeleteAsync(int num) {
            IEnumerable<Employee> Employees = (await Database.Employees.GetAsync()).Take(num);
            int EmployeesCount = Employees.Count();
            await RunTestDeleteAsync(Employees);
            WriteResultTestDeleteInFile(EmployeesCount);
        }

        private async Task RunTestDeleteAsync(IEnumerable<Employee> Employees) {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            Database.Employees.RemoveSeries(Employees);
            await Database.SaveAsync();
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            ElapsedTime = $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds:000}";
        }

        private void WriteResultTestDeleteInFile(int EmployeesCount) {
            StringBuilder sb = new StringBuilder();
            sb.Append("Количество удаленных сотрудников: ");
            sb.Append(EmployeesCount);
            sb.Append("; Время: ");
            sb.Append(ElapsedTime);
            using (StreamWriter sw = new StreamWriter(PathToFileForTests, true, Encoding.UTF8)) {
                sw.WriteLine(sb.ToString());
            }
        }

        public override void Dispose() {
            Database.Dispose();
        }

        public override Task<EmployeeDTO> GetFirstAsync() {
            throw new NotImplementedException();
        }
    }
}
