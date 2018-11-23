using AutoMapper;
using DiplomMSSQLApp.BLL.DTO;
using DiplomMSSQLApp.BLL.Infrastructure;
using DiplomMSSQLApp.BLL.Interfaces;
using DiplomMSSQLApp.BLL.Services;
using DiplomMSSQLApp.WEB.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace DiplomMSSQLApp.WEB.Controllers {
    [HandleError]
    [Authorize]
    public class PostController : Controller {
        private IService<EmployeeDTO> employeeService;
        private IService<PostDTO> postService;
        private IService<DepartmentDTO> departmentService;

        public PostController(IService<EmployeeDTO> es, IService<PostDTO> ps, IService<DepartmentDTO> ds) {
            employeeService = es;
            postService = ps;
            departmentService = ds;
        }

        public async Task<ActionResult> Index(int page = 1) {
            IEnumerable<PostDTO> pDto = await postService.GetAllAsync();
            ViewBag.TotalNumberOfUnits = pDto.Sum(p => p.NumberOfUnits);
            ViewBag.TotalSalary = pDto.Sum(p => p.TotalSalary);
            pDto = postService.GetPage(pDto, page);     // Paging
            Mapper.Initialize(cfg => {
                cfg.CreateMap<PostDTO, PostViewModel>()
                    .ForMember(p => p.Employees, opt => opt.Ignore());
                cfg.CreateMap<DepartmentDTO, DepartmentViewModel>()
                    .ForMember(d => d.Posts, opt => opt.Ignore());
            });
            IEnumerable<PostViewModel> posts = Mapper.Map<IEnumerable<PostDTO>, IEnumerable<PostViewModel>>(pDto);

            return View("Index", new PostListViewModel { Posts = posts, PageInfo = postService.PageInfo });
        }

        // Добавление новой должности
        public async Task<ActionResult> Create() {
            IEnumerable<DepartmentDTO> departments = await departmentService.GetAllAsync();
            if (departments.Count() == 0)
                return View("Error", new string[] { "Нельзя добавить должность, если нет ни одного отдела" });
            ViewBag.Departments = await GetSelectListDepartmentsAsync();
            return View("Create");
        }
        [HttpPost, ValidateAntiForgeryToken, ActionName("Create")]
        public async Task<ActionResult> CreateAsync(PostViewModel p) {
            try {
                PostDTO pDto = MapViewModelWithDTO(p);
                await postService.CreateAsync(pDto);
                return RedirectToAction("Index");
            }
            catch (ValidationException ex) {
                ModelState.AddModelError(ex.Property, ex.Message);
            }
            ViewBag.Departments = await GetSelectListDepartmentsAsync();
            return View("Create", p);
        }

        private async Task<SelectList> GetSelectListDepartmentsAsync() {
            IEnumerable<DepartmentDTO> departments = await departmentService.GetAllAsync();
            //if (departments.Count() == 0) {
            //    DepartmentDTO btDto = new DepartmentDTO { Id = 1, DepartmentName = "unknown" };
            //    await departmentService.CreateAsync(btDto);
            //    departments = new List<DepartmentDTO> { btDto };
            //}
            return new SelectList(departments.OrderBy(d => d.DepartmentName), "Id", "DepartmentName");
        }

        private PostDTO MapViewModelWithDTO(PostViewModel post) {
            Mapper.Initialize(cfg => {
                cfg.CreateMap<PostViewModel, PostDTO>()
                    .ForMember(p => p.Department, opt => opt.Ignore());
                cfg.CreateMap<EmployeeViewModel, EmployeeDTO>()
                    .ForMember(e => e.BusinessTrips, opt => opt.Ignore())
                    .ForMember(e => e.Post, opt => opt.Ignore());
            });
            PostDTO pDto = Mapper.Map<PostViewModel, PostDTO>(post);
            return pDto;
        }

        // Обновление информации о должности
        [ActionName("Edit")]
        public async Task<ActionResult> EditAsync(int? id) {
            ViewBag.Departments = await GetSelectListDepartmentsAsync();
            return await GetViewAsync(id, "Edit");
        }
        [HttpPost, ValidateAntiForgeryToken, ActionName("Edit")]
        public async Task<ActionResult> EditAsync(PostViewModel p) {
            try {
                PostDTO pDto = MapViewModelWithDTO(p);
                await postService.EditAsync(pDto);
                return RedirectToAction("Index");
            }
            catch (ValidationException ex) {
                ModelState.AddModelError(ex.Property, ex.Message);
            }
            ViewBag.Departments = await GetSelectListDepartmentsAsync();
            return View("Edit", p);
        }

        private async Task<ActionResult> GetViewAsync(int? id, string viewName) {
            try {
                PostDTO pDto = await postService.FindByIdAsync(id);
                PostViewModel p = MapDTOWithViewModel(pDto);
                return View(viewName, p);
            }
            catch (ValidationException ex) {
                return View("Error", new string[] { ex.Message });
            }
        }

        private PostViewModel MapDTOWithViewModel(PostDTO pDto) {
            Mapper.Initialize(cfg => {
                cfg.CreateMap<PostDTO, PostViewModel>();
                cfg.CreateMap<DepartmentDTO, DepartmentViewModel>()
                    .ForMember(d => d.Manager, opt => opt.Ignore())
                    .ForMember(d => d.Organization, opt => opt.Ignore())
                    .ForMember(d => d.Posts, opt => opt.Ignore());
                cfg.CreateMap<EmployeeDTO, EmployeeViewModel>()
                    .ForMember(e => e.Birth, opt => opt.Ignore())
                    .ForMember(e => e.BusinessTrips, opt => opt.Ignore())
                    .ForMember(e => e.Contacts, opt => opt.Ignore())
                    .ForMember(e => e.Education, opt => opt.Ignore())
                    .ForMember(e => e.Passport, opt => opt.Ignore())
                    .ForMember(e => e.Post, opt => opt.Ignore());
            });
            PostViewModel post = Mapper.Map<PostDTO, PostViewModel>(pDto);
            return post;
        }

        // Подробная информация о должности
        [ActionName("Details")]
        public async Task<ActionResult> DetailsAsync(int? id) {
            return await GetViewAsync(id, "Details");
        }

        // Удаление должности
        [ActionName("Delete")]
        public async Task<ActionResult> DeleteAsync(int? id) {
            return await GetViewAsync(id, "Delete");
        }
        [HttpPost, ValidateAntiForgeryToken, ActionName("Delete")]
        public async Task<ActionResult> DeleteConfirmedAsync(int id) {
            try {
                await postService.DeleteAsync(id);
            }
            catch (Exception) {
                return View("Error", new string[] { "Нельзя удалить должность, пока в ней работает хотя бы один сотрудник." });
            }
            return RedirectToAction("Index");
        }

        // Удаление всех должностей
        public ActionResult DeleteAll() {
            return View("DeleteAll");
        }
        [HttpPost, ValidateAntiForgeryToken, ActionName("DeleteAll")]
        public async Task<ActionResult> DeleteAllAsync() {
            try {
                await postService.DeleteAllAsync();
            }
            catch (Exception) {
                return View("Error", new string[] { "Нельзя удалить должность, пока в ней работает хотя бы один сотрудник." });
            }
            return RedirectToAction("Index");
        }

        // Запись информации о должностях в файл
        [ActionName("ExportJson")]
        public async Task<ActionResult> ExportJsonAsync() {
            string fullPath = CreateDirectoryToFile("Posts.json");
            System.IO.File.Delete(fullPath);
            await (postService as PostService).ExportJsonAsync(fullPath);
            return RedirectToAction("Index");
        }

        private string CreateDirectoryToFile(string filename) {
            string dir = Server.MapPath("~/Results/Post/");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            string fullPath = dir + filename;
            return fullPath;
        }

        protected override void Dispose(bool disposing) {
            employeeService.Dispose();
            postService.Dispose();
            departmentService.Dispose();
            base.Dispose(disposing);
        }
    }
}
