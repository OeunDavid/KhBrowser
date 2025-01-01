using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Features2D;
using Emgu.CV.Flann;
using Emgu.CV.OCR;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolLib.Img;

namespace ToolLib
{
    
    public interface IImageHelper
    {
     
        Image<Bgr, byte> img(string path);
        void grayScale(string path, string newPath);
        Image<Bgr, byte> resize(Image<Bgr, byte> img, int width = 0, int height = 0);
        Rectangle match(string cmpImg, string sourceImg, TemplateMatchingType templateMatchingType);

        Rectangle match(Image<Bgr, byte> template, Image<Bgr, byte> source);
        Rectangle match(string cmpImg, string srcImg);
        Rectangle match(IconItem cmpIcon, string srcImage);
        Point midPoint(string deviceId, Rectangle rect);
        string ocr(Image<Bgr, byte> img);
        List<Rectangle> matchList(Image<Bgr, byte> template, Image<Bgr, byte> source);
        List<Rectangle> matchList(string template, string source);
        List<Point> pointList(Rectangle rectangle);
        Rectangle featureDetect(Image<Bgr, byte> template, Image<Bgr, byte> source);
        Rectangle fromPoints(Point[] points);
        Rectangle basicMatch(Image<Bgr, byte> template, Image<Bgr, byte> source);
        Rectangle basicMatch(string template, string source);
        Rectangle anotherMatch(Image<Bgr, byte> template, Image<Bgr, byte> sceneImage);
        Rectangle anotherMatch(string template, string source);
        Rectangle deepMatch(DeepIcon icon, Image<Bgr, byte> source);
        List<Rectangle> deepMatchList(DeepIcon icon, Image<Bgr, byte> source);
        List<Rectangle> deepMatchList(DeepIcon icon, string source);
        int screenHeight(string screenPath);
        string getText(string srcImage);
        Image<Bgr, byte> cleanForGroup(string path);



    }
    public class ImageHelper:IImageHelper
    {
       public  const double CMP_THRESHOLD = 0.83;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public Image<Bgr, byte> img(string path)
        {
            try
            {
                return new Image<Bgr, byte>(path);
            }
            catch { }

            return null;
        }
        public void grayScale(string path, string newPath)
        {
            Image<Bgr, byte> image = new Image<Bgr, byte>(path);
            Image<Gray, byte> grayImg = image.Convert<Gray, byte>();
            grayImg.Save(newPath);

        }
        public Image<Bgr, byte> resize(Image<Bgr, byte> img, int width = 0, int height = 0)
        {
            int newWidth = width;
            int newHeight = height;
            if (newWidth == 0)
            {
                double diff = (double)newHeight / img.Height;
                newWidth = (int)(img.Width * diff);
            }
            else if (newHeight == 0)
            {
                double diff = (double)newWidth / img.Width;
                newHeight = (int)(img.Height * diff);
            }
            return img.Resize(newWidth, newHeight, Emgu.CV.CvEnum.Inter.Linear);
        }


        public Rectangle match(string cmpImg, string sourceImg, TemplateMatchingType templateMatchingType)
        {
            log.Info("match " + cmpImg + ", vs " + sourceImg);
            var source = new Image<Bgr, byte>(sourceImg);
            var template = new Image<Bgr, byte>(cmpImg);
            using (Image<Gray, float> result = source.MatchTemplate(template, Emgu.CV.CvEnum.TemplateMatchingType.CcorrNormed))
            {
                double[] minValues, maxValues;
                Point[] minLocations, maxLocations;
                result.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);

                // You can try different values of the threshold. I guess somewhere between 0.75 and 0.95 would be good.
                if (maxValues[0] > CMP_THRESHOLD)
                {
                    log.Info("match " + maxValues[0] + " : " + minValues[0]);
                    // This is a match. Do something with it, for example draw a rectangle around it.
                    Rectangle match = new Rectangle(maxLocations[0], template.Size);

                    //var copy = source.GetSubRect(match);
                    //copy.Save(sourceImg + "_" + templateMatchingType.ToString().ToLower() + "_matched.png");
                    //var text = ocr(copy);
                    //log.Info("match text : " + text);

                    //source.Draw(match, new Bgr(Color.Red), 3);
                    //source.Save(sourceImg + "_"+templateMatchingType.ToString()+".png");

                    return match;
                }
            }
            return new Rectangle();
        }

