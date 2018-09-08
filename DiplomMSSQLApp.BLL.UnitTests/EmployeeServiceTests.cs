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

        /// <summary>
        /// // CreateAsync method
        /// </summary>
        [Test]
        public void CreateAsync_LastNamePropertyIsNull_Throws()
        {
            EmployeeService es = GetNewService();
            EmployeeDTO item = new EmployeeDTO {
                LastName = null
            };

            Exception ex = Assert.CatchAsync(async () => await es.CreateAsync(item));

            StringAssert.Contains("Требуется ввести фамилию", ex.Message);
        }

        [Test]
        public void CreateAsync_FirstNamePropertyIsNull_Throws()
        {
            EmployeeService es = GetNewService();
            EmployeeDTO item = new EmployeeDTO {
                LastName = "Petrov"
            };

            Exception ex = Assert.CatchAsync(async () => await es.CreateAsync(item));

            StringAssert.Contains("Требуется ввести имя", ex.Message);
        }

        [Test]
        public void CreateAsync_EmailPropertyIsNotCorrectFormat_Throws()
        {
            EmployeeService es = GetNewService();
            EmployeeDTO item = new EmployeeDTO {
                LastName = "Petrov",
                FirstName = "Max",
                Email = "this is a bad email"
            };

            Exception ex = Assert.CatchAsync(async () => await es.CreateAsync(item));

            StringAssert.Contains("Некорректный email", ex.Message);
        }

        [Test]
        public void CreateAsync_SalaryPropertyLessThanMinSalaryProperty_Throws()
        {
            EmployeeService es = GetNewService();
            EmployeeDTO item = new EmployeeDTO {
                LastName = "Petrov",
                FirstName = "Max",
                Salary = 10000,
                Post = new PostDTO() { MinSalary = 20000 }
            };

            Exception ex = Assert.CatchAsync(async () => await es.CreateAsync(item));

            StringAssert.Contains("Зарплата должна быть больше 20000", ex.Message);
        }

        [Test]
        public void CreateAsync_SalaryPropertyMoreThanMaxSalaryProperty_Throws()
        {
            EmployeeService es = GetNewService();
            EmployeeDTO item = new EmployeeDTO {
                LastName = "Petrov",
                FirstName = "Max",
                Salary = 30000,
                Post = new PostDTO() { MaxSalary = 20000 }
            };

            Exception ex = Assert.CatchAsync(async () => await es.CreateAsync(item));

            StringAssert.Contains("Зарплата должна быть меньше 20000", ex.Message);
        }

        [Test]
        public void CreateAsync_DepartmentPropertyIsNull_Throws()
        {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Departments.FindByIdAsync(It.IsAny<int>())).Returns(Task.FromResult<Department>(null));
            EmployeeService es = GetNewService(mock.Object);
            EmployeeDTO item = new EmployeeDTO {
                LastName = "Petrov",
                FirstName = "Max"
            };

            Exception ex = Assert.CatchAsync(async () => await es.CreateAsync(item));

            StringAssert.Contains("Отдел не найден", ex.Message);
        }

        [Test]
        public void CreateAsync_PostPropertyIsNull_Throws()
        {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Departments.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(new Department());
            mock.Setup(m => m.Posts.FindByIdAsync(It.IsAny<int>())).Returns(Task.FromResult<Post>(null));
            EmployeeService es = GetNewService(mock.Object);
            EmployeeDTO item = new EmployeeDTO {
                LastName = "Petrov",
                FirstName = "Max"
            };

            Exception ex = Assert.CatchAsync(async () => await es.CreateAsync(item));

            StringAssert.Contains("Должность не найдена", ex.Message);
        }

        [Test]
        public async Task CreateAsync_CallsWithGoodParams_CallsCreateMethodOnсe()
        {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Departments.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(new Department());
            mock.Setup(m => m.Posts.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(new Post());
            mock.Setup(m => m.Employees.Create(It.IsAny<Employee>()));
            EmployeeService es = GetNewService(mock.Object);
            EmployeeDTO item = new EmployeeDTO {
                LastName = "Petrov",
                FirstName = "Max"
            };

            await es.CreateAsync(item);

            mock.Verify(m => m.Employees.Create(It.IsAny<Employee>()), Times.Once());
        }

        [Test]
        public async Task CreateAsync_CallsWithGoodParams_CallsSaveAsyncMethodOnсe()
        {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Departments.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(new Department());
            mock.Setup(m => m.Posts.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(new Post());
            mock.Setup(m => m.Employees.Create(It.IsAny<Employee>()));
            EmployeeService es = GetNewService(mock.Object);
            EmployeeDTO item = new EmployeeDTO {
                LastName = "Petrov",
                FirstName = "Max"
            };

            await es.CreateAsync(item);

            mock.Verify((m => m.SaveAsync()), Times.Once());
        }

        /// <summary>
        /// // DeleteAsync method
        /// </summary>
        [Test]
        public override async Task DeleteAsync_FindByIdAsyncMethodReturnsNull_RemoveMethodIsNeverCalled()
        {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Employees.FindByIdAsync(It.IsAny<int>())).Returns(Task.FromResult<Employee>(null));
            EmployeeService es = GetNewService(mock.Object);

            await es.DeleteAsync(It.IsAny<int>());

            mock.Verify(m => m.Employees.Remove(It.IsAny<Employee>()), Times.Never);
        }

        [Test]
        public override async Task DeleteAsync_FindByIdAsyncMethodReturnsNull_SaveAsyncMethodIsNeverCalled()
        {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Employees.FindByIdAsync(It.IsAny<int>())).Returns(Task.FromResult<Employee>(null));
            EmployeeService es = GetNewService(mock.Object);

            await es.DeleteAsync(It.IsAny<int>());

            mock.Verify(m => m.SaveAsync(), Times.Never);
        }

        [Test]
        public override async Task DeleteAsync_FindByIdAsyncMethodReturnsObject_RemoveMethodIsCalledOnce()
        {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Employees.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(new Employee());
            EmployeeService es = GetNewService(mock.Object);

            await es.DeleteAsync(It.IsAny<int>());

            mock.Verify(m => m.Employees.Remove(It.IsAny<Employee>()), Times.Once);
        }

        [Test]
        public override async Task DeleteAsync_FindByIdAsyncMethodReturnsObject_SaveAsyncMethodIsCalledOnce()
        {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Employees.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(new Employee());
            EmployeeService es = GetNewService(mock.Object);

            await es.DeleteAsync(It.IsAny<int>());

            mock.Verify(m => m.SaveAsync(), Times.Once);
        }

        /// <summary>
        /// // DeleteAllAsync method
        /// </summary>
        public override Task DeleteAllAsync_Calls_RemoveAllAsyncMethodIsCalledOnce()
        {
            throw new NotImplementedException();
        }

        public override Task DeleteAllAsync_Calls_SaveAsyncMethodIsCalledOnce()
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
