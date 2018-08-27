using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebCrawler.Models;

namespace WebCrawler.Controllers
{
    public class CrawlerController : Controller
    {
        // GET: Crawler
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                try
                {
                    var model = new CrawlerModel(url);
                    return View(model);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("Invalid Webpage", "Could not process the webpage you entered - " + ex.Message);
                    return View();
                }
            }
            else
            {
                return View();
            }
        }
    }
}