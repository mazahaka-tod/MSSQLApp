using AutoMapper;
using DiplomMSSQLApp.BLL.DTO;
using DiplomMSSQLApp.BLL.Infrastructure;
using DiplomMSSQLApp.BLL.Interfaces;
using DiplomMSSQLApp.WEB.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        [ActionName("Index")]
        public async Task<ActionResult> IndexAsync(int page = 1)
        {
            // Получаем список отделов
            IEnumerable<DepartmentDTO> dtDto = await departmentService.GetAllAsync();
            dtDto = departmentService.GetPage(dtDto, page);     // Paging
            Mapper.Initialize(cfg => {
                cfg.CreateMap<DepartmentDTO, DepartmentViewModel>()
                    .ForMember(d => d.Employees, opt => opt.Ignore());
            });
            IEnumerable<DepartmentViewModel> departments = Mapper.Map<IEnumerable<DepartmentDTO>, IEnumerable<DepartmentViewModel>>(dtDto);
            return View(new DepartmentListViewModel { Departments = departments, PageInfo = departmentService.PageInfo });
        }

        // Добавление нового отдела
        [ActionName("Create")]
        public async Task<ActionResult> CreateAsync()
        {
            ViewBag.Employees = await GetSelectListAsync();
            return View();
        }
        [HttpPost, ValidateAntiForgeryToken, ActionName("Create")]
        public async Task<ActionResult> CreateAsync(DepartmentViewModel department)
        {
            try
            {
                Mapper.Initialize(cfg => {
                    cfg.CreateMap<DepartmentViewModel, DepartmentDTO>()
                        .ForMember(d => d.Employees, opt => opt.Ignore());
                });
                DepartmentDTO dDto = Mapper.Map<DepartmentViewModel, DepartmentDTO>(department);
                await departmentService.CreateAsync(dDto);
                return RedirectToAction("Index");
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError(ex.Property, ex.Message);
            }
            ViewBag.Employees = await GetSelectListAsync();
            return View(department);
        }

        // Обновление информации об отделе
        [ActionName("Edit")]
        public async Task<ActionResult> EditAsync(int? id)
        {
            try
            {
                DepartmentDTO dDto = await departmentService.FindByIdAsync(id);
                Mapper.Initialize(cfg => cfg.CreateMap<DepartmentDTO, DepartmentViewModel>()
                    .ForMember(dp => dp.Employees, opt => opt.Ignore()));
                DepartmentViewModel d = Mapper.Map<DepartmentDTO, DepartmentViewModel>(dDto);
                ViewBag.Employees = await GetSelectListAsync();
                return View(d);
            }
            catch (ValidationException ex)
            {
                return Content(ex.Message);
            }
        }
        [HttpPost, ValidateAntiForgeryToken, ActionName("Edit")]
        public async Task<ActionResult> EditAsync(DepartmentViewModel d, string old)
        {
            try
            {
                // Обновляем данные отдела
                Mapper.Initialize(cfg => cfg.CreateMap<DepartmentViewModel, DepartmentDTO>());
                DepartmentDTO dDto = Mapper.Map<DepartmentViewModel, DepartmentDTO>(d);
                await departmentService.EditAsync(dDto);
                return RedirectToAction("Index");
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError(ex.Property, ex.Message);
            }
            ViewBag.Employees = await GetSelectListAsync();
            return View(d);
        }

        // Удаление отдела
        [ActionName("Delete")]
        public async Task<ActionResult> DeleteAsync(int? id)
        {
            try
            {
                DepartmentDTO dDto = await departmentService.FindByIdAsync(id);
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
        public async Task<ActionResult> DeleteConfirmedAsync(int id)
        {
            await departmentService.DeleteAsync(id);
            return RedirectToAction("Index");
        }

        // Подробная информация об отделе
        [ActionName("Details")]
        public async Task<ActionResult> DetailsAsync(int? id)
        {
            try
            {
                DepartmentDTO dDto = await departmentService.FindByIdAsync(id);
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
        public ActionResult DeleteAll()
        {
            return View();
        }
        [HttpPost, ValidateAntiForgeryToken, ActionName("DeleteAll")]
        public async Task<ActionResult> DeleteAllAsync()
        {
            await departmentService.DeleteAllAsync();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            employeeService.Dispose();
            departmentService.Dispose();
            base.Dispose(disposing);
        }

        // Получение списка сотрудников
        private async Task<SelectList> GetSelectListAsync()
        {
            IEnumerable<EmployeeDTO> employees = await employeeService.GetAllAsync();
            List<SelectListItem> items = employees.Select(e => new SelectListItem()
            {
                Value = e.LastName + " " + e.FirstName,
                Text = e.LastName + " " + e.FirstName
            }).OrderBy(e => e.Text).ToList();
            return new SelectList(items, "Value", "Text");
        }
    }
}