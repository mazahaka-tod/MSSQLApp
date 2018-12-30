using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Diagnostics;
using System.IO;

namespace DiplomMSSQLApp.IntegrationTests {
    [TestFixture]
    public abstract class SeleniumTest {
        const int iisPort = 50319;
        private readonly string _applicationName;
        private Process _iisProcess;

        protected ChromeDriver ChromeDriver { get; set; }

        protected SeleniumTest(string applicationName) {
            _applicationName = applicationName;
        }

        [SetUp]
        protected void RunBeforeEachTest() {
            StartIIS();
            ChromeDriver = new ChromeDriver(Path.Combine(Directory.GetCurrentDirectory(), "DiplomMSSQLApp.IntegrationTests"));
        }

        [TearDown]
        protected void RunAfterEachTest() {
            if (_iisProcess.HasExited == false) _iisProcess.Kill();
            ChromeDriver.Quit();
        }

        private void StartIIS() {
            string applicationPath = Path.Combine(Directory.GetCurrentDirectory(), _applicationName);
            string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            _iisProcess = new Process();
            _iisProcess.StartInfo.FileName = Path.Combine(programFiles, "IIS Express\\iisexpress.exe");
            _iisProcess.StartInfo.Arguments = $"/path:{applicationPath} /port:{iisPort}";
            _iisProcess.Start();
        }

        protected string GetAbsoluteUrl(string relativeUrl) {
            if (!relativeUrl.StartsWith("/")) relativeUrl = "/" + relativeUrl;
            return $"http://localhost:{iisPort}{relativeUrl}";
        }

        protected void SignIn() {
            ChromeDriver.FindElement(By.Id("Name")).SendKeys("Admin");
            ChromeDriver.FindElement(By.Id("Password")).SendKeys("MySecret");
            ChromeDriver.FindElement(By.XPath("//input[@value='Войти']")).Click();
        }
    }
}
