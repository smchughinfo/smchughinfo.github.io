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
            //ScrapeTranscripts();
            //ParseTranscripts();
            CreateHeaders();
        }

        static void ScrapeTranscripts()
        {
            driver = new FirefoxDriver();

            var videoIds = GetVideosIds();
            videoIds = videoIds.Where(id => TranscriptExists(id) == false).ToList();

            foreach (var videoId in videoIds)
            {
                GetVideoTranscript(videoId);
            }
        }

        static List<string> GetExistingVideoIds()
        {
            var videoIds = Directory.GetFiles(transcriptDirectory).Select(p => Path.GetFileName(p)).ToList();
            videoIds = videoIds.Where(v => v.EndsWith("-timestamps.txt")).ToList(); // dont get the json files
            videoIds = videoIds.Select(v => v.Replace("-timestamps.txt", "")).ToList();

            return videoIds;
        }

        static void ParseTranscripts()
        {
            var videoIds = GetExistingVideoIds();

            foreach(var videoId in videoIds)
            {
                CreateJsonFile(videoId);
            }
        }

        static string getMetadataPath(string videoId)
        {
            return Path.Combine(transcriptDirectory, $"{videoId}-metadata.txt");
        }

        static string getHeadersPath()
        {
            return Path.Combine(transcriptDirectory, $"headers.js");
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
            return Path.Combine(transcriptDirectory, $"{videoId}-json.js");
        }

        static void CreateJsonFile(string videoId)
        {
            var jsonPath = getJsonPath(videoId);
            var o = CreateVideoJson(videoId);

            File.WriteAllText(jsonPath, "var transcript = " + o.ToString());
        }

        static JObject CreateVideoJson(string videoId, bool headersOnly = false)
        {
            var timestampsPath = getTimestampPath(videoId);
            var transcriptsPath = getTranscriptPath(videoId);
            var metadataPath = getMetadataPath(videoId);

            var timestamps = File.ReadAllLines(timestampsPath);
            var transcripts = File.ReadAllLines(transcriptsPath);
            var metadata = File.ReadAllLines(metadataPath);

            var title = metadata[0];
            var dateString = metadata[1];
            var date = GetDate(dateString);

            JObject o = JObject.FromObject(new
            {
                title = title,
                dateString = dateString,
                date = date,
                timestamps = headersOnly ? null : timestamps.ToList(),
                transcripts = headersOnly ? null : transcripts.ToList()
            });

            return o;
        }

        static void CreateHeaders()
        {
            var videoIds = GetExistingVideoIds();

            var headers = new List<JObject>();
            foreach (var videoId in videoIds)
            {
                var o = CreateVideoJson(videoId, true);
                headers.Add(o);
            }

            headers.Sort((JObject j1, JObject j2) =>
            {
                var dateTime1 = (DateTime)j1["date"];
                var dateTime2 = (DateTime)j2["date"];

                return dateTime1.CompareTo(dateTime2);
            });

            headers.Reverse();

            JArray headersArray = new JArray(); 
            foreach(var header in headers)
            {
                headersArray.Add(header);
            }

            var headersPath = getHeadersPath();
            File.WriteAllText(headersPath, $"var headers = {headersArray.ToString()}");
        }

        static DateTime GetDate(string dateString)
        {
            var date = dateString.Replace("•", "");
            date = date.Replace("Streamed live on", "");
            date = date.Replace(",", "");

            var dateComponents = date.Trim().Split(' ').Select(c => c.Trim().ToLower()).ToList();
            var month = dateComponents[0];
            var day = dateComponents[1];
            var year = dateComponents[2];

            var months = new string[] { "jan", "feb", "mar", "apr", "may", "jun", "jul", "aug", "sep", "oct", "nov", "dec" };
            month = (Array.IndexOf(months, month) + 1).ToString();

            return new DateTime(Convert.ToInt32(year), Convert.ToInt32(month), Convert.ToInt32(day));
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

            // now do metadata...
            var title = driver.FindElementByCssSelector("h1.title.ytd-video-primary-info-renderer").Text;
            var date = driver.FindElementByCssSelector("#date.ytd-video-primary-info-renderer").Text;
            var metadata = new List<string> { title, date };
            var metadataPath = getMetadataPath(videoId);
            File.WriteAllLines(metadataPath, metadata);
        }

    }
}
