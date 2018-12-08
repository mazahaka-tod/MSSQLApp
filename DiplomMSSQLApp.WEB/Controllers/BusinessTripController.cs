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
    public class BusinessTripController : Controller {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private IService<BusinessTripDTO> _businessTripService;
        private IService<EmployeeDTO> _employeeService;

        public BusinessTripController(IService<BusinessTripDTO> bs, IService<EmployeeDTO> es) {
            _businessTripService = bs;
            _employeeService = es;
        }

        public ActionResult Index(BusinessTripFilter filter, string filterAsJsonString, int page = 1) {
            if (filterAsJsonString != null)
                filter = System.Web.Helpers.Json.Decode<BusinessTripFilter>(filterAsJsonString);
            IEnumerable<BusinessTripDTO> btDto = (_businessTripService as BusinessTripService).Get(filter);  // Filter
            btDto = _businessTripService.GetPage(btDto, page);     // Paging
            Mapper.Initialize(cfg => cfg.CreateMap<BusinessTripDTO, BusinessTripViewModel>()
                                        .ForMember(bt => bt.Employees, opt => opt.Ignore()));
            IEnumerable<BusinessTripViewModel> businessTrips = Mapper.Map<IEnumerable<BusinessTripDTO>, IEnumerable<BusinessTripViewModel>>(btDto);
            BusinessTripListViewModel model = new BusinessTripListViewModel {
                BusinessTrips = businessTrips,
                Filter = filter,
                PageInfo = _businessTripService.PageInfo
            };
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest") {
                _logger.Info("Executed async request");
                var transformModel = new {
                    BusinessTrips = model.BusinessTrips.Select(bt => new {
                        bt.Id,
                        Code = bt.Name,
                        DateStart = bt.DateStart.ToString("dd MMMM yyyy"),
                        DateEnd = bt.DateEnd.ToString("dd MMMM yyyy"),
                        bt.Destination
                    }).ToArray(),
                    model.Filter,
                    model.PageInfo
                };
                return Json(transformModel, JsonRequestBehavior.AllowGet);
            }
            _logger.Info("Executed sync request");
            return View("Index", model);
        }

        // Добавление новой командировки
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
        public async Task<ActionResult> Create(BusinessTripViewModel bt) {
            try {
                BusinessTripDTO btDto = MapViewModelWithDTO(bt);
                await (_businessTripService as BusinessTripService).CreateAsync(btDto);
                return RedirectToAction("Index");
            }
            catch (ValidationException ex) {
                _logger.Warn(ex.Message);
                ModelState.AddModelError(ex.Property, ex.Message);
            }
            ViewBag.Employees = await GetSelectListEmployeesAsync();
            return View("Create", bt);
        }

        private async Task<SelectList> GetSelectListEmployeesAsync() {
            IEnumerable<EmployeeDTO> employees = await _employeeService.GetAllAsync();
            IEnumerable<SelectListItem> items = employees.Select(e => new SelectListItem() {
                Value = e.Id.ToString(),
                Text = e.LastName + " " + e.FirstName + " " + e.Patronymic
            }).OrderBy(e => e.Text);
            return new SelectList(items, "Value", "Text");
        }

        private BusinessTripDTO MapViewModelWithDTO(BusinessTripViewModel bt) {
            Mapper.Initialize(cfg => {
                cfg.CreateMap<BusinessTripViewModel, BusinessTripDTO>();
                cfg.CreateMap<EmployeeViewModel, EmployeeDTO>()
                    .ForMember(e => e.Birth, opt => opt.Ignore())
                    .ForMember(e => e.BusinessTrips, opt => opt.Ignore())
                    .ForMember(e => e.Contacts, opt => opt.Ignore())
                    .ForMember(e => e.Education, opt => opt.Ignore())
                    .ForMember(e => e.Passport, opt => opt.Ignore())
                    .ForMember(e => e.Post, opt => opt.Ignore());
            });
            BusinessTripDTO btDto = Mapper.Map<BusinessTripViewModel, BusinessTripDTO>(bt);
            return btDto;
        }

        // Обновление информации о командировке
        public async Task<ActionResult> Edit(int? id) {
            ViewBag.Employees = await GetSelectListEmployeesAsync();
            return await GetViewAsync(id, "Edit");
        }
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(BusinessTripViewModel bt) {
            try {
                BusinessTripDTO btDto = MapViewModelWithDTO(bt);
                await (_businessTripService as BusinessTripService).EditAsync(btDto);
                return RedirectToAction("Index");
            }
            catch (ValidationException ex) {
                _logger.Warn(ex.Message);
                ModelState.AddModelError(ex.Property, ex.Message);
            }
            ViewBag.Employees = await GetSelectListEmployeesAsync();
            return View("Edit", bt);
        }

        private async Task<ActionResult> GetViewAsync(int? id, string viewName) {
            try {
                BusinessTripDTO btDto = await _businessTripService.FindByIdAsync(id);
                BusinessTripViewModel bt = MapDTOWithViewModel(btDto);
                return View(viewName, bt);
            }
            catch (ValidationException ex) {
                _logger.Warn(ex.Message);
                return View("Error", new string[] { ex.Message });
            }
        }

        private BusinessTripViewModel MapDTOWithViewModel(BusinessTripDTO btDTO) {
            Mapper.Initialize(cfg => {
                cfg.CreateMap<BusinessTripDTO, BusinessTripViewModel>();
                cfg.CreateMap<EmployeeDTO, EmployeeViewModel>()
                    .ForMember(e => e.Birth, opt => opt.Ignore())
                    .ForMember(e => e.BusinessTrips, opt => opt.Ignore())
                    .ForMember(e => e.Contacts, opt => opt.Ignore())
                    .ForMember(e => e.Education, opt => opt.Ignore())
                    .ForMember(e => e.Passport, opt => opt.Ignore())
                    .ForMember(e => e.Post, opt => opt.Ignore());
            });
            BusinessTripViewModel bt = Mapper.Map<BusinessTripDTO, BusinessTripViewModel>(btDTO);
            return bt;
        }

        // Подробная информация о командировке
        public async Task<ActionResult> Details(int? id) {
            return await GetViewAsync(id, "Details");
        }

        // Удаление командировки
        public async Task<ActionResult> Delete(int? id) {
            return await GetViewAsync(id, "Delete");
        }
        [HttpPost, ValidateAntiForgeryToken, ActionName("Delete")]
        public async Task<ActionResult> DeleteConfirmed(int id) {
            await _businessTripService.DeleteAsync(id);
            return RedirectToAction("Index");
        }

        // Удаление всех командировок
        public ActionResult DeleteAll() {
            return View("DeleteAll");
        }
        [HttpPost, ValidateAntiForgeryToken, ActionName("DeleteAll")]
        public async Task<ActionResult> DeleteAllConfirmed() {
            await _businessTripService.DeleteAllAsync();
            return RedirectToAction("Index");
        }

        // Запись информации о командировках в JSON-файл
        public async Task<ActionResult> ExportJson() {
            string fullPath = CreateDirectoryToFile("BusinessTrips.json");
            System.IO.File.Delete(fullPath);
            await _businessTripService.ExportJsonAsync(fullPath);
            return File(fullPath, "application/json", "BusinessTrips.json");
        }

        private string CreateDirectoryToFile(string filename) {
            string dir = Server.MapPath("~/Results/");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            string fullPath = dir + filename;
            return fullPath;
        }

        // Добавление командировок для тестирования
        public async Task<ActionResult> TestCreate() {
            try {
                await (_businessTripService as BusinessTripService).TestCreateAsync();
            }
            catch (ValidationException ex) {
                _logger.Warn("No employees");
                return View("Error", new string[] { ex.Message });
            }
            return RedirectToAction("Index");
        }

        // Частичное представление добавляет выпадающий список сотрудников
        public async Task<ActionResult> AddEmployeeAsync(int index) {
            ViewBag.Index = index;
            ViewBag.Employees = await GetSelectListEmployeesAsync();
            return PartialView("AddEmployee");
        }

        protected override void Dispose(bool disposing) {
            _businessTripService.Dispose();
            _employeeService.Dispose();
            base.Dispose(disposing);
        }
    }
}
