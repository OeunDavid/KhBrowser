using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolLib.Img;

namespace ToolLib.Test
{
    public class ImageHelperTest
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public static void testCmp()
        {
            var screen1 = @"D:\RD\android\FacebookApp\ToolLib\bin\Debug\screen\R58M464RBRW.png";
            var screen2 = @"D:\RD\android\FacebookApp\wpf_ui\bin\Debug\screen\06157df6a3ef6e09.png";
            var icon = @"D:\RD\android\FacebookApp\ToolLib\bin\Debug\icon\lite\2fa_ok_button.png";
            var imgHelper = new ImageHelper();
            TemplateMatchingType[] templateTypes = {
                TemplateMatchingType.CcorrNormed,
                TemplateMatchingType.Ccoeff,
                TemplateMatchingType.Ccorr,
                TemplateMatchingType.CcoeffNormed,
                TemplateMatchingType.Sqdiff,
                TemplateMatchingType.SqdiffNormed
            }; 

            foreach ( var templateType in templateTypes)
            {
                log.Info("template type : " + templateType);
                var search1 = imgHelper.match(icon, screen1, templateType);
                log.Info("search 1 ->  "  +screen1 +" "+  ( !search1.IsEmpty) +" -> " + search1);
                var search2 = imgHelper.match(icon, screen2, templateType);
                log.Info("search 2 -> " + screen2 + " " + ( !search2.IsEmpty) +" -> " + search2);
            }
            
        }
        public static void testOcr()
        {
            var imgHelper = ToolDiConfig.Get<IImageHelper>();
            var imgPath = @"D:\RD\android\FacebookApp\ToolLib\bin\Debug\icon\lite\login_another_account1.png";
            var img = new Image<Bgr, byte>(imgPath);
            var str = imgHelper.ocr(img)+"";
            Console.WriteLine("result :" + str.Trim());
        }
        public static void TestMatchList()
        {
            var imgHelper = ToolDiConfig.Get<IImageHelper>();
            var path = @"D:\RD\android\FacebookApp\ToolLib\bin\Debug\screen\06157df6a3ef6e09.png";
            var img = new Image<Bgr, byte>(path);
            var iconPath = @"D:\RD\android\FacebookApp\ToolLib\bin\Debug\icon\lite\group_icon2.png";
            var icon = new Image<Bgr, byte>(iconPath);
            var list = imgHelper.matchList(icon, img);
            Console.WriteLine("match :" + list.Count);
        }

        public static void TestFeatureMatch()
        {
               var screen = @"D:\RD\android\FacebookApp\ToolLib\bin\Debug\screen\df3b90f6.png";
            //var screen = @"D:\RD\android\FacebookApp\ToolLib\bin\Debug\screen\22X0219A31008771.png";
            var icon = @"D:\RD\android\FacebookApp\ToolLib\bin\Debug\icon\lite\share_text_box_3.png";
            var source = new Image<Bgr, byte>(screen);
            var template = new Image<Bgr, byte>(icon);
            var imgHelper = ToolDiConfig.Get<IImageHelper>();
    
            var rect = imgHelper.featureDetect(template, source);
            var bRect = imgHelper.anotherMatch(icon, screen);// 
            //Console.WriteLine(" match rect  :" + (!rect.IsEmpty) + " " + rect);
            //ImageHelper.ProcessImage(template, source);
            //ImageHelper.ProcessImageFLANN(template, source);
            log.Info(" f detect " + rect + " vs another match : " + bRect);

        }
        public static void TestMatchItem()
        {
            var screen = @"D:\RD\android\FacebookApp\ToolLib\bin\Debug\screen\backup\allow_contact_j8.jpg";
            //var screen = @"D:\RD\android\FacebookApp\ToolLib\bin\Debug\screen\22X0219A31008771.png";

            //var source = new Image<Bgr, byte>(screen);

            var imgHelper = ToolDiConfig.Get<IImageHelper>();
            var rect = imgHelper.match(IconItem.FB_LITE_ALLOW_CONTACT, screen);
            log.Info("rect : " + rect);

        }
        public static void TestDeepMatch()
        {
            var imgHelper = ToolDiConfig.Get<IImageHelper>();
            var path = @"D:\RD\android\FacebookApp\ToolLib\bin\Debug\screen\06157df6a3ef6e09.png";
           // var match = imgHelper.deepMatch(DeepIcon.FB_LITE_GROUP_ICON, new Image<Bgr, byte>(path));
           //log.Info(" result : " + match);
        }
        public static void TestDeepMatchList()
        {
            var imgHelper = ToolDiConfig.Get<IImageHelper>();
            var path = @"D:\RD\android\FacebookApp\ToolLib\bin\Debug\screen\backup\video_view.png";
            var source = new Image<Bgr, byte>(path);
            source = imgHelper.resize(source, DeviceInfo.DEVICE_DEFAULT_WIDTH * 2);
            var tmpSource = source.Not().SmoothGaussian(3);
            var template = new Image<Bgr, byte>(@"D:\RD\android\FacebookApp\ToolLib\bin\Debug\icon\lite\share_to_group.png");
            template = imgHelper.resize(template, template.Width * 2).Not().SmoothGaussian(3);
            //var match = imgHelper.anotherMatch(template, tmpSource);
            //var newMatch = new Rectangle(match.X / 2, match.Y / 2, match.Width / 2, match.Height / 2);
            //var point = imgHelper.midPoint("22X0219A31008771", newMatch);
            //log.Info("match " + match + " , point : "+point);
            var list = imgHelper.matchList(@"D:\RD\android\FacebookApp\ToolLib\bin\Debug\icon\lite\share_to_group.png", path);
            Console.WriteLine("group size : " + list.Count);
            foreach(var r in list)
            {
                Console.WriteLine("point " + imgHelper.midPoint("22X0219A31008771", r));
            }
            //  source.Save(DeviceInfo.SCREEN_PATH + "/not.png");
            //  
            // log.Info("match " + match);
            //   var mL = imgHelper.matchList(path, DeviceInfo.SCREEN_PATH + "/n5ot.png");
            //log.Info("ml " + mL);
            // var match = imgHelper.deepMatchList(DeepIcon.FB_LITE_GROUP_ICON, new Image<Bgr, byte>(path));
            //  log.Info(" result : " + match);
        
        
        
        }
    }
}
