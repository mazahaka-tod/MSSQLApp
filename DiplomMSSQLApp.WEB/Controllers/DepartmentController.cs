using AutoMapper;
using DiplomMSSQLApp.BLL.DTO;
using DiplomMSSQLApp.BLL.Infrastructure;
using DiplomMSSQLApp.BLL.Interfaces;
using DiplomMSSQLApp.BLL.Services;
using DiplomMSSQLApp.WEB.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace DiplomMSSQLApp.WEB.Controllers
{
    public class DepartmentController : Controller
    {
        private IService<EmployeeDTO> employeeService;
        private IService<DepartmentDTO> departmentService;
        
        public DepartmentController(IService<EmployeeDTO> es, IService<DepartmentDTO> ds)
        {
            employeeService = es;
            departmentService = ds;
        }

        public ActionResult Index(int page = 1)
        {
            // Получаем список отделов
            IEnumerable<DepartmentDTO> dDto = departmentService.GetAll();
            var cnt = dDto.Count();
            // Пагинация (paging)
            dDto = departmentService.GetPage(dDto, page, cnt);

            Mapper.Initialize(cfg => cfg.CreateMap<DepartmentDTO, DepartmentViewModel>());
            var d = Mapper.Map<IEnumerable<DepartmentDTO>, IEnumerable<DepartmentViewModel>>(dDto);

            return View(new DepartmentListViewModel { Departments = d, PageInfo = departmentService.PageInfo });
        }

        // Добавление нового отдела
        public ActionResult Create()
        {
            ViewBag.Employees = new SelectList(employeeService.GetAll().ToList(), "LastName", "LastName");
            return View();
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Create(DepartmentViewModel d)
        {
            try
            {
                Mapper.Initialize(cfg => cfg.CreateMap<DepartmentViewModel, DepartmentDTO>());
                DepartmentDTO dDto = Mapper.Map<DepartmentViewModel, DepartmentDTO>(d);
                departmentService.Create(dDto);
                return RedirectToAction("Index");
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError(ex.Property, ex.Message);
            }
            ViewBag.Employees = new SelectList(employeeService.GetAll().ToList(), "LastName", "LastName");
            return View(d);
        }

        // Обновление информации об отделе
        public ActionResult Edit(int? id)
        {
            try
            {
                DepartmentDTO dDto = departmentService.FindById(id);
                Mapper.Initialize(cfg => cfg.CreateMap<DepartmentDTO, DepartmentViewModel>()
                    .ForMember(dp => dp.Employees, opt => opt.Ignore()));
                DepartmentViewModel d = Mapper.Map<DepartmentDTO, DepartmentViewModel>(dDto);
                ViewBag.Employees = new SelectList(employeeService.GetAll().ToList(), "LastName", "LastName");
                return View(d);
            }
            catch (ValidationException ex)
            {
                return Content(ex.Message);
            }
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Edit(DepartmentViewModel d, string old)
        {
            try
            {
                // Обновляем данные отдела
                Mapper.Initialize(cfg => cfg.CreateMap<DepartmentViewModel, DepartmentDTO>());
                DepartmentDTO dDto = Mapper.Map<DepartmentViewModel, DepartmentDTO>(d);
                departmentService.Edit(dDto);
                // Обновляем данные сотрудников
                (employeeService as EmployeeService).UpdateEmployees(old, dDto);
                return RedirectToAction("Index");
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError(ex.Property, ex.Message);
            }
            ViewBag.Employees = new SelectList(employeeService.GetAll().ToList(), "LastName", "LastName");
            return View(d);
        }

        // Удаление отдела
        public ActionResult Delete(int? id)
        {
            try
            {
                DepartmentDTO dDto = departmentService.FindById(id);
                Mapper.Initialize(cfg =>
                {
                    cfg.CreateMap<DepartmentDTO, DepartmentViewModel>();
                    cfg.CreateMap<EmployeeDTO, EmployeeViewModel>();
                });
                DepartmentViewModel d = Mapper.Map<DepartmentDTO, DepartmentViewModel>(dDto);
                return View(d);
            }
            catch (ValidationException ex)
            {
                return Content(ex.Message);
            }
        }
        [HttpPost, ValidateAntiForgeryToken, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            departmentService.Delete(id);
            return RedirectToAction("Index");
        }

        // Подробная информация об отделе
        public ActionResult Details(int? id)
        {
            try
            {
                DepartmentDTO dDto = departmentService.FindById(id);
                Mapper.Initialize(cfg =>
                {
                    cfg.CreateMap<DepartmentDTO, DepartmentViewModel>();
                    cfg.CreateMap<EmployeeDTO, EmployeeViewModel>();
                });
                DepartmentViewModel d = Mapper.Map<DepartmentDTO, DepartmentViewModel>(dDto);
                return View(d);
            }
            catch (ValidationException ex)
            {
                return Content(ex.Message);
            }
        }

        // Удаление всех отделов
        public ActionResult AllDelete()
        {
            departmentService.DeleteAll();
            return RedirectToAction("Index");
        }
    }
}