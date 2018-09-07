using DiplomMSSQLApp.BLL.DTO;
using DiplomMSSQLApp.BLL.Services;
using DiplomMSSQLApp.DAL.Interfaces;
using NUnit.Framework;
using System.Linq;

namespace DiplomMSSQLApp.BLL.UnitTests
{
    [TestFixture]
    public class DepartmentServiceTests : BaseServiceTests<DepartmentService>
    {
        protected override DepartmentService GetNewService()
        {
            return new DepartmentService();
        }

        protected override DepartmentService GetNewService(IUnitOfWork uow)
        {
            return new DepartmentService(uow);
        }

        /// <summary>
        /// // GetPage method
        /// </summary>
        [Test]
        public override void GetPage_CallsWithGoodParams_FillsPageInfoProperty()
        {
            DepartmentService ds = GetNewService();
            ds.NumberOfObjectsPerPage = 2;
            DepartmentDTO[] col = new DepartmentDTO[] {
                new DepartmentDTO() { Id = 1 },
                new DepartmentDTO() { Id = 2 },
                new DepartmentDTO() { Id = 3 }
            };

            ds.GetPage(col, 1);

            Assert.AreEqual(1, ds.PageInfo.PageNumber);
            Assert.AreEqual(2, ds.PageInfo.PageSize);
            Assert.AreEqual(col.Length, ds.PageInfo.TotalItems);
        }

        [Test]
        public override void GetPage_RequestedPageLessThan1_ReturnsFirstPage()
        {
            DepartmentService ds = GetNewService();

            ds.GetPage(new DepartmentDTO[0], -5);

            Assert.AreEqual(1, ds.PageInfo.PageNumber);
        }

        [Test]
        public override void GetPage_RequestedPageMoreThanTotalPages_ReturnsLastPage()
        {
            DepartmentService ds = GetNewService();
            ds.NumberOfObjectsPerPage = 3;
            DepartmentDTO[] col = new DepartmentDTO[] {
                new DepartmentDTO() { Id = 1 },
                new DepartmentDTO() { Id = 2 },
                new DepartmentDTO() { Id = 3 }
            };
            ds.GetPage(col, 2);
            int totalPages = ds.PageInfo.TotalPages;   // 1

            Assert.AreEqual(totalPages, ds.PageInfo.PageNumber);
        }

        [Test]
        public override void GetPage_CallsExistingPage_ReturnsSpecifiedPage()
        {
            DepartmentService ds = GetNewService();
            ds.NumberOfObjectsPerPage = 3;
            DepartmentDTO[] col = new DepartmentDTO[] {
                new DepartmentDTO() { Id = 1, DepartmentName = "HR" },
                new DepartmentDTO() { Id = 2, DepartmentName = "IT" },
                new DepartmentDTO() { Id = 3, DepartmentName = "Management" },
                new DepartmentDTO() { Id = 4, DepartmentName = "Logistics" },
                new DepartmentDTO() { Id = 5, DepartmentName = "Production" }
            };

            DepartmentDTO[] result = ds.GetPage(col, 2).ToArray();

            Assert.AreEqual(2, result.Length);
            Assert.AreEqual(4, result[0].Id);
            Assert.AreEqual(5, result[1].Id);
            Assert.AreEqual("Logistics", result[0].DepartmentName);
            Assert.AreEqual("Production", result[1].DepartmentName);
        }
    }
}
