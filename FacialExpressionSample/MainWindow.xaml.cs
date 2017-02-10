using Emgu.CV;
using Emgu.CV.Structure;
using FacialExpressionSample.Language;
using FacialExpressionSample.Other;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FacialExpressionSample
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        /// <summary>
        /// Theme styles for change window style
        /// </summary>
        public enum ThemeStyle { BaseLight, BaseDark }

        /// <summary>
        /// Theme accents for change window color
        /// </summary>
        public enum ThemeAccent { Red, Green, Blue, Purple, Orange, Lime, Emerald, Teal, Cyan, Cobalt, Indigo, Violet, Pink, Magenta, Crimson, Amber, Yellow, Brown, Olive, Steel, Mauve, Taupe, Sienna }

        private List<string> labelList;

        public UIData uiData;

        public string filePathNowLoaded;

        /// <summary>
        /// Drawing group for Image_Bbox
        /// </summary>
        private DrawingGroup BboxGD;

        /// <summary>
        /// Bounding box line
        /// </summary>
        private Pen BboxPen = new Pen(Brushes.Green, 2d);

        /// <summary>
        /// Bouding box line for choose label
        /// </summary>
        private Pen BboxLabelPen = new Pen(Brushes.MediumVioletRed, 4d);

        /// <summary>
        /// thickness 0
        /// </summary>
        private Pen drawingBboxNoticePen = new Pen(Brushes.Red, 0d);

        /// <summary>
        /// Bbox rect list
        /// </summary>
        private List<Rect> BboxList;

        /// <summary>
        /// UIImage actual rect
        /// </summary>
        private Rect UIImageActualRect;

        /// <summary>
        /// Thread reset event
        /// </summary>
        private AutoResetEvent autoResetEvent;

        private System.Drawing.Bitmap image;

        private int CountBBox = 0;

        private Image<Rgb, float> image_RGB;

        private EvaluatorHelper cntkFacialHelper;

        public MainWindow()
        {
            InitializeComponent();
            uiData = new UIData();
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            GlobalLanguage.SetLanguage(Setting.MainLanguage);
            SetLabelButton();
            uiData.progressRing_IsActive = false;
            grid_Main.DataContext = uiData;
            cntkFacialHelper = new EvaluatorHelper(Setting.FacialCNNModelPath, 48, 48, 3);
        }

        private System.Windows.Input.ICommand openFirstFlyoutCommand;

        public System.Windows.Input.ICommand OpenFirstFlyoutCommand
        {
            get
            {
                return this.openFirstFlyoutCommand ?? (this.openFirstFlyoutCommand = new SimpleCommand
                {
                    CanExecuteDelegate = x => this.Flyouts.Items.Count > 0,
                    ExecuteDelegate = x => this.ToggleFlyout(0)
                });
            }
        }

        /// <summary>
        /// Show the flyout
        /// </summary>
        /// <param name="index">the number of Flyout Item</param>
        private void ToggleFlyout(int index)
        {
            var flyout = this.Flyouts.Items[index] as Flyout;
            if (flyout == null)
            {
                return;
            }

            flyout.IsOpen = !flyout.IsOpen;
        }

        /// <summary>
        /// Change app accent
        /// </summary>
        /// <param name="accent">New accent</param>
        /// <param name="theme">Theme</param>
        public void ChangeAppStyle(ThemeAccent accent, MahApps.Metro.AppTheme theme)
        {
            // set the accent and theme to the window
            MahApps.Metro.ThemeManager.ChangeAppStyle(Application.Current,
                                        MahApps.Metro.ThemeManager.GetAccent(accent.ToString()),
                                        MahApps.Metro.ThemeManager.GetAppTheme(theme.Name));
        }

        /// <summary>
        /// Change app theme
        /// </summary>
        /// <param name="accent">Accent</param>
        /// <param name="style">New theme</param>
        public void ChangeAppStyle(MahApps.Metro.Accent accent, ThemeStyle style)
        {
            // set the accent and theme to the window
            MahApps.Metro.ThemeManager.ChangeAppStyle(Application.Current,
                                        MahApps.Metro.ThemeManager.GetAccent(accent.Name),
                                        MahApps.Metro.ThemeManager.GetAppTheme(style.ToString()));
        }

        private void SetLabelButton()
        {
            // Label missing
            if (Setting.Label == null || Setting.Label.Equals(""))
            {
                this.ShowMessageAsync(GlobalLanguage.FindText("Common_Notice"), GlobalLanguage.FindText("MainWindow_LabelNotFound"));
                return;
            }
            // Get label
            labelList = new List<string>();
            foreach (string label in Setting.Label.Split('|'))
            {
                labelList.Add(label);
            }
            // Set label
            Button[] button = new Button[labelList.Count()];
            for (int i = 0; i < labelList.Count(); i++)
            {
                button[i] = new Button
                {
                    Content = labelList[i],
                    Margin = new Thickness(5d),
                    ToolTip = i
                };
                button[i].Click += button_Template_Click;
                wrapPanel_Button.Children.Add(button[i]);
            }
        }

        private void button_Template_Click(object sender, RoutedEventArgs e)
        {
            // Get button content(label) which user click
            Button b = (Button)sender;
            if (b.ToolTip.ToString().Equals("7"))
            {
                autoResetEvent.Set();
                return;
            }
            Image<Gray, byte> image_Gray = image_RGB.GetSubRect(new System.Drawing.Rectangle(Convert.ToInt32(BboxList[CountBBox].X), Convert.ToInt32(BboxList[CountBBox].Y), 
                Convert.ToInt32(BboxList[CountBBox].Width), Convert.ToInt32(BboxList[CountBBox].Height))).Convert<Gray, byte>().Resize(48, 48, Emgu.CV.CvEnum.Inter.Cubic);
            if (!Directory.Exists(Setting.SampleSavePath))
            {
                Directory.CreateDirectory(Setting.SampleSavePath);
            }
            FileInfo info = new FileInfo(filePathNowLoaded);
            
            string saveFile = System.IO.Path.Combine(Setting.SampleSavePath, string.Format("{0}_{1}.png", info.Name.Split('.')[0], CountBBox));
            image_Gray.Save(saveFile);
            image_Gray.Dispose();
            image_Gray = null;
            using (StreamWriter SW = new StreamWriter(new FileStream(Setting.InfoSavePath, FileMode.Append)))
            {
                SW.WriteLine(string.Format("{0}	{1}", saveFile, b.ToolTip));
            }

            autoResetEvent.Set();
        }

        /// <summary>
        /// Show image on Mainwindow
        /// </summary>
        /// <param name="file">Image file path</param>
        private Task<int> ShowImage(string file)
        {
            return Task.Factory.StartNew(() => {
                if (image != null)
                {
                    image.Dispose();
                    image = null;
                }
                //uiData.progressRing_IsActive = true;
                filePathNowLoaded = file;
                FileInfo info = new FileInfo(file);
                uiData.TextMessage = string.Format(GlobalLanguage.FindText("MainWindow_ShowImage_Message"), info.Name);
                CallResetOtherImage_Delegate();
                image = System.Drawing.Bitmap.FromFile(file) as System.Drawing.Bitmap;
                BitmapSource bitmap = Imaging.CreateBitmapSourceFromBitmap(ref image);
                uiData.UIImage = bitmap;
                //uiData.progressRing_IsActive = false;
                //image.Dispose();
                bitmap = null;
                return uiData.LastCount--;
            });
        }

        /// <summary>
        /// Reset other layout image when show next image
        /// </summary>
        private void ResetOtherImage()
        {
            //BboxList = new List<Rect>();
            BboxGD = new DrawingGroup();
            uiData.BboxImage = new DrawingImage(BboxGD);
        }

        private delegate void ResetOtherImage_Delegate();

        private void CallResetOtherImage_Delegate()
        {
            this.Dispatcher.Invoke(new ResetOtherImage_Delegate(ResetOtherImage));
        }

        /// <summary>
        /// Draw bounding box on DrawingGroup
        /// </summary>
        /// <param name="DG">DrawingGroup which used to draw Bbox</param>
        /// <param name="start">Rect first point</param>
        /// <param name="end">Rect Second point</param>
        /// <param name="focusPen">Rect pen</param>
        /// <param name="noticePen">Notice pen</param>
        private void DrawBbox(ref DrawingGroup DG, ref Point start, ref Point end, ref Pen focusPen, ref Pen noticePen)
        {
            using (DrawingContext DC = DG.Open())
            {
                DC.DrawRectangle(null, noticePen, UIImageActualRect);
                DC.DrawRectangle(null, focusPen, new Rect(start, end));
            }
        }

        /// <summary>
        /// Draw bounding box on DrawingGroup
        /// </summary>
        /// <param name="DG">DrawingGroup which used to draw Bbox</param>
        /// <param name="list">Rect list</param>
        /// <param name="focusPen">Rect pen</param>
        /// <param name="noticePen">Notice pen</param>
        private void DrawBbox(ref DrawingGroup DG, ref List<Rect> list, ref Pen focusPen, ref Pen noticePen)
        {
            if (list.Count() == 0)
            {
                using (DrawingContext DC = DG.Open())
                {
                    DC.DrawRectangle(null, noticePen, UIImageActualRect);
                }
                return;
            }
            using (DrawingContext DC = DG.Open())
            {
                DC.DrawRectangle(null, noticePen, UIImageActualRect);
                foreach (Rect rect in list)
                {
                    DC.DrawRectangle(null, focusPen, rect);
                }
            }
        }

        private void DrawBbox(ref DrawingGroup DG, Rect rect, ref Pen focusPen, ref Pen noticePen)
        {
            using (DrawingContext DC = DG.Open())
            {
                DC.DrawRectangle(null, noticePen, UIImageActualRect);
                DC.DrawRectangle(null, focusPen, rect);
            }
        }

        private delegate void DrawBbox_Delegate(ref DrawingGroup DG, Rect rect, ref Pen focusPen, ref Pen noticePen);

        private void CallDrawBbox_Delegate(ref DrawingGroup DG, Rect rect, ref Pen focusPen, ref Pen noticePen)
        {
            this.Dispatcher.Invoke(new DrawBbox_Delegate(DrawBbox), DG, rect, focusPen, noticePen);
        }

        private async void button_LoadLocalFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string[] infoFile = Tools.GetFile(GlobalLanguage.FindText("MainWindow_Button_LoadLocalFile_Title"), "Info.dat|*.dat|Info.txt|*.txt", Setting.BasePath);
                if (infoFile == null || infoFile.Count() != 1 || infoFile[0].Equals(""))
                {
                    await this.ShowMessageAsync(GlobalLanguage.FindText("Common_Notice"), GlobalLanguage.FindText("MainWindow_Button_LoadLocalFile_FileSelectError"));
                    return;
                }
                var result = await SplitInfo(infoFile[0]);
                uiData.LastCount = result.Count();

                Thread thread = new Thread(new ParameterizedThreadStart(Thread_StartShowAndLabel));
                thread.IsBackground = true;
                thread.Start(result);
            }catch (Exception ex)
            {
                await this.ShowMessageAsync(GlobalLanguage.FindText("Common_Error"), ex.Message);
            }
        }

        private void button_Setting_Click(object sender, RoutedEventArgs e)
        {
            ToggleFlyout(0);
        }

        private Task<Dictionary<string, Rect[]>> SplitInfo(string infoFile)
        {
            return Task.Factory.StartNew(()=> {
                List<string> rowList = new List<string>();
                using (StreamReader SW = new StreamReader(new FileStream(infoFile, FileMode.Open)))
                {
                    while (!SW.EndOfStream)
                    {
                        rowList.Add(SW.ReadLine());
                    }
                }
                Dictionary<string, Rect[]> dic = new Dictionary<string, Rect[]>();
                foreach (string row in rowList)
                {
                    string[] eachItem = row.Split(' ');
                    Rect[] rect = new Rect[Convert.ToInt32(eachItem[1])];
                    for (int i = 0; i < Convert.ToInt32(eachItem[1]); i++)
                    {
                        List<int> intList = new List<int>();
                        for (int j = 2 + i * 4; j < 6 + i * 4; j++)
                        {
                            intList.Add(Convert.ToInt32(eachItem[j]));
                        }
                        rect[i] = new Rect(intList[0], intList[1], intList[2], intList[3]);
                    }
                    dic.Add(eachItem[0], rect);
                }
                return dic;
            });
        }

        private async void Thread_StartShowAndLabel(object dic)
        {
            try
            {
                Dictionary<string, Rect[]> dictionary = (Dictionary<string, Rect[]>)dic;
                foreach (KeyValuePair<string, Rect[]> row in dictionary)
                {
                    uiData.progressRing_IsActive = true;
                    filePathNowLoaded = row.Key;
                    int result = await ShowImage(row.Key);
                    await Task.Delay(200);
                    UIImageActualRect = new Rect(0, 0, image_Show.ActualWidth, image_Show.ActualHeight);
                    List<Rect> list = ConvertBBox(row.Value.ToList());
                    BboxList = row.Value.ToList();
                    if (autoResetEvent != null)
                    {
                        autoResetEvent.Dispose();
                    }
                    autoResetEvent = new AutoResetEvent(false);
                    image_RGB = new Image<Rgb, float>(image);
                    uiData.progressRing_IsActive = false;
                    foreach (Rect rect in list)
                    {
                        CallDrawBbox_Delegate(ref BboxGD, rect, ref BboxLabelPen, ref drawingBboxNoticePen);
                        AutoRecognition();
                        autoResetEvent.WaitOne();
                        CountBBox++;
                    }
                    CountBBox = 0;
                    image_RGB.Dispose();
                    image_RGB = null;
                }
                await this.Dispatcher.Invoke(async () => {
                    await this.ShowMessageAsync(GlobalLanguage.FindText("Common_Done"), GlobalLanguage.FindText("MainWindow_Thread_StartShowAndLabel_DoneMessage"));
                });
            }catch (Exception ex)
            {
                await this.Dispatcher.Invoke(async () => {
                    await this.ShowMessageAsync(GlobalLanguage.FindText("Common_Error"), ex.Message);
                });
            }
        }

        private List<Rect> ConvertBBox(List<Rect> list)
        {
            List<Rect> newList = new List<Rect>();
            double rate = uiData.UIImage.PixelWidth / image_Show.ActualWidth;
            foreach(Rect rect in list)
            {
                newList.Add(new Rect(rect.X / rate, rect.Y / rate, rect.Width / rate, rect.Height / rate));
            }
            return newList;
        }

        private void AutoRecognition()
        {
            try
            {
                Rect rect = BboxList[CountBBox];
                Image<Rgb, float> image_Converted = image_RGB.GetSubRect(new System.Drawing.Rectangle(Convert.ToInt32(BboxList[CountBBox].X), Convert.ToInt32(BboxList[CountBBox].Y),
                    Convert.ToInt32(BboxList[CountBBox].Width), Convert.ToInt32(BboxList[CountBBox].Height))).Convert<Gray, byte>().Resize(48, 48, Emgu.CV.CvEnum.Inter.Cubic).Convert<Rgb, float>();
                var result = cntkFacialHelper.RecognizeWithConvNet(ref image_Converted, ref rect);
                uiData.FacialResult = string.Format("无表情:{0}%, 高兴:{1}%, 惊讶:{2}%, 沮丧:{3}%, 恐惧:{4}%, 生气:{5}%, 厌恶:{6}%", Math.Round(result[0] * 100, 1),
                    Math.Round(result[1] * 100, 1), Math.Round(result[2] * 100, 1), Math.Round(result[3] * 100, 1), Math.Round(result[4] * 100, 1), Math.Round(result[5] * 100, 1),
                    Math.Round(result[6] * 100, 1));
                image_Converted.Dispose();
                image_Converted = null;
            }catch (Exception ex)
            {
                this.Dispatcher.Invoke(async () => {
                    await this.ShowMessageAsync(GlobalLanguage.FindText("Common_Error"), ex.Message);
                });
            }
        }
    }
}
