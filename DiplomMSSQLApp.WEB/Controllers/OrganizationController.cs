﻿using AutoMapper;
using DiplomMSSQLApp.BLL.DTO;
using DiplomMSSQLApp.BLL.Infrastructure;
using DiplomMSSQLApp.BLL.Interfaces;
using DiplomMSSQLApp.WEB.Models;
using NLog;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace DiplomMSSQLApp.WEB.Controllers {
    [HandleError]
    [Authorize]
    public class OrganizationController : Controller
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private IService<OrganizationDTO> _OrganizationService;

        public OrganizationController(IService<OrganizationDTO> os) {
            _OrganizationService = os;
        }

        // Отображение информации об организации
        public async Task<ActionResult> Index()
        {
            OrganizationDTO oDto = await _OrganizationService.GetFirstAsync();
            InitializeMapper();
            OrganizationViewModel Organization = Mapper.Map<OrganizationDTO, OrganizationViewModel>(oDto);
            return View("Index", Organization);
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
                OrganizationDTO oDto = await _OrganizationService.FindByIdAsync(id);
                InitializeMapper();
                OrganizationViewModel Organization = Mapper.Map<OrganizationDTO, OrganizationViewModel>(oDto);
                return View("Edit", Organization);
            }
            catch (ValidationException ex) {
                _logger.Warn(ex.Message);
                return View("Error", new string[] { ex.Message });
            }
        }
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(OrganizationViewModel Organization) {
            try {
                Mapper.Initialize(cfg => {
                    cfg.CreateMap<OrganizationViewModel, OrganizationDTO>();
                    cfg.CreateMap<Models.Requisites, BLL.DTO.Requisites>();
                    cfg.CreateMap<Models.Bank, BLL.DTO.Bank>();
                });
                OrganizationDTO oDto = Mapper.Map<OrganizationViewModel, OrganizationDTO>(Organization);
                await _OrganizationService.EditAsync(oDto);
                return RedirectToAction("Index");
            }
            catch (ValidationException ex) {
                _logger.Warn(ex.Message);
                ModelState.AddModelError(ex.Property, ex.Message);
            }
            return View("Edit", Organization);
        }
    }
}
