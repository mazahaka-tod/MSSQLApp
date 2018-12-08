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
using System.Threading.Tasks;

namespace DiplomMSSQLApp.BLL.UnitTests {
    [TestFixture]
    public class BusinessTripServiceTests : BaseServiceTests<BusinessTripService> {
        protected override BusinessTripService GetNewService() {
            return new BusinessTripService();
        }

        protected override BusinessTripService GetNewService(IUnitOfWork uow) {
            return new BusinessTripService(uow);
        }

        /// <summary>
        /// // GetPage method
        /// </summary>
        [Test]
        public override void GetPage_CallsWithGoodParams_FillsPageInfoProperty() {
            BusinessTripService bts = GetNewService();
            bts.NumberOfObjectsPerPage = 2;
            BusinessTripDTO[] col = new BusinessTripDTO[] {
                new BusinessTripDTO() { Id = 1 },
                new BusinessTripDTO() { Id = 2 },
                new BusinessTripDTO() { Id = 3 }
            };

            bts.GetPage(col, 1);

            Assert.AreEqual(1, bts.PageInfo.PageNumber);
            Assert.AreEqual(2, bts.PageInfo.PageSize);
            Assert.AreEqual(col.Length, bts.PageInfo.TotalItems);
        }

        [Test]
        public override void GetPage_RequestedPageLessThan1_ReturnsFirstPage() {
            BusinessTripService bts = GetNewService();

            bts.GetPage(new BusinessTripDTO[0], -5);

            Assert.AreEqual(1, bts.PageInfo.PageNumber);
        }

        [Test]
        public override void GetPage_RequestedPageMoreThanTotalPages_ReturnsLastPage() {
            BusinessTripService bts = GetNewService();
            bts.NumberOfObjectsPerPage = 3;
            BusinessTripDTO[] col = new BusinessTripDTO[] {
                new BusinessTripDTO() { Id = 1 },
                new BusinessTripDTO() { Id = 2 },
                new BusinessTripDTO() { Id = 3 }
            };

            bts.GetPage(col, 2);
            int totalPages = bts.PageInfo.TotalPages;   // 1

            Assert.AreEqual(totalPages, bts.PageInfo.PageNumber);
        }

        [Test]
        public override void GetPage_CallsExistingPage_ReturnsSpecifiedPage() {
            BusinessTripService bts = GetNewService();
            bts.NumberOfObjectsPerPage = 3;
            BusinessTripDTO[] col = new BusinessTripDTO[] {
                new BusinessTripDTO() { Id = 1, Name = "01.09.2018_021" },
                new BusinessTripDTO() { Id = 2, Name = "02.09.2018_022" },
                new BusinessTripDTO() { Id = 3, Name = "03.09.2018_023" },
                new BusinessTripDTO() { Id = 4, Name = "04.09.2018_024" },
                new BusinessTripDTO() { Id = 5, Name = "05.09.2018_025" }
            };

            BusinessTripDTO[] result = bts.GetPage(col, 2).ToArray();

            Assert.AreEqual(2, result.Length);
            Assert.AreEqual(4, result[0].Id);
            Assert.AreEqual(5, result[1].Id);
            Assert.AreEqual("04.09.2018_024", result[0].Name);
            Assert.AreEqual("05.09.2018_025", result[1].Name);
        }

        /// <summary>
        /// // CreateAsync method
        /// </summary>
        [Test]
        public void CreateAsync_NamePropertyIsNull_Throws() {
            BusinessTripService bts = GetNewService();
            BusinessTripDTO item = new BusinessTripDTO {
                Name = null,
                DateStart = new DateTime(2018, 8, 10),
                DateEnd = new DateTime(2018, 8, 20),
                Destination = "Moscow",
                Purpose = "Seminar"
            };

            Exception ex = Assert.CatchAsync(async () => await bts.CreateAsync(item));

            StringAssert.Contains("Требуется ввести код командировки", ex.Message);
        }

