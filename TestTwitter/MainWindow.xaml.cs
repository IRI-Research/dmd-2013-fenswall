using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;
using Microsoft.Samples.Kinect.WpfViewers;
using MvtKinect;
using System.IO;
using System.Threading;
using System.Timers;
using System.Runtime.CompilerServices;
using System.Collections;


namespace TestTwitter
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {



        //differentes étapes de l'appli
        private static int SELECTIONFOND = 0;
        private static int FONDSELECTIONNER = 1;
        private static int DECOUPEAVATAR = 2;
        private static int SELECTIONPHOTO = 3;
        private static int FIN = 4;

        private static int AFFICHAGEGRID = 5;

        private static string AVATAR_PATH = "";

        //private int uniqueID = 0;
        private int imageNameNb = 0;

        private int Etape;


        // niveaux de zoom dans l'appli
        private static int ZOOMMIN = 0; // lors du zoom minimum ==> 53 images
        private static int ZOOMMED = 1;  // NIveau de soom medium ==> 6 images
        private static int ZOOMMAX = 2;  // Zoom maximum ==> 1 image

        private int Zoom;


        private Image[] tabImages = new Image[6];

        //Utilisateur
        private int id = 0; //  ==> pour incrementation
        private Utilisateur user;



        //image A utiliser lors du detourage de l'avatar avant de decouper
        private Image imageFond;




        //liste de tous les tweets recuperés
        List<Tweet> lstTweet = new List<Tweet>();
        //liste des motif pour la premiere photo
        private List<ImageSource> LstMotif;
        //liste d'image source recuperer dans les tweets
        private List<ImageSource> listImg = new List<ImageSource>();
        //liste d'image pour le chrono
        private List<ImageSource> listImgChrono = new List<ImageSource>();
        
        private List<Image> avatarImgs = new List<Image>();
       

        int CurrentIndex = 0;




        private System.Timers.Timer timer;
        private int tickTimer = 0;


        // pour la kinect
        private DepthImagePixel[] depthPixels;
        private const DepthImageFormat DepthFormat = DepthImageFormat.Resolution640x480Fps30;
        private const ColorImageFormat ColorFormat = ColorImageFormat.RgbResolution640x480Fps30;
        private readonly KinectSensorChooser sensorChooser = new KinectSensorChooser();
        private KinectSensor sensor;


        // skeleton gesture recognizer
        private GestureController gestureController;
        private const float SkeletonMaxX = 0.60f;
        private const float SkeletonMaxY = 0.40f;
        private Skeleton[] skeletons = new Skeleton[0];



        //thread de recuperation des tweets
        private Thread t;
        private RechercheTwitter recup;

        private bool refreshDone;



        string[] files;






        public MainWindow()
        {
            InitializeComponent();

          

            Init();
        }

        #region Initialisation
        public void ActivationKinect()
            {
                // initialise la kinect
            KinectSensorManager = new KinectSensorManager();
            KinectSensorManager.KinectSensorChanged += this.KinectSensorChanged;
            sensorChooser.Start();


            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    this.sensor = potentialSensor;
                    break;
                }
            }
            
            // bind chooser's sensor value to the local sensor manager
            var kinectSensorBinding = new Binding("Kinect") { Source = this.sensorChooser };
            BindingOperations.SetBinding(this.KinectSensorManager, KinectSensorManager.KinectSensorProperty, kinectSensorBinding);

            sensor.AllFramesReady += this.SensorAllFramesReady;
            depthPixels = new DepthImagePixel[640 * 480];
        }

        public void Init()
        {
            Etape = 0;
            Zoom = 0;


            //Border myBorder = new Border();
            //myBorder.BorderBrush = Brushes.DarkBlue;
            GridMaxPhoto.Background = Brushes.Aquamarine;
            GridMaxPhoto.Visibility = Visibility.Hidden;



            //recup = new RechercheTwitter("DataRoomIri");
            ////thread de recuperation des tweets
            //t = new Thread(recup.getUri);
            //t.Name = "thread recuperation tweet";
            //t.Start();


            //active la kinect
            ActivationKinect();


            refreshDone = false;

            LstMotif = new List<ImageSource>();
            
            tabImages[0] = ImageAffichee1;
            tabImages[1] = ImageAffichee2;
            tabImages[2] = ImageAffichee3;
            tabImages[3] = ImageAffichee4;
            tabImages[4] = ImageAffichee5;
            tabImages[5] = ImageAffichee6;


            this.imageFond = new Image();



            initListeMotif();

            listImgChrono.Add(new BitmapImage(new Uri("/Images/Chrono1", UriKind.Relative)));
            listImgChrono.Add(new BitmapImage(new Uri("/Images/Chrono2", UriKind.Relative)));
            listImgChrono.Add(new BitmapImage(new Uri("/Images/Chrono3", UriKind.Relative)));
            listImgChrono.Add(new BitmapImage(new Uri("/Images/Chrono4", UriKind.Relative)));
            listImgChrono.Add(new BitmapImage(new Uri("/Images/Chrono5", UriKind.Relative)));



            ImageAfficher.Source = LstMotif[0];
            createAvatarsFolder();
        }


        private void initListeMotif()
        {
            LstMotif.Clear();
            try
            {

                files = Directory.GetFiles("../../Images/");
                Array.Sort(files, new Melangeur());
                foreach (var str in files)
                {
                    LstMotif.Add(new BitmapImage(new Uri(str, UriKind.Relative)));
                }
            }
            catch (Exception exe)
            {
                Debug.WriteLine(exe.ToString());
            }

        }

        public void createAvatarsFolder() 
        {
            string path = System.IO.Directory.GetCurrentDirectory();
            if (path.EndsWith("\\bin\\Debug"))
            {
                path = path.Replace("\\bin\\Debug", "");
            }
            path += "\\Images\\Avatars";
            Directory.CreateDirectory(path);
            AVATAR_PATH = path;
        }
  
        #endregion Initialisation                       

        #region Kinect Discovery & Setup

        private void KinectSensorChanged(object sender, KinectSensorManagerEventArgs<KinectSensor> args)
        {
            if (null != args.OldValue)
                UninitializeKinectServices(args.OldValue);

            if (null != args.NewValue)
                InitializeKinectServices(KinectSensorManager, args.NewValue);
        }

        /// <summary>
        /// Kinect enabled apps should customize which Kinect services it initializes here.
        /// </summary>
        /// <param name="kinectSensorManager"></param>
        /// <param name="sensor"></param>
        private void InitializeKinectServices(KinectSensorManager kinectSensorManager, KinectSensor sensor)
        {
            // Application should enable all streams first.

            // configure the color stream
            kinectSensorManager.ColorFormat = ColorImageFormat.RgbResolution640x480Fps30;
            kinectSensorManager.ColorStreamEnabled = true;

            // configure the depth stream
            kinectSensorManager.DepthStreamEnabled = true;

            kinectSensorManager.TransformSmoothParameters =
                new TransformSmoothParameters
                {
                    Smoothing = 0.5f,
                    Correction = 0.5f,
                    Prediction = 0.5f,
                    JitterRadius = 0.05f,
                    MaxDeviationRadius = 0.04f
                };

            // configure the skeleton stream
            sensor.SkeletonFrameReady += OnSkeletonFrameReady;
            kinectSensorManager.SkeletonStreamEnabled = true;

            // initialize the gesture recognizer
            gestureController = new GestureController();
            gestureController.GestureRecognized += OnGestureRecognized;

            kinectSensorManager.KinectSensorEnabled = true;

            if (!kinectSensorManager.KinectSensorAppConflict)
            {
                // addition configuration, as needed
            }
        }

        /// <summary>
        /// Kinect enabled apps should uninitialize all Kinect services that were initialized in InitializeKinectServices() here.
        /// </summary>
        /// <param name="sensor"></param>
        private void UninitializeKinectServices(KinectSensor sensor)
        {

        }

        #endregion Kinect Discovery & Setup

        #region Properties

        public static readonly DependencyProperty KinectSensorManagerProperty =
            DependencyProperty.Register(
                "KinectSensorManager",
                typeof(KinectSensorManager),
                typeof(MainWindow),
                new PropertyMetadata(null));

        public KinectSensorManager KinectSensorManager
        {
            get { return (KinectSensorManager)GetValue(KinectSensorManagerProperty); }
            set { SetValue(KinectSensorManagerProperty, value); }
        }

        /// <summary>
        /// Gets or sets the last recognized gesture.
        /// </summary>
        private string _gesture;


        #endregion Properties

        #region Events

        /// <summary>
        /// Event implementing INotifyPropertyChanged interface.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        private ColorImagePoint[] colorCoordinates = new ColorImagePoint[640 * 480];

        #endregion Events

        #region Event Handlers

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Gesture event arguments.</param>
        private void OnGestureRecognized(object sender, GestureEventArgs e)
        {
            Debug.WriteLine(e.GestureType);

            switch (e.GestureType)
            {
                
                case GestureType.SwipeLeft:
                    if (Etape == SELECTIONFOND || Etape==AFFICHAGEGRID)
                    {
                        if (ImageAfficher.Visibility != Visibility.Visible)
                        {
                            ImageAfficher.Visibility = Visibility.Visible;
                            GridMaxPhoto.Visibility = Visibility.Hidden;
                        }
                        PreviousMotif();
                        Etape = SELECTIONFOND;
                    }
                    break;
                case GestureType.SwipeRight:
                    if (Etape == SELECTIONFOND || Etape==AFFICHAGEGRID)
                    {
                        if (ImageAfficher.Visibility != Visibility.Visible)
                        {
                            ImageAfficher.Visibility = Visibility.Visible;
                            GridMaxPhoto.Visibility = Visibility.Hidden;
                        }
                        NextMotif();
                        Etape = SELECTIONFOND;
                    }
                  /*  else
                    {

                        if (Etape == AFFICHAGEGRID)
                        {
                            Etape = SELECTIONFOND;
                            NextMotif();
                        }
                        if (Etape == SELECTIONPHOTO)
                        {
                            if (Zoom == ZOOMMAX)
                            {
                                NextPhoto();
                            }
                            if (Zoom == ZOOMMED)
                            {
                                Next6Photos();
                            }
                            
                        }
                      
                    }*/
                    break;
                /*case GestureType.ZoomIn:
                    if (Etape == SELECTIONPHOTO)
                    {
                        if (Zoom < ZOOMMAX)
                        {
                            Zoom++;
                            UpdateAffichage();
                        }
                    }

                    break;*/
              /*  case GestureType.ZoomOut:
                    if (Etape == SELECTIONPHOTO)
                    {
                        if (Zoom > ZOOMMIN)
                        {
                            Zoom--;
                            UpdateAffichage();
                        }
                    }
                    break;*/
                case GestureType.SwipeDownLeft:
                case GestureType.SwipeDown:
                     if (Etape == SELECTIONFOND)
                    {
                        Wait(5000);
                    }
                    else
                    {
                        if (Zoom == ZOOMMAX)
                        {
                            //AddToFrise();
                        }
                    }
                    break;
                //case GestureType.SwipeDown:
                //case GestureType.SwipeDownLeft:
                //    supprFrise();
                //    break;

                default:
                    break;
            
                /*case GestureType.SwipeUp:
                case GestureType.SwipeUpLeft:
                    if (Etape == SELECTIONFOND)
                    {
                        Wait(5000);
                    }
                    else
                    {
                        if (Zoom == ZOOMMAX)
                        {
                            //AddToFrise();
                        }
                    }
                    break;
                //case GestureType.SwipeDown:
                //case GestureType.SwipeDownLeft:
                //    supprFrise();
                //    break;

                default:
                    break;*/
            }
        }

        

        /// <summary>
        /// fonction de tracking du skelette d'une personne
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// 
        private void OnSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            if (Etape == SELECTIONPHOTO) ;
            {
                using (SkeletonFrame frame = e.OpenSkeletonFrame())
                {
                    if (frame == null)
                        return;

                    // resize the skeletons array if needed
                    if (skeletons.Length != frame.SkeletonArrayLength)
                        skeletons = new Skeleton[frame.SkeletonArrayLength];

                    // get the skeleton data
                    frame.CopySkeletonDataTo(skeletons);

                    foreach (Skeleton sd in skeletons)
                    {
                        // the first found/tracked skeleton moves the mouse cursor
                        if (sd.TrackingState == SkeletonTrackingState.Tracked)
                        {
                            // make sure both hands are tracked
                            if (sd.Joints[JointType.HandRight].TrackingState == JointTrackingState.Tracked)
                            {
                                var wristRight = sd.Joints[JointType.WristRight];
                                var x = wristRight.Position.X;
                                var y = wristRight.Position.Y;
                                Point p = new Point(x, y);
                                if (p.X > 1)
                                {

                                }
                                //Double xScaled = (rightHand.Position.X - leftShoulder.Position.X) / ((rightShoulder.Position.X - leftShoulder.Position.X) * 2) * SystemParameters.PrimaryScreenWidth;
                                //var scaledRightHand = wristRight.ScaleTo((int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight, SkeletonMaxX, SkeletonMaxY);

                                //var leftClick = CheckForClickHold(scaledRightHand);
                                //NativeMethods.SendMouseInput(cursorX, cursorY, (int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight, leftClick);

                                gestureController.UpdateAllGestures(sd);
                            }
                        }
                    }
                }
            }
        }



        private void SensorAllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            if (this.Etape == DECOUPEAVATAR || this.Etape == FONDSELECTIONNER)
            {
                //rawr - test
                //GridMaxPhoto.Visibility = Visibility.Hidden;
                //ImageAfficher.Visibility = Visibility.Visible;
                // in the middle of shutting down, so nothing to do
                if (null == this.sensor)
                {
                    return;
                }

                bool depthReceived = false;

                using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
                {
                    if (null != depthFrame)
                    {
                        // Copy the pixel data from the image to a temporary array
                        depthFrame.CopyDepthImagePixelDataTo(this.depthPixels);

                        depthReceived = true;
                    }
                }
                if (true == depthReceived)
                {

                    this.sensor.CoordinateMapper.MapDepthFrameToColorFrame(
                        DepthFormat,
                        this.depthPixels,
                        ColorFormat,
                        this.colorCoordinates);
                    //test inutile => problem with source not initialised correctly => check Wait and val source. 
                    if (this.Etape == DECOUPEAVATAR && imageFond!=null)
                    {
                        try
                        {
                            Uri test = new Uri(imageFond.Source.ToString());
                            BitmapImage bitmapTest = new BitmapImage(test);
                            ImageAfficher.Source = Detourage(bitmapTest);
                        }
                        catch (Exception g)
                        {
                            Debug.WriteLine(g.ToString());
                        }
                    }
                    else if(imageFond!=null)
                    {

                        Uri test = new Uri(imageFond.Source.ToString());
                        BitmapImage bitmapTest = new BitmapImage(test);
                        Image ImageAvatar = new Image();
                        user = new Utilisateur();

                        ImageAvatar.Source = Decoupage(bitmapTest);
                        searchPanel.Children.Add(ImageAvatar);
                        //NextPhoto();
                        this.Etape = SELECTIONPHOTO;
                        //this.user = new Utilisateur(id, ImageAvatar);
                        user.setAvatar(ImageAvatar);

                        //=> Keep the avatar visible 2 sec before hiding it and shozing the grid:
                        Thread.Sleep(2000);
                        ImageAfficher.Visibility = Visibility.Hidden;


                        //prolly shouldnt be here yet - rawr
                        GridMaxPhoto.Visibility = Visibility.Visible;
                        QuitAppli();
                       // initPatchworkPhoto();
                        return;

                    }
                }
            }
        }


        #endregion Event Handlers

        #region PhotoTweet

         [MethodImpl(MethodImplOptions.Synchronized)]
        private void PreviousPhoto()
        {
           // refreshUi();
            if (CurrentIndex == 0)
                CurrentIndex = listImg.Count - 1;
            else
                CurrentIndex -= 1;
            ImageAfficher.Source = (listImg[CurrentIndex]);
        }

         [MethodImpl(MethodImplOptions.Synchronized)]
        private void NextPhoto()
        {
            //refreshUi();
            if (CurrentIndex == listImg.Count - 1)
                CurrentIndex = 0;
            else
                CurrentIndex += 1;

            ImageAfficher.Source = (listImg[CurrentIndex]);


        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        private void Previous6Photos()
        {
            //refreshUi();
            for (int i = 0; i <= 5; i++)
            {
                if (CurrentIndex >= 0 && CurrentIndex <= listImg.Count - 1)
                {
                    tabImages[i].Source = (listImg[CurrentIndex]);
                    CurrentIndex--;
                }
                else
                {
                    CurrentIndex = listImg.Count - 1;
                    tabImages[i].Source = (listImg[CurrentIndex]);
                    CurrentIndex--;
                }
            }


        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        private void Next6Photos()
        {
            //refreshUi();
            for (int i = 0; i <= 5; i++)
            {
                if (CurrentIndex <= listImg.Count - 1 && CurrentIndex >= 0)
                {
                    tabImages[i].Source = (listImg[CurrentIndex]);
                    CurrentIndex++;
                }
                else
                {
                    CurrentIndex = 0;
                    tabImages[i].Source = (listImg[CurrentIndex]);
                    CurrentIndex++;
                }
            }


        }

        #endregion PhotoTweet


        #region Interface
        [MethodImpl(MethodImplOptions.Synchronized)]
        private void AddToFrise()
        {
            if (Etape == SELECTIONPHOTO)
            {
                if (this.Dispatcher.Thread != System.Threading.Thread.CurrentThread)
                {

                }
                else
                {
                    ImageSource src = ImageAfficher.Source;
                    Image imageFresque = new Image();
                    imageFresque.Source = src;
                    if (searchPanel.Children.Count > 7)
                    {
                        searchPanel.Children.RemoveAt(0);
                    }

                    user.AddFavPhoto(imageFresque);

                    //Save pic
                    saveImgAsPNG(imageFresque);
                    searchPanel.Children.Add(imageFresque);
                }
            }
            else
                SelectFondAvatar();
        }

        private void initPatchworkPhoto()
        {
            refreshUi();
            GridMaxPhoto.Children.Clear();
            for (int ligne = 0; ligne < GridMaxPhoto.RowDefinitions.Count; ligne++)
            {
                for (int colonne = 0; colonne < GridMaxPhoto.ColumnDefinitions.Count; colonne++)
                {
                    if (listImg.Count > (ligne * GridMaxPhoto.ColumnDefinitions.Count) + colonne)
                    {
                        Image img = new Image();
                        img.Source = listImg[(ligne * GridMaxPhoto.ColumnDefinitions.Count) + colonne];
                        GridMaxPhoto.Children.Add(img);
                        Grid.SetColumn(img, colonne);
                        Grid.SetRow(img, ligne);
                    }

                }
            }
            initPatchWorkBackGround(0,0);
        }

        private void initPatchWorkBackGround(int ligne, int colonne)
        {
            GridbackGround.Children.Clear();

            Border myBorder = new Border();
            myBorder.BorderBrush = Brushes.DarkBlue;
            myBorder.BorderThickness = new Thickness(5, 5, 5, 5);
            GridbackGround.Children.Add(myBorder);
            Grid.SetColumn(myBorder,colonne);
            Grid.SetRow(myBorder, ligne);
            

        }

        public void Initialiser6Photos()
        {
            //refreshUi();
            for (int i = 0; i <= 5; i++)
            {
                if (CurrentIndex <= listImg.Count - 1)
                {
                    tabImages[i].Source = listImg[CurrentIndex];
                    CurrentIndex++;
                }
                else
                {
                    CurrentIndex = 0;
                    tabImages[i].Source = listImg[CurrentIndex];
                    CurrentIndex++;
                }
            }

        }

        public void refreshUi()
        {
           // listImg.Clear();
            //empeche le refresh de l'interface si il n'y a plus de tweet a aller chercher
            if (!(recup.isFinished() && refreshDone))
            {

                //si le refresh a été fais quand le thread a finit sa recherche ou psa 
                if(recup.isFinished())
                    refreshDone = true;
                lstTweet = new List<Tweet>(recup.listTweet);

                // Pas de for each pour ne pas mettre en boucles les mêmes tweets
                for (int i = 0; i < recup.listTweet.Count; i++)
                {
                    if(i > listImg.Count)
                    {
                        if (recup.listTweet[i].Image != null)
                        {
                            Uri Photo = new Uri(recup.listTweet[i].Image);
                            if (Photo != null)
                            {

                                listImg.Add(new BitmapImage(Photo));
                            }
                        }

                    }
                }
            }
        }

        private void QuitAppli()
        {
            //tout les machins lors du changement de User
            initListeMotif();
            this.Etape = AFFICHAGEGRID;
            this.CurrentIndex = 0;

            //GridMaxPhoto.Visibility = Visibility.Hidden;
            //grid6Images.Visibility = Visibility.Hidden;

            //ImageAfficher.Visibility = Visibility.Visible;
            //GridMaxPhoto.Visibility = Visibility.Hidden;
            ImageAfficher.Source = user.getAvatar();
           // Thread.Sleep(3000);

            searchPanel.Children.Clear();

            //Affiche + ajout des photos + remet imageAfficher à hidden <=> changer le temps
            //ImageAfficher.Visibility = Visibility.Hidden;
            afficheGridAvatar();
            //Test rawr 
            //imageFond.Source = LstMotif[CurrentIndex]; ;
            


         
            lstTweet.Clear();

           
            user.picasend();
        }
        
        private void afficheGridAvatar()
        {
            //Test
            Image img = new Image();
            img.Source = user.getAvatar();
            //GridMaxPhoto.Children.Add(img);
            GridMaxPhoto.Visibility = Visibility.Visible;
            fillAvatarGrid();
        }

        //rawr
        private void fillAvatarGrid()
        {
            Image img = new Image();
            img.Source = user.getAvatar();
            avatarImgs.Insert(0, img);
            if (avatarImgs.Count > 54)
            {
                avatarImgs.RemoveAt(avatarImgs.Count - 1);
            }
            setGridPointers();
        }

        private void setGridPointers()
        {
            GridMaxPhoto.Children.Clear();
            for (int ligne = 0; ligne < GridMaxPhoto.RowDefinitions.Count; ligne++)
            {
                for (int colonne = 0; colonne < GridMaxPhoto.ColumnDefinitions.Count; colonne++)
                {
                    if (avatarImgs.Count > (ligne * GridMaxPhoto.ColumnDefinitions.Count) + colonne)
                    {
                        Image img = new Image();
                        img.Source = avatarImgs[(ligne * GridMaxPhoto.ColumnDefinitions.Count) + colonne].Source;
                        GridMaxPhoto.Children.Add(img);
                        Grid.SetColumn(img, colonne);
                        Grid.SetRow(img, ligne);
                    }

                }
            }
        }

        private void UpdateAffichage()
        {
            if (Zoom == ZOOMMIN)
            {
                GridMaxPhoto.Visibility = Visibility.Visible;
                ImageAfficher.Visibility = Visibility.Hidden;
                grid6Images.Visibility = Visibility.Hidden;
                initPatchworkPhoto();

            }
            else
            {
                if (Zoom == ZOOMMED)
                {
                    GridMaxPhoto.Visibility = Visibility.Hidden;
                    ImageAfficher.Visibility = Visibility.Hidden;
                    grid6Images.Visibility = Visibility.Visible;

                }
                else
                {
                    GridMaxPhoto.Visibility = Visibility.Hidden;
                    grid6Images.Visibility = Visibility.Hidden;

                    ImageAfficher.Visibility = Visibility.Visible;
                    ImageAfficher.Source = listImg[CurrentIndex];
                }
            }
        }
        #endregion Interface







        #region Avatar

        private void NextMotif()
        {

            if (CurrentIndex == LstMotif.Count - 1)
                CurrentIndex = 0;
            else
                CurrentIndex += 1;

            ImageAfficher.Source = (LstMotif[CurrentIndex]);
        }

        private void PreviousMotif()
        {
            if (CurrentIndex == 0)
                CurrentIndex = LstMotif.Count - 1;
            else
                CurrentIndex -= 1;
            ImageAfficher.Source = (LstMotif[CurrentIndex]);
        }

        private void SelectFondAvatar()
        {
            this.Etape = FONDSELECTIONNER;
        }
        
        public WriteableBitmap Decoupage(BitmapImage img)
        {
            BitmapSource bitmapSource = new FormatConvertedBitmap(img, PixelFormats.Pbgra32, null, 0);
            WriteableBitmap imgmodif = new WriteableBitmap(bitmapSource);

            int width = (int)img.PixelWidth;
            int height = (int)img.PixelHeight;
            int widthinByte = width * 4;



            int[] matriceimg = new int[width * height];
            int[] matricemotif = new int[width * height];
            int[] matriceimgmodif = new int[width * height];



            img.CopyPixels(matricemotif, widthinByte, 0);


            for (long i = 0; i < width * height; i++)
            {
                if (i < depthPixels.Length)
                {
                    if (depthPixels[i].PlayerIndex > 0)
                    {
                        matriceimgmodif[i] = matricemotif[i];
                    }
                    else
                    {
                        matriceimgmodif[i] = 0;
                    }
                }
                else
                {
                    matriceimgmodif[i] = 0;
                }
            }

            //* ((imgmodif.Format.BitsPerPixel + 7) / 8
            imgmodif.WritePixels(new Int32Rect(0, 0, width, height),
                matriceimgmodif,
                width * 4,
                0);

            saveImgAsPNG(imgmodif);
            return imgmodif;
        }

        public WriteableBitmap Detourage(BitmapImage img)
        {
            BitmapSource bitmapSource = new FormatConvertedBitmap(img, PixelFormats.Pbgra32, null, 0);
            WriteableBitmap imgmodif = new WriteableBitmap(bitmapSource);

            int width = (int)img.PixelWidth;
            int height = (int)img.PixelHeight;
            int widthinByte = width * 4;


            int[] matriceimg = new int[width * height];
            int[] matricemotif = new int[width * height];
            int[] matriceimgmodif = new int[width * height];



            img.CopyPixels(matricemotif, widthinByte, 0);

            //int p1tokenColor = Convert.ToInt32(;

            for (long i = 0; i < width * height; i++)
            {
                if (i < depthPixels.Length - 1)
                {
                    if (depthPixels[i].PlayerIndex > 0)
                    {
                        if (depthPixels[i + 1].PlayerIndex == 0)
                        {
                            matriceimgmodif[i] = 0;
                            matriceimgmodif[i + 1] = 0;
                            matriceimgmodif[i - 1] = 0;
                        }
                        else
                        {
                            matriceimgmodif[i] = matricemotif[i];
                        }

                    }
                    else
                    {
                        if (depthPixels[i + 1].PlayerIndex > 0)
                        {
                            matriceimgmodif[i] = 0;
                            matriceimgmodif[i + 1] = 0;
                            matriceimgmodif[i - 1] = 0;
                        }
                        else
                        {
                            matriceimgmodif[i] = matricemotif[i];
                        }
                    }
                }
                else
                {
                    matriceimgmodif[i] = matricemotif[i];
                }
            }
            imgmodif.WritePixels(new Int32Rect(0, 0, width, height),
                matriceimgmodif,
                width * 4,
                0);
            return imgmodif;
        }

#endregion Avatar

        //Sauvegarde des photos
        public void saveImgAsPNG(WriteableBitmap imgmodif) 
        {
            Image imgTest = new Image();
            imgTest.Source = imgmodif;
            var encoder = new PngBitmapEncoder();
            var encoder2 = new PngBitmapEncoder();

          
            string path = System.IO.Directory.GetCurrentDirectory();
            if (path.EndsWith("\\bin\\Debug"))
            {
                path = path.Replace("\\bin\\Debug", "");
            }
            path += "\\Images";
            if(Directory.Exists(AVATAR_PATH))
            {
                String path2 = AVATAR_PATH + "\\avatar" + user.getId() + ".png";
                encoder2.Frames.Add(BitmapFrame.Create((BitmapSource)imgTest.Source));
                using (FileStream stream = new FileStream(path2, FileMode.Create))
                    encoder2.Save(stream);
            }
            user.indexPlus();
            path += "\\" + user.getId();
            

            Directory.CreateDirectory(path);
            //ID used => increment for new value 
            //uniqueID++;
            path += "\\avatar" + user.getId() + ".png";
            imageNameNb = 0;

            encoder.Frames.Add(BitmapFrame.Create((BitmapSource)imgTest.Source));
            using (FileStream stream = new FileStream(path, FileMode.Create))
            encoder.Save(stream);
        }

        public void saveImgAsPNG(Image imgmodif)
        {
         
            string path = System.IO.Directory.GetCurrentDirectory();
            if (path.EndsWith("\\bin\\Debug"))
            {
                path = path.Replace("\\bin\\Debug", "");
            }
            path += "\\Images";
            path += "\\" + user.getId();

            path += "\\img" + imageNameNb + ".png";
            imageNameNb++;

            Image imgTest = new Image();
            imgTest.Source = imgmodif.Source;
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create((BitmapSource)imgTest.Source));
            using (FileStream stream = new FileStream(path, FileMode.Create))
                encoder.Save(stream);
        }

        #region Timer
        public void Wait(double milliSeconds)
        {
            this.Etape = DECOUPEAVATAR;
            //imageFond = new Image();
            this.imageFond.Source = ImageAfficher.Source;
            timer = new System.Timers.Timer();
            timer.Elapsed += new ElapsedEventHandler(timer_Tick);
            timer.Interval = milliSeconds / 5;
            timer.Enabled = true;
            timer.Start();

        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        void timer_Tick(object sender, EventArgs e)
        {
            //chaque tick de timer doit faire un truc
            if(tickTimer<5)
            {


                //Chrono.Source = listImgChrono[tickTimer];

               //change une image dans l'interface
                
                tickTimer++;
            }
            else{
                //Chrono.Visibility = Visibility.Hidden;
                AddToFrise();
                timer.Stop();
                //reset du timer
                tickTimer = 0;
            }

        }

        #endregion Timer

    }
}



public class Melangeur : IComparer
{
    private static Random rnd;
    static Melangeur()
    {
        rnd = new Random();
    }

    public int Compare(object x, object y)
    {
        if (Object.Equals(x, y))
            return 0;
        else
        {
            return rnd.Next(-1, 1);
        }
    }
}


