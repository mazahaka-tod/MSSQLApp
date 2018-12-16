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
    public class LeaveScheduleService : BaseService<LeaveScheduleDTO> {
        public IUnitOfWork Database { get; set; }

        public LeaveScheduleService(IUnitOfWork uow) {
            Database = uow;
        }

        public LeaveScheduleService() { }

        // Добавление нового графика отпусков (с валидацией)
        public override async Task CreateAsync(LeaveScheduleDTO leaveScheduleDto) {
            await ValidationLeaveSchedule(leaveScheduleDto);
            InitializeMapperDTO();
            LeaveSchedule leaveSchedule = Mapper.Map<LeaveScheduleDTO, LeaveSchedule>(leaveScheduleDto);
            Database.LeaveSchedules.Create(leaveSchedule);
            await Database.SaveAsync();
        }

        private async Task ValidationLeaveSchedule(LeaveScheduleDTO item) {
            if (item.Year < 1900 || item.Year > 2100)
                throw new ValidationException("Некорректный год", "Year");
            if (item.DateOfPreparation.HasValue && item.DateOfApproval.HasValue && item.DateOfPreparation.Value > item.DateOfApproval.Value)
                throw new ValidationException("Дата утверждения не должна быть до даты составления", "DateOfApproval");
            int leaveScheduleCount = await Database.LeaveSchedules.CountAsync(ls => ls.Year == item.Year && ls.Id != item.Id);
            if (leaveScheduleCount > 0)
                throw new ValidationException("График отпусков уже существует", "Year");
        }

        private void InitializeMapperDTO() {
            Mapper.Initialize(cfg => {
                cfg.CreateMap<LeaveScheduleDTO, LeaveSchedule>();
                cfg.CreateMap<AnnualLeaveDTO, AnnualLeave>()
                    .ForMember(al => al.Employee, opt => opt.Ignore())
                    .ForMember(al => al.LeaveSchedule, opt => opt.Ignore());
            });
        }

        // Обновление информации о графике отпусков
        public override async Task EditAsync(LeaveScheduleDTO leaveScheduleDto) {
            await ValidationLeaveSchedule(leaveScheduleDto);
            LeaveSchedule leaveSchedule = await Database.LeaveSchedules.FindByIdAsync(leaveScheduleDto.Id);
            if (leaveSchedule == null) return;
            InitializeMapperDTO();
            Mapper.Map(leaveScheduleDto, leaveSchedule);
            Database.LeaveSchedules.Update(leaveSchedule);
            await Database.SaveAsync();
        }

        // Получение графика отпусков по id
        public override async Task<LeaveScheduleDTO> FindByIdAsync(int? id) {
            if (id == null)
                throw new ValidationException("Не установлен id графика", "");
            LeaveSchedule leaveSchedule = await Database.LeaveSchedules.FindByIdAsync(id.Value);
            if (leaveSchedule == null)
                throw new ValidationException("График не найден", "");
            InitializeMapper();
            LeaveScheduleDTO leaveScheduleDto = Mapper.Map<LeaveSchedule, LeaveScheduleDTO>(leaveSchedule);
            return leaveScheduleDto;
        }

        private void InitializeMapper() {
            Mapper.Initialize(cfg => {
                cfg.CreateMap<LeaveSchedule, LeaveScheduleDTO>();
                cfg.CreateMap<AnnualLeave, AnnualLeaveDTO>()
                    .ForMember(al => al.Employee, opt => opt.Ignore())
                    .ForMember(al => al.LeaveSchedule, opt => opt.Ignore());
            });
        }

        // Получение списка всех графиков отпусков
        public override async Task<IEnumerable<LeaveScheduleDTO>> GetAllAsync() {
            IEnumerable<LeaveSchedule> leaveSchedules = await Database.LeaveSchedules.GetAllAsync();
            InitializeMapper();
            IEnumerable<LeaveScheduleDTO> collection = Mapper.Map<IEnumerable<LeaveSchedule>, IEnumerable<LeaveScheduleDTO>>(leaveSchedules);
            return collection;
        }

        // Удаление графика отпусков
        public override async Task DeleteAsync(int id) {
            LeaveSchedule leaveSchedule = await Database.LeaveSchedules.FindByIdAsync(id);
            if (leaveSchedule == null) return;
            if (leaveSchedule.AnnualLeaves.Count > 0)
                throw new ValidationException("Нельзя удалить график отпусков", "");
            Database.LeaveSchedules.Remove(leaveSchedule);
            await Database.SaveAsync();
        }

        // Удаление всех графиков отпусков
        public override async Task DeleteAllAsync() {
            IEnumerable<LeaveSchedule> leaveSchedules = Database.LeaveSchedules.Get(ls => ls.AnnualLeaves.Count == 0);
            Database.LeaveSchedules.RemoveSeries(leaveSchedules);
            await Database.SaveAsync();
        }

        // Запись информации о графиках отпусков в JSON-файл
        public override async Task ExportJsonAsync(string fullPath) {
            IEnumerable<LeaveSchedule> leaveSchedules = await Database.LeaveSchedules.GetAllAsync();
            var transformLeaveSchedules = leaveSchedules.Select(ls => new {
                ls.Number,
                ls.Year,
                ls.DateOfPreparation,
                ls.DateOfApproval
            }).ToArray();
            using (StreamWriter sw = new StreamWriter(fullPath, true, Encoding.UTF8)) {
                sw.WriteLine("{\"LeaveSchedules\":");
                sw.WriteLine(new JavaScriptSerializer().Serialize(transformLeaveSchedules));
                sw.WriteLine("}");
            }
        }

        // Количество графиков отпусков
        public override async Task<int> CountAsync() {
            return await Database.LeaveSchedules.CountAsync();
        }

        // Количество графиков отпусков, удовлетворяющих предикату
        public override async Task<int> CountAsync(Expression<Func<LeaveScheduleDTO, bool>> predicateDTO) {
            InitializeMapperDTO();
            var predicate = Mapper.Map<Expression<Func<LeaveScheduleDTO, bool>>, Expression<Func<LeaveSchedule, bool>>>(predicateDTO);
            return await Database.LeaveSchedules.CountAsync(predicate);
        }

        public override void Dispose() {
            Database.Dispose();
        }
    }
}