        [Test]
        public void CreateAsync_DateStartPropertyMoreThanDateEndProperty_Throws() {
            BusinessTripService bts = GetNewService();
            BusinessTripDTO item = new BusinessTripDTO {
                Name = "01.09.2018_021",
                DateStart = new DateTime(2018, 8, 20),
                DateEnd = new DateTime(2018, 8, 10),
                Destination = "Moscow",
                Purpose = "Seminar"
            };

            Exception ex = Assert.CatchAsync(async () => await bts.CreateAsync(item));

            StringAssert.Contains("Дата окончания командировки не должна быть до даты начала", ex.Message);
        }

        [Test]
        public void CreateAsync_DestinationPropertyIsNull_Throws() {
            BusinessTripService bts = GetNewService();
            BusinessTripDTO item = new BusinessTripDTO {
                Name = "01.09.2018_021",
                DateStart = new DateTime(2018, 8, 10),
                DateEnd = new DateTime(2018, 8, 20),
                Destination = null,
                Purpose = "Seminar"
            };

            Exception ex = Assert.CatchAsync(async () => await bts.CreateAsync(item));

            StringAssert.Contains("Требуется ввести место назначения", ex.Message);
        }

        [Test]
        public void CreateAsync_PurposePropertyIsNull_Throws() {
            BusinessTripService bts = GetNewService();
            BusinessTripDTO item = new BusinessTripDTO {
                Name = "01.09.2018_021",
                DateStart = new DateTime(2018, 8, 10),
                DateEnd = new DateTime(2018, 8, 20),
                Destination = "Moscow",
                Purpose = null
            };

            Exception ex = Assert.CatchAsync(async () => await bts.CreateAsync(item));

            StringAssert.Contains("Требуется ввести цель командировки", ex.Message);
        }

        [Test]
        public async Task CreateAsync_IdsParameterContainsThreeDifferentValues_CallsFindByIdAsyncMethodThreeTimes() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.BusinessTrips.Create(It.IsAny<BusinessTrip>()));
            mock.Setup(m => m.Employees.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(It.IsAny<Employee>());
            BusinessTripService bts = GetNewService(mock.Object);
            BusinessTripDTO item = new BusinessTripDTO {
                Name = "01.09.2018_021",
                DateStart = new DateTime(2018, 8, 10),
                DateEnd = new DateTime(2018, 8, 20),
                Destination = "Moscow",
                Purpose = "Seminar",
                Employees = new EmployeeDTO[] {
                    new EmployeeDTO { Id = 1 },
                    new EmployeeDTO { Id = 2 },
                    new EmployeeDTO { Id = 2 },
                    new EmployeeDTO { Id = 3 }
                }
            };

            await bts.CreateAsync(item);

            mock.Verify(m => m.Employees.FindByIdAsync(It.IsAny<int>()), Times.Exactly(3));
        }

