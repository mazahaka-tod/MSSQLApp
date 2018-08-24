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
    public class BusinessTripController : Controller
    {
        private IService<BusinessTripDTO> businessTripService;
        private IService<EmployeeDTO> employeeService;

        public BusinessTripController(IService<BusinessTripDTO> bs, IService<EmployeeDTO> es)
        {
            businessTripService = bs;
            employeeService = es;
        }

        public ActionResult Index(int page = 1)
        {
            // Получаем список командировок
            IEnumerable<BusinessTripDTO> bDto = businessTripService.GetAll();
            var cnt = bDto.Count();
            // Пагинация (paging)
            bDto = businessTripService.GetPage(bDto, page, cnt);

            Mapper.Initialize(cfg => {
                cfg.CreateMap<BusinessTripDTO, BusinessTripViewModel>();
                cfg.CreateMap<EmployeeDTO, EmployeeViewModel>();
            });
            var b = Mapper.Map<IEnumerable<BusinessTripDTO>, IEnumerable<BusinessTripViewModel>>(bDto);

            return View(new BusinessTripListViewModel { BusinessTrips = b, PageInfo = businessTripService.PageInfo });
        }

        // Добавление новой командировки
        public ActionResult Create()
        {
            ViewBag.Employees = GetSelectList();
            return View();
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Create(BusinessTripViewModel bt, int[] ids)
        {
            try
            {
                Mapper.Initialize(cfg => {
                    cfg.CreateMap<BusinessTripViewModel, BusinessTripDTO>();
                    cfg.CreateMap<EmployeeViewModel, EmployeeDTO>();
                    cfg.CreateMap<PostViewModel, PostDTO>();
                    cfg.CreateMap<DepartmentViewModel, DepartmentDTO>();
                });
                BusinessTripDTO bDto = Mapper.Map<BusinessTripViewModel, BusinessTripDTO>(bt);
                (businessTripService as BusinessTripService).Create(bDto, ids);
                return RedirectToAction("Index");
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError(ex.Property, ex.Message);
            }
            ViewBag.Employees = GetSelectList();
            return View(bt);
        }

        // Обновление информации о командировке
        public ActionResult Edit(int? id)
        {
            try
            {
                BusinessTripDTO bDto = businessTripService.FindById(id);
                Mapper.Initialize(cfg => {
                    cfg.CreateMap<BusinessTripDTO, BusinessTripViewModel>();
                    cfg.CreateMap<EmployeeDTO, EmployeeViewModel>();
                    cfg.CreateMap<PostDTO, PostViewModel>();
                    cfg.CreateMap<DepartmentDTO, DepartmentViewModel>();
                });
                BusinessTripViewModel bt = Mapper.Map<BusinessTripDTO, BusinessTripViewModel>(bDto);
                ViewBag.Employees = GetSelectList();
                return View(bt);
            }
            catch (ValidationException ex)
            {
                return Content(ex.Message);
            }
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Edit(BusinessTripViewModel bt, int[] ids)
        {
            try
            {
                Mapper.Initialize(cfg => {
                    cfg.CreateMap<BusinessTripViewModel, BusinessTripDTO>();
                    cfg.CreateMap<EmployeeViewModel, EmployeeDTO>();
                    cfg.CreateMap<PostViewModel, PostDTO>();
                    cfg.CreateMap<DepartmentViewModel, DepartmentDTO>();
                });
                BusinessTripDTO bDto = Mapper.Map<BusinessTripViewModel, BusinessTripDTO>(bt);
                (businessTripService as BusinessTripService).Edit(bDto, ids);
                return RedirectToAction("Index");
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError(ex.Property, ex.Message);
            }
            ViewBag.Employees = GetSelectList();
            return View(bt);
        }

        // Получение списка сотрудников
        private SelectList GetSelectList()
        {
            List<SelectListItem> items = employeeService.GetAll().Select(e => new SelectListItem()
            {
                Value = e.Id.ToString(),
                Text = e.LastName + " " + e.FirstName
            }).ToList();
            return new SelectList(items, "Value", "Text");
        }

        public ActionResult AddEmployee(int index)
        {
            ViewBag.Index = index;
            ViewBag.Employees = GetSelectList();
            return PartialView();
        }

        // Удаление командировки
        public ActionResult Delete(int? id)
        {
            try
            {
                BusinessTripDTO bDto = businessTripService.FindById(id);
                Mapper.Initialize(cfg => {
                    cfg.CreateMap<BusinessTripDTO, BusinessTripViewModel>();
                    cfg.CreateMap<EmployeeDTO, EmployeeViewModel>()
                        .ForMember(d => d.Post, opt => opt.Ignore())
                        .ForMember(d => d.Department, opt => opt.Ignore());
                });
                BusinessTripViewModel b = Mapper.Map<BusinessTripDTO, BusinessTripViewModel>(bDto);
                return View(b);
            }
            catch (ValidationException ex)
            {
                return Content(ex.Message);
            }
        }
        [HttpPost, ValidateAntiForgeryToken, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            businessTripService.Delete(id);
            return RedirectToAction("Index");
        }

        // Подробная информация о командировке
        public ActionResult Details(int? id)
        {
            try
            {
                BusinessTripDTO bDto = businessTripService.FindById(id);
                Mapper.Initialize(cfg => {
                    cfg.CreateMap<BusinessTripDTO, BusinessTripViewModel>();
                    cfg.CreateMap<EmployeeDTO, EmployeeViewModel>()
                        .ForMember(d => d.Post, opt => opt.Ignore())
                        .ForMember(d => d.Department, opt => opt.Ignore());
                });
                BusinessTripViewModel b = Mapper.Map<BusinessTripDTO, BusinessTripViewModel>(bDto);
                return View(b);
            }
            catch (ValidationException ex)
            {
                return Content(ex.Message);
            }
        }

        // Удаление всех командировок
        public ActionResult AllDelete()
        {
            businessTripService.DeleteAll();
            return RedirectToAction("Index");
        }
    }
}