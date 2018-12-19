using DiplomMSSQLApp.BLL.DTO;
using DiplomMSSQLApp.BLL.Services;
using DiplomMSSQLApp.DAL.Entities;
using DiplomMSSQLApp.DAL.Interfaces;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace DiplomMSSQLApp.BLL.UnitTests {
    [TestFixture]
    public class LeaveScheduleServiceTests : BaseServiceTests<LeaveScheduleService> {
        protected override LeaveScheduleService GetNewService() {
            return new LeaveScheduleService();
        }

        protected override LeaveScheduleService GetNewService(IUnitOfWork uow) {
            return new LeaveScheduleService(uow);
        }

        /// <summary>
        /// // GetPage method
        /// </summary>
        [Test]
        public override void GetPage_CallsWithGoodParams_FillsPageInfoProperty() {
            LeaveScheduleService lss = GetNewService();
            lss.NumberOfObjectsPerPage = 2;
            LeaveScheduleDTO[] col = new LeaveScheduleDTO[] {
                new LeaveScheduleDTO() { Id = 1 },
                new LeaveScheduleDTO() { Id = 2 },
                new LeaveScheduleDTO() { Id = 3 }
            };

            lss.GetPage(col, 1);

            Assert.AreEqual(1, lss.PageInfo.PageNumber);
            Assert.AreEqual(2, lss.PageInfo.PageSize);
            Assert.AreEqual(col.Length, lss.PageInfo.TotalItems);
        }

        [Test]
        public override void GetPage_RequestedPageLessThan1_ReturnsFirstPage() {
            LeaveScheduleService lss = GetNewService();

            lss.GetPage(new LeaveScheduleDTO[0], -5);

            Assert.AreEqual(1, lss.PageInfo.PageNumber);
        }

        [Test]
        public override void GetPage_RequestedPageMoreThanTotalPages_ReturnsLastPage() {
            LeaveScheduleService lss = GetNewService();
            lss.NumberOfObjectsPerPage = 3;
            LeaveScheduleDTO[] col = new LeaveScheduleDTO[] {
                new LeaveScheduleDTO() { Id = 1 },
                new LeaveScheduleDTO() { Id = 2 },
                new LeaveScheduleDTO() { Id = 3 }
            };

            lss.GetPage(col, 2);
            int totalPages = lss.PageInfo.TotalPages;   // 1

            Assert.AreEqual(totalPages, lss.PageInfo.PageNumber);
        }

        [Test]
        public override void GetPage_CallsExistingPage_ReturnsSpecifiedPage() {
            LeaveScheduleService lss = GetNewService();
            lss.NumberOfObjectsPerPage = 3;
            LeaveScheduleDTO[] col = new LeaveScheduleDTO[] {
                new LeaveScheduleDTO() { Id = 1 },
                new LeaveScheduleDTO() { Id = 2 },
                new LeaveScheduleDTO() { Id = 3 },
                new LeaveScheduleDTO() { Id = 4 },
                new LeaveScheduleDTO() { Id = 5 }
            };

            LeaveScheduleDTO[] result = lss.GetPage(col, 2).ToArray();

            Assert.AreEqual(2, result.Length);
            Assert.AreEqual(4, result[0].Id);
            Assert.AreEqual(5, result[1].Id);
        }

        /// <summary>
        /// // CreateAsync method
        /// </summary>
        [Test]
        public void CreateAsync_YearPropertyLessThan1900_Throws() {
            LeaveScheduleService lss = GetNewService();
            LeaveScheduleDTO item = new LeaveScheduleDTO {
                Year = 1899
            };

            Exception ex = Assert.CatchAsync(async () => await lss.CreateAsync(item));

            StringAssert.Contains("Некорректный год", ex.Message);
        }

        [Test]
        public void CreateAsync_YearPropertyMoreThan2100_Throws() {
            LeaveScheduleService lss = GetNewService();
            LeaveScheduleDTO item = new LeaveScheduleDTO {
                Year = 2101
            };

            Exception ex = Assert.CatchAsync(async () => await lss.CreateAsync(item));

            StringAssert.Contains("Некорректный год", ex.Message);
        }

        [Test]
        public void CreateAsync_DateOfPreparationPropertyMoreThanDateOfApprovalProperty_Throws() {
            LeaveScheduleService lss = GetNewService();
            LeaveScheduleDTO item = new LeaveScheduleDTO {
                Year = 2018,
                DateOfPreparation = new DateTime(2000, 11, 20),
                DateOfApproval = new DateTime(2000, 8, 20)
            };

            Exception ex = Assert.CatchAsync(async () => await lss.CreateAsync(item));

            StringAssert.Contains("Дата утверждения не должна быть до даты составления", ex.Message);
        }

        [Test]
        public void CreateAsync_LeaveScheduleWithSameYearAlreadyExists_Throws() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.LeaveSchedules.CountAsync(It.IsAny<Expression<Func<LeaveSchedule, bool>>>())).ReturnsAsync(1);
            LeaveScheduleService lss = GetNewService(mock.Object);
            LeaveScheduleDTO item = new LeaveScheduleDTO {
                Year = 2018
            };

            Exception ex = Assert.CatchAsync(async () => await lss.CreateAsync(item));

            StringAssert.Contains("График отпусков уже существует", ex.Message);
        }

        [Test]
        public async Task CreateAsync_CallsWithGoodParams_CallsCreateMethodOnсe() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.LeaveSchedules.CountAsync(It.IsAny<Expression<Func<LeaveSchedule, bool>>>())).ReturnsAsync(0);
            LeaveScheduleService lss = GetNewService(mock.Object);
            LeaveScheduleDTO item = new LeaveScheduleDTO {
                Year = 2018
            };

            await lss.CreateAsync(item);

            mock.Verify(m => m.LeaveSchedules.Create(It.IsAny<LeaveSchedule>()), Times.Once());
        }

        [Test]
        public async Task CreateAsync_CallsWithGoodParams_CallsSaveAsyncMethodOnсe() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.LeaveSchedules.CountAsync(It.IsAny<Expression<Func<LeaveSchedule, bool>>>())).ReturnsAsync(0);
            LeaveScheduleService lss = GetNewService(mock.Object);
            LeaveScheduleDTO item = new LeaveScheduleDTO {
                Year = 2018
            };

            await lss.CreateAsync(item);

            mock.Verify(m => m.SaveAsync(), Times.Once());
        }

        /// <summary>
        /// // DeleteAsync method
        /// </summary>
        [Test]
        public override async Task DeleteAsync_FindByIdAsyncMethodReturnsNull_RemoveMethodIsNeverCalled() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.LeaveSchedules.FindByIdAsync(It.IsAny<int>())).Returns(Task.FromResult<LeaveSchedule>(null));
            LeaveScheduleService lss = GetNewService(mock.Object);

            await lss.DeleteAsync(It.IsAny<int>());

            mock.Verify(m => m.LeaveSchedules.Remove(It.IsAny<LeaveSchedule>()), Times.Never);
        }

        [Test]
        public override async Task DeleteAsync_FindByIdAsyncMethodReturnsNull_SaveAsyncMethodIsNeverCalled() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.LeaveSchedules.FindByIdAsync(It.IsAny<int>())).Returns(Task.FromResult<LeaveSchedule>(null));
            LeaveScheduleService lss = GetNewService(mock.Object);

            await lss.DeleteAsync(It.IsAny<int>());

            mock.Verify(m => m.SaveAsync(), Times.Never);
        }

        [Test]
        public void DeleteAsync_ExistsAnnualLeaveInThisLeaveSchedule_Throws() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.LeaveSchedules.FindByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new LeaveSchedule { AnnualLeaves = new AnnualLeave[] { new AnnualLeave() } });
            LeaveScheduleService lss = GetNewService(mock.Object);

            Exception ex = Assert.CatchAsync(async () => await lss.DeleteAsync(It.IsAny<int>()));

            StringAssert.Contains("Нельзя удалить график отпусков", ex.Message);
        }

        [Test]
        public override async Task DeleteAsync_FindByIdAsyncMethodReturnsObject_RemoveMethodIsCalledOnce() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.LeaveSchedules.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(new LeaveSchedule());
            LeaveScheduleService lss = GetNewService(mock.Object);

            await lss.DeleteAsync(It.IsAny<int>());

            mock.Verify(m => m.LeaveSchedules.Remove(It.IsAny<LeaveSchedule>()), Times.Once);
        }

        [Test]
        public override async Task DeleteAsync_FindByIdAsyncMethodReturnsObject_SaveAsyncMethodIsCalledOnce() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.LeaveSchedules.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(new LeaveSchedule());
            LeaveScheduleService lss = GetNewService(mock.Object);

            await lss.DeleteAsync(It.IsAny<int>());

            mock.Verify(m => m.SaveAsync(), Times.Once);
        }

        /// <summary>
        /// // DeleteAllAsync method
        /// </summary>
        [Test]
        public async Task DeleteAllAsync_Calls_RemoveSeriesMethodIsCalledOnce() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.LeaveSchedules.Get(It.IsAny<Func<LeaveSchedule, bool>>()));
            LeaveScheduleService lss = GetNewService(mock.Object);

            await lss.DeleteAllAsync();

            mock.Verify(m => m.LeaveSchedules.RemoveSeries(It.IsAny<IEnumerable<LeaveSchedule>>()), Times.Once);
        }

        [Test]
        public override async Task DeleteAllAsync_Calls_SaveAsyncMethodIsCalledOnce() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.LeaveSchedules.RemoveSeries(It.IsAny<IEnumerable<LeaveSchedule>>()));
            LeaveScheduleService lss = GetNewService(mock.Object);

            await lss.DeleteAllAsync();

            mock.Verify(m => m.SaveAsync(), Times.Once);
        }

        /// <summary>
        /// // EditAsync method
        /// </summary>
        [Test]
        public void EditAsync_YearPropertyLessThan1900_Throws() {
            LeaveScheduleService lss = GetNewService();
            LeaveScheduleDTO item = new LeaveScheduleDTO {
                Year = 1899
            };

            Exception ex = Assert.CatchAsync(async () => await lss.EditAsync(item));

            StringAssert.Contains("Некорректный год", ex.Message);
        }

        [Test]
        public void EditAsync_YearPropertyMoreThan2100_Throws() {
            LeaveScheduleService lss = GetNewService();
            LeaveScheduleDTO item = new LeaveScheduleDTO {
                Year = 2101
            };

            Exception ex = Assert.CatchAsync(async () => await lss.EditAsync(item));

            StringAssert.Contains("Некорректный год", ex.Message);
        }

        [Test]
        public void EditAsync_DateOfPreparationPropertyMoreThanDateOfApprovalProperty_Throws() {
            LeaveScheduleService lss = GetNewService();
            LeaveScheduleDTO item = new LeaveScheduleDTO {
                Year = 2018,
                DateOfPreparation = new DateTime(2000, 11, 20),
                DateOfApproval = new DateTime(2000, 8, 20)
            };

            Exception ex = Assert.CatchAsync(async () => await lss.EditAsync(item));

            StringAssert.Contains("Дата утверждения не должна быть до даты составления", ex.Message);
        }

        [Test]
        public void EditAsync_LeaveScheduleWithSameYearAlreadyExists_Throws() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.LeaveSchedules.CountAsync(It.IsAny<Expression<Func<LeaveSchedule, bool>>>())).ReturnsAsync(1);
            LeaveScheduleService lss = GetNewService(mock.Object);
            LeaveScheduleDTO item = new LeaveScheduleDTO {
                Year = 2018
            };

            Exception ex = Assert.CatchAsync(async () => await lss.EditAsync(item));

            StringAssert.Contains("График отпусков уже существует", ex.Message);
        }

        [Test]
        public override async Task EditAsync_CallsWithGoodParams_CallsUpdateMethodOnсe() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.LeaveSchedules.CountAsync(It.IsAny<Expression<Func<LeaveSchedule, bool>>>())).ReturnsAsync(0);
            mock.Setup(m => m.LeaveSchedules.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(new LeaveSchedule { Id = 1 });
            LeaveScheduleService lss = GetNewService(mock.Object);
            LeaveScheduleDTO item = new LeaveScheduleDTO {
                Year = 2018
            };

            await lss.EditAsync(item);

            mock.Verify(m => m.LeaveSchedules.Update(It.IsAny<LeaveSchedule>()), Times.Once);
        }

        [Test]
        public override async Task EditAsync_CallsWithGoodParams_CallsSaveAsyncMethodOnсe() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.LeaveSchedules.CountAsync(It.IsAny<Expression<Func<LeaveSchedule, bool>>>())).ReturnsAsync(0);
            mock.Setup(m => m.LeaveSchedules.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(new LeaveSchedule { Id = 1 });
            LeaveScheduleService lss = GetNewService(mock.Object);
            LeaveScheduleDTO item = new LeaveScheduleDTO {
                Year = 2018
            };

            await lss.EditAsync(item);

            mock.Verify(m => m.SaveAsync(), Times.Once);
        }

        /// <summary>
        /// // FindByIdAsync method
        /// </summary>
        [Test]
        public override void FindByIdAsync_IdParameterIsNull_Throws() {
            LeaveScheduleService lss = GetNewService();

            Exception ex = Assert.CatchAsync(async () => await lss.FindByIdAsync(null));

            StringAssert.Contains("Не установлен id графика", ex.Message);
        }

        [Test]
        public void FindByIdAsync_LeaveScheduleNotFound_Throws() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.LeaveSchedules.FindByIdAsync(It.IsAny<int>())).Returns(Task.FromResult<LeaveSchedule>(null));
            LeaveScheduleService lss = GetNewService(mock.Object);

            Exception ex = Assert.CatchAsync(async () => await lss.FindByIdAsync(It.IsAny<int>()));

            StringAssert.Contains("График не найден", ex.Message);
        }

        [Test]
        public override async Task FindByIdAsync_IdEqualTo2_ReturnsObjectWithIdEqualTo2() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.LeaveSchedules.FindByIdAsync(It.IsAny<int>())).ReturnsAsync((int item_id) => new LeaveSchedule() { Id = item_id });
            LeaveScheduleService lss = GetNewService(mock.Object);

            LeaveScheduleDTO result = await lss.FindByIdAsync(2);

            Assert.AreEqual(2, result.Id);
        }

        /// <summary>
        /// // GetAllAsync method
        /// </summary>
        [Test]
        public override async Task GetAllAsync_GetAsyncMethodReturnsArray_ReturnsSameArray() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.LeaveSchedules.GetAllAsync()).ReturnsAsync(() => new LeaveSchedule[] {
                new LeaveSchedule() { Id = 1 },
                new LeaveSchedule() { Id = 2 },
                new LeaveSchedule() { Id = 3 }
            });
            LeaveScheduleService lss = GetNewService(mock.Object);

            LeaveScheduleDTO[] result = (await lss.GetAllAsync()).ToArray();

            Assert.AreEqual(1, result[0].Id);
            Assert.AreEqual(2, result[1].Id);
            Assert.AreEqual(3, result[2].Id);
        }

        /// <summary>
        /// // ExportJsonAsync method
        /// </summary>
        [Test]
        public override async Task ExportJsonAsync_CreatesJsonFile() {
            string fullPath = "./DiplomMSSQLApp.WEB/Results/LeaveSchedules.json";
            File.Delete(fullPath);
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.LeaveSchedules.GetAllAsync()).ReturnsAsync(new LeaveSchedule[] { });
            LeaveScheduleService LeaveScheduleService = GetNewService(mock.Object);

            await LeaveScheduleService.ExportJsonAsync(fullPath);

            Assert.IsTrue(File.Exists(fullPath));
        }
    }
}
