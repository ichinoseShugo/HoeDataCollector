using System;
using System.Windows;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using WiimoteLib;

using Microsoft.Kinect;
using System.Collections.Generic;
using System.IO;

namespace HoeDataCollector
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        Wiimote Wii = new Wiimote();
        /// <summary>
        /// kinectのインスタンス
        /// </summary>
        public KinectSensor kinect;
        public ColorFrameReader colorFrameReader;
        public FrameDescription colorFrameDesc;
        public byte[] colorBuffer;
        public BodyFrameReader bodyFrameReader;
        public Body[] bodies;

        /// <summary>
        /// 画像保存用bitmap source
        /// </summary>
        public static BitmapSource bitmapSource = null;
        /// <summary>
        /// frame数のカウント
        /// </summary>
        static int frameCount = 0;
        
        List<double[]> kinectDataList = new List<double[]> { new double[2], new double[2] };
        List<double[]> wiiDataList = new List<double[]> { new double[2], new double[2] };
        List<double[]> rowList = new List<double[]>();

        PlotViewModel kinectModel;
        PlotViewModel wiiModel;

        List<string> selectedNames = new List<string>();
        
        /// <summary>
        /// マイドキュメント下のHoeDataフォルダへのパス
        /// </summary>
        static public string pathHoeDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "/HoeData/";
        /// <summary>
        /// 保存先のフォルダパス
        /// </summary>
        static public string pathSaveFolder;
        /// <summary>
        /// 画像保存フォルダのパス
        /// </summary>
        static public string pathImageFolder;

        bool checkRecord = false;
        /// <summary>
        /// Kinect書き込み用ストリーム
        /// </summary>
        private StreamWriter kinectWriter = null;
        /// <summary>
        /// Wiiデータ書き込み用ストリーム
        /// </summary>
        private StreamWriter wiiWriter = null;
        /// <summary>
        /// 時間計測用ストップウォッチ
        /// </summary>
        public static System.Diagnostics.Stopwatch StopWatch = new System.Diagnostics.Stopwatch();
        
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeWii();
            InitializeKinect();
            InitializeView();
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            if (Wii != null) Wii = null;

            if (colorFrameReader != null)
            {
                colorFrameReader.Dispose();
                colorFrameReader = null;
            }

            if (bodyFrameReader != null)
            {
                bodyFrameReader.Dispose();
                bodyFrameReader = null;
            }

            if (kinect != null)
            {
                kinect.Close();
                kinect = null;
            }
        }

        void InitializeWii()
        {
            try
            {
                Wii.WiimoteChanged += WiimoteChanged;  //イベント関数の登録
                Wii.Connect();
                Wii.SetReportType(InputReport.ButtonsAccel, true);   //リモコンのイベント取得条件を設定
                WiiConnection.Text = "Wii:Connect";
            }
            catch (Exception e)
            {
                //MessageBox.Show(e.Message);
                WiiConnection.Text = "Wii:UnConnect";
            }
        }
        
        void InitializeKinect()
        {
            try
            {
                //kinectを開く
                kinect = KinectSensor.GetDefault();
                kinect.Open();

                // 抜き差し検出イベントを設定
                kinect.IsAvailableChanged += kinect_IsAvailableChanged;
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
                KinectConnection.Text = "Kinect:UnConnect";
            }
        }

        private void InitializeView()
        {
            //グラフ関連
            //dataList = new DataList();
            //this.NameBox.ItemsSource = dataList.DataNames;

            //kinectModel = new PlotViewModel(dataList);
            //wiiModel = new PlotViewModel(dataList);
            //this.KinectPlot.Model = kinectModel.GetModel();
            //this.WiiPlot.Model = wiiModel.GetModel();
        }

        void WiimoteChanged(object sender, WiimoteChangedEventArgs args)
        {
            WiimoteState wiiState = args.WiimoteState;
            this.Dispatcher.BeginInvoke(
                new Action(() =>
                {
                    XAxis.Content = "X:"+wiiState.AccelState.RawValues.X;
                    YAxis.Content = "Y:"+wiiState.AccelState.RawValues.Y;
                    ZAxis.Content = "Z:"+wiiState.AccelState.RawValues.Z;
                })
            );
            if (checkRecord)
            {
                wiiWriter.WriteLine(StopWatch.ElapsedMilliseconds
                    + "," + wiiState.AccelState.RawValues.X
                    + "," + wiiState.AccelState.RawValues.Y
                    + "," + wiiState.AccelState.RawValues.Z);
            }
        }

        /// <summary>
        /// Kinectの抜き差し検知イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void kinect_IsAvailableChanged(object sender, IsAvailableChangedEventArgs e)
        {
            // Kinectが接続された
            if (e.IsAvailable)
            {
                KinectConnection.Text = "Kinect:Connect";
                // カラーを設定する
                if (colorFrameReader == null)
                {
                    //カラー画像の情報を作成する（BGRA）
                    colorFrameDesc = kinect.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Bgra);

                    //データを読み込むカラーリーダーを開くとイベントハンドラの登録
                    colorFrameReader = kinect.ColorFrameSource.OpenReader();
                    colorFrameReader.FrameArrived += colorFrameReader_FrameArrived;
                }

                if (bodyFrameReader == null)
                {
                    // Bodyを入れる配列を作る
                    bodies = new Body[kinect.BodyFrameSource.BodyCount];

                    // ボディーリーダーを開く
                    bodyFrameReader = kinect.BodyFrameSource.OpenReader();
                    bodyFrameReader.FrameArrived += bodyFrameReader_FrameArrived;
                }
            }
            // Kinectが外された
            else
            {
                KinectConnection.Text = "Kinect:UnConnect";
            }
        }

        /// <summary>
        /// カラーフレームを取得した時のイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void colorFrameReader_FrameArrived(object sender, ColorFrameArrivedEventArgs e)
        {
            using (var colorFrame = e.FrameReference.AcquireFrame())
            {
                if (colorFrame == null)
                {
                    return;
                }
                //BGRAデータを登録
                colorBuffer = new byte[colorFrameDesc.Width * colorFrameDesc.Height * colorFrameDesc.BytesPerPixel];
                colorFrame.CopyConvertedFrameDataToArray(colorBuffer, ColorImageFormat.Bgra);

                bitmapSource = BitmapSource.Create(colorFrameDesc.Width, colorFrameDesc.Height, 96, 96,
                PixelFormats.Bgra32, null, colorBuffer, colorFrameDesc.Width * (int)colorFrameDesc.BytesPerPixel);
                //ImageColor.Source = bitmapSource;
                ImageColor.SetCurrentValue(System.Windows.Controls.Image.SourceProperty, bitmapSource);

                frameCount++;
            }
        }

        /// <summary>
        /// ボディフレームを取得した時のイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void bodyFrameReader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            UpdateBodyFrame(e);
            DrawBodyFrame();
        }

        /// <summary>
        /// ボディの更新
        /// </summary>
        /// <param name="e"></param>
        private void UpdateBodyFrame(BodyFrameArrivedEventArgs e)
        {
            using (var bodyFrame = e.FrameReference.AcquireFrame())
            {
                if (bodyFrame == null)
                {
                    return;
                }

                // ボディデータを取得する
                bodyFrame.GetAndRefreshBodyData(bodies);
            }
        }

        /// <summary>
        /// ボディの表示
        /// </summary>
        private void DrawBodyFrame()
        {
            CanvasBody.Children.Clear();
            foreach (var body in bodies.Where(b => b.IsTracked))
            {
                Console.WriteLine(bodies.Length);
                if (checkRecord)
                    kinectWriter.Write(StopWatch.ElapsedMilliseconds);
                foreach (var joint in body.Joints)
                {
                    if (checkRecord) {
                        kinectWriter.Write(
                        "," + joint.Value.Position.X + "," 
                            + joint.Value.Position.Y + ","
                            + joint.Value.Position.Z);
                    }
                    DrawEllipse(joint.Value, 10, System.Windows.Media.Brushes.Red);
                }
                if (checkRecord)
                    kinectWriter.WriteLine();
            }
        }

        /// <summary>
        /// 関節の描画
        /// </summary>
        /// <param name="joint"></param>
        /// <param name="R"></param>
        /// <param name="brush"></param>
        private void DrawEllipse(Joint joint, int R, System.Windows.Media.Brush brush)
        {
            var ellipse = new Ellipse()
            {
                Width = R,
                Height = R,
                Fill = brush,
            };

            // カメラ座標系をDepth座標系に変換する
            var point = kinect.CoordinateMapper.MapCameraPointToDepthSpace(joint.Position);
            if ((point.X < 0) || (point.Y < 0))
            {
                return;
            }

            // Depth座標系で円を配置する
            Canvas.SetLeft(ellipse, point.X - (R / 2));
            Canvas.SetTop(ellipse, point.Y - (R / 2));

            CanvasBody.Children.Add(ellipse);
        }

        private void RecordButton_Click(object sender, RoutedEventArgs e)
        {
            if (RecordButton.IsChecked == true)
            {
                InitializeRecord();
                RecordButton.Content = "データの記録中";
            }
            else
            {
                UnInitializeRecord();
                RecordButton.Content = "データを記録する";
            }
        }

        private void InitializeRecord()
        {
            //pathHoeDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "/HoeData/";
            pathSaveFolder = pathHoeDataFolder + CreateFolderName(DateTime.Now) + "/";
            pathImageFolder = pathSaveFolder + "Image/";

            if (kinectWriter == null && wiiWriter == null)
            {
                //MY Document直下にHoeDataフォルダを作成
                //Directory.CreateDirectory(pathHoeDataFolder);
                //ファイル書き込み用のdirectoryを用意
                Directory.CreateDirectory(pathSaveFolder);
                //画像書き込み用のdirectoryを用意
                Directory.CreateDirectory(pathImageFolder);

                //Kinect座標書き込み用csvファイルを用意
                kinectWriter = new StreamWriter(pathSaveFolder + "Kinect.csv", true);
                kinectWriter.Write("KinectTime");

                for (int i = 0; i < JointNames.Joints.Length; i++)
                {
                    string joint = JointNames.Joints[i];
                    kinectWriter.Write("," + joint + "_X," + joint + "_Y," + joint + "_Z");
                }
                kinectWriter.WriteLine();

                //Wiiデータ書き込み用csvファイルを用意
                wiiWriter = new StreamWriter(pathSaveFolder + "Wii.csv", true);
                wiiWriter.WriteLine("WiiTime,Wii_Accel_X,Wii_Accel_Y,Wii_Accel_Z");
                StopWatch.Start();
                checkRecord = true;
            }
        }

        private void UnInitializeRecord()
        {
            checkRecord = false;
            NameInputBox.Text = "";
            StopWatch.Reset();
            if (kinectWriter != null)
            {
                kinectWriter.Close();
                kinectWriter = null;
            }
            if (wiiWriter != null)
            {
                wiiWriter.Close();
                wiiWriter = null;
            }
        }

        /// <summary>
        /// 1桁の場合の桁の補正：1時1分→0101
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public String CreateFolderName(DateTime dateTime)
        {
            string month;
            string day;
            string hour;
            string minute;
            string second;

            if (dateTime.Month.ToString().Length < 2) month = "0" + dateTime.Month;
            else month = dateTime.Month.ToString();

            if (dateTime.Day.ToString().Length < 2) day = "0" + dateTime.Day;
            else day = dateTime.Day.ToString();

            if (dateTime.Hour.ToString().Length < 2) hour = "0" + dateTime.Hour;
            else hour = dateTime.Hour.ToString();

            if (dateTime.Minute.ToString().Length < 2) minute = "0" + dateTime.Minute;
            else minute = dateTime.Minute.ToString();

            if (dateTime.Second.ToString().Length < 2) second = "0" + dateTime.Second;
            else second = dateTime.Second.ToString();
            
            return NameInputBox.Text + dateTime.Year + month + day + hour + minute + second;
        }
    }
}