        public Rectangle match(Image<Bgr, byte> template, Image<Bgr, byte> source)
        {
            ////var grTemplate = template.Convert<Gray, byte>();
            ////var grSource = template.Convert<Gray, byte>();

            //using (Image<Gray, float> result = source.MatchTemplate(template, Emgu.CV.CvEnum.TemplateMatchingType.CcorrNormed))
            //{
            //    double[] minValues, maxValues;
            //    Point[] minLocations, maxLocations;
            //    result.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);

            //    // You can try different values of the threshold. I guess somewhere between 0.75 and 0.95 would be good.
            //    if (maxValues[0] > CMP_THRESHOLD)
            //    {
            //        log.Info("match " + maxValues[0] + " : " + minValues[0]);
            //        // This is a match. Do something with it, for example draw a rectangle around it.
            //        Rectangle match = new Rectangle(maxLocations[0], template.Size);
            //        source.Draw(match, new Bgr(Color.Red), 3);
            //        source.Save(DeviceInfo.SCREEN_PATH+"/tmp.png" );
            //        return match;
            //    }
            //}
            //return new Rectangle();

            
            return featureDetect(template, source);
        }
        public Rectangle match(string cmpImg, string srcImg)
        {
            try
            {
                Image<Bgr, byte> source = new Image<Bgr, byte>(srcImg);
                Image<Bgr, byte> template = new Image<Bgr, byte>(cmpImg);
                log.Info(" source : " + srcImg + " vs  cmp : " + cmpImg);
                return match(template, source);
            }
            catch { }

            return new Rectangle();
        }
        public Point midPoint(string deviceId, Rectangle rect)
        {
            int midX = (rect.Left + rect.Right) / 2;
            int midY = (rect.Top + rect.Bottom) / 2;
            int currentScreen = DeviceInfo.deviceScreen(deviceId);
            if( currentScreen > 0)
            {
                double diff = (double)currentScreen / DeviceInfo.DEVICE_DEFAULT_WIDTH;
                midX = (int)(midX * diff);
                midY = (int)(midY * diff);
            }
            return new Point( midX,midY);
        }


        public string ocr(Image<Bgr, byte> img)
        {
            string tessdata = Environment.CurrentDirectory + "/tesseract";
            using (var ocrProvider = new Tesseract(tessdata, "eng", OcrEngineMode.TesseractOnly))
            {
                ocrProvider.SetImage(img);
                ocrProvider.Recognize();
                return ocrProvider.GetUTF8Text();
            }
                
        }

        public Rectangle match(IconItem cmpIcon, string srcImage)
        {
            //log.Info("mach " + cmpIcon.Path + " -> " + cmpIcon.Text + " vs " + srcImage);
            //Image<Bgr, byte> source = new Image<Bgr, byte>(srcImage);
            //Image<Bgr, byte> template = new Image<Bgr, byte>(cmpIcon.Path);
            //using (Image<Gray, float> result = source.MatchTemplate(template, Emgu.CV.CvEnum.TemplateMatchingType.CcorrNormed))
            //{
            //    double[] minValues, maxValues;
            //    Point[] minLocations, maxLocations;
            //    result.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);

            //    // You can try different values of the threshold. I guess somewhere between 0.75 and 0.95 would be good.
            //    if (maxValues[0] > CMP_THRESHOLD)
            //    {
            //        log.Info("match " + maxValues[0] + " : " + minValues[0]);
            //        // This is a match. Do something with it, for example draw a rectangle around it.
            //        Rectangle match = new Rectangle(maxLocations[0], template.Size);
            //        var copy = source.GetSubRect(match);
            //        var text = ocr(copy);
            //        log.Info("match text : " + text);

            //        source.Draw(match, new Bgr(Color.Red), 3);
            //        source.Save(DeviceInfo.SCREEN_PATH + "/tmp.png");
            //        if(!text.Contains(cmpIcon.Text))
            //        {
            //            return new Rectangle();
            //        }
            //        return match;
            //    }
            //}

            log.Info("begin to match :" + cmpIcon.Path + " vs " + srcImage);
            Image<Bgr, byte> source = new Image<Bgr, byte>(srcImage);
            Image<Bgr, byte> template = new Image<Bgr, byte>(cmpIcon.Path);
            var rect = featureDetect(template, source);
            log.Info( "match " + cmpIcon.Path + " vs " + srcImage + " -> " + rect);
            if (!rect.IsEmpty)
            {
                try
                {
                    int x = rect.X;
                    if( x < 0)
                    {
                        x = 1;
                    }
                    int y = rect.Y;
                    if( y <0)
                    {
                        y = 1;
                    }
                  int maxX = x + rect.Width;
                    if( maxX > source.Width)
                    {
                        maxX = source.Width -1 - x;
                    }
                    int maxY =  y + rect.Height;
                    if (maxY > source.Height)
                    {
                        maxY = source.Height - 1 - y ;
                    }
                    int width = maxX - x;
                    int height = maxY - y;
                    var newRect = new Rectangle(x,y, width, height);

                    var copy = source.GetSubRect(newRect);
                    var text = ocr(copy);
                    log.Info("match text : " + text);
                    //source.Draw(newRect, new Bgr(0, 0, 1), 5);
                    //source.Save(DeviceInfo.SCREEN_PATH + "/matched_orc.png");
                    if (text.Contains(cmpIcon.Text))
                    {
                        return rect;
                    }
                }catch(Exception e)
                {
                    log.Error("error ocr cmp", e);
                }
            }
            log.Info("no matched at all");

            return new Rectangle();
        }

