using System.Collections.Generic;
using OpenQA.Selenium.Chrome;

namespace CowinChecker
{
    public interface IMessageService
    {
        bool IsReady { get; set; }
        void Send(string to, string message);
        void Send(string to, IEnumerable<string> messages);
    }
}