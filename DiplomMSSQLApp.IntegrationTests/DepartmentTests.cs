using NUnit.Framework;
using OpenQA.Selenium;
using System;
using System.IO;

namespace DiplomMSSQLApp.IntegrationTests {
    [TestFixture]
    public class DepartmentTests : SeleniumTest {
        public DepartmentTests() : base("DiplomMSSQLApp.WEB") { }

        [Test]
        public void AddNewDepartment() {
            string actualTitle;
            int expectedNumberOfDepartments = GetNumberOfDepartments() + 1;
            string code = new Random().Next(100000, 999999).ToString();
            string departmentName = "IT" + code;

            GoToUrl("/Department/Index");
            ChromeDriver.FindElement(By.PartialLinkText("Добавить отдел")).Click();
            actualTitle = ChromeDriver.Title;
            ChromeDriver.FindElement(By.Id("Code")).SendKeys(code);
            ChromeDriver.FindElement(By.Id("DepartmentName")).SendKeys(departmentName);
            ChromeDriver.FindElement(By.XPath("//input[@value='Добавить']")).Click();

            Assert.AreEqual("Добавление отдела", actualTitle);
            Assert.AreEqual(expectedNumberOfDepartments, GetNumberOfDepartments());
        }

        [Test]
        public void EditDepartmentCode() {
            AddDepartmentIfNumberOfDepartmentsIsZero();
            string actualTitle;
            string expectedCode = new Random().Next(100000, 999999).ToString();

            GoToUrl("/Department/Index");
            ChromeDriver.FindElement(By.XPath("//tr[position()=last()]/td[position()=last()]/a")).Click(); // Click on the last link "Edit"
            actualTitle = ChromeDriver.Title;
            ChromeDriver.FindElement(By.Id("Code")).Clear();
            ChromeDriver.FindElement(By.Id("Code")).SendKeys(expectedCode);
            ChromeDriver.FindElement(By.XPath("//input[@value='Обновить']")).Click();

            Assert.AreEqual("Обновление информации об отделе", actualTitle);
            string actualCode = ChromeDriver.FindElement(By.XPath("//tr[position()=last()]/td")).Text;  // value from the last row and first column ("Department Code")
            Assert.AreEqual(expectedCode, actualCode);
        }

        [Test]
        public void ViewDepartmentDetails() {
            AddDepartmentIfNumberOfDepartmentsIsZero();
            string expectedCode;

            GoToUrl("/Department/Index");
            expectedCode = ChromeDriver.FindElement(By.XPath("//tr[position()=last()]/td")).Text;   // value from the last row and first column ("Department Code")
            ChromeDriver.FindElement(By.XPath("//tr[position()=last()]/td[position()=last()]/a[2]")).Click();   // Click on the last link "Details"

            Assert.AreEqual("Подробная информация об отделе", ChromeDriver.Title);
            string actualCode = ChromeDriver.FindElement(By.XPath("//dt[contains(.,'Код отдела')]/following-sibling::dd")).Text;
            Assert.AreEqual(expectedCode, actualCode);
        }

        [Test]
        public void DeleteDepartment() {
            RemoveAllPosts();
            AddDepartmentIfNumberOfDepartmentsIsZero();
            string actualTitle;
            int expectedNumberOfDepartments = GetNumberOfDepartments() - 1;

            GoToUrl("/Department/Index");
            ChromeDriver.FindElement(By.XPath("//tr[position()=last()]/td[position()=last()]/a[3]")).Click();   // Click on the last link "Delete"
            actualTitle = ChromeDriver.Title;
            ChromeDriver.FindElement(By.XPath("//input[@value='Удалить']")).Click();

            Assert.AreEqual("Удаление отдела", actualTitle);
            Assert.AreEqual(expectedNumberOfDepartments, GetNumberOfDepartments());
        }

        [Test]
        public void DeleteAllDepartments() {
            RemoveAllPosts();
            AddDepartmentIfNumberOfDepartmentsIsZero();
            string actualTitle;

            GoToUrl("/Department/Index");
            ChromeDriver.FindElement(By.PartialLinkText("Удалить все отделы")).Click();
            actualTitle = ChromeDriver.Title;
            ChromeDriver.FindElement(By.XPath("//input[@value='Удалить']")).Click();

            Assert.AreEqual("Удаление отделов", actualTitle);
            Assert.AreEqual(0, GetNumberOfDepartments());
        }

        [Test]
        public void ExportDataToJsonFile() {
            string fullPath = "./DiplomMSSQLApp.WEB/Results/Departments.json";
            if (File.Exists(fullPath)) File.Delete(fullPath);

            GoToUrl("/Department/Index");
            ChromeDriver.FindElement(By.PartialLinkText("Экспортировать данные в JSON файл")).Click();

            Assert.IsTrue(File.Exists(fullPath));
        }
    }
}
