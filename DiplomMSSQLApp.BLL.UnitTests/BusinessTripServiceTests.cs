using DiplomMSSQLApp.BLL.DTO;
using DiplomMSSQLApp.BLL.Services;
using Moq;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DiplomMSSQLApp.BLL.UnitTests
{
    [TestFixture]
    public class BusinessTripServiceTests
    {
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
            IEnumerable<BusinessTripDTO> col = new List<BusinessTripDTO>() { new BusinessTripDTO() { Id = 1 } };

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
    }
}
