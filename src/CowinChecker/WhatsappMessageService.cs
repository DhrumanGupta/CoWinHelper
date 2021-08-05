using System;
using System.Collections.Generic;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace CowinChecker
{
    public class WhatsappMessageService : IMessageService
    {
        public bool IsReady { get; set; }
        private ChromeDriver _driver;
        
        private const string _inputXpath = "/html/body/div/div[1]/div[1]/div[4]/div[1]/footer/div[1]/div[2]/div/div[1]/div/div[2]";

        public WhatsappMessageService()
        {
            // var driverService = ChromeDriverService.CreateDefaultService("C:/webDrivers/");

            var options = new ChromeOptions();

            options.AddExcludedArgument("enable-automation");

            options.AddArgument("disable-gpu");
            options.AddArgument("disable-software-rasterizer");
            options.AddArgument("no-sandbox");

            // options.AddArgument("headless");

            var chromeDriverService = ChromeDriverService.CreateDefaultService();
            chromeDriverService.HideCommandPromptWindow = true;
            chromeDriverService.SuppressInitialDiagnosticInformation = true;

            _driver = new ChromeDriver(options) {Url = "https://web.whatsapp.com/"};
            _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(4);

            try
            {
                while (_driver.FindElement(
                           By.XPath("//*[@id=\"app\"]/div[1]/div/div[2]/div[1]/div/div[2]/div/canvas")) !=
                       null)
                {
                }
            }
            catch (NoSuchElementException)
            {
                this.IsReady = true;
                Console.WriteLine("READY!");
            }

        }

        public void Send(string to, string message)
        {
            try
            {
                var searchBox = _driver.FindElement(By.XPath("//*[@id=\"side\"]/div[1]/div/label/div/div[2]"));

                searchBox.SendKeys(to);

                var groupTitle = _driver.FindElement(By.XPath($"//span[contains(@title,'{to}')]"));
                groupTitle.Click();
                var inputBox = _driver.FindElement(By.XPath(_inputXpath));
                inputBox.SendKeys(message);
                inputBox.SendKeys(Keys.Enter);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                // ignored
            }
        }

        public void Send(string to, IEnumerable<string> messages)
        {
            try
            {
                var searchBox = _driver.FindElement(By.XPath("//*[@id=\"side\"]/div[1]/div/label/div/div[2]"));
                if (searchBox == null) return;

                searchBox.SendKeys(to);

                var groupTitle = _driver.FindElement(By.XPath($"//span[contains(@title,'{to}')]"));
                groupTitle.Click();

                var inputBox = _driver.FindElement(By.XPath(_inputXpath));

                foreach (var message in messages)
                {
                    inputBox.SendKeys(message);
                    inputBox.SendKeys(Keys.Enter);
                }
            }
            catch
            {
                // ignored
            }
        }
    }
}