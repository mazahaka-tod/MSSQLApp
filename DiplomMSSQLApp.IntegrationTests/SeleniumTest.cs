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

        protected void GoToUrl(string url) {
            ChromeDriver.Navigate().GoToUrl(GetAbsoluteUrl(url));
            if (ChromeDriver.Title == "Вход") SignIn();
        }

        protected int GetNumberOfDepartments() {
            GoToUrl("/Department/Index");
            return GetTextLabelForCount(2);
        }

        protected int GetTextLabelForCount(int index) {
            IWebElement label = ChromeDriver.FindElement(By.CssSelector("label[for='Count']"));
            return int.Parse(label.Text.Split(new char[] { ' ' })[index]);
        }

        protected void AddDepartmentIfNumberOfDepartmentsIsZero() {
            if (GetNumberOfDepartments() > 0) return;
            GoToUrl("/Department/Index");
            ChromeDriver.FindElement(By.PartialLinkText("Добавить отдел")).Click();
            ChromeDriver.FindElement(By.Id("Code")).SendKeys("100000");
            ChromeDriver.FindElement(By.Id("DepartmentName")).SendKeys("IT");
            ChromeDriver.FindElement(By.XPath("//input[@value='Добавить']")).Click();
        }

        protected int GetNumberOfPosts() {
            GoToUrl("/Post/Index");
            return GetTextLabelForCount(2);
        }

        protected void AddPostIfNumberOfPostsIsZero() {
            if (GetNumberOfPosts() > 0) return;
            AddDepartmentIfNumberOfDepartmentsIsZero();
            GoToUrl("/Post/Index");
            ChromeDriver.FindElement(By.PartialLinkText("Добавить должность")).Click();
            ChromeDriver.FindElement(By.Id("Title")).SendKeys("Programmer");
            ChromeDriver.FindElement(By.Id("NumberOfUnits")).SendKeys("10");
            ChromeDriver.FindElement(By.Id("Salary")).SendKeys("100000");
            ChromeDriver.FindElement(By.Id("Premium")).SendKeys("20000");
            ChromeDriver.FindElement(By.Id("NumberOfDaysOfLeave")).SendKeys("28");
            ChromeDriver.FindElement(By.XPath("//input[@value='Добавить']")).Click();
        }

        protected int GetNumberOfEmployees() {
            GoToUrl("/Employee/Index");
            return GetTextLabelForCount(2);
        }

        protected void AddEmployeeIfNumberOfEmployeesIsZero() {
            if (GetNumberOfEmployees() > 0) return;
            AddPostIfNumberOfPostsIsZero();
            GoToUrl("/Employee/Index");
            ChromeDriver.FindElement(By.PartialLinkText("Добавить сотрудника")).Click();
            ChromeDriver.FindElement(By.Id("PersonnelNumber")).SendKeys("100000");
            ChromeDriver.FindElement(By.Id("LastName")).SendKeys("Петров");
            ChromeDriver.FindElement(By.Id("FirstName")).SendKeys("Петр");
            ChromeDriver.FindElement(By.Id("HireDate")).SendKeys("12122017");
            ChromeDriver.FindElement(By.Id("Birth_BirthDate")).SendKeys("23061990");
            ChromeDriver.FindElement(By.XPath("//input[@value='Добавить']")).Click();
        }

        protected void RemoveAllEmployees() {
            GoToUrl("/Employee/Index");
            ChromeDriver.FindElement(By.PartialLinkText("Удалить всех сотрудников")).Click();
            ChromeDriver.FindElement(By.XPath("//input[@value='Удалить']")).Click();
        }

        protected void RemoveAllPosts() {
            RemoveAllEmployees();
            GoToUrl("/Post/Index");
            ChromeDriver.FindElement(By.PartialLinkText("Удалить все должности")).Click();
            ChromeDriver.FindElement(By.XPath("//input[@value='Удалить']")).Click();
        }

        protected int GetNumberOfBusinessTrips() {
            GoToUrl("/BusinessTrip/Index");
            return GetTextLabelForCount(2);
        }

        protected void AddBusinessTripIfNumberOfBusinessTripsIsZero() {
            if (GetNumberOfBusinessTrips() > 0) return;
            AddEmployeeIfNumberOfEmployeesIsZero();
            GoToUrl("/BusinessTrip/Index");
            ChromeDriver.FindElement(By.PartialLinkText("Добавить командировку")).Click();
            ChromeDriver.FindElement(By.Id("Name")).SendKeys("100000");
            ChromeDriver.FindElement(By.Id("DateStart")).SendKeys("12122018");
            ChromeDriver.FindElement(By.Id("DateEnd")).SendKeys("16122018");
            ChromeDriver.FindElement(By.Id("Destination")).SendKeys("Москва");
            ChromeDriver.FindElement(By.Id("Purpose")).SendKeys("Семинар");
            ChromeDriver.FindElement(By.XPath("//input[@value='Добавить']")).Click();
        }

        protected void RemoveAllBusinessTrips() {
            GoToUrl("/BusinessTrip/Index");
            ChromeDriver.FindElement(By.PartialLinkText("Удалить все командировки")).Click();
            ChromeDriver.FindElement(By.XPath("//input[@value='Удалить']")).Click();
        }

        protected int GetNumberOfAnnualLeaves() {
            GoToUrl("/AnnualLeave/Index");
            return GetTextLabelForCount(2);
        }

        protected void AddAnnualLeaveIfNumberOfAnnualLeavesIsZero() {
            if (GetNumberOfAnnualLeaves() > 0) return;
            AddLeaveScheduleFor2018();
            AddEmployeeIfNumberOfEmployeesIsZero();
            GoToUrl("/AnnualLeave/Index");
            ChromeDriver.FindElement(By.PartialLinkText("Добавить отпуск")).Click();
            ChromeDriver.FindElement(By.Id("ScheduledDate")).SendKeys("12122018");
            ChromeDriver.FindElement(By.Id("ScheduledNumberOfDays")).SendKeys("28");
            ChromeDriver.FindElement(By.XPath("//input[@value='Добавить']")).Click();
        }

        protected void RemoveAllAnnualLeaves() {
            GoToUrl("/AnnualLeave/Index");
            ChromeDriver.FindElement(By.PartialLinkText("Удалить все отпуска")).Click();
            ChromeDriver.FindElement(By.XPath("//input[@value='Удалить']")).Click();
        }

        protected int GetNumberOfLeaveSchedules() {
            GoToUrl("/LeaveSchedule/Index");
            return GetTextLabelForCount(3);
        }

        protected void AddLeaveScheduleIfNumberOfLeaveSchedulesIsZero() {
            if (GetNumberOfLeaveSchedules() > 0) return;
            GoToUrl("/LeaveSchedule/Index");
            ChromeDriver.FindElement(By.PartialLinkText("Добавить график отпусков")).Click();
            ChromeDriver.FindElement(By.Id("Number")).SendKeys("1");
            ChromeDriver.FindElement(By.Id("Year")).SendKeys("2019");
            ChromeDriver.FindElement(By.Id("DateOfPreparation")).SendKeys("12122018");
            ChromeDriver.FindElement(By.XPath("//input[@value='Добавить']")).Click();
        }

        protected void AddLeaveScheduleFor2018() {
            GoToUrl("/LeaveSchedule/Index");
            ChromeDriver.FindElement(By.PartialLinkText("Добавить график отпусков")).Click();
            ChromeDriver.FindElement(By.Id("Number")).SendKeys("1");
            ChromeDriver.FindElement(By.Id("Year")).SendKeys("2018");
            ChromeDriver.FindElement(By.Id("DateOfPreparation")).SendKeys("12122017");
            ChromeDriver.FindElement(By.XPath("//input[@value='Добавить']")).Click();
        }
    }
}
