using AutoMapper;
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
using System.Threading.Tasks;
using System.Web.Script.Serialization;

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
            Post post = await MapDTOAndDomainModelWithValidationAsync(pDto);
            Database.Posts.Create(post);
            await Database.SaveAsync();
        }

        private async Task<Post> MapDTOAndDomainModelWithValidationAsync(PostDTO pDto) {
            await ValidationPostAsync(pDto);
            Mapper.Initialize(cfg => {
                cfg.CreateMap<PostDTO, Post>();
                cfg.CreateMap<DepartmentDTO, Department>()
                    .ForMember(d => d.Organization, opt => opt.Ignore())
                    .ForMember(d => d.Posts, opt => opt.Ignore());
                cfg.CreateMap<EmployeeDTO, Employee>()
                    .ForMember(e => e.Birth, opt => opt.Ignore())
                    .ForMember(e => e.BusinessTrips, opt => opt.Ignore())
                    .ForMember(e => e.Contacts, opt => opt.Ignore())
                    .ForMember(e => e.Education, opt => opt.Ignore())
                    .ForMember(e => e.Passport, opt => opt.Ignore())
                    .ForMember(e => e.Post, opt => opt.Ignore());
            });
            Post post = Mapper.Map<PostDTO, Post>(pDto);
            return post;
        }

        private async Task ValidationPostAsync(PostDTO pDto) {
            if (pDto.Title == null)
                throw new ValidationException("Требуется ввести название должности", "Title");
            if (pDto.NumberOfUnits == null)
                throw new ValidationException("Требуется ввести количество штатных единиц", "NumberOfUnits");
            if (pDto.NumberOfUnits < 0 || pDto.NumberOfUnits > 10000)
                throw new ValidationException("Значение должно быть в диапазоне [0, 10000]", "NumberOfUnits");
            if (pDto.Salary == null)
                throw new ValidationException("Требуется ввести оклад", "Salary");
            if (pDto.Salary < 0 || pDto.Salary > 1000000)
                throw new ValidationException("Оклад должен быть в диапазоне [0, 1000000]", "Salary");
            if (pDto.Premium == null)
                throw new ValidationException("Требуется ввести надбавку", "Premium");
            if (pDto.Premium < 0 || pDto.Premium > 100000)
                throw new ValidationException("Надбавка должна быть в диапазоне [0, 100000]", "Premium");
            int employeesCount = await Database.Employees.CountAsync(e => e.PostId == pDto.Id);
            if (pDto.NumberOfUnits < employeesCount)
                throw new ValidationException("Количество штатных единиц не может быть меньше, " +
                    "чем количество сотрудников, работающих в должности в данный момент [" + employeesCount + "]", "NumberOfUnits");
            int postsCount = await Database.Posts.CountAsync(p => p.Title == pDto.Title && p.DepartmentId == pDto.DepartmentId && p.Id != pDto.Id);
            if (postsCount > 0)
                throw new ValidationException("Должность уже существует", "Title");
            postsCount = await Database.Posts.CountAsync(p => p.Id == pDto.Id && p.Title != pDto.Title);
            if (employeesCount > 0 && postsCount > 0)
                throw new ValidationException("Нельзя изменить название должности, пока в данной должности работает хотя бы один сотрудник", "Title");
            postsCount = await Database.Posts.CountAsync(p => p.Id == pDto.Id && p.DepartmentId != pDto.DepartmentId);
            if (employeesCount > 0 && postsCount > 0)
                throw new ValidationException("Нельзя изменить название отдела, пока в данной должности работает хотя бы один сотрудник", "DepartmentId");
        }

        // Обновление информации о должности
        public override async Task EditAsync(PostDTO pDto) {
            Post post = await MapDTOAndDomainModelWithValidationAsync(pDto);
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
                cfg.CreateMap<Department, DepartmentDTO>()
                    .ForMember(d => d.Manager, opt => opt.Ignore())
                    .ForMember(d => d.Organization, opt => opt.Ignore())
                    .ForMember(d => d.Posts, opt => opt.Ignore());
                cfg.CreateMap<Employee, EmployeeDTO>()
                    .ForMember(e => e.Birth, opt => opt.Ignore())
                    .ForMember(e => e.BusinessTrips, opt => opt.Ignore())
                    .ForMember(e => e.Contacts, opt => opt.Ignore())
                    .ForMember(e => e.Education, opt => opt.Ignore())
                    .ForMember(e => e.Passport, opt => opt.Ignore())
                    .ForMember(e => e.Post, opt => opt.Ignore());
            });
        }

        // Получение списка всех должностей
        public override async Task<IEnumerable<PostDTO>> GetAllAsync() {
            IEnumerable<Post> posts = await Database.Posts.GetAllAsync();
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

        // Запись информации о должностях в JSON-файл
        public override async Task ExportJsonAsync(string fullPath) {
            IEnumerable<Post> posts = await Database.Posts.GetAllAsync();
            var transformPosts = posts.Select(p => new {
                p.DepartmentId,
                p.NumberOfUnits,
                p.Premium,
                p.Salary,
                p.Title
            }).ToArray();
            using (StreamWriter sw = new StreamWriter(fullPath, true, Encoding.UTF8)) {
                sw.WriteLine("{\"Posts\":");
                sw.WriteLine(new JavaScriptSerializer().Serialize(transformPosts));
                sw.WriteLine("}");
            }
        }

        // Количество должностей
        public override async Task<int> CountAsync() {
            return await Database.Posts.CountAsync();
        }

        // Количество должностей, удовлетворяющих предикату
        public override async Task<int> CountAsync(Expression<Func<PostDTO, bool>> predicateDTO) {
            Mapper.Initialize(cfg => cfg.CreateMap<PostDTO, Post>());
            var predicate = Mapper.Map<Expression<Func<PostDTO, bool>>, Expression<Func<Post, bool>>>(predicateDTO);
            return await Database.Posts.CountAsync(predicate);
        }

        // Добавление должностей для тестирования
        public async Task TestCreateAsync() {
            string[] titles = { "Accountant", "Actuary", "Biologist", "Chemist", "Ecologist", "Economist", "Geophysicist", "Investigator", "Meteorologist", "Microbiologist",
                "Administrator", "Analyst", "Specialist", "Manager", "Statistician", "Team Leader", "Coordinator", "Writer", "Agent", "Outreach", "Educator", "Scientist",
                "Engineer", "Clerk", "Appraiser", "Intern", "Officer", "Cost Account", "Evaluator", "Technologist" };
            int[] departmentIds = (await Database.Departments.GetAllAsync()).Select(d => d.Id).ToArray();
            if (departmentIds.Length == 0)
                throw new ValidationException("Сначала нужно добавить хотя бы один отдел", "");
            for (int i = 0; i < 100; i++) {
                Post post = new Post {
                    Title = titles[i % titles.Length],
                    NumberOfUnits = Convert.ToInt32(1 + Math.Round(new Random(i).NextDouble() * 100)),
                    Salary = Convert.ToInt32(50000 + Math.Round(new Random(i).NextDouble() * 100) * 400),
                    Premium = Convert.ToInt32(10000 + Math.Round(new Random(i).NextDouble() * 100) * 400),
                    DepartmentId = departmentIds[i % departmentIds.Length]
                };
                Database.Posts.Create(post);
            }
            await Database.SaveAsync();
        }

        public override void Dispose() {
            Database.Dispose();
        }
    }
}
