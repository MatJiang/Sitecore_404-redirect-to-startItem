namespace SitecoreCustom.Extension.Override
{
    using Sitecore.Abstractions;
    using Sitecore.Data.Items;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

    public class ItemNotfoundHandler : global::Sitecore.Pipelines.HttpRequest.ExecuteRequest
    {
        /// <summary>
        /// Base Link Manager
        /// </summary>
        private readonly BaseLinkManager _baseLinkManager;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="baseSiteManager"></param>
        /// <param name="baseItemManager"></param>
        /// <param name="baseLinkManager"></param>
        public ItemNotfoundHandler(BaseSiteManager baseSiteManager, BaseItemManager baseItemManager, BaseLinkManager baseLinkManager) : base(baseSiteManager, baseItemManager)
        {
            _baseLinkManager = baseLinkManager;
        }

        /// <summary>
        /// Sites Setting
        /// </summary>
        private readonly List<string> _sites = new List<string>();
        public List<string> sites{get{return this._sites;}}

        /// <summary>
        /// enable log
        /// </summary>
        public bool EnableLog { get; set; }

        /// <summary>
        /// Redirect On ItemNotFound
        /// </summary>
        /// <param name="url"></param>
        protected override void RedirectOnItemNotFound(string url)
        {
            var context = System.Web.HttpContext.Current;

            var settingSite = sites.Where(site => site.Equals(Sitecore.Context.GetSiteName(), StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
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
                    Sitecore.Web.WebUtil.Redirect(_baseLinkManager.GetItemUrl(rootPage));
                }
                else
                    base.RedirectOnItemNotFound(url);
            }
            else
                base.RedirectOnItemNotFound(url);
        }

        /// <summary>
        /// log
        /// </summary>
        /// <param name="oriUrl"></param>
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
