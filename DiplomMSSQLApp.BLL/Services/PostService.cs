using AutoMapper;
using DiplomMSSQLApp.BLL.DTO;
using DiplomMSSQLApp.BLL.Infrastructure;
using DiplomMSSQLApp.DAL.Entities;
using DiplomMSSQLApp.DAL.Interfaces;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
                    .ForMember(e => e.Post, opt => opt.Ignore());
            });
            Post post = Mapper.Map<PostDTO, Post>(pDto);
            return post;
        }

        private void ValidationPost(PostDTO pDto) {
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
                cfg.CreateMap<Post, PostDTO>()
                    .ForMember(p => p.Department, opt => opt.Ignore())
                    .ForMember(p => p.Employees, opt => opt.Ignore());
                //cfg.CreateMap<Department, DepartmentDTO>()
                    //.ForMember(d => d.Employees, opt => opt.Ignore());
                //cfg.CreateMap<Employee, EmployeeDTO>()
                    //.ForMember(e => e.BusinessTrips, opt => opt.Ignore())
                    //.ForMember(e => e.Post, opt => opt.Ignore());
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

        // Запись информации о должностях в файл
        public virtual async Task ExportJsonAsync(string fullPath) {
            IEnumerable<Post> posts = await Database.Posts.GetAllAsync();
            var transformPosts = posts.Select(p => new {
                p.Title,
                p.NumberOfUnits,
                p.Salary,
                p.Premium,
                p.Department?.DepartmentName
            }).ToArray();
            using (StreamWriter sw = new StreamWriter(fullPath, true, Encoding.UTF8)) {
                sw.WriteLine("{\"Posts\":");
                sw.WriteLine(new JavaScriptSerializer().Serialize(transformPosts));
                sw.WriteLine("}");
            }
        }

        public override void Dispose() {
            Database.Dispose();
        }
    }
}