        public string getText(string srcImage)
        {
            string text = "";
            try
            {
                Image<Bgr, byte> source = new Image<Bgr, byte>(srcImage);
                text = ocr(source);
            }
            catch (Exception e)
            {
                log.Error("error ocr cmp", e);
            }

            return text;
        }

        public List<Rectangle> matchList(Image<Bgr, byte> template, Image<Bgr, byte> source)
        {
            List<Rectangle> items = new List<Rectangle>();
            var copy = source.Copy();
            copy = resize(copy, DeviceInfo.DEVICE_DEFAULT_WIDTH * 2);
            copy = copy.Not().SmoothGaussian(3);
            int i = 0;
            template = resize(template, template.Width * 2).Not().SmoothGaussian(3);
            HashSet<Rectangle> set = new HashSet<Rectangle>();
            while (true)
            {
                //using (Image<Gray, float> result = copy.MatchTemplate(template, Emgu.CV.CvEnum.TemplateMatchingType.CcorrNormed))
                //{
                //    double[] minValues, maxValues;
                //    Point[] minLocations, maxLocations;
                //    result.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);

                //    // You can try different values of the threshold. I guess somewhere between 0.75 and 0.95 would be good.
                //    if (maxValues[0] >=0.99)
                //    {
                //        log.Info("match " + maxValues[0] + " : " + minValues[0]);

                //        // This is a match. Do something with it, for example draw a rectangle around it.
                //        Rectangle match = new Rectangle(maxLocations[0], template.Size);
                //        //var text = ocr(copy.GetSubRect(match));
                //        //log.Info("match text " + text);

                //        items.Add(match);
                //        copy.FillConvexPoly(pointList(match).ToArray(), new Bgr(Color.Black));
                //        copy.Save(DeviceInfo.SCREEN_PATH + "/tmp"+i+++".png");

                //    }
                //    else
                //    {
                //        break;
                //    }
                //}

                var cmp = anotherMatch(template, copy);

                if (cmp.IsEmpty || cmp.X < 0 || cmp.Y < 0)
                {
                    break;
                }
                else
                {
                    if( shouldSelect(template.Height, cmp.Height) && shouldSelect( template.Width, cmp.Width))
                    {
                        var selection = new Rectangle(cmp.X / 2, cmp.Y / 2, cmp.Width / 2, cmp.Height / 2);
                        if(set.Contains(selection))
                        {
                            return items;
                        }
                        items.Add(selection);
                        
                        set.Add(selection);
                        //copy.FillConvexPoly(pointList(cmp).ToArray(), new Bgr(Color.Black));
                        //copy.Save(DeviceInfo.SCREEN_PATH + "/tmp" + i++ + ".png");
                    }
                   
                }

            }
          
            return items;
        }

        public List<Point> pointList(Rectangle rectangle)
        {
            List<Point> points = new List<Point>();
            for(int x = rectangle.Left; x <= rectangle.Right; x++)
            {
                for(int y = rectangle.Top; y <= rectangle.Bottom; y++)
                {
                    points.Add(new Point() { X = x, Y = y });
                }
            }
            return points;
        
        }

