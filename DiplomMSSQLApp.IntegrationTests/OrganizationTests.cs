using NUnit.Framework;
using OpenQA.Selenium;
using System;
using System.IO;

namespace DiplomMSSQLApp.IntegrationTests {
    [TestFixture]
    public class OrganizationTests : SeleniumTest {
        public OrganizationTests() : base("DiplomMSSQLApp.WEB") { }

        [Test]
        public void SignInAsAdmin() {
            ChromeDriver.Navigate().GoToUrl(GetAbsoluteUrl("/Organization/Index"));
            SignIn();

            Assert.AreEqual("Информация об организации", ChromeDriver.Title);
        }

        [Test]
        public void EditPhoneOrganization() {
            string phoneNumber = "+7 (922) " + new Random().Next(10000000, 99999999).ToString();

            ChromeDriver.Navigate().GoToUrl(GetAbsoluteUrl("/Organization/Index"));
            if (ChromeDriver.Title == "Вход") SignIn();
            ChromeDriver.FindElement(By.PartialLinkText("Изменить информацию")).Click();
            ChromeDriver.FindElement(By.Id("Phone")).Clear();
            ChromeDriver.FindElement(By.Id("Phone")).SendKeys(phoneNumber);
            ChromeDriver.FindElement(By.XPath("//input[@value='Сохранить']")).Click();

            IWebElement element = ChromeDriver.FindElement(By.XPath("//dt[contains(.,'Телефон')]/following-sibling::dd"));
            Assert.AreEqual(phoneNumber, element.Text);
        }

        [Test]
        public void ExportDataToJsonFile() {
            string fullPath = "./DiplomMSSQLApp.WEB/Results/Organizations.json";
            if (File.Exists(fullPath)) File.Delete(fullPath);

            ChromeDriver.Navigate().GoToUrl(GetAbsoluteUrl("/Organization/Index"));
            if (ChromeDriver.Title == "Вход") SignIn();
            ChromeDriver.FindElement(By.PartialLinkText("Экспортировать данные в JSON файл")).Click();

            Assert.IsTrue(File.Exists(fullPath));
        }
    }
}
