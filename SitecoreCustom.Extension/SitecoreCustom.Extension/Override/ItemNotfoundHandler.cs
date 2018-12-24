namespace SitecoreCustom.Extension.Override
{
    using Sitecore.Data.Items;
    using Sitecore.Links;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

    public class ItemNotfoundHandler : Sitecore.Pipelines.HttpRequest.ExecuteRequest
    {
        public string Sites { get; set; }
        public bool EnableLog { get; set; }
        private static string[] _excuteList;

        protected override void RedirectOnItemNotFound(string url)
        {
            if (!string.IsNullOrEmpty(Sites) && _excuteList == null)
                _excuteList = Sites.Split(',').Select(s => s.Trim()).ToArray();

            var context = System.Web.HttpContext.Current;

            var settingSite = _excuteList.Where(site => site.Equals(Sitecore.Context.GetSiteName(), StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
            if (settingSite != null && Sitecore.Context.Database != null)
            {
                Item rootPage = Sitecore.Context.Database.GetItem(Sitecore.Context.Site.StartPath);
                if (rootPage != null && rootPage.Versions.Count > 0)
                {
                    try
                    {
                        if (EnableLog)
                            Log(url);
                    }
                    catch (Exception ex)
                    {
                        Sitecore.Diagnostics.Log.Debug(string.Format("ItemNotfoundHandler Logger Error,Message：{0}", ex.Message));
                    }
                    Sitecore.Web.WebUtil.Redirect(LinkManager.GetItemUrl(rootPage));
                }
                else
                    base.RedirectOnItemNotFound(url);
            }
            else
                base.RedirectOnItemNotFound(url);
        }

        private static void Log(string oriUrl)
        {
            var logger = Sitecore.Diagnostics.LoggerFactory.GetLogger("NotfoundLoger");
            var decodeString = System.Web.HttpUtility.UrlDecode(oriUrl);
            int idx = decodeString.IndexOf('?');
            string query = idx >= 0 ? decodeString.Substring(idx) : "";
            var keys = HttpUtility.ParseQueryString(query);
            IDictionary<string, string> dict = new Dictionary<string, string>();
            foreach (var k in keys.AllKeys)
            {
                if (!dict.ContainsKey(k))
                    dict.Add(k, keys[k]);
            }
            if (dict.Any())
                logger.Info(Newtonsoft.Json.JsonConvert.SerializeObject(dict));
        }
    }
}
