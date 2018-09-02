using DiplomMSSQLApp.BLL.DTO;
using DiplomMSSQLApp.BLL.Services;
using DiplomMSSQLApp.DAL.Entities;
using DiplomMSSQLApp.DAL.Interfaces;
using DiplomMSSQLApp.DAL.Repositories;
using Moq;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiplomMSSQLApp.BLL.UnitTests
{
    [TestFixture]
    public class BusinessTripServiceTests
    {
        /// <summary>
        /// // GetPage method
        /// </summary>
        [Test]
        public void GetPage_Calls_FillsPropertyPageinfo()
        {
            BusinessTripService bts = new BusinessTripService();
            List<BusinessTripDTO> col = new List<BusinessTripDTO>() { new BusinessTripDTO() { Id = 1 } };

            bts.GetPage(col, 1);

            Assert.AreEqual(1, bts.PageInfo.PageNumber);
            Assert.AreEqual(1, bts.PageInfo.TotalItems);
        }

        [Test]
        public void GetPage_SecondParamPageLess1_PageinfoPagenumberEqualsTo1()
        {
            BusinessTripService bts = new BusinessTripService();
            List<BusinessTripDTO> col = new List<BusinessTripDTO>() { new BusinessTripDTO() { Id = 1 } };

            bts.GetPage(col, -5);

            Assert.AreEqual(1, bts.PageInfo.PageNumber);
        }

        [Test]
        public void GetPage_SecondParamPageLargerPageinfoTotalpages_PageinfoPagenumberEqualsToPageinfoTotalpages()
        {
            BusinessTripService bts = new BusinessTripService();
            List<BusinessTripDTO> col = new List<BusinessTripDTO>() { new BusinessTripDTO() { Id = 1 } };

            bts.GetPage(col, 5);
            int totalPages = bts.PageInfo.TotalPages;

            Assert.AreEqual(totalPages, bts.PageInfo.PageNumber);
        }

        [Test]
        public void GetPage_Calls_ReturnsSpecifiedPage()
        {
            BusinessTripService bts = new BusinessTripService();
            int numberOfObjectsPerPage = bts.NumberOfObjectsPerPage;
            List<BusinessTripDTO> col = new List<BusinessTripDTO>();
            for (int i = 1; i < 20; i++)
            {
                col.Add(new BusinessTripDTO() { Id = i });
            }
            List<BusinessTripDTO> expectedCol = new List<BusinessTripDTO>();
            for (int i = numberOfObjectsPerPage + 1; i < 2 * numberOfObjectsPerPage + 1; i++)
            {
                expectedCol.Add(new BusinessTripDTO() { Id = i });     // second page
            }

            var result = bts.GetPage(col, 2);

            CollectionAssert.AreEqual(expectedCol, result);
        }
        /// <summary>
        /// // CreateAsync method
        /// </summary>
        [Test]
        public void CreateAsync_PropertyDatestartLargerPropertyDateend_Throws()
        {
            BusinessTripService bts = new BusinessTripService();
            BusinessTripDTO btDTO = new BusinessTripDTO
            {
                DateStart = new DateTime(2018, 8, 20),
                DateEnd = new DateTime(2018, 8, 10)
            };

            Exception ex = Assert.CatchAsync(async () => await bts.CreateAsync(btDTO, It.IsAny<int[]>()));

            StringAssert.Contains("Дата окончания командировки не должна быть до даты начала", ex.Message);
        }
        [Test]
        public async Task CreateAsync_Calls_CallsFindbyidasyncMethodThreeTimes()
        {
            int actual = 0;
            int[] ids = new int[] { 1, 2, 2, 3 };
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Employees.FindByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(It.IsAny<Employee>())
                .Callback(() => actual++);
            mock.Setup(m => m.BusinessTrips.Create(It.IsAny<BusinessTrip>()));
            BusinessTripService bts = new BusinessTripService(mock.Object);

            await bts.CreateAsync(new BusinessTripDTO(), ids);

            Assert.AreEqual(3, actual);
        }
        [Test]
        public async Task CreateAsync_Calls_CallsCreateMethodOnсe()
        {
            int actual = 0;
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.BusinessTrips.Create(It.IsAny<BusinessTrip>())).Callback(() => actual++);
            BusinessTripService bts = new BusinessTripService(mock.Object);

            await bts.CreateAsync(new BusinessTripDTO(), new int[0]);

            Assert.AreEqual(1, actual);
        }
        [Test]
        public async Task CreateAsync_Calls_CallsSaveasyncMethodOnсe()
        {
            int actual = 0;
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.BusinessTrips.Create(It.IsAny<BusinessTrip>()));
            mock.Setup(m => m.SaveAsync())
                .Returns(Task.CompletedTask)
                .Callback(() => actual++);
            BusinessTripService bts = new BusinessTripService(mock.Object);
            
            await bts.CreateAsync(new BusinessTripDTO(), new int[0]);

            Assert.AreEqual(1, actual);
        }
    }
}
