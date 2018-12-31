using NUnit.Framework;
using OpenQA.Selenium;
using System;
using System.IO;
using System.Threading;

namespace DiplomMSSQLApp.IntegrationTests {
    [TestFixture]
    public class PostTests : SeleniumTest {
        public PostTests() : base("DiplomMSSQLApp.WEB") { }

        [Test]
        public void AddNewPost() {
            AddDepartmentIfNumberOfDepartmentsIsZero();
            string actualTitle;
            int expectedNumberOfPosts = GetNumberOfPosts() + 1;
            string postTitle = "Programmer" + new Random().Next(100000, 999999).ToString();

            GoToUrl("/Post/Index");
            ChromeDriver.FindElement(By.PartialLinkText("Добавить должность")).Click();
            actualTitle = ChromeDriver.Title;
            ChromeDriver.FindElement(By.Id("Title")).SendKeys(postTitle);
            ChromeDriver.FindElement(By.Id("NumberOfUnits")).SendKeys("10");
            ChromeDriver.FindElement(By.Id("Salary")).SendKeys("100000");
            ChromeDriver.FindElement(By.Id("Premium")).SendKeys("20000");
            ChromeDriver.FindElement(By.Id("NumberOfDaysOfLeave")).SendKeys("28");
            ChromeDriver.FindElement(By.XPath("//input[@value='Добавить']")).Click();

            Assert.AreEqual("Добавление должности", actualTitle);
            Assert.AreEqual(expectedNumberOfPosts, GetNumberOfPosts());
        }

        [Test]
        public void EditPostSalary() {
            AddPostIfNumberOfPostsIsZero();
            string actualTitle;
            string expectedSalary = new Random().Next(100000, 999999).ToString();

            GoToUrl("/Post/Index");
            ChromeDriver.FindElement(By.XPath("//tr[position()=last()]/td[position()=last()]/a")).Click(); // Click on the last link "Edit"
            actualTitle = ChromeDriver.Title;
            ChromeDriver.FindElement(By.Id("Salary")).Clear();
            ChromeDriver.FindElement(By.Id("Salary")).SendKeys(expectedSalary);
            ChromeDriver.FindElement(By.XPath("//input[@value='Обновить']")).Click();

            Assert.AreEqual("Обновление информации о должности", actualTitle);
            string actualSalary = ChromeDriver.FindElement(By.XPath("//tr[position()=last()]/td[5]")).Text;  // value from the last row and fifth column ("Salary")
            Assert.AreEqual(expectedSalary, actualSalary);
        }

        [Test]
        public void ViewPostDetails() {
            AddPostIfNumberOfPostsIsZero();
            string expectedPostTitle;

            GoToUrl("/Post/Index");
            expectedPostTitle = ChromeDriver.FindElement(By.XPath("//tr[position()=last()]/td[3]")).Text;   // value from the last row and third column ("Post Title")
            ChromeDriver.FindElement(By.XPath("//tr[position()=last()]/td[position()=last()]/a[2]")).Click();   // Click on the last link "Details"

            Assert.AreEqual("Подробная информация о должности", ChromeDriver.Title);
            string actualPostTitle = ChromeDriver.FindElement(By.XPath("//dt[contains(.,'Название должности')]/following-sibling::dd")).Text;
            Assert.AreEqual(expectedPostTitle, actualPostTitle);
        }

        [Test]
        public void DeletePost() {
            RemoveAllEmployees();
            AddPostIfNumberOfPostsIsZero();
            string actualTitle;
            int expectedNumberOfPosts = GetNumberOfPosts() - 1;

            GoToUrl("/Post/Index");
            ChromeDriver.FindElement(By.XPath("//tr[position()=last()]/td[position()=last()]/a[3]")).Click();   // Click on the last link "Delete"
            actualTitle = ChromeDriver.Title;
            ChromeDriver.FindElement(By.XPath("//input[@value='Удалить']")).Click();

            Assert.AreEqual("Удаление должности", actualTitle);
            Assert.AreEqual(expectedNumberOfPosts, GetNumberOfPosts());
        }

        [Test]
        public void DeleteAllPosts() {
            RemoveAllEmployees();
            AddPostIfNumberOfPostsIsZero();
            string actualTitle;

            GoToUrl("/Post/Index");
            ChromeDriver.FindElement(By.PartialLinkText("Удалить все должности")).Click();
            actualTitle = ChromeDriver.Title;
            ChromeDriver.FindElement(By.XPath("//input[@value='Удалить']")).Click();

            Assert.AreEqual("Удаление всех свободных должностей", actualTitle);
            Assert.AreEqual(0, GetNumberOfPosts());
        }

        [Test]
        public void ExportDataToJsonFile() {
            string fullPath = "./DiplomMSSQLApp.WEB/Results/Posts.json";
            if (File.Exists(fullPath)) File.Delete(fullPath);

            GoToUrl("/Post/Index");
            ChromeDriver.FindElement(By.PartialLinkText("Экспортировать данные в JSON файл")).Click();

            Assert.IsTrue(File.Exists(fullPath));
        }

        [Test]
        public void FilterPostsByTitle() {
            string expectedPostTitle = "Менеджер";
            RemoveAllPosts();
            AddPosts();

            GoToUrl("/Post/Index");
            ChromeDriver.FindElement(By.Id("filterToggle")).Click();
            Thread.Sleep(500);
            ChromeDriver.FindElement(By.Id("Filter_PostTitle_0_")).SendKeys(expectedPostTitle);
            ChromeDriver.FindElement(By.XPath("//input[@value='Найти']")).Click();
            Thread.Sleep(500);

            Assert.AreEqual(1, GetTextLabelForCount());
            string actualPostTitle = ChromeDriver.FindElement(By.XPath("//tbody[@id='tableBody']/tr[position()=last()]/td[3]")).Text;
            Assert.AreEqual(expectedPostTitle, actualPostTitle);
        }

        private void AddPosts() {
            string[] titles = { "Программист", "Менеджер", "Директор" };
            AddDepartmentIfNumberOfDepartmentsIsZero();
            GoToUrl("/Post/Index");
            foreach (string title in titles) {
                ChromeDriver.FindElement(By.PartialLinkText("Добавить должность")).Click();
                ChromeDriver.FindElement(By.Id("Title")).SendKeys(title);
                ChromeDriver.FindElement(By.Id("NumberOfUnits")).SendKeys("10");
                ChromeDriver.FindElement(By.Id("Salary")).SendKeys("100000");
                ChromeDriver.FindElement(By.Id("Premium")).SendKeys("20000");
                ChromeDriver.FindElement(By.Id("NumberOfDaysOfLeave")).SendKeys("28");
                ChromeDriver.FindElement(By.XPath("//input[@value='Добавить']")).Click();
            }
        }
    }
}
