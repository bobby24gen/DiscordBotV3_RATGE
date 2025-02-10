using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBotV3
{
    public class HttpHelper
    {
        Uri baseUri = new Uri ("http://anecdotica.ru/item/");

        public string? Anek()
        {
            Random rng = new Random();

            HtmlWeb htmlWeb = new HtmlWeb ();
            HtmlDocument doc = htmlWeb.Load(baseUri + rng.Next(1,30000).ToString());
            HtmlNode anek = doc.GetElementbyId("description");

            if (anek != null)
            {
                return anek.InnerText;
            }
            return null;
        }

    }
}
