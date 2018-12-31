﻿using NUnit.Framework;
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
            return GetTextLabelForCount();
        }

        protected int GetTextLabelForCount() {
            IWebElement label = ChromeDriver.FindElement(By.CssSelector("label[for='Count']"));
            return int.Parse(label.Text.Split(new char[] { ' ' })[2]);
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
            return GetTextLabelForCount();
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
            return GetTextLabelForCount();
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
    }
}