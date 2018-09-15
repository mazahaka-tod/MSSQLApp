using AutoMapper;
using DiplomMSSQLApp.BLL.BusinessModels;
using DiplomMSSQLApp.BLL.DTO;
using DiplomMSSQLApp.BLL.Infrastructure;
using DiplomMSSQLApp.BLL.Interfaces;
using DiplomMSSQLApp.BLL.Services;
using DiplomMSSQLApp.WEB.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace DiplomMSSQLApp.WEB.Controllers {
    public class HomeController : Controller {
        private IService<EmployeeDTO> employeeService;
        private IService<DepartmentDTO> departmentService;
        private IService<PostDTO> postService;

        public HomeController(IService<EmployeeDTO> es, IService<DepartmentDTO> ds, IService<PostDTO> ps) {
            employeeService = es;
            departmentService = ds;
            postService = ps;
        }

        public ActionResult Index(EmployeeFilter filter, string str, int page = 1) {
            // Условие истинно, когда пользователь использует paging,
            // в этом случае передается строка str, а фильтр EmployeeFilter filter нет,
            // поэтому производим декодирование строки в объект EmployeeFilter и присваиваем его фильтру filter
            if (str != null) filter = System.Web.Helpers.Json.Decode<EmployeeFilter>(str);
            (employeeService as EmployeeService).PathToFileForTests = Server.MapPath("~/Results/Employee/Filter.txt");
            IEnumerable<EmployeeDTO> eDto = employeeService.Get(filter);
            eDto = employeeService.GetPage(eDto, page);     // Paging
            Mapper.Initialize(cfg => {
                cfg.CreateMap<EmployeeDTO, EmployeeViewModel>()
                    .ForMember(e => e.BusinessTrips, opt => opt.Ignore());
                cfg.CreateMap<DepartmentDTO, DepartmentViewModel>()
                    .ForMember(d => d.Employees, opt => opt.Ignore());
                cfg.CreateMap<PostDTO, PostViewModel>()
                    .ForMember(p => p.Employees, opt => opt.Ignore());
            });
            IEnumerable<EmployeeViewModel> employees = Mapper.Map<IEnumerable<EmployeeDTO>, IEnumerable<EmployeeViewModel>>(eDto);
            return View(new EmployeeListViewModel { Employees = employees, Filter = filter, PageInfo = employeeService.PageInfo });
        }

        // Добавление нового сотрудника
        [ActionName("Create")]
        public async Task<ActionResult> CreateAsync() {
            ViewBag.Departments = await GetSelectListDepartmentsAsync();
            ViewBag.Posts = await GetSelectListPostsAsync();
            return View();
        }
        [HttpPost, ValidateAntiForgeryToken, ActionName("Create")]
        public async Task<ActionResult> CreateAsync(EmployeeViewModel e) {
            try {
                var eDto = MapViewModelWithDTO(e);
                await employeeService.CreateAsync(eDto);
                return RedirectToAction("Index");
            }
            catch (ValidationException ex) {
                ModelState.AddModelError(ex.Property, ex.Message);
            }
            ViewBag.Departments = await GetSelectListDepartmentsAsync();
            ViewBag.Posts = await GetSelectListPostsAsync();
            return View(e);
        }

        // Обновление информации о сотруднике
        [ActionName("Edit")]
        public async Task<ActionResult> EditAsync(int? id) {
            ViewBag.Departments = await GetSelectListDepartmentsAsync();
            ViewBag.Posts = await GetSelectListPostsAsync();
            return await GetViewAsync(id);
        }
        [HttpPost, ValidateAntiForgeryToken, ActionName("Edit")]
        public async Task<ActionResult> EditAsync(EmployeeViewModel e) {
            try {
                EmployeeDTO eDto = MapViewModelWithDTO(e);
                await employeeService.EditAsync(eDto);
                return RedirectToAction("Index");
            }
            catch (ValidationException ex) {
                ModelState.AddModelError(ex.Property, ex.Message);
            }
            ViewBag.Departments = await GetSelectListDepartmentsAsync();
            ViewBag.Posts = await GetSelectListPostsAsync();
            return View(e);
        }

        // Удаление сотрудника
        [ActionName("Delete")]
        public async Task<ActionResult> DeleteAsync(int? id) {
            return await GetViewAsync(id);
        }
        [HttpPost, ValidateAntiForgeryToken, ActionName("Delete")]
        public async Task<ActionResult> DeleteConfirmedAsync(int id) {
            await employeeService.DeleteAsync(id);
            return RedirectToAction("Index");
        }

        // Подробная информация о сотруднике
        [ActionName("Details")]
        public async Task<ActionResult> DetailsAsync(int? id) {
            return await GetViewAsync(id);
        }

        // Удаление всех сотрудников
        public ActionResult DeleteAll() {
            return View();
        }
        [HttpPost, ValidateAntiForgeryToken, ActionName("DeleteAll")]
        public async Task<ActionResult> DeleteAllAsync() {
            await employeeService.DeleteAllAsync();
            return RedirectToAction("Index");
        }

        // Запись информации о сотрудниках в файл
        [ActionName("ExportJson")]
        public async Task<ActionResult> ExportJsonAsync() {
            string path = Server.MapPath("~/Results/Employee/Employees.json");
            IEnumerable<EmployeeDTO> eDto = await employeeService.GetAllAsync();
            var employees = eDto.Select(e => new {
                e.LastName,
                e.FirstName,
                e.Email,
                e.PhoneNumber,
                e.Address,
                HireDate = e.HireDate.ToShortDateString(),
                e.Salary,
                e.Bonus,
                Post = e.Post.Title,
                Department = e.Department.DepartmentName,
                BusinessTrips = e.BusinessTrips.Select(bt => bt.Name)
            }).ToArray();
            using (StreamWriter sw = new StreamWriter(path, true, System.Text.Encoding.UTF8)) {
                sw.WriteLine("{\"Employees\":[");
                int employeesLength = employees.Length;
                for (int i = 1; i < employeesLength; i++) {
                    sw.Write(System.Web.Helpers.Json.Encode(employees[i]));
                    if (i != employeesLength-1) sw.WriteLine(",");
                }
                sw.WriteLine("]}");
            }
            return RedirectToAction("Index");
        }

        // Тест добавления сотрудников
        [ActionName("TestCreate")]
        public async Task<ActionResult> TestCreateAsync(int num) {
            string path = Server.MapPath("~/Results/Employee/Create.txt");
            await employeeService.TestCreateAsync(num, path);
            return RedirectToAction("Index");
        }

        // Тест выборки сотрудников
        [ActionName("TestRead")]
        public async Task<ActionResult> TestReadAsync(int num, int salary) {
            string path = Server.MapPath("~/Results/Employee/Read.txt");
            await employeeService.TestReadAsync(num, path, salary);
            return RedirectToAction("Index");
        }

        // Тест обновления сотрудников
        [ActionName("TestUpdate")]
        public async Task<ActionResult> TestUpdateAsync(int num) {
            string path = Server.MapPath("~/Results/Employee/Update.txt");
            await employeeService.TestUpdateAsync(num, path);
            return RedirectToAction("Index");
        }

        // Тест удаления сотрудников
        [ActionName("TestDelete")]
        public async Task<ActionResult> TestDeleteAsync(int num) {
            string path = Server.MapPath("~/Results/Employee/Delete.txt");
            await employeeService.TestDeleteAsync(num, path);
            return RedirectToAction("Index");
        }

        public ActionResult About() {
            return View();
        }

        public ActionResult Contact() {
            return View();
        }

        // Частичное представление добавляет текстовое поле
        public ActionResult AddTextBox(string name) {
            ViewBag.Name = name;
            return PartialView();
        }

        protected override void Dispose(bool disposing) {
            employeeService.Dispose();
            postService.Dispose();
            departmentService.Dispose();
            base.Dispose(disposing);
        }

        // Получение списка отделов
        private async Task<SelectList> GetSelectListDepartmentsAsync() {
            IEnumerable<DepartmentDTO> departments = await departmentService.GetAllAsync();
            return new SelectList(departments.OrderBy(d => d.DepartmentName), "Id", "DepartmentName");
        }

        // Получение списка должностей
        private async Task<SelectList> GetSelectListPostsAsync() {
            IEnumerable<PostDTO> posts = await postService.GetAllAsync();
            return new SelectList(posts.OrderBy(p => p.Title), "Id", "Title");
        }

        private async Task<ActionResult> GetViewAsync(int? id) {
            try {
                EmployeeDTO eDto = await employeeService.FindByIdAsync(id);
                EmployeeViewModel e = MapDTOWithViewModel(eDto);
                return View(e);
            }
            catch (ValidationException ex) {
                return View("CustomError", (object)ex.Message);
            }
        }

        private EmployeeViewModel MapDTOWithViewModel(EmployeeDTO eDto) {
            Mapper.Initialize(cfg => {
                cfg.CreateMap<EmployeeDTO, EmployeeViewModel>();
                cfg.CreateMap<BaseBusinessTripDTO, BaseBusinessTripViewModel>();
                cfg.CreateMap<DepartmentDTO, DepartmentViewModel>()
                   .ForMember(d => d.Employees, opt => opt.Ignore());
                cfg.CreateMap<PostDTO, PostViewModel>()
                   .ForMember(p => p.Employees, opt => opt.Ignore());
            });
            return Mapper.Map<EmployeeDTO, EmployeeViewModel>(eDto);
        }

        private EmployeeDTO MapViewModelWithDTO(EmployeeViewModel e) {
            Mapper.Initialize(cfg => {
                cfg.CreateMap<EmployeeViewModel, EmployeeDTO>();
                cfg.CreateMap<BaseBusinessTripViewModel, BaseBusinessTripDTO>();
                cfg.CreateMap<DepartmentViewModel, DepartmentDTO>()
                   .ForMember(d => d.Employees, opt => opt.Ignore());
                cfg.CreateMap<PostViewModel, PostDTO>()
                   .ForMember(p => p.Employees, opt => opt.Ignore());
            });
            return Mapper.Map<EmployeeViewModel, EmployeeDTO>(e);
        }
    }
}