using AutoMapper;
using DiplomMSSQLApp.BLL.DTO;
using DiplomMSSQLApp.BLL.Infrastructure;
using DiplomMSSQLApp.BLL.Interfaces;
using DiplomMSSQLApp.WEB.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace DiplomMSSQLApp.WEB.Controllers {
    public class PostController : Controller {
        private IService<EmployeeDTO> employeeService;
        private IService<PostDTO> postService;
        private IService<DepartmentDTO> departmentService;

        public PostController(IService<EmployeeDTO> es, IService<PostDTO> ps, IService<DepartmentDTO> ds) {
            employeeService = es;
            postService = ps;
            departmentService = ds;
        }

        [ActionName("Index")]
        public async Task<ActionResult> IndexAsync(int page = 1) {
            IEnumerable<PostDTO> pDto = await postService.GetAllAsync();
            pDto = postService.GetPage(pDto, page);     // Paging
            Mapper.Initialize(cfg => cfg.CreateMap<PostDTO, PostViewModel>()
                                        .ForMember(p => p.Employees, opt => opt.Ignore()));
            IEnumerable<PostViewModel> posts = Mapper.Map<IEnumerable<PostDTO>, IEnumerable<PostViewModel>>(pDto);
            return View(new PostListViewModel { Posts = posts, PageInfo = postService.PageInfo });
        }

        // Добавление новой должности
        public ActionResult Create() {
            return View();
        }
        [HttpPost, ValidateAntiForgeryToken, ActionName("Create")]
        public async Task<ActionResult> CreateAsync(PostViewModel p) {
            try {
                PostDTO pDto = MapViewModelWithDTO(p);
                await postService.CreateAsync(pDto);
                return RedirectToAction("Index");
            }
            catch (ValidationException ex) {
                ModelState.AddModelError(ex.Property, ex.Message);
            }
            return View(p);
        }

        private PostDTO MapViewModelWithDTO(PostViewModel p) {
            Mapper.Initialize(cfg => {
                cfg.CreateMap<PostViewModel, PostDTO>();
                cfg.CreateMap<EmployeeViewModel, EmployeeDTO>()
                    .ForMember(e => e.BusinessTrips, opt => opt.Ignore())
                    .ForMember(e => e.Department, opt => opt.Ignore())
                    .ForMember(e => e.Post, opt => opt.Ignore());
            });
            PostDTO pDto = Mapper.Map<PostViewModel, PostDTO>(p);
            return pDto;
        }

        // Обновление информации о должности
        [ActionName("Edit")]
        public async Task<ActionResult> EditAsync(int? id) {
            return await GetViewAsync(id);
        }
        [HttpPost, ValidateAntiForgeryToken, ActionName("Edit")]
        public async Task<ActionResult> EditAsync(PostViewModel p) {
            try {
                PostDTO pDto = MapViewModelWithDTO(p);
                await postService.EditAsync(pDto);
                return RedirectToAction("Index");
            }
            catch (ValidationException ex) {
                ModelState.AddModelError(ex.Property, ex.Message);
            }
            return View(p);
        }

        private async Task<ActionResult> GetViewAsync(int? id) {
            try {
                PostDTO pDto = await postService.FindByIdAsync(id);
                PostViewModel p = MapDTOWithViewModel(pDto);
                return View(p);
            }
            catch (ValidationException ex) {
                return View("CustomError", (object)ex.Message);
            }
        }

        private PostViewModel MapDTOWithViewModel(PostDTO pDto) {
            Mapper.Initialize(cfg => {
                cfg.CreateMap<PostDTO, PostViewModel>();
                cfg.CreateMap<EmployeeDTO, EmployeeViewModel>()
                    .ForMember(e => e.BusinessTrips, opt => opt.Ignore())
                    .ForMember(e => e.Department, opt => opt.Ignore())
                    .ForMember(e => e.Post, opt => opt.Ignore());
            });
            PostViewModel p = Mapper.Map<PostDTO, PostViewModel>(pDto);
            return p;
        }

        // Подробная информация о должности
        [ActionName("Details")]
        public async Task<ActionResult> DetailsAsync(int? id) {
            return await GetViewAsync(id);
        }

        // Удаление должности
        [ActionName("Delete")]
        public async Task<ActionResult> DeleteAsync(int? id) {
            return await GetViewAsync(id);
        }
        [HttpPost, ValidateAntiForgeryToken, ActionName("Delete")]
        public async Task<ActionResult> DeleteConfirmedAsync(int id) {
            try {
                await postService.DeleteAsync(id);
            }
            catch (Exception) {
                return View("CustomError", (object)"Нельзя удалить должность, пока в ней работает хотя бы один сотрудник.");
            }
            return RedirectToAction("Index");
        }

        // Удаление всех должностей
        public ActionResult DeleteAll() {
            return View();
        }
        [HttpPost, ValidateAntiForgeryToken, ActionName("DeleteAll")]
        public async Task<ActionResult> DeleteAllAsync() {
            try {
                await postService.DeleteAllAsync();
            }
            catch (Exception) {
                return View("CustomError", (object)"Нельзя удалить должность, пока в ней работает хотя бы один сотрудник.");
            }
            return RedirectToAction("Index");
        }

        // Запись информации о должностях в файл
        [ActionName("ExportJson")]
        public async Task<ActionResult> ExportJsonAsync() {
            string path = Server.MapPath("~/Results/Post/Posts.json");
            System.IO.File.Delete(path);
            IEnumerable<PostDTO> pDto = await postService.GetAllAsync();
            var posts = pDto.Select(p => new {
                p.Title,
                p.MinSalary,
                p.MaxSalary
            }).ToArray();
            using (StreamWriter sw = new StreamWriter(path, true, System.Text.Encoding.UTF8)) {
                sw.WriteLine("{\"Posts\":[");
                int postsLength = posts.Length;
                for (int i = 1; i < postsLength; i++) {
                    sw.Write(System.Web.Helpers.Json.Encode(posts[i]));
                    if (i != postsLength-1) sw.WriteLine(",");
                }
                sw.WriteLine("]}");
            }
            return RedirectToAction("Index");
        }

        // Тест добавления сотрудников
        [ActionName("TestCreate")]
        public async Task<ActionResult> TestCreateAsync(int num) {
            string path = Server.MapPath("~/Results/Post/Create.txt");
            await postService.TestCreateAsync(num, path);
            return RedirectToAction("Index");
        }

        // Тест выборки сотрудников
        [ActionName("TestRead")]
        public async Task<ActionResult> TestReadAsync(int num, int salary) {
            string path = Server.MapPath("~/Results/Post/Read.txt");
            await postService.TestReadAsync(num, path, salary);
            return RedirectToAction("Index");
        }

        // Тест обновления сотрудников
        [ActionName("TestUpdate")]
        public async Task<ActionResult> TestUpdateAsync(int num) {
            string path = Server.MapPath("~/Results/Post/Update.txt");
            await postService.TestUpdateAsync(num, path);
            return RedirectToAction("Index");
        }

        // Тест удаления сотрудников
        [ActionName("TestDelete")]
        public async Task<ActionResult> TestDeleteAsync(int num) {
            string path = Server.MapPath("~/Results/Post/Delete.txt");
            try {
                await postService.TestDeleteAsync(num, path);
            }
            catch (Exception) {
                return View("CustomError", (object)"Нельзя удалить должность, пока в ней работает хотя бы один сотрудник.");
            }
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing) {
            employeeService.Dispose();
            postService.Dispose();
            departmentService.Dispose();
            base.Dispose(disposing);
        }
    }
}