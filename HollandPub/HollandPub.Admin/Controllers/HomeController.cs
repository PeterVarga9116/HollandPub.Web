using Facebook;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Web.Mvc;

namespace HollandPub.Admin.Controllers
{
    public class HomeController : Controller
    {
        private string fbPageId = "1814843152063603";
        private string fbPageName = "QNA";
        private string fbAppId = "1854287538143996";
        private string fbAppSecret = "7faf7f184f26d7b613a12961292aacc3";
        private string scope = "manage_pages,publish_pages,publish_actions,user_posts";
        private string accessToken = string.Empty;

        private string newLine = "\r\n\r\n\r\n\r\n";

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult PostToFacebook()
        {
            var token = CheckAuth();

            var menu = Menu.New();
            menu.Date = DateTime.Now;
            menu.Description = "\r\n\r\n\r\n\r\nOkrem uvedených jedál je v ponuke aj bravčový / kurací rezeň alebo vyprážaný syr + príloha za 4, 20€ V cene jedla je aj dezert.\r\n\r\nJedlo sa podáva formou minutiek.";
            menu.AddItem(new MenuItem() { Amount = new decimal(4.2), Name = "1. Domáca živánska v alobale", Description = "(karé, zemiaky, klobása, slanina)" });
            menu.AddItem(new MenuItem() { Amount = new decimal(4.2), Name = "2. Cestoviny s tuniakom, A Peperoncino", Description = "" });
            menu.AddItem(new MenuItem() { Amount = new decimal(4.2), Name = "3. Jelení guláš s domácou žemľovou knedľou", Description = "(karé, zemiaky, klobása, slanina)" });

            var sb = new StringBuilder();
            sb.AppendFormat("{0} - {1}", menu.Date.ToString("dd.MM.yyyy"));
            sb.Append(newLine);

            PostMenuToFacebook(token, menu);
            return Json(string.Empty, JsonRequestBehavior.AllowGet);
        }

        public void PostMenuToFacebook(string pageAccessToken, Menu menu)
        {
            var clientx = new FacebookClient(pageAccessToken);
            clientx.Post("/" + fbPageId + "/feed",
                new
                {
                    message = "12.4.2017 - streda\r\n\r\n\r\n\r\nTekvicová krémová polievka<center></center> 1.Domáca živánska v alobale(karé, zemiaky, klobása, slanina) <center></center>2.Cestoviny s tuniakom, A Peperoncino, <center></center>3.Jelení guláš s domácou žemľovou knedľou <center></center><center></center>Okrem uvedených jedál je v ponuke aj bravčový / kurací rezeň alebo vyprážaný syr + príloha za 4, 20€ V cene jedla je aj dezert. <center></center>Jedlo sa podáva formou minutiek."
                });
        }

        private string CheckAuth()
        {
            if (Request["code"] == null)
            {
                Response.Redirect(string.Format("https://graph.facebook.com/oauth/authorize?client_id={0}&redirect_uri={1}&scope={2}", fbAppId, Request.Url.AbsoluteUri, scope));
                return string.Empty;
            }
            else
            {
                Dictionary<string, string> tokens = new Dictionary<string, string>();
                string url = string.Format("https://graph.facebook.com/oauth/access_token?client_id={0}&redirect_uri={1}&scope={2}&code={3}&client_secret={4}", fbAppId, Request.Url.AbsoluteUri, scope, Request["code"].ToString(), fbAppSecret);
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;

                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    StreamReader reader = new StreamReader(response.GetResponseStream());

                    string vals = reader.ReadToEnd();

                    JObject json = JObject.Parse(vals);

                    accessToken = json["access_token"].ToString();
                }

                var client = new FacebookClient(accessToken);

                string pageAccessToken = "";
                JsonObject jsonResponse = client.Get("me/accounts") as JsonObject;
                foreach (var account in (JsonArray)jsonResponse["data"])
                {
                    string accountName = (string)(((JsonObject)account)["name"]);

                    if (accountName == fbPageName)
                    {
                        pageAccessToken = (string)(((JsonObject)account)["access_token"]);
                        break;
                    }
                }


                return pageAccessToken;

            }
        }
    }

    public class Menu
    {
        public static Menu New()
        {
            var menu = new Menu()
            {
                Items = new List<MenuItem>(),
                Id = Guid.NewGuid()
            };

            return menu;
        }

        private CultureInfo cultureInfo { get { return new CultureInfo("sk-SK"); } }

        public Guid Id { get; set; }

        public DateTime Date { get; set; }

        public string Description { get; set; }

        public string DateDisplayName
        {
            get
            {
                return string.Format("{0} - {1}", Date.ToString("dd.MM.yyyy"), cultureInfo.DateTimeFormat.GetDayName(Date.DayOfWeek));
            }
        }

        public List<MenuItem> Items { get; set; }

        public void AddItem(MenuItem item)
        {
            Items.Add(item);
        }
    }

    public class MenuItem
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public decimal Amount { get; set; }
    }

}