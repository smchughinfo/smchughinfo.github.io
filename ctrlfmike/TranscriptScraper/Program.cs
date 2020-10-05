using Newtonsoft.Json.Linq;
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
        static FirefoxDriver driver;
        static string transcriptDirectory = @"C:\Users\Sean\Desktop\smchughinfo.github.io\ctrlfmike\TranscriptDirectory\";

        static void Main(string[] args)
        {
            driver = new FirefoxDriver();

            while (true)
            {
                try
                {
                    ScrapeTranscripts();
                }
                catch
                {

                }
            }
            
            //ParseTranscripts();
        }

        static void ScrapeTranscripts()
        {

            var videoIds = GetVideosIds();
            videoIds = videoIds.Where(id => TranscriptExists(id) == false).ToList();

            foreach (var videoId in videoIds)
            {
                GetVideoTranscript(videoId);
            }
        }

        static void ParseTranscripts()
        {
            var videoIds = Directory.GetFiles(transcriptDirectory).Select(p => Path.GetFileName(p)).ToList();
            videoIds = videoIds.Where(v => v.EndsWith(".txt")).ToList(); // dont get the json files
            videoIds = videoIds.Select(v => v.Replace("-timestamps.txt", "")).ToList();
            videoIds = videoIds.Select(v => v.Replace("-transcripts.txt", "")).ToList();

            foreach(var videoId in videoIds)
            {
                CreateJsonFile(videoId);
            }
        }

        static string getTimestampPath(string videoId)
        {
            return Path.Combine(transcriptDirectory, $"{videoId}-timestamps.txt");
        }

        static string getTranscriptPath(string videoId)
        {
            return Path.Combine(transcriptDirectory, $"{videoId}-transcripts.txt");
        }

        static string getJsonPath(string videoId)
        {
            return Path.Combine(transcriptDirectory, $"{videoId}-json.json");
        }

        static void CreateJsonFile(string videoId)
        {
            var timestampsPath = getTimestampPath(videoId);
            var transcriptsPath = getTranscriptPath(videoId);
            var jsonPath = getJsonPath(videoId);

            var timestamps = File.ReadAllLines(timestampsPath);
            var transcripts = File.ReadAllLines(transcriptsPath);

            JObject o = JObject.FromObject(new
            {
                timestamps = timestamps.ToList(),
                transcripts = transcripts.ToList()
            });


            File.WriteAllText(jsonPath, o.ToString());
        } 

        static List<string> GetVideosIds()
        {
            driver.Url = "https://www.youtube.com/channel/UCporaXCeaOJgZKz7y3C0zbg/videos";

            Thread.Sleep(10 * 1000);
            var videoLinks = driver.FindElementsByCssSelector("ytd-grid-video-renderer a[href^='/watch?v=']");

            var ids = videoLinks.Select(l => l.GetAttribute("href")).ToList();
            ids = ids.Select(id => id.Replace("https://www.youtube.com/watch?v=", "")).ToList();

            return ids;
        }

        static bool TranscriptExists(string videoId)
        {
            var timestampsPath = getTimestampPath(videoId);
            var transcriptsPath = getTranscriptPath(videoId);

            var timestampsExist = File.Exists(timestampsPath);
            var transcriptsExist = File.Exists(transcriptsPath);

            return timestampsExist && transcriptsExist;
        }

        static void GetVideoTranscript(string videoId)
        {
            driver.Url = $"https://www.youtube.com/watch?v={videoId}";

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

            var timestampsPath = getTimestampPath(videoId);
            var transcriptsPath = getTranscriptPath(videoId);

            File.WriteAllLines(timestampsPath, timestamps);
            File.WriteAllLines(transcriptsPath, transcripts);
        }

    }
}
