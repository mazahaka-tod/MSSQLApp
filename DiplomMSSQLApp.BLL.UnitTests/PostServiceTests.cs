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
using System.Text.RegularExpressions;
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
        [Test]
        public void EditAsync_TitlePropertyIsNull_Throws()
        {
            PostService ps = GetNewService();
            PostDTO item = new PostDTO {
                Title = null
            };

            Exception ex = Assert.CatchAsync(async () => await ps.EditAsync(item));

            StringAssert.Contains("Требуется ввести название должности", ex.Message);
        }

        [Test]
        public void EditAsync_MinSalaryPropertyMoreThanMaxSalaryProperty_Throws()
        {
            PostService ps = GetNewService();
            PostDTO item = new PostDTO {
                Title = "Administrator",
                MinSalary = 20000,
                MaxSalary = 10000
            };

            Exception ex = Assert.CatchAsync(async () => await ps.EditAsync(item));

            StringAssert.Contains("Минимальная зарплата не может быть больше максимальной", ex.Message);
        }

        [Test]
        public override async Task EditAsync_CallsWithGoodParams_CallsUpdateMethodOnсe()
        {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Posts.Update(It.IsAny<Post>()));
            PostService ps = GetNewService(mock.Object);
            PostDTO item = new PostDTO {
                Title = "Administrator"
            };

            await ps.EditAsync(item);

            mock.Verify(m => m.Posts.Update(It.IsAny<Post>()), Times.Once());
        }

        [Test]
        public override async Task EditAsync_CallsWithGoodParams_CallsSaveAsyncMethodOnсe()
        {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Posts.Update(It.IsAny<Post>()));
            PostService ps = GetNewService(mock.Object);
            PostDTO item = new PostDTO {
                Title = "Administrator"
            };

            await ps.EditAsync(item);

            mock.Verify((m => m.SaveAsync()), Times.Once());
        }

        /// <summary>
        /// // FindByIdAsync method
        /// </summary>
        [Test]
        public override void FindByIdAsync_IdParameterIsNull_Throws()
        {
            PostService ps = GetNewService();

            Exception ex = Assert.CatchAsync(async () => await ps.FindByIdAsync(null));

            StringAssert.Contains("Не установлено id должности", ex.Message);
        }

        [Test]
        public void FindByIdAsync_PostNotFound_Throws()
        {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Posts.FindByIdAsync(It.IsAny<int>())).Returns(Task.FromResult<Post>(null));
            PostService ps = GetNewService(mock.Object);

            Exception ex = Assert.CatchAsync(async () => await ps.FindByIdAsync(It.IsAny<int>()));

            StringAssert.Contains("Должность не найдена", ex.Message);
        }

        [Test]
        public override async Task FindByIdAsync_IdEqualTo2_ReturnsObjectWithIdEqualTo2()
        {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Posts.FindByIdAsync(It.IsAny<int>())).ReturnsAsync((int item_id) => new Post() { Id = item_id });
            PostService ps = GetNewService(mock.Object);

            PostDTO result = await ps.FindByIdAsync(2);

            Assert.AreEqual(2, result.Id);
        }

        /// <summary>
        /// // GetAllAsync method
        /// </summary>
        [Test]
        public override async Task GetAllAsync_GetAsyncMethodReturnsArray_ReturnsSameArray()
        {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Posts.GetAsync()).ReturnsAsync(() => new Post[] {
                new Post() { Id = 1, Title = "Economist" },
                new Post() { Id = 2, Title = "Manager" },
                new Post() { Id = 3, Title = "Agent" }
            });
            PostService ps = GetNewService(mock.Object);

            PostDTO[] result = (await ps.GetAllAsync()).ToArray();

            Assert.AreEqual(1, result[0].Id);
            Assert.AreEqual(2, result[1].Id);
            Assert.AreEqual(3, result[2].Id);
            Assert.AreEqual("Economist", result[0].Title);
            Assert.AreEqual("Manager", result[1].Title);
            Assert.AreEqual("Agent", result[2].Title);
        }

        /// <summary>
        /// // ExportJsonAsync method
        /// </summary>
        [Test]
        public async Task ExportJsonAsync_CreatesJsonFile() {
            string fullPath = "./DiplomMSSQLApp.WEB/Results/Post/Posts.json";
            File.Delete(fullPath);
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Posts.GetAsync()).ReturnsAsync(new Post[] { new Post() { Title = "1" } });
            PostService postService = GetNewService(mock.Object);

            await postService.ExportJsonAsync(fullPath);

            Assert.IsTrue(File.Exists(fullPath));
        }

        /// <summary>
        /// // TestCreateAsync method
        /// </summary>
        [Test]
        public async Task TestCreateAsync_CallsWithGoodParameter_CallsCreateMethodOnce() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Posts.Create(It.IsAny<IEnumerable<Post>>()));
            PostService postService = GetNewService(mock.Object);
            postService.PathToFileForTests = "./DiplomMSSQLApp.WEB/Results/Post/Create.txt";

            await postService.TestCreateAsync(1);

            mock.Verify(m => m.Posts.Create(It.IsAny<IEnumerable<Post>>()), Times.Once());
        }

        [Test]
        public async Task TestCreateAsync_CallsWithGoodParameter_CallsSaveAsyncMethodOnce() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Posts.Create(It.IsAny<IEnumerable<Post>>()));
            PostService postService = GetNewService(mock.Object);
            postService.PathToFileForTests = "./DiplomMSSQLApp.WEB/Results/Post/Create.txt";

            await postService.TestCreateAsync(1);

            mock.Verify((m => m.SaveAsync()), Times.Once());
        }

        [Test]
        public async Task TestCreateAsync_CallsWithGoodParameter_CreatesResultFile() {
            string fullPath = "./DiplomMSSQLApp.WEB/Results/Post/Create.txt";
            File.Delete(fullPath);
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Posts.Create(It.IsAny<IEnumerable<Post>>()));
            PostService postService = GetNewService(mock.Object);
            postService.PathToFileForTests = fullPath;

            await postService.TestCreateAsync(1);

            Assert.IsTrue(File.Exists(fullPath));
        }

        [Test]
        public async Task TestCreateAsync_CallsWithGoodParameter_TestTimeIsWrittenToElapsedTimeProperty() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Posts.Create(It.IsAny<IEnumerable<Post>>()));
            PostService postService = GetNewService(mock.Object);
            postService.PathToFileForTests = "./DiplomMSSQLApp.WEB/Results/Post/Create.txt";

            await postService.TestCreateAsync(1);

            Assert.IsTrue(Regex.IsMatch(postService.ElapsedTime, @"^\d{2}:\d{2}:\d{2}\.\d{3}$"));
        }

        /// <summary>
        /// // TestReadAsync method
        /// </summary>
        [Test]
        public async Task TestReadAsync_ParameterIsThree_CallsGetMethodFourTimes() {
            int num = 3;
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Posts.GetAsync()).ReturnsAsync(new Post[] { });
            mock.Setup(m => m.Posts.Get(It.IsAny<int>())).Returns(new Post[] { });
            PostService postService = GetNewService(mock.Object);
            postService.PathToFileForTests = "./DiplomMSSQLApp.WEB/Results/Post/Read.txt";

            await postService.TestReadAsync(num, 0);

            mock.Verify(m => m.Posts.Get(It.IsAny<int>()), Times.Exactly(4));
        }

        [Test]
        public async Task TestReadAsync_CallsWithGoodParameter_CreatesResultFile() {
            string fullPath = "./DiplomMSSQLApp.WEB/Results/Post/Read.txt";
            File.Delete(fullPath);
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Posts.GetAsync()).ReturnsAsync(new Post[] { });
            mock.Setup(m => m.Posts.Get(It.IsAny<int>())).Returns(new Post[] { });
            PostService postService = GetNewService(mock.Object);
            postService.PathToFileForTests = fullPath;

            await postService.TestReadAsync(1, 0);

            Assert.IsTrue(File.Exists(fullPath));
        }

        [Test]
        public async Task TestReadAsync_CallsWithGoodParameter_TestTimeIsWrittenToElapsedTimeProperty() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Posts.GetAsync()).ReturnsAsync(new Post[] { });
            mock.Setup(m => m.Posts.Get(It.IsAny<int>())).Returns(new Post[] { });
            PostService postService = GetNewService(mock.Object);
            postService.PathToFileForTests = "./DiplomMSSQLApp.WEB/Results/Post/Read.txt";

            await postService.TestReadAsync(1, 0);

            Assert.IsTrue(Regex.IsMatch(postService.ElapsedTime, @"^\d{2}:\d{2}:\d{2}\.\d{3}$"));
        }

        /// <summary>
        /// // TestUpdateAsync method
        /// </summary>
        [Test]
        public async Task TestUpdateAsync_ParameterIsOne_CallsUpdateMethodOnce() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Posts.GetAsync()).ReturnsAsync(new Post[] { new Post(), new Post() });
            PostService postService = GetNewService(mock.Object);
            postService.PathToFileForTests = "./DiplomMSSQLApp.WEB/Results/Post/Update.txt";

            await postService.TestUpdateAsync(1);

            mock.Verify(m => m.Posts.Update(It.IsAny<Post>()), Times.Once());
        }

        [Test]
        public async Task TestUpdateAsync_ParameterIsOne_CallsSaveAsyncMethodOnce() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Posts.GetAsync()).ReturnsAsync(new Post[] { new Post(), new Post() });
            PostService postService = GetNewService(mock.Object);
            postService.PathToFileForTests = "./DiplomMSSQLApp.WEB/Results/Post/Update.txt";

            await postService.TestUpdateAsync(1);

            mock.Verify((m => m.SaveAsync()), Times.Once());
        }

        [Test]
        public async Task TestUpdateAsync_CallsWithGoodParameter_CreatesResultFile() {
            string fullPath = "./DiplomMSSQLApp.WEB/Results/Post/Update.txt";
            File.Delete(fullPath);
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Posts.GetAsync()).ReturnsAsync(new Post[] { });
            PostService postService = GetNewService(mock.Object);
            postService.PathToFileForTests = fullPath;

            await postService.TestUpdateAsync(1);

            Assert.IsTrue(File.Exists(fullPath));
        }

        [Test]
        public async Task TestUpdateAsync_CallsWithGoodParameter_TestTimeIsWrittenToElapsedTimeProperty() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Posts.GetAsync()).ReturnsAsync(new Post[] { });
            PostService postService = GetNewService(mock.Object);
            postService.PathToFileForTests = "./DiplomMSSQLApp.WEB/Results/Post/Update.txt";

            await postService.TestUpdateAsync(1);

            Assert.IsTrue(Regex.IsMatch(postService.ElapsedTime, @"^\d{2}:\d{2}:\d{2}\.\d{3}$"));
        }

        /// <summary>
        /// // TestDeleteAsync method
        /// </summary>
        [Test]
        public async Task TestDeleteAsync_ParameterIsOne_CallsUpdateMethodOnce() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Posts.GetAsync()).ReturnsAsync(new Post[] { });
            PostService postService = GetNewService(mock.Object);
            postService.PathToFileForTests = "./DiplomMSSQLApp.WEB/Results/Post/Delete.txt";

            await postService.TestDeleteAsync(1);

            mock.Verify(m => m.Posts.RemoveSeries(It.IsAny<IEnumerable<Post>>()), Times.Once());
        }

        [Test]
        public async Task TestDeleteAsync_ParameterIsOne_CallsSaveAsyncMethodOnce() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Posts.GetAsync()).ReturnsAsync(new Post[] { });
            PostService postService = GetNewService(mock.Object);
            postService.PathToFileForTests = "./DiplomMSSQLApp.WEB/Results/Post/Delete.txt";

            await postService.TestDeleteAsync(1);

            mock.Verify((m => m.SaveAsync()), Times.Once());
        }

        [Test]
        public async Task TestDeleteAsync_CallsWithGoodParameter_CreatesResultFile() {
            string fullPath = "./DiplomMSSQLApp.WEB/Results/Post/Delete.txt";
            File.Delete(fullPath);
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Posts.GetAsync()).ReturnsAsync(new Post[] { });
            PostService postService = GetNewService(mock.Object);
            postService.PathToFileForTests = fullPath;

            await postService.TestDeleteAsync(1);

            Assert.IsTrue(File.Exists(fullPath));
        }

        [Test]
        public async Task TestDeleteAsync_CallsWithGoodParameter_TestTimeIsWrittenToElapsedTimeProperty() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Posts.GetAsync()).ReturnsAsync(new Post[] { });
            PostService postService = GetNewService(mock.Object);
            postService.PathToFileForTests = "./DiplomMSSQLApp.WEB/Results/Post/Delete.txt";

            await postService.TestDeleteAsync(1);

            Assert.IsTrue(Regex.IsMatch(postService.ElapsedTime, @"^\d{2}:\d{2}:\d{2}\.\d{3}$"));
        }

    }
}
