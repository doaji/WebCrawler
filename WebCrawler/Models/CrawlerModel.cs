using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace WebCrawler.Models
{
    public class CrawlerModel
    {
        /// <summary>
        /// Website address
        /// </summary>
        public string SiteUrl { get; set; }
        /// <summary>
        /// Full text downloaded from the given website
        /// </summary>
        public string SiteText { get; set; }

        /// <summary>
        /// Words downloaded from the given website and the number of occurences
        /// </summary>
        public Dictionary<string, int> SiteWordCount { get; set; }
        /// <summary>
        /// Collection of image links downloaded from the givenn website
        /// </summary>
        public List<string> SiteImages { get; set; }

        public CrawlerModel(string url)
        {
            try
            {
                string webpagecontent = DownloadWebPageContent(url);

                HtmlDocument Htmldoc = new HtmlDocument();
                Htmldoc.LoadHtml(webpagecontent);
                if (Htmldoc.DocumentNode!=null)
                {
                    RemoveUnwantedTags(Htmldoc);
                    SiteUrl = url;
                    SiteText = Htmldoc.DocumentNode.InnerText;
                    SiteWordCount = GetSiteWordCount(Htmldoc);
                    SiteImages = GetSiteImages(Htmldoc, url);
                }
                else
                {
                    throw new Exception("Invalid HTML Document");
                }

            }
            catch (Exception ex)
            {

                throw new Exception("Error processing url", ex);
            }
        }

        /// <summary>
        /// This strips out comments, script and style tags
        /// </summary>
        /// <param name="htmldoc"></param>
        private void RemoveUnwantedTags(HtmlDocument htmldoc)
        {
            if (htmldoc != null)
            {
                // remove script, comments, and style sections
                htmldoc.DocumentNode?.DescendantsAndSelf().OfType<HtmlCommentNode>().ToList().ForEach(x => x.Remove());
                htmldoc.DocumentNode?.DescendantsAndSelf().Where(n => n.Name == "script" || n.Name == "style").ToList().ForEach(n => n.Remove());
            }
        }
        /// <summary>
        /// This method will attempt to get all images found from the img element, and test them if they can be hotlinked.
        /// </summary>
        /// <param name="htmldoc"></param>
        /// <param name="url"></param>
        /// <returns>This returns the src values of all img elements</returns>
        private List<string> GetSiteImages(HtmlDocument htmldoc, string url)
        {
            List<string> imageurls = new List<string>();
            if (htmldoc != null)
            {
                var images = htmldoc.DocumentNode.SelectNodes("//img");
                if (images!=null)
                {
                    HttpWebRequest myHttpWebRequest;
                    Uri uri = new Uri(url);
                    string domain = $"{uri.Scheme}://{uri.Host}";
                    string tempsrc = "";
                    foreach (var item in images)
                    {
                        // check if src is relative or absolute before adding to list
                        tempsrc = item.Attributes["src"]?.Value;
                        if (string.IsNullOrWhiteSpace(tempsrc))
                        {
                            continue;
                        }
                        if (!tempsrc.StartsWith("http://") && !tempsrc.StartsWith("https://"))
                        {
                            if (tempsrc.StartsWith("/"))
                            {
                                tempsrc = domain + tempsrc;
                            }
                            else
                            {
                                tempsrc = $"{domain}/{tempsrc}";
                            }
                        }

                        // test if image link is valid and can be hotlinked
                        try
                        {
                            myHttpWebRequest = (HttpWebRequest)WebRequest.Create(tempsrc);
                            using (HttpWebResponse myHttpWebResponse = (HttpWebResponse)myHttpWebRequest.GetResponse())
                            {
                                if (myHttpWebResponse.StatusCode == HttpStatusCode.OK && myHttpWebResponse.ContentType.ToLower().Contains("image"))
                                {
                                    imageurls.Add(tempsrc);
                                }
                            }


                        }
                        catch (Exception)
                        {

                            continue;
                        }

                    }
                }
            }
            return imageurls;
        }

        /// <summary>
        /// Get the total number of words on the page and group them based on their occurences
        /// </summary>
        /// <param name="htmldoc"></param>
        /// <returns></returns>
        private Dictionary<string, int> GetSiteWordCount(HtmlDocument htmldoc)
        {
            Dictionary<string, int> data = new Dictionary<string, int>();
            if (htmldoc != null)
            {
                string text = htmldoc.DocumentNode.InnerText.ToLower(); // normalize text content for accurate word count
                foreach (var item in text.Split(' ').Where(x => !string.IsNullOrWhiteSpace(x)))
                {
                    if (data.ContainsKey(item))
                    {
                        data[item]++;
                    }
                    else
                    {
                        data.Add(item, 1);
                    }
                }
            }

            return data;
        }

        /// <summary>
        /// Download content of the given web address
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private string DownloadWebPageContent(string url)
        {
            var webClient = new WebClient();

            return webClient.DownloadString(url);
        }

    }
}