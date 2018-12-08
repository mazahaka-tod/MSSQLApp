using AutoMapper;
using DiplomMSSQLApp.BLL.DTO;
using DiplomMSSQLApp.BLL.Infrastructure;
using DiplomMSSQLApp.BLL.Interfaces;
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
    public class DepartmentController : Controller {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private IService<EmployeeDTO> _employeeService;
        private IService<DepartmentDTO> _departmentService;
        private IService<OrganizationDTO> _organizationService;

        public DepartmentController(IService<EmployeeDTO> es, IService<DepartmentDTO> ds, IService<OrganizationDTO> os) {
            _employeeService = es;
            _departmentService = ds;
            _organizationService = os;
        }

        public async Task<ActionResult> Index(int page = 1) {
            IEnumerable<DepartmentDTO> dDto = await _departmentService.GetAllAsync();
            dDto = _departmentService.GetPage(dDto, page);     // Paging
            Mapper.Initialize(cfg => {
                cfg.CreateMap<DepartmentDTO, DepartmentViewModel>()
                    .ForMember(d => d.Posts, opt => opt.Ignore());
                cfg.CreateMap<EmployeeDTO, EmployeeViewModel>()
                    .ForMember(e => e.BusinessTrips, opt => opt.Ignore())
                    .ForMember(e => e.Post, opt => opt.Ignore())
                    .ForMember(e => e.Birth, opt => opt.Ignore())
                    .ForMember(e => e.Contacts, opt => opt.Ignore())
                    .ForMember(e => e.Education, opt => opt.Ignore())
                    .ForMember(e => e.Passport, opt => opt.Ignore());
                cfg.CreateMap<OrganizationDTO, OrganizationViewModel>()
                    .ForMember(o => o.Requisites, opt => opt.Ignore())
                    .ForMember(o => o.Bank, opt => opt.Ignore());
            });
            IEnumerable<DepartmentViewModel> departments = Mapper.Map<IEnumerable<DepartmentDTO>, IEnumerable<DepartmentViewModel>>(dDto);
            DepartmentListViewModel model = new DepartmentListViewModel {
                Departments = departments,
                PageInfo = _departmentService.PageInfo
            };
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest") {
                _logger.Info("Executed async request");
                var transformModel = new {
                    Departments = model.Departments.Select(d => new {
                        d.Id,
                        d.Code,
                        d.DepartmentName,
                        OrganizationName = d.Organization.Name,
                        Manager = d.Manager != null ? d.Manager.LastName + " " + d.Manager.FirstName + " " + d.Manager.Patronymic : ""
                    }).ToArray(),
                    model.PageInfo
                };
                return Json(transformModel, JsonRequestBehavior.AllowGet);
            }
            _logger.Info("Executed sync request");
            return View("Index", model);
        }

        // Добавление нового отдела
        public async Task<ActionResult> Create() {
            ViewBag.Organizations = await GetSelectListOrganizationsAsync();
            ViewBag.Employees = await GetSelectListEmployeesAsync();
            return View("Create");
        }
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(DepartmentViewModel d) {
            try {
                DepartmentDTO dDto = MapViewModelWithDTO(d);
                await _departmentService.CreateAsync(dDto);
                return RedirectToAction("Index");
            }
            catch (ValidationException ex) {
                _logger.Warn(ex.Message);
                ModelState.AddModelError(ex.Property, ex.Message);
            }
            ViewBag.Organizations = await GetSelectListOrganizationsAsync();
            ViewBag.Employees = await GetSelectListEmployeesAsync();
            return View("Create", d);
        }

        private async Task<SelectList> GetSelectListOrganizationsAsync() {
            IEnumerable<OrganizationDTO> organizations = await _organizationService.GetAllAsync();
            return new SelectList(organizations.OrderBy(o => o.Name), "Id", "Name");
        }

        private async Task<SelectList> GetSelectListEmployeesAsync() {
            IEnumerable<EmployeeDTO> employees = await _employeeService.GetAllAsync();
            IEnumerable<SelectListItem> items = employees.Select(e => new SelectListItem() {
                Value = e.Id.ToString(),
                Text = e.LastName + " " + e.FirstName + " " + e.Patronymic
            }).OrderBy(e => e.Text);
            return new SelectList(items, "Value", "Text");
        }

        private DepartmentDTO MapViewModelWithDTO(DepartmentViewModel department) {
            Mapper.Initialize(cfg => {
                cfg.CreateMap<DepartmentViewModel, DepartmentDTO>()
                    .ForMember(d => d.Posts, opt => opt.Ignore());
            });
            DepartmentDTO dDto = Mapper.Map<DepartmentViewModel, DepartmentDTO>(department);
            return dDto;
        }

        // Обновление информации об отделе
        public async Task<ActionResult> Edit(int? id) {
            ViewBag.Organizations = await GetSelectListOrganizationsAsync();
            ViewBag.Employees = await GetSelectListEmployeesAsync();
            return await GetViewAsync(id, "Edit");
        }
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(DepartmentViewModel d) {
            try {
                DepartmentDTO dDto = MapViewModelWithDTO(d);
                await _departmentService.EditAsync(dDto);
                return RedirectToAction("Index");
            }
            catch (ValidationException ex) {
                _logger.Warn(ex.Message);
                ModelState.AddModelError(ex.Property, ex.Message);
            }
            ViewBag.Organizations = await GetSelectListOrganizationsAsync();
            ViewBag.Employees = await GetSelectListEmployeesAsync();
            return View("Edit", d);
        }

        private async Task<ActionResult> GetViewAsync(int? id, string viewName) {
            try {
                DepartmentDTO dDto = await _departmentService.FindByIdAsync(id);
                DepartmentViewModel d = MapDTOWithViewModel(dDto);
                return View(viewName, d);
            }
            catch (ValidationException ex) {
                _logger.Warn(ex.Message);
                return View("Error", new string[] { ex.Message });
            }
        }

        private DepartmentViewModel MapDTOWithViewModel(DepartmentDTO dDto) {
            Mapper.Initialize(cfg => {
                cfg.CreateMap<DepartmentDTO, DepartmentViewModel>();
                cfg.CreateMap<EmployeeDTO, EmployeeViewModel>()
                    .ForMember(e => e.BusinessTrips, opt => opt.Ignore())
                    .ForMember(e => e.Post, opt => opt.Ignore())
                    .ForMember(e => e.Birth, opt => opt.Ignore())
                    .ForMember(e => e.Contacts, opt => opt.Ignore())
                    .ForMember(e => e.Education, opt => opt.Ignore())
                    .ForMember(e => e.Passport, opt => opt.Ignore());
                cfg.CreateMap<OrganizationDTO, OrganizationViewModel>()
                    .ForMember(o => o.Requisites, opt => opt.Ignore())
                    .ForMember(o => o.Bank, opt => opt.Ignore());
                cfg.CreateMap<PostDTO, PostViewModel>()
                    .ForMember(p => p.Employees, opt => opt.Ignore());
            });
            DepartmentViewModel department = Mapper.Map<DepartmentDTO, DepartmentViewModel>(dDto);
            return department;
        }

        // Подробная информация об отделе
        public async Task<ActionResult> Details(int? id) {
            return await GetViewAsync(id, "Details");
        }

        // Удаление отдела
        public async Task<ActionResult> Delete(int? id) {
            return await GetViewAsync(id, "Delete");
        }
        [HttpPost, ValidateAntiForgeryToken, ActionName("Delete")]
        public async Task<ActionResult> DeleteConfirmed(int id) {
            try {
                await _departmentService.DeleteAsync(id);
            }
            catch (ValidationException ex) {
                _logger.Warn("Failed to delete department");
                return View("Error", new string[] { ex.Message });
            }
            return RedirectToAction("Index");
        }

        // Удаление отделов, для которых еще не создано штатное расписание
        public ActionResult DeleteAll() {
            return View("DeleteAll");
        }
        [HttpPost, ValidateAntiForgeryToken, ActionName("DeleteAll")]
        public async Task<ActionResult> DeleteAllConfirmed() {
            await _departmentService.DeleteAllAsync();
            return RedirectToAction("Index");
        }

        // Запись информации об отделах в JSON-файл
        public async Task<ActionResult> ExportJson() {
            string fullPath = CreateDirectoryToFile("Departments.json");
            System.IO.File.Delete(fullPath);
            await _departmentService.ExportJsonAsync(fullPath);
            return File(fullPath, "application/json", "Departments.json");
        }

        private string CreateDirectoryToFile(string filename) {
            string dir = Server.MapPath("~/Results/");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            string fullPath = dir + filename;
            return fullPath;
        }

        protected override void Dispose(bool disposing) {
            _employeeService.Dispose();
            _departmentService.Dispose();
            _organizationService.Dispose();
            base.Dispose(disposing);
        }
    }
}
