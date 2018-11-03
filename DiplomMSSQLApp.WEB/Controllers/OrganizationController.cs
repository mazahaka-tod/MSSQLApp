﻿using AutoMapper;
using DiplomMSSQLApp.BLL.DTO;
using DiplomMSSQLApp.BLL.Infrastructure;
using DiplomMSSQLApp.BLL.Interfaces;
using DiplomMSSQLApp.WEB.Models;
using NLog;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace DiplomMSSQLApp.WEB.Controllers
{
    [HandleError]
    public class OrganizationController : Controller
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private IService<OrganizationDTO> _organizationService;

        public OrganizationController(IService<OrganizationDTO> os) {
            _organizationService = os;
        }

        // Отображение информации об организации
        public async Task<ActionResult> Index()
        {
            OrganizationDTO oDto = await _organizationService.GetFirstAsync();
            Mapper.Initialize(cfg => cfg.CreateMap<OrganizationDTO, OrganizationViewModel>());
            OrganizationViewModel organization = Mapper.Map<OrganizationDTO, OrganizationViewModel>(oDto);
            return View("Index", organization);
        }

        // Обновление информации об организации
        public async Task<ActionResult> Edit(int? id) {
            try {
                OrganizationDTO oDto = await _organizationService.FindByIdAsync(id);
                Mapper.Initialize(cfg => cfg.CreateMap<OrganizationDTO, OrganizationViewModel>());
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
                Mapper.Initialize(cfg => cfg.CreateMap<OrganizationViewModel, OrganizationDTO>());
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
    }
}
