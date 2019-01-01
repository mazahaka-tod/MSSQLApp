using NUnit.Framework;
using OpenQA.Selenium;
using System;
using System.IO;

namespace DiplomMSSQLApp.IntegrationTests {
    [TestFixture]
    public class LeaveScheduleTests : SeleniumTest {
        public LeaveScheduleTests() : base("DiplomMSSQLApp.WEB") { }

        [Test]
        public void AddNewLeaveSchedule() {
            string actualTitle;
            string year = new Random().Next(1900, 2100).ToString();
            int expectedNumberOfLeaveSchedules = GetNumberOfLeaveSchedules() + 1;

            GoToUrl("/LeaveSchedule/Index");
            ChromeDriver.FindElement(By.PartialLinkText("Добавить график отпусков")).Click();
            actualTitle = ChromeDriver.Title;
            ChromeDriver.FindElement(By.Id("Number")).SendKeys("1");
            ChromeDriver.FindElement(By.Id("Year")).SendKeys(year);
            ChromeDriver.FindElement(By.Id("DateOfPreparation")).SendKeys("12122018");
            ChromeDriver.FindElement(By.XPath("//input[@value='Добавить']")).Click();

            Assert.AreEqual("Добавление графика отпусков", actualTitle);
            Assert.AreEqual(expectedNumberOfLeaveSchedules, GetNumberOfLeaveSchedules());
        }

        [Test]
        public void EditLeaveScheduleNumber() {
            AddLeaveScheduleIfNumberOfLeaveSchedulesIsZero();
            string actualTitle;
            string expectedLeaveScheduleNumber = new Random().Next(100000, 999999).ToString();

            GoToUrl("/LeaveSchedule/Index");
            ChromeDriver.FindElement(By.XPath("//tr[position()=last()]/td[position()=last()]/a")).Click(); // Click on the last link "Edit"
            actualTitle = ChromeDriver.Title;
            ChromeDriver.FindElement(By.Id("Number")).Clear();
            ChromeDriver.FindElement(By.Id("Number")).SendKeys(expectedLeaveScheduleNumber);
            ChromeDriver.FindElement(By.XPath("//input[@value='Обновить']")).Click();

            Assert.AreEqual("Обновление информации о графике отпусков", actualTitle);
            string actualLeaveScheduleNumber = ChromeDriver.FindElement(By.XPath("//tbody[@id='tableBody']/tr[position()=last()]/td")).Text;  // value from the last row and first column ("Number")
            Assert.AreEqual(expectedLeaveScheduleNumber, actualLeaveScheduleNumber);
        }

        [Test]
        public void DeleteLeaveSchedule() {
            AddLeaveScheduleIfNumberOfLeaveSchedulesIsZero();
            string actualTitle;
            int expectedNumberOfLeaveSchedules = GetNumberOfLeaveSchedules() - 1;

            GoToUrl("/LeaveSchedule/Index");
            ChromeDriver.FindElement(By.XPath("//tr[position()=last()]/td[position()=last()]/a[2]")).Click();   // Click on the last link "Delete"
            actualTitle = ChromeDriver.Title;
            ChromeDriver.FindElement(By.XPath("//input[@value='Удалить']")).Click();

            Assert.AreEqual("Удаление графика отпусков", actualTitle);
            Assert.AreEqual(expectedNumberOfLeaveSchedules, GetNumberOfLeaveSchedules());
        }

        [Test]
        public void DeleteAllLeaveSchedules() {
            AddLeaveScheduleIfNumberOfLeaveSchedulesIsZero();
            string actualTitle;

            GoToUrl("/LeaveSchedule/Index");
            ChromeDriver.FindElement(By.PartialLinkText("Удалить все графики отпусков")).Click();
            actualTitle = ChromeDriver.Title;
            ChromeDriver.FindElement(By.XPath("//input[@value='Удалить']")).Click();

            Assert.AreEqual("Удаление всех графиков отпусков", actualTitle);
            Assert.AreEqual(0, GetNumberOfLeaveSchedules());
        }

        [Test]
        public void ExportDataToJsonFile() {
            string fullPath = "./DiplomMSSQLApp.WEB/Results/LeaveSchedules.json";
            if (File.Exists(fullPath)) File.Delete(fullPath);

            GoToUrl("/LeaveSchedule/Index");
            ChromeDriver.FindElement(By.PartialLinkText("Экспортировать данные в JSON файл")).Click();

            Assert.IsTrue(File.Exists(fullPath));
        }
    }
}
