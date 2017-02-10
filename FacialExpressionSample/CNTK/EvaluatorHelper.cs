using Emgu.CV;
using FacialExpressionSample.Language;
using Microsoft.MSR.CNTK.Extensibility.Managed;
using Microsoft.MSR.CNTK.Extensibility.Managed.CSEvalClient;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace FacialExpressionSample
{
    /// <summary>
    /// Used to Evaluate the input data by use trained model.
    /// </summary>
    public class EvaluatorHelper
    {
        enum FacialEnum { Natural, Happiness, Surprise, Sadness, Fear, Anger, Disgust }
        /// <summary>
        /// Model Path,used full path.
        /// </summary>
        private string modelPath;

        /// <summary>
        /// deviceId = -1 for CPU, >=0 for GPU devices.
        /// </summary>
        private int deviceId;

        /// <summary>
        /// Model for Evaluate.
        /// </summary>
        private IEvaluateModelManagedF model;

        /// <summary>
        /// CNN input image width.
        /// </summary>
        private int image_Width;

        /// <summary>
        /// CNN input image heigh.
        /// </summary>
        private int image_Heigh;

        /// <summary>
        /// CNN input image channel.
        /// </summary>
        private int image_Channel;

        /// <summary>
        /// Model input dim
        /// </summary>
        private Dictionary<string, int> inputDims;

        /// <summary>
        /// Model output dim
        /// </summary>
        Dictionary<string, int> outputDims;

        /// <summary>
        /// Init EvaluatorHelper and create network.
        /// </summary>
        /// <param name="ModelPath">Model Path,used full path.</param>
        /// <param name="DeviceId">deviceId = -1 for CPU, >=0 for GPU devices.</param>
        public EvaluatorHelper(string ModelPath, int DeviceId = -1)
        {

            if (!File.Exists(ModelPath))
            {
                MessageBox.Show(GlobalLanguage.FindText("Message_Notice_FileNotExist"),GlobalLanguage.FindText("Message_Notice"));
                return;
            }
            this.modelPath = ModelPath;
            this.deviceId = DeviceId;
            this.model = new IEvaluateModelManagedF();
            this.model.CreateNetwork(string.Format("modelPath=\"{0}\"", this.modelPath), this.deviceId);

            this.inputDims = this.model.GetNodeDimensions(NodeGroup.Input);
            this.outputDims = this.model.GetNodeDimensions(NodeGroup.Output);
        }

        /// <summary>
        /// Init EvaluatorHelper and create network for image classification.
        /// </summary>
        /// <param name="ModelPath">Model Path,used full path.</param>
        /// <param name="ImageWidth">The network input image width.</param>
        /// <param name="ImageHeigh">The network input image heigh.</param>
        /// <param name="ImageChannel">The network input image channel.</param>
        /// <param name="DeviceId">deviceId = -1 for CPU, >=0 for GPU devices.</param>
        public EvaluatorHelper(string ModelPath, int ImageWidth, int ImageHeigh, int ImageChannel, int DeviceId = -1)
        {
            if (!File.Exists(ModelPath))
            {
                MessageBox.Show(GlobalLanguage.FindText("EvaluatorHelper_ModelNotExist"), GlobalLanguage.FindText("Common_Notice"));
                return;
            }
            if (ImageWidth <= 0 || ImageHeigh <= 0 || ImageChannel <= 0)
            {
                MessageBox.Show(GlobalLanguage.FindText("EvaluatorHelper_ImageSizeError"),GlobalLanguage.FindText("Common_Error"));
                return;
            }
            this.modelPath = ModelPath;
            this.deviceId = DeviceId;
            this.model = new IEvaluateModelManagedF();
            this.model.CreateNetwork(string.Format("modelPath=\"{0}\"", this.modelPath), this.deviceId);
            this.image_Width = ImageWidth;
            this.image_Heigh = ImageHeigh;
            this.image_Channel = ImageChannel;

            this.inputDims = this.model.GetNodeDimensions(NodeGroup.Input);
            this.outputDims = this.model.GetNodeDimensions(NodeGroup.Output);
        }

        /// <summary>
        /// Init EvaluatorHelper and create network for image classification.
        /// </summary>
        /// <param name="ModelPath">Model Path,used full path.</param>
        /// <param name="ImageWidth">The network input image width.</param>
        /// <param name="ImageHeigh">The network input image heigh.</param>
        /// <param name="ImageChannel">The network input image channel.</param>
        /// <param name="DeviceId">deviceId = -1 for CPU, >=0 for GPU devices.</param>
        /// <param name="outputNodeNameList">multiple layers output.</param>
        public EvaluatorHelper(string ModelPath, int ImageWidth, int ImageHeigh, int ImageChannel,List<string> outputNodeNameList , int DeviceId = -1)
        {
            if (!File.Exists(ModelPath))
            {
                MessageBox.Show(GlobalLanguage.FindText("EvaluatorHelper_ModelNotExist"), GlobalLanguage.FindText("Common_Notice"));
                return;
            }
            if (ImageWidth <= 0 || ImageHeigh <= 0 || ImageChannel <= 0)
            {
                MessageBox.Show(GlobalLanguage.FindText("EvaluatorHelper_ImageSizeError"), GlobalLanguage.FindText("Common_Error"));
                return;
            }
            this.modelPath = ModelPath;
            this.deviceId = DeviceId;
            this.model = new IEvaluateModelManagedF();
            this.model.CreateNetwork(string.Format("modelPath=\"{0}\"", this.modelPath), this.deviceId, outputNodeNames: outputNodeNameList);
            this.image_Width = ImageWidth;
            this.image_Heigh = ImageHeigh;
            this.image_Channel = ImageChannel;

            this.inputDims = this.model.GetNodeDimensions(NodeGroup.Input);
            this.outputDims = this.model.GetNodeDimensions(NodeGroup.Output);
        }

        /// <summary>
        /// Release resources
        /// </summary>
        ~EvaluatorHelper()
        {
            Close();
        }

        /// <summary>
        /// Evaluates a trained model and obtains a single layer output
        /// </summary>
        public IList<double> EvaluateModelSingleLayer(ref float[] inputData)
        {
            try
            {
                //string outputLayerName;

                // Generate random input values in the appropriate structure and size
                //var inDims = model.GetNodeDimensions(NodeGroup.Input);
                var inputs = GetDictionary(inputDims.First().Key,ref inputData);

                // We request the output layer names(s) and dimension, we'll use the first one.
                //var outDims = model.GetNodeDimensions(NodeGroup.Output);
                //outputLayerName = outDims.First().Key;
                // We can call the evaluate method and get back the results (single layer)...
                List<float> outputs = model.Evaluate(inputs, outputDims.First().Key);
                //OutputResults(outputLayerName, outputs);
                return Softmax(ref outputs);
            }
            catch (CNTKException ex)
            {
                //LogHelper.WriteLog(String.Format("Error: {0}\nNative CallStack: {1}\n Inner Exception: {2}", ex.Message, ex.NativeCallStack, ex.InnerException != null ? ex.InnerException.Message : "No Inner Exception"),ex);
                return null;
            }
            catch (Exception ex)
            {
                //LogHelper.WriteLog(String.Format("Error: {0}\nCallStack: {1}\n Inner Exception: {2}", ex.Message, ex.StackTrace, ex.InnerException != null ? ex.InnerException.Message : "No Inner Exception"),ex);
                return null;
            }
        }

        /// <summary>
        /// Creates a Dictionary for input entries or output allocation 
        /// </summary>
        /// <param name="key">The key for the mapping</param>
        /// <param name="size">The number of element entries associated to the key</param>
        /// <param name="maxValue">The maximum value for random generation values</param>
        /// <returns>A dictionary with a single entry for the key/values</returns>
        private static Dictionary<string, List<float>> GetDictionary(string key,ref float[] inputData)
        {
            var dict = new Dictionary<string, List<float>>();
            List<float> list = new List<float>();
            for (int i = 0; i < inputData.Count();i++)
            {
                list.Add(inputData[i]);
            }
            if (key != string.Empty)
            {
                dict.Add(key, list);
            }
            return dict;
        }

        /// <summary>
        /// Convert softmax to double
        /// </summary>
        /// <param name="outputs"></param>
        /// <returns></returns>
        private IList<double> Softmax(ref List<float> outputs)
        {
            var results = new double[outputs.Count];

            var total = 0d;
            for (int index = 0, length = results.Length; index < length; index++)
            {
                results[index] = Math.Exp(outputs[index]);
                total += results[index];
            }

            for (int index = 0, length = results.Length; index < length; index++)
            {
                results[index] /= total;
            }

            return results;
        }

        /// <summary>
        /// Dispose the model
        /// </summary>
        public void Close()
        {
            if (model != null)
            {
                model.Dispose();
                model = null;
            }
        }

        /// <summary>
        /// Evaluate a trained image classification model
        /// </summary>
        /// <param name="imageFileName">Image full path.</param>
        /// <returns>The pacentage result</returns>
        public IList<double> EvaluateImageClassificationModel(string imageFileName)
        {
            try
            {
                // Image not exist
                if (!File.Exists(imageFileName) || this.model == null)
                {
                    return null;
                }

                // the output list after evaluated
                List<float> outputs;

                // Prepare input value in the appropriate structure and size
                //var inDims = this.model.GetNodeDimensions(NodeGroup.Input);
                if (inputDims.First().Value != this.image_Width * this.image_Heigh * this.image_Channel)
                {
                    return null;
                }

                // Transform the image
                //string imageFileName = Setting.BasePath + @"Model\CNTK\ObjectRecognition\dog.png";
                using (Bitmap bmp = new Bitmap(Bitmap.FromFile(imageFileName)))
                {
                    if (bmp.Width != this.image_Width || bmp.Height != this.image_Heigh)
                    {
                        var resized = bmp.Resize(this.image_Width, this.image_Heigh, true);
                        var resizedCHW = resized.ParallelExtractCHW();
                        var inputs = new Dictionary<string, List<float>>() { { inputDims.First().Key, resizedCHW } };

                        // We can call the evaluate method and get back the results (single layer output)...
                        //var outDims = this.model.GetNodeDimensions(NodeGroup.Output);
                        outputs = this.model.Evaluate(inputs, outputDims.First().Key);
                    }else
                    {
                        var bmpCHW = bmp.ParallelExtractCHW();
                        var inputs = new Dictionary<string, List<float>>() { { inputDims.First().Key, bmpCHW } };
                        //var outDims = this.model.GetNodeDimensions(NodeGroup.Output);
                        outputs = this.model.Evaluate(inputs, outputDims.First().Key);
                    }
                    return Softmax(ref outputs);
                }

                // Retrieve the outcome index (so we can compare it with the expected index)
                //var max = outputs.Select((value, index) => new { Value = value, Index = index })
                //    .Aggregate((a, b) => (a.Value > b.Value) ? a : b)
                //    .Index;
            }catch (Exception ex)
            {
                //LogHelper.WriteLog("EvaluatorHeleper.EvaluateImageClassificationModel",ex);
                return null;
            }
        }

        public IList<double> EvaluateImageClassificationModel(ref Emgu.CV.Image<Emgu.CV.Structure.Rgb, float> image)
        {
            try
            {

                // the output list after evaluated
                List<float> outputs;

                // Prepare input value in the appropriate structure and size
                //var inDims = this.model.GetNodeDimensions(NodeGroup.Input);
                if (inputDims.First().Value != this.image_Width * this.image_Heigh * this.image_Channel)
                {
                    return null;
                }

                if (image.Width != this.image_Width || image.Height != this.image_Heigh)
                {
                    image = image.Resize(this.image_Width, this.image_Heigh, Emgu.CV.CvEnum.Inter.Cubic);
                    var resizedCHW = image.ToBitmap().ParallelExtractCHW();
                    var inputs = new Dictionary<string, List<float>>() { { inputDims.First().Key, resizedCHW } };
                    outputs = this.model.Evaluate(inputs, outputDims.First().Key);
                }else
                {
                    var bmpCHW = image.ToBitmap().ParallelExtractCHW();
                    var inputs = new Dictionary<string, List<float>>() { { inputDims.First().Key, bmpCHW } };
                    outputs = this.model.Evaluate(inputs, outputDims.First().Key);
                }
                return Softmax(ref outputs);
            }
            catch (Exception ex)
            {
                //LogHelper.WriteLog("EvaluatorHeleper.EvaluateImageClassificationModel", ex);
                return null;
            }
        }

        /// <summary>
        /// For test first
        /// </summary>
        /// <param name="imageFileName"></param>
        /// <param name="roiCoordinates"></param>
        /// <param name="numLabels"></param>
        public void EvaluateObjectDetectionModel(string imageFileName, string roiCoordinates, int numLabels)
        {
            if (inputDims.First().Value != 1000 * 1000 * 3)
            {
                //LogHelper.WriteLog("EvaluatorHeleper.EvaluateObjectDetectionModel");
                return;
            }
            using (Bitmap bmp = new Bitmap(Bitmap.FromFile(imageFileName)))
            {
                var resized = bmp.Resize(1000, 1000, true);
                var resizedCHW = resized.ParallelExtractCHW();

                var rois = roiCoordinates.Split(' ').Select(x => float.Parse(x)).ToList();
                var inputs = new Dictionary<string, List<float>>() { { inputDims.First().Key, resizedCHW }, { inputDims.Last().Key, rois } };
                var outputs = model.Evaluate(inputs, outputDims.First().Key);
                int numRois = outputs.Count / numLabels;
                int numBackgroundRois = 0;
                // test
                string[] labels = new string[] { "__background__", "head", "eye" };
                for (int i = 0; i < numRois; i++)
                {
                    var outputForRoi = outputs.Skip(i * numLabels).Take(numLabels).ToList();
                    var max = outputForRoi.Select((value, index) => new { Value = value, Index = index })
                        .Aggregate((a, b) => (a.Value > b.Value) ? a : b)
                        .Index;

                    if (max > 0)
                    {
                        MessageBox.Show(string.Format("Outcome for ROI {0}: {1} \t({2})", i, max, labels[max]),"test");
                    }else
                    {
                        numBackgroundRois++;
                    }
                }
            }

        }

        //public Task<IList<double>> RecognizeWithConvNet(ref Emgu.CV.Image<Emgu.CV.Structure.Rgb, float> image, ref Rect faceBox)
        //{
        //    return Task.Factory.StartNew(() => {
        //        var result = EvaluateImageClassificationModel(ref image);
        //        image.Dispose();
        //        image = null;
        //        return result;
        //    });
        //}

        public IList<double> RecognizeWithConvNet(ref Emgu.CV.Image<Emgu.CV.Structure.Rgb, float> image, ref Rect faceBox)
        {
            var result = EvaluateImageClassificationModel(ref image);
            //image.Dispose();
            //image = null;
            return result;
        }

        /// <summary>
        /// Evaluate a trained model use input node names and features
        /// </summary>
        /// <param name="dic">Input node names and features</param>
        /// <returns>The pacentage result</returns>
        public IList<double> Evaluate(Dictionary<string, List<float>> dic)
        {
            var outputs = model.Evaluate(dic, outputDims.First().Key);
            return Softmax(ref outputs);
        }

        public static Image<Emgu.CV.Structure.Rgb, float> ConvertImage2RGB(WriteableBitmap wtbBmp, Rect useToCheckFaceRect)
        {
            RenderTargetBitmap rtbitmap = new RenderTargetBitmap(wtbBmp.PixelWidth, wtbBmp.PixelHeight, wtbBmp.DpiX, wtbBmp.DpiY, System.Windows.Media.PixelFormats.Default);
            System.Windows.Media.DrawingVisual drawingVisual = new System.Windows.Media.DrawingVisual();
            using (var dc = drawingVisual.RenderOpen())
            {
                dc.DrawImage(wtbBmp, new Rect(0, 0, wtbBmp.Width, wtbBmp.Height));

            }
            rtbitmap.Render(drawingVisual);
            JpegBitmapEncoder bitmapEncoder = new JpegBitmapEncoder();
            bitmapEncoder.Frames.Add(BitmapFrame.Create(rtbitmap));
            MemoryStream ms = new MemoryStream();
            bitmapEncoder.Save(ms);
            using (System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(ms))
            {
                Image<Emgu.CV.Structure.Rgb, float> image = new Image<Emgu.CV.Structure.Rgb, float>(bitmap).GetSubRect(new System.Drawing.Rectangle((int)useToCheckFaceRect.X, (int)useToCheckFaceRect.Y, (int)useToCheckFaceRect.Width, (int)useToCheckFaceRect.Height)).Convert<Emgu.CV.Structure.Rgb, float>();
                bitmapEncoder = null;
                rtbitmap = null;
                drawingVisual = null;
                return image;
            }
        }
    }
}
