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
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace DiplomMSSQLApp.BLL.UnitTests {
    [TestFixture]
    public class PostServiceTests : BaseServiceTests<PostService> {
        protected override PostService GetNewService() {
            return new PostService();
        }

        protected override PostService GetNewService(IUnitOfWork uow) {
            return new PostService(uow);
        }

        /// <summary>
        /// // GetPage method
        /// </summary>
        [Test]
        public override void GetPage_CallsWithGoodParams_FillsPageInfoProperty() {
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
        public override void GetPage_RequestedPageLessThan1_ReturnsFirstPage() {
            PostService ps = GetNewService();

            ps.GetPage(new PostDTO[0], -5);

            Assert.AreEqual(1, ps.PageInfo.PageNumber);
        }

        [Test]
        public override void GetPage_RequestedPageMoreThanTotalPages_ReturnsLastPage() {
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
        public override void GetPage_CallsExistingPage_ReturnsSpecifiedPage() {
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
        public void CreateAsync_TitlePropertyIsNull_Throws() {
            PostService ps = GetNewService();
            PostDTO item = new PostDTO {
                Title = null
            };

            Exception ex = Assert.CatchAsync(async () => await ps.CreateAsync(item));

            StringAssert.Contains("Требуется ввести название должности", ex.Message);
        }

        [Test]
        public void CreateAsync_NumberOfUnitsPropertyIsNull_Throws() {
            PostService ps = GetNewService();
            PostDTO item = new PostDTO {
                Title = "Manager"
            };

            Exception ex = Assert.CatchAsync(async () => await ps.CreateAsync(item));

            StringAssert.Contains("Требуется ввести количество штатных единиц", ex.Message);
        }

        [Test]
        public void CreateAsync_NumberOfUnitsPropertyLess0_Throws() {
            PostService ps = GetNewService();
            PostDTO item = new PostDTO {
                Title = "Manager",
                NumberOfUnits = -1
            };

            Exception ex = Assert.CatchAsync(async () => await ps.CreateAsync(item));

            StringAssert.Contains("Значение должно быть в диапазоне [0, 10000]", ex.Message);
        }

        [Test]
        public void CreateAsync_NumberOfUnitsPropertyMore10000_Throws() {
            PostService ps = GetNewService();
            PostDTO item = new PostDTO {
                Title = "Manager",
                NumberOfUnits = 10001
            };

            Exception ex = Assert.CatchAsync(async () => await ps.CreateAsync(item));

            StringAssert.Contains("Значение должно быть в диапазоне [0, 10000]", ex.Message);
        }

        [Test]
        public void CreateAsync_SalaryPropertyIsNull_Throws() {
            PostService ps = GetNewService();
            PostDTO item = new PostDTO {
                Title = "Manager",
                NumberOfUnits = 3
            };

            Exception ex = Assert.CatchAsync(async () => await ps.CreateAsync(item));

            StringAssert.Contains("Требуется ввести оклад", ex.Message);
        }

        [Test]
        public void CreateAsync_SalaryPropertyLess0_Throws() {
            PostService ps = GetNewService();
            PostDTO item = new PostDTO {
                Title = "Manager",
                NumberOfUnits = 3,
                Salary = -1
            };

            Exception ex = Assert.CatchAsync(async () => await ps.CreateAsync(item));

            StringAssert.Contains("Оклад должен быть в диапазоне [0, 1000000]", ex.Message);
        }

        [Test]
        public void CreateAsync_SalaryPropertyMore1000000_Throws() {
            PostService ps = GetNewService();
            PostDTO item = new PostDTO {
                Title = "Manager",
                NumberOfUnits = 3,
                Salary = 1000001
            };

            Exception ex = Assert.CatchAsync(async () => await ps.CreateAsync(item));

            StringAssert.Contains("Оклад должен быть в диапазоне [0, 1000000]", ex.Message);
        }

        [Test]
        public void CreateAsync_PremiumPropertyIsNull_Throws() {
            PostService ps = GetNewService();
            PostDTO item = new PostDTO {
                Title = "Manager",
                NumberOfUnits = 3,
                Salary = 60000
            };

            Exception ex = Assert.CatchAsync(async () => await ps.CreateAsync(item));

            StringAssert.Contains("Требуется ввести надбавку", ex.Message);
        }

        [Test]
        public void CreateAsync_PremiumPropertyLess0_Throws() {
            PostService ps = GetNewService();
            PostDTO item = new PostDTO {
                Title = "Manager",
                NumberOfUnits = 3,
                Salary = 60000,
                Premium = -1
            };

            Exception ex = Assert.CatchAsync(async () => await ps.CreateAsync(item));

            StringAssert.Contains("Надбавка должна быть в диапазоне [0, 100000]", ex.Message);
        }

        [Test]
        public void CreateAsync_PremiumPropertyMore100000_Throws() {
            PostService ps = GetNewService();
            PostDTO item = new PostDTO {
                Title = "Manager",
                NumberOfUnits = 3,
                Salary = 60000,
                Premium = 100001
            };

            Exception ex = Assert.CatchAsync(async () => await ps.CreateAsync(item));

            StringAssert.Contains("Надбавка должна быть в диапазоне [0, 100000]", ex.Message);
        }

        [Test]
        public void CreateAsync_PostAlreadyExists_Throws() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Employees.CountAsync(It.IsAny<Expression<Func<Employee, bool>>>())).ReturnsAsync(3);
            mock.Setup(m => m.Posts.CountAsync(It.IsAny<Expression<Func<Post, bool>>>())).ReturnsAsync(1);
            PostService ps = GetNewService(mock.Object);
            PostDTO item = new PostDTO {
                Title = "Manager",
                NumberOfUnits = 3,
                Salary = 60000,
                Premium = 10000
            };

            Exception ex = Assert.CatchAsync(async () => await ps.CreateAsync(item));

            StringAssert.Contains("Должность уже существует", ex.Message);
        }

        [Test]
        public async Task CreateAsync_CallsWithGoodParams_CallsCreateMethodOnсe() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Posts.Create(It.IsAny<Post>()));
            mock.Setup(m => m.Employees.CountAsync(It.IsAny<Expression<Func<Employee, bool>>>())).ReturnsAsync(-1);
            PostService ps = GetNewService(mock.Object);
            PostDTO item = new PostDTO {
                Title = "Administrator",
                NumberOfUnits = 1,
                Salary = 50000,
                Premium = 10000
            };

            await ps.CreateAsync(item);

            mock.Verify(m => m.Posts.Create(It.IsAny<Post>()), Times.Once());
        }

        [Test]
        public async Task CreateAsync_CallsWithGoodParams_CallsSaveAsyncMethodOnсe() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Posts.Create(It.IsAny<Post>()));
            mock.Setup(m => m.Employees.CountAsync(It.IsAny<Expression<Func<Employee, bool>>>())).ReturnsAsync(-1);
            PostService ps = GetNewService(mock.Object);
            PostDTO item = new PostDTO {
                Title = "Administrator",
                NumberOfUnits = 1,
                Salary = 50000,
                Premium = 10000
            };

            await ps.CreateAsync(item);

            mock.Verify(m => m.SaveAsync(), Times.Once());
        }

        /// <summary>
        /// // DeleteAsync method
        /// </summary>
        [Test]
        public override async Task DeleteAsync_FindByIdAsyncMethodReturnsNull_RemoveMethodIsNeverCalled() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Posts.FindByIdAsync(It.IsAny<int>())).Returns(Task.FromResult<Post>(null));
            PostService ps = GetNewService(mock.Object);

            await ps.DeleteAsync(It.IsAny<int>());

            mock.Verify(m => m.Posts.Remove(It.IsAny<Post>()), Times.Never);
        }

        [Test]
        public override async Task DeleteAsync_FindByIdAsyncMethodReturnsNull_SaveAsyncMethodIsNeverCalled() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Posts.FindByIdAsync(It.IsAny<int>())).Returns(Task.FromResult<Post>(null));
            PostService ps = GetNewService(mock.Object);

            await ps.DeleteAsync(It.IsAny<int>());

            mock.Verify(m => m.SaveAsync(), Times.Never);
        }

        [Test]
        public override async Task DeleteAsync_FindByIdAsyncMethodReturnsObject_RemoveMethodIsCalledOnce() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Posts.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(new Post());
            PostService ps = GetNewService(mock.Object);

            await ps.DeleteAsync(It.IsAny<int>());

            mock.Verify(m => m.Posts.Remove(It.IsAny<Post>()), Times.Once);
        }

        [Test]
        public override async Task DeleteAsync_FindByIdAsyncMethodReturnsObject_SaveAsyncMethodIsCalledOnce() {
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
        public async Task DeleteAllAsync_Calls_RemoveSeriesMethodIsCalledOnce() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Posts.RemoveSeries(It.IsAny<IEnumerable<Post>>()));
            PostService ps = GetNewService(mock.Object);

            await ps.DeleteAllAsync();

            mock.Verify(m => m.Posts.RemoveSeries(It.IsAny<IEnumerable<Post>>()), Times.Once);
        }

        [Test]
        public override async Task DeleteAllAsync_Calls_SaveAsyncMethodIsCalledOnce() {
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
        public void EditAsync_TitlePropertyIsNull_Throws() {
            PostService ps = GetNewService();
            PostDTO item = new PostDTO {
                Title = null
            };

            Exception ex = Assert.CatchAsync(async () => await ps.EditAsync(item));

            StringAssert.Contains("Требуется ввести название должности", ex.Message);
        }

        [Test]
        public void EditAsync_NumberOfUnitsPropertyIsNull_Throws() {
            PostService ps = GetNewService();
            PostDTO item = new PostDTO {
                Title = "Manager"
            };

            Exception ex = Assert.CatchAsync(async () => await ps.EditAsync(item));

            StringAssert.Contains("Требуется ввести количество штатных единиц", ex.Message);
        }

        [Test]
        public void EditAsync_NumberOfUnitsPropertyLess0_Throws() {
            PostService ps = GetNewService();
            PostDTO item = new PostDTO {
                Title = "Manager",
                NumberOfUnits = -1
            };

            Exception ex = Assert.CatchAsync(async () => await ps.EditAsync(item));

            StringAssert.Contains("Значение должно быть в диапазоне [0, 10000]", ex.Message);
        }

        [Test]
        public void EditAsync_NumberOfUnitsPropertyMore10000_Throws() {
            PostService ps = GetNewService();
            PostDTO item = new PostDTO {
                Title = "Manager",
                NumberOfUnits = 10001
            };

            Exception ex = Assert.CatchAsync(async () => await ps.EditAsync(item));

            StringAssert.Contains("Значение должно быть в диапазоне [0, 10000]", ex.Message);
        }

        [Test]
        public void EditAsync_SalaryPropertyIsNull_Throws() {
            PostService ps = GetNewService();
            PostDTO item = new PostDTO {
                Title = "Manager",
                NumberOfUnits = 3
            };

            Exception ex = Assert.CatchAsync(async () => await ps.EditAsync(item));

            StringAssert.Contains("Требуется ввести оклад", ex.Message);
        }

        [Test]
        public void EditAsync_SalaryPropertyLess0_Throws() {
            PostService ps = GetNewService();
            PostDTO item = new PostDTO {
                Title = "Manager",
                NumberOfUnits = 3,
                Salary = -1
            };

            Exception ex = Assert.CatchAsync(async () => await ps.EditAsync(item));

            StringAssert.Contains("Оклад должен быть в диапазоне [0, 1000000]", ex.Message);
        }

        [Test]
        public void EditAsync_SalaryPropertyMore1000000_Throws() {
            PostService ps = GetNewService();
            PostDTO item = new PostDTO {
                Title = "Manager",
                NumberOfUnits = 3,
                Salary = 1000001
            };

            Exception ex = Assert.CatchAsync(async () => await ps.EditAsync(item));

            StringAssert.Contains("Оклад должен быть в диапазоне [0, 1000000]", ex.Message);
        }

        [Test]
        public void EditAsync_PremiumPropertyIsNull_Throws() {
            PostService ps = GetNewService();
            PostDTO item = new PostDTO {
                Title = "Manager",
                NumberOfUnits = 3,
                Salary = 60000
            };

            Exception ex = Assert.CatchAsync(async () => await ps.EditAsync(item));

            StringAssert.Contains("Требуется ввести надбавку", ex.Message);
        }

        [Test]
        public void EditAsync_PremiumPropertyLess0_Throws() {
            PostService ps = GetNewService();
            PostDTO item = new PostDTO {
                Title = "Manager",
                NumberOfUnits = 3,
                Salary = 60000,
                Premium = -1
            };

            Exception ex = Assert.CatchAsync(async () => await ps.EditAsync(item));

            StringAssert.Contains("Надбавка должна быть в диапазоне [0, 100000]", ex.Message);
        }

        [Test]
        public void EditAsync_PremiumPropertyMore100000_Throws() {
            PostService ps = GetNewService();
            PostDTO item = new PostDTO {
                Title = "Manager",
                NumberOfUnits = 3,
                Salary = 60000,
                Premium = 100001
            };

            Exception ex = Assert.CatchAsync(async () => await ps.EditAsync(item));

            StringAssert.Contains("Надбавка должна быть в диапазоне [0, 100000]", ex.Message);
        }

        [Test]
        public void EditAsync_EmployeesCountMoreNumberOfUnitsProperty_Throws() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Employees.CountAsync(It.IsAny<Expression<Func<Employee, bool>>>())).ReturnsAsync(4);
            PostService ps = GetNewService(mock.Object);
            PostDTO item = new PostDTO {
                Title = "Manager",
                NumberOfUnits = 3,
                Salary = 60000,
                Premium = 10000
            };

            Exception ex = Assert.CatchAsync(async () => await ps.EditAsync(item));

            StringAssert.Contains("Количество штатных единиц не может быть меньше, " +
                    "чем количество сотрудников, работающих в должности в данный момент [4]", ex.Message);
        }

        [Test]
        public void EditAsync_PostAlreadyExists_Throws() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Employees.CountAsync(It.IsAny<Expression<Func<Employee, bool>>>())).ReturnsAsync(3);
            mock.Setup(m => m.Posts.CountAsync(It.IsAny<Expression<Func<Post, bool>>>())).ReturnsAsync(1);
            PostService ps = GetNewService(mock.Object);
            PostDTO item = new PostDTO {
                Title = "Manager",
                NumberOfUnits = 3,
                Salary = 60000,
                Premium = 10000
            };

            Exception ex = Assert.CatchAsync(async () => await ps.EditAsync(item));

            StringAssert.Contains("Должность уже существует", ex.Message);
        }

        [Test]
        public override async Task EditAsync_CallsWithGoodParams_CallsUpdateMethodOnсe() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Posts.Update(It.IsAny<Post>()));
            mock.Setup(m => m.Employees.CountAsync(It.IsAny<Expression<Func<Employee, bool>>>())).ReturnsAsync(-1);
            PostService ps = GetNewService(mock.Object);
            PostDTO item = new PostDTO {
                Title = "Administrator",
                NumberOfUnits = 1,
                Salary = 50000,
                Premium = 10000
            };

            await ps.EditAsync(item);

            mock.Verify(m => m.Posts.Update(It.IsAny<Post>()), Times.Once());
        }

        [Test]
        public override async Task EditAsync_CallsWithGoodParams_CallsSaveAsyncMethodOnсe() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Posts.Update(It.IsAny<Post>()));
            mock.Setup(m => m.Employees.CountAsync(It.IsAny<Expression<Func<Employee, bool>>>())).ReturnsAsync(-1);
            PostService ps = GetNewService(mock.Object);
            PostDTO item = new PostDTO {
                Title = "Administrator",
                NumberOfUnits = 1,
                Salary = 50000,
                Premium = 10000
            };

            await ps.EditAsync(item);

            mock.Verify((m => m.SaveAsync()), Times.Once());
        }

        /// <summary>
        /// // FindByIdAsync method
        /// </summary>
        [Test]
        public override void FindByIdAsync_IdParameterIsNull_Throws() {
            PostService ps = GetNewService();

            Exception ex = Assert.CatchAsync(async () => await ps.FindByIdAsync(null));

            StringAssert.Contains("Не установлено id должности", ex.Message);
        }

        [Test]
        public void FindByIdAsync_PostNotFound_Throws() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Posts.FindByIdAsync(It.IsAny<int>())).Returns(Task.FromResult<Post>(null));
            PostService ps = GetNewService(mock.Object);

            Exception ex = Assert.CatchAsync(async () => await ps.FindByIdAsync(It.IsAny<int>()));

            StringAssert.Contains("Должность не найдена", ex.Message);
        }

        [Test]
        public override async Task FindByIdAsync_IdEqualTo2_ReturnsObjectWithIdEqualTo2() {
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
        public override async Task GetAllAsync_GetAsyncMethodReturnsArray_ReturnsSameArray() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Posts.GetAllAsync()).ReturnsAsync(() => new Post[] {
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
        public override async Task ExportJsonAsync_CreatesJsonFile() {
            string fullPath = "./DiplomMSSQLApp.WEB/Results/Posts.json";
            File.Delete(fullPath);
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Posts.GetAllAsync()).ReturnsAsync(new Post[] { });
            PostService postService = GetNewService(mock.Object);

            await postService.ExportJsonAsync(fullPath);

            Assert.IsTrue(File.Exists(fullPath));
        }
    }
}
