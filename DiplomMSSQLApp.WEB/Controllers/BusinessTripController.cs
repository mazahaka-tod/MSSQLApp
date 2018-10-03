using AutoMapper;
using DiplomMSSQLApp.BLL.DTO;
using DiplomMSSQLApp.BLL.Infrastructure;
using DiplomMSSQLApp.BLL.Interfaces;
using DiplomMSSQLApp.BLL.Services;
using DiplomMSSQLApp.WEB.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace DiplomMSSQLApp.WEB.Controllers {
    [HandleError]
    public class BusinessTripController : Controller {
        private IService<BusinessTripDTO> businessTripService;
        private IService<EmployeeDTO> employeeService;

        public BusinessTripController(IService<BusinessTripDTO> bs, IService<EmployeeDTO> es) {
            businessTripService = bs;
            employeeService = es;
        }

        [ActionName("Index")]
        public async Task<ActionResult> IndexAsync(int page = 1) {
            IEnumerable<BusinessTripDTO> btDto = await businessTripService.GetAllAsync();
            btDto = businessTripService.GetPage(btDto, page);     // Paging
            Mapper.Initialize(cfg => cfg.CreateMap<BusinessTripDTO, BusinessTripViewModel>()
                                        .ForMember(bt => bt.Employees, opt => opt.Ignore()));
            IEnumerable<BusinessTripViewModel> businessTrips = Mapper.Map<IEnumerable<BusinessTripDTO>, IEnumerable<BusinessTripViewModel>>(btDto);
            return View("Index", new BusinessTripListViewModel { BusinessTrips = businessTrips, PageInfo = businessTripService.PageInfo });
        }

        // Добавление новой командировки
        [ActionName("Create")]
        public async Task<ActionResult> CreateAsync() {
            ViewBag.Employees = await GetSelectListEmployeesAsync();
            return View("Create");
        }
        [HttpPost, ValidateAntiForgeryToken, ActionName("Create")]
        public async Task<ActionResult> CreateAsync(BusinessTripViewModel bt, int[] ids) {
            try {
                BusinessTripDTO btDto = MapViewModelWithDTO(bt);
                await (businessTripService as BusinessTripService).CreateAsync(btDto, ids);
                return RedirectToAction("Index");
            }
            catch (ValidationException ex) {
                ModelState.AddModelError(ex.Property, ex.Message);
            }
            ViewBag.Employees = await GetSelectListEmployeesAsync();
            return View("Create", bt);
        }

        private async Task<SelectList> GetSelectListEmployeesAsync() {
            IEnumerable<EmployeeDTO> employees = await employeeService.GetAllAsync();
            IEnumerable<SelectListItem> items = employees.Select(e => new SelectListItem() {
                                                    Value = e.Id.ToString(),
                                                    Text = e.LastName + " " + e.FirstName
                                                }).OrderBy(e => e.Text);
            return new SelectList(items, "Value", "Text");
        }

        private BusinessTripDTO MapViewModelWithDTO(BusinessTripViewModel bt) {
            Mapper.Initialize(cfg => {
                cfg.CreateMap<BusinessTripViewModel, BusinessTripDTO>();
                cfg.CreateMap<EmployeeViewModel, EmployeeDTO>()
                    .ForMember(e => e.BusinessTrips, opt => opt.Ignore())
                    .ForMember(e => e.Department, opt => opt.Ignore())
                    .ForMember(e => e.Post, opt => opt.Ignore());
            });
            BusinessTripDTO btDto = Mapper.Map<BusinessTripViewModel, BusinessTripDTO>(bt);
            return btDto;
        }

        // Обновление информации о командировке
        [ActionName("Edit")]
        public async Task<ActionResult> EditAsync(int? id) {
            ViewBag.Employees = await GetSelectListEmployeesAsync();
            return await GetViewAsync(id, "Edit");
        }
        [HttpPost, ValidateAntiForgeryToken, ActionName("Edit")]
        public async Task<ActionResult> EditAsync(BusinessTripViewModel bt, int[] ids) {
            try {
                BusinessTripDTO btDto = MapViewModelWithDTO(bt);
                await (businessTripService as BusinessTripService).EditAsync(btDto, ids);
                return RedirectToAction("Index");
            }
            catch (ValidationException ex) {
                ModelState.AddModelError(ex.Property, ex.Message);
            }
            ViewBag.Employees = await GetSelectListEmployeesAsync();
            return View("Edit", bt);
        }

        private async Task<ActionResult> GetViewAsync(int? id, string viewName) {
            try {
                BusinessTripDTO btDto = await businessTripService.FindByIdAsync(id);
                BusinessTripViewModel bt = MapDTOWithViewModel(btDto);
                return View(viewName, bt);
            }
            catch (ValidationException ex) {
                return View("CustomError", (object)ex.Message);
            }
        }

        private BusinessTripViewModel MapDTOWithViewModel(BusinessTripDTO btDTO) {
            Mapper.Initialize(cfg => {
                cfg.CreateMap<BusinessTripDTO, BusinessTripViewModel>();
                cfg.CreateMap<EmployeeDTO, EmployeeViewModel>()
                    .ForMember(e => e.BusinessTrips, opt => opt.Ignore())
                    .ForMember(e => e.Department, opt => opt.Ignore())
                    .ForMember(e => e.Post, opt => opt.Ignore());
            });
            BusinessTripViewModel bt = Mapper.Map<BusinessTripDTO, BusinessTripViewModel>(btDTO);
            return bt;
        }

        // Подробная информация о командировке
        [ActionName("Details")]
        public async Task<ActionResult> DetailsAsync(int? id) {
            return await GetViewAsync(id, "Details");
        }

        // Удаление командировки
        [ActionName("Delete")]
        public async Task<ActionResult> DeleteAsync(int? id) {
            return await GetViewAsync(id, "Delete");
        }
        [HttpPost, ValidateAntiForgeryToken, ActionName("Delete")]
        public async Task<ActionResult> DeleteConfirmedAsync(int id) {
            await businessTripService.DeleteAsync(id);
            return RedirectToAction("Index");
        }

        // Удаление всех командировок
        public ActionResult DeleteAll() {
            return View("DeleteAll");
        }
        [HttpPost, ValidateAntiForgeryToken, ActionName("DeleteAll")]
        public async Task<ActionResult> DeleteAllAsync() {
            await businessTripService.DeleteAllAsync();
            return RedirectToAction("Index");
        }

        // Частичное представление добавляет выпадающий список сотрудников
        public async Task<ActionResult> AddEmployeeAsync(int index) {
            ViewBag.Index = index;
            ViewBag.Employees = await GetSelectListEmployeesAsync();
            return PartialView("AddEmployee");
        }

        protected override void Dispose(bool disposing) {
            businessTripService.Dispose();
            employeeService.Dispose();
            base.Dispose(disposing);
        }
    }
}
