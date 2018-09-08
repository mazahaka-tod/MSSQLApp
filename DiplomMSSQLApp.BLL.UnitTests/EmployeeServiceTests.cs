using DiplomMSSQLApp.BLL.DTO;
using DiplomMSSQLApp.BLL.Services;
using DiplomMSSQLApp.DAL.Interfaces;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DiplomMSSQLApp.BLL.UnitTests
{
    [TestFixture]
    public class EmployeeServiceTests : BaseServiceTests<EmployeeService>
    {
        protected override EmployeeService GetNewService()
        {
            return new EmployeeService();
        }

        protected override EmployeeService GetNewService(IUnitOfWork uow)
        {
            return new EmployeeService(uow);
        }

        /// <summary>
        /// // GetPage method
        /// </summary>
        [Test]
        public override void GetPage_CallsWithGoodParams_FillsPageInfoProperty()
        {
            EmployeeService es = GetNewService();
            es.NumberOfObjectsPerPage = 2;
            EmployeeDTO[] col = new EmployeeDTO[] {
                new EmployeeDTO() { Id = 1 },
                new EmployeeDTO() { Id = 2 },
                new EmployeeDTO() { Id = 3 }
            };

            es.GetPage(col, 1);

            Assert.AreEqual(1, es.PageInfo.PageNumber);
            Assert.AreEqual(2, es.PageInfo.PageSize);
            Assert.AreEqual(col.Length, es.PageInfo.TotalItems);
        }

        [Test]
        public override void GetPage_RequestedPageLessThan1_ReturnsFirstPage()
        {
            EmployeeService es = GetNewService();

            es.GetPage(new EmployeeDTO[0], -5);

            Assert.AreEqual(1, es.PageInfo.PageNumber);
        }

        [Test]
        public override void GetPage_RequestedPageMoreThanTotalPages_ReturnsLastPage()
        {
            EmployeeService es = GetNewService();
            es.NumberOfObjectsPerPage = 3;
            EmployeeDTO[] col = new EmployeeDTO[] {
                new EmployeeDTO() { Id = 1 },
                new EmployeeDTO() { Id = 2 },
                new EmployeeDTO() { Id = 3 }
            };

            es.GetPage(col, 2);
            int totalPages = es.PageInfo.TotalPages;   // 1

            Assert.AreEqual(totalPages, es.PageInfo.PageNumber);
        }

        [Test]
        public override void GetPage_CallsExistingPage_ReturnsSpecifiedPage()
        {
            EmployeeService es = GetNewService();
            es.NumberOfObjectsPerPage = 3;
            EmployeeDTO[] col = new EmployeeDTO[] {
                new EmployeeDTO() { Id = 1, LastName = "Petrov" },
                new EmployeeDTO() { Id = 2, LastName = "Popov" },
                new EmployeeDTO() { Id = 3, LastName = "Ivanov" },
                new EmployeeDTO() { Id = 4, LastName = "Rozhkov" },
                new EmployeeDTO() { Id = 5, LastName = "Sidorov" }
            };

            EmployeeDTO[] result = es.GetPage(col, 2).ToArray();

            Assert.AreEqual(2, result.Length);
            Assert.AreEqual(4, result[0].Id);
            Assert.AreEqual(5, result[1].Id);
            Assert.AreEqual("Rozhkov", result[0].LastName);
            Assert.AreEqual("Sidorov", result[1].LastName);
        }







        public override Task DeleteAllAsync_Calls_RemoveAllAsyncMethodIsCalledOnce()
        {
            throw new NotImplementedException();
        }

        public override Task DeleteAllAsync_Calls_SaveAsyncMethodIsCalledOnce()
        {
            throw new NotImplementedException();
        }

        public override Task DeleteAsync_FindByIdAsyncMethodReturnsNull_RemoveMethodIsNeverCalled()
        {
            throw new NotImplementedException();
        }

        public override Task DeleteAsync_FindByIdAsyncMethodReturnsNull_SaveAsyncMethodIsNeverCalled()
        {
            throw new NotImplementedException();
        }

        public override Task DeleteAsync_FindByIdAsyncMethodReturnsObject_RemoveMethodIsCalledOnce()
        {
            throw new NotImplementedException();
        }

        public override Task DeleteAsync_FindByIdAsyncMethodReturnsObject_SaveAsyncMethodIsCalledOnce()
        {
            throw new NotImplementedException();
        }

        public override Task EditAsync_CallsWithGoodParams_CallsSaveAsyncMethodOnсe()
        {
            throw new NotImplementedException();
        }

        public override Task EditAsync_CallsWithGoodParams_CallsUpdateMethodOnсe()
        {
            throw new NotImplementedException();
        }

        public override Task FindByIdAsync_IdEqualTo2_ReturnsObjectWithIdEqualTo2()
        {
            throw new NotImplementedException();
        }

        public override void FindByIdAsync_IdParameterIsNull_Throws()
        {
            throw new NotImplementedException();
        }

        public override Task GetAllAsync_GetAsyncMethodReturnsArray_ReturnsSameArray()
        {
            throw new NotImplementedException();
        }
    }
}
