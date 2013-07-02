using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Xml.Linq;
using System.Diagnostics;
using System.Windows.Media;
using Twitterizer;


namespace TestTwitter
{
    class RechercheTwitter
    {
        public List<Tweet> listTweet;
        private String HashTag;
        private Boolean WorkDone;

        public RechercheTwitter(String HashTag)
        {
            
            this.HashTag = "#"+HashTag;
            this.listTweet = new List<Tweet>();
            this.WorkDone = false;
        }

        private void getTweet()
        {


            OAuthTokens token = new OAuthTokens();
            token.AccessToken = "1377532153-7CiBZQfskgzBYL9uShjQDCYGCAfhNok8NIlpglT";
            token.AccessTokenSecret = "YAwHBFhGJgyI1dTjUIzzU4grQcNfUdgtKw7S8yCvY8";
            token.ConsumerKey = "ZYrW61HQNnWeSw78vELw";
            token.ConsumerSecret = "a3oVz91h0oSLOQ6V0qzkHQAs7bL5iEt6gFzrYhWGCiM";
            SearchOptions sc = new SearchOptions();
            sc.NumberPerPage = 100;
            

            TwitterResponse<TwitterSearchResultCollection> Tweets = TwitterSearch.Search(token, HashTag, sc);
            foreach (var tweet in Tweets.ResponseObject)
            {
                Tweet twt = new Tweet(tweet.Text);
                listTweet.Add(twt);
                String urlimg = twt.GetUrlPhoto();
                if (urlimg != null)
                {
                    try
                    {
                        twt.GetPicture(urlimg);
                    }
                    catch (Exception exe)
                    {
                    }
                }
            }
        }
        public bool isFinished()
        {
            return this.WorkDone;
        }

        public void getUri()
        {
            this.getTweet();
            Debug.WriteLine("Fin thread");

            this.WorkDone = true;

        }

        public void clear()
        {
            this.WorkDone = false;
            this.listTweet.Clear();
        }
    }      
}
