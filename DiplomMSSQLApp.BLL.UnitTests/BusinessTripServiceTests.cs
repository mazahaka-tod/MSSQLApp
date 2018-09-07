using DiplomMSSQLApp.BLL.DTO;
using DiplomMSSQLApp.BLL.Services;
using DiplomMSSQLApp.DAL.Entities;
using DiplomMSSQLApp.DAL.Interfaces;
using Moq;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DiplomMSSQLApp.BLL.UnitTests
{
    [TestFixture]
    public class BusinessTripServiceTests : BaseServiceTests<BusinessTripService>
    {
        protected override BusinessTripService GetNewService()
        {
            return new BusinessTripService();
        }

        protected override BusinessTripService GetNewService(IUnitOfWork uow)
        {
            return new BusinessTripService(uow);
        }

        /// <summary>
        /// // GetPage method
        /// </summary>
        [Test]
        public override void GetPage_CallsWithGoodParams_FillsPageInfoProperty()
        {
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
        public override void GetPage_RequestedPageLessThan1_ReturnsFirstPage()
        {
            BusinessTripService bts = GetNewService();

            bts.GetPage(new BusinessTripDTO[0], -5);

            Assert.AreEqual(1, bts.PageInfo.PageNumber);
        }

        [Test]
        public override void GetPage_RequestedPageMoreThanTotalPages_ReturnsLastPage()
        {
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
        public override void GetPage_CallsExistingPage_ReturnsSpecifiedPage()
        {
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
        public void CreateAsync_DateStartPropertyMoreThanDateEndProperty_Throws()
        {
            BusinessTripService bts = GetNewService();
            BusinessTripDTO item = new BusinessTripDTO {
                DateStart = new DateTime(2018, 8, 20),
                DateEnd = new DateTime(2018, 8, 10)
            };

            Exception ex = Assert.CatchAsync(async () => await bts.CreateAsync(item, It.IsAny<int[]>()));

            StringAssert.Contains("Дата окончания командировки не должна быть до даты начала", ex.Message);
        }
        [Test]
        public async Task CreateAsync_IdsParameterContainsThreeDifferentValues_CallsFindByIdAsyncMethodThreeTimes()
        {
            int[] ids = new int[] { 1, 2, 2, 3 };
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.BusinessTrips.Create(It.IsAny<BusinessTrip>()));
            mock.Setup(m => m.Employees.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(It.IsAny<Employee>());
            BusinessTripService bts = GetNewService(mock.Object);

            await bts.CreateAsync(new BusinessTripDTO(), ids);

            mock.Verify(m => m.Employees.FindByIdAsync(It.IsAny<int>()), Times.Exactly(3));
        }
        [Test]
        public async Task CreateAsync_CallsWithGoodParams_CallsCreateMethodOnсe()
        {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.BusinessTrips.Create(It.IsAny<BusinessTrip>()));
            BusinessTripService bts = GetNewService(mock.Object);

            await bts.CreateAsync(new BusinessTripDTO(), new int[0]);

            mock.Verify(m => m.BusinessTrips.Create(It.IsAny<BusinessTrip>()), Times.Once());
        }
        [Test]
        public async Task CreateAsync_CallsWithGoodParams_CallsSaveAsyncMethodOnсe()
        {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.BusinessTrips.Create(It.IsAny<BusinessTrip>()));
            BusinessTripService bts = GetNewService(mock.Object);
            
            await bts.CreateAsync(new BusinessTripDTO(), new int[0]);

            mock.Verify((m => m.SaveAsync()), Times.Once());
        }
    }
}
