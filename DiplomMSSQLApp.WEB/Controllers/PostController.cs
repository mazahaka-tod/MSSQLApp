using AutoMapper;
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
    public class PostController : Controller
    {
        private IService<EmployeeDTO> employeeService;
        private IService<PostDTO> postService;
        private IService<DepartmentDTO> departmentService;

        public PostController(IService<EmployeeDTO> es, IService<PostDTO> ps, IService<DepartmentDTO> ds)
        {
            employeeService = es;
            postService = ps;
            departmentService = ds;
        }

        [ActionName("Index")]
        public async Task<ActionResult> IndexAsync(int page = 1)
        {
            // Получаем список должностей
            IEnumerable<PostDTO> pDto = await postService.GetAllAsync();
            // Пагинация (paging)
            pDto = postService.GetPage(pDto, page, pDto.Count());
            Mapper.Initialize(cfg => cfg.CreateMap<PostDTO, PostViewModel>());
            var p = Mapper.Map<IEnumerable<PostDTO>, IEnumerable<PostViewModel>>(pDto);
            return View(new PostListViewModel { Posts = p, PageInfo = postService.PageInfo });
        }

        // Добавление новой должности
        public ActionResult Create()
        {
            return View();
        }
        [HttpPost, ValidateAntiForgeryToken, ActionName("Create")]
        public async Task<ActionResult> CreateAsync(PostViewModel p)
        {
            try
            {
                Mapper.Initialize(cfg => cfg.CreateMap<PostViewModel, PostDTO>());
                PostDTO pDto = Mapper.Map<PostViewModel, PostDTO>(p);
                await postService.CreateAsync(pDto);
                return RedirectToAction("Index");
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError(ex.Property, ex.Message);
            }
            return View(p);
        }

        // Обновление информации о должности
        [ActionName("Edit")]
        public async Task<ActionResult> EditAsync(int? id)
        {
            try
            {
                PostDTO pDto = await postService.FindByIdAsync(id);
                Mapper.Initialize(cfg => cfg.CreateMap<PostDTO, PostViewModel>()
                    .ForMember(pt => pt.Employees, opt => opt.Ignore()));
                PostViewModel p = Mapper.Map<PostDTO, PostViewModel>(pDto);
                return View(p);
            }
            catch (ValidationException ex)
            {
                return Content(ex.Message);
            }
        }
        [HttpPost, ValidateAntiForgeryToken, ActionName("Edit")]
        public async Task<ActionResult> EditAsync(PostViewModel p)
        {
            try
            {
                // Обновляем данные отдела
                Mapper.Initialize(cfg => cfg.CreateMap<PostViewModel, PostDTO>());
                PostDTO pDto = Mapper.Map<PostViewModel, PostDTO>(p);
                await postService.EditAsync(pDto);
                return RedirectToAction("Index");
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError(ex.Property, ex.Message);
            }
            return View(p);
        }

        // Удаление должности
        [ActionName("Delete")]
        public async Task<ActionResult> DeleteAsync(int? id)
        {
            try
            {
                PostDTO pDto = await postService.FindByIdAsync(id);
                Mapper.Initialize(cfg => {
                    cfg.CreateMap<PostDTO, PostViewModel>();
                    cfg.CreateMap<EmployeeDTO, EmployeeViewModel>();
                });
                PostViewModel p = Mapper.Map<PostDTO, PostViewModel>(pDto);
                return View(p);
            }
            catch (ValidationException ex)
            {
                return Content(ex.Message);
            }
        }
        [HttpPost, ValidateAntiForgeryToken, ActionName("Delete")]
        public async Task<ActionResult> DeleteConfirmedAsync(int id)
        {
            await postService.DeleteAsync(id);
            return RedirectToAction("Index");
        }

        // Подробная информация о должности
        [ActionName("Details")]
        public async Task<ActionResult> DetailsAsync(int? id)
        {
            try
            {
                PostDTO pDto = await postService.FindByIdAsync(id);
                Mapper.Initialize(cfg => {
                    cfg.CreateMap<PostDTO, PostViewModel>();
                    cfg.CreateMap<EmployeeDTO, EmployeeViewModel>();
                });
                PostViewModel p = Mapper.Map<PostDTO, PostViewModel>(pDto);
                return View(p);
            }
            catch (ValidationException ex)
            {
                return Content(ex.Message);
            }
        }

        // Удаление всех должностей
        public ActionResult DeleteAll()
        {
            return View();
        }
        [HttpPost, ValidateAntiForgeryToken, ActionName("DeleteAll")]
        public async Task<ActionResult> DeleteAllAsync()
        {
            await postService.DeleteAllAsync();
            return RedirectToAction("Index");
        }

        // Запись базы в файл
        [ActionName("ExportJson")]
        public async Task<ActionResult> ExportJsonAsync()
        {
            string path = Server.MapPath("~/Results/Post/Json.txt");
            IEnumerable<PostDTO> pDto = await postService.GetAllAsync();
            var posts = pDto.Select(p => new
            {
                p.Title,
                p.MinSalary,
                p.MaxSalary
            });
            using (StreamWriter sw = new StreamWriter(path, true, System.Text.Encoding.UTF8))
            {
                foreach (var item in posts)
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
            string path = Server.MapPath("~/Results/Post/Create.txt");
            await postService.TestCreateAsync(num, path);
            return RedirectToAction("Index");
        }

        // Тест выборки сотрудников
        [ActionName("TestRead")]
        public async Task<ActionResult> TestReadAsync(int num, int salary)
        {
            string path = Server.MapPath("~/Results/Post/Read.txt");
            await postService.TestReadAsync(num, path, salary);
            return RedirectToAction("Index");
        }

        // Тест обновления сотрудников
        [ActionName("TestUpdate")]
        public async Task<ActionResult> TestUpdateAsync(int num)
        {
            string path = Server.MapPath("~/Results/Post/Update.txt");
            await postService.TestUpdateAsync(num, path);
            return RedirectToAction("Index");
        }

        // Тест удаления сотрудников
        [ActionName("TestDelete")]
        public async Task<ActionResult> TestDeleteAsync(int num)
        {
            string path = Server.MapPath("~/Results/Post/Delete.txt");
            await postService.TestDeleteAsync(num, path);
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