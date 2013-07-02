using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Net;

namespace TestTwitter
{
    class Tweet
    {
        
        public Tweet(String Text)
        {
            this.Title = Text;
        }

        public string Id { get; set; }
        public DateTime Published { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Link { get; set; }
        //public Author Author { get; set; }
        public string Image { get; set; }

        public String GetUrlPhoto()
        {

            string[] Liste = Title.Split(' ');

            for (int i = 0; i < Liste.Length; i++)
            {
                if (Liste[i].IndexOf("http://") != -1)
                {
                    return Liste[i];
                }
            }
            return null;
        }


        public string GetPicture(string twitterUri)
        {
            using (var webClient = new WebClient())
            {
                string html = webClient.DownloadString(twitterUri);
                int imgIndex = html.IndexOf("<img class=");
                if (imgIndex != -1)
                {
                    int srcStartIndex = html.IndexOf("https://pbs.", imgIndex);
                    int srcEndIndex = html.IndexOf("\"", srcStartIndex);
                    string imgSrc = html.Substring(srcStartIndex, srcEndIndex - srcStartIndex);
                    this.Image = imgSrc;
                    return imgSrc;
                }
                return null;
            }
        }
    }
}

