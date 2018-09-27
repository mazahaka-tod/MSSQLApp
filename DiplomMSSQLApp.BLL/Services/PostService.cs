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
using System.Text;
using System.Threading.Tasks;

namespace DiplomMSSQLApp.BLL.Services {
    public class PostService : BaseService<PostDTO> {
        public IUnitOfWork Database { get; set; }
        public string ElapsedTime { get; set; }
        public string PathToFileForTests { get; set; }

        public PostService(IUnitOfWork uow) {
            Database = uow;
        }

        public PostService() { }

        // Добавление новой должности
        public override async Task CreateAsync(PostDTO pDto) {
            Post post = MapDTOAndDomainModelWithValidation(pDto);
            Database.Posts.Create(post);
            await Database.SaveAsync();
        }

        private Post MapDTOAndDomainModelWithValidation(PostDTO pDto) {
            ValidationPost(pDto);
            Mapper.Initialize(cfg => {
                cfg.CreateMap<PostDTO, Post>();
                cfg.CreateMap<EmployeeDTO, Employee>()
                    .ForMember(e => e.BusinessTrips, opt => opt.Ignore())
                    .ForMember(e => e.Department, opt => opt.Ignore())
                    .ForMember(e => e.Post, opt => opt.Ignore());
            });
            Post post = Mapper.Map<PostDTO, Post>(pDto);
            return post;
        }

        private void ValidationPost(PostDTO pDto) {
            if (pDto.Title == null)
                throw new ValidationException("Требуется ввести название должности", "Title");
            if (pDto.MinSalary != null && pDto.MaxSalary != null && pDto.MinSalary > pDto.MaxSalary)
                throw new ValidationException("Минимальная зарплата не может быть больше максимальной", "MinSalary");
        }

        // Обновление информации о должности
        public override async Task EditAsync(PostDTO pDto) {
            Post post = MapDTOAndDomainModelWithValidation(pDto);
            Database.Posts.Update(post);
            await Database.SaveAsync();
        }

        // Получение должности по id
        public override async Task<PostDTO> FindByIdAsync(int? id) {
            if (id == null)
                throw new ValidationException("Не установлено id должности", "");
            Post post = await Database.Posts.FindByIdAsync(id.Value);
            if (post == null)
                throw new ValidationException("Должность не найдена", "");
            InitializeMapper();
            PostDTO pDto = Mapper.Map<Post, PostDTO>(post);
            return pDto;
        }

        private void InitializeMapper() {
            Mapper.Initialize(cfg => {
                cfg.CreateMap<Post, PostDTO>();
                cfg.CreateMap<Employee, EmployeeDTO>()
                    .ForMember(e => e.BusinessTrips, opt => opt.Ignore())
                    .ForMember(e => e.Department, opt => opt.Ignore())
                    .ForMember(e => e.Post, opt => opt.Ignore());
            });
        }

        // Получение списка всех должностей
        public override async Task<IEnumerable<PostDTO>> GetAllAsync() {
            IEnumerable<Post> posts = await Database.Posts.GetAsync();
            InitializeMapper();
            IEnumerable<PostDTO> collection = Mapper.Map<IEnumerable<Post>, IEnumerable<PostDTO>>(posts);
            return collection;
        }

        // Удаление должности
        public override async Task DeleteAsync(int id) {
            Post post = await Database.Posts.FindByIdAsync(id);
            if (post == null) return;
            Database.Posts.Remove(post);
            await Database.SaveAsync();
        }

        // Удаление всех должностей
        public override async Task DeleteAllAsync() {
            await Database.Posts.RemoveAllAsync();
            await Database.SaveAsync();
        }

        // Тест добавления должностей
        public override async Task TestCreateAsync(int num) {
            IEnumerable<Post> posts = CreatePostsCollectionForTest(num);
            await RunTestCreateAsync(posts);
            WriteResultTestCreateInFile(num);
        }

