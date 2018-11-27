using AutoMapper;
using DiplomMSSQLApp.BLL.DTO;
using DiplomMSSQLApp.BLL.Infrastructure;
using DiplomMSSQLApp.DAL.Entities;
using DiplomMSSQLApp.DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

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
            Organization organization = Mapper.Map<OrganizationDTO, Organization>(oDto);
            Database.Organizations.Update(organization);
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
            Organization organization = await Database.Organizations.FindByIdAsync(id.Value);
            if (organization == null)
                throw new ValidationException("Организация не найдена", "");
            InitializeMapper();
            OrganizationDTO oDto = Mapper.Map<Organization, OrganizationDTO>(organization);
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
            IEnumerable<Organization> organizations = await Database.Organizations.GetAllAsync();
            InitializeMapper();
            IEnumerable<OrganizationDTO> collection = Mapper.Map<IEnumerable<Organization>, IEnumerable<OrganizationDTO>>(organizations);
            return collection;
        }

        // Получение первой организации
        public virtual async Task<OrganizationDTO> GetFirstAsync() {
            Organization organization = await Database.Organizations.GetFirstAsync();
            InitializeMapper();
            OrganizationDTO oDto = Mapper.Map<Organization, OrganizationDTO>(organization);
            return oDto;
        }

        // Запись информации об организации в JSON-файл
        public override async Task ExportJsonAsync(string fullPath) {
            IEnumerable<Organization> organizations = await Database.Organizations.GetAllAsync();
            var transformOrganizations = organizations.Select(o => new {
                o.ActualAddress,
                o.Bank,
                o.Email,
                o.Fax,
                o.LegalAddress,
                o.Name,
                o.Phone,
                o.Requisites,
                o.WWW
            }).ToArray();
            using (StreamWriter sw = new StreamWriter(fullPath, true, Encoding.UTF8)) {
                sw.WriteLine("{\"Organizations\":");
                sw.WriteLine(new JavaScriptSerializer().Serialize(transformOrganizations));
                sw.WriteLine("}");
            }
        }

        public override void Dispose() {
            Database.Dispose();
        }

        // Нереализованные методы
        public override Task CreateAsync(OrganizationDTO item) {
            throw new NotImplementedException();
        }

        public override Task DeleteAsync(int id) {
            throw new NotImplementedException();
        }

        public override Task DeleteAllAsync() {
            throw new NotImplementedException();
        }

        public override Task<int> CountAsync(Expression<Func<OrganizationDTO, bool>> predicate) {
            throw new NotImplementedException();
        }

        public override Task<int> CountAsync() {
            throw new NotImplementedException();
        }
    }
}
