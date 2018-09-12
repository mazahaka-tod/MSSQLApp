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
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DiplomMSSQLApp.BLL.Services
{
    public class EmployeeService : BaseService<EmployeeDTO>
    {
        IUnitOfWork Database { get; set; }

        string MessageAboutFilterParametersUsed { get; set; }   // Сообщение записывается в файл WEB/Results/Employee/Filter.txt

        string ElapsedTime { get; set; }

        public string PathToFileForTests { get; set; }

        public EmployeeService(IUnitOfWork uow)
        {
            Database = uow;
        }

        public EmployeeService() { }

        // Добавление нового сотрудника (с валидацией)
        public override async Task CreateAsync(EmployeeDTO item)
        {
            await ValidationEmployeeAsync(item);
            Mapper.Initialize(cfg => cfg.CreateMap<EmployeeDTO, Employee>());
            Database.Employees.Create(Mapper.Map<EmployeeDTO, Employee>(item));
            await Database.SaveAsync();
        }

        // Удаление сотрудника
        public override async Task DeleteAsync(int id)
        {
            Employee item = await Database.Employees.FindByIdAsync(id);
            if (item == null) return;
            Database.Employees.Remove(item);
            await Database.SaveAsync();
        }

        // Удаление всех сотрудников
        public override async Task DeleteAllAsync()
        {
            await Database.Employees.RemoveAllAsync();
            await Database.SaveAsync();
        }

        public override void Dispose()
        {
            Database.Dispose();
        }

        // Обновление информации о сотруднике
        public override async Task EditAsync(EmployeeDTO item)
        {
            await ValidationEmployeeAsync(item);
            Mapper.Initialize(cfg => cfg.CreateMap<EmployeeDTO, Employee>());
            Database.Employees.Update(Mapper.Map<EmployeeDTO, Employee>(item));
            await Database.SaveAsync();
        }

        // Получение сотрудника по id
        public override async Task<EmployeeDTO> FindByIdAsync(int? id)
        {
            if (id == null)
                throw new ValidationException("Не установлено id сотрудника", "");
            Employee employee = await Database.Employees.FindByIdAsync(id.Value);
            if (employee == null)
                throw new ValidationException("Сотрудник не найден", "");
            Mapper.Initialize(cfg => {
                cfg.CreateMap<Employee, EmployeeDTO>();
                cfg.CreateMap<BusinessTrip, BusinessTripDTO>();
                cfg.CreateMap<Post, PostDTO>();
                cfg.CreateMap<Department, DepartmentDTO>();
            });
            return Mapper.Map<Employee, EmployeeDTO>(employee);
        }

        // Получение списка сотрудников по фильтру
        public override IEnumerable<EmployeeDTO> Get(EmployeeFilter filter)
        {
            Func<Employee, bool> predicate = CreatePredicate(filter);
            CreateMessageAboutFilterParametersUsed(filter);
            IEnumerable<Employee> sortedCollection = SortEmployees(filter, predicate);
            Mapper.Initialize(cfg => {
                cfg.CreateMap<BusinessTrip, BusinessTripDTO>()
                    .ForMember(bt => bt.Employees, opt => opt.Ignore());
                cfg.CreateMap<Employee, EmployeeDTO>();
                cfg.CreateMap<Department, DepartmentDTO>();
                cfg.CreateMap<Post, PostDTO>();
            });
            List<EmployeeDTO> col = Mapper.Map<IEnumerable<Employee>, List<EmployeeDTO>>(sortedCollection);
            return col;
        }

        private Func<Employee, bool> CreatePredicate(EmployeeFilter filter)
        {
            bool predicate(Employee employee)
            {
                bool returnValue = false;
                if (filter.IsAntiFilter)    // Если флаг установлен, то выбираются записи несоответствующие фильтру
                    returnValue = true;
                // Фильтр по фамилии
                if (filter.LastName != null)
                {
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
                                if (Regex.IsMatch(employee.LastName, lastName, RegexOptions.IgnoreCase))
                                {
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
                if (filter.Bonus != null)
                {
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

        private void CreateMessageAboutFilterParametersUsed(EmployeeFilter filter)
        {
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

        private IEnumerable<Employee> SortEmployees(EmployeeFilter filter, Func<Employee, bool> predicate)
        {
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

        private IEnumerable<Employee> SortByLastName(Func<Employee, bool> predicate, IComparer<string> stringComparer)
        {
            IEnumerable<Employee> sortedCollection = SortByLastNameWithTimeMeasurement(predicate, stringComparer);
            WriteMessageAndElapsedTimeInFile();
            return sortedCollection;
        }

        private IEnumerable<Employee> SortByLastNameWithTimeMeasurement(Func<Employee, bool> predicate, IComparer<string> stringComparer)
        {
            IEnumerable<Employee> sortedCollection;
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
                sortedCollection = Database.Employees.Get(predicate).OrderBy(e => e.LastName, stringComparer).ToList();
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            ElapsedTime = $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds:000}";
            return sortedCollection;
        }

        private void WriteMessageAndElapsedTimeInFile()
        {
            using (StreamWriter sw = new StreamWriter(PathToFileForTests, true, System.Text.Encoding.UTF8))
            {
                sw.WriteLine("Используемые параметры фильтра: " + MessageAboutFilterParametersUsed + "Время: " + ElapsedTime);
            }
        }

        // Получение списка всех сотрудников
        public override async Task<IEnumerable<EmployeeDTO>> GetAllAsync()
        {
            Mapper.Initialize(cfg => cfg.CreateMap<Employee, EmployeeDTO>()
                .ForMember(e => e.BusinessTrips, opt => opt.Ignore()));
            List<EmployeeDTO> col = Mapper.Map<IEnumerable<Employee>, List<EmployeeDTO>>(await Database.Employees.GetAsync());
            return col;
        }

        // Валидация модели
        private async Task ValidationEmployeeAsync(EmployeeDTO item)
        {
            if (item.LastName == null)
                throw new ValidationException("Требуется ввести фамилию", "LastName");
            if (item.FirstName == null)
                throw new ValidationException("Требуется ввести имя", "FirstName");
            if (item.Email != null)
            {
                try
                {
                    MailAddress m = new MailAddress(item.Email);
                }
                catch (FormatException)
                {
                    throw new ValidationException("Некорректный email", "Email");
                }
            }
            if (item.Salary != null && item.Post != null)
            {
                if (item.Salary < item.Post.MinSalary)
                    throw new ValidationException("Зарплата должна быть больше " + item.Post.MinSalary, "Salary");
                if (item.Salary > item.Post.MaxSalary)
                    throw new ValidationException("Зарплата должна быть меньше " + item.Post.MaxSalary, "Salary");
            }

            Department department = await Database.Departments.FindByIdAsync(item.DepartmentId ?? 0);
            if (department == null)
                throw new ValidationException("Отдел не найден", "");
            Post post = await Database.Posts.FindByIdAsync(item.PostId ?? 0);
            if (post == null)
                throw new ValidationException("Должность не найдена", "");
        }

        // Тест добавления сотрудников
        public override async Task TestCreateAsync(int num, string path)
        {
            Post firstPost = Database.Posts.GetFirst() ?? new Post();
            Department firstDepartment = Database.Departments.GetFirst() ?? new Department();
            List<Employee> emps = new List<Employee>();
            string[] lastNames = { "Smith", "Johnson", "Williams", "Jones", "Brown", "Davis", "Miller",
                "Wilson", "Moore", "Taylor", "Anderson", "Thomas", "Jackson", "White", "Harris",
                "Martin", "Thompson", "Wood", "Lewis", "Scott", "Cooper", "King", "Green", "Walker",
                "Edwards", "Turner", "Morgan", "Baker", "Hill", "Phillips" };
            Stopwatch stopWatch = new Stopwatch();
            for (int i = 0; i < num; i++)
            {
                Employee e = new Employee
                {
                    LastName = lastNames[i % lastNames.Length],
                    FirstName = "Daniel",
                    Email = "Smith@mail.ru",
                    HireDate = new DateTime(2015, 3, 14),
                    Salary = new double?(10000.0 + Math.Round(new Random(i).NextDouble() * 100) * 500),
                    PhoneNumber = "+7-935-728-35-07",
                    Bonus = 0.15,
                    Address = "Moskow, Kutuzovsky Avenue 57",
                    Post = firstPost,
                    PostId = firstPost.Id,
                    Department = firstDepartment,
                    DepartmentId = firstDepartment.Id
                };
                emps.Add(e);
            }
            Database.Employees.Create(emps);
            stopWatch.Start();
            await Database.SaveAsync();
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTime = $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds:000}";
            using (StreamWriter sw = new StreamWriter(path, true, System.Text.Encoding.UTF8))
            {
                sw.WriteLine("Количество сотрудников: " + emps.Count + "; Количество документов: " + num + "; Время: " + elapsedTime);
            }
        }

        // Тест выборки сотрудников
        public override async Task TestReadAsync(int num, string path, int salary)
        {
            for (int ii = 0; ii < 3; ii++)
            {
                using (StreamWriter sw = new StreamWriter(path, true, System.Text.Encoding.UTF8))
                {
                    sw.WriteLine("++-");
                }
                for (salary = 55000; salary <= 61000; salary += 1000)
                {
                    var e = await Database.Employees.GetAsync();
                    IEnumerable<Employee> result = null;
                    Stopwatch stopWatch = new Stopwatch();
                    stopWatch.Start();
                    for (int i = 0; i < num; i++)
                    {
                        result = Database.Employees.Get(salary);
                    }
                    stopWatch.Stop();
                    TimeSpan ts = stopWatch.Elapsed;
                    string elapsedTime = $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds:000}";
                    using (StreamWriter sw = new StreamWriter(path, true, System.Text.Encoding.UTF8))
                    {
                        sw.WriteLine("Общее количество сотрудников: " + e.Count() + "; Условие выборки: Salary >= " + salary + "; Индекс: нет; Количество найденных сотрудников: " + result.Count() + "; Количество выборок: " + num + "; Время: " + elapsedTime);
                    }
                }
            }
        }

        // Тест обновления сотрудников
        public override async Task TestUpdateAsync(int num, string path)
        {
            long matchedCount = 0;
            TimeSpan ts = new TimeSpan();
            Stopwatch stopWatch = new Stopwatch();
            //stopWatch.Start();
            for (int i = 0; i < num; i++)
            {
                Employee emp = Database.Employees.GetFirst();
                if (emp != null)
                {
                    emp.LastName = "Martin";
                    emp.FirstName = "Semen";
                    emp.Email = "Martin@mail.ru";
                    emp.HireDate = new DateTime(2015, 3, 14);
                    emp.Salary = 120000;
                    emp.PhoneNumber = "+7-944-569-85-36";
                    emp.Bonus = 0.2;
                    emp.Address = "Moskow Mira Avenue 105";

                    stopWatch.Start();
                    Database.Employees.Update(emp);
                    await Database.SaveAsync();
                    stopWatch.Stop();
                    ts = ts.Add(stopWatch.Elapsed);
                    ++matchedCount;
                }
            }
            //stopWatch.Stop();
            
            ts = new TimeSpan(ts.Ticks / num);

            //TimeSpan ts = stopWatch.Elapsed;
            string elapsedTime = $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds:000}";
            using (StreamWriter sw = new StreamWriter(path, true, System.Text.Encoding.UTF8))
            {
                sw.WriteLine("Количество замен: " + matchedCount + "; Количество документов: " + num + "; Время: " + elapsedTime);
            }
        }

        // Тест удаления сотрудников
        public override async Task TestDeleteAsync(int num, string path)
        {
            Stopwatch stopWatch = new Stopwatch();
            IEnumerable<Employee> emps = await Database.Employees.GetAsync();
            Database.Employees.RemoveSeries(emps.Take(num));
            stopWatch.Start();
            await Database.SaveAsync();
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTime = $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds:000}";
            using (StreamWriter sw = new StreamWriter(path, true, System.Text.Encoding.UTF8))
            {
                sw.WriteLine("Количество найденных записей: " + emps.Count() + "; Количество документов: " + num + "; Время: " + elapsedTime);
            }
        }
    }
}
