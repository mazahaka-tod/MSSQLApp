using AutoMapper;
using DiplomMSSQLApp.BLL.BusinessModels;
using DiplomMSSQLApp.BLL.DTO;
using DiplomMSSQLApp.BLL.Infrastructure;
using DiplomMSSQLApp.BLL.Interfaces;
using DiplomMSSQLApp.BLL.Services;
using DiplomMSSQLApp.WEB.Models;
using NLog;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace DiplomMSSQLApp.WEB.Controllers {
    [HandleError]
    [Authorize]
    public class EmployeeController : Controller {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private IService<EmployeeDTO> employeeService;
        private IService<DepartmentDTO> departmentService;
        private IService<PostDTO> postService;

        public EmployeeController(IService<EmployeeDTO> es, IService<DepartmentDTO> ds, IService<PostDTO> ps) {
            employeeService = es;
            departmentService = ds;
            postService = ps;
        }

        public ActionResult Index(EmployeeFilter filter, string filterAsJsonString, int page = 1) {
            logger.Info("Called action");
            // При использовании функции Paging передается строка filterAsJsonString, а аргумент EmployeeFilter filter == null,
            // поэтому производим декодирование строки filterAsJsonString в объект EmployeeFilter и присваиваем его переменной filter
            if (filterAsJsonString != null)
                filter = System.Web.Helpers.Json.Decode<EmployeeFilter>(filterAsJsonString);
            IEnumerable<EmployeeDTO> eDto = (employeeService as EmployeeService).Get(filter);
            eDto = employeeService.GetPage(eDto, page);     // Paging
            Mapper.Initialize(cfg => {
                cfg.CreateMap<EmployeeDTO, EmployeeViewModel>()
                    .ForMember(e => e.BusinessTrips, opt => opt.Ignore());
                cfg.CreateMap<DepartmentDTO, DepartmentViewModel>()
                    .ForMember(d => d.Posts, opt => opt.Ignore());
                cfg.CreateMap<PostDTO, PostViewModel>()
                    .ForMember(p => p.Employees, opt => opt.Ignore());

                cfg.CreateMap<BLL.DTO.Birth, Models.Birth>();
                cfg.CreateMap<BLL.DTO.Passport, Models.Passport>();
                cfg.CreateMap<BLL.DTO.Contacts, Models.Contacts>();
                cfg.CreateMap<BLL.DTO.Education, Models.Education>();
            });
            IEnumerable<EmployeeViewModel> employees = Mapper.Map<IEnumerable<EmployeeDTO>, IEnumerable<EmployeeViewModel>>(eDto);
            EmployeeListViewModel model = new EmployeeListViewModel { Employees = employees, Filter = filter, PageInfo = employeeService.PageInfo };
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest") {
                logger.Info("Executed async request");
                return PartialView("GetEmployeesData", model);
            }
            logger.Info("Executed sync request");
            return View("Index", model);
        }

        // Добавление нового сотрудника
        [ActionName("Create")]
        public async Task<ActionResult> CreateAsync() {
            int totalNumberOfUnits = (await postService.GetAllAsync()).Sum(p => p.NumberOfUnits).Value;
            int employeesCount = await employeeService.CountAsync();
            if (employeesCount >= totalNumberOfUnits) {
                logger.Warn("No vacancy");
                return View("Error", new string[] { "Нельзя принять сотрудника, если нет свободных вакансий" });
            }
            ViewBag.Posts = await GetSelectListPostsAsync();
            return View("Create");
        }
        [HttpPost, ValidateAntiForgeryToken, ActionName("Create")]
        public async Task<ActionResult> CreateAsync(EmployeeViewModel e) {
            try {
                var eDto = MapViewModelWithDTO(e);
                await employeeService.CreateAsync(eDto);
                return RedirectToAction("Index");
            }
            catch (ValidationException ex) {
                ModelState.AddModelError(ex.Property, ex.Message);
            }
            ViewBag.Posts = await GetSelectListPostsAsync();
            return View("Create", e);
        }

        private async Task<SelectList> GetSelectListPostsAsync() {
            IEnumerable<PostDTO> posts = await postService.GetAllAsync();
            IEnumerable<SelectListItem> items = posts.Select(p => new SelectListItem() {
                Value = p.Id.ToString(),
                Text = p.Title + " [" + p.Department.DepartmentName + "]"
            }).OrderBy(p => p.Text);
            return new SelectList(items, "Value", "Text");
        }

        private EmployeeDTO MapViewModelWithDTO(EmployeeViewModel e) {
            Mapper.Initialize(cfg => {
                cfg.CreateMap<EmployeeViewModel, EmployeeDTO>();
                cfg.CreateMap<BaseBusinessTripViewModel, BaseBusinessTripDTO>();
                cfg.CreateMap<DepartmentViewModel, DepartmentDTO>()
                   .ForMember(d => d.Posts, opt => opt.Ignore());
                cfg.CreateMap<PostViewModel, PostDTO>()
                   .ForMember(p => p.Employees, opt => opt.Ignore());

                cfg.CreateMap<Models.Birth, BLL.DTO.Birth>();
                cfg.CreateMap<Models.Passport, BLL.DTO.Passport>();
                cfg.CreateMap<Models.Contacts, BLL.DTO.Contacts>();
                cfg.CreateMap<Models.Education, BLL.DTO.Education>();
            });
            EmployeeDTO eDto = Mapper.Map<EmployeeViewModel, EmployeeDTO>(e);
            return eDto;
        }

        // Обновление информации о сотруднике
        [ActionName("Edit")]
        public async Task<ActionResult> EditAsync(int? id) {
            ViewBag.Posts = await GetSelectListPostsAsync();
            return await GetViewAsync(id, "Edit");
        }
        [HttpPost, ValidateAntiForgeryToken, ActionName("Edit")]
        public async Task<ActionResult> EditAsync(EmployeeViewModel e) {
            try {
                EmployeeDTO eDto = MapViewModelWithDTO(e);
                await employeeService.EditAsync(eDto);
                return RedirectToAction("Index");
            }
            catch (ValidationException ex) {
                ModelState.AddModelError(ex.Property, ex.Message);
            }
            ViewBag.Posts = await GetSelectListPostsAsync();
            return View("Edit", e);
        }

        private async Task<ActionResult> GetViewAsync(int? id, string viewName) {
            try {
                EmployeeDTO eDto = await employeeService.FindByIdAsync(id);
                EmployeeViewModel e = MapDTOWithViewModel(eDto);
                return View(viewName, e);
            }
            catch (ValidationException ex) {
                return View("Error", new string[] { ex.Message });
            }
        }

        private EmployeeViewModel MapDTOWithViewModel(EmployeeDTO eDto) {
            Mapper.Initialize(cfg => {
                cfg.CreateMap<EmployeeDTO, EmployeeViewModel>();
                cfg.CreateMap<BaseBusinessTripDTO, BaseBusinessTripViewModel>();
                cfg.CreateMap<DepartmentDTO, DepartmentViewModel>()
                   .ForMember(d => d.Posts, opt => opt.Ignore());
                cfg.CreateMap<PostDTO, PostViewModel>()
                   .ForMember(p => p.Employees, opt => opt.Ignore());

                cfg.CreateMap<BLL.DTO.Birth, Models.Birth>();
                cfg.CreateMap<BLL.DTO.Passport, Models.Passport>();
                cfg.CreateMap<BLL.DTO.Contacts, Models.Contacts>();
                cfg.CreateMap<BLL.DTO.Education, Models.Education>();
            });
            EmployeeViewModel e = Mapper.Map<EmployeeDTO, EmployeeViewModel>(eDto);
            return e;
        }

        // Подробная информация о сотруднике
        [ActionName("Details")]
        public async Task<ActionResult> DetailsAsync(int? id) {
            return await GetViewAsync(id, "Details");
        }

        // Удаление сотрудника
        [ActionName("Delete")]
        public async Task<ActionResult> DeleteAsync(int? id) {
            return await GetViewAsync(id, "Delete");
        }
        [HttpPost, ValidateAntiForgeryToken, ActionName("Delete")]
        public async Task<ActionResult> DeleteConfirmedAsync(int id) {
            await employeeService.DeleteAsync(id);
            return RedirectToAction("Index");
        }

        // Удаление всех сотрудников
        public ActionResult DeleteAll() {
            return View("DeleteAll");
        }
        [HttpPost, ValidateAntiForgeryToken, ActionName("DeleteAll")]
        public async Task<ActionResult> DeleteAllAsync() {
            await employeeService.DeleteAllAsync();
            return RedirectToAction("Index");
        }

        // Запись информации о сотрудниках в файл
        [ActionName("ExportJson")]
        public async Task<ActionResult> ExportJsonAsync() {
            string fullPath = CreateDirectoryToFile("Employees.json");
            System.IO.File.Delete(fullPath);
            await (employeeService as EmployeeService).ExportJsonAsync(fullPath);
            return RedirectToAction("Index");
        }

        private string CreateDirectoryToFile(string filename) {
            string dir = Server.MapPath("~/Results/Employee/");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            string fullPath = dir + filename;
            return fullPath;
        }

        public ActionResult About() {
            return View("About");
        }

        public ActionResult Contact() {
            return View("Contact");
        }

        protected override void Dispose(bool disposing) {
            employeeService.Dispose();
            postService.Dispose();
            departmentService.Dispose();
            base.Dispose(disposing);
        }
    }
}
