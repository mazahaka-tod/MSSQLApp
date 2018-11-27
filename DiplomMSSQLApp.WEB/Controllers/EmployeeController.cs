﻿using AutoMapper;
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
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private IService<EmployeeDTO> _employeeService;
        private IService<DepartmentDTO> _departmentService;
        private IService<PostDTO> _postService;

        public EmployeeController(IService<EmployeeDTO> es, IService<DepartmentDTO> ds, IService<PostDTO> ps) {
            _employeeService = es;
            _departmentService = ds;
            _postService = ps;
        }

        public ActionResult Index(EmployeeFilter filter, string filterAsJsonString, int page = 1) {
            // При использовании функции Paging передается строка filterAsJsonString, а аргумент EmployeeFilter filter == null,
            // поэтому производим декодирование строки filterAsJsonString в объект EmployeeFilter и присваиваем его переменной filter
            if (filterAsJsonString != null)
                filter = System.Web.Helpers.Json.Decode<EmployeeFilter>(filterAsJsonString);
            IEnumerable<EmployeeDTO> eDto = (_employeeService as EmployeeService).Get(filter);
            eDto = _employeeService.GetPage(eDto, page);     // Paging
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
            EmployeeListViewModel model = new EmployeeListViewModel { Employees = employees, Filter = filter, PageInfo = _employeeService.PageInfo };
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest") {
                _logger.Info("Executed async request");
                return PartialView("GetEmployeesData", model);
            }
            _logger.Info("Executed sync request");
            return View("Index", model);
        }

        // Добавление нового сотрудника
        public async Task<ActionResult> Create() {
            int totalNumberOfUnits = (await _postService.GetAllAsync()).Sum(p => p.NumberOfUnits).Value;
            int employeesCount = await _employeeService.CountAsync();
            if (employeesCount >= totalNumberOfUnits) {
                _logger.Warn("No vacancy");
                return View("Error", new string[] { "Нельзя принять сотрудника, если нет свободных вакансий" });
            }
            ViewBag.Posts = await GetSelectListPostsAsync();
            return View("Create");
        }
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(EmployeeViewModel e) {
            try {
                var eDto = MapViewModelWithDTO(e);
                await _employeeService.CreateAsync(eDto);
                return RedirectToAction("Index");
            }
            catch (ValidationException ex) {
                _logger.Warn(ex.Message);
                ModelState.AddModelError(ex.Property, ex.Message);
            }
            ViewBag.Posts = await GetSelectListPostsAsync();
            return View("Create", e);
        }

        private async Task<SelectList> GetSelectListPostsAsync() {
            IEnumerable<PostDTO> posts = await _postService.GetAllAsync();
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
        public async Task<ActionResult> Edit(int? id) {
            ViewBag.Posts = await GetSelectListPostsAsync();
            return await GetViewAsync(id, "Edit");
        }
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(EmployeeViewModel e) {
            try {
                EmployeeDTO eDto = MapViewModelWithDTO(e);
                await _employeeService.EditAsync(eDto);
                return RedirectToAction("Index");
            }
            catch (ValidationException ex) {
                _logger.Warn(ex.Message);
                ModelState.AddModelError(ex.Property, ex.Message);
            }
            ViewBag.Posts = await GetSelectListPostsAsync();
            return View("Edit", e);
        }

        private async Task<ActionResult> GetViewAsync(int? id, string viewName) {
            try {
                EmployeeDTO eDto = await _employeeService.FindByIdAsync(id);
                EmployeeViewModel e = MapDTOWithViewModel(eDto);
                return View(viewName, e);
            }
            catch (ValidationException ex) {
                _logger.Warn(ex.Message);
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
        public async Task<ActionResult> Details(int? id) {
            return await GetViewAsync(id, "Details");
        }

        // Удаление сотрудника
        public async Task<ActionResult> Delete(int? id) {
            return await GetViewAsync(id, "Delete");
        }
        [HttpPost, ValidateAntiForgeryToken, ActionName("Delete")]
        public async Task<ActionResult> DeleteConfirmed(int id) {
            await _employeeService.DeleteAsync(id);
            return RedirectToAction("Index");
        }

        // Удаление всех сотрудников
        public ActionResult DeleteAll() {
            return View("DeleteAll");
        }
        [HttpPost, ValidateAntiForgeryToken, ActionName("DeleteAll")]
        public async Task<ActionResult> DeleteAllConfirmed() {
            await _employeeService.DeleteAllAsync();
            return RedirectToAction("Index");
        }

        // Запись информации о сотрудниках в JSON-файл
        public async Task<ActionResult> ExportJson() {
            string fullPath = CreateDirectoryToFile("Employees.json");
            System.IO.File.Delete(fullPath);
            await _employeeService.ExportJsonAsync(fullPath);
            return RedirectToAction("Index");
        }

        private string CreateDirectoryToFile(string filename) {
            string dir = Server.MapPath("~/Results/");
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
            _employeeService.Dispose();
            _departmentService.Dispose();
            _postService.Dispose();
            base.Dispose(disposing);
        }
    }
}
