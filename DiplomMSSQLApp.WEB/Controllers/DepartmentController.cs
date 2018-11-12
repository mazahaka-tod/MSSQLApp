using AutoMapper;
using DiplomMSSQLApp.BLL.DTO;
using DiplomMSSQLApp.BLL.Infrastructure;
using DiplomMSSQLApp.BLL.Interfaces;
using DiplomMSSQLApp.WEB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace DiplomMSSQLApp.WEB.Controllers {
    [HandleError]
    [Authorize]
    public class DepartmentController : Controller {
        private IService<EmployeeDTO> employeeService;
        private IService<DepartmentDTO> departmentService;

        public DepartmentController(IService<EmployeeDTO> es, IService<DepartmentDTO> ds) {
            employeeService = es;
            departmentService = ds;
        }

        [ActionName("Index")]
        public async Task<ActionResult> IndexAsync(int page = 1) {
            IEnumerable<DepartmentDTO> dDto = await departmentService.GetAllAsync();
            dDto = departmentService.GetPage(dDto, page);     // Paging
            Mapper.Initialize(cfg => cfg.CreateMap<DepartmentDTO, DepartmentViewModel>()
                                        .ForMember(d => d.Employees, opt => opt.Ignore()));
            IEnumerable<DepartmentViewModel> departments = Mapper.Map<IEnumerable<DepartmentDTO>, IEnumerable<DepartmentViewModel>>(dDto);
            return View("Index", new DepartmentListViewModel { Departments = departments, PageInfo = departmentService.PageInfo });
        }

        // Добавление нового отдела
        [ActionName("Create")]
        public async Task<ActionResult> CreateAsync() {
            ViewBag.Employees = await GetSelectListEmployeesAsync();
            return View("Create");
        }
        [HttpPost, ValidateAntiForgeryToken, ActionName("Create")]
        public async Task<ActionResult> CreateAsync(DepartmentViewModel d) {
            try {
                DepartmentDTO dDto = MapViewModelWithDTO(d);
                await departmentService.CreateAsync(dDto);
                return RedirectToAction("Index");
            }
            catch (ValidationException ex) {
                ModelState.AddModelError(ex.Property, ex.Message);
            }
            ViewBag.Employees = await GetSelectListEmployeesAsync();
            return View("Create", d);
        }

        private async Task<SelectList> GetSelectListEmployeesAsync() {
            IEnumerable<EmployeeDTO> employees = await employeeService.GetAllAsync();
            IEnumerable<SelectListItem> items = employees.Select(e => new SelectListItem() {
                                                    Value = e.LastName + " " + e.FirstName,
                                                    Text = e.LastName + " " + e.FirstName
                                                }).OrderBy(e => e.Text);
            return new SelectList(items, "Value", "Text");
        }

        private DepartmentDTO MapViewModelWithDTO(DepartmentViewModel d) {
            Mapper.Initialize(cfg => {
                cfg.CreateMap<DepartmentViewModel, DepartmentDTO>();
                cfg.CreateMap<EmployeeViewModel, EmployeeDTO>()
                    .ForMember(e => e.BusinessTrips, opt => opt.Ignore())
                    .ForMember(e => e.Department, opt => opt.Ignore())
                    .ForMember(e => e.Post, opt => opt.Ignore());
            });
            DepartmentDTO dDto = Mapper.Map<DepartmentViewModel, DepartmentDTO>(d);
            return dDto;
        }

        // Обновление информации об отделе
        [ActionName("Edit")]
        public async Task<ActionResult> EditAsync(int? id) {
            ViewBag.Employees = await GetSelectListEmployeesAsync();
            return await GetViewAsync(id, "Edit");
        }
        [HttpPost, ValidateAntiForgeryToken, ActionName("Edit")]
        public async Task<ActionResult> EditAsync(DepartmentViewModel d) {
            try {
                DepartmentDTO dDto = MapViewModelWithDTO(d);
                await departmentService.EditAsync(dDto);
                return RedirectToAction("Index");
            }
            catch (ValidationException ex) {
                ModelState.AddModelError(ex.Property, ex.Message);
            }
            ViewBag.Employees = await GetSelectListEmployeesAsync();
            return View("Edit", d);
        }

        private async Task<ActionResult> GetViewAsync(int? id, string viewName) {
            try {
                DepartmentDTO dDto = await departmentService.FindByIdAsync(id);
                DepartmentViewModel d = MapDTOWithViewModel(dDto);
                return View(viewName, d);
            }
            catch (ValidationException ex) {
                return View("Error", new string[] { ex.Message });
            }
        }

        private DepartmentViewModel MapDTOWithViewModel(DepartmentDTO dDto) {
            Mapper.Initialize(cfg => {
                cfg.CreateMap<DepartmentDTO, DepartmentViewModel>();
                cfg.CreateMap<EmployeeDTO, EmployeeViewModel>()
                    .ForMember(e => e.BusinessTrips, opt => opt.Ignore())
                    .ForMember(e => e.Department, opt => opt.Ignore())
                    .ForMember(e => e.Post, opt => opt.Ignore());
            });
            DepartmentViewModel d = Mapper.Map<DepartmentDTO, DepartmentViewModel>(dDto);
            return d;
        }

        // Подробная информация об отделе
        [ActionName("Details")]
        public async Task<ActionResult> DetailsAsync(int? id) {
            return await GetViewAsync(id, "Details");
        }

        // Удаление отдела
        [ActionName("Delete")]
        public async Task<ActionResult> DeleteAsync(int? id) {
            return await GetViewAsync(id, "Delete");
        }
        [HttpPost, ValidateAntiForgeryToken, ActionName("Delete")]
        public async Task<ActionResult> DeleteConfirmedAsync(int id) {
            try {
                await departmentService.DeleteAsync(id);
            }
            catch (Exception) {
                return View("Error", new string[] { "Нельзя удалить отдел, пока в нем есть хотя бы одна должность или работает хотя бы один сотрудник." });
            }
            return RedirectToAction("Index");
        }

        // Удаление всех отделов
        public ActionResult DeleteAll() {
            return View("DeleteAll");
        }
        [HttpPost, ValidateAntiForgeryToken, ActionName("DeleteAll")]
        public async Task<ActionResult> DeleteAllAsync() {
            try {
                await departmentService.DeleteAllAsync();
            }
            catch (Exception) {
                return View("Error", new string[] { "Нельзя удалить отдел, пока в нем есть хотя бы одна должность или работает хотя бы один сотрудник." });
            }
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing) {
            employeeService.Dispose();
            departmentService.Dispose();
            base.Dispose(disposing);
        }
    }
}