using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TranscriptScraper
{
    class Program
    {
        static void Main(string[] args)
        {
            var driver = new FirefoxDriver();
            driver.Url = "https://www.youtube.com/watch?v=ErM8ME3_HS4";

            Thread.Sleep(10 * 1000);

            var threeDotsButton = driver.FindElementByCssSelector("ytd-menu-renderer.ytd-video-primary-info-renderer .dropdown-trigger");
            threeDotsButton.Click();

            Thread.Sleep(5 * 1000);
            var xp = driver.FindElementByXPath("//*[contains(text(), 'Open transcript')]");
            xp.Click();

            Thread.Sleep(10 * 1000);
            var transcriptContainer = driver.FindElementByCssSelector("ytd-transcript-body-renderer");
            var timestampsHTML = transcriptContainer.FindElements(By.CssSelector(".cue-group > :nth-child(1)"));
            var transcriptsHTML = transcriptContainer.FindElements(By.CssSelector(".cue-group > :nth-child(2)"));

            var timestamps = timestampsHTML.Select(t => t.GetAttribute("textContent").Trim()).ToList();
            var transcripts = transcriptsHTML.Select(t => t.GetAttribute("textContent").Trim()).ToList();

            File.WriteAllLines(@"c:\users\sean\desktop\ctrlfmike\timestamps.txt", timestamps);
            File.WriteAllLines(@"c:\users\sean\desktop\ctrlfmike\transcripts.txt", transcripts);
        }
    }
}
