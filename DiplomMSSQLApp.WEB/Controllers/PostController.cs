﻿using AutoMapper;
using DiplomMSSQLApp.BLL.DTO;
using DiplomMSSQLApp.BLL.Infrastructure;
using DiplomMSSQLApp.BLL.Interfaces;
using DiplomMSSQLApp.BLL.Services;
using DiplomMSSQLApp.WEB.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace DiplomMSSQLApp.WEB.Controllers {
    [HandleError]
    public class PostController : Controller {
        private IService<EmployeeDTO> employeeService;
        private IService<PostDTO> postService;
        private IService<DepartmentDTO> departmentService;

        public PostController(IService<EmployeeDTO> es, IService<PostDTO> ps, IService<DepartmentDTO> ds) {
            employeeService = es;
            postService = ps;
            departmentService = ds;
        }

        public async Task<ActionResult> Index(int page = 1) {
            IEnumerable<PostDTO> pDto = await postService.GetAllAsync();
            ViewBag.TotalNumberOfUnits = pDto.Sum(p => p.NumberOfUnits);
            ViewBag.TotalSalary = pDto.Sum(p => p.TotalSalary);
            pDto = postService.GetPage(pDto, page);     // Paging
            Mapper.Initialize(cfg => {
                cfg.CreateMap<PostDTO, PostViewModel>()
                    .ForMember(p => p.Employees, opt => opt.Ignore());
                cfg.CreateMap<DepartmentDTO, DepartmentViewModel>()
                    .ForMember(d => d.Employees, opt => opt.Ignore());
            });
            IEnumerable<PostViewModel> posts = Mapper.Map<IEnumerable<PostDTO>, IEnumerable<PostViewModel>>(pDto);

            return View("Index", new PostListViewModel { Posts = posts, PageInfo = postService.PageInfo });
        }

        // Добавление новой должности
        public async Task<ActionResult> Create() {
            ViewBag.Departments = await GetSelectListDepartmentsAsync();
            return View("Create");
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
            ViewBag.Departments = await GetSelectListDepartmentsAsync();
            return View("Create", p);
        }

        private async Task<SelectList> GetSelectListDepartmentsAsync() {
            IEnumerable<DepartmentDTO> departments = await departmentService.GetAllAsync();
            if (departments.Count() == 0) {
                DepartmentDTO btDto = new DepartmentDTO { Id = 1, DepartmentName = "unknown" };
                await departmentService.CreateAsync(btDto);
                departments = new List<DepartmentDTO> { btDto };
            }
            return new SelectList(departments.OrderBy(d => d.DepartmentName), "Id", "DepartmentName");
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
            ViewBag.Departments = await GetSelectListDepartmentsAsync();
            return await GetViewAsync(id, "Edit");
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
            ViewBag.Departments = await GetSelectListDepartmentsAsync();
            return View("Edit", p);
        }

        private async Task<ActionResult> GetViewAsync(int? id, string viewName) {
            try {
                PostDTO pDto = await postService.FindByIdAsync(id);
                PostViewModel p = MapDTOWithViewModel(pDto);
                return View(viewName, p);
            }
            catch (ValidationException ex) {
                return View("Error", new string[] { ex.Message });
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
            return await GetViewAsync(id, "Details");
        }

        // Удаление должности
        [ActionName("Delete")]
        public async Task<ActionResult> DeleteAsync(int? id) {
            return await GetViewAsync(id, "Delete");
        }
        [HttpPost, ValidateAntiForgeryToken, ActionName("Delete")]
        public async Task<ActionResult> DeleteConfirmedAsync(int id) {
            try {
                await postService.DeleteAsync(id);
            }
            catch (Exception) {
                return View("Error", new string[] { "Нельзя удалить должность, пока в ней работает хотя бы один сотрудник." });
            }
            return RedirectToAction("Index");
        }

        // Удаление всех должностей
        public ActionResult DeleteAll() {
            return View("DeleteAll");
        }
        [HttpPost, ValidateAntiForgeryToken, ActionName("DeleteAll")]
        public async Task<ActionResult> DeleteAllAsync() {
            try {
                await postService.DeleteAllAsync();
            }
            catch (Exception) {
                return View("Error", new string[] { "Нельзя удалить должность, пока в ней работает хотя бы один сотрудник." });
            }
            return RedirectToAction("Index");
        }

        // Запись информации о должностях в файл
        [ActionName("ExportJson")]
        public async Task<ActionResult> ExportJsonAsync() {
            string fullPath = CreateDirectoryToFile("Posts.json");
            System.IO.File.Delete(fullPath);
            await (postService as PostService).ExportJsonAsync(fullPath);
            return RedirectToAction("Index");
        }

        private string CreateDirectoryToFile(string filename) {
            string dir = Server.MapPath("~/Results/Post/");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            string fullPath = dir + filename;
            return fullPath;
        }

        // Тест добавления сотрудников
        [ActionName("TestCreate")]
        public async Task<ActionResult> TestCreateAsync(int num) {
            string fullPath = CreateDirectoryToFile("Create.txt");
            (postService as PostService).PathToFileForTests = fullPath;
            await postService.TestCreateAsync(num);
            return RedirectToAction("Index");
        }

        // Тест выборки сотрудников
        [ActionName("TestRead")]
        public async Task<ActionResult> TestReadAsync(int num, int salary) {
            string fullPath = CreateDirectoryToFile("Read.txt");
            (postService as PostService).PathToFileForTests = fullPath;
            await postService.TestReadAsync(num, salary);
            return RedirectToAction("Index");
        }

        // Тест обновления сотрудников
        [ActionName("TestUpdate")]
        public async Task<ActionResult> TestUpdateAsync(int num) {
            string fullPath = CreateDirectoryToFile("Update.txt");
            (postService as PostService).PathToFileForTests = fullPath;
            await postService.TestUpdateAsync(num);
            return RedirectToAction("Index");
        }

        // Тест удаления сотрудников
        [ActionName("TestDelete")]
        public async Task<ActionResult> TestDeleteAsync(int num) {
            string fullPath = CreateDirectoryToFile("Delete.txt");
            (postService as PostService).PathToFileForTests = fullPath;
            try {
                await postService.TestDeleteAsync(num);
            }
            catch (Exception) {
                return View("Error", new string[] { "Нельзя удалить должность, пока в ней работает хотя бы один сотрудник." });
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
