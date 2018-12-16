using AutoMapper;
using DiplomMSSQLApp.BLL.DTO;
using DiplomMSSQLApp.BLL.Infrastructure;
using DiplomMSSQLApp.BLL.Interfaces;
using DiplomMSSQLApp.BLL.Services;
using DiplomMSSQLApp.WEB.Models;
using DiplomMSSQLApp.WEB.Util;
using NLog;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace DiplomMSSQLApp.WEB.Controllers {
    [HandleError]
    [Authorize]
    [Internationalization]
    public class LeaveScheduleController : Controller {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private IService<LeaveScheduleDTO> _leaveScheduleService;

        public LeaveScheduleController(IService<LeaveScheduleDTO> ls) {
            _leaveScheduleService = ls;
        }

        public async Task<ActionResult> Index(int page = 1) {
            IEnumerable<LeaveScheduleDTO> lsDto = await _leaveScheduleService.GetAllAsync();
            lsDto = _leaveScheduleService.GetPage(lsDto, page);     // Paging
            InitializeMapper();
            IEnumerable<LeaveScheduleViewModel> leaveSchedules = Mapper.Map<IEnumerable<LeaveScheduleDTO>, IEnumerable<LeaveScheduleViewModel>>(lsDto);
            LeaveScheduleListViewModel model = new LeaveScheduleListViewModel {
                LeaveSchedules = leaveSchedules,
                PageInfo = _leaveScheduleService.PageInfo
            };
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest") {
                _logger.Info("Executed async request");
                var transformModel = new {
                    LeaveSchedules = model.LeaveSchedules.Select(ls => new {
                        ls.Id,
                        ls.Number,
                        ls.Year,
                        DateOfPreparation = ls.DateOfPreparation.ToString("dd MMMM yyyy"),
                        DateOfApproval = ls.DateOfApproval.HasValue ? ls.DateOfApproval.Value.ToString("dd MMMM yyyy") : ""
                    }).ToArray(),
                    model.PageInfo
                };
                return Json(transformModel, JsonRequestBehavior.AllowGet);
            }
            _logger.Info("Executed sync request");
            return View("Index", model);
        }

        // Добавление нового графика отпусков
        public ActionResult Create() {
            return View("Create");
        }
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(LeaveScheduleViewModel ls) {
            try {
                LeaveScheduleDTO lsDto = MapViewModelWithDTO(ls);
                await (_leaveScheduleService as LeaveScheduleService).CreateAsync(lsDto);
                return RedirectToAction("Index");
            }
            catch (ValidationException ex) {
                _logger.Warn(ex.Message);
                ModelState.AddModelError(ex.Property, ex.Message);
            }
            return View("Create", ls);
        }

        private LeaveScheduleDTO MapViewModelWithDTO(LeaveScheduleViewModel ls) {
            Mapper.Initialize(cfg => cfg.CreateMap<LeaveScheduleViewModel, LeaveScheduleDTO>());
            Mapper.Initialize(cfg => {
                cfg.CreateMap<LeaveScheduleViewModel, LeaveScheduleDTO>();
                cfg.CreateMap<AnnualLeaveViewModel, AnnualLeaveDTO>()
                    .ForMember(al => al.Employee, opt => opt.Ignore())
                    .ForMember(al => al.LeaveSchedule, opt => opt.Ignore());
            });
            LeaveScheduleDTO lsDto = Mapper.Map<LeaveScheduleViewModel, LeaveScheduleDTO>(ls);
            return lsDto;
        }

        // Обновление информации о графике отпусков
        public async Task<ActionResult> Edit(int? id) {
            return await GetViewAsync(id, "Edit");
        }
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(LeaveScheduleViewModel ls) {
            try {
                LeaveScheduleDTO lsDto = MapViewModelWithDTO(ls);
                await (_leaveScheduleService as LeaveScheduleService).EditAsync(lsDto);
                return RedirectToAction("Index");
            }
            catch (ValidationException ex) {
                _logger.Warn(ex.Message);
                ModelState.AddModelError(ex.Property, ex.Message);
            }
            return View("Edit", ls);
        }

        private async Task<ActionResult> GetViewAsync(int? id, string viewName) {
            try {
                LeaveScheduleDTO lsDto = await _leaveScheduleService.FindByIdAsync(id);
                LeaveScheduleViewModel ls = MapDTOWithViewModel(lsDto);
                return View(viewName, ls);
            }
            catch (ValidationException ex) {
                _logger.Warn(ex.Message);
                return View("Error", new string[] { ex.Message });
            }
        }

        private LeaveScheduleViewModel MapDTOWithViewModel(LeaveScheduleDTO lsDto) {
            InitializeMapper();
            LeaveScheduleViewModel ls = Mapper.Map<LeaveScheduleDTO, LeaveScheduleViewModel>(lsDto);
            return ls;
        }

        private void InitializeMapper() {
            Mapper.Initialize(cfg => {
                cfg.CreateMap<LeaveScheduleDTO, LeaveScheduleViewModel>();
                cfg.CreateMap<AnnualLeaveDTO, AnnualLeaveViewModel>()
                    .ForMember(al => al.Employee, opt => opt.Ignore())
                    .ForMember(al => al.LeaveSchedule, opt => opt.Ignore());
            });
        }

        // Удаление графика отпусков
        public async Task<ActionResult> Delete(int? id) {
            return await GetViewAsync(id, "Delete");
        }
        [HttpPost, ValidateAntiForgeryToken, ActionName("Delete")]
        public async Task<ActionResult> DeleteConfirmed(int id) {
            try {
                await _leaveScheduleService.DeleteAsync(id);
            }
            catch (ValidationException ex) {
                _logger.Warn("Failed to delete a leave schedule");
                return View("Error", new string[] { ex.Message });
            }
            return RedirectToAction("Index");
        }

        // Удаление всех графиков отпусков
        public ActionResult DeleteAll() {
            return View("DeleteAll");
        }
        [HttpPost, ValidateAntiForgeryToken, ActionName("DeleteAll")]
        public async Task<ActionResult> DeleteAllConfirmed() {
            await _leaveScheduleService.DeleteAllAsync();
            return RedirectToAction("Index");
        }

        // Запись информации о графиках отпусков в JSON-файл
        public async Task<ActionResult> ExportJson() {
            string fullPath = CreateDirectoryToFile("LeaveSchedules.json");
            System.IO.File.Delete(fullPath);
            await _leaveScheduleService.ExportJsonAsync(fullPath);
            return File(fullPath, "application/json", "LeaveSchedules.json");
        }

        private string CreateDirectoryToFile(string filename) {
            string dir = Server.MapPath("~/Results/");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            string fullPath = dir + filename;
            return fullPath;
        }

        protected override void Dispose(bool disposing) {
            _leaveScheduleService.Dispose();
            base.Dispose(disposing);
        }
    }
}
