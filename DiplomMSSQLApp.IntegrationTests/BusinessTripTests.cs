using NUnit.Framework;
using OpenQA.Selenium;
using System;
using System.IO;
using System.Threading;

namespace DiplomMSSQLApp.IntegrationTests {
    [TestFixture]
    public class BusinessTripTests : SeleniumTest {
        public BusinessTripTests() : base("DiplomMSSQLApp.WEB") { }

        [Test]
        public void AddNewBusinessTrip() {
            AddEmployeeIfNumberOfEmployeesIsZero();
            string actualTitle;
            string businessTripCode = new Random().Next(100000, 999999).ToString();
            int expectedNumberOfBusinessTrips = GetNumberOfBusinessTrips() + 1;

            GoToUrl("/BusinessTrip/Index");
            ChromeDriver.FindElement(By.PartialLinkText("Добавить командировку")).Click();
            actualTitle = ChromeDriver.Title;
            ChromeDriver.FindElement(By.Id("Name")).SendKeys(businessTripCode);
            ChromeDriver.FindElement(By.Id("DateStart")).SendKeys("12122018");
            ChromeDriver.FindElement(By.Id("DateEnd")).SendKeys("16122018");
            ChromeDriver.FindElement(By.Id("Destination")).SendKeys("Москва");
            ChromeDriver.FindElement(By.Id("Purpose")).SendKeys("Семинар");
            ChromeDriver.FindElement(By.XPath("//input[@value='Добавить']")).Click();

            Assert.AreEqual("Добавление командировки", actualTitle);
            Assert.AreEqual(expectedNumberOfBusinessTrips, GetNumberOfBusinessTrips());
        }

        [Test]
        public void EditBusinessTripCode() {
            AddBusinessTripIfNumberOfBusinessTripsIsZero();
            string actualTitle;
            string expectedBusinessTripCode = new Random().Next(100000, 999999).ToString();

            GoToUrl("/BusinessTrip/Index");
            ChromeDriver.FindElement(By.XPath("//tr[position()=last()]/td[position()=last()]/a")).Click(); // Click on the last link "Edit"
            actualTitle = ChromeDriver.Title;
            ChromeDriver.FindElement(By.Id("Name")).Clear();
            ChromeDriver.FindElement(By.Id("Name")).SendKeys(expectedBusinessTripCode);
            ChromeDriver.FindElement(By.XPath("//input[@value='Обновить']")).Click();

            Assert.AreEqual("Обновление информации о командировке", actualTitle);
            string actualBusinessTripCode = ChromeDriver.FindElement(By.XPath("//tbody[@id='tableBody']/tr[position()=last()]/td")).Text;  // value from the last row and first column ("BusinessTripCode")
            Assert.AreEqual(expectedBusinessTripCode, actualBusinessTripCode);
        }

        [Test]
        public void ViewBusinessTripDetails() {
            AddBusinessTripIfNumberOfBusinessTripsIsZero();
            string expectedBusinessTripCode;

            GoToUrl("/BusinessTrip/Index");
            expectedBusinessTripCode = ChromeDriver.FindElement(By.XPath("//tbody[@id='tableBody']/tr[position()=last()]/td")).Text;   // value from the last row and first column ("BusinessTripCode")
            ChromeDriver.FindElement(By.XPath("//tr[position()=last()]/td[position()=last()]/a[2]")).Click();   // Click on the last link "Details"

            Assert.AreEqual("Подробная информация о командировке", ChromeDriver.Title);
            string actualBusinessTripCode = ChromeDriver.FindElement(By.XPath("//dt[contains(.,'Код командировки')]/following-sibling::dd")).Text;
            Assert.AreEqual(expectedBusinessTripCode, actualBusinessTripCode);
        }

        [Test]
        public void DeleteBusinessTrip() {
            AddBusinessTripIfNumberOfBusinessTripsIsZero();
            string actualTitle;
            int expectedNumberOfBusinessTrips = GetNumberOfBusinessTrips() - 1;

            GoToUrl("/BusinessTrip/Index");
            ChromeDriver.FindElement(By.XPath("//tr[position()=last()]/td[position()=last()]/a[3]")).Click();   // Click on the last link "Delete"
            actualTitle = ChromeDriver.Title;
            ChromeDriver.FindElement(By.XPath("//input[@value='Удалить']")).Click();

            Assert.AreEqual("Удаление командировки", actualTitle);
            Assert.AreEqual(expectedNumberOfBusinessTrips, GetNumberOfBusinessTrips());
        }

        [Test]
        public void DeleteAllBusinessTrips() {
            AddBusinessTripIfNumberOfBusinessTripsIsZero();
            string actualTitle;

            GoToUrl("/BusinessTrip/Index");
            ChromeDriver.FindElement(By.PartialLinkText("Удалить все командировки")).Click();
            actualTitle = ChromeDriver.Title;
            ChromeDriver.FindElement(By.XPath("//input[@value='Удалить']")).Click();

            Assert.AreEqual("Удаление всех командировок", actualTitle);
            Assert.AreEqual(0, GetNumberOfBusinessTrips());
        }

        [Test]
        public void ExportDataToJsonFile() {
            string fullPath = "./DiplomMSSQLApp.WEB/Results/BusinessTrips.json";
            if (File.Exists(fullPath)) File.Delete(fullPath);

            GoToUrl("/BusinessTrip/Index");
            ChromeDriver.FindElement(By.PartialLinkText("Экспортировать данные в JSON файл")).Click();

            Assert.IsTrue(File.Exists(fullPath));
        }

        [Test]
        public void FilterBusinessTripsByCode() {
            string expectedBusinessTripCode = "100058";
            RemoveAllBusinessTrips();
            AddBusinessTrips();

            GoToUrl("/BusinessTrip/Index");
            ChromeDriver.FindElement(By.Id("filterToggle")).Click();
            Thread.Sleep(500);
            ChromeDriver.FindElement(By.Id("Filter_Code_0_")).SendKeys(expectedBusinessTripCode);
            ChromeDriver.FindElement(By.XPath("//input[@value='Найти']")).Click();
            Thread.Sleep(500);

            Assert.AreEqual(1, GetTextLabelForCount(2));
            string actualBusinessTripCode = ChromeDriver.FindElement(By.XPath("//tbody[@id='tableBody']/tr[position()=last()]/td")).Text;
            Assert.AreEqual(expectedBusinessTripCode, actualBusinessTripCode);
        }

        private void AddBusinessTrips() {
            string[] codes = { "100023", "100058", "100082" };
            AddEmployeeIfNumberOfEmployeesIsZero();
            GoToUrl("/BusinessTrip/Index");
            foreach (string code in codes) {
                ChromeDriver.FindElement(By.PartialLinkText("Добавить командировку")).Click();
                ChromeDriver.FindElement(By.Id("Name")).SendKeys(code);
                ChromeDriver.FindElement(By.Id("DateStart")).SendKeys("12122018");
                ChromeDriver.FindElement(By.Id("DateEnd")).SendKeys("16122018");
                ChromeDriver.FindElement(By.Id("Destination")).SendKeys("Москва");
                ChromeDriver.FindElement(By.Id("Purpose")).SendKeys("Семинар");
                ChromeDriver.FindElement(By.XPath("//input[@value='Добавить']")).Click();
            }
        }
    }
}
