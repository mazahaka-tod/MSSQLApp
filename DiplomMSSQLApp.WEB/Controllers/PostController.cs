using AutoMapper;
using DiplomMSSQLApp.BLL.DTO;
using DiplomMSSQLApp.BLL.Infrastructure;
using DiplomMSSQLApp.BLL.Interfaces;
using DiplomMSSQLApp.BLL.Services;
using DiplomMSSQLApp.WEB.Models;
using NLog;
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
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private IService<EmployeeDTO> _employeeService;
        private IService<PostDTO> _postService;
        private IService<DepartmentDTO> _departmentService;

        public PostController(IService<EmployeeDTO> es, IService<PostDTO> ps, IService<DepartmentDTO> ds) {
            _employeeService = es;
            _postService = ps;
            _departmentService = ds;
        }

        public async Task<ActionResult> Index(int page = 1) {
            IEnumerable<PostDTO> pDto = await _postService.GetAllAsync();
            ViewBag.TotalNumberOfUnits = pDto.Sum(p => p.NumberOfUnits);
            ViewBag.TotalSalary = pDto.Sum(p => p.TotalSalary);
            pDto = _postService.GetPage(pDto, page);     // Paging
            Mapper.Initialize(cfg => {
                cfg.CreateMap<PostDTO, PostViewModel>()
                    .ForMember(p => p.Employees, opt => opt.Ignore());
                cfg.CreateMap<DepartmentDTO, DepartmentViewModel>()
                    .ForMember(d => d.Posts, opt => opt.Ignore());
            });
            IEnumerable<PostViewModel> posts = Mapper.Map<IEnumerable<PostDTO>, IEnumerable<PostViewModel>>(pDto);
            PostListViewModel model = new PostListViewModel { Posts = posts, PageInfo = _postService.PageInfo };
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest") {
                _logger.Info("Executed async request");
                //return PartialView("GetPostsData", model);
                var transformModel = new {
                    Posts = model.Posts.Select(p => new {
                        p.Id,
                        DepartmentCode = p.Department.Code,
                        DepartmentName = p.Department.DepartmentName,
                        p.Title,
                        p.NumberOfUnits,
                        p.Salary,
                        p.Premium,
                        p.TotalSalary
                    }).ToArray(),
                    model.PageInfo
                };
                return Json(transformModel, JsonRequestBehavior.AllowGet);
            }
            _logger.Info("Executed sync request");
            return View("Index", model);
        }

        // Добавление новой должности
        public async Task<ActionResult> Create() {
            IEnumerable<DepartmentDTO> departments = await _departmentService.GetAllAsync();
            if (departments.Count() == 0) {
                _logger.Warn("No departments");
                return View("Error", new string[] { "Нельзя добавить должность, если нет ни одного отдела" });
            }
            ViewBag.Departments = await GetSelectListDepartmentsAsync();
            return View("Create");
        }
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(PostViewModel p) {
            try {
                PostDTO pDto = MapViewModelWithDTO(p);
                await _postService.CreateAsync(pDto);
                return RedirectToAction("Index");
            }
            catch (ValidationException ex) {
                _logger.Warn(ex.Message);
                ModelState.AddModelError(ex.Property, ex.Message);
            }
            ViewBag.Departments = await GetSelectListDepartmentsAsync();
            return View("Create", p);
        }

        private async Task<SelectList> GetSelectListDepartmentsAsync() {
            IEnumerable<DepartmentDTO> departments = await _departmentService.GetAllAsync();
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
        public async Task<ActionResult> Edit(int? id) {
            ViewBag.Departments = await GetSelectListDepartmentsAsync();
            return await GetViewAsync(id, "Edit");
        }
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(PostViewModel p) {
            try {
                PostDTO pDto = MapViewModelWithDTO(p);
                await _postService.EditAsync(pDto);
                return RedirectToAction("Index");
            }
            catch (ValidationException ex) {
                _logger.Warn(ex.Message);
                ModelState.AddModelError(ex.Property, ex.Message);
            }
            ViewBag.Departments = await GetSelectListDepartmentsAsync();
            return View("Edit", p);
        }

        private async Task<ActionResult> GetViewAsync(int? id, string viewName) {
            try {
                PostDTO pDto = await _postService.FindByIdAsync(id);
                PostViewModel p = MapDTOWithViewModel(pDto);
                return View(viewName, p);
            }
            catch (ValidationException ex) {
                _logger.Warn(ex.Message);
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
        public async Task<ActionResult> Details(int? id) {
            return await GetViewAsync(id, "Details");
        }

        // Удаление должности
        public async Task<ActionResult> Delete(int? id) {
            return await GetViewAsync(id, "Delete");
        }
        [HttpPost, ValidateAntiForgeryToken, ActionName("Delete")]
        public async Task<ActionResult> DeleteConfirmed(int id) {
            try {
                await _postService.DeleteAsync(id);
            }
            catch (Exception) {
                _logger.Warn("Failed to delete post");
                return View("Error", new string[] { "Нельзя удалить должность, пока в ней работает хотя бы один сотрудник" });
            }
            return RedirectToAction("Index");
        }

        // Удаление всех должностей
        public ActionResult DeleteAll() {
            return View("DeleteAll");
        }
        [HttpPost, ValidateAntiForgeryToken, ActionName("DeleteAll")]
        public async Task<ActionResult> DeleteAllConfirmed() {
            try {
                await _postService.DeleteAllAsync();
            }
            catch (Exception) {
                _logger.Warn("Failed to delete post");
                return View("Error", new string[] { "Нельзя удалить должность, пока в ней работает хотя бы один сотрудник" });
            }
            return RedirectToAction("Index");
        }

        // Запись информации о должностях в JSON-файл
        public async Task<ActionResult> ExportJson() {
            string fullPath = CreateDirectoryToFile("Posts.json");
            System.IO.File.Delete(fullPath);
            await _postService.ExportJsonAsync(fullPath);
            return RedirectToAction("Index");
        }

        private string CreateDirectoryToFile(string filename) {
            string dir = Server.MapPath("~/Results/");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            string fullPath = dir + filename;
            return fullPath;
        }

        // Добавление должностей для тестирования
        public async Task<ActionResult> TestCreate() {
            try {
                await (_postService as PostService).TestCreateAsync();
            }
            catch (ValidationException ex) {
                _logger.Warn("No departments");
                return View("Error", new string[] { ex.Message });
            }
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing) {
            _employeeService.Dispose();
            _postService.Dispose();
            _departmentService.Dispose();
            base.Dispose(disposing);
        }
    }
}
