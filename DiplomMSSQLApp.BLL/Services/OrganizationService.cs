using AutoMapper;
using DiplomMSSQLApp.BLL.BusinessModels;
using DiplomMSSQLApp.BLL.DTO;
using DiplomMSSQLApp.BLL.Infrastructure;
using DiplomMSSQLApp.DAL.Entities;
using DiplomMSSQLApp.DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiplomMSSQLApp.BLL.Services {
    public class OrganizationService : BaseService<OrganizationDTO> {
        public IUnitOfWork Database { get; set; }

        public OrganizationService(IUnitOfWork uow) {
            Database = uow;
        }

        public OrganizationService() { }

        // Обновление информации об организации
        public override async Task EditAsync(OrganizationDTO oDto) {
            ValidationOrganization(oDto);
            Mapper.Initialize(cfg => {
                cfg.CreateMap<OrganizationDTO, Organization>();
                cfg.CreateMap<DTO.Requisites, DAL.Entities.Requisites>();
                cfg.CreateMap<DTO.Bank, DAL.Entities.Bank>();
            });
            Organization Organization = Mapper.Map<OrganizationDTO, Organization>(oDto);
            Database.Organizations.Update(Organization);
            await Database.SaveAsync();
        }

        private void ValidationOrganization(OrganizationDTO oDto) {
            if (oDto.Name == null)
                throw new ValidationException("Требуется ввести наименование организации", "Name");
        }

        // Получение организации по id
        public override async Task<OrganizationDTO> FindByIdAsync(int? id) {
            if (id == null)
                throw new ValidationException("Не установлен id организации", "");
            Organization Organization = await Database.Organizations.FindByIdAsync(id.Value);
            if (Organization == null)
                throw new ValidationException("Организация не найдена", "");
            InitializeMapper();
            OrganizationDTO oDto = Mapper.Map<Organization, OrganizationDTO>(Organization);
            return oDto;
        }

        private void InitializeMapper() {
            Mapper.Initialize(cfg => {
                cfg.CreateMap<Organization, OrganizationDTO>();
                cfg.CreateMap<DAL.Entities.Requisites, DTO.Requisites>();
                cfg.CreateMap<DAL.Entities.Bank, DTO.Bank>();
            });
        }

        // Получение списка организаций
        public override async Task<IEnumerable<OrganizationDTO>> GetAllAsync() {
            IEnumerable<Organization> Organizations = await Database.Organizations.GetAsync();
            InitializeMapper();
            IEnumerable<OrganizationDTO> collection = Mapper.Map<IEnumerable<Organization>, IEnumerable<OrganizationDTO>>(Organizations);
            return collection;
        }

        // Получение первой организации
        public override async Task<OrganizationDTO> GetFirstAsync() {
            Organization Organization = await Database.Organizations.GetFirstAsync();
            InitializeMapper();
            OrganizationDTO oDto = Mapper.Map<Organization, OrganizationDTO>(Organization);
            return oDto;
        }

        public override void Dispose() {
            Database.Dispose();
        }

        // Нереализованные методы
        public override Task CreateAsync(OrganizationDTO item) {
            throw new NotImplementedException();
        }

        public override Task DeleteAllAsync() {
            throw new NotImplementedException();
        }

        public override Task DeleteAsync(int id) {
            throw new NotImplementedException();
        }

        public override Task TestCreateAsync(int num) {
            throw new NotImplementedException();
        }

        public override Task TestDeleteAsync(int num) {
            throw new NotImplementedException();
        }

        public override Task TestReadAsync(int num, int salary) {
            throw new NotImplementedException();
        }

        public override Task TestUpdateAsync(int num) {
            throw new NotImplementedException();
        }

        public override IEnumerable<OrganizationDTO> Get(EmployeeFilter filter) {
            throw new NotImplementedException();
        }
    }
}
