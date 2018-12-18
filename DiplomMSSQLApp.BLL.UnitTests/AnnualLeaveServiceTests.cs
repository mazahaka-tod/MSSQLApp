using DiplomMSSQLApp.BLL.BusinessModels;
using DiplomMSSQLApp.BLL.DTO;
using DiplomMSSQLApp.BLL.Services;
using DiplomMSSQLApp.DAL.Entities;
using DiplomMSSQLApp.DAL.Interfaces;
using Moq;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace DiplomMSSQLApp.BLL.UnitTests {
    [TestFixture]
    public class AnnualLeaveServiceTests : BaseServiceTests<AnnualLeaveService> {
        protected override AnnualLeaveService GetNewService() {
            return new AnnualLeaveService();
        }

        protected override AnnualLeaveService GetNewService(IUnitOfWork uow) {
            return new AnnualLeaveService(uow);
        }

        /// <summary>
        /// // GetPage method
        /// </summary>
        [Test]
        public override void GetPage_CallsWithGoodParams_FillsPageInfoProperty() {
            AnnualLeaveService als = GetNewService();
            als.NumberOfObjectsPerPage = 2;
            AnnualLeaveDTO[] col = new AnnualLeaveDTO[] {
                new AnnualLeaveDTO() { Id = 1 },
                new AnnualLeaveDTO() { Id = 2 },
                new AnnualLeaveDTO() { Id = 3 }
            };

            als.GetPage(col, 1);

            Assert.AreEqual(1, als.PageInfo.PageNumber);
            Assert.AreEqual(2, als.PageInfo.PageSize);
            Assert.AreEqual(col.Length, als.PageInfo.TotalItems);
        }

        [Test]
        public override void GetPage_RequestedPageLessThan1_ReturnsFirstPage() {
            AnnualLeaveService als = GetNewService();

            als.GetPage(new AnnualLeaveDTO[0], -5);

            Assert.AreEqual(1, als.PageInfo.PageNumber);
        }

        [Test]
        public override void GetPage_RequestedPageMoreThanTotalPages_ReturnsLastPage() {
            AnnualLeaveService als = GetNewService();
            als.NumberOfObjectsPerPage = 3;
            AnnualLeaveDTO[] col = new AnnualLeaveDTO[] {
                new AnnualLeaveDTO() { Id = 1 },
                new AnnualLeaveDTO() { Id = 2 },
                new AnnualLeaveDTO() { Id = 3 }
            };

            als.GetPage(col, 2);
            int totalPages = als.PageInfo.TotalPages;   // 1

            Assert.AreEqual(totalPages, als.PageInfo.PageNumber);
        }

        [Test]
        public override void GetPage_CallsExistingPage_ReturnsSpecifiedPage() {
            AnnualLeaveService als = GetNewService();
            als.NumberOfObjectsPerPage = 3;
            AnnualLeaveDTO[] col = new AnnualLeaveDTO[] {
                new AnnualLeaveDTO() { Id = 1 },
                new AnnualLeaveDTO() { Id = 2 },
                new AnnualLeaveDTO() { Id = 3 },
                new AnnualLeaveDTO() { Id = 4 },
                new AnnualLeaveDTO() { Id = 5 }
            };

            AnnualLeaveDTO[] result = als.GetPage(col, 2).ToArray();

            Assert.AreEqual(2, result.Length);
            Assert.AreEqual(4, result[0].Id);
            Assert.AreEqual(5, result[1].Id);
        }

        /// <summary>
        /// // CreateAsync method
        /// </summary>
        [Test]
        public void CreateAsync_ScheduledDatePropertyIsNull_Throws() {
            AnnualLeaveService als = GetNewService();
            AnnualLeaveDTO item = new AnnualLeaveDTO {
                ScheduledDate = null
            };

            Exception ex = Assert.CatchAsync(async () => await als.CreateAsync(item));

            StringAssert.Contains("Требуется ввести запланированную дату отпуска", ex.Message);
        }

        [Test]
        public void CreateAsync_LeaveScheduleNotFound_Throws() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.LeaveSchedules.CountAsync(It.IsAny<Expression<Func<LeaveSchedule, bool>>>())).ReturnsAsync(0);
            AnnualLeaveService als = GetNewService(mock.Object);
            AnnualLeaveDTO item = new AnnualLeaveDTO {
                ScheduledDate = new DateTime(2000, 8, 20)
            };

            Exception ex = Assert.CatchAsync(async () => await als.CreateAsync(item));

            StringAssert.Contains("Не найден график отпусков на 2000 год", ex.Message);
        }

        [Test]
        public void CreateAsync_ScheduledNumberOfDaysPropertyIsNull_Throws() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.LeaveSchedules.CountAsync(It.IsAny<Expression<Func<LeaveSchedule, bool>>>())).ReturnsAsync(1);
            AnnualLeaveService als = GetNewService(mock.Object);
            AnnualLeaveDTO item = new AnnualLeaveDTO {
                ScheduledDate = new DateTime(2000, 8, 20),
                ScheduledNumberOfDays = null
            };

            Exception ex = Assert.CatchAsync(async () => await als.CreateAsync(item));

            StringAssert.Contains("Требуется ввести запланированное количество дней отпуска", ex.Message);
        }

        [Test]
        public void CreateAsync_ScheduledNumberOfDaysPropertyLessThan1_Throws() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.LeaveSchedules.CountAsync(It.IsAny<Expression<Func<LeaveSchedule, bool>>>())).ReturnsAsync(1);
            AnnualLeaveService als = GetNewService(mock.Object);
            AnnualLeaveDTO item = new AnnualLeaveDTO {
                ScheduledDate = new DateTime(2000, 8, 20),
                ScheduledNumberOfDays = 0
            };

            Exception ex = Assert.CatchAsync(async () => await als.CreateAsync(item));

            StringAssert.Contains("Некорректное значение", ex.Message);
        }

        [Test]
        public void CreateAsync_ScheduledNumberOfDaysPropertyMoreThan1000_Throws() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.LeaveSchedules.CountAsync(It.IsAny<Expression<Func<LeaveSchedule, bool>>>())).ReturnsAsync(1);
            AnnualLeaveService als = GetNewService(mock.Object);
            AnnualLeaveDTO item = new AnnualLeaveDTO {
                ScheduledDate = new DateTime(2000, 8, 20),
                ScheduledNumberOfDays = 1001
            };

            Exception ex = Assert.CatchAsync(async () => await als.CreateAsync(item));

            StringAssert.Contains("Некорректное значение", ex.Message);
        }

        [Test]
        public void CreateAsync_ActualDateYearNotEqualsScheduledDateYear_Throws() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.LeaveSchedules.CountAsync(It.IsAny<Expression<Func<LeaveSchedule, bool>>>())).ReturnsAsync(1);
            AnnualLeaveService als = GetNewService(mock.Object);
            AnnualLeaveDTO item = new AnnualLeaveDTO {
                ScheduledDate = new DateTime(2000, 8, 20),
                ScheduledNumberOfDays = 1,
                ActualDate = new DateTime(2001, 8, 20)
            };

            Exception ex = Assert.CatchAsync(async () => await als.CreateAsync(item));

            StringAssert.Contains("Некорректный год", ex.Message);
        }

        [Test]
        public void CreateAsync_ActualDatePropertyHasValueAndActualNumberOfDaysPropertyIsNull_Throws() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.LeaveSchedules.CountAsync(It.IsAny<Expression<Func<LeaveSchedule, bool>>>())).ReturnsAsync(1);
            AnnualLeaveService als = GetNewService(mock.Object);
            AnnualLeaveDTO item = new AnnualLeaveDTO {
                ScheduledDate = new DateTime(2000, 8, 20),
                ScheduledNumberOfDays = 1,
                ActualDate = new DateTime(2000, 8, 20),
                ActualNumberOfDays = null
            };

            Exception ex = Assert.CatchAsync(async () => await als.CreateAsync(item));

            StringAssert.Contains("Требуется ввести значение", ex.Message);
        }

        [Test]
        public void CreateAsync_ActualNumberOfDaysPropertyLessThan1_Throws() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.LeaveSchedules.CountAsync(It.IsAny<Expression<Func<LeaveSchedule, bool>>>())).ReturnsAsync(1);
            AnnualLeaveService als = GetNewService(mock.Object);
            AnnualLeaveDTO item = new AnnualLeaveDTO {
                ScheduledDate = new DateTime(2000, 8, 20),
                ScheduledNumberOfDays = 1,
                ActualDate = new DateTime(2000, 8, 20),
                ActualNumberOfDays = 0
            };

            Exception ex = Assert.CatchAsync(async () => await als.CreateAsync(item));

            StringAssert.Contains("Некорректное значение", ex.Message);
        }

        [Test]
        public void CreateAsync_ActualNumberOfDaysPropertyMoreThan1000_Throws() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.LeaveSchedules.CountAsync(It.IsAny<Expression<Func<LeaveSchedule, bool>>>())).ReturnsAsync(1);
            AnnualLeaveService als = GetNewService(mock.Object);
            AnnualLeaveDTO item = new AnnualLeaveDTO {
                ScheduledDate = new DateTime(2000, 8, 20),
                ScheduledNumberOfDays = 1,
                ActualDate = new DateTime(2000, 8, 20),
                ActualNumberOfDays = 1001
            };

            Exception ex = Assert.CatchAsync(async () => await als.CreateAsync(item));

            StringAssert.Contains("Некорректное значение", ex.Message);
        }

        [Test]
        public void CreateAsync_ActualNumberOfDaysPropertyHasValueAndActualDatePropertyIsNull_Throws() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.LeaveSchedules.CountAsync(It.IsAny<Expression<Func<LeaveSchedule, bool>>>())).ReturnsAsync(1);
            AnnualLeaveService als = GetNewService(mock.Object);
            AnnualLeaveDTO item = new AnnualLeaveDTO {
                ScheduledDate = new DateTime(2000, 8, 20),
                ScheduledNumberOfDays = 1,
                ActualDate = null,
                ActualNumberOfDays = 1
            };

            Exception ex = Assert.CatchAsync(async () => await als.CreateAsync(item));

            StringAssert.Contains("Требуется ввести значение", ex.Message);
        }

        [Test]
        public async Task CreateAsync_CallsWithGoodParams_CallsCreateMethodOnсe() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.LeaveSchedules.CountAsync(It.IsAny<Expression<Func<LeaveSchedule, bool>>>())).ReturnsAsync(1);
            mock.Setup(m => m.LeaveSchedules.GetFirstAsync(It.IsAny<Expression<Func<LeaveSchedule, bool>>>())).ReturnsAsync(new LeaveSchedule { Id = 1 });
            mock.Setup(m => m.AnnualLeaves.Create(It.IsAny<AnnualLeave>()));
            AnnualLeaveService als = GetNewService(mock.Object);
            AnnualLeaveDTO item = new AnnualLeaveDTO {
                ScheduledDate = new DateTime(2000, 8, 20),
                ScheduledNumberOfDays = 1,
                ActualDate = new DateTime(2000, 8, 20),
                ActualNumberOfDays = 1
            };

            await als.CreateAsync(item);

            mock.Verify(m => m.AnnualLeaves.Create(It.IsAny<AnnualLeave>()), Times.Once());
        }

        [Test]
        public async Task CreateAsync_CallsWithGoodParams_CallsSaveAsyncMethodOnсe() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.LeaveSchedules.CountAsync(It.IsAny<Expression<Func<LeaveSchedule, bool>>>())).ReturnsAsync(1);
            mock.Setup(m => m.LeaveSchedules.GetFirstAsync(It.IsAny<Expression<Func<LeaveSchedule, bool>>>())).ReturnsAsync(new LeaveSchedule { Id = 1 });
            mock.Setup(m => m.AnnualLeaves.Create(It.IsAny<AnnualLeave>()));
            AnnualLeaveService als = GetNewService(mock.Object);
            AnnualLeaveDTO item = new AnnualLeaveDTO {
                ScheduledDate = new DateTime(2000, 8, 20),
                ScheduledNumberOfDays = 1,
                ActualDate = new DateTime(2000, 8, 20),
                ActualNumberOfDays = 1
            };

            await als.CreateAsync(item);

            mock.Verify(m => m.SaveAsync(), Times.Once());
        }

        /// <summary>
        /// // DeleteAsync method
        /// </summary>
        [Test]
        public override async Task DeleteAsync_FindByIdAsyncMethodReturnsNull_RemoveMethodIsNeverCalled() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.AnnualLeaves.FindByIdAsync(It.IsAny<int>())).Returns(Task.FromResult<AnnualLeave>(null));
            AnnualLeaveService als = GetNewService(mock.Object);

            await als.DeleteAsync(It.IsAny<int>());

            mock.Verify(m => m.AnnualLeaves.Remove(It.IsAny<AnnualLeave>()), Times.Never);
        }

        [Test]
        public override async Task DeleteAsync_FindByIdAsyncMethodReturnsNull_SaveAsyncMethodIsNeverCalled() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.AnnualLeaves.FindByIdAsync(It.IsAny<int>())).Returns(Task.FromResult<AnnualLeave>(null));
            AnnualLeaveService als = GetNewService(mock.Object);

            await als.DeleteAsync(It.IsAny<int>());

            mock.Verify(m => m.SaveAsync(), Times.Never);
        }

        [Test]
        public override async Task DeleteAsync_FindByIdAsyncMethodReturnsObject_RemoveMethodIsCalledOnce() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.AnnualLeaves.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(new AnnualLeave());
            AnnualLeaveService als = GetNewService(mock.Object);

            await als.DeleteAsync(It.IsAny<int>());

            mock.Verify(m => m.AnnualLeaves.Remove(It.IsAny<AnnualLeave>()), Times.Once);
        }

        [Test]
        public override async Task DeleteAsync_FindByIdAsyncMethodReturnsObject_SaveAsyncMethodIsCalledOnce() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.AnnualLeaves.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(new AnnualLeave());
            AnnualLeaveService als = GetNewService(mock.Object);

            await als.DeleteAsync(It.IsAny<int>());

            mock.Verify(m => m.SaveAsync(), Times.Once);
        }

        /// <summary>
        /// // DeleteAllAsync method
        /// </summary>
        [Test]
        public async Task DeleteAllAsync_Calls_RemoveAllAsyncMethodIsCalledOnce() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.AnnualLeaves.RemoveAllAsync()).Returns(Task.CompletedTask);
            AnnualLeaveService als = GetNewService(mock.Object);

            await als.DeleteAllAsync();

            mock.Verify(m => m.AnnualLeaves.RemoveAllAsync(), Times.Once);
        }

        [Test]
        public override async Task DeleteAllAsync_Calls_SaveAsyncMethodIsCalledOnce() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.AnnualLeaves.RemoveAllAsync()).Returns(Task.CompletedTask);
            AnnualLeaveService als = GetNewService(mock.Object);

            await als.DeleteAllAsync();

            mock.Verify(m => m.SaveAsync(), Times.Once);
        }

        /// <summary>
        /// // EditAsync method
        /// </summary>
        [Test]
        public void EditAsync_ScheduledDatePropertyIsNull_Throws() {
            AnnualLeaveService als = GetNewService();
            AnnualLeaveDTO item = new AnnualLeaveDTO {
                ScheduledDate = null
            };

            Exception ex = Assert.CatchAsync(async () => await als.EditAsync(item));

            StringAssert.Contains("Требуется ввести запланированную дату отпуска", ex.Message);
        }

        [Test]
        public void EditAsync_LeaveScheduleNotFound_Throws() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.LeaveSchedules.CountAsync(It.IsAny<Expression<Func<LeaveSchedule, bool>>>())).ReturnsAsync(0);
            AnnualLeaveService als = GetNewService(mock.Object);
            AnnualLeaveDTO item = new AnnualLeaveDTO {
                ScheduledDate = new DateTime(2000, 8, 20)
            };

            Exception ex = Assert.CatchAsync(async () => await als.EditAsync(item));

            StringAssert.Contains("Не найден график отпусков на 2000 год", ex.Message);
        }

        [Test]
        public void EditAsync_ScheduledNumberOfDaysPropertyIsNull_Throws() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.LeaveSchedules.CountAsync(It.IsAny<Expression<Func<LeaveSchedule, bool>>>())).ReturnsAsync(1);
            AnnualLeaveService als = GetNewService(mock.Object);
            AnnualLeaveDTO item = new AnnualLeaveDTO {
                ScheduledDate = new DateTime(2000, 8, 20),
                ScheduledNumberOfDays = null
            };

            Exception ex = Assert.CatchAsync(async () => await als.EditAsync(item));

            StringAssert.Contains("Требуется ввести запланированное количество дней отпуска", ex.Message);
        }

        [Test]
        public void EditAsync_ScheduledNumberOfDaysPropertyLessThan1_Throws() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.LeaveSchedules.CountAsync(It.IsAny<Expression<Func<LeaveSchedule, bool>>>())).ReturnsAsync(1);
            AnnualLeaveService als = GetNewService(mock.Object);
            AnnualLeaveDTO item = new AnnualLeaveDTO {
                ScheduledDate = new DateTime(2000, 8, 20),
                ScheduledNumberOfDays = 0
            };

            Exception ex = Assert.CatchAsync(async () => await als.EditAsync(item));

            StringAssert.Contains("Некорректное значение", ex.Message);
        }

        [Test]
        public void EditAsync_ScheduledNumberOfDaysPropertyMoreThan1000_Throws() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.LeaveSchedules.CountAsync(It.IsAny<Expression<Func<LeaveSchedule, bool>>>())).ReturnsAsync(1);
            AnnualLeaveService als = GetNewService(mock.Object);
            AnnualLeaveDTO item = new AnnualLeaveDTO {
                ScheduledDate = new DateTime(2000, 8, 20),
                ScheduledNumberOfDays = 1001
            };

            Exception ex = Assert.CatchAsync(async () => await als.EditAsync(item));

            StringAssert.Contains("Некорректное значение", ex.Message);
        }

        [Test]
        public void EditAsync_ActualDateYearNotEqualsScheduledDateYear_Throws() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.LeaveSchedules.CountAsync(It.IsAny<Expression<Func<LeaveSchedule, bool>>>())).ReturnsAsync(1);
            AnnualLeaveService als = GetNewService(mock.Object);
            AnnualLeaveDTO item = new AnnualLeaveDTO {
                ScheduledDate = new DateTime(2000, 8, 20),
                ScheduledNumberOfDays = 1,
                ActualDate = new DateTime(2001, 8, 20)
            };

            Exception ex = Assert.CatchAsync(async () => await als.EditAsync(item));

            StringAssert.Contains("Некорректный год", ex.Message);
        }

        [Test]
        public void EditAsync_ActualDatePropertyHasValueAndActualNumberOfDaysPropertyIsNull_Throws() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.LeaveSchedules.CountAsync(It.IsAny<Expression<Func<LeaveSchedule, bool>>>())).ReturnsAsync(1);
            AnnualLeaveService als = GetNewService(mock.Object);
            AnnualLeaveDTO item = new AnnualLeaveDTO {
                ScheduledDate = new DateTime(2000, 8, 20),
                ScheduledNumberOfDays = 1,
                ActualDate = new DateTime(2000, 8, 20),
                ActualNumberOfDays = null
            };

            Exception ex = Assert.CatchAsync(async () => await als.EditAsync(item));

            StringAssert.Contains("Требуется ввести значение", ex.Message);
        }

        [Test]
        public void EditAsync_ActualNumberOfDaysPropertyLessThan1_Throws() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.LeaveSchedules.CountAsync(It.IsAny<Expression<Func<LeaveSchedule, bool>>>())).ReturnsAsync(1);
            AnnualLeaveService als = GetNewService(mock.Object);
            AnnualLeaveDTO item = new AnnualLeaveDTO {
                ScheduledDate = new DateTime(2000, 8, 20),
                ScheduledNumberOfDays = 1,
                ActualDate = new DateTime(2000, 8, 20),
                ActualNumberOfDays = 0
            };

            Exception ex = Assert.CatchAsync(async () => await als.EditAsync(item));

            StringAssert.Contains("Некорректное значение", ex.Message);
        }

        [Test]
        public void EditAsync_ActualNumberOfDaysPropertyMoreThan1000_Throws() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.LeaveSchedules.CountAsync(It.IsAny<Expression<Func<LeaveSchedule, bool>>>())).ReturnsAsync(1);
            AnnualLeaveService als = GetNewService(mock.Object);
            AnnualLeaveDTO item = new AnnualLeaveDTO {
                ScheduledDate = new DateTime(2000, 8, 20),
                ScheduledNumberOfDays = 1,
                ActualDate = new DateTime(2000, 8, 20),
                ActualNumberOfDays = 1001
            };

            Exception ex = Assert.CatchAsync(async () => await als.EditAsync(item));

            StringAssert.Contains("Некорректное значение", ex.Message);
        }

        [Test]
        public void EditAsync_ActualNumberOfDaysPropertyHasValueAndActualDatePropertyIsNull_Throws() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.LeaveSchedules.CountAsync(It.IsAny<Expression<Func<LeaveSchedule, bool>>>())).ReturnsAsync(1);
            AnnualLeaveService als = GetNewService(mock.Object);
            AnnualLeaveDTO item = new AnnualLeaveDTO {
                ScheduledDate = new DateTime(2000, 8, 20),
                ScheduledNumberOfDays = 1,
                ActualDate = null,
                ActualNumberOfDays = 1
            };

            Exception ex = Assert.CatchAsync(async () => await als.EditAsync(item));

            StringAssert.Contains("Требуется ввести значение", ex.Message);
        }

        [Test]
        public override async Task EditAsync_CallsWithGoodParams_CallsUpdateMethodOnсe() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.LeaveSchedules.CountAsync(It.IsAny<Expression<Func<LeaveSchedule, bool>>>())).ReturnsAsync(1);
            mock.Setup(m => m.LeaveSchedules.GetFirstAsync(It.IsAny<Expression<Func<LeaveSchedule, bool>>>())).ReturnsAsync(new LeaveSchedule { Id = 1 });
            mock.Setup(m => m.AnnualLeaves.Update(It.IsAny<AnnualLeave>()));
            AnnualLeaveService als = GetNewService(mock.Object);
            AnnualLeaveDTO item = new AnnualLeaveDTO {
                ScheduledDate = new DateTime(2000, 8, 20),
                ScheduledNumberOfDays = 1,
                ActualDate = new DateTime(2000, 8, 20),
                ActualNumberOfDays = 1
            };

            await als.EditAsync(item);

            mock.Verify(m => m.AnnualLeaves.Update(It.IsAny<AnnualLeave>()), Times.Once);
        }

        [Test]
        public override async Task EditAsync_CallsWithGoodParams_CallsSaveAsyncMethodOnсe() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.LeaveSchedules.CountAsync(It.IsAny<Expression<Func<LeaveSchedule, bool>>>())).ReturnsAsync(1);
            mock.Setup(m => m.LeaveSchedules.GetFirstAsync(It.IsAny<Expression<Func<LeaveSchedule, bool>>>())).ReturnsAsync(new LeaveSchedule { Id = 1 });
            mock.Setup(m => m.AnnualLeaves.Update(It.IsAny<AnnualLeave>()));
            AnnualLeaveService als = GetNewService(mock.Object);
            AnnualLeaveDTO item = new AnnualLeaveDTO {
                ScheduledDate = new DateTime(2000, 8, 20),
                ScheduledNumberOfDays = 1,
                ActualDate = new DateTime(2000, 8, 20),
                ActualNumberOfDays = 1
            };

            await als.EditAsync(item);

            mock.Verify(m => m.SaveAsync(), Times.Once);
        }

        /// <summary>
        /// // FindByIdAsync method
        /// </summary>
        [Test]
        public override void FindByIdAsync_IdParameterIsNull_Throws() {
            AnnualLeaveService als = GetNewService();

            Exception ex = Assert.CatchAsync(async () => await als.FindByIdAsync(null));

            StringAssert.Contains("Не установлен id отпуска", ex.Message);
        }

        [Test]
        public void FindByIdAsync_AnnualLeaveNotFound_Throws() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.AnnualLeaves.FindByIdAsync(It.IsAny<int>())).Returns(Task.FromResult<AnnualLeave>(null));
            AnnualLeaveService als = GetNewService(mock.Object);

            Exception ex = Assert.CatchAsync(async () => await als.FindByIdAsync(It.IsAny<int>()));

            StringAssert.Contains("Отпуск не найден", ex.Message);
        }

        [Test]
        public override async Task FindByIdAsync_IdEqualTo2_ReturnsObjectWithIdEqualTo2() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.AnnualLeaves.FindByIdAsync(It.IsAny<int>())).ReturnsAsync((int item_id) => new AnnualLeave() { Id = item_id });
            AnnualLeaveService als = GetNewService(mock.Object);

            AnnualLeaveDTO result = await als.FindByIdAsync(2);

            Assert.AreEqual(2, result.Id);
        }

        /// <summary>
        /// // Get method
        /// </summary>
        [TestCase("Иванов", null, null, null, null, null, null, null)]          // Name
        [TestCase(null, "Программист", null, null, null, null, null, null)]     // PostTitle
        [TestCase(null, null, "ИТ", null, null, null, null, null)]              // DepartmentName
        [TestCase(null, null, null, "20.11.2018", null, null, null, null)]      // ScheduledDate
        [TestCase(null, null, null, null, "27.11.2018", null, null, null)]      // ActualDate
        [TestCase(null, null, null, null, null, 28, null, null)]                // MinNumberOfDaysOfLeave
        [TestCase(null, null, null, null, null, null, 21, null)]                // MinScheduledNumberOfDays
        [TestCase(null, null, null, null, null, null, null, 28)]                // MinActualNumberOfDays
        public void Get_OneFilterParameterIsSet_ReturnsFilteredArray(string name, string title, string depName, string schDate, string actDate,
            int? minNum, int? minSchNum, int? minActNum) {
            AnnualLeaveFilter filter = new AnnualLeaveFilter {
                Name = new string[] { name, null, "" },
                PostTitle = new string[] { title, null, "" },
                DepartmentName = new string[] { depName, null, "" },
                ScheduledDate = schDate,
                ActualDate = actDate,
                MinNumberOfDaysOfLeave = minNum,
                MinScheduledNumberOfDays = minSchNum,
                MinActualNumberOfDays = minActNum
            };
            AnnualLeave[] AnnualLeaves = new AnnualLeave[] {
                new AnnualLeave() {
                    Id = 1,
                    Employee = new Employee { LastName = "Иванов", FirstName = "Иван",
                        Post = new Post { Title = "Программист", NumberOfDaysOfLeave = 28,
                            Department = new Department { DepartmentName = "ИТ" } } },
                    ScheduledDate = new DateTime(2018, 11, 20),
                    ActualDate = new DateTime(2018, 11, 27),
                    ScheduledNumberOfDays = 21,
                    ActualNumberOfDays = 28
                },
                new AnnualLeave() {
                    Id = 2,
                    Employee = new Employee { LastName = "Иванов", FirstName = "Петр",
                        Post = new Post { Title = "Программист", NumberOfDaysOfLeave = 28,
                            Department = new Department { DepartmentName = "ИТ" } } },
                    ScheduledDate = new DateTime(2018, 11, 20),
                    ActualDate = new DateTime(2018, 11, 27),
                    ScheduledNumberOfDays = 21,
                    ActualNumberOfDays = 28
                },
                new AnnualLeave() {
                    Id = 3,
                    Employee = new Employee { LastName = "Сидоров", FirstName = "Иван",
                        Post = new Post { Title = "Специалист", NumberOfDaysOfLeave = 21,
                            Department = new Department { DepartmentName = "Отдел кадров" } } },
                    ScheduledDate = new DateTime(2018, 09, 12),
                    ActualDate = new DateTime(2018, 09, 11),
                    ScheduledNumberOfDays = 18,
                    ActualNumberOfDays = 20
                },
                new AnnualLeave() {
                    Id = 4,
                    Employee = new Employee { LastName = "Петров", FirstName = "Иван",
                        Post = new Post { Title = "Специалист", NumberOfDaysOfLeave = 24,
                            Department = new Department { DepartmentName = "Отдел кадров" } } },
                    ScheduledDate = new DateTime(2018, 09, 01),
                    ActualDate = new DateTime(2018, 09, 05),
                    ScheduledNumberOfDays = 19,
                    ActualNumberOfDays = 27
                }
            };
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.AnnualLeaves.Get(It.IsAny<Func<AnnualLeave, bool>>()))
                .Returns((Func<AnnualLeave, bool> predicate) => AnnualLeaves.Where(predicate));
            AnnualLeaveService AnnualLeaveService = GetNewService(mock.Object);

            AnnualLeaveDTO[] result = AnnualLeaveService.Get(filter).ToArray();

            Assert.AreEqual(2, result.Length);
            Assert.AreEqual(1, result[0].Id);
            Assert.AreEqual(2, result[1].Id);
        }

        [TestCase("Иванов", null, null, null, null, null, null, null)]          // Name
        [TestCase(null, "Программист", null, null, null, null, null, null)]     // PostTitle
        [TestCase(null, null, "ИТ", null, null, null, null, null)]              // DepartmentName
        [TestCase(null, null, null, "20.11.2018", null, null, null, null)]      // ScheduledDate
        [TestCase(null, null, null, null, "27.11.2018", null, null, null)]      // ActualDate
        [TestCase(null, null, null, null, null, 28, null, null)]                // MinNumberOfDaysOfLeave
        [TestCase(null, null, null, null, null, null, 21, null)]                // MinScheduledNumberOfDays
        [TestCase(null, null, null, null, null, null, null, 28)]                // MinActualNumberOfDays
        public void Get_OneFilterParameterAndIsAntiFilterIsSet_ReturnsFilteredArray(string name, string title, string depName, string schDate, string actDate,
            int? minNum, int? minSchNum, int? minActNum) {
            AnnualLeaveFilter filter = new AnnualLeaveFilter {
                Name = new string[] { name, null, "" },
                PostTitle = new string[] { title, null, "" },
                DepartmentName = new string[] { depName, null, "" },
                ScheduledDate = schDate,
                ActualDate = actDate,
                MinNumberOfDaysOfLeave = minNum,
                MinScheduledNumberOfDays = minSchNum,
                MinActualNumberOfDays = minActNum,
                IsAntiFilter = true
            };
            AnnualLeave[] AnnualLeaves = new AnnualLeave[] {
                new AnnualLeave() {
                    Id = 1,
                    Employee = new Employee { LastName = "Иванов", FirstName = "Иван",
                        Post = new Post { Title = "Программист", NumberOfDaysOfLeave = 28,
                            Department = new Department { DepartmentName = "ИТ" } } },
                    ScheduledDate = new DateTime(2018, 11, 20),
                    ActualDate = new DateTime(2018, 11, 27),
                    ScheduledNumberOfDays = 21,
                    ActualNumberOfDays = 28
                },
                new AnnualLeave() {
                    Id = 2,
                    Employee = new Employee { LastName = "Иванов", FirstName = "Петр",
                        Post = new Post { Title = "Программист", NumberOfDaysOfLeave = 28,
                            Department = new Department { DepartmentName = "ИТ" } } },
                    ScheduledDate = new DateTime(2018, 11, 20),
                    ActualDate = new DateTime(2018, 11, 27),
                    ScheduledNumberOfDays = 21,
                    ActualNumberOfDays = 28
                },
                new AnnualLeave() {
                    Id = 3,
                    Employee = new Employee { LastName = "Петров", FirstName = "Иван",
                        Post = new Post { Title = "Специалист", NumberOfDaysOfLeave = 21,
                            Department = new Department { DepartmentName = "Отдел кадров" } } },
                    ScheduledDate = new DateTime(2018, 09, 12),
                    ActualDate = new DateTime(2018, 09, 11),
                    ScheduledNumberOfDays = 18,
                    ActualNumberOfDays = 20
                },
                new AnnualLeave() {
                    Id = 4,
                    Employee = new Employee {  LastName = "Сидоров", FirstName = "Иван",
                        Post = new Post { Title = "Специалист", NumberOfDaysOfLeave = 24,
                            Department = new Department { DepartmentName = "Отдел кадров" } } },
                    ScheduledDate = new DateTime(2018, 09, 01),
                    ActualDate = new DateTime(2018, 09, 05),
                    ScheduledNumberOfDays = 19,
                    ActualNumberOfDays = 27
                }
            };
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.AnnualLeaves.Get(It.IsAny<Func<AnnualLeave, bool>>()))
                .Returns((Func<AnnualLeave, bool> predicate) => AnnualLeaves.Where(predicate));
            AnnualLeaveService AnnualLeaveService = GetNewService(mock.Object);

            AnnualLeaveDTO[] result = AnnualLeaveService.Get(filter).ToArray();

            Assert.AreEqual(2, result.Length);
            Assert.AreEqual(3, result[0].Id);
            Assert.AreEqual(4, result[1].Id);
        }

        [TestCase("Иванов", "Петров", null, null, null, null, null, null, null, null, null, null, null, null)]              // Name
        [TestCase(null, null, "Программист", "Специалист", null, null, null, null, null, null, null, null, null, null)]     // PostTitle
        [TestCase(null, null, null, null, "ИТ", "Отдел кадров", null, null, null, null, null, null, null, null)]            // DepartmentName
        [TestCase(null, null, null, null, null, null, "20.11.2018", "27.11.2018", null, null, null, null, null, null)]      // ScheduledDate, ActualDate
        [TestCase(null, null, null, null, null, null, null, null, 30, 35, null, null, null, null)]                          // NumberOfDaysOfLeave
        [TestCase(null, null, null, null, null, null, null, null, null, null, 15, 20, null, null)]                          // ScheduledNumberOfDays
        [TestCase(null, null, null, null, null, null, null, null, null, null, null, null, 20, 25)]                          // ActualNumberOfDays
        public void Get_TwoFilterParametersAreSet_ReturnsFilteredArray(string name1, string name2, string title1, string title2,
            string depName1, string depName2, string schDate, string actDate, int? minNum, int? maxNum,
            int? minSchNum, int? maxSchNum, int? minActNum, int? maxActNum) {
            AnnualLeaveFilter filter = new AnnualLeaveFilter {
                Name = new string[] { name1, null, "", name2 },
                PostTitle = new string[] { title1, null, "", title2 },
                DepartmentName = new string[] { depName1, null, "", depName2 },
                ScheduledDate = schDate,
                ActualDate = actDate,
                MinNumberOfDaysOfLeave = minNum,
                MaxNumberOfDaysOfLeave = maxNum,
                MinScheduledNumberOfDays = minSchNum,
                MaxScheduledNumberOfDays = maxSchNum,
                MinActualNumberOfDays = minActNum,
                MaxActualNumberOfDays = maxActNum
            };
            AnnualLeave[] AnnualLeaves = new AnnualLeave[] {
                new AnnualLeave() {
                    Id = 1,
                    Employee = new Employee { LastName = "Иванов", FirstName = "Иван",
                        Post = new Post { Title = "Программист", NumberOfDaysOfLeave = 31,
                            Department = new Department { DepartmentName = "ИТ" } } },
                    ScheduledDate = new DateTime(2018, 11, 20),
                    ActualDate = new DateTime(2018, 11, 27),
                    ScheduledNumberOfDays = 18,
                    ActualNumberOfDays = 23
                },
                new AnnualLeave() {
                    Id = 2,
                    Employee = new Employee { LastName = "Петров", FirstName = "Петр",
                        Post = new Post { Title = "Специалист", NumberOfDaysOfLeave = 33,
                            Department = new Department { DepartmentName = "Отдел кадров" } } },
                    ScheduledDate = new DateTime(2018, 11, 20),
                    ActualDate = new DateTime(2018, 11, 27),
                    ScheduledNumberOfDays = 19,
                    ActualNumberOfDays = 22
                },
                new AnnualLeave() {
                    Id = 3,
                    Employee = new Employee { LastName = "Попов", FirstName = "Иван",
                        Post = new Post { Title = "Секретарь", NumberOfDaysOfLeave = 28,
                            Department = new Department { DepartmentName = "Управление" } } },
                    ScheduledDate = new DateTime(2018, 11, 20),
                    ActualDate = new DateTime(2018, 09, 11),
                    ScheduledNumberOfDays = 14,
                    ActualNumberOfDays = 18
                },
                new AnnualLeave() {
                    Id = 4,
                    Employee = new Employee {  LastName = "Сидоров", FirstName = "Иван",
                        Post = new Post { Title = "Директор", NumberOfDaysOfLeave = 36,
                            Department = new Department { DepartmentName = "Управление" } } },
                    ScheduledDate = new DateTime(2018, 09, 01),
                    ActualDate = new DateTime(2018, 11, 27),
                    ScheduledNumberOfDays = 21,
                    ActualNumberOfDays = 27
                }
            };
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.AnnualLeaves.Get(It.IsAny<Func<AnnualLeave, bool>>()))
                .Returns((Func<AnnualLeave, bool> predicate) => AnnualLeaves.Where(predicate));
            AnnualLeaveService AnnualLeaveService = GetNewService(mock.Object);

            AnnualLeaveDTO[] result = AnnualLeaveService.Get(filter).ToArray();

            Assert.AreEqual(2, result.Length);
            Assert.AreEqual(1, result[0].Id);
            Assert.AreEqual(2, result[1].Id);
        }

        [TestCase("Иванов", "Петров", null, null, null, null, null, null, null, null, null, null, null, null)]              // Name
        [TestCase(null, null, "Программист", "Специалист", null, null, null, null, null, null, null, null, null, null)]     // PostTitle
        [TestCase(null, null, null, null, "ИТ", "Отдел кадров", null, null, null, null, null, null, null, null)]            // DepartmentName
        [TestCase(null, null, null, null, null, null, "20.11.2018", "27.11.2018", null, null, null, null, null, null)]      // ScheduledDate, ActualDate
        [TestCase(null, null, null, null, null, null, null, null, 30, 35, null, null, null, null)]                          // NumberOfDaysOfLeave
        [TestCase(null, null, null, null, null, null, null, null, null, null, 15, 20, null, null)]                          // ScheduledNumberOfDays
        [TestCase(null, null, null, null, null, null, null, null, null, null, null, null, 20, 25)]                          // ActualNumberOfDays
        public void Get_TwoFilterParametersAndIsAntiFilterAreSet_ReturnsFilteredArray(string name1, string name2, string title1, string title2,
            string depName1, string depName2, string schDate, string actDate, int? minNum, int? maxNum,
            int? minSchNum, int? maxSchNum, int? minActNum, int? maxActNum) {
            AnnualLeaveFilter filter = new AnnualLeaveFilter {
                Name = new string[] { name1, null, "", name2 },
                PostTitle = new string[] { title1, null, "", title2 },
                DepartmentName = new string[] { depName1, null, "", depName2 },
                ScheduledDate = schDate,
                ActualDate = actDate,
                MinNumberOfDaysOfLeave = minNum,
                MaxNumberOfDaysOfLeave = maxNum,
                MinScheduledNumberOfDays = minSchNum,
                MaxScheduledNumberOfDays = maxSchNum,
                MinActualNumberOfDays = minActNum,
                MaxActualNumberOfDays = maxActNum,
                IsAntiFilter = true
            };
            AnnualLeave[] AnnualLeaves = new AnnualLeave[] {
                new AnnualLeave() {
                    Id = 1,
                    Employee = new Employee { LastName = "Иванов", FirstName = "Иван",
                        Post = new Post { Title = "Программист", NumberOfDaysOfLeave = 31,
                            Department = new Department { DepartmentName = "ИТ" } } },
                    ScheduledDate = new DateTime(2018, 11, 20),
                    ActualDate = new DateTime(2018, 11, 27),
                    ScheduledNumberOfDays = 18,
                    ActualNumberOfDays = 23
                },
                new AnnualLeave() {
                    Id = 2,
                    Employee = new Employee { LastName = "Петров", FirstName = "Петр",
                        Post = new Post { Title = "Специалист", NumberOfDaysOfLeave = 33,
                            Department = new Department { DepartmentName = "Отдел кадров" } } },
                    ScheduledDate = new DateTime(2018, 11, 20),
                    ActualDate = new DateTime(2018, 11, 27),
                    ScheduledNumberOfDays = 19,
                    ActualNumberOfDays = 22
                },
                new AnnualLeave() {
                    Id = 3,
                    Employee = new Employee { LastName = "Попов", FirstName = "Иван",
                        Post = new Post { Title = "Секретарь", NumberOfDaysOfLeave = 28,
                            Department = new Department { DepartmentName = "Управление" } } },
                    ScheduledDate = new DateTime(2018, 11, 20),
                    ActualDate = new DateTime(2018, 09, 11),
                    ScheduledNumberOfDays = 14,
                    ActualNumberOfDays = 18
                },
                new AnnualLeave() {
                    Id = 4,
                    Employee = new Employee {  LastName = "Сидоров", FirstName = "Иван",
                        Post = new Post { Title = "Директор", NumberOfDaysOfLeave = 36,
                            Department = new Department { DepartmentName = "Управление" } } },
                    ScheduledDate = new DateTime(2018, 09, 01),
                    ActualDate = new DateTime(2018, 11, 27),
                    ScheduledNumberOfDays = 21,
                    ActualNumberOfDays = 27
                }
            };
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.AnnualLeaves.Get(It.IsAny<Func<AnnualLeave, bool>>()))
                .Returns((Func<AnnualLeave, bool> predicate) => AnnualLeaves.Where(predicate));
            AnnualLeaveService AnnualLeaveService = GetNewService(mock.Object);

            AnnualLeaveDTO[] result = AnnualLeaveService.Get(filter).ToArray();

            Assert.AreEqual(2, result.Length);
            Assert.AreEqual(3, result[0].Id);
            Assert.AreEqual(4, result[1].Id);
        }

        [TestCase(null)]
        [TestCase("Name")]
        [TestCase("PostTitle")]
        [TestCase("DepartmentName")]
        [TestCase("ScheduledDate")]
        [TestCase("ActualDate")]
        [TestCase("NumberOfDaysOfLeave")]
        [TestCase("ScheduledNumberOfDays")]
        [TestCase("ActualNumberOfDays")]
        public void Get_AscSortIsSet_ReturnsSortedArray(string field) {
            AnnualLeaveFilter filter = new AnnualLeaveFilter {
                SortField = field,
                SortOrder = "Asc"
            };
            AnnualLeave[] AnnualLeaves = new AnnualLeave[] {
                new AnnualLeave() {
                    Id = 1,
                    Employee = new Employee { LastName = "Сидоров", FirstName = "Юрий",
                        Post = new Post { Title = "Специалист", NumberOfDaysOfLeave = 33,
                            Department = new Department { DepartmentName = "Управление" } } },
                    ScheduledDate = new DateTime(2018, 12, 20),
                    ActualDate = new DateTime(2018, 12, 27),
                    ScheduledNumberOfDays = 24,
                    ActualNumberOfDays = 23
                },
                new AnnualLeave() {
                    Id = 2,
                    Employee = new Employee { LastName = "Попов", FirstName = "Петр",
                        Post = new Post { Title = "Секретарь", NumberOfDaysOfLeave = 31,
                            Department = new Department { DepartmentName = "Производство" } } },
                    ScheduledDate = new DateTime(2018, 11, 20),
                    ActualDate = new DateTime(2018, 11, 4),
                    ScheduledNumberOfDays = 19,
                    ActualNumberOfDays = 22
                },
                new AnnualLeave() {
                    Id = 3,
                    Employee = new Employee { LastName = "Петров", FirstName = "Максим",
                        Post = new Post { Title = "Программист", NumberOfDaysOfLeave = 28,
                            Department = new Department { DepartmentName = "Отдел кадров" } } },
                    ScheduledDate = new DateTime(2018, 09, 20),
                    ActualDate = new DateTime(2018, 09, 11),
                    ScheduledNumberOfDays = 14,
                    ActualNumberOfDays = 18
                },
                new AnnualLeave() {
                    Id = 4,
                    Employee = new Employee {  LastName = "Иванов", FirstName = "Иван",
                        Post = new Post { Title = "Директор", NumberOfDaysOfLeave = 25,
                            Department = new Department { DepartmentName = "ИТ" } } },
                    ScheduledDate = new DateTime(2018, 06, 01),
                    ActualDate = new DateTime(2018, 06, 27),
                    ScheduledNumberOfDays = 11,
                    ActualNumberOfDays = 17
                }
            };
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.AnnualLeaves.Get(It.IsAny<Func<AnnualLeave, bool>>()))
                .Returns((Func<AnnualLeave, bool> predicate) => AnnualLeaves.Where(predicate));
            AnnualLeaveService AnnualLeaveService = GetNewService(mock.Object);

            AnnualLeaveDTO[] result = AnnualLeaveService.Get(filter).ToArray();

            Assert.AreEqual(4, result.Length);
            Assert.AreEqual(4, result[0].Id);
            Assert.AreEqual(3, result[1].Id);
            Assert.AreEqual(2, result[2].Id);
            Assert.AreEqual(1, result[3].Id);
        }

        [TestCase("Name")]
        [TestCase("PostTitle")]
        [TestCase("DepartmentName")]
        [TestCase("ScheduledDate")]
        [TestCase("ActualDate")]
        [TestCase("NumberOfDaysOfLeave")]
        [TestCase("ScheduledNumberOfDays")]
        [TestCase("ActualNumberOfDays")]
        public void Get_DescSortIsSet_ReturnsSortedArray(string field) {
            AnnualLeaveFilter filter = new AnnualLeaveFilter {
                SortField = field,
                SortOrder = "Desc"
            };
            AnnualLeave[] AnnualLeaves = new AnnualLeave[] {
                new AnnualLeave() {
                    Id = 1,
                    Employee = new Employee {  LastName = "Иванов", FirstName = "Иван",
                        Post = new Post { Title = "Директор", NumberOfDaysOfLeave = 25,
                            Department = new Department { DepartmentName = "ИТ" } } },
                    ScheduledDate = new DateTime(2018, 06, 01),
                    ActualDate = new DateTime(2018, 06, 27),
                    ScheduledNumberOfDays = 11,
                    ActualNumberOfDays = 17
                },
                new AnnualLeave() {
                    Id = 2,
                    Employee = new Employee { LastName = "Петров", FirstName = "Максим",
                        Post = new Post { Title = "Программист", NumberOfDaysOfLeave = 28,
                            Department = new Department { DepartmentName = "Отдел кадров" } } },
                    ScheduledDate = new DateTime(2018, 09, 20),
                    ActualDate = new DateTime(2018, 09, 11),
                    ScheduledNumberOfDays = 14,
                    ActualNumberOfDays = 18
                },
                new AnnualLeave() {
                    Id = 3,
                    Employee = new Employee { LastName = "Попов", FirstName = "Петр",
                        Post = new Post { Title = "Секретарь", NumberOfDaysOfLeave = 31,
                            Department = new Department { DepartmentName = "Производство" } } },
                    ScheduledDate = new DateTime(2018, 11, 20),
                    ActualDate = new DateTime(2018, 11, 4),
                    ScheduledNumberOfDays = 19,
                    ActualNumberOfDays = 22
                },
                new AnnualLeave() {
                    Id = 4,
                    Employee = new Employee { LastName = "Сидоров", FirstName = "Юрий",
                        Post = new Post { Title = "Специалист", NumberOfDaysOfLeave = 33,
                            Department = new Department { DepartmentName = "Управление" } } },
                    ScheduledDate = new DateTime(2018, 12, 20),
                    ActualDate = new DateTime(2018, 12, 27),
                    ScheduledNumberOfDays = 24,
                    ActualNumberOfDays = 23
                }
            };
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.AnnualLeaves.Get(It.IsAny<Func<AnnualLeave, bool>>()))
                .Returns((Func<AnnualLeave, bool> predicate) => AnnualLeaves.Where(predicate));
            AnnualLeaveService AnnualLeaveService = GetNewService(mock.Object);

            AnnualLeaveDTO[] result = AnnualLeaveService.Get(filter).ToArray();

            Assert.AreEqual(4, result.Length);
            Assert.AreEqual(4, result[0].Id);
            Assert.AreEqual(3, result[1].Id);
            Assert.AreEqual(2, result[2].Id);
            Assert.AreEqual(1, result[3].Id);
        }

        /// <summary>
        /// // GetAllAsync method
        /// </summary>
        [Test]
        public override async Task GetAllAsync_GetAsyncMethodReturnsArray_ReturnsSameArray() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.AnnualLeaves.GetAllAsync()).ReturnsAsync(() => new AnnualLeave[] {
                new AnnualLeave() { Id = 1 },
                new AnnualLeave() { Id = 2 },
                new AnnualLeave() { Id = 3 }
            });
            AnnualLeaveService als = GetNewService(mock.Object);

            AnnualLeaveDTO[] result = (await als.GetAllAsync()).ToArray();

            Assert.AreEqual(1, result[0].Id);
            Assert.AreEqual(2, result[1].Id);
            Assert.AreEqual(3, result[2].Id);
        }

        /// <summary>
        /// // ExportJsonAsync method
        /// </summary>
        [Test]
        public override async Task ExportJsonAsync_CreatesJsonFile() {
            string fullPath = "./DiplomMSSQLApp.WEB/Results/AnnualLeaves.json";
            File.Delete(fullPath);
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.AnnualLeaves.GetAllAsync()).ReturnsAsync(new AnnualLeave[] { });
            AnnualLeaveService AnnualLeaveService = GetNewService(mock.Object);

            await AnnualLeaveService.ExportJsonAsync(fullPath);

            Assert.IsTrue(File.Exists(fullPath));
        }
    }
}