        public List<Rectangle> matchList(string template, string source)
        {
            var imgTemplate = new Image<Bgr, byte>(template);
            var imgSource = new Image<Bgr, byte>(source);
            return matchList(imgTemplate, imgSource);
        }
        
        private bool shouldSelect(int origin, int matched)
        {
            double diff = (double)matched / ( double) origin;
            log.Info(" should select diff : " + diff + " of " + origin + " vs " + matched);

             return 0.5 <= diff && diff <= 3;
        }


        public Rectangle featureDetect(Image<Bgr, byte> template, Image<Bgr, byte> source)
        {
            try
            {
                VectorOfPoint finalPoints = null;
                Mat homography = null;
                VectorOfKeyPoint templateKeyPoints = new VectorOfKeyPoint();
                VectorOfKeyPoint sceneKeyPoints = new VectorOfKeyPoint();
                Mat tempalteDescriptor = new Mat();
                Mat sceneDescriptor = new Mat();

                Mat mask;
                int k = 2;
                double uniquenessthreshold = 0.80;
                VectorOfVectorOfDMatch matches = new VectorOfVectorOfDMatch();

                // feature detectino and description
                Brisk featureDetector = new Brisk();
                featureDetector.DetectAndCompute(template, null, templateKeyPoints, tempalteDescriptor, false);
                featureDetector.DetectAndCompute(source, null, sceneKeyPoints, sceneDescriptor, false);

                // Matching
                BFMatcher matcher = new BFMatcher(DistanceType.Hamming);
                matcher.Add(tempalteDescriptor);
                matcher.KnnMatch(sceneDescriptor, matches, k);

                mask = new Mat(matches.Size, 1, Emgu.CV.CvEnum.DepthType.Cv8U, 1);
                mask.SetTo(new MCvScalar(255));

                Features2DToolbox.VoteForUniqueness(matches, uniquenessthreshold, mask);

                int count = Features2DToolbox.VoteForSizeAndOrientation(templateKeyPoints, sceneKeyPoints, matches, mask, 1.5, 20);

                if (count >= 4)
                {
                    homography = Features2DToolbox.GetHomographyMatrixFromMatchedFeatures(templateKeyPoints,
                        sceneKeyPoints, matches, mask, 2);
                }

                if (homography != null)
                {
                    Rectangle rect = new Rectangle(Point.Empty, template.Size);
                    PointF[] pts = new PointF[]
                    {
                        new PointF(rect.Left,rect.Bottom),
                        new PointF(rect.Right,rect.Bottom),
                        new PointF(rect.Right,rect.Top),
                        new PointF(rect.Left,rect.Top)
                    };

                    pts = CvInvoke.PerspectiveTransform(pts, homography);
                    Point[] points = Array.ConvertAll<PointF, Point>(pts, Point.Round);
                    //var copy = source.Clone();                    //finalPoints = new VectorOfPoint(points);
                    //CvInvoke.Polylines(copy, points, true, new MCvScalar(0, 0, 255), 5);
                    //copy.Save(DeviceInfo.SCREEN_PATH + "/matched.png");
                    var matchRect = fromPoints(points);
                    log.Info(" matched rect : " + matchRect.Width + " : " + matchRect.Height +" --> " +template.Width +" :" +template.Height);
                    if( shouldSelect( template.Height , matchRect.Height) && shouldSelect(template.Width, matchRect.Width))
                    {
                        return matchRect;
                    }
                }

            }
            catch(Exception e)
            {
                log.Info("error",e);

            }
            return new Rectangle();


        }

