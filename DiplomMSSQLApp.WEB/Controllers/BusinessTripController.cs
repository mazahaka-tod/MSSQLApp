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

namespace DiplomMSSQLApp.WEB.Controllers
{
    public class BusinessTripController : Controller
    {
        private IService<BusinessTripDTO> businessTripService;
        private IService<EmployeeDTO> employeeService;

        public BusinessTripController(IService<BusinessTripDTO> bs, IService<EmployeeDTO> es)
        {
            businessTripService = bs;
            employeeService = es;
        }

        [ActionName("Index")]
        public async Task<ActionResult> IndexAsync(int page = 1)
        {
            // Получаем список командировок
            IEnumerable<BusinessTripDTO> bDto = await businessTripService.GetAllAsync();
            bDto = businessTripService.GetPage(bDto, page);     // Paging
            Mapper.Initialize(cfg => {
                cfg.CreateMap<BusinessTripDTO, BusinessTripViewModel>()
                    .ForMember(bt => bt.Employees, opt => opt.Ignore());
            });
            IEnumerable<BusinessTripViewModel> businessTrips = Mapper.Map<IEnumerable<BusinessTripDTO>, IEnumerable<BusinessTripViewModel>>(bDto);
            return View(new BusinessTripListViewModel { BusinessTrips = businessTrips, PageInfo = businessTripService.PageInfo });
        }

        // Добавление новой командировки
        [ActionName("Create")]
        public async Task<ActionResult> CreateAsync()
        {
            ViewBag.Employees = await GetSelectListAsync();
            return View();
        }
        [HttpPost, ValidateAntiForgeryToken, ActionName("Create")]
        public async Task<ActionResult> CreateAsync(BusinessTripViewModel bt, int[] ids)
        {
            try
            {
                BusinessTripDTO btDto = InitializeMapper(bt);
                await (businessTripService as BusinessTripService).CreateAsync(btDto, ids);
                return RedirectToAction("Index");
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError(ex.Property, ex.Message);
            }
            ViewBag.Employees = await GetSelectListAsync();
            return View(bt);
        }

        // Обновление информации о командировке
        [ActionName("Edit")]
        public async Task<ActionResult> EditAsync(int? id)
        {
            try
            {
                BusinessTripDTO btDto = await businessTripService.FindByIdAsync(id);
                BusinessTripViewModel bt = InitializeMapper(btDto);
                ViewBag.Employees = await GetSelectListAsync();
                return View(bt);
            }
            catch (ValidationException ex)
            {
                return Content(ex.Message);
            }
        }
        [HttpPost, ValidateAntiForgeryToken, ActionName("Edit")]
        public async Task<ActionResult> EditAsync(BusinessTripViewModel bt, int[] ids)
        {
            try
            {
                BusinessTripDTO btDto = InitializeMapper(bt);
                await (businessTripService as BusinessTripService).EditAsync(btDto, ids);
                return RedirectToAction("Index");
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError(ex.Property, ex.Message);
            }
            ViewBag.Employees = await GetSelectListAsync();
            return View(bt);
        }

        // Частичное представление
        public async Task<ActionResult> AddEmployeeAsync(int index)
        {
            ViewBag.Index = index;
            ViewBag.Employees = await GetSelectListAsync();
            return PartialView("AddEmployee");
        }

        // Удаление командировки
        [ActionName("Delete")]
        public async Task<ActionResult> DeleteAsync(int? id)
        {
            try
            {
                BusinessTripDTO btDto = await businessTripService.FindByIdAsync(id);
                BusinessTripViewModel businessTrip = InitializeMapper(btDto);
                return View(businessTrip);
            }
            catch (ValidationException ex)
            {
                return Content(ex.Message);
            }
        }
        [HttpPost, ValidateAntiForgeryToken, ActionName("Delete")]
        public async Task<ActionResult> DeleteConfirmedAsync(int id)
        {
            await businessTripService.DeleteAsync(id);
            return RedirectToAction("Index");
        }

        // Подробная информация о командировке
        [ActionName("Details")]
        public async Task<ActionResult> DetailsAsync(int? id)
        {
            try
            {
                BusinessTripDTO btDto = await businessTripService.FindByIdAsync(id);
                BusinessTripViewModel businessTrip = InitializeMapper(btDto);
                return View(businessTrip);
            }
            catch (ValidationException ex)
            {
                return Content(ex.Message);
            }
        }

        // Удаление всех командировок
        public ActionResult DeleteAll()
        {
            return View();
        }
        [HttpPost, ValidateAntiForgeryToken, ActionName("DeleteAll")]
        public async Task<ActionResult> DeleteAllAsync()
        {
            await businessTripService.DeleteAllAsync();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            businessTripService.Dispose();
            employeeService.Dispose();
            base.Dispose(disposing);
        }

        // Получение списка сотрудников
        private async Task<SelectList> GetSelectListAsync()
        {
            IEnumerable<EmployeeDTO> employees = await employeeService.GetAllAsync();
            List<SelectListItem> items = employees.Select(e => new SelectListItem()
            {
                Value = e.Id.ToString(),
                Text = e.LastName + " " + e.FirstName
            }).OrderBy(e => e.Text).ToList();
            return new SelectList(items, "Value", "Text");
        }

        private BusinessTripViewModel InitializeMapper(BusinessTripDTO btDTO)
        {
            Mapper.Initialize(cfg => {
                cfg.CreateMap<BusinessTripDTO, BusinessTripViewModel>();
                cfg.CreateMap<EmployeeDTO, EmployeeViewModel>()
                    .ForMember(e => e.Department, opt => opt.Ignore())
                    .ForMember(e => e.Post, opt => opt.Ignore());
            });
            return Mapper.Map<BusinessTripDTO, BusinessTripViewModel>(btDTO);
        }

        private BusinessTripDTO InitializeMapper(BusinessTripViewModel bt)
        {
            Mapper.Initialize(cfg => {
                cfg.CreateMap<BusinessTripViewModel, BusinessTripDTO>();
                cfg.CreateMap<EmployeeViewModel, EmployeeDTO>()
                    .ForMember(e => e.Department, opt => opt.Ignore())
                    .ForMember(e => e.Post, opt => opt.Ignore());
            });
            return Mapper.Map<BusinessTripViewModel, BusinessTripDTO>(bt);
        }
    }
}