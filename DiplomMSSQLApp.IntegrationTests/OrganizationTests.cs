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
            string actualTitle;

            ChromeDriver.Navigate().GoToUrl(GetAbsoluteUrl("/Organization/Index"));
            actualTitle = ChromeDriver.Title;
            SignIn();

            Assert.AreEqual("Вход", actualTitle);
            Assert.AreEqual("Информация об организации", ChromeDriver.Title);
        }

        [Test]
        public void EditPhoneOrganization() {
            string actualTitle;
            string expectedPhone = "+7 (922) " + new Random().Next(10000000, 99999999).ToString();

            GoToUrl("/Organization/Index");
            ChromeDriver.FindElement(By.PartialLinkText("Изменить информацию")).Click();
            actualTitle = ChromeDriver.Title;
            ChromeDriver.FindElement(By.Id("Phone")).Clear();
            ChromeDriver.FindElement(By.Id("Phone")).SendKeys(expectedPhone);
            ChromeDriver.FindElement(By.XPath("//input[@value='Сохранить']")).Click();

            Assert.AreEqual("Редактирование информации об организации", actualTitle);
            string actualPhone = ChromeDriver.FindElement(By.XPath("//dt[contains(.,'Телефон')]/following-sibling::dd")).Text;
            Assert.AreEqual(expectedPhone, actualPhone);
        }

        [Test]
        public void ExportDataToJsonFile() {
            string fullPath = "./DiplomMSSQLApp.WEB/Results/Organizations.json";
            if (File.Exists(fullPath)) File.Delete(fullPath);

            GoToUrl("/Organization/Index");
            ChromeDriver.FindElement(By.PartialLinkText("Экспортировать данные в JSON файл")).Click();

            Assert.IsTrue(File.Exists(fullPath));
        }
    }
}
