using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace CowinChecker
{
    public class WebdriverHttpManager : IHttpManager
    {
        private ChromeDriver _driver;

        public WebdriverHttpManager()
        {
            // var driverService = ChromeDriverService.CreateDefaultService();

            var options = new ChromeOptions();

            options.AddExcludedArgument("enable-automation");

            options.AddArgument("disable-gpu");
            options.AddArgument("disable-software-rasterizer");
            options.AddArgument("no-sandbox");

            // options.AddArgument("headless");

            var chromeDriverService = ChromeDriverService.CreateDefaultService();
            chromeDriverService.HideCommandPromptWindow = true;
            chromeDriverService.SuppressInitialDiagnosticInformation = true;

            _driver = new ChromeDriver(options);

            _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(2);
        }

        public string Get(string uri)
        {
            _driver.Url = uri;
            try
            {
                return _driver.FindElement(By.XPath("/html/body/pre")).Text;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}