        public   Rectangle anotherMatch(Image<Bgr, byte> template, Image<Bgr, byte> sceneImage)
        {
            try
            {
                // initializations done
                VectorOfPoint finalPoints = null;
                Mat homography = null;
                VectorOfKeyPoint templateKeyPoints = new VectorOfKeyPoint();
                VectorOfKeyPoint sceneKeyPoints = new VectorOfKeyPoint();
                Mat tempalteDescriptor = new Mat();
                Mat sceneDescriptor = new Mat();

                Mat mask;
                int k = 2;
                double uniquenessthreshold = 0.80;
                VectorOfVectorOfDMatch matches = new VectorOfVectorOfDMatch();

                // feature detectino and description
                KAZE featureDetector = new KAZE();
                featureDetector.DetectAndCompute(template, null, templateKeyPoints, tempalteDescriptor, false);
                featureDetector.DetectAndCompute(sceneImage, null, sceneKeyPoints, sceneDescriptor, false);


                // Matching

                //KdTreeIndexParams ip = new KdTreeIndexParams();
                //var ip = new AutotunedIndexParams();
                var ip = new LinearIndexParams();
                SearchParams sp = new SearchParams();
                FlannBasedMatcher matcher = new FlannBasedMatcher(ip, sp);


                matcher.Add(tempalteDescriptor);
                matcher.KnnMatch(sceneDescriptor, matches, k);

                mask = new Mat(matches.Size, 1, Emgu.CV.CvEnum.DepthType.Cv8U, 1);
                mask.SetTo(new MCvScalar(255));

                Features2DToolbox.VoteForUniqueness(matches, uniquenessthreshold, mask);

                int count = Features2DToolbox.VoteForSizeAndOrientation(templateKeyPoints, sceneKeyPoints, matches, mask, 1.5, 20);

                if (count >= 4)
                {
                    homography = Features2DToolbox.GetHomographyMatrixFromMatchedFeatures(templateKeyPoints,
                        sceneKeyPoints, matches, mask, 5);
                }

                if (homography != null)
                {
                    Rectangle rect = new Rectangle(Point.Empty, template.Size);
                    PointF[] pts = new PointF[]
                    {
                        new PointF(rect.Left,rect.Bottom),
                        new PointF(rect.Right,rect.Bottom),
                        new PointF(rect.Right,rect.Top),
                        new PointF(rect.Left,rect.Top)
                    };

                    pts = CvInvoke.PerspectiveTransform(pts, homography);
                    Point[] points = Array.ConvertAll<PointF, Point>(pts, Point.Round);
                    //    finalPoints = new VectorOfPoint(points);
                    var matchRect = fromPoints(points);
                    //CvInvoke.Polylines(sceneImage, points, true, new MCvScalar(0, 0, 255), 5);
                    //sceneImage.Save(DeviceInfo.SCREEN_PATH + "/matched_flann.png");
                    log.Info(" matched rect : " + matchRect.Width + " : " + matchRect.Height + " --> " + template.Width + " :" + template.Height);
                    return matchRect;
                 }

                return new Rectangle();
            }
            catch (Exception ex)
            {
                log.Error("flann match error", ex);
            }
            return new Rectangle();

        }

