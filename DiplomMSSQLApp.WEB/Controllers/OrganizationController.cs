using AutoMapper;
using DiplomMSSQLApp.BLL.DTO;
using DiplomMSSQLApp.BLL.Infrastructure;
using DiplomMSSQLApp.BLL.Interfaces;
using DiplomMSSQLApp.BLL.Services;
using DiplomMSSQLApp.WEB.Models;
using NLog;
using System.IO;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace DiplomMSSQLApp.WEB.Controllers {
    [HandleError]
    [Authorize]
    public class OrganizationController : Controller {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private IService<OrganizationDTO> _organizationService;

        public OrganizationController(IService<OrganizationDTO> os) {
            _organizationService = os;
        }

        // Отображение информации об организации
        public async Task<ActionResult> Index() {
            OrganizationDTO oDto = await (_organizationService as OrganizationService).GetFirstAsync();
            InitializeMapper();
            OrganizationViewModel organization = Mapper.Map<OrganizationDTO, OrganizationViewModel>(oDto);
            return View("Index", organization);
        }

        private void InitializeMapper() {
            Mapper.Initialize(cfg => {
                cfg.CreateMap<OrganizationDTO, OrganizationViewModel>();
                cfg.CreateMap<BLL.DTO.Requisites, Models.Requisites>();
                cfg.CreateMap<BLL.DTO.Bank, Models.Bank>();
            });
        }

        // Обновление информации об организации
        public async Task<ActionResult> Edit(int? id) {
            try {
                OrganizationDTO oDto = await _organizationService.FindByIdAsync(id);
                InitializeMapper();
                OrganizationViewModel organization = Mapper.Map<OrganizationDTO, OrganizationViewModel>(oDto);
                return View("Edit", organization);
            }
            catch (ValidationException ex) {
                _logger.Warn(ex.Message);
                return View("Error", new string[] { ex.Message });
            }
        }
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(OrganizationViewModel organization) {
            try {
                Mapper.Initialize(cfg => {
                    cfg.CreateMap<OrganizationViewModel, OrganizationDTO>();
                    cfg.CreateMap<Models.Requisites, BLL.DTO.Requisites>();
                    cfg.CreateMap<Models.Bank, BLL.DTO.Bank>();
                });
                OrganizationDTO oDto = Mapper.Map<OrganizationViewModel, OrganizationDTO>(organization);
                await _organizationService.EditAsync(oDto);
                return RedirectToAction("Index");
            }
            catch (ValidationException ex) {
                _logger.Warn(ex.Message);
                ModelState.AddModelError(ex.Property, ex.Message);
            }
            return View("Edit", organization);
        }

        // Запись информации об организации в JSON-файл
        public async Task<ActionResult> ExportJson() {
            string fullPath = CreateDirectoryToFile("Organizations.json");
            System.IO.File.Delete(fullPath);
            await _organizationService.ExportJsonAsync(fullPath);
            return RedirectToAction("Index");
        }

        private string CreateDirectoryToFile(string filename) {
            string dir = Server.MapPath("~/Results/");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            string fullPath = dir + filename;
            return fullPath;
        }

        protected override void Dispose(bool disposing) {
            _organizationService.Dispose();
            base.Dispose(disposing);
        }
    }
}
