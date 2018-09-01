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

        public EmployeeService(IUnitOfWork uow)
        {
            Database = uow;
        }

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
            Employee e = await Database.Employees.FindByIdAsync(id.Value);
            if (e == null)
                throw new ValidationException("Сотрудник не найден", "");
            Mapper.Initialize(cfg => {
                cfg.CreateMap<Employee, EmployeeDTO>();
                cfg.CreateMap<BusinessTrip, BusinessTripDTO>();
                cfg.CreateMap<Post, PostDTO>();
                cfg.CreateMap<Department, DepartmentDTO>();
            });
            return Mapper.Map<Employee, EmployeeDTO>(e);
        }

        // Получение списка сотрудников по фильтру
        public override IEnumerable<EmployeeDTO> Get(EmployeeFilter f, string path)
        {
            bool bl = true;
            string message = "";
            bool predicate(Employee e)
            {
                bool flag = true;
                // Фильтр по фамилии
                if (f.LastName != null && !string.IsNullOrEmpty(f.LastName[0]))
                {
                    List<bool> list = new List<bool>();
                    foreach (var item in f.LastName)
                    {
                        if (!string.IsNullOrWhiteSpace(item))
                        {
                            if (bl) message += "Фамилия = " + item + "; ";
                            if (Regex.IsMatch(e.LastName, item, RegexOptions.IgnoreCase))
                                list.Add(true);
                            else
                                list.Add(false);
                        }
                    }
                    if (list.Count != 0)
                        if (f.Or)
                            flag = list.Contains(true) ? true : false;
                        else
                            flag = list.Contains(false) ? false : true;
                }
                // Фильтр по email
                if (!string.IsNullOrWhiteSpace(f.Email))
                {
                    if (bl) message += "Email = " + f.Email + "; ";
                    flag = Regex.IsMatch(e.Email, f.Email, RegexOptions.IgnoreCase) ? true : false;
                }
                // Фильтр по номеру телефона
                if (f.PhoneNumber)
                {
                    if (bl) message += "Есть телефон; ";
                    flag = !string.IsNullOrWhiteSpace(e.PhoneNumber) ? true : false;
                }
                // Фильтр по дате найма на работу
                if (!string.IsNullOrWhiteSpace(f.HireDate))
                {
                    if (bl) message += "Дата приема на работу = " + f.HireDate + "; ";
                    if (DateTime.TryParse(f.HireDate, out DateTime date))
                    {
                        flag = e.HireDate == date ? true : false;
                        //filters.Add(builder.Eq("HireDate", new DateTime(date.Year, date.Month, date.Day)));
                    }
                }
                // Фильтры по зарплате
                if (f.MinSalary.HasValue)
                {
                    //if (bl)
                    message += "Зарплата >= " + f.MinSalary + "; ";
                    flag = e.Salary >= f.MinSalary ? true : false;
                }
                if (f.MaxSalary.HasValue)
                {
                    if (bl) message += "Зарплата <= " + f.MaxSalary + "; ";
                    flag = e.Salary <= f.MaxSalary ? true : false;
                }
                // Фильтр по премии
                if (f.Bonus != null && f.Bonus.Where(b => b != null).Count() > 0)
                {
                    if (bl) message += "Премия = " + f.Bonus + "; ";
                    flag = f.Bonus.Where(c => c != null).Contains(e.Bonus) ? true : false;
                }
                if (f.BonusExists)
                {
                    if (bl) message += "Есть премия; ";
                    flag = e.Bonus.HasValue ? true : false;
                }
                // Фильтр по должности
                if (!string.IsNullOrWhiteSpace(f.Post))
                {
                    if (bl) message += "Должность = " + f.Post + "; ";
                    flag = Regex.IsMatch(e.Post.Title, f.Post, RegexOptions.IgnoreCase) ? true : false;
                }
                // Фильтр по названию отдела
                if (!string.IsNullOrWhiteSpace(f.DepartmentName))
                {
                    if (bl) message += "Название отдела = " + f.DepartmentName + "; ";
                    flag = Regex.IsMatch(e.Department.DepartmentName, f.DepartmentName, RegexOptions.IgnoreCase) ? true : false;
                }
                if (f.Not)
                {
                    if (bl) message += "Используется отрицание; ";
                    flag = !flag;
                }
                if (bl) bl = false;
                return flag;
            }
            // Параметры для сортировки
            string sort = f.Sort ?? "LastName";
            string asc = f.Asc ?? "1";
            IEnumerable<Employee> col;
            if (asc.Equals("1"))
                switch (sort)
                {
                    case "LastName":
                        col = Database.Employees.Get(predicate).OrderBy(e => e.LastName);
                        break;
                    case "SurName":
                        Stopwatch stopWatch = new Stopwatch();
                        stopWatch.Start();
                        col = Database.Employees.Get(predicate);
                        stopWatch.Stop();
                        TimeSpan ts = stopWatch.Elapsed;
                        string elapsedTime = $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds:000}";
                        using (StreamWriter sw = new StreamWriter(path, true, System.Text.Encoding.UTF8))
                        {
                            sw.WriteLine(message.Length.ToString() + " - " + message + "Время: " + elapsedTime);
                        }
                        break;
                    case "Email":
                        col = Database.Employees.Get(predicate).OrderBy(e => e.Email);
                        break;
                    case "HireDate":
                        col = Database.Employees.Get(predicate).OrderBy(e => e.HireDate);
                        break;
                    case "Salary":
                        col = Database.Employees.Get(predicate).OrderBy(e => e.Salary);
                        break;
                    case "Bonus":
                        col = Database.Employees.Get(predicate).OrderBy(e => e.Bonus);
                        break;
                    case "Post":
                        col = Database.Employees.Get(predicate).OrderBy(e => e.Post.Title);
                        break;
                    default:
                        col = Database.Employees.Get(predicate).OrderBy(e => e.Department.DepartmentName);
                        break;
                }
            else
                switch (sort)
                {
                    case "LastName":
                        col = Database.Employees.Get(predicate).OrderByDescending(e => e.LastName);
                        break;
                    case "Email":
                        col = Database.Employees.Get(predicate).OrderByDescending(e => e.Email);
                        break;
                    case "HireDate":
                        col = Database.Employees.Get(predicate).OrderByDescending(e => e.HireDate);
                        break;
                    case "Salary":
                        col = Database.Employees.Get(predicate).OrderByDescending(e => e.Salary);
                        break;
                    case "Bonus":
                        col = Database.Employees.Get(predicate).OrderByDescending(e => e.Bonus);
                        break;
                    case "Post":
                        col = Database.Employees.Get(predicate).OrderByDescending(e => e.Post.Title);
                        break;
                    default:
                        col = Database.Employees.Get(predicate).OrderByDescending(e => e.Department.DepartmentName);
                        break;
                }
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<BusinessTrip, BusinessTripDTO>()
                    .ForMember(bt => bt.Employees, opt => opt.Ignore());
                cfg.CreateMap<Employee, EmployeeDTO>();
                cfg.CreateMap<Department, DepartmentDTO>();
                cfg.CreateMap<Post, PostDTO>();
            });
            var collection = Mapper.Map<IEnumerable<Employee>, List<EmployeeDTO>>(col);
            return collection;
        }

        // Получение списка всех сотрудников
        public override async Task<IEnumerable<EmployeeDTO>> GetAllAsync()
        {
            Mapper.Initialize(cfg => cfg.CreateMap<Employee, EmployeeDTO>()
                .ForMember(e => e.BusinessTrips, opt => opt.Ignore()));
            return Mapper.Map<IEnumerable<Employee>, List<EmployeeDTO>>(await Database.Employees.GetAsync());
        }

        // Обновление информации о сотрудниках (при редактировании информации об отделе)
        public async Task UpdateEmployeesAsync(string old, DepartmentDTO dDto)
        {
            // Если при обновлении изменился начальник отдела, то нужно внести изменения в таблицу сотрудников:
            // 1. У сотрудника, который был начальником, установить должность в null
            // 2. У сотрудника, который стал начальником, обновить должность и отдел
            if (old != dDto.Manager)
            {
                // 1.
                Employee oldManager = Database.Employees.Get(e => e.LastName == old).FirstOrDefault();
                if (oldManager != null)
                {
                    oldManager.PostId = null;
                    Database.Employees.Update(oldManager);
                    await Database.SaveAsync();
                }
                // 2.
                Post postManager = Database.Posts.Get(p => p.Title == "Manager").FirstOrDefault();
                Mapper.Initialize(cfg => cfg.CreateMap<DepartmentDTO, Department>());
                Department d = Mapper.Map<DepartmentDTO, Department>(dDto);
                Employee newManager = Database.Employees.Get(e => e.LastName == dDto.Manager).FirstOrDefault();
                newManager.PostId = postManager.Id;
                newManager.DepartmentId = d.Id;
                Database.Employees.Update(newManager);
                await Database.SaveAsync();
            }
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
            if (item.HireDate == null)
                throw new ValidationException("Требуется ввести дату приема на работу", "HireDate");

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
                    Address = "Moskow, Kutuzovsky Avenue 57"
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