        public static VectorOfPoint ProcessImage(Image<Bgr, byte> template, Image<Bgr, byte> sceneImage)
        {
            try
            {
                //template.Save("./template.png");
                //sceneImage.Save("./scene.png");
                // initialization
                VectorOfPoint finalPoints = null;
                Mat homography = null;
                VectorOfKeyPoint templateKeyPoints = new VectorOfKeyPoint();
                VectorOfKeyPoint sceneKeyPoints = new VectorOfKeyPoint();
                Mat tempalteDescriptor = new Mat();
                Mat sceneDescriptor = new Mat();

                Mat mask;
                int k = 2;
                double uniquenessthreshold = 0.80;
                VectorOfVectorOfDMatch matches = new VectorOfVectorOfDMatch();

                // feature detectino and description
                Brisk featureDetector = new Brisk();
                featureDetector.DetectAndCompute(template, null, templateKeyPoints, tempalteDescriptor, false);
                featureDetector.DetectAndCompute(sceneImage, null, sceneKeyPoints, sceneDescriptor, false);

                // Matching
                BFMatcher matcher = new BFMatcher(DistanceType.Hamming);
                matcher.Add(tempalteDescriptor);
                matcher.KnnMatch(sceneDescriptor, matches, k);

                mask = new Mat(matches.Size, 1, Emgu.CV.CvEnum.DepthType.Cv8U, 1);
                mask.SetTo(new MCvScalar(255));

                Features2DToolbox.VoteForUniqueness(matches, uniquenessthreshold, mask);

                int count = Features2DToolbox.VoteForSizeAndOrientation(templateKeyPoints, sceneKeyPoints, matches, mask, 1.5, 20);

                if (count >= 4)
                {
                    homography = Features2DToolbox.GetHomographyMatrixFromMatchedFeatures(templateKeyPoints,
                        sceneKeyPoints, matches, mask, 5);
                }

                if (homography != null)
                {
                    Rectangle rect = new Rectangle(Point.Empty, template.Size);
                    PointF[] pts = new PointF[]
                    {
                        new PointF(rect.Left,rect.Bottom),
                        new PointF(rect.Right,rect.Bottom),
                        new PointF(rect.Right,rect.Top),
                        new PointF(rect.Left,rect.Top)
                    };

                    pts = CvInvoke.PerspectiveTransform(pts, homography);
                    Point[] points = Array.ConvertAll<PointF, Point>(pts, Point.Round);
                    finalPoints = new VectorOfPoint(points);
                    CvInvoke.Polylines(sceneImage, points, true, new MCvScalar(0, 0, 255), 5);
                    sceneImage.Save(DeviceInfo.SCREEN_PATH + "/matched.png");
                }

                return finalPoints;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }

        public static VectorOfPoint ProcessImageFLANN(Image<Bgr, byte> template, Image<Bgr, byte> sceneImage)
        {
            try
            {
                // initializations done
                VectorOfPoint finalPoints = null;
                Mat homography = null;
                VectorOfKeyPoint templateKeyPoints = new VectorOfKeyPoint();
                VectorOfKeyPoint sceneKeyPoints = new VectorOfKeyPoint();
                Mat tempalteDescriptor = new Mat();
                Mat sceneDescriptor = new Mat();

                Mat mask;
                int k = 2;
                double uniquenessthreshold = 0.80;
                VectorOfVectorOfDMatch matches = new VectorOfVectorOfDMatch();

                // feature detectino and description
                KAZE featureDetector = new KAZE();
                featureDetector.DetectAndCompute(template, null, templateKeyPoints, tempalteDescriptor, false);
                featureDetector.DetectAndCompute(sceneImage, null, sceneKeyPoints, sceneDescriptor, false);


                // Matching

                //KdTreeIndexParams ip = new KdTreeIndexParams();
                //var ip = new AutotunedIndexParams();
                var ip = new LinearIndexParams();
                SearchParams sp = new SearchParams();
                FlannBasedMatcher matcher = new FlannBasedMatcher(ip, sp);


                matcher.Add(tempalteDescriptor);
                matcher.KnnMatch(sceneDescriptor, matches, k);

                mask = new Mat(matches.Size, 1, Emgu.CV.CvEnum.DepthType.Cv8U, 1);
                mask.SetTo(new MCvScalar(255));

                Features2DToolbox.VoteForUniqueness(matches, uniquenessthreshold, mask);

                int count = Features2DToolbox.VoteForSizeAndOrientation(templateKeyPoints, sceneKeyPoints, matches, mask, 1.5, 20);

                if (count >= 4)
                {
                    homography = Features2DToolbox.GetHomographyMatrixFromMatchedFeatures(templateKeyPoints,
                        sceneKeyPoints, matches, mask, 5);
                }

                if (homography != null)
                {
                    Rectangle rect = new Rectangle(Point.Empty, template.Size);
                    PointF[] pts = new PointF[]
                    {
                        new PointF(rect.Left,rect.Bottom),
                        new PointF(rect.Right,rect.Bottom),
                        new PointF(rect.Right,rect.Top),
                        new PointF(rect.Left,rect.Top)
                    };

                    pts = CvInvoke.PerspectiveTransform(pts, homography);
                    Point[] points = Array.ConvertAll<PointF, Point>(pts, Point.Round);
                    finalPoints = new VectorOfPoint(points);
                    //CvInvoke.Polylines(sceneImage, points, true, new MCvScalar(0, 0, 255), 5);
                    //sceneImage.Save(DeviceInfo.SCREEN_PATH + "/matched_flann.png");

                }

                return finalPoints;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }


        public Rectangle fromPoints(Point[] points)
        {
            if( points.Length == 0)
            {
                return new Rectangle();
            }
            var point = points[0];
            int minX = point.X;
            int maxX = point.X;
            int minY = point.Y;
            int maxY = point.Y;
            for(int i =1;i<points.Length;i++)
            {
                var p = points[i];
                minX = Math.Min(p.X, minX);
                maxX = Math.Max(p.X, maxX);
                minY = Math.Min(p.Y, minY);
                maxY = Math.Max(p.Y, maxY);
            }
            var rect = new Rectangle();
            rect.X = minX;
            rect.Y = minY;
            rect.Width = maxX - minX;
            rect.Height = maxY - minY;
            return rect;
        }

        public Rectangle basicMatch(Image<Bgr, byte> template, Image<Bgr, byte> source)
        {
            //log.Info("mach " + cmpIcon.Path + " -> " + cmpIcon.Text + " vs " + srcImage);
            //Image<Bgr, byte> source = new Image<Bgr, byte>(srcImage);
            //Image<Bgr, byte> template = new Image<Bgr, byte>(cmpIcon.Path);
            using (Image<Gray, float> result = source.MatchTemplate(template, Emgu.CV.CvEnum.TemplateMatchingType.CcorrNormed))
            {
                double[] minValues, maxValues;
                Point[] minLocations, maxLocations;
                result.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);

                // You can try different values of the threshold. I guess somewhere between 0.75 and 0.95 would be good.
                if (maxValues[0] > CMP_THRESHOLD)
                {
                    log.Info("match " + maxValues[0] + " : " + minValues[0]);
                    // This is a match. Do something with it, for example draw a rectangle around it.
                    Rectangle match = new Rectangle(maxLocations[0], template.Size);
                    var copy = source.GetSubRect(match);
                    var text = ocr(copy);
                    log.Info("match text : " + text);

                    //source.Draw(match, new Bgr(Color.Red), 3);
                    //source.Save(DeviceInfo.SCREEN_PATH + "/tmp.png");
                   
                    return match;
                }
            }
            return new Rectangle();
        }

        public Rectangle basicMatch(string template, string source)
        {
            var imgTemplate = new Image<Bgr, byte>(template);
            var imgSource = new Image<Bgr, byte>(source);
            return basicMatch(imgTemplate, imgSource);
        }

        public Rectangle anotherMatch(string template, string source)
        {
            var imgTemplate = new Image<Bgr, byte>(template);
            var imgSource = new Image<Bgr, byte>(source);
            return anotherMatch(imgTemplate, imgSource);
        }

        public Rectangle deepMatch(DeepIcon icon, Image<Bgr, byte> source)
        {
            foreach(var iconPath in icon.IconList)
            {
                var template = new Image<Bgr, byte>(iconPath);
                var cmp = featureDetect(template, source);
                if(!cmp.IsEmpty)
                {
                    return cmp;
                }
            }
            return new Rectangle();
        }

        public List<Rectangle> deepMatchList(DeepIcon icon, Image<Bgr, byte> source)
        {
            List<Rectangle> items = new List<Rectangle>();
            var copy = source.Copy();
            int i = 0;
            while (true)
            {
                var cmp = deepMatch(icon, copy);

                if (cmp.IsEmpty)
                {
                    break;
                }
                else
                {
                    items.Add(cmp);
                    //copy.FillConvexPoly(pointList(cmp).ToArray(), new Bgr(Color.Black));
                    //copy.Save(DeviceInfo.SCREEN_PATH + "/tmp" + i++ + ".png");
                  

                }
            }
            foreach(var item in items)
            {
                //source.Draw(item, new Bgr(Color.Black));
                //source.Save(DeviceInfo.SCREEN_PATH + "/tmp_list.png");
            }
            return items;
        }

        public List<Rectangle> deepMatchList(DeepIcon icon, string source)
        {
            return deepMatchList(icon, new Image<Bgr, byte>(source));
        }

        public int screenHeight(string screenPath)
        {
            var img = new Image<Gray, byte>(screenPath);
            return img.Height;
        }

        public Image<Bgr, byte> cleanForGroup(string path)
        {
            var img = new Image<Bgr, byte>(path);
            var template = new Image<Bgr, byte>(IconName.FB_LITE_GROUP_SHARE_BAR);
            var rect = match(template, img);
            if(!rect.IsEmpty)
            {
                
                img.FillConvexPoly(pointList(rect).ToArray(), new Bgr(Color.White));
                var newRect = new Rectangle(0, 0, DeviceInfo.DEVICE_DEFAULT_WIDTH, rect.Y);
                img.FillConvexPoly(pointList(newRect).ToArray(), new Bgr(Color.White));
                var anotherRect = new Rectangle(0, rect.Y, DeviceInfo.DEVICE_DEFAULT_WIDTH - 170, img.Height - rect.Y);
                img.FillConvexPoly(pointList(anotherRect).ToArray(), new Bgr(Color.White));

            }
            return img;

        }
    }
}
