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
    public class PostServiceTests : BaseServiceTests<PostService>
    {
        protected override PostService GetNewService()
        {
            return new PostService();
        }

        protected override PostService GetNewService(IUnitOfWork uow)
        {
            return new PostService(uow);
        }

        /// <summary>
        /// // GetPage method
        /// </summary>
        [Test]
        public override void GetPage_CallsWithGoodParams_FillsPageInfoProperty()
        {
            PostService ps = GetNewService();
            ps.NumberOfObjectsPerPage = 2;
            PostDTO[] col = new PostDTO[] {
                new PostDTO() { Id = 1 },
                new PostDTO() { Id = 2 },
                new PostDTO() { Id = 3 }
            };

            ps.GetPage(col, 1);

            Assert.AreEqual(1, ps.PageInfo.PageNumber);
            Assert.AreEqual(2, ps.PageInfo.PageSize);
            Assert.AreEqual(col.Length, ps.PageInfo.TotalItems);
        }

        [Test]
        public override void GetPage_RequestedPageLessThan1_ReturnsFirstPage()
        {
            PostService ps = GetNewService();

            ps.GetPage(new PostDTO[0], -5);

            Assert.AreEqual(1, ps.PageInfo.PageNumber);
        }

        [Test]
        public override void GetPage_RequestedPageMoreThanTotalPages_ReturnsLastPage()
        {
            PostService ps = GetNewService();
            ps.NumberOfObjectsPerPage = 3;
            PostDTO[] col = new PostDTO[] {
                new PostDTO() { Id = 1 },
                new PostDTO() { Id = 2 },
                new PostDTO() { Id = 3 }
            };

            ps.GetPage(col, 2);
            int totalPages = ps.PageInfo.TotalPages;   // 1

            Assert.AreEqual(totalPages, ps.PageInfo.PageNumber);
        }

        [Test]
        public override void GetPage_CallsExistingPage_ReturnsSpecifiedPage()
        {
            PostService ps = GetNewService();
            ps.NumberOfObjectsPerPage = 3;
            PostDTO[] col = new PostDTO[] {
                new PostDTO() { Id = 1, Title = "Economist" },
                new PostDTO() { Id = 2, Title = "Manager" },
                new PostDTO() { Id = 3, Title = "Agent" },
                new PostDTO() { Id = 4, Title = "Intern" },
                new PostDTO() { Id = 5, Title = "Engineer" }
            };

            PostDTO[] result = ps.GetPage(col, 2).ToArray();

            Assert.AreEqual(2, result.Length);
            Assert.AreEqual(4, result[0].Id);
            Assert.AreEqual(5, result[1].Id);
            Assert.AreEqual("Intern", result[0].Title);
            Assert.AreEqual("Engineer", result[1].Title);
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
