using AutoMapper;
using DiplomMSSQLApp.BLL.BusinessModels;
using DiplomMSSQLApp.BLL.DTO;
using DiplomMSSQLApp.BLL.Infrastructure;
using DiplomMSSQLApp.BLL.Interfaces;
using DiplomMSSQLApp.BLL.Services;
using DiplomMSSQLApp.WEB.Models;
using DiplomMSSQLApp.WEB.Util;
using NLog;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace DiplomMSSQLApp.WEB.Controllers {
    [HandleError]
    [Authorize]
    [Internationalization]
    public class AnnualLeaveController : Controller {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private IService<AnnualLeaveDTO> _annualLeaveService;
        private IService<EmployeeDTO> _employeeService;

        public AnnualLeaveController(IService<AnnualLeaveDTO> al, IService<EmployeeDTO> es) {
            _annualLeaveService = al;
            _employeeService = es;
        }

        public ActionResult Index(AnnualLeaveFilter filter, string filterAsJsonString, int page = 1) {
            if (filterAsJsonString != null)
                filter = System.Web.Helpers.Json.Decode<AnnualLeaveFilter>(filterAsJsonString);
            IEnumerable<AnnualLeaveDTO> alDto = (_annualLeaveService as AnnualLeaveService).Get(filter);  // Filter
            alDto = _annualLeaveService.GetPage(alDto, page);     // Paging
            InitializeMapper();
            IEnumerable<AnnualLeaveViewModel> annualLeaves = Mapper.Map<IEnumerable<AnnualLeaveDTO>, IEnumerable<AnnualLeaveViewModel>>(alDto);
            AnnualLeaveListViewModel model = new AnnualLeaveListViewModel {
                AnnualLeaves = annualLeaves,
                Filter = filter,
                PageInfo = _annualLeaveService.PageInfo
            };
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest") {
                _logger.Info("Executed async request");
                var transformModel = new {
                    AnnualLeaves = model.AnnualLeaves.Select(al => new {
                        al.Id,
                        al.ScheduledNumberOfDays,
                        al.ActualNumberOfDays,
                        ScheduledDate = al.ScheduledDate.ToString("dd MMMM yyyy"),
                        ActualDate = al.ActualDate.HasValue ? al.ActualDate.Value.ToString("dd MMMM yyyy") : "",
                        EmployeeName = new System.Text.StringBuilder(al.Employee.LastName).Append(" ")
                            .Append(al.Employee.FirstName).Append(" ").Append(al.Employee.Patronymic ?? "").ToString(),
                        PostTitle = al.Employee.Post.Title,
                        NumberOfDaysOfLeave = al.Employee.Post.NumberOfDaysOfLeave,
                        DepartmentName = al.Employee.Post.Department.DepartmentName
                    }).ToArray(),
                    model.Filter,
                    model.PageInfo
                };
                return Json(transformModel, JsonRequestBehavior.AllowGet);
            }
            _logger.Info("Executed sync request");
            return View("Index", model);
        }

        // Добавление нового отпуска
        public async Task<ActionResult> Create() {
            int employeesCount = await _employeeService.CountAsync();
            if (employeesCount == 0) {
                _logger.Warn("No employees");
                return View("Error", new string[] { "В организации нет сотрудников" });
            }
            ViewBag.Employees = await GetSelectListEmployeesAsync();
            return View("Create");
        }
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(AnnualLeaveViewModel al) {
            try {
                AnnualLeaveDTO alDto = MapViewModelWithDTO(al);
                await (_annualLeaveService as AnnualLeaveService).CreateAsync(alDto);
                return RedirectToAction("Index");
            }
            catch (ValidationException ex) {
                _logger.Warn(ex.Message);
                ModelState.AddModelError(ex.Property, ex.Message);
            }
            ViewBag.Employees = await GetSelectListEmployeesAsync();
            return View("Create", al);
        }

        private async Task<SelectList> GetSelectListEmployeesAsync() {
            IEnumerable<EmployeeDTO> employees = await _employeeService.GetAllAsync();
            IEnumerable<SelectListItem> items = employees.Select(e => new SelectListItem() {
                Value = e.Id.ToString(),
                Text = new System.Text.StringBuilder(e.LastName).Append(" ").Append(e.FirstName).Append(" ")
                    .Append(e.Patronymic ?? "").Append(" [").Append(e.Post.Department.DepartmentName).Append("]").ToString()
            }).OrderBy(e => e.Text);
            return new SelectList(items, "Value", "Text");
        }

        private AnnualLeaveDTO MapViewModelWithDTO(AnnualLeaveViewModel al) {
            Mapper.Initialize(cfg => {
                cfg.CreateMap<AnnualLeaveViewModel, AnnualLeaveDTO>();
                cfg.CreateMap<LeaveScheduleViewModel, LeaveScheduleDTO>()
                    .ForMember(ls => ls.AnnualLeaves, opt => opt.Ignore());
                cfg.CreateMap<EmployeeViewModel, EmployeeDTO>()
                    .ForMember(e => e.AnnualLeaves, opt => opt.Ignore())
                    .ForMember(e => e.Birth, opt => opt.Ignore())
                    .ForMember(e => e.BusinessTrips, opt => opt.Ignore())
                    .ForMember(e => e.Contacts, opt => opt.Ignore())
                    .ForMember(e => e.Education, opt => opt.Ignore())
                    .ForMember(e => e.Passport, opt => opt.Ignore());
                cfg.CreateMap<PostViewModel, PostDTO>()
                    .ForMember(p => p.Employees, opt => opt.Ignore());
                cfg.CreateMap<DepartmentViewModel, DepartmentDTO>()
                    .ForMember(d => d.Manager, opt => opt.Ignore())
                    .ForMember(d => d.Organization, opt => opt.Ignore())
                    .ForMember(d => d.Posts, opt => opt.Ignore());
            });
            AnnualLeaveDTO alDto = Mapper.Map<AnnualLeaveViewModel, AnnualLeaveDTO>(al);
            return alDto;
        }

        // Обновление информации об отпуске
        public async Task<ActionResult> Edit(int? id) {
            ViewBag.Employees = await GetSelectListEmployeesAsync();
            return await GetViewAsync(id, "Edit");
        }
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(AnnualLeaveViewModel al) {
            try {
                AnnualLeaveDTO alDto = MapViewModelWithDTO(al);
                await (_annualLeaveService as AnnualLeaveService).EditAsync(alDto);
                return RedirectToAction("Index");
            }
            catch (ValidationException ex) {
                _logger.Warn(ex.Message);
                ModelState.AddModelError(ex.Property, ex.Message);
            }
            ViewBag.Employees = await GetSelectListEmployeesAsync();
            return View("Edit", al);
        }

        private async Task<ActionResult> GetViewAsync(int? id, string viewName) {
            try {
                AnnualLeaveDTO alDto = await _annualLeaveService.FindByIdAsync(id);
                AnnualLeaveViewModel al = MapDTOWithViewModel(alDto);
                return View(viewName, al);
            }
            catch (ValidationException ex) {
                _logger.Warn(ex.Message);
                return View("Error", new string[] { ex.Message });
            }
        }

        private AnnualLeaveViewModel MapDTOWithViewModel(AnnualLeaveDTO alDto) {
            InitializeMapper();
            AnnualLeaveViewModel al = Mapper.Map<AnnualLeaveDTO, AnnualLeaveViewModel>(alDto);
            return al;
        }

        private void InitializeMapper() {
            Mapper.Initialize(cfg => {
                cfg.CreateMap<AnnualLeaveDTO, AnnualLeaveViewModel>();
                cfg.CreateMap<LeaveScheduleDTO, LeaveScheduleViewModel>()
                    .ForMember(ls => ls.AnnualLeaves, opt => opt.Ignore());
                cfg.CreateMap<EmployeeDTO, EmployeeViewModel>()
                    .ForMember(e => e.AnnualLeaves, opt => opt.Ignore())
                    .ForMember(e => e.Birth, opt => opt.Ignore())
                    .ForMember(e => e.BusinessTrips, opt => opt.Ignore())
                    .ForMember(e => e.Contacts, opt => opt.Ignore())
                    .ForMember(e => e.Education, opt => opt.Ignore())
                    .ForMember(e => e.Passport, opt => opt.Ignore());
                cfg.CreateMap<PostDTO, PostViewModel>()
                    .ForMember(p => p.Employees, opt => opt.Ignore());
                cfg.CreateMap<DepartmentDTO, DepartmentViewModel>()
                    .ForMember(d => d.Manager, opt => opt.Ignore())
                    .ForMember(d => d.Organization, opt => opt.Ignore())
                    .ForMember(d => d.Posts, opt => opt.Ignore());
            });
        }

        // Подробная информация об отпуске
        public async Task<ActionResult> Details(int? id) {
            return await GetViewAsync(id, "Details");
        }

        // Удаление отпуска
        public async Task<ActionResult> Delete(int? id) {
            return await GetViewAsync(id, "Delete");
        }
        [HttpPost, ValidateAntiForgeryToken, ActionName("Delete")]
        public async Task<ActionResult> DeleteConfirmed(int id) {
            await _annualLeaveService.DeleteAsync(id);
            return RedirectToAction("Index");
        }

        // Удаление всех отпусков
        public ActionResult DeleteAll() {
            return View("DeleteAll");
        }
        [HttpPost, ValidateAntiForgeryToken, ActionName("DeleteAll")]
        public async Task<ActionResult> DeleteAllConfirmed() {
            await _annualLeaveService.DeleteAllAsync();
            return RedirectToAction("Index");
        }

        // Запись информации об отпусках в JSON-файл
        public async Task<ActionResult> ExportJson() {
            string fullPath = CreateDirectoryToFile("AnnualLeaves.json");
            System.IO.File.Delete(fullPath);
            await _annualLeaveService.ExportJsonAsync(fullPath);
            return File(fullPath, "application/json", "AnnualLeaves.json");
        }

        private string CreateDirectoryToFile(string filename) {
            string dir = Server.MapPath("~/Results/");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            string fullPath = dir + filename;
            return fullPath;
        }

        // Добавление отпусков для тестирования
        public async Task<ActionResult> TestCreate() {
            try {
                await (_annualLeaveService as AnnualLeaveService).TestCreateAsync();
            }
            catch (ValidationException ex) {
                _logger.Warn("No employees");
                return View("Error", new string[] { ex.Message });
            }
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing) {
            _annualLeaveService.Dispose();
            _employeeService.Dispose();
            base.Dispose(disposing);
        }
    }
}
