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

        /// <summary>
        /// // CreateAsync method
        /// </summary>
        [Test]
        public void CreateAsync_TitlePropertyIsNull_Throws()
        {
            PostService ps = GetNewService();
            PostDTO item = new PostDTO {
                Title = null
            };

            Exception ex = Assert.CatchAsync(async () => await ps.CreateAsync(item));

            StringAssert.Contains("Требуется ввести название должности", ex.Message);
        }

        [Test]
        public void CreateAsync_MinSalaryPropertyMoreThanMaxSalaryProperty_Throws()
        {
            PostService ps = GetNewService();
            PostDTO item = new PostDTO {
                Title = "Administrator",
                MinSalary = 20000,
                MaxSalary = 10000
            };

            Exception ex = Assert.CatchAsync(async () => await ps.CreateAsync(item));

            StringAssert.Contains("Минимальная зарплата не может быть больше максимальной", ex.Message);
        }

        [Test]
        public async Task CreateAsync_CallsWithGoodParams_CallsCreateMethodOnсe()
        {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Posts.Create(It.IsAny<Post>()));
            PostService ps = GetNewService(mock.Object);
            PostDTO item = new PostDTO {
                Title = "Administrator"
            };

            await ps.CreateAsync(item);

            mock.Verify(m => m.Posts.Create(It.IsAny<Post>()), Times.Once());
        }

        [Test]
        public async Task CreateAsync_CallsWithGoodParams_CallsSaveAsyncMethodOnсe()
        {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Posts.Create(It.IsAny<Post>()));
            PostService ps = GetNewService(mock.Object);
            PostDTO item = new PostDTO {
                Title = "Administrator"
            };

            await ps.CreateAsync(item);

            mock.Verify((m => m.SaveAsync()), Times.Once());
        }

        /// <summary>
        /// // DeleteAsync method
        /// </summary>
        [Test]
        public override async Task DeleteAsync_FindByIdAsyncMethodReturnsNull_RemoveMethodIsNeverCalled()
        {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Posts.FindByIdAsync(It.IsAny<int>())).Returns(Task.FromResult<Post>(null));
            PostService ps = GetNewService(mock.Object);

            await ps.DeleteAsync(It.IsAny<int>());

            mock.Verify(m => m.Posts.Remove(It.IsAny<Post>()), Times.Never);
        }

        [Test]
        public override async Task DeleteAsync_FindByIdAsyncMethodReturnsNull_SaveAsyncMethodIsNeverCalled()
        {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Posts.FindByIdAsync(It.IsAny<int>())).Returns(Task.FromResult<Post>(null));
            PostService ps = GetNewService(mock.Object);

            await ps.DeleteAsync(It.IsAny<int>());

            mock.Verify(m => m.SaveAsync(), Times.Never);
        }

        [Test]
        public override async Task DeleteAsync_FindByIdAsyncMethodReturnsObject_RemoveMethodIsCalledOnce()
        {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Posts.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(new Post());
            PostService ps = GetNewService(mock.Object);

            await ps.DeleteAsync(It.IsAny<int>());

            mock.Verify(m => m.Posts.Remove(It.IsAny<Post>()), Times.Once);
        }

        [Test]
        public override async Task DeleteAsync_FindByIdAsyncMethodReturnsObject_SaveAsyncMethodIsCalledOnce()
        {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Posts.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(new Post());
            PostService ps = GetNewService(mock.Object);

            await ps.DeleteAsync(It.IsAny<int>());

            mock.Verify(m => m.SaveAsync(), Times.Once);
        }

        /// <summary>
        /// // DeleteAllAsync method
        /// </summary>
        [Test]
        public override async Task DeleteAllAsync_Calls_RemoveAllAsyncMethodIsCalledOnce()
        {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Posts.RemoveAllAsync()).Returns(Task.CompletedTask);
            PostService ps = GetNewService(mock.Object);

            await ps.DeleteAllAsync();

            mock.Verify(m => m.Posts.RemoveAllAsync(), Times.Once);
        }

        [Test]
        public override async Task DeleteAllAsync_Calls_SaveAsyncMethodIsCalledOnce()
        {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Posts.RemoveAllAsync()).Returns(Task.CompletedTask);
            PostService ps = GetNewService(mock.Object);

            await ps.DeleteAllAsync();

            mock.Verify(m => m.SaveAsync(), Times.Once);
        }

        /// <summary>
        /// // EditAsync method
        /// </summary>
        public override Task EditAsync_CallsWithGoodParams_CallsSaveAsyncMethodOnсe()
        {
            throw new NotImplementedException();
        }

        public override Task EditAsync_CallsWithGoodParams_CallsUpdateMethodOnсe()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// // FindByIdAsync method
        /// </summary>
        public override Task FindByIdAsync_IdEqualTo2_ReturnsObjectWithIdEqualTo2()
        {
            throw new NotImplementedException();
        }

        public override void FindByIdAsync_IdParameterIsNull_Throws()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// // GetAllAsync method
        /// </summary>
        public override Task GetAllAsync_GetAsyncMethodReturnsArray_ReturnsSameArray()
        {
            throw new NotImplementedException();
        }
    }
}
