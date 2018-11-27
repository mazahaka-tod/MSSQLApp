using DiplomMSSQLApp.BLL.DTO;
using DiplomMSSQLApp.BLL.Services;
using DiplomMSSQLApp.DAL.Entities;
using DiplomMSSQLApp.DAL.Interfaces;
using Moq;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DiplomMSSQLApp.BLL.UnitTests {
    [TestFixture]
    public class OrganizationServiceTests {
        protected OrganizationService GetNewService() {
            return new OrganizationService();
        }

        protected OrganizationService GetNewService(IUnitOfWork uow) {
            return new OrganizationService(uow);
        }

        /// <summary>
        /// // EditAsync method
        /// </summary>
        [Test]
        public void EditAsync_NamePropertyIsNull_Throws() {
            OrganizationService os = GetNewService();
            OrganizationDTO item = new OrganizationDTO {
                Name = null
            };

            Exception ex = Assert.CatchAsync(async () => await os.EditAsync(item));

            StringAssert.Contains("Требуется ввести наименование организации", ex.Message);
        }

        [Test]
        public async Task EditAsync_CallsWithGoodParams_CallsUpdateMethodOnсe() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Organizations.Update(It.IsAny<Organization>()));
            OrganizationService os = GetNewService(mock.Object);
            OrganizationDTO item = new OrganizationDTO {
                Name = "GazProm"
            };

            await os.EditAsync(item);

            mock.Verify(m => m.Organizations.Update(It.IsAny<Organization>()), Times.Once);
        }

        [Test]
        public async Task EditAsync_CallsWithGoodParams_CallsSaveAsyncMethodOnсe() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Organizations.Update(It.IsAny<Organization>()));
            OrganizationService os = GetNewService(mock.Object);
            OrganizationDTO item = new OrganizationDTO {
                Name = "GazProm"
            };

            await os.EditAsync(item);

            mock.Verify(m => m.SaveAsync(), Times.Once);
        }

        /// <summary>
        /// // FindByIdAsync method
        /// </summary>
        [Test]
        public void FindByIdAsync_IdParameterIsNull_Throws() {
            OrganizationService os = GetNewService();

            Exception ex = Assert.CatchAsync(async () => await os.FindByIdAsync(null));

            StringAssert.Contains("Не установлен id организации", ex.Message);
        }

        [Test]
        public void FindByIdAsync_OrganizationNotFound_Throws() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Organizations.FindByIdAsync(It.IsAny<int>())).Returns(Task.FromResult<Organization>(null));
            OrganizationService os = GetNewService(mock.Object);

            Exception ex = Assert.CatchAsync(async () => await os.FindByIdAsync(It.IsAny<int>()));

            StringAssert.Contains("Организация не найдена", ex.Message);
        }

        [Test]
        public async Task FindByIdAsync_IdEqualTo2_ReturnsObjectWithIdEqualTo2() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Organizations.FindByIdAsync(It.IsAny<int>())).ReturnsAsync((int item_id) => new Organization() { Id = item_id });
            OrganizationService os = GetNewService(mock.Object);

            OrganizationDTO result = await os.FindByIdAsync(2);

            Assert.AreEqual(2, result.Id);
        }

        /// <summary>
        /// // GetAllAsync method
        /// </summary>
        [Test]
        public async Task GetAllAsync_GetAsyncMethodReturnsArray_ReturnsSameArray() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Organizations.GetAllAsync()).ReturnsAsync(() => new Organization[] {
                new Organization() { Id = 1, Name = "Gazprom" },
                new Organization() { Id = 2, Name = "MTS" },
                new Organization() { Id = 3, Name = "Nike" }
            });
            OrganizationService os = GetNewService(mock.Object);

            OrganizationDTO[] result = (await os.GetAllAsync()).ToArray();

            Assert.AreEqual(1, result[0].Id);
            Assert.AreEqual(2, result[1].Id);
            Assert.AreEqual(3, result[2].Id);
            Assert.AreEqual("Gazprom", result[0].Name);
            Assert.AreEqual("MTS", result[1].Name);
            Assert.AreEqual("Nike", result[2].Name);
        }

        /// <summary>
        /// // GetFirstAsync method
        /// </summary>
        [Test]
        public async Task GetFirstAsync_GetFirstAsyncMethodReturnsOrganization_ReturnsSameOrganization() {
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Organizations.GetFirstAsync()).ReturnsAsync(() => new Organization() {
                Id = 1,
                Name = "Gazprom"
            });
            OrganizationService os = GetNewService(mock.Object);

            OrganizationDTO result = await os.GetFirstAsync();

            Assert.AreEqual(1, result.Id);
            Assert.AreEqual("Gazprom", result.Name);
        }

        /// <summary>
        /// // ExportJsonAsync method
        /// </summary>
        [Test]
        public async Task ExportJsonAsync_CreatesJsonFile() {
            string fullPath = "./DiplomMSSQLApp.WEB/Results/Organizations.json";
            File.Delete(fullPath);
            Mock<IUnitOfWork> mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Organizations.GetAllAsync()).ReturnsAsync(new Organization[] { });
            OrganizationService organizationService = GetNewService(mock.Object);

            await organizationService.ExportJsonAsync(fullPath);

            Assert.IsTrue(File.Exists(fullPath));
        }
    }
}
