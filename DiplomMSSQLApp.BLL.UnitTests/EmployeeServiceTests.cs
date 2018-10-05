using DiplomMSSQLApp.BLL.BusinessModels;
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
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Posts.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(new Post { MinSalary = 20000 });
            EmployeeService es = GetNewService(mock.Object);
            EmployeeDTO item = new EmployeeDTO {
                LastName = "Petrov",
                FirstName = "Max",
                Salary = 10000
            };

            Exception ex = Assert.CatchAsync(async () => await es.CreateAsync(item));

            StringAssert.Contains("Зарплата должна быть больше 20000", ex.Message);
        }

        [Test]
        public void CreateAsync_SalaryPropertyMoreThanMaxSalaryProperty_Throws()
        {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Posts.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(new Post { MaxSalary = 20000 });
            EmployeeService es = GetNewService(mock.Object);
            EmployeeDTO item = new EmployeeDTO {
                LastName = "Petrov",
                FirstName = "Max",
                Salary = 30000
            };

            Exception ex = Assert.CatchAsync(async () => await es.CreateAsync(item));

            StringAssert.Contains("Зарплата должна быть меньше 20000", ex.Message);
        }

        [Test]
        public void CreateAsync_DepartmentPropertyIsNull_Throws()
        {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Posts.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(new Post());
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
        [Test]
        public override async Task DeleteAllAsync_Calls_RemoveAllAsyncMethodIsCalledOnce()
        {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Employees.RemoveAllAsync()).Returns(Task.CompletedTask);
            EmployeeService es = GetNewService(mock.Object);

            await es.DeleteAllAsync();

            mock.Verify(m => m.Employees.RemoveAllAsync(), Times.Once);
        }

        [Test]
        public override async Task DeleteAllAsync_Calls_SaveAsyncMethodIsCalledOnce()
        {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Employees.RemoveAllAsync()).Returns(Task.CompletedTask);
            EmployeeService es = GetNewService(mock.Object);

            await es.DeleteAllAsync();

            mock.Verify(m => m.SaveAsync(), Times.Once);
        }

        /// <summary>
        /// // EditAsync method
        /// </summary>
        [Test]
        public void EditAsync_LastNamePropertyIsNull_Throws()
        {
            EmployeeService es = GetNewService();
            EmployeeDTO item = new EmployeeDTO {
                LastName = null
            };

            Exception ex = Assert.CatchAsync(async () => await es.EditAsync(item));

            StringAssert.Contains("Требуется ввести фамилию", ex.Message);
        }

        [Test]
        public void EditAsync_FirstNamePropertyIsNull_Throws()
        {
            EmployeeService es = GetNewService();
            EmployeeDTO item = new EmployeeDTO {
                LastName = "Petrov"
            };

            Exception ex = Assert.CatchAsync(async () => await es.EditAsync(item));

            StringAssert.Contains("Требуется ввести имя", ex.Message);
        }

        [Test]
        public void EditAsync_EmailPropertyIsNotCorrectFormat_Throws()
        {
            EmployeeService es = GetNewService();
            EmployeeDTO item = new EmployeeDTO {
                LastName = "Petrov",
                FirstName = "Max",
                Email = "this is a bad email"
            };

            Exception ex = Assert.CatchAsync(async () => await es.EditAsync(item));

            StringAssert.Contains("Некорректный email", ex.Message);
        }

        [Test]
        public void EditAsync_SalaryPropertyLessThanMinSalaryProperty_Throws()
        {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Posts.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(new Post { MinSalary = 20000 });
            EmployeeService es = GetNewService(mock.Object);
            EmployeeDTO item = new EmployeeDTO {
                LastName = "Petrov",
                FirstName = "Max",
                Salary = 10000
            };

            Exception ex = Assert.CatchAsync(async () => await es.EditAsync(item));

            StringAssert.Contains("Зарплата должна быть больше 20000", ex.Message);
        }

        [Test]
        public void EditAsync_SalaryPropertyMoreThanMaxSalaryProperty_Throws()
        {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Posts.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(new Post { MaxSalary = 20000 });
            EmployeeService es = GetNewService(mock.Object);
            EmployeeDTO item = new EmployeeDTO {
                LastName = "Petrov",
                FirstName = "Max",
                Salary = 30000
            };

            Exception ex = Assert.CatchAsync(async () => await es.EditAsync(item));

            StringAssert.Contains("Зарплата должна быть меньше 20000", ex.Message);
        }

        [Test]
        public void EditAsync_DepartmentPropertyIsNull_Throws()
        {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Posts.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(new Post());
            mock.Setup(m => m.Departments.FindByIdAsync(It.IsAny<int>())).Returns(Task.FromResult<Department>(null));
            EmployeeService es = GetNewService(mock.Object);
            EmployeeDTO item = new EmployeeDTO {
                LastName = "Petrov",
                FirstName = "Max"
            };

            Exception ex = Assert.CatchAsync(async () => await es.EditAsync(item));

            StringAssert.Contains("Отдел не найден", ex.Message);
        }

        [Test]
        public void EditAsync_PostPropertyIsNull_Throws()
        {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Departments.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(new Department());
            mock.Setup(m => m.Posts.FindByIdAsync(It.IsAny<int>())).Returns(Task.FromResult<Post>(null));
            EmployeeService es = GetNewService(mock.Object);
            EmployeeDTO item = new EmployeeDTO {
                LastName = "Petrov",
                FirstName = "Max"
            };

            Exception ex = Assert.CatchAsync(async () => await es.EditAsync(item));

            StringAssert.Contains("Должность не найдена", ex.Message);
        }

        [Test]
        public override async Task EditAsync_CallsWithGoodParams_CallsUpdateMethodOnсe()
        {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Departments.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(new Department());
            mock.Setup(m => m.Posts.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(new Post());
            mock.Setup(m => m.Employees.Update(It.IsAny<Employee>()));
            EmployeeService es = GetNewService(mock.Object);
            EmployeeDTO item = new EmployeeDTO {
                LastName = "Petrov",
                FirstName = "Max"
            };

            await es.EditAsync(item);

            mock.Verify(m => m.Employees.Update(It.IsAny<Employee>()), Times.Once());
        }

        [Test]
        public override async Task EditAsync_CallsWithGoodParams_CallsSaveAsyncMethodOnсe()
        {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Departments.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(new Department());
            mock.Setup(m => m.Posts.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(new Post());
            mock.Setup(m => m.Employees.Update(It.IsAny<Employee>()));
            EmployeeService es = GetNewService(mock.Object);
            EmployeeDTO item = new EmployeeDTO {
                LastName = "Petrov",
                FirstName = "Max"
            };

            await es.EditAsync(item);

            mock.Verify((m => m.SaveAsync()), Times.Once());
        }

        /// <summary>
        /// // FindByIdAsync method
        /// </summary>
        [Test]
        public override void FindByIdAsync_IdParameterIsNull_Throws()
        {
            EmployeeService es = GetNewService();

            Exception ex = Assert.CatchAsync(async () => await es.FindByIdAsync(null));

            StringAssert.Contains("Не установлено id сотрудника", ex.Message);
        }

        [Test]
        public void FindByIdAsync_EmployeeNotFound_Throws()
        {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Employees.FindByIdAsync(It.IsAny<int>())).Returns(Task.FromResult<Employee>(null));
            EmployeeService es = GetNewService(mock.Object);

            Exception ex = Assert.CatchAsync(async () => await es.FindByIdAsync(It.IsAny<int>()));

            StringAssert.Contains("Сотрудник не найден", ex.Message);
        }

        [Test]
        public override async Task FindByIdAsync_IdEqualTo2_ReturnsObjectWithIdEqualTo2()
        {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Employees.FindByIdAsync(It.IsAny<int>())).ReturnsAsync((int item_id) => new Employee() { Id = item_id });
            EmployeeService es = GetNewService(mock.Object);

            EmployeeDTO result = await es.FindByIdAsync(2);

            Assert.AreEqual(2, result.Id);
        }

        /// <summary>
        /// // GetAllAsync method
        /// </summary>
        [Test]
        public override async Task GetAllAsync_GetAsyncMethodReturnsArray_ReturnsSameArray()
        {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Employees.GetAsync()).ReturnsAsync(() => new Employee[] {
                new Employee() { Id = 1, LastName = "Petrov" },
                new Employee() { Id = 2, LastName = "Popov" },
                new Employee() { Id = 3, LastName = "Ivanov" }
            });
            EmployeeService es = GetNewService(mock.Object);

            EmployeeDTO[] result = (await es.GetAllAsync()).ToArray();

            Assert.AreEqual(1, result[0].Id);
            Assert.AreEqual(2, result[1].Id);
            Assert.AreEqual(3, result[2].Id);
            Assert.AreEqual("Petrov", result[0].LastName);
            Assert.AreEqual("Popov", result[1].LastName);
            Assert.AreEqual("Ivanov", result[2].LastName);
        }

        /// <summary>
        /// // Get method
        /// </summary>
        [TestCase("P", false, null, false, null, null, null, null, false, null, null)]          // LastName
        [TestCase(null, false, "mail.ru", false, null, null, null, null, false, null, null)]    // Email
        [TestCase(null, false, null, true, null, null, null, null, false, null, null)]          // IsPhoneNumber
        [TestCase(null, false, null, false, "2018-09-01", null, null, null, false, null, null)] // HireDate
        [TestCase(null, false, null, false, null, null, 20000, null, false, null, null)]        // MaxSalary
        [TestCase(null, false, null, false, null, null, null, 0.1, false, null, null)]          // Bonus
        [TestCase(null, false, null, false, null, null, null, null, true, null, null)]          // IsBonus
        [TestCase(null, false, null, false, null, null, null, null, false, "Manager", null)]    // PostTitle
        [TestCase(null, false, null, false, null, null, null, null, false, null, "Management")] // DepartmentName
        public void Get_OneFilterParameterIsSet_ReturnsFilteredArray(string ln, bool im, string em, 
            bool ipn, string hd, double? min, double? max, double? b, bool ib, string pt, string dn)
        {
            EmployeeFilter filter = new EmployeeFilter {
                LastName = new string[] { ln, null, "" },
                IsMatchAnyLastName = im,
                Email = em,
                IsPhoneNumber = ipn,
                HireDate = hd,
                MinSalary = min,
                MaxSalary = max,
                Bonus = new double?[] { b },
                IsBonus = ib,
                PostTitle = pt,
                DepartmentName = dn
            };
            Employee[] employees = new Employee[] {
                new Employee() {
                    Id = 1,
                    LastName = "Petrov",
                    Email = "Petrov@mail.ru",
                    PhoneNumber = "89991554545",
                    HireDate = new DateTime(2018, 09, 01),
                    Salary = 13000,
                    Bonus = 0.1,
                    Post = new Post { Title = "Manager" },
                    Department = new Department { DepartmentName = "Management" }
                },
                new Employee() {
                    Id = 2,
                    LastName = "Panin",
                    Email = "Panin@mail.ru",
                    PhoneNumber = "89991554546",
                    HireDate = new DateTime(2018, 09, 01),
                    Salary = 17000,
                    Bonus = 0.1,
                    Post = new Post { Title = "Manager" },
                    Department = new Department { DepartmentName = "Management" }
                },
                new Employee() {
                    Id = 3,
                    LastName = "Ivanov",
                    Email = "Ivanov@yandex.ru",
                    HireDate = new DateTime(2018, 09, 02),
                    Salary = 21000,
                    Post = new Post { Title = "Intern" },
                    Department = new Department { DepartmentName = "IT" }
                },
                new Employee() {
                    Id = 4,
                    LastName = "Brown",
                    Email = "Brown@gmail.com",
                    HireDate = new DateTime(2018, 09, 03),
                    Salary = 23000,
                    Post = new Post { Title = "Engineer" },
                    Department = new Department { DepartmentName = "Logistics" }
                }
            };
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Employees.Get(It.IsAny<Func<Employee, bool>>()))
                .Returns((Func<Employee, bool> predicate) => employees.Where(predicate));
            EmployeeService employeeService = GetNewService(mock.Object);

            EmployeeDTO[] result = employeeService.Get(filter).ToArray();

            Assert.AreEqual(2, result.Length);
            Assert.AreEqual(2, result[0].Id);
            Assert.AreEqual(1, result[1].Id);
            Assert.AreEqual("Panin", result[0].LastName);
            Assert.AreEqual("Petrov", result[1].LastName);
        }

        [TestCase("P", false, null, false, null, null, null, null, false, null, null)]          // LastName
        [TestCase(null, false, "mail.ru", false, null, null, null, null, false, null, null)]    // Email
        [TestCase(null, false, null, true, null, null, null, null, false, null, null)]          // IsPhoneNumber
        [TestCase(null, false, null, false, "2018-09-01", null, null, null, false, null, null)] // HireDate
        [TestCase(null, false, null, false, null, 12000, null, null, false, null, null)]        // MinSalary
        [TestCase(null, false, null, false, null, null, null, 0.1, false, null, null)]          // Bonus
        [TestCase(null, false, null, false, null, null, null, null, true, null, null)]          // IsBonus
        [TestCase(null, false, null, false, null, null, null, null, false, "Manager", null)]    // PostTitle
        [TestCase(null, false, null, false, null, null, null, null, false, null, "Management")] // DepartmentName
        public void Get_OneFilterParameterAndIsAntiFilterIsSet_ReturnsFilteredArray(string ln, bool im, string em,
            bool ipn, string hd, double? min, double? max, double? b, bool ib, string pt, string dn)
        {
            EmployeeFilter filter = new EmployeeFilter {
                LastName = new string[] { ln, null, "" },
                IsMatchAnyLastName = im,
                Email = em,
                IsPhoneNumber = ipn,
                HireDate = hd,
                MinSalary = min,
                MaxSalary = max,
                Bonus = new double?[] { b },
                IsBonus = ib,
                PostTitle = pt,
                DepartmentName = dn,
                IsAntiFilter = true
            };
            Employee[] employees = new Employee[] {
                new Employee() {
                    Id = 1,
                    LastName = "Petrov",
                    Email = "Petrov@mail.ru",
                    PhoneNumber = "89991554545",
                    HireDate = new DateTime(2018, 09, 01),
                    Salary = 13000,
                    Bonus = 0.1,
                    Post = new Post { Title = "Manager" },
                    Department = new Department { DepartmentName = "Management" }
                },
                new Employee() {
                    Id = 2,
                    LastName = "Panin",
                    Email = "Panin@mail.ru",
                    PhoneNumber = "89991554546",
                    HireDate = new DateTime(2018, 09, 01),
                    Salary = 17000,
                    Bonus = 0.1,
                    Post = new Post { Title = "Manager" },
                    Department = new Department { DepartmentName = "Management" }
                },
                new Employee() {
                    Id = 3,
                    LastName = "Ivanov",
                    Email = "Ivanov@yandex.ru",
                    HireDate = new DateTime(2018, 09, 02),
                    Salary = 11000,
                    Post = new Post { Title = "Intern" },
                    Department = new Department { DepartmentName = "IT" }
                },
                new Employee() {
                    Id = 4,
                    LastName = "Brown",
                    Email = "Brown@gmail.com",
                    HireDate = new DateTime(2018, 09, 03),
                    Salary = 9000,
                    Post = new Post { Title = "Engineer" },
                    Department = new Department { DepartmentName = "Logistics" }
                }
            };
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Employees.Get(It.IsAny<Func<Employee, bool>>()))
                .Returns((Func<Employee, bool> predicate) => employees.Where(predicate));
            EmployeeService employeeService = GetNewService(mock.Object);

            EmployeeDTO[] result = employeeService.Get(filter).ToArray();

            Assert.AreEqual(2, result.Length);
            Assert.AreEqual(4, result[0].Id);
            Assert.AreEqual(3, result[1].Id);
            Assert.AreEqual("Brown", result[0].LastName);
            Assert.AreEqual("Ivanov", result[1].LastName);
        }

        [TestCase("P", "n",   false, null, false, null, null, null, null, false, null, null)]           // LastName1, LastName2
        [TestCase("P", null, false, "mail.ru", false, null, null, null, null, false, null, null)]       // LastName, Email
        [TestCase(null, null, false, "mail.ru", true, null, null, null, null, false, null, null)]       // Email, IsPhoneNumber
        [TestCase(null, null, false, null, true, "2018-09-01", null, null, null, false, null, null)]    // IsPhoneNumber, HireDate
        [TestCase(null, null, false, null, false, "2018-09-01", 12000, null, null, false, null, null)]  // HireDate, MinSalary
        [TestCase(null, null, false, null, false, null, 12000, 15000, null, false, null, null)]         // MinSalary, MaxSalary
        [TestCase(null, null, false, null, false, null, null, 15000, 0.1, false, null, null)]           // MaxSalary, Bonus
        [TestCase(null, null, false, null, false, null, null, null, null, true, "Manager", null)]       // IsBonus, PostTitle
        [TestCase(null, null, false, null, false, null, null, null, null, true, null, "Management")]    // IsBonus, DepartmentName
        public void Get_TwoFilterParametersAreSet_ReturnsFilteredArray(string ln1, string ln2, bool im, string em,
            bool ipn, string hd, double? min, double? max, double? b, bool ib, string pt, string dn)
        {
            EmployeeFilter filter = new EmployeeFilter {
                LastName = new string[] { ln1, ln2, null, "" },
                IsMatchAnyLastName = im,
                Email = em,
                IsPhoneNumber = ipn,
                HireDate = hd,
                MinSalary = min,
                MaxSalary = max,
                Bonus = new double?[] { b },
                IsBonus = ib,
                PostTitle = pt,
                DepartmentName = dn
            };
            Employee[] employees = new Employee[] {
                new Employee() {
                    Id = 1,
                    LastName = "Petrov",
                    Email = "Petrov@yandex.ru",
                    PhoneNumber = "89991554545",
                    HireDate = new DateTime(2018, 09, 02),
                    Salary = 17000,
                    Bonus = 0.1,
                    Post = new Post { Title = "Intern" },
                    Department = new Department { DepartmentName = "IT" }
                },
                new Employee() {
                    Id = 2,
                    LastName = "Panin",
                    Email = "Panin@mail.ru",
                    PhoneNumber = "89991554546",
                    HireDate = new DateTime(2018, 09, 01),
                    Salary = 13000,
                    Bonus = 0.1,
                    Post = new Post { Title = "Manager" },
                    Department = new Department { DepartmentName = "Management" }
                },
                new Employee() {
                    Id = 3,
                    LastName = "Ivanov",
                    Email = "Ivanov@mail.ru",
                    HireDate = new DateTime(2018, 09, 01),
                    Salary = 11000,
                    Post = new Post { Title = "Manager" },
                    Department = new Department { DepartmentName = "Management" }
                },
                new Employee() {
                    Id = 4,
                    LastName = "Sidorov",
                    Email = "Sidorov@gmail.com",
                    HireDate = new DateTime(2018, 09, 03),
                    Salary = 9000,
                    Post = new Post { Title = "Engineer" },
                    Department = new Department { DepartmentName = "Logistics" }
                }
            };
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Employees.Get(It.IsAny<Func<Employee, bool>>()))
                .Returns((Func<Employee, bool> predicate) => employees.Where(predicate));
            EmployeeService employeeService = GetNewService(mock.Object);

            EmployeeDTO[] result = employeeService.Get(filter).ToArray();

            Assert.AreEqual(1, result.Length);
            Assert.AreEqual(2, result[0].Id);
            Assert.AreEqual("Panin", result[0].LastName);
        }

        [TestCase("P", "n", false, null, false, null, null, null, null, false, null, null)]             // LastName1, LastName2
        [TestCase("P", null, false, "mail.ru", false, null, null, null, null, false, null, null)]       // LastName, Email
        [TestCase(null, null, false, "mail.ru", true, null, null, null, null, false, null, null)]       // Email, IsPhoneNumber
        [TestCase(null, null, false, null, true, "2018-09-01", null, null, null, false, null, null)]    // IsPhoneNumber, HireDate
        [TestCase(null, null, false, null, false, "2018-09-01", 12000, null, null, false, null, null)]  // HireDate, MinSalary
        [TestCase(null, null, false, null, false, null, 12000, 15000, null, false, null, null)]         // MinSalary, MaxSalary
        [TestCase(null, null, false, null, false, null, null, 15000, 0.1, false, null, null)]           // MaxSalary, Bonus
        [TestCase(null, null, false, null, false, null, null, null, null, true, "Manager", null)]       // IsBonus, PostTitle
        [TestCase(null, null, false, null, false, null, null, null, null, true, null, "Management")]    // IsBonus, DepartmentName
        public void Get_TwoFilterParametersAndIsAntiFilterAreSet_ReturnsFilteredArray(string ln1, string ln2, bool im, string em,
            bool ipn, string hd, double? min, double? max, double? b, bool ib, string pt, string dn)
        {
            EmployeeFilter filter = new EmployeeFilter {
                LastName = new string[] { ln1, ln2, null, "" },
                IsMatchAnyLastName = im,
                Email = em,
                IsPhoneNumber = ipn,
                HireDate = hd,
                MinSalary = min,
                MaxSalary = max,
                Bonus = new double?[] { b },
                IsBonus = ib,
                PostTitle = pt,
                DepartmentName = dn,
                IsAntiFilter = true
            };
            Employee[] employees = new Employee[] {
                new Employee() {
                    Id = 1,
                    LastName = "Petrov",
                    Email = "Petrov@yandex.ru",
                    PhoneNumber = "89991554545",
                    HireDate = new DateTime(2018, 09, 02),
                    Salary = 17000,
                    Bonus = 0.1,
                    Post = new Post { Title = "Intern" },
                    Department = new Department { DepartmentName = "IT" }
                },
                new Employee() {
                    Id = 2,
                    LastName = "Panin",
                    Email = "Panin@mail.ru",
                    PhoneNumber = "89991554546",
                    HireDate = new DateTime(2018, 09, 01),
                    Salary = 13000,
                    Bonus = 0.1,
                    Post = new Post { Title = "Manager" },
                    Department = new Department { DepartmentName = "Management" }
                },
                new Employee() {
                    Id = 3,
                    LastName = "Ivanov",
                    Email = "Ivanov@mail.ru",
                    HireDate = new DateTime(2018, 09, 01),
                    Salary = 11000,
                    Post = new Post { Title = "Manager" },
                    Department = new Department { DepartmentName = "Management" }
                },
                new Employee() {
                    Id = 4,
                    LastName = "Sidorov",
                    Email = "Sidorov@gmail.com",
                    HireDate = new DateTime(2018, 09, 03),
                    Salary = 9000,
                    Post = new Post { Title = "Engineer" },
                    Department = new Department { DepartmentName = "Logistics" }
                }
            };
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Employees.Get(It.IsAny<Func<Employee, bool>>()))
                .Returns((Func<Employee, bool> predicate) => employees.Where(predicate));
            EmployeeService employeeService = GetNewService(mock.Object);

            EmployeeDTO[] result = employeeService.Get(filter).ToArray();

            Assert.AreEqual(3, result.Length);
            Assert.AreEqual(3, result[0].Id);
            Assert.AreEqual(1, result[1].Id);
            Assert.AreEqual(4, result[2].Id);
            Assert.AreEqual("Ivanov", result[0].LastName);
            Assert.AreEqual("Petrov", result[1].LastName);
            Assert.AreEqual("Sidorov", result[2].LastName);
        }

        [TestCase(null)]
        [TestCase("LastName")]
        [TestCase("Email")]
        [TestCase("HireDate")]
        [TestCase("Salary")]
        [TestCase("Bonus")]
        [TestCase("PostTitle")]
        [TestCase("DepartmentName")]
        public void Get_AscSortIsSet_ReturnsSortedArray(string field)
        {
            EmployeeFilter filter = new EmployeeFilter {
                SortField = field,
                SortOrder = "Asc"
            };
            Employee[] employees = new Employee[] {
                new Employee() {
                    Id = 1,
                    LastName = "Sidorov",
                    Email = "Sidorov@yandex.ru",
                    PhoneNumber = "89991554547",
                    HireDate = new DateTime(2018, 09, 05),
                    Salary = 17000,
                    Bonus = 0.3,
                    Post = new Post { Title = "Manager" },
                    Department = new Department { DepartmentName = "Management" }
                },
                new Employee() {
                    Id = 2,
                    LastName = "Petrov",
                    Email = "Petrov@mail.ru",
                    PhoneNumber = "89991554546",
                    HireDate = new DateTime(2018, 09, 04),
                    Salary = 16000,
                    Bonus = 0.2,
                    Post = new Post { Title = "Intern" },
                    Department = new Department { DepartmentName = "Logistics" }
                },
                new Employee() {
                    Id = 3,
                    LastName = "Panin",
                    Email = "Panin@mail.ru",
                    PhoneNumber = "89991554545",
                    HireDate = new DateTime(2018, 09, 03),
                    Salary = 15000,
                    Bonus = 0.1,
                    Post = new Post { Title = "Engineer" },
                    Department = new Department { DepartmentName = "IT" }
                },
                new Employee() {
                    Id = 4,
                    LastName = "Ivanov",
                    PhoneNumber = "89991554544",
                    HireDate = new DateTime(2018, 09, 02),
                    Post = new Post { Title = "Administrator" },
                    Department = new Department { DepartmentName = "HR" }
                }
            };
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Employees.Get(It.IsAny<Func<Employee, bool>>()))
                .Returns((Func<Employee, bool> predicate) => employees.Where(predicate));
            EmployeeService employeeService = GetNewService(mock.Object);
            employeeService.PathToFileForTests = "./DiplomMSSQLApp.WEB/Results/Employee/Filter.txt";

            EmployeeDTO[] result = employeeService.Get(filter).ToArray();

            Assert.AreEqual(4, result.Length);
            Assert.AreEqual(4, result[0].Id);
            Assert.AreEqual(3, result[1].Id);
            Assert.AreEqual(2, result[2].Id);
            Assert.AreEqual(1, result[3].Id);
            Assert.AreEqual("Ivanov", result[0].LastName);
            Assert.AreEqual("Panin", result[1].LastName);
            Assert.AreEqual("Petrov", result[2].LastName);
            Assert.AreEqual("Sidorov", result[3].LastName);
        }

        [TestCase("LastName")]
        [TestCase("Email")]
        [TestCase("HireDate")]
        [TestCase("Salary")]
        [TestCase("Bonus")]
        [TestCase("PostTitle")]
        [TestCase("DepartmentName")]
        public void Get_DescSortIsSet_ReturnsSortedArray(string field)
        {
            EmployeeFilter filter = new EmployeeFilter {
                SortField = field,
                SortOrder = "Desc"
            };
            Employee[] employees = new Employee[] {
                new Employee() {
                    Id = 1,
                    LastName = "Sidorov",
                    Email = "Sidorov@yandex.ru",
                    PhoneNumber = "89991554547",
                    HireDate = new DateTime(2018, 09, 05),
                    Salary = 17000,
                    Bonus = 0.3,
                    Post = new Post { Title = "Manager" },
                    Department = new Department { DepartmentName = "Management" }
                },
                new Employee() {
                    Id = 2,
                    LastName = "Petrov",
                    Email = "Petrov@mail.ru",
                    PhoneNumber = "89991554546",
                    HireDate = new DateTime(2018, 09, 04),
                    Salary = 16000,
                    Bonus = 0.2,
                    Post = new Post { Title = "Intern" },
                    Department = new Department { DepartmentName = "Logistics" }
                },
                new Employee() {
                    Id = 3,
                    LastName = "Panin",
                    Email = "Panin@mail.ru",
                    PhoneNumber = "89991554545",
                    HireDate = new DateTime(2018, 09, 03),
                    Salary = 15000,
                    Bonus = 0.1,
                    Post = new Post { Title = "Engineer" },
                    Department = new Department { DepartmentName = "IT" }
                },
                new Employee() {
                    Id = 4,
                    LastName = "Ivanov",
                    PhoneNumber = "89991554544",
                    HireDate = new DateTime(2018, 09, 02),
                    Post = new Post { Title = "Administrator" },
                    Department = new Department { DepartmentName = "HR" }
                }
            };
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Employees.Get(It.IsAny<Func<Employee, bool>>()))
                .Returns((Func<Employee, bool> predicate) => employees.Where(predicate));
            EmployeeService employeeService = GetNewService(mock.Object);
            employeeService.PathToFileForTests = "./DiplomMSSQLApp.WEB/Results/Employee/Filter.txt";

            EmployeeDTO[] result = employeeService.Get(filter).ToArray();

            Assert.AreEqual(4, result.Length);
            Assert.AreEqual(1, result[0].Id);
            Assert.AreEqual(2, result[1].Id);
            Assert.AreEqual(3, result[2].Id);
            Assert.AreEqual(4, result[3].Id);
            Assert.AreEqual("Sidorov", result[0].LastName);
            Assert.AreEqual("Petrov", result[1].LastName);
            Assert.AreEqual("Panin", result[2].LastName);
            Assert.AreEqual("Ivanov", result[3].LastName);
        }

        [Test]
        public void Get_SortFieldPropertyIsEqualToLastName_CallsGetMethodOnce() {
            EmployeeFilter filter = new EmployeeFilter { SortField = "LastName" };
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Employees.Get(It.IsAny<Func<Employee, bool>>())).Returns(new Employee[] { });
            EmployeeService employeeService = GetNewService(mock.Object);
            employeeService.PathToFileForTests = "./DiplomMSSQLApp.WEB/Results/Employee/Filter.txt";

            employeeService.Get(filter);

            mock.Verify(m => m.Employees.Get(It.IsAny<Func<Employee, bool>>()), Times.Once());
        }

        [Test]
        public void Get_SortFieldPropertyIsEqualToLastName_CreatesResultFile() {
            EmployeeFilter filter = new EmployeeFilter { SortField = "LastName" };
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Employees.Get(It.IsAny<Func<Employee, bool>>())).Returns(new Employee[] { });
            EmployeeService employeeService = GetNewService(mock.Object);
            employeeService.PathToFileForTests = "./DiplomMSSQLApp.WEB/Results/Employee/Filter.txt";

            employeeService.Get(filter);

            Assert.IsTrue(File.Exists(employeeService.PathToFileForTests));
        }

        [Test]
        public void Get_SortFieldPropertyIsEqualToLastName_TestTimeIsWrittenToElapsedTimeProperty() {
            EmployeeFilter filter = new EmployeeFilter { SortField = "LastName" };
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Employees.Get(It.IsAny<Func<Employee, bool>>())).Returns(new Employee[] { });
            EmployeeService employeeService = GetNewService(mock.Object);
            employeeService.PathToFileForTests = "./DiplomMSSQLApp.WEB/Results/Employee/Filter.txt";

            employeeService.Get(filter);

            Assert.IsTrue(Regex.IsMatch(employeeService.ElapsedTime, @"^\d{2}:\d{2}:\d{2}\.\d{3}$"));
        }

        [Test]
        public void Get_AllFilterParameterIsSet_CreatesMessageAboutFilterParametersUsedProperty() {
            EmployeeFilter filter = new EmployeeFilter {
                LastName = new string[] { "W", null, "", "Z" },
                Email = "mail.ru",
                IsPhoneNumber = true,
                HireDate = "2018-09-01",
                MinSalary = 20000,
                MaxSalary = 50000,
                Bonus = new double?[] { 0.1, null, 0.2 },
                IsBonus = true,
                PostTitle = "Manager",
                DepartmentName = "Management",
                IsAntiFilter = true
            };
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Employees.Get(It.IsAny<Func<Employee, bool>>())).Returns(new Employee[] { });
            EmployeeService employeeService = GetNewService(mock.Object);

            employeeService.Get(filter);

            Assert.AreEqual("Фамилия = W; Фамилия = Z; Email = mail.ru; Есть телефон; Дата приема на работу = 2018-09-01; " +
                "Зарплата >= 20000; Зарплата <= 50000; Премия = 0,1; Премия = 0,2; Есть премия; Должность = Manager; " +
                "Название отдела = Management; Используется отрицание; ", employeeService.MessageAboutFilterParametersUsed);
        }

        [Test]
        public void Get_NoneFilterParameterIsSet_CreatesMessageAboutFilterParametersUsedProperty() {
            EmployeeFilter filter = new EmployeeFilter { };
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Employees.Get(It.IsAny<Func<Employee, bool>>())).Returns(new Employee[] { });
            EmployeeService employeeService = GetNewService(mock.Object);

            employeeService.Get(filter);

            Assert.AreEqual("Фильтр не задан; ", employeeService.MessageAboutFilterParametersUsed);
        }

        /// <summary>
        /// // ExportJsonAsync method
        /// </summary>
        [Test]
        public async Task ExportJsonAsync_CreatesJsonFile() {
            string fullPath = "./DiplomMSSQLApp.WEB/Results/Employee/Employees.json";
            File.Delete(fullPath);
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Employees.GetAsync()).ReturnsAsync(new Employee[] { });
            EmployeeService employeeService = GetNewService(mock.Object);

            await employeeService.ExportJsonAsync(fullPath);

            Assert.IsTrue(File.Exists(fullPath));
        }

        /// <summary>
        /// // TestCreateAsync method
        /// </summary>
        [Test]
        public async Task TestCreateAsync_CallsWithGoodParameter_CallsCreateMethodOnce() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Posts.GetFirst());
            mock.Setup(m => m.Departments.GetFirst());
            mock.Setup(m => m.Employees.Create(It.IsAny<IEnumerable<Employee>>()));
            EmployeeService employeeService = GetNewService(mock.Object);
            employeeService.PathToFileForTests = "./DiplomMSSQLApp.WEB/Results/Employee/Create.txt";

            await employeeService.TestCreateAsync(1);

            mock.Verify(m => m.Employees.Create(It.IsAny<IEnumerable<Employee>>()), Times.Once());
        }

        [Test]
        public async Task TestCreateAsync_CallsWithGoodParameter_CallsSaveAsyncMethodOnce() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Posts.GetFirst());
            mock.Setup(m => m.Departments.GetFirst());
            mock.Setup(m => m.Employees.Create(It.IsAny<IEnumerable<Employee>>()));
            EmployeeService employeeService = GetNewService(mock.Object);
            employeeService.PathToFileForTests = "./DiplomMSSQLApp.WEB/Results/Employee/Create.txt";

            await employeeService.TestCreateAsync(1);

            mock.Verify((m => m.SaveAsync()), Times.Once());
        }

        [Test]
        public async Task TestCreateAsync_CallsWithGoodParameter_CreatesResultFile() {
            string fullPath = "./DiplomMSSQLApp.WEB/Results/Employee/Create.txt";
            File.Delete(fullPath);
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Posts.GetFirst());
            mock.Setup(m => m.Departments.GetFirst());
            mock.Setup(m => m.Employees.Create(It.IsAny<IEnumerable<Employee>>()));
            EmployeeService employeeService = GetNewService(mock.Object);
            employeeService.PathToFileForTests = fullPath;

            await employeeService.TestCreateAsync(1);

            Assert.IsTrue(File.Exists(fullPath));
        }

        [Test]
        public async Task TestCreateAsync_CallsWithGoodParameter_TestTimeIsWrittenToElapsedTimeProperty() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Posts.GetFirst());
            mock.Setup(m => m.Departments.GetFirst());
            mock.Setup(m => m.Employees.Create(It.IsAny<IEnumerable<Employee>>()));
            EmployeeService employeeService = GetNewService(mock.Object);
            employeeService.PathToFileForTests = "./DiplomMSSQLApp.WEB/Results/Employee/Create.txt";

            await employeeService.TestCreateAsync(1);

            Assert.IsTrue(Regex.IsMatch(employeeService.ElapsedTime, @"^\d{2}:\d{2}:\d{2}\.\d{3}$"));
        }

        /// <summary>
        /// // TestReadAsync method
        /// </summary>
        [Test]
        public async Task TestReadAsync_ParameterIsThree_CallsGetMethodFourTimes() {
            int num = 3;
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Employees.GetAsync()).ReturnsAsync(new Employee[] { });
            mock.Setup(m => m.Employees.Get(It.IsAny<int>())).Returns(new Employee[] { });
            EmployeeService employeeService = GetNewService(mock.Object);
            employeeService.PathToFileForTests = "./DiplomMSSQLApp.WEB/Results/Employee/Read.txt";

            await employeeService.TestReadAsync(num, 0);

            mock.Verify(m => m.Employees.Get(It.IsAny<int>()), Times.Exactly(4));
        }

        [Test]
        public async Task TestReadAsync_CallsWithGoodParameter_CreatesResultFile() {
            string fullPath = "./DiplomMSSQLApp.WEB/Results/Employee/Read.txt";
            File.Delete(fullPath);
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Employees.GetAsync()).ReturnsAsync(new Employee[] { });
            mock.Setup(m => m.Employees.Get(It.IsAny<int>())).Returns(new Employee[] { });
            EmployeeService employeeService = GetNewService(mock.Object);
            employeeService.PathToFileForTests = fullPath;

            await employeeService.TestReadAsync(1, 0);

            Assert.IsTrue(File.Exists(fullPath));
        }

        [Test]
        public async Task TestReadAsync_CallsWithGoodParameter_TestTimeIsWrittenToElapsedTimeProperty() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Employees.GetAsync()).ReturnsAsync(new Employee[] { });
            mock.Setup(m => m.Employees.Get(It.IsAny<int>())).Returns(new Employee[] { });
            EmployeeService employeeService = GetNewService(mock.Object);
            employeeService.PathToFileForTests = "./DiplomMSSQLApp.WEB/Results/Employee/Read.txt";

            await employeeService.TestReadAsync(1, 0);

            Assert.IsTrue(Regex.IsMatch(employeeService.ElapsedTime, @"^\d{2}:\d{2}:\d{2}\.\d{3}$"));
        }

        /// <summary>
        /// // TestUpdateAsync method
        /// </summary>
        [Test]
        public async Task TestUpdateAsync_ParameterIsOne_CallsUpdateMethodOnce() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Employees.GetAsync()).ReturnsAsync(new Employee[] { new Employee(), new Employee() });
            EmployeeService employeeService = GetNewService(mock.Object);
            employeeService.PathToFileForTests = "./DiplomMSSQLApp.WEB/Results/Employee/Update.txt";

            await employeeService.TestUpdateAsync(1);

            mock.Verify(m => m.Employees.Update(It.IsAny<Employee>()), Times.Once());
        }

        [Test]
        public async Task TestUpdateAsync_ParameterIsOne_CallsSaveAsyncMethodOnce() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Employees.GetAsync()).ReturnsAsync(new Employee[] { new Employee(), new Employee() });
            EmployeeService employeeService = GetNewService(mock.Object);
            employeeService.PathToFileForTests = "./DiplomMSSQLApp.WEB/Results/Employee/Update.txt";

            await employeeService.TestUpdateAsync(1);

            mock.Verify((m => m.SaveAsync()), Times.Once());
        }

        [Test]
        public async Task TestUpdateAsync_CallsWithGoodParameter_CreatesResultFile() {
            string fullPath = "./DiplomMSSQLApp.WEB/Results/Employee/Update.txt";
            File.Delete(fullPath);
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Employees.GetAsync()).ReturnsAsync(new Employee[] { });
            EmployeeService employeeService = GetNewService(mock.Object);
            employeeService.PathToFileForTests = fullPath;

            await employeeService.TestUpdateAsync(1);

            Assert.IsTrue(File.Exists(fullPath));
        }

        [Test]
        public async Task TestUpdateAsync_CallsWithGoodParameter_TestTimeIsWrittenToElapsedTimeProperty() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Employees.GetAsync()).ReturnsAsync(new Employee[] { });
            EmployeeService employeeService = GetNewService(mock.Object);
            employeeService.PathToFileForTests = "./DiplomMSSQLApp.WEB/Results/Employee/Update.txt";

            await employeeService.TestUpdateAsync(1);

            Assert.IsTrue(Regex.IsMatch(employeeService.ElapsedTime, @"^\d{2}:\d{2}:\d{2}\.\d{3}$"));
        }

        /// <summary>
        /// // TestDeleteAsync method
        /// </summary>
        [Test]
        public async Task TestDeleteAsync_ParameterIsOne_CallsUpdateMethodOnce() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Employees.GetAsync()).ReturnsAsync(new Employee[] { });
            EmployeeService employeeService = GetNewService(mock.Object);
            employeeService.PathToFileForTests = "./DiplomMSSQLApp.WEB/Results/Employee/Delete.txt";

            await employeeService.TestDeleteAsync(1);

            mock.Verify(m => m.Employees.RemoveSeries(It.IsAny<IEnumerable<Employee>>()), Times.Once());
        }

        [Test]
        public async Task TestDeleteAsync_ParameterIsOne_CallsSaveAsyncMethodOnce() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Employees.GetAsync()).ReturnsAsync(new Employee[] { });
            EmployeeService employeeService = GetNewService(mock.Object);
            employeeService.PathToFileForTests = "./DiplomMSSQLApp.WEB/Results/Employee/Delete.txt";

            await employeeService.TestDeleteAsync(1);

            mock.Verify((m => m.SaveAsync()), Times.Once());
        }

        [Test]
        public async Task TestDeleteAsync_CallsWithGoodParameter_CreatesResultFile() {
            string fullPath = "./DiplomMSSQLApp.WEB/Results/Employee/Delete.txt";
            File.Delete(fullPath);
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Employees.GetAsync()).ReturnsAsync(new Employee[] { });
            EmployeeService employeeService = GetNewService(mock.Object);
            employeeService.PathToFileForTests = fullPath;

            await employeeService.TestDeleteAsync(1);

            Assert.IsTrue(File.Exists(fullPath));
        }

        [Test]
        public async Task TestDeleteAsync_CallsWithGoodParameter_TestTimeIsWrittenToElapsedTimeProperty() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Employees.GetAsync()).ReturnsAsync(new Employee[] { });
            EmployeeService employeeService = GetNewService(mock.Object);
            employeeService.PathToFileForTests = "./DiplomMSSQLApp.WEB/Results/Employee/Delete.txt";

            await employeeService.TestDeleteAsync(1);

            Assert.IsTrue(Regex.IsMatch(employeeService.ElapsedTime, @"^\d{2}:\d{2}:\d{2}\.\d{3}$"));
        }
    }
}
