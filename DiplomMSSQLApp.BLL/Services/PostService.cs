using AutoMapper;
using DiplomMSSQLApp.BLL.DTO;
using DiplomMSSQLApp.BLL.Infrastructure;
using DiplomMSSQLApp.DAL.Entities;
using DiplomMSSQLApp.DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace DiplomMSSQLApp.BLL.Services
{
    public class PostService : BaseService<PostDTO>
    {
        IUnitOfWork Database { get; set; }

        public PostService(IUnitOfWork uow)
        {
            Database = uow;
        }

        // Добавление новой должности (с валидацией)
        public override void Create(PostDTO item)
        {
            ValidationPost(item);
            Mapper.Initialize(cfg => cfg.CreateMap<PostDTO, Post>());
            Database.Posts.Create(Mapper.Map<PostDTO, Post>(item));
            Database.Save();
        }

        // Получение списка всех должностей
        public override IEnumerable<PostDTO> GetAll()
        {
            Mapper.Initialize(cfg => cfg.CreateMap<Post, PostDTO>()
                .ForMember(p => p.Employees, opt => opt.Ignore())
            );
            return Mapper.Map<IEnumerable<Post>, List<PostDTO>>(Database.Posts.Get());
        }

        // Удаление должности
        public override void Delete(int id)
        {
            Post item = Database.Posts.FindById(id);
            if (item == null) return;
            Database.Posts.Remove(item);
            Database.Save();
        }

        // Удаление всех должностей
        public override void DeleteAll()
        {
            Database.Posts.RemoveAll();
            Database.Save();
        }

        // Обновление информации о должности
        public override void Edit(PostDTO item)
        {
            ValidationPost(item);
            Mapper.Initialize(cfg => cfg.CreateMap<PostDTO, Post>());
            Database.Posts.Update(Mapper.Map<PostDTO, Post>(item));
            Database.Save();
        }

        public override void Dispose()
        {
            Database.Dispose();
        }

        // Получение должности по id
        public override PostDTO FindById(int? id)
        {
            if (id == null)
                throw new ValidationException("Не установлено id должности", "");
            Post p = Database.Posts.FindById(id.Value);
            if (p == null)
                throw new ValidationException("Должность не найден", "");
            Mapper.Initialize(cfg => {
                cfg.CreateMap<Post, PostDTO>();
                cfg.CreateMap<Employee, EmployeeDTO>()
                    .ForMember(dp => dp.BusinessTrips, opt => opt.Ignore())
                    .ForMember(dp => dp.Department, opt => opt.Ignore())
                    .ForMember(dp => dp.Post, opt => opt.Ignore());
            });
            return Mapper.Map<Post, PostDTO>(p);
        }

        // Валидация модели
        private void ValidationPost(PostDTO item)
        {
            if (item.Title == null)
                throw new ValidationException("Требуется ввести название должности", "Title");
            if (item.MinSalary > item.MaxSalary)
                throw new ValidationException("Минимальная зарплата не может быть больше максимальной", "MinSalary");
        }

        // Тест добавления должностей
        public override void TestCreate(int num, string path)
        {
            List<Post> posts = new List<Post>();
            string[] titles = { "Accountant", "Actuary", "Biologist", "Chemist", "Ecologist", "Economist",
                "Geophysicist", "Investigator", "Meteorologist", "Microbiologist", "Administrator", "Analyst",
                "Specialist", "Manager", "Statistician", "Team Leader", "Coordinator", "Writer", "Agent",
                "Outreach", "Educator", "Scientist", "Engineer", "Clerk", "Appraiser", "Intern", "Officer",
                "Cost Account", "Evaluator", "Technologist" };
            Stopwatch stopWatch = new Stopwatch();
            for (int i = 0; i < num; i++)
            {
                Post p = new Post
                {
                    Title = titles[i % titles.Length],
                    MinSalary = Convert.ToInt32(10000 + Math.Round(new Random(i).NextDouble() * 100) * 400),
                    MaxSalary = Convert.ToInt32(50000 + Math.Round(new Random(i).NextDouble() * 100) * 400)
                };
                posts.Add(p);
            }
            Database.Posts.Create(posts);
            stopWatch.Start();
            Database.Save();
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTime = $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds:000}";
            using (StreamWriter sw = new StreamWriter(path, true, System.Text.Encoding.UTF8))
            {
                sw.WriteLine("Количество должностей: " + num + "; Время: " + elapsedTime);
            }
        }

        // Тест выборки должностей
        public override void TestRead(int num, string path, int val)
        {
            var cnt = Database.Posts.Get().Count();
            IEnumerable<Post> result = null;
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            for (int i = 0; i < num; i++)
            {
                result = Database.Posts.Get(val);
            }
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTime = $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds:000}";
            using (StreamWriter sw = new StreamWriter(path, true, System.Text.Encoding.UTF8))
            {
                sw.WriteLine("Общее количество должностей: " + cnt + "; Условие выборки: MaxSalary == 60000; Индекс: нет; Количество найденных должностей: " + result.Count() + "; Количество выборок: " + num + "; Время: " + elapsedTime);
            }
        }

        // Тест обновления должностей
        public override void TestUpdate(int num, string path)
        {
            long matchedCount = 0;
            TimeSpan ts = new TimeSpan();
            Stopwatch stopWatch = new Stopwatch();
            //stopWatch.Start();
            for (int i = 0; i < num; i++)
            {
                Post post = Database.Posts.GetFirst();
                if (post != null)
                {
                    post.Title = "Programmer";
                    post.MinSalary = 10000;
                    post.MaxSalary = 90000;

                    stopWatch.Start();
                    Database.Posts.Update(post);
                    Database.Save();
                    stopWatch.Stop();
                    ts = ts.Add(stopWatch.Elapsed);
                    ++matchedCount;
                }
            }
            //stopWatch.Stop();
            //TimeSpan ts = stopWatch.Elapsed;
            ts = new TimeSpan(ts.Ticks / num);
            string elapsedTime = $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds:000}";
            using (StreamWriter sw = new StreamWriter(path, true, System.Text.Encoding.UTF8))
            {
                sw.WriteLine("Количество замен: " + matchedCount + "; Количество должностей: " + num + "; Время: " + elapsedTime);
            }
        }

        // Тест удаления должностей
        public override void TestDelete(int num, string path)
        {
            Stopwatch stopWatch = new Stopwatch();
            IEnumerable<Post> posts = Database.Posts.Get(false);
            Database.Posts.RemoveSeries(posts.Take(num));
            stopWatch.Start();
            Database.Save();
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTime = $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds:000}";
            using (StreamWriter sw = new StreamWriter(path, true, System.Text.Encoding.UTF8))
            {
                sw.WriteLine("Количество найденных записей: " + posts.Count() + "; Количество должностей: " + num + "; Время: " + elapsedTime);
            }
        }
    }
}
