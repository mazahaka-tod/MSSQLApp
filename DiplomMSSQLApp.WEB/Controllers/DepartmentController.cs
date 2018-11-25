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
        private IService<OrganizationDTO> organizationService;

        public DepartmentController(IService<EmployeeDTO> es, IService<DepartmentDTO> ds, IService<OrganizationDTO> os) {
            employeeService = es;
            departmentService = ds;
            organizationService = os;
        }

        [ActionName("Index")]
        public async Task<ActionResult> IndexAsync(int page = 1) {
            IEnumerable<DepartmentDTO> dDto = await departmentService.GetAllAsync();
            dDto = departmentService.GetPage(dDto, page);     // Paging
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
            return View("Index", new DepartmentListViewModel { Departments = departments, PageInfo = departmentService.PageInfo });
        }

        // Добавление нового отдела
        [ActionName("Create")]
        public async Task<ActionResult> CreateAsync() {
            ViewBag.Organizations = await GetSelectListOrganizationsAsync();
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
            ViewBag.Organizations = await GetSelectListOrganizationsAsync();
            ViewBag.Employees = await GetSelectListEmployeesAsync();
            return View("Create", d);
        }

        private async Task<SelectList> GetSelectListOrganizationsAsync() {
            IEnumerable<OrganizationDTO> organizations = await organizationService.GetAllAsync();
            return new SelectList(organizations.OrderBy(o => o.Name), "Id", "Name");
        }

        private async Task<SelectList> GetSelectListEmployeesAsync() {
            IEnumerable<EmployeeDTO> employees = await employeeService.GetAllAsync();
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
        [ActionName("Edit")]
        public async Task<ActionResult> EditAsync(int? id) {
            ViewBag.Organizations = await GetSelectListOrganizationsAsync();
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
            ViewBag.Organizations = await GetSelectListOrganizationsAsync();
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
            catch (ValidationException ex) {
                return View("Error", new string[] { ex.Message });
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
                return View("Error", new string[] { "Нельзя удалить отдел, пока в нем есть хотя бы одна должность" });
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
