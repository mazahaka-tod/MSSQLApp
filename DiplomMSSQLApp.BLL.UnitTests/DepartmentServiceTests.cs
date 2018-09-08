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

        /// <summary>
        /// // CreateAsync method
        /// </summary>
        [Test]
        public void CreateAsync_DepartmentNamePropertyIsNull_Throws()
        {
            DepartmentService ds = GetNewService();
            DepartmentDTO item = new DepartmentDTO {
                DepartmentName = null
            };

            Exception ex = Assert.CatchAsync(async () => await ds.CreateAsync(item));

            StringAssert.Contains("Требуется ввести название отдела", ex.Message);
        }

        [Test]
        public async Task CreateAsync_CallsWithGoodParameter_CallsCreateMethodOnсe()
        {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Departments.Create(It.IsAny<Department>()));
            DepartmentService ds = GetNewService(mock.Object);
            DepartmentDTO item = new DepartmentDTO {
                DepartmentName = "IT"
            };

            await ds.CreateAsync(item);

            mock.Verify(m => m.Departments.Create(It.IsAny<Department>()), Times.Once);
        }

        [Test]
        public async Task CreateAsync_CallsWithGoodParameter_CallsSaveAsyncMethodOnсe()
        {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Departments.Create(It.IsAny<Department>()));
            DepartmentService ds = GetNewService(mock.Object);
            DepartmentDTO item = new DepartmentDTO {
                DepartmentName = "IT"
            };

            await ds.CreateAsync(item);

            mock.Verify(m => m.SaveAsync(), Times.Once);
        }

        /// <summary>
        /// // DeleteAsync method
        /// </summary>
        [Test]
        public override async Task DeleteAsync_FindByIdAsyncMethodReturnsNull_RemoveMethodIsNeverCalled()
        {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Departments.FindByIdAsync(It.IsAny<int>())).Returns(Task.FromResult<Department>(null));
            DepartmentService ds = GetNewService(mock.Object);

            await ds.DeleteAsync(It.IsAny<int>());

            mock.Verify(m => m.Departments.Remove(It.IsAny<Department>()), Times.Never);
        }

        [Test]
        public override async Task DeleteAsync_FindByIdAsyncMethodReturnsNull_SaveAsyncMethodIsNeverCalled()
        {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Departments.FindByIdAsync(It.IsAny<int>())).Returns(Task.FromResult<Department>(null));
            DepartmentService ds = GetNewService(mock.Object);

            await ds.DeleteAsync(It.IsAny<int>());

            mock.Verify(m => m.SaveAsync(), Times.Never);
        }

        [Test]
        public override async Task DeleteAsync_FindByIdAsyncMethodReturnsObject_RemoveMethodIsCalledOnce()
        {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Departments.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(new Department());
            DepartmentService ds = GetNewService(mock.Object);

            await ds.DeleteAsync(It.IsAny<int>());

            mock.Verify(m => m.Departments.Remove(It.IsAny<Department>()), Times.Once);
        }

        [Test]
        public override async Task DeleteAsync_FindByIdAsyncMethodReturnsObject_SaveAsyncMethodIsCalledOnce()
        {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Departments.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(new Department());
            DepartmentService ds = GetNewService(mock.Object);

            await ds.DeleteAsync(It.IsAny<int>());

            mock.Verify(m => m.SaveAsync(), Times.Once);
        }

        /// <summary>
        /// // DeleteAllAsync method
        /// </summary>
        [Test]
        public override async Task DeleteAllAsync_Calls_RemoveAllAsyncMethodIsCalledOnce()
        {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Departments.RemoveAllAsync()).Returns(Task.CompletedTask);
            DepartmentService ds = GetNewService(mock.Object);

            await ds.DeleteAllAsync();

            mock.Verify(m => m.Departments.RemoveAllAsync(), Times.Once);
        }

        [Test]
        public override async Task DeleteAllAsync_Calls_SaveAsyncMethodIsCalledOnce()
        {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Departments.RemoveAllAsync()).Returns(Task.CompletedTask);
            DepartmentService ds = GetNewService(mock.Object);

            await ds.DeleteAllAsync();

            mock.Verify(m => m.SaveAsync(), Times.Once);
        }

        /// <summary>
        /// // EditAsync method
        /// </summary>
        [Test]
        public void EditAsync_DepartmentNamePropertyIsNull_Throws()
        {
            DepartmentService ds = GetNewService();
            DepartmentDTO item = new DepartmentDTO {
                DepartmentName = null
            };

            Exception ex = Assert.CatchAsync(async () => await ds.EditAsync(item));

            StringAssert.Contains("Требуется ввести название отдела", ex.Message);
        }

        [Test]
        public override async Task EditAsync_CallsWithGoodParams_CallsUpdateMethodOnсe()
        {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Departments.Update(It.IsAny<Department>()));
            DepartmentService ds = GetNewService(mock.Object);
            DepartmentDTO item = new DepartmentDTO {
                DepartmentName = "IT"
            };

            await ds.EditAsync(item);

            mock.Verify(m => m.Departments.Update(It.IsAny<Department>()), Times.Once);
        }

        [Test]
        public override async Task EditAsync_CallsWithGoodParams_CallsSaveAsyncMethodOnсe()
        {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Departments.Update(It.IsAny<Department>()));
            DepartmentService ds = GetNewService(mock.Object);
            DepartmentDTO item = new DepartmentDTO {
                DepartmentName = "IT"
            };

            await ds.EditAsync(item);

            mock.Verify(m => m.SaveAsync(), Times.Once);
        }

        /// <summary>
        /// // FindByIdAsync method
        /// </summary>
        [Test]
        public override void FindByIdAsync_IdParameterIsNull_Throws()
        {
            DepartmentService ds = GetNewService();

            Exception ex = Assert.CatchAsync(async () => await ds.FindByIdAsync(null));

            StringAssert.Contains("Не установлено id отдела", ex.Message);
        }

        [Test]
        public void FindByIdAsync_DepartmentNotFound_Throws()
        {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Departments.FindByIdAsync(It.IsAny<int>())).Returns(Task.FromResult<Department>(null));
            DepartmentService ds = GetNewService(mock.Object);

            Exception ex = Assert.CatchAsync(async () => await ds.FindByIdAsync(It.IsAny<int>()));

            StringAssert.Contains("Отдел не найден", ex.Message);
        }

        [Test]
        public override async Task FindByIdAsync_IdEqualTo2_ReturnsObjectWithIdEqualTo2()
        {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Departments.FindByIdAsync(It.IsAny<int>())).ReturnsAsync((int item_id) => new Department() { Id = item_id });
            DepartmentService ds = GetNewService(mock.Object);

            DepartmentDTO result = await ds.FindByIdAsync(2);

            Assert.AreEqual(2, result.Id);
        }
    }
}
