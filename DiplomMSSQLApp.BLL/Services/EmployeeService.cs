﻿using AutoMapper;
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

        // Добавление нового сотрудника (с валидацией)
        public override async Task CreateAsync(EmployeeDTO eDto) {
            Employee employee = await MapDTOAndDomainModelWithValidationAsync(eDto);
            Database.Employees.Create(employee);
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
            return Mapper.Map<EmployeeDTO, Employee>(eDto);
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

        public override void Dispose() {
            Database.Dispose();
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
            return Mapper.Map<Employee, EmployeeDTO>(employee);
        }

        // Получение списка сотрудников по фильтру
        public override IEnumerable<EmployeeDTO> Get(EmployeeFilter filter) {
            Func<Employee, bool> predicate = CreatePredicate(filter);
            CreateMessageAboutFilterParametersUsed(filter);
            IEnumerable<Employee> sortedCollection = FilterAndSortEmployees(filter, predicate);
            InitializeMapper();
            List<EmployeeDTO> collection = Mapper.Map<IEnumerable<Employee>, List<EmployeeDTO>>(sortedCollection);
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
                if (!string.IsNullOrWhiteSpace(filter.Email) && !Regex.IsMatch(employee.Email, filter.Email, RegexOptions.IgnoreCase))
                    return returnValue;             // Проверяемая запись не соответствует фильтру по email
                // Фильтр по номеру телефона
                if (filter.IsPhoneNumber && string.IsNullOrWhiteSpace(employee.PhoneNumber))
                    return returnValue;             // Проверяемая запись не соответствует фильтру по наличию номера телефона
                // Фильтр по дате найма на работу
                if (!string.IsNullOrWhiteSpace(filter.HireDate) && DateTime.TryParse(filter.HireDate, out DateTime date) && employee.HireDate != date)
                    return returnValue;             // Проверяемая запись не соответствует фильтру по дате найма на работу
                // Фильтры по зарплате
                if (filter.MinSalary.HasValue && employee.Salary.HasValue && employee.Salary.Value < filter.MinSalary.Value)
                    return returnValue;             // Проверяемая запись не соответствует фильтру по минимальной зарплате
                if (filter.MaxSalary.HasValue && employee.Salary.HasValue && employee.Salary.Value > filter.MaxSalary.Value)
                    return returnValue;             // Проверяемая запись не соответствует фильтру по максимальной зарплате
                // Фильтр по премии
                if (filter.Bonus != null) {
                    double?[] bonuses = filter.Bonus.Where(b => b != null).ToArray();
                    if (bonuses.Length > 0 && !bonuses.Contains(employee.Bonus))
                        return returnValue;         // Проверяемая запись не соответствует фильтру по премии
                }
                if (filter.IsBonus && !employee.Bonus.HasValue)
                    return returnValue;             // Проверяемая запись не соответствует фильтру по наличию премии
                // Фильтр по должности
                if (!string.IsNullOrWhiteSpace(filter.PostTitle) && !Regex.IsMatch(employee.Post.Title, filter.PostTitle, RegexOptions.IgnoreCase))
                    return returnValue;             // Проверяемая запись не соответствует фильтру по должности
                // Фильтр по названию отдела
                if (!string.IsNullOrWhiteSpace(filter.DepartmentName) && !Regex.IsMatch(employee.Department.DepartmentName, filter.DepartmentName, RegexOptions.IgnoreCase))
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
            IEnumerable<Employee> sortedCollection;
            // Параметры сортировки
            string sortField = filter.SortField ?? "Default";
            string order = filter.SortOrder ?? "Asc";
            // Компараторы сортировки по возрастанию или по убыванию
            IComparer<string> stringComparer = Comparer<string>.Create((x, y) => order.Equals("Asc") ? (x ?? "").CompareTo(y ?? "") : (y ?? "").CompareTo(x ?? ""));
            IComparer<double?> doubleNullableComparer = Comparer<double?>.Create((x, y) => order.Equals("Asc") ? (x ?? 0).CompareTo(y ?? 0) : (y ?? 0).CompareTo(x ?? 0));
            IComparer<DateTime> dateTimeComparer = Comparer<DateTime>.Create((x, y) => order.Equals("Asc") ? x.CompareTo(y) : y.CompareTo(x));
            switch (sortField) {
                case "LastName": sortedCollection = SortByLastName(predicate, stringComparer); break;
                case "Email": sortedCollection = Database.Employees.Get(predicate).OrderBy(e => e.Email, stringComparer); break;
                case "HireDate": sortedCollection = Database.Employees.Get(predicate).OrderBy(e => e.HireDate, dateTimeComparer); break;
                case "Salary": sortedCollection = Database.Employees.Get(predicate).OrderBy(e => e.Salary, doubleNullableComparer); break;
                case "Bonus": sortedCollection = Database.Employees.Get(predicate).OrderBy(e => e.Bonus, doubleNullableComparer); break;
                case "PostTitle": sortedCollection = Database.Employees.Get(predicate).OrderBy(e => e.Post.Title, stringComparer); break;
                case "DepartmentName": sortedCollection = Database.Employees.Get(predicate).OrderBy(e => e.Department.DepartmentName, stringComparer); break;
                default: sortedCollection = Database.Employees.Get(predicate).OrderBy(e => e.LastName); break;
            }
            return sortedCollection;
        }

        private IEnumerable<Employee> SortByLastName(Func<Employee, bool> predicate, IComparer<string> stringComparer) {
            IEnumerable<Employee> sortedCollection = SortByLastNameWithTimeMeasurement(predicate, stringComparer);
            WriteMessageAndElapsedTimeInFile();
            return sortedCollection;
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
            using (StreamWriter sw = new StreamWriter(PathToFileForTests, true, Encoding.UTF8)) {
                sw.WriteLine("Используемые параметры фильтра: " + MessageAboutFilterParametersUsed + "Время: " + ElapsedTime);
            }
        }

        // Получение списка всех сотрудников
        public override async Task<IEnumerable<EmployeeDTO>> GetAllAsync() {
            IEnumerable<Employee> employees = await Database.Employees.GetAsync();
            InitializeMapper();
            IEnumerable<EmployeeDTO> collection = Mapper.Map<IEnumerable<Employee>, IEnumerable<EmployeeDTO>>(employees);
            return collection;
        }

        // Валидация модели
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
            if (item.Salary != null && post?.MinSalary != null && item.Salary < post.MinSalary)
                throw new ValidationException("Зарплата должна быть больше " + post.MinSalary, "Salary");
            if (item.Salary != null && post?.MaxSalary != null && item.Salary > post.MaxSalary)
                throw new ValidationException("Зарплата должна быть меньше " + post.MaxSalary, "Salary");
            Department department = await Database.Departments.FindByIdAsync(item.DepartmentId ?? 0);
            if (department == null)
                throw new ValidationException("Отдел не найден", "");
        }

        // Тест добавления сотрудников
        public override async Task TestCreateAsync(int num, string path) {
            IEnumerable<Employee> employees = CreateEmployeesCollectionForTest(num);
            string elapsedTime = await RunTestCreateAsync(employees);
            WriteResultTestCreateInFile(num, elapsedTime, path);
        }

        private IEnumerable<Employee> CreateEmployeesCollectionForTest(int num) {
            List<Employee> employees = new List<Employee>();
            string[] lastNames = { "Smith", "Johnson", "Williams", "Jones", "Brown", "Davis", "Miller", "Wilson", "Moore", "Taylor", "Anderson", "Thomas", "Jackson",
                "White", "Harris", "Martin", "Thompson", "Wood", "Lewis", "Scott", "Cooper", "King", "Green", "Walker", "Edwards", "Turner", "Morgan", "Baker", "Hill" };
            string[] firstNames = { "James", "Alex", "Ben", "Daniel", "Tom", "Ryan", "Sam", "Lewis", "Joe", "David", "Harry", "George", "Jamie", "Dan", "Matt", "Robert" };
            string[] emails = { "@mail.ru", "@yandex.ru", "@gmail.com" };
            double[] bonuses = { 0.05, 0.1, 0.15, 0.2 };
            Post firstPost = Database.Posts.GetFirst() ?? new Post { Id = 1, Title = "unknown" };
            Department firstDepartment = Database.Departments.GetFirst() ?? new Department { Id = 1, DepartmentName = "unknown" };
            for (int i = 0; i < num; i++) {
                Employee employee = new Employee {
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
                employees.Add(employee);
            }
            return employees;
        }

        private async Task<string> RunTestCreateAsync(IEnumerable<Employee> employees) {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            Database.Employees.Create(employees);
            await Database.SaveAsync();
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTime = $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds:000}";
            return elapsedTime;
        }

        private void WriteResultTestCreateInFile(int num, string elapsedTime, string path) {
            StringBuilder sb = new StringBuilder();
            sb.Append("Количество добавленных сотрудников: ");
            sb.Append(num);
            sb.Append("; Время: ");
            sb.Append(elapsedTime);
            using (StreamWriter sw = new StreamWriter(path, true, Encoding.UTF8)) {
                sw.WriteLine(sb.ToString());
            }
        }

        // Тест выборки сотрудников
        public override async Task TestReadAsync(int num, string path, int salary) {
            int employeesCount = (await Database.Employees.GetAsync()).Count();
            int resultCount = Database.Employees.Get(salary).Count();
            string elapsedTime = RunTestRead(num, salary);
            WriteResultTestReadInFile(employeesCount,  resultCount, num, salary, elapsedTime, path);
        }

        private string RunTestRead(int num, int salary) {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            for (int i = 0; i < num; i++) {
                Database.Employees.Get(salary);
            }
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTime = $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds:000}";
            return elapsedTime;
        }

        private void WriteResultTestReadInFile(int employeesCount, int resultCount, int num, int salary, string elapsedTime, string path) {
            StringBuilder sb = new StringBuilder();
            sb.Append("Общее количество сотрудников: ");
            sb.Append(employeesCount);
            sb.Append("; Количество найденных сотрудников: ");
            sb.Append(resultCount);
            sb.Append("; Количество выборок: ");
            sb.Append(num);
            sb.Append("; Условие выборки: Salary >= ");
            sb.Append(salary);
            sb.Append("; Время: ");
            sb.Append(elapsedTime);
            using (StreamWriter sw = new StreamWriter(path, true, Encoding.UTF8)) {
                sw.WriteLine(sb.ToString());
            }
        }

        // Тест обновления сотрудников
        public override async Task TestUpdateAsync(int num, string path) {
            Employee[] employees = (await Database.Employees.GetAsync()).Take(num).ToArray();
            int employeesLength = employees.Length;
            string elapsedTime = await RunTestUpdateAsync(employees, num);
            WriteResultTestUpdateInFile(employeesLength, elapsedTime, path);
        }

        private async Task<string> RunTestUpdateAsync(Employee[] employees, int num) {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            for (int i = 0; i < employees.Length; i++) {
                employees[i].LastName = "Martin";
                employees[i].FirstName = "Max";
                employees[i].Email = "Martin@mail.ru";
                employees[i].HireDate = new DateTime(2018, 8, 8);
                employees[i].Salary = 100000;
                employees[i].PhoneNumber = "+7-944-569-85-36";
                employees[i].Bonus = 0.2;
                employees[i].Address = "Moskow Mira Avenue 105";
                Database.Employees.Update(employees[i]);
                await Database.SaveAsync();
            }
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTime = $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds:000}";
            return elapsedTime;
        }

        private void WriteResultTestUpdateInFile(int employeesLength, string elapsedTime, string path) {
            StringBuilder sb = new StringBuilder();
            sb.Append("Количество обновленных сотрудников: ");
            sb.Append(employeesLength);
            sb.Append("; Время: ");
            sb.Append(elapsedTime);
            using (StreamWriter sw = new StreamWriter(path, true, Encoding.UTF8)) {
                sw.WriteLine(sb.ToString());
            }
        }

        // Тест удаления сотрудников
        public override async Task TestDeleteAsync(int num, string path) {
            IEnumerable<Employee> employees = (await Database.Employees.GetAsync()).Take(num);
            int employeesCount = employees.Count();
            string elapsedTime = await RunTestDeleteAsync(employees);
            WriteResultTestDeleteInFile(employeesCount, elapsedTime, path);
        }

        private async Task<string> RunTestDeleteAsync(IEnumerable<Employee> employees) {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            Database.Employees.RemoveSeries(employees);
            await Database.SaveAsync();
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTime = $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds:000}";
            return elapsedTime;
        }

        private void WriteResultTestDeleteInFile(int employeesCount, string elapsedTime, string path) {
            StringBuilder sb = new StringBuilder();
            sb.Append("Количество удаленных сотрудников: ");
            sb.Append(employeesCount);
            sb.Append("; Время: ");
            sb.Append(elapsedTime);
            using (StreamWriter sw = new StreamWriter(path, true, Encoding.UTF8)) {
                sw.WriteLine(sb.ToString());
            }
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
    }
}