        private IEnumerable<Post> CreatePostsCollectionForTest(int num) {
            List<Post> posts = new List<Post>();
            string[] titles = { "Accountant", "Actuary", "Biologist", "Chemist", "Ecologist", "Economist", "Geophysicist", "Investigator", "Meteorologist", "Microbiologist",
                "Administrator", "Analyst", "Specialist", "Manager", "Statistician", "Team Leader", "Coordinator", "Writer", "Agent", "Outreach", "Educator", "Scientist",
                "Engineer", "Clerk", "Appraiser", "Intern", "Officer", "Cost Account", "Evaluator", "Technologist" };
            for (int i = 0; i < num; i++) {
                Post post = new Post {
                    Title = titles[i % titles.Length],
                    MinSalary = Convert.ToInt32(10000 + Math.Round(new Random(i).NextDouble() * 100) * 400),
                    MaxSalary = Convert.ToInt32(50000 + Math.Round(new Random(i).NextDouble() * 100) * 400)
                };
                posts.Add(post);
            }
            return posts;
        }

        private async Task RunTestCreateAsync(IEnumerable<Post> posts) {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            Database.Posts.Create(posts);
            await Database.SaveAsync();
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            ElapsedTime = $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds:000}";
        }

        private void WriteResultTestCreateInFile(int num) {
            StringBuilder sb = new StringBuilder();
            sb.Append("Количество добавленных должностей: ");
            sb.Append(num);
            sb.Append("; Время: ");
            sb.Append(ElapsedTime);
            using (StreamWriter sw = new StreamWriter(PathToFileForTests, true, Encoding.UTF8)) {
                sw.WriteLine(sb.ToString());
            }
        }

        // Тест выборки должностей
        public override async Task TestReadAsync(int num, int salary) {
            int postsCount = (await Database.Posts.GetAsync()).Count();
            int resultCount = Database.Posts.Get(salary).Count();
            RunTestRead(num, salary);
            WriteResultTestReadInFile(postsCount, resultCount, num, salary);
        }

        private void RunTestRead(int num, int salary) {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            for (int i = 0; i < num; i++) {
                Database.Posts.Get(salary);
            }
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            ElapsedTime = $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds:000}";
        }

        private void WriteResultTestReadInFile(int postsCount, int resultCount, int num, int salary) {
            StringBuilder sb = new StringBuilder();
            sb.Append("Общее количество должностей: ");
            sb.Append(postsCount);
            sb.Append("; Количество найденных должностей: ");
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

        // Тест обновления должностей
        public override async Task TestUpdateAsync(int num) {
            Post[] posts = (await Database.Posts.GetAsync()).Take(num).ToArray();
            int postsLength = posts.Length;
            await RunTestUpdateAsync(posts);
            WriteResultTestUpdateInFile(postsLength);
        }

        private async Task RunTestUpdateAsync(Post[] posts) {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            for (int i = 0; i < posts.Length; i++) {
                posts[i].Title = "Programmer";
                posts[i].MinSalary = 50000;
                posts[i].MaxSalary = 190000;
                Database.Posts.Update(posts[i]);
                await Database.SaveAsync();
            }
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            ElapsedTime = $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds:000}";
        }

        private void WriteResultTestUpdateInFile(int postsLength) {
            StringBuilder sb = new StringBuilder();
            sb.Append("Количество обновленных должностей: ");
            sb.Append(postsLength);
            sb.Append("; Время: ");
            sb.Append(ElapsedTime);
            using (StreamWriter sw = new StreamWriter(PathToFileForTests, true, Encoding.UTF8)) {
                sw.WriteLine(sb.ToString());
            }
        }

        // Тест удаления должностей
        public override async Task TestDeleteAsync(int num) {
            IEnumerable<Post> posts = (await Database.Posts.GetAsync()).Take(num);
            int postsCount = posts.Count();
            await RunTestDeleteAsync(posts);
            WriteResultTestDeleteInFile(postsCount);
        }

        private async Task RunTestDeleteAsync(IEnumerable<Post> posts) {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            Database.Posts.RemoveSeries(posts);
            await Database.SaveAsync();
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            ElapsedTime = $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds:000}";
        }

        private void WriteResultTestDeleteInFile(int postsCount) {
            StringBuilder sb = new StringBuilder();
            sb.Append("Количество удаленных должностей: ");
            sb.Append(postsCount);
            sb.Append("; Время: ");
            sb.Append(ElapsedTime);
            using (StreamWriter sw = new StreamWriter(PathToFileForTests, true, Encoding.UTF8)) {
                sw.WriteLine(sb.ToString());
            }
        }

        public override void Dispose() {
            Database.Dispose();
        }

        // Нереализованные методы
        public override IEnumerable<PostDTO> Get(EmployeeFilter f) {
            throw new NotImplementedException();
        }
    }
}
