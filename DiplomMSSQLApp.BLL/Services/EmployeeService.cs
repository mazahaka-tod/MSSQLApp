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
        public override void Create(EmployeeDTO item)
        {
            ValidationEmployee(item);
            Mapper.Initialize(cfg => cfg.CreateMap<EmployeeDTO, Employee>());
            Database.Employees.Create(Mapper.Map<EmployeeDTO, Employee>(item));
            Database.Save();
        }

        // Получение списка всех сотрудников
        public override IEnumerable<EmployeeDTO> GetAll()
        {
            Mapper.Initialize(cfg => cfg.CreateMap<Employee, EmployeeDTO>()
                .ForMember(e => e.BusinessTrips, opt => opt.Ignore())
            );
            return Mapper.Map<IEnumerable<Employee>, List<EmployeeDTO>>(Database.Employees.Get());
        }

        // Получение списка сотрудников по фильтру
        public override IEnumerable<EmployeeDTO> Get(EmployeeFilter f, string path, ref int cnt)
        {
            bool bl = true;
            string message = "";
            Func<Employee, bool> predicate = e => {
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
                    DateTime date;
                    if (DateTime.TryParse(f.HireDate, out date))
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
            };
            // Параметры для сортировки
            string sort = f.Sort == null ? "LastName" : f.Sort;
            string asc = f.Asc == null ? "1" : f.Asc;
            IEnumerable<Employee> col;
            if (asc.Equals("1"))
                switch (sort)
                {
                    case "LastName":
                        col = Database.Employees.Get(predicate).OrderBy(e => e.LastName);
                        cnt = col.Count();
                        //col = col.Take(8);
                        break;
                    case "SurName":
                        Stopwatch stopWatch = new Stopwatch();
                        stopWatch.Start();
                        col = Database.Employees.Get(predicate);
                        cnt = col.Count();
                        stopWatch.Stop();
                        TimeSpan ts = stopWatch.Elapsed;
                        string elapsedTime = string.Format("{0:00}:{1:00}:{2:00}.{3:000}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds);
                        using (StreamWriter sw = new StreamWriter(path, true, System.Text.Encoding.Default))
                        {
                            sw.WriteLine(message.Length.ToString() + " - " + message + "Время: " + elapsedTime);
                        }
                        break;
                    case "Email":
                        col = Database.Employees.Get(predicate).OrderBy(e => e.Email);
                        cnt = col.Count();
                        break;
                    case "HireDate":
                        col = Database.Employees.Get(predicate).OrderBy(e => e.HireDate);
                        cnt = col.Count();
                        break;
                    case "Salary":
                        col = Database.Employees.Get(predicate).OrderBy(e => e.Salary);
                        cnt = col.Count();
                        break;
                    case "Bonus":
                        col = Database.Employees.Get(predicate).OrderBy(e => e.Bonus);
                        cnt = col.Count();
                        break;
                    case "Post":
                        col = Database.Employees.Get(predicate).OrderBy(e => e.Post.Title);
                        cnt = col.Count();
                        break;
                    default:
                        col = Database.Employees.Get(predicate).OrderBy(e => e.Department.DepartmentName);
                        cnt = col.Count();
                        break;
                }
            else
                switch (sort)
                {
                    case "LastName":
                        col = Database.Employees.Get(predicate).OrderByDescending(e => e.LastName);
                        cnt = col.Count();
                        break;
                    case "Email":
                        col = Database.Employees.Get(predicate).OrderByDescending(e => e.Email);
                        cnt = col.Count();
                        break;
                    case "HireDate":
                        col = Database.Employees.Get(predicate).OrderByDescending(e => e.HireDate);
                        cnt = col.Count();
                        break;
                    case "Salary":
                        col = Database.Employees.Get(predicate).OrderByDescending(e => e.Salary);
                        cnt = col.Count();
                        break;
                    case "Bonus":
                        col = Database.Employees.Get(predicate).OrderByDescending(e => e.Bonus);
                        cnt = col.Count();
                        break;
                    case "Post":
                        col = Database.Employees.Get(predicate).OrderByDescending(e => e.Post.Title);
                        cnt = col.Count();
                        break;
                    default:
                        col = Database.Employees.Get(predicate).OrderByDescending(e => e.Department.DepartmentName);
                        cnt = col.Count();
                        break;
                }
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<BusinessTrip, BusinessTripDTO>()
                    .ForMember(bt => bt.Employees, opt => opt.Ignore());
                cfg.CreateMap<Employee, EmployeeDTO>()
                    //.ForMember(e => e.Department, opt => opt.Ignore())
                    //.ForMember(e => e.Post, opt => opt.Ignore())
                    ;
                cfg.CreateMap<Department, DepartmentDTO>();
                cfg.CreateMap<Post, PostDTO>();
            });
            var collection = Mapper.Map<IEnumerable<Employee>, List<EmployeeDTO>>(col);
            return collection;
        }

        // Удаление сотрудника
        public override void Delete(int id)
        {
            Employee item = Database.Employees.FindById(id);
            if (item == null) return;
            Database.Employees.Remove(item);
            Database.Save();
        }

        // Удаление всех сотрудников
        public override void DeleteAll()
        {
            Database.Employees.RemoveAll();
            Database.Save();
        }

        // Обновление информации о сотруднике
        public override void Edit(EmployeeDTO item)
        {
            ValidationEmployee(item);
            Mapper.Initialize(cfg => cfg.CreateMap<EmployeeDTO, Employee>());
            Database.Employees.Update(Mapper.Map<EmployeeDTO, Employee>(item));
            Database.Save();
        }

        public override void Dispose()
        {
            Database.Dispose();
        }

        // Получение сотрудника по id
        public override EmployeeDTO FindById(int? id)
        {
            if (id == null)
                throw new ValidationException("Не установлено id сотрудника", "");
            Employee e = Database.Employees.FindById(id.Value);
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

        // Обновление информации о сотрудниках (при редактировании информации об отделе)
        public void UpdateEmployees(string old, DepartmentDTO dDto)
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
                    Database.Save();
                }
                // 2.
                Post postManager = Database.Posts.Get(p => p.Title == "Manager").FirstOrDefault();
                Mapper.Initialize(cfg => cfg.CreateMap<DepartmentDTO, Department>());
                Department d = Mapper.Map<DepartmentDTO, Department>(dDto);
                Employee newManager = Database.Employees.Get(e => e.LastName == dDto.Manager).FirstOrDefault();
                newManager.PostId = postManager.Id;
                newManager.DepartmentId = d.Id;
                Database.Employees.Update(newManager);
                Database.Save();
            }
        }

        // Валидация модели
        private void ValidationEmployee(EmployeeDTO item)
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

            Department department = Database.Departments.FindById(item.DepartmentId.HasValue ? item.DepartmentId.Value : 0);
            if (department == null)
                throw new ValidationException("Отдел не найден", "");
            Post post = Database.Posts.FindById(item.PostId.HasValue ? item.PostId.Value : 0);
            if (post == null)
                throw new ValidationException("Должность не найдена", "");
        }

        // Тест добавления сотрудников
        public override void TestCreate(int num, string path)
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
            Database.Save();
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTime = string.Format("{0:00}:{1:00}:{2:00}.{3:000}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds);
            using (StreamWriter sw = new StreamWriter(path, true, System.Text.Encoding.Default))
            {
                sw.WriteLine("Количество сотрудников: " + emps.Count + "; Количество документов: " + num + "; Время: " + elapsedTime);
            }
        }

        // Тест выборки сотрудников
        public override void TestRead(int num, string path, int val)
        {
            for (int ii = 0; ii < 3; ii++)
            {
                using (StreamWriter sw = new StreamWriter(path, true, System.Text.Encoding.Default))
                {
                    sw.WriteLine("++-");
                }
                for (val = 55000; val <= 61000; val += 1000)
                {
                    var cnt = Database.Employees.Get().Count();
                    IEnumerable<Employee> result = null;
                    Stopwatch stopWatch = new Stopwatch();
                    stopWatch.Start();
                    for (int i = 0; i < num; i++)
                    {
                        result = Database.Employees.Get(val);
                    }
                    stopWatch.Stop();
                    TimeSpan ts = stopWatch.Elapsed;
                    string elapsedTime = string.Format("{0:00}:{1:00}:{2:00}.{3:000}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds);
                    using (StreamWriter sw = new StreamWriter(path, true, System.Text.Encoding.Default))
                    {
                        sw.WriteLine("Общее количество сотрудников: " + cnt + "; Условие выборки: Salary >= " + val + "; Индекс: нет; Количество найденных сотрудников: " + result.Count() + "; Количество выборок: " + num + "; Время: " + elapsedTime);
                    }
                }
            }
        }

        // Тест обновления сотрудников
        public override void TestUpdate(int num, string path)
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
                    Database.Save();
                    stopWatch.Stop();
                    ts = ts.Add(stopWatch.Elapsed);
                    ++matchedCount;
                }
            }
            //stopWatch.Stop();
            
            ts = new TimeSpan(ts.Ticks / num);

            //TimeSpan ts = stopWatch.Elapsed;
            string elapsedTime = string.Format("{0:00}:{1:00}:{2:00}.{3:000}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds);
            using (StreamWriter sw = new StreamWriter(path, true, System.Text.Encoding.Default))
            {
                sw.WriteLine("Количество замен: " + matchedCount + "; Количество документов: " + num + "; Время: " + elapsedTime);
            }
        }

        // Тест удаления сотрудников
        public override void TestDelete(int num, string path)
        {
            Stopwatch stopWatch = new Stopwatch();
            IEnumerable<Employee> emps = Database.Employees.Get(false);
            Database.Employees.RemoveSeries(emps.Take(num));
            stopWatch.Start();
            Database.Save();
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTime = string.Format("{0:00}:{1:00}:{2:00}.{3:000}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds);
            using (StreamWriter sw = new StreamWriter(path, true, System.Text.Encoding.Default))
            {
                sw.WriteLine("Количество найденных записей: " + emps.Count() + "; Количество документов: " + num + "; Время: " + elapsedTime);
            }
        }
    }
}
