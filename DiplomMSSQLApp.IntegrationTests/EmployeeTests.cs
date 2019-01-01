using NUnit.Framework;
using OpenQA.Selenium;
using System;
using System.IO;
using System.Threading;

namespace DiplomMSSQLApp.IntegrationTests {
    [TestFixture]
    public class EmployeeTests : SeleniumTest {
        public EmployeeTests() : base("DiplomMSSQLApp.WEB") { }

        [Test]
        public void AddNewEmployee() {
            AddPostIfNumberOfPostsIsZero();
            RemoveAllEmployees();
            string actualTitle;
            string personnelNumber = new Random().Next(100000, 999999).ToString();

            GoToUrl("/Employee/Index");
            ChromeDriver.FindElement(By.PartialLinkText("Добавить сотрудника")).Click();
            actualTitle = ChromeDriver.Title;
            ChromeDriver.FindElement(By.Id("PersonnelNumber")).SendKeys(personnelNumber);
            ChromeDriver.FindElement(By.Id("LastName")).SendKeys("Петров");
            ChromeDriver.FindElement(By.Id("FirstName")).SendKeys("Петр");
            ChromeDriver.FindElement(By.Id("HireDate")).SendKeys("12122017");
            ChromeDriver.FindElement(By.Id("Birth_BirthDate")).SendKeys("23061990");
            ChromeDriver.FindElement(By.XPath("//input[@value='Добавить']")).Click();

            Assert.AreEqual("Добавление сотрудника", actualTitle);
            Assert.AreEqual(1, GetNumberOfEmployees());
        }

        [Test]
        public void EditEmployeeMobilePhone() {
            AddEmployeeIfNumberOfEmployeesIsZero();
            string actualTitle;
            string expectedMobilePhone ="+7 (922) " + new Random().Next(10000000, 99999999).ToString();

            GoToUrl("/Employee/Index");
            ChromeDriver.FindElement(By.XPath("//tr[position()=last()]/td[position()=last()]/a")).Click(); // Click on the last link "Edit"
            actualTitle = ChromeDriver.Title;
            ChromeDriver.FindElement(By.Id("Contacts_MobilePhone")).Clear();
            ChromeDriver.FindElement(By.Id("Contacts_MobilePhone")).SendKeys(expectedMobilePhone);
            ChromeDriver.FindElement(By.XPath("//input[@value='Обновить']")).Click();

            Assert.AreEqual("Обновление информации о сотруднике", actualTitle);
            string actualMobilePhone = ChromeDriver.FindElement(By.XPath("//tr[position()=last()]/td[4]")).Text;  // value from the last row and fourth column ("MobilePhone")
            Assert.AreEqual(expectedMobilePhone, actualMobilePhone);
        }

        [Test]
        public void ViewEmployeeDetails() {
            AddEmployeeIfNumberOfEmployeesIsZero();
            string expectedEmployeeLastName;

            GoToUrl("/Employee/Index");
            expectedEmployeeLastName = ChromeDriver.FindElement(By.XPath("//tbody[@id='tableBody']/tr[position()=last()]/td")).Text;   // value from the last row and first column ("LastName")
            ChromeDriver.FindElement(By.XPath("//tr[position()=last()]/td[position()=last()]/a[2]")).Click();   // Click on the last link "Details"

            Assert.AreEqual("Подробная информация о сотруднике", ChromeDriver.Title);
            string actualEmployeeLastName = ChromeDriver.FindElement(By.XPath("//dt[contains(.,'Фамилия')]/following-sibling::dd")).Text;
            Assert.AreEqual(expectedEmployeeLastName, actualEmployeeLastName);
        }

        [Test]
        public void DeleteEmployee() {
            AddEmployeeIfNumberOfEmployeesIsZero();
            string actualTitle;
            int expectedNumberOfEmployees = GetNumberOfEmployees() - 1;

            GoToUrl("/Employee/Index");
            ChromeDriver.FindElement(By.XPath("//tr[position()=last()]/td[position()=last()]/a[3]")).Click();   // Click on the last link "Delete"
            actualTitle = ChromeDriver.Title;
            ChromeDriver.FindElement(By.XPath("//input[@value='Удалить']")).Click();

            Assert.AreEqual("Удаление сотрудника", actualTitle);
            Assert.AreEqual(expectedNumberOfEmployees, GetNumberOfEmployees());
        }

        [Test]
        public void DeleteAllEmployees() {
            AddEmployeeIfNumberOfEmployeesIsZero();
            string actualTitle;

            GoToUrl("/Employee/Index");
            ChromeDriver.FindElement(By.PartialLinkText("Удалить всех сотрудников")).Click();
            actualTitle = ChromeDriver.Title;
            ChromeDriver.FindElement(By.XPath("//input[@value='Удалить']")).Click();

            Assert.AreEqual("Удаление всех сотрудников", actualTitle);
            Assert.AreEqual(0, GetNumberOfEmployees());
        }

        [Test]
        public void ExportDataToJsonFile() {
            string fullPath = "./DiplomMSSQLApp.WEB/Results/Employees.json";
            if (File.Exists(fullPath)) File.Delete(fullPath);

            GoToUrl("/Employee/Index");
            ChromeDriver.FindElement(By.PartialLinkText("Экспортировать данные в JSON файл")).Click();

            Assert.IsTrue(File.Exists(fullPath));
        }

        [Test]
        public void FilterEmployeesByLastName() {
            string expectedEmployeeLastName = "Петров";
            RemoveAllEmployees();
            AddEmployees();

            GoToUrl("/Employee/Index");
            ChromeDriver.FindElement(By.Id("filterToggle")).Click();
            Thread.Sleep(500);
            ChromeDriver.FindElement(By.Id("Filter_LastName_0_")).SendKeys(expectedEmployeeLastName);
            ChromeDriver.FindElement(By.XPath("//input[@value='Найти']")).Click();
            Thread.Sleep(500);

            Assert.AreEqual(1, GetTextLabelForCount(2));
            string actualEmployeeLastName = ChromeDriver.FindElement(By.XPath("//tbody[@id='tableBody']/tr[position()=last()]/td")).Text;
            Assert.AreEqual(expectedEmployeeLastName, actualEmployeeLastName);
        }

        private void AddEmployees() {
            string[] lastNames = { "Петров", "Иванов", "Сидоров" };
            AddPostIfNumberOfPostsIsZero();
            GoToUrl("/Employee/Index");
            foreach (string lastName in lastNames) {
                ChromeDriver.FindElement(By.PartialLinkText("Добавить сотрудника")).Click();
                ChromeDriver.FindElement(By.Id("PersonnelNumber")).SendKeys("100000");
                ChromeDriver.FindElement(By.Id("LastName")).SendKeys(lastName);
                ChromeDriver.FindElement(By.Id("FirstName")).SendKeys("Макс");
                ChromeDriver.FindElement(By.Id("HireDate")).SendKeys("12122017");
                ChromeDriver.FindElement(By.Id("Birth_BirthDate")).SendKeys("23061990");
                ChromeDriver.FindElement(By.XPath("//input[@value='Добавить']")).Click();
            }
        }
    }
}
