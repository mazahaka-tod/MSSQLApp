using AutoMapper;
using DiplomMSSQLApp.BLL.BusinessModels;
using DiplomMSSQLApp.BLL.DTO;
using DiplomMSSQLApp.BLL.Infrastructure;
using DiplomMSSQLApp.BLL.Interfaces;
using DiplomMSSQLApp.WEB.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace DiplomMSSQLApp.WEB.Controllers
{
    public class HomeController : Controller
    {
        private IService<EmployeeDTO> employeeService;
        private IService<PostDTO> postService;
        private IService<DepartmentDTO> departmentService;

        public HomeController(IService<EmployeeDTO> es, IService<PostDTO> ps, IService<DepartmentDTO> ds)
        {
            employeeService = es;
            postService = ps;
            departmentService = ds;
        }

        public ActionResult Index(EmployeeFilter f, string str, int page = 1)
        {
            // Условие истинно, когда пользователь использует paging,
            // в этом случае передается строка str, а фильтр EmployeeFilter f нет,
            // поэтому производим декодирование строки в объект EmployeeFilter и присваиваем его фильтру f
            if (str != null) f = System.Web.Helpers.Json.Decode<EmployeeFilter>(str);
            string path = Server.MapPath("~/Results/Employee/Filter.txt");
            // Получаем список сотрудников
            int cnt = 0;
            IEnumerable<EmployeeDTO> eDto = employeeService.Get(f, path, ref cnt);     // фильтруем данные
            // Пагинация (paging)
            eDto = employeeService.GetPage(eDto, page, cnt);

            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<BusinessTripDTO, BusinessTripViewModel>()
                    .ForMember(bt => bt.Employees, opt => opt.Ignore());
                cfg.CreateMap<EmployeeDTO, EmployeeViewModel>();
                cfg.CreateMap<DepartmentDTO, DepartmentViewModel>()
                    .ForMember(d => d.Employees, opt => opt.Ignore());
                cfg.CreateMap<PostDTO, PostViewModel>()
                    .ForMember(p => p.Employees, opt => opt.Ignore());
            });
            var e = Mapper.Map<IEnumerable<EmployeeDTO>, IEnumerable<EmployeeViewModel>>(eDto);

            return View(new EmployeeListViewModel { Employees = e, Filter = f, PageInfo = employeeService.PageInfo });
        }

        // Добавление нового сотрудника
        public ActionResult Create()
        {
            ViewBag.Posts = new SelectList(postService.GetAll().ToList(), "Id", "Title");
            ViewBag.Departments = new SelectList(departmentService.GetAll().ToList(), "Id", "DepartmentName");
            return View();
        }
        [HttpPost, ValidateAntiForgeryToken, ActionName("Create")]
        public async Task<ActionResult> CreateAsync(EmployeeViewModel e)
        {
            try
            {
                Mapper.Initialize(cfg => cfg.CreateMap<EmployeeViewModel, EmployeeDTO>());
                var eDto = Mapper.Map<EmployeeViewModel, EmployeeDTO>(e);
                await employeeService.CreateAsync(eDto);
                return RedirectToAction("Index");
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError(ex.Property, ex.Message);
            }
            ViewBag.Posts = new SelectList(postService.GetAll().ToList(), "Id", "Title");
            ViewBag.Departments = new SelectList(departmentService.GetAll().ToList(), "Id", "DepartmentName");
            return View(e);
        }

        // Обновление информации о сотруднике
        public ActionResult Edit(int? id)
        {
            try
            {
                EmployeeDTO eDto = employeeService.FindById(id);
                Mapper.Initialize(cfg => {
                    cfg.CreateMap<EmployeeDTO, EmployeeViewModel>()
                        .ForMember(d => d.BusinessTrips, opt => opt.Ignore());
                    cfg.CreateMap<PostDTO, PostViewModel>();
                    cfg.CreateMap<DepartmentDTO, DepartmentViewModel>();
                });
                EmployeeViewModel e = Mapper.Map<EmployeeDTO, EmployeeViewModel>(eDto);
                ViewBag.Posts = new SelectList(postService.GetAll().ToList(), "Id", "Title");
                ViewBag.Departments = new SelectList(departmentService.GetAll().ToList(), "Id", "DepartmentName");
                return View(e);
            }
            catch (ValidationException ex)
            {
                return Content(ex.Message);
            }
        }
        [HttpPost, ValidateAntiForgeryToken, ActionName("Edit")]
        public async Task<ActionResult> EditAsync(EmployeeViewModel e)
        {
            try
            {
                // Обновляем данные сотрудника
                Mapper.Initialize(cfg => cfg.CreateMap<EmployeeViewModel, EmployeeDTO>());
                EmployeeDTO eDto = Mapper.Map<EmployeeViewModel, EmployeeDTO>(e);
                await employeeService.EditAsync(eDto);
                return RedirectToAction("Index");
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError(ex.Property, ex.Message);
            }
            ViewBag.Posts = new SelectList(postService.GetAll().ToList(), "Id", "Title");
            ViewBag.Departments = new SelectList(departmentService.GetAll().ToList(), "Id", "DepartmentName");
            return View(e);
        }

        public ActionResult About()
        {
            ViewBag.Message = "";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "";

            return View();
        }

        // Частичное представление
        public ActionResult AddTextBox(string name)
        {
            ViewBag.Name = name;
            return PartialView();
        }

        // Удаление сотрудника
        public ActionResult Delete(int? id)
        {
            try
            {
                EmployeeDTO eDto = employeeService.FindById(id);
                Mapper.Initialize(cfg => {
                    cfg.CreateMap<BusinessTripDTO, BusinessTripViewModel>();
                    cfg.CreateMap<EmployeeDTO, EmployeeViewModel>();
                    cfg.CreateMap<PostDTO, PostViewModel>().MaxDepth(0);
                    cfg.CreateMap<DepartmentDTO, DepartmentViewModel>().MaxDepth(0);
                });
                EmployeeViewModel e = Mapper.Map<EmployeeDTO, EmployeeViewModel>(eDto);
                return View(e);
            }
            catch (ValidationException ex)
            {
                return Content(ex.Message);
            }
        }
        [HttpPost, ValidateAntiForgeryToken, ActionName("Delete")]
        public async Task<ActionResult> DeleteConfirmedAsync(int id)
        {
            await employeeService.DeleteAsync(id);
            return RedirectToAction("Index");
        }

        // Подробная информация о сотруднике
        public ActionResult Details(int? id)
        {
            try
            {
                EmployeeDTO eDto = employeeService.FindById(id);
                Mapper.Initialize(cfg => {
                    cfg.CreateMap<BusinessTripDTO, BusinessTripViewModel>();
                    cfg.CreateMap<EmployeeDTO, EmployeeViewModel>();
                    cfg.CreateMap<PostDTO, PostViewModel>().MaxDepth(0);
                    cfg.CreateMap<DepartmentDTO, DepartmentViewModel>().MaxDepth(0);
                });
                EmployeeViewModel e = Mapper.Map<EmployeeDTO, EmployeeViewModel>(eDto);
                return View(e);
            }
            catch (ValidationException ex)
            {
                return Content(ex.Message);
            }
        }

        // Удаление всех сотрудников
        [ActionName("DeleteAll")]
        public async Task<ActionResult> DeleteAllAsync()
        {
            await employeeService.DeleteAllAsync();
            return RedirectToAction("Index");
        }

        // Запись базы в файл
        public ActionResult ExportJson()
        {
            string path = Server.MapPath("~/Results/Employee/Json.txt");
            IEnumerable<EmployeeDTO> eDto = employeeService.GetAll();
            var employees = eDto.Select(e => new
            {
                SurName = e.LastName,
                FirstName = e.FirstName,
                Email = e.Email,
                PhoneNumber = e.PhoneNumber,
                Addresses = "Addresses",
                //"[{ \"Name\" : \"home\", \"City\" : \"Moskow\", \"Street\" : \"Kutuzovsky Avenue 57\", \"Zip\" : \"643976\" }]",
                HireDate = "HireDate",
                Salary = e.Salary,
                Bonus = e.Bonus,
                Post = "",
                DepartmentName = ""
            });
            using (StreamWriter sw = new StreamWriter(path, true, System.Text.Encoding.UTF8))
            {
                foreach (var item in employees)
                {
                    sw.WriteLine(System.Web.Helpers.Json.Encode(item));
                }
            }
            return RedirectToAction("Index");
        }

        // Тест добавления сотрудников
        [ActionName("TestCreate")]
        public async Task<ActionResult> TestCreateAsync(int num)
        {
            string path = Server.MapPath("~/Results/Employee/Create.txt");
            await employeeService.TestCreateAsync(num, path);
            return RedirectToAction("Index");
        }

        // Тест выборки сотрудников
        public ActionResult TestRead(int num, int val)
        {
            string path = Server.MapPath("~/Results/Employee/Read.txt");
            employeeService.TestRead(num, path, val);
            return RedirectToAction("Index");
        }

        // Тест обновления сотрудников
        [ActionName("TestUpdate")]
        public async Task<ActionResult> TestUpdateAsync(int num)
        {
            string path = Server.MapPath("~/Results/Employee/Update.txt");
            await employeeService.TestUpdateAsync(num, path);
            return RedirectToAction("Index");
        }

        // Тест удаления сотрудников
        [ActionName("TestDelete")]
        public async Task<ActionResult> TestDeleteAsync(int num)
        {
            string path = Server.MapPath("~/Results/Employee/Delete.txt");
            await employeeService.TestDeleteAsync(num, path);
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            employeeService.Dispose();
            postService.Dispose();
            departmentService.Dispose();
            base.Dispose(disposing);
        }
    }
}