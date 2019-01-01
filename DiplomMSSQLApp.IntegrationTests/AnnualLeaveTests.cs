using NUnit.Framework;
using OpenQA.Selenium;
using System;
using System.IO;
using System.Threading;

namespace DiplomMSSQLApp.IntegrationTests {
    [TestFixture]
    public class AnnualLeaveTests : SeleniumTest {
        public AnnualLeaveTests() : base("DiplomMSSQLApp.WEB") { }

        [Test]
        public void AddNewAnnualLeave() {
            AddLeaveScheduleFor2018();
            AddEmployeeIfNumberOfEmployeesIsZero();
            string actualTitle;
            int expectedNumberOfAnnualLeaves = GetNumberOfAnnualLeaves() + 1;

            GoToUrl("/AnnualLeave/Index");
            ChromeDriver.FindElement(By.PartialLinkText("Добавить отпуск")).Click();
            actualTitle = ChromeDriver.Title;
            ChromeDriver.FindElement(By.Id("ScheduledDate")).SendKeys("12122018");
            ChromeDriver.FindElement(By.Id("ScheduledNumberOfDays")).SendKeys("28");
            ChromeDriver.FindElement(By.XPath("//input[@value='Добавить']")).Click();

            Assert.AreEqual("Добавление отпуска", actualTitle);
            Assert.AreEqual(expectedNumberOfAnnualLeaves, GetNumberOfAnnualLeaves());
        }

        [Test]
        public void EditAnnualLeaveScheduledNumberOfDays() {
            AddAnnualLeaveIfNumberOfAnnualLeavesIsZero();
            string actualTitle;
            string expectedAnnualLeaveScheduledNumberOfDays = new Random().Next(1, 99).ToString();

            GoToUrl("/AnnualLeave/Index");
            ChromeDriver.FindElement(By.XPath("//tr[position()=last()]/td[position()=last()]/a")).Click(); // Click on the last link "Edit"
            actualTitle = ChromeDriver.Title;
            ChromeDriver.FindElement(By.Id("ScheduledNumberOfDays")).Clear();
            ChromeDriver.FindElement(By.Id("ScheduledNumberOfDays")).SendKeys(expectedAnnualLeaveScheduledNumberOfDays);
            ChromeDriver.FindElement(By.XPath("//input[@value='Обновить']")).Click();

            Assert.AreEqual("Обновление информации об отпуске", actualTitle);
            string actualAnnualLeaveScheduledNumberOfDays = ChromeDriver.FindElement(By.XPath("//tbody[@id='tableBody']/tr[position()=last()]/td[6]")).Text;  // value from the last row and sixth column ("ScheduledNumberOfDays")
            Assert.AreEqual(expectedAnnualLeaveScheduledNumberOfDays, actualAnnualLeaveScheduledNumberOfDays);
        }

        [Test]
        public void ViewAnnualLeaveDetails() {
            AddAnnualLeaveIfNumberOfAnnualLeavesIsZero();
            string expectedAnnualLeaveScheduledNumberOfDays;

            GoToUrl("/AnnualLeave/Index");
            expectedAnnualLeaveScheduledNumberOfDays = ChromeDriver.FindElement(By.XPath("//tbody[@id='tableBody']/tr[position()=last()]/td[6]")).Text;   // value from the last row and sixth column ("ScheduledNumberOfDays")
            ChromeDriver.FindElement(By.XPath("//tr[position()=last()]/td[position()=last()]/a[2]")).Click();   // Click on the last link "Details"

            Assert.AreEqual("Подробная информация об отпуске", ChromeDriver.Title);
            string actualAnnualLeaveScheduledNumberOfDays = ChromeDriver.FindElement(By.XPath("//dt[contains(.,'Запланированное количество дней отпуска')]/following-sibling::dd")).Text;
            Assert.AreEqual(expectedAnnualLeaveScheduledNumberOfDays, actualAnnualLeaveScheduledNumberOfDays);
        }

        [Test]
        public void DeleteAnnualLeave() {
            AddAnnualLeaveIfNumberOfAnnualLeavesIsZero();
            string actualTitle;
            int expectedNumberOfAnnualLeaves = GetNumberOfAnnualLeaves() - 1;

            GoToUrl("/AnnualLeave/Index");
            ChromeDriver.FindElement(By.XPath("//tr[position()=last()]/td[position()=last()]/a[3]")).Click();   // Click on the last link "Delete"
            actualTitle = ChromeDriver.Title;
            ChromeDriver.FindElement(By.XPath("//input[@value='Удалить']")).Click();

            Assert.AreEqual("Удаление отпуска", actualTitle);
            Assert.AreEqual(expectedNumberOfAnnualLeaves, GetNumberOfAnnualLeaves());
        }

        [Test]
        public void DeleteAllAnnualLeaves() {
            AddAnnualLeaveIfNumberOfAnnualLeavesIsZero();
            string actualTitle;

            GoToUrl("/AnnualLeave/Index");
            ChromeDriver.FindElement(By.PartialLinkText("Удалить все отпуска")).Click();
            actualTitle = ChromeDriver.Title;
            ChromeDriver.FindElement(By.XPath("//input[@value='Удалить']")).Click();

            Assert.AreEqual("Удаление всех отпусков", actualTitle);
            Assert.AreEqual(0, GetNumberOfAnnualLeaves());
        }

        [Test]
        public void ExportDataToJsonFile() {
            string fullPath = "./DiplomMSSQLApp.WEB/Results/AnnualLeaves.json";
            if (File.Exists(fullPath)) File.Delete(fullPath);

            GoToUrl("/AnnualLeave/Index");
            ChromeDriver.FindElement(By.PartialLinkText("Экспортировать данные в JSON файл")).Click();

            Assert.IsTrue(File.Exists(fullPath));
        }

        [Test]
        public void FilterAnnualLeavesByScheduledNumberOfDays() {
            string expectedAnnualLeaveScheduledNumberOfDays = "21";
            AddLeaveScheduleFor2018();
            RemoveAllAnnualLeaves();
            AddAnnualLeaves();

            GoToUrl("/AnnualLeave/Index");
            ChromeDriver.FindElement(By.Id("filterToggle")).Click();
            Thread.Sleep(500);
            ChromeDriver.FindElement(By.Id("Filter_MinScheduledNumberOfDays")).SendKeys("18");
            ChromeDriver.FindElement(By.Id("Filter_MaxScheduledNumberOfDays")).SendKeys("25");
            ChromeDriver.FindElement(By.XPath("//input[@value='Найти']")).Click();
            Thread.Sleep(500);

            Assert.AreEqual(1, GetTextLabelForCount(2));
            string actualAnnualLeaveScheduledNumberOfDays = ChromeDriver.FindElement(By.XPath("//tbody[@id='tableBody']/tr[position()=last()]/td[6]")).Text;
            Assert.AreEqual(expectedAnnualLeaveScheduledNumberOfDays, actualAnnualLeaveScheduledNumberOfDays);
        }

        private void AddAnnualLeaves() {
            string[] arrayScheduledNumberOfDays = { "14", "21", "28" };
            AddEmployeeIfNumberOfEmployeesIsZero();
            GoToUrl("/AnnualLeave/Index");
            foreach (string scheduledNumberOfDays in arrayScheduledNumberOfDays) {
                ChromeDriver.FindElement(By.PartialLinkText("Добавить отпуск")).Click();
                ChromeDriver.FindElement(By.Id("ScheduledDate")).SendKeys("12122018");
                ChromeDriver.FindElement(By.Id("ScheduledNumberOfDays")).SendKeys(scheduledNumberOfDays);
                ChromeDriver.FindElement(By.XPath("//input[@value='Добавить']")).Click();
            }
        }
    }
}