        [Test]
        public async Task CreateAsync_CallsWithGoodParams_CallsCreateMethodOnсe() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.BusinessTrips.Create(It.IsAny<BusinessTrip>()));
            BusinessTripService bts = GetNewService(mock.Object);
            BusinessTripDTO item = new BusinessTripDTO {
                Name = "01.09.2018_021",
                DateStart = new DateTime(2018, 8, 10),
                DateEnd = new DateTime(2018, 8, 20),
                Destination = "Moscow",
                Purpose = "Seminar"
            };

            await bts.CreateAsync(item);

            mock.Verify(m => m.BusinessTrips.Create(It.IsAny<BusinessTrip>()), Times.Once());
        }

        [Test]
        public async Task CreateAsync_CallsWithGoodParams_CallsSaveAsyncMethodOnсe() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.BusinessTrips.Create(It.IsAny<BusinessTrip>()));
            BusinessTripService bts = GetNewService(mock.Object);
            BusinessTripDTO item = new BusinessTripDTO {
                Name = "01.09.2018_021",
                DateStart = new DateTime(2018, 8, 10),
                DateEnd = new DateTime(2018, 8, 20),
                Destination = "Moscow",
                Purpose = "Seminar"
            };

            await bts.CreateAsync(item);

            mock.Verify((m => m.SaveAsync()), Times.Once());
        }

        /// <summary>
        /// // DeleteAsync method
        /// </summary>
        [Test]
        public override async Task DeleteAsync_FindByIdAsyncMethodReturnsNull_RemoveMethodIsNeverCalled() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.BusinessTrips.FindByIdAsync(It.IsAny<int>())).Returns(Task.FromResult<BusinessTrip>(null));
            BusinessTripService bts = GetNewService(mock.Object);

            await bts.DeleteAsync(It.IsAny<int>());

            mock.Verify(m => m.BusinessTrips.Remove(It.IsAny<BusinessTrip>()), Times.Never);
        }

        [Test]
        public override async Task DeleteAsync_FindByIdAsyncMethodReturnsNull_SaveAsyncMethodIsNeverCalled() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.BusinessTrips.FindByIdAsync(It.IsAny<int>())).Returns(Task.FromResult<BusinessTrip>(null));
            BusinessTripService bts = GetNewService(mock.Object);

            await bts.DeleteAsync(It.IsAny<int>());

            mock.Verify(m => m.SaveAsync(), Times.Never);
        }

        [Test]
        public override async Task DeleteAsync_FindByIdAsyncMethodReturnsObject_RemoveMethodIsCalledOnce() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.BusinessTrips.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(new BusinessTrip());
            BusinessTripService bts = GetNewService(mock.Object);

            await bts.DeleteAsync(It.IsAny<int>());

            mock.Verify(m => m.BusinessTrips.Remove(It.IsAny<BusinessTrip>()), Times.Once);
        }

        [Test]
        public override async Task DeleteAsync_FindByIdAsyncMethodReturnsObject_SaveAsyncMethodIsCalledOnce() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.BusinessTrips.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(new BusinessTrip());
            BusinessTripService bts = GetNewService(mock.Object);

            await bts.DeleteAsync(It.IsAny<int>());

            mock.Verify(m => m.SaveAsync(), Times.Once);
        }

        /// <summary>
        /// // DeleteAllAsync method
        /// </summary>
        [Test]
        public async Task DeleteAllAsync_Calls_RemoveAllAsyncMethodIsCalledOnce() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.BusinessTrips.RemoveAllAsync()).Returns(Task.CompletedTask);
            BusinessTripService bts = GetNewService(mock.Object);

            await bts.DeleteAllAsync();

            mock.Verify(m => m.BusinessTrips.RemoveAllAsync(), Times.Once);
        }

        [Test]
        public override async Task DeleteAllAsync_Calls_SaveAsyncMethodIsCalledOnce() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.BusinessTrips.RemoveAllAsync()).Returns(Task.CompletedTask);
            BusinessTripService bts = GetNewService(mock.Object);

            await bts.DeleteAllAsync();

            mock.Verify(m => m.SaveAsync(), Times.Once);
        }

        /// <summary>
        /// // EditAsync method
        /// </summary>
        [Test]
        public void EditAsync_NamePropertyIsNull_Throws() {
            BusinessTripService bts = GetNewService();
            BusinessTripDTO item = new BusinessTripDTO {
                Name = null,
                DateStart = new DateTime(2018, 8, 10),
                DateEnd = new DateTime(2018, 8, 20),
                Destination = "Moscow",
                Purpose = "Seminar"
            };

            Exception ex = Assert.CatchAsync(async () => await bts.EditAsync(item));

            StringAssert.Contains("Требуется ввести код командировки", ex.Message);
        }

        [Test]
        public void EditAsync_DateStartPropertyMoreThanDateEndProperty_Throws() {
            BusinessTripService bts = GetNewService();
            BusinessTripDTO item = new BusinessTripDTO {
                Name = "01.09.2018_021",
                DateStart = new DateTime(2018, 8, 20),
                DateEnd = new DateTime(2018, 8, 10),
                Destination = "Moscow",
                Purpose = "Seminar"
            };

            Exception ex = Assert.CatchAsync(async () => await bts.EditAsync(item));

            StringAssert.Contains("Дата окончания командировки не должна быть до даты начала", ex.Message);
        }

        [Test]
        public void EditAsync_DestinationPropertyIsNull_Throws() {
            BusinessTripService bts = GetNewService();
            BusinessTripDTO item = new BusinessTripDTO {
                Name = "01.09.2018_021",
                DateStart = new DateTime(2018, 8, 10),
                DateEnd = new DateTime(2018, 8, 20),
                Destination = null,
                Purpose = "Seminar"
            };

            Exception ex = Assert.CatchAsync(async () => await bts.EditAsync(item));

            StringAssert.Contains("Требуется ввести место назначения", ex.Message);
        }

        [Test]
        public void EditAsync_PurposePropertyIsNull_Throws() {
            BusinessTripService bts = GetNewService();
            BusinessTripDTO item = new BusinessTripDTO {
                Name = "01.09.2018_021",
                DateStart = new DateTime(2018, 8, 10),
                DateEnd = new DateTime(2018, 8, 20),
                Destination = "Moscow",
                Purpose = null
            };

            Exception ex = Assert.CatchAsync(async () => await bts.EditAsync(item));

            StringAssert.Contains("Требуется ввести цель командировки", ex.Message);
        }

        [Test]
        public async Task EditAsync_IdsParameterContainsThreeDifferentValue_CallsFindByIdAsyncMethodThreeTimes() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.BusinessTrips.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(new BusinessTrip());
            mock.Setup(m => m.Employees.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(new Employee());
            BusinessTripService bts = GetNewService(mock.Object);
            BusinessTripDTO item = new BusinessTripDTO {
                Name = "01.09.2018_021",
                DateStart = new DateTime(2018, 8, 10),
                DateEnd = new DateTime(2018, 8, 20),
                Destination = "Moscow",
                Purpose = "Seminar",
                Employees = new EmployeeDTO[] {
                    new EmployeeDTO { Id = 1 },
                    new EmployeeDTO { Id = 2 },
                    new EmployeeDTO { Id = 2 },
                    new EmployeeDTO { Id = 3 }
                }
            };

            await bts.EditAsync(item);

            mock.Verify(m => m.Employees.FindByIdAsync(It.IsAny<int>()), Times.Exactly(3));
        }

        [Test]
        public override async Task EditAsync_CallsWithGoodParams_CallsUpdateMethodOnсe() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.BusinessTrips.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(new BusinessTrip());
            BusinessTripService bts = GetNewService(mock.Object);
            BusinessTripDTO item = new BusinessTripDTO {
                Name = "01.09.2018_021",
                DateStart = new DateTime(2018, 8, 10),
                DateEnd = new DateTime(2018, 8, 20),
                Destination = "Moscow",
                Purpose = "Seminar"
            };

            await bts.EditAsync(item);

            mock.Verify(m => m.BusinessTrips.Update(It.IsAny<BusinessTrip>()), Times.Once);
        }

        [Test]
        public override async Task EditAsync_CallsWithGoodParams_CallsSaveAsyncMethodOnсe() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.BusinessTrips.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(new BusinessTrip());
            BusinessTripService bts = GetNewService(mock.Object);
            BusinessTripDTO item = new BusinessTripDTO {
                Name = "01.09.2018_021",
                DateStart = new DateTime(2018, 8, 10),
                DateEnd = new DateTime(2018, 8, 20),
                Destination = "Moscow",
                Purpose = "Seminar"
            };

            await bts.EditAsync(item);

            mock.Verify(m => m.SaveAsync(), Times.Once);
        }

        /// <summary>
        /// // FindByIdAsync method
        /// </summary>
        [Test]
        public override void FindByIdAsync_IdParameterIsNull_Throws() {
            BusinessTripService bts = GetNewService();

            Exception ex = Assert.CatchAsync(async () => await bts.FindByIdAsync(null));

            StringAssert.Contains("Не установлено id командировки", ex.Message);
        }

        [Test]
        public void FindByIdAsync_BusinessTripNotFound_Throws() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.BusinessTrips.FindByIdAsync(It.IsAny<int>())).Returns(Task.FromResult<BusinessTrip>(null));
            BusinessTripService bts = GetNewService(mock.Object);

            Exception ex = Assert.CatchAsync(async () => await bts.FindByIdAsync(It.IsAny<int>()));

            StringAssert.Contains("Командировка не найдена", ex.Message);
        }

        [Test]
        public override async Task FindByIdAsync_IdEqualTo2_ReturnsObjectWithIdEqualTo2() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.BusinessTrips.FindByIdAsync(It.IsAny<int>())).ReturnsAsync((int item_id) => new BusinessTrip() { Id = item_id });
            BusinessTripService bts = GetNewService(mock.Object);

            BusinessTripDTO result = await bts.FindByIdAsync(2);

            Assert.AreEqual(2, result.Id);
        }

        /// <summary>
        /// // Get method
        /// </summary>
        [TestCase("bt_3", null, null, null)]            // Code
        [TestCase(null, "2018-09-01", null, null)]      // DateStart
        [TestCase(null, null, "2018-09-05", null)]      // DateEnd
        [TestCase(null, null, null, "Moscow")]          // Destination
        public void Get_OneFilterParameterIsSet_ReturnsFilteredArray(string code, string dateStart, string dateEnd, string destination) {
            BusinessTripFilter filter = new BusinessTripFilter {
                Code = new string[] { code, null, "" },
                DateStart = dateStart,
                DateEnd = dateEnd,
                Destination = new string[] { destination, null, "" }
            };
            BusinessTrip[] businessTrips = new BusinessTrip[] {
                new BusinessTrip() {
                    Id = 1,
                    Name = "bt_33",
                    DateStart = new DateTime(2018, 09, 01),
                    DateEnd = new DateTime(2018, 09, 05),
                    Destination = "Moscow"
                },
                new BusinessTrip() {
                    Id = 2,
                    Name = "bt_34",
                    DateStart = new DateTime(2018, 09, 01),
                    DateEnd = new DateTime(2018, 09, 05),
                    Destination = "Moscow"
                },
                new BusinessTrip() {
                    Id = 3,
                    Name = "bt_45",
                    DateStart = new DateTime(2018, 09, 02),
                    DateEnd = new DateTime(2018, 09, 07),
                    Destination = "Spb"
                },
                new BusinessTrip() {
                    Id = 4,
                    Name = "bt_47",
                    DateStart = new DateTime(2018, 09, 03),
                    DateEnd = new DateTime(2018, 09, 08),
                    Destination = "Spb"
                }
            };
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.BusinessTrips.Get(It.IsAny<Func<BusinessTrip, bool>>()))
                .Returns((Func<BusinessTrip, bool> predicate) => businessTrips.Where(predicate));
            BusinessTripService businessTripService = GetNewService(mock.Object);

            BusinessTripDTO[] result = businessTripService.Get(filter).ToArray();

            Assert.AreEqual(2, result.Length);
            Assert.AreEqual(1, result[0].Id);
            Assert.AreEqual(2, result[1].Id);
            Assert.AreEqual("bt_33", result[0].Name);
            Assert.AreEqual("bt_34", result[1].Name);
        }

        [TestCase("bt_3", null, null, null)]            // Code
        [TestCase(null, "2018-09-01", null, null)]      // DateStart
        [TestCase(null, null, "2018-09-05", null)]      // DateEnd
        [TestCase(null, null, null, "Moscow")]          // Destination
        public void Get_OneFilterParameterAndIsAntiFilterIsSet_ReturnsFilteredArray(string code, string dateStart, string dateEnd, string destination) {
            BusinessTripFilter filter = new BusinessTripFilter {
                Code = new string[] { code, null, "" },
                DateStart = dateStart,
                DateEnd = dateEnd,
                Destination = new string[] { destination, null, "" },
                IsAntiFilter = true
            };
            BusinessTrip[] businessTrips = new BusinessTrip[] {
                new BusinessTrip() {
                    Id = 1,
                    Name = "bt_33",
                    DateStart = new DateTime(2018, 09, 01),
                    DateEnd = new DateTime(2018, 09, 05),
                    Destination = "Moscow"
                },
                new BusinessTrip() {
                    Id = 2,
                    Name = "bt_34",
                    DateStart = new DateTime(2018, 09, 01),
                    DateEnd = new DateTime(2018, 09, 05),
                    Destination = "Moscow"
                },
                new BusinessTrip() {
                    Id = 3,
                    Name = "bt_45",
                    DateStart = new DateTime(2018, 09, 02),
                    DateEnd = new DateTime(2018, 09, 07),
                    Destination = "Spb"
                },
                new BusinessTrip() {
                    Id = 4,
                    Name = "bt_47",
                    DateStart = new DateTime(2018, 09, 03),
                    DateEnd = new DateTime(2018, 09, 08),
                    Destination = "Spb"
                }
            };
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.BusinessTrips.Get(It.IsAny<Func<BusinessTrip, bool>>()))
                .Returns((Func<BusinessTrip, bool> predicate) => businessTrips.Where(predicate));
            BusinessTripService businessTripService = GetNewService(mock.Object);

            BusinessTripDTO[] result = businessTripService.Get(filter).ToArray();

            Assert.AreEqual(2, result.Length);
            Assert.AreEqual(3, result[0].Id);
            Assert.AreEqual(4, result[1].Id);
            Assert.AreEqual("bt_45", result[0].Name);
            Assert.AreEqual("bt_47", result[1].Name);
        }

        [TestCase("bt_3", "bt_4", null, null, null, null)]                  // Code1, Code2
        [TestCase("bt_4", null, "2018-09-01", null, null, null)]            // Code, DateStart
        [TestCase(null, null, "2018-09-01", "2018-09-05", null, null)]      // DateStart, DateEnd
        [TestCase(null, null, null, "2018-09-05", "Moscow", null)]          // DateEnd, Destination
        [TestCase(null, null, null, null, "Moscow", "Spb")]                 // Destination1, Destination2
        public void Get_TwoFilterParametersAreSet_ReturnsFilteredArray(string code1, string code2,
                string dateStart, string dateEnd, string destination1, string destination2) {
            BusinessTripFilter filter = new BusinessTripFilter {
                Code = new string[] { code1, code2, null, "" },
                DateStart = dateStart,
                DateEnd = dateEnd,
                Destination = new string[] { destination1, destination2, null, "" }
            };
            BusinessTrip[] businessTrips = new BusinessTrip[] {
                new BusinessTrip() {
                    Id = 1,
                    Name = "bt_53",
                    DateStart = new DateTime(2018, 09, 02),
                    DateEnd = new DateTime(2018, 09, 05),
                    Destination = "Rostov"
                },
                new BusinessTrip() {
                    Id = 2,
                    Name = "bt_54",
                    DateStart = new DateTime(2018, 09, 02),
                    DateEnd = new DateTime(2018, 09, 05),
                    Destination = "Kazan"
                },
                new BusinessTrip() {
                    Id = 3,
                    Name = "bt_55",
                    DateStart = new DateTime(2018, 09, 01),
                    DateEnd = new DateTime(2018, 09, 07),
                    Destination = "Sochi"
                },
                new BusinessTrip() {
                    Id = 4,
                    Name = "bt_47",
                    DateStart = new DateTime(2018, 09, 01),
                    DateEnd = new DateTime(2018, 09, 05),
                    Destination = "Moscow"
                }
            };
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.BusinessTrips.Get(It.IsAny<Func<BusinessTrip, bool>>()))
                .Returns((Func<BusinessTrip, bool> predicate) => businessTrips.Where(predicate));
            BusinessTripService businessTripService = GetNewService(mock.Object);

            BusinessTripDTO[] result = businessTripService.Get(filter).ToArray();

            Assert.AreEqual(1, result.Length);
            Assert.AreEqual(4, result[0].Id);
            Assert.AreEqual("bt_47", result[0].Name);
        }

        [TestCase("bt_3", "bt_4", null, null, null, null)]                  // Code1, Code2
        [TestCase("bt_4", null, "2018-09-01", null, null, null)]            // Code, DateStart
        [TestCase(null, null, "2018-09-01", "2018-09-05", null, null)]      // DateStart, DateEnd
        [TestCase(null, null, null, "2018-09-05", "Moscow", null)]          // DateEnd, Destination
        [TestCase(null, null, null, null, "Moscow", "Spb")]                 // Destination1, Destination2
        public void Get_TwoFilterParametersAndIsAntiFilterAreSet_ReturnsFilteredArray(string code1, string code2,
                string dateStart, string dateEnd, string destination1, string destination2) {
            BusinessTripFilter filter = new BusinessTripFilter {
                Code = new string[] { code1, code2, null, "" },
                DateStart = dateStart,
                DateEnd = dateEnd,
                Destination = new string[] { destination1, destination2, null, "" },
                IsAntiFilter = true
            };
            BusinessTrip[] businessTrips = new BusinessTrip[] {
                new BusinessTrip() {
                    Id = 1,
                    Name = "bt_53",
                    DateStart = new DateTime(2018, 09, 02),
                    DateEnd = new DateTime(2018, 09, 05),
                    Destination = "Rostov"
                },
                new BusinessTrip() {
                    Id = 2,
                    Name = "bt_54",
                    DateStart = new DateTime(2018, 09, 02),
                    DateEnd = new DateTime(2018, 09, 05),
                    Destination = "Kazan"
                },
                new BusinessTrip() {
                    Id = 3,
                    Name = "bt_55",
                    DateStart = new DateTime(2018, 09, 01),
                    DateEnd = new DateTime(2018, 09, 07),
                    Destination = "Sochi"
                },
                new BusinessTrip() {
                    Id = 4,
                    Name = "bt_47",
                    DateStart = new DateTime(2018, 09, 01),
                    DateEnd = new DateTime(2018, 09, 05),
                    Destination = "Moscow"
                }
            };
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.BusinessTrips.Get(It.IsAny<Func<BusinessTrip, bool>>()))
                .Returns((Func<BusinessTrip, bool> predicate) => businessTrips.Where(predicate));
            BusinessTripService businessTripService = GetNewService(mock.Object);

            BusinessTripDTO[] result = businessTripService.Get(filter).ToArray();

            Assert.AreEqual(3, result.Length);
            Assert.AreEqual(1, result[0].Id);
            Assert.AreEqual(2, result[1].Id);
            Assert.AreEqual(3, result[2].Id);
            Assert.AreEqual("bt_53", result[0].Name);
            Assert.AreEqual("bt_54", result[1].Name);
            Assert.AreEqual("bt_55", result[2].Name);
        }

        [TestCase(null)]
        [TestCase("Code")]
        [TestCase("DateStart")]
        [TestCase("DateEnd")]
        [TestCase("Destination")]
        public void Get_AscSortIsSet_ReturnsSortedArray(string field) {
            BusinessTripFilter filter = new BusinessTripFilter {
                SortField = field,
                SortOrder = "Asc"
            };
            BusinessTrip[] businessTrips = new BusinessTrip[] {
                new BusinessTrip() {
                    Id = 1,
                    Name = "bt_53",
                    DateStart = new DateTime(2018, 09, 12),
                    DateEnd = new DateTime(2018, 09, 25),
                    Destination = "Sochi"
                },
                new BusinessTrip() {
                    Id = 2,
                    Name = "bt_50",
                    DateStart = new DateTime(2018, 09, 08),
                    DateEnd = new DateTime(2018, 09, 15),
                    Destination = "Rostov"
                },
                new BusinessTrip() {
                    Id = 3,
                    Name = "bt_48",
                    DateStart = new DateTime(2018, 09, 03),
                    DateEnd = new DateTime(2018, 09, 07),
                    Destination = "Moscow"
                },
                new BusinessTrip() {
                    Id = 4,
                    Name = "bt_47",
                    DateStart = new DateTime(2018, 09, 01),
                    DateEnd = new DateTime(2018, 09, 05),
                    Destination = "Kazan"
                }
            };
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.BusinessTrips.Get(It.IsAny<Func<BusinessTrip, bool>>()))
                .Returns((Func<BusinessTrip, bool> predicate) => businessTrips.Where(predicate));
            BusinessTripService businessTripService = GetNewService(mock.Object);

            BusinessTripDTO[] result = businessTripService.Get(filter).ToArray();

            Assert.AreEqual(4, result.Length);
            Assert.AreEqual(4, result[0].Id);
            Assert.AreEqual(3, result[1].Id);
            Assert.AreEqual(2, result[2].Id);
            Assert.AreEqual(1, result[3].Id);
            Assert.AreEqual("bt_47", result[0].Name);
            Assert.AreEqual("bt_48", result[1].Name);
            Assert.AreEqual("bt_50", result[2].Name);
            Assert.AreEqual("bt_53", result[3].Name);
        }

        [TestCase("Code")]
        [TestCase("DateStart")]
        [TestCase("DateEnd")]
        [TestCase("Destination")]
        public void Get_DescSortIsSet_ReturnsSortedArray(string field) {
            BusinessTripFilter filter = new BusinessTripFilter {
                SortField = field,
                SortOrder = "Desc"
            };
            BusinessTrip[] businessTrips = new BusinessTrip[] {
                new BusinessTrip() {
                    Id = 1,
                    Name = "bt_47",
                    DateStart = new DateTime(2018, 09, 12),
                    DateEnd = new DateTime(2018, 09, 25),
                    Destination = "Kazan"
                },
                new BusinessTrip() {
                    Id = 2,
                    Name = "bt_48",
                    DateStart = new DateTime(2018, 10, 08),
                    DateEnd = new DateTime(2018, 10, 15),
                    Destination = "Moscow"
                },
                new BusinessTrip() {
                    Id = 3,
                    Name = "bt_50",
                    DateStart = new DateTime(2018, 11, 03),
                    DateEnd = new DateTime(2018, 11, 07),
                    Destination = "Rostov"
                },
                new BusinessTrip() {
                    Id = 4,
                    Name = "bt_53",
                    DateStart = new DateTime(2018, 12, 01),
                    DateEnd = new DateTime(2018, 12, 05),
                    Destination = "Sochi"
                }
            };
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.BusinessTrips.Get(It.IsAny<Func<BusinessTrip, bool>>()))
                .Returns((Func<BusinessTrip, bool> predicate) => businessTrips.Where(predicate));
            BusinessTripService businessTripService = GetNewService(mock.Object);

            BusinessTripDTO[] result = businessTripService.Get(filter).ToArray();

            Assert.AreEqual(4, result.Length);
            Assert.AreEqual(4, result[0].Id);
            Assert.AreEqual(3, result[1].Id);
            Assert.AreEqual(2, result[2].Id);
            Assert.AreEqual(1, result[3].Id);
            Assert.AreEqual("bt_53", result[0].Name);
            Assert.AreEqual("bt_50", result[1].Name);
            Assert.AreEqual("bt_48", result[2].Name);
            Assert.AreEqual("bt_47", result[3].Name);
        }

        /// <summary>
        /// // GetAllAsync method
        /// </summary>
        [Test]
        public override async Task GetAllAsync_GetAsyncMethodReturnsArray_ReturnsSameArray() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.BusinessTrips.GetAllAsync()).ReturnsAsync(() => new BusinessTrip[] {
                new BusinessTrip() { Id = 1, Name = "01.09.2018_022" },
                new BusinessTrip() { Id = 2, Name = "02.09.2018_023" },
                new BusinessTrip() { Id = 3, Name = "03.09.2018_024" }
            });
            BusinessTripService bts = GetNewService(mock.Object);

            BusinessTripDTO[] result = (await bts.GetAllAsync()).ToArray();

            Assert.AreEqual(1, result[0].Id);
            Assert.AreEqual(2, result[1].Id);
            Assert.AreEqual(3, result[2].Id);
            Assert.AreEqual("01.09.2018_022", result[0].Name);
            Assert.AreEqual("02.09.2018_023", result[1].Name);
            Assert.AreEqual("03.09.2018_024", result[2].Name);
        }

        /// <summary>
        /// // ExportJsonAsync method
        /// </summary>
        [Test]
        public override async Task ExportJsonAsync_CreatesJsonFile() {
            string fullPath = "./DiplomMSSQLApp.WEB/Results/BusinessTrips.json";
            File.Delete(fullPath);
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.BusinessTrips.GetAllAsync()).ReturnsAsync(new BusinessTrip[] { });
            BusinessTripService businessTripService = GetNewService(mock.Object);

            await businessTripService.ExportJsonAsync(fullPath);

            Assert.IsTrue(File.Exists(fullPath));
        }
    }
}
