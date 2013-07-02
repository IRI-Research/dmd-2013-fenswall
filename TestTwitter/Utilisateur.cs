using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;

using Google.GData.Photos;
using Google.Picasa;
using System.Threading;
using System.Diagnostics;

namespace TestTwitter
{
    class Utilisateur
    {
        private static int ID = 0;
        private Image Avatar;
        private List<Image> Favoris;
        private List<Tweet> FavoriTweet;

        public Utilisateur()
        {
            Favoris = new List<Image>();
            FavoriTweet = new List<Tweet>();
            correctIDIfNecessary();
        }
       
        public Utilisateur(Image Avatar)
        {
            Favoris = new List<Image>();
            FavoriTweet = new List<Tweet>();
            this.Avatar = Avatar;
            correctIDIfNecessary();
        }

        public void correctIDIfNecessary()
        {
           //ring path = @"D:\";//Example.txt";
            //File.CreateDirectory(path);
            /*if (!File.Exists(path))
            {
                File.Create(path);
                TextWriter tw = new StreamWriter(path);
                tw.WriteLine("The very first line!");
                tw.Close();
            }
            else if (File.Exists(path))
            {
                TextWriter tw = new StreamWriter(path, true);
                tw.WriteLine("The next line!");
                tw.Close();
            }*/

        }

        public void setAvatar(Image avatar)
        {
            this.Avatar = avatar;
        }
        public void indexPlus()
        {
           ID++;
        }

        public int getId()
        {
            return ID;
        }

        public ImageSource getAvatar()
        {
            return this.Avatar.Source;
        }

        public String getUserDirectory()
        {
            string path = System.IO.Directory.GetCurrentDirectory();
            if (path.EndsWith("\\bin\\Debug"))
            {
                path = path.Replace("\\bin\\Debug", "");
            }
            path += "\\Images\\" + ID;
            return path;
        }

        public void AddFavPhoto(Image img)
        {
            this.Favoris.Add(img);
        }

        public void AddFavTweet(Tweet Tw)
        {
            this.FavoriTweet.Add(Tw);

        }

        public String getAlbumName()
        {
            String idString = Convert.ToString(ID);
            while (idString.Length < 4)
                idString = "0" + idString;
            return idString;
        }



        public void picasend()
        {

            Debug.WriteLine("envoi sur picasa via un nouveau thread");
            //Thread t = new Thread(SendToPicasa);
            //t.Start();
        }
        public void SendToPicasa()
        {
            /* Configuration */
            string username = "fenswall.esilv";
            string password = "esilv2013";
            string nomAlbum = "Avatar";
            
            // images à poster
            Favoris.Insert(0, Avatar);
            List<string> imagePath=new List<string>();

            string directoryPath = System.IO.Directory.GetCurrentDirectory();
            if (directoryPath.EndsWith("\\bin\\Debug"))
            {
                directoryPath = directoryPath.Replace("\\bin\\Debug", "");
            }
            directoryPath += "\\Images\\Avatars";

            DirectoryInfo dir = new DirectoryInfo(directoryPath);
            //DirectoryInfo[] dirInfo = dir.GetDirectories();
            FileInfo[] info = dir.GetFiles("*.*");
            
            if (info.Length != ID)
                ID = info.Length;
            foreach (FileInfo f in info)
            {
                imagePath.Add(f.FullName);
            }
            /* urlAlbum sera le lien, l'image destQRCODE pour le QR code correspondant */
            string urlAlbum = "";

            /* Execution */

            PicasaService service = new PicasaService("exampleCo-exampleApp-1");
            service.setUserCredentials(username, password);

            AlbumEntry newEntry = new AlbumEntry();
            newEntry.Title.Text = nomAlbum;            
            newEntry.Summary.Text = "Votre album FensWall ESILV";
            AlbumAccessor ac = new AlbumAccessor(newEntry);            
            ac.Access = "public";
            Uri feedUri = new Uri(PicasaQuery.CreatePicasaUri(username));
            PicasaEntry createdEntry = (PicasaEntry)service.Insert(feedUri, newEntry);
            var id = createdEntry.Id;
           
            foreach(var l in createdEntry.Links)
            {
                if (!l.AbsoluteUri.Contains("user"))
                {
                    urlAlbum = l.AbsoluteUri;
                }
            }

            AlbumQuery query = new AlbumQuery(PicasaQuery.CreatePicasaUri(username));
            PicasaFeed feed = service.Query(query);
            var album = feed.Entries.Where(p => p.Title.Text == nomAlbum ).ToList();

            Album alb = new Album();
            alb.AtomEntry = album.First(); 


            Uri postUri = new Uri(PicasaQuery.CreatePicasaUri(username, alb.Id));

            foreach (var image in imagePath)
            {
                System.IO.FileInfo fileInfo = new System.IO.FileInfo(image);
                System.IO.FileStream fileStream = fileInfo.OpenRead();

                PicasaEntry entry = (PicasaEntry)service.Insert(postUri, fileStream, "image/jpeg", image);

                fileStream.Close();
            }
            Console.WriteLine(urlAlbum);

            ID++;
        }
    }


}
