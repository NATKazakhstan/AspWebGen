using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Web;
using System.Web.UI.WebControls;
using Image=System.Drawing.Image;

namespace Nat.Web.Tools
{
    public class ImageUtils
    {
        private static Image inputImage;

        // вспомогательный метод - получение нужного кодека
        private static ImageCodecInfo getCodecInfo(string mt)
        {
            ImageCodecInfo[] ici = ImageCodecInfo.GetImageEncoders();
            int idx = 0;
            for (int ii = 0; ii < ici.Length; ii++)
            {
                if (ici[ii].MimeType == mt)
                {
                    idx = ii;
                    break;
                }
            }
            return ici[idx];
        }

        private static void GetThumbnail(string fname, int width, int heigth, HttpRequest request)
        {
            string physicalPath = fname;
            physicalPath = request.MapPath(physicalPath);
            inputImage = GetImage(fname, request);
            // размен тамбнейла - 108*81
//            Bitmap b = new Bitmap(width, heigth);
            Bitmap b = new Bitmap(width, heigth);
            Graphics gTemp = Graphics.FromImage(b);
            gTemp.InterpolationMode = InterpolationMode.Bicubic;
            gTemp.PixelOffsetMode = PixelOffsetMode.HighQuality;
            gTemp.DrawImage(inputImage, 0, 0, width, heigth);

            EncoderParameters ep = new EncoderParameters(1);
            ImageCodecInfo icJPG = getCodecInfo("image/jpeg");
            // качество jpeg
            ep.Param[0] = new EncoderParameter(Encoder.Quality, (long) 80);

            b.Save(GetThumbnailPath(physicalPath, heigth, width), icJPG, ep);
            gTemp.Dispose();
            b.Dispose();
            inputImage.Dispose();
        }

        private static Image GetImage(string fName, HttpRequest request)
        {
            string physicalPath = fName;
            Image image;
            physicalPath = request.MapPath(physicalPath);

            try
            {
                image = 
                    Image.FromFile(physicalPath);
            }
            catch (FileNotFoundException)
            {
                throw new FileNotFoundException("File not found");
            }
            
            return image;
        }

        /// <summary>
        /// Вычисление iирины пропорционально высоте
        /// </summary>
        /// <param name="fName">Имя входного файла</param>
        /// <param name="newHeigth">высота обработанного файла</param>
        /// <param name="request">Page.Request</param>
        /// <returns></returns>
        public static Unit SaveProportion(string fName, int newHeigth, HttpRequest request)
        {
            inputImage = GetImage(fName, request);
            return inputImage.Width * newHeigth / inputImage.Height;
        }

        private static string GetThumbnailPath(string fname, int heigth, int width)
        {
            string pathNew = Path.GetDirectoryName(fname) + "\\" + Path.GetFileNameWithoutExtension(fname) + "_t" +
                             width + "x" + heigth + ".jpg";
            pathNew = pathNew.Replace("..\\", "");
            return pathNew;
        }

        public static string GetThumbPath(string fname, int width, int heigth, HttpRequest request)
        {
            try
            {
                string thumbnailPath = GetThumbnailPath(fname, heigth, width);
                FileInfo fileInfo = new FileInfo(request.MapPath(thumbnailPath));
                if (!fileInfo.Exists)
                {
                    GetThumbnail(fname, width, heigth, request);
                }
                return thumbnailPath;
            }
            catch
            {
                return fname;
            }
        }

        /// <summary>
        /// Изменение размеров изображения и рисование уголка для фотографий на документы
        /// </summary>
        /// <param name="buffer">Картинка</param>
        /// <param name="request">Page.Request</param>
        public static byte[] ResizingGraphicsFile(byte[] buffer, HttpRequest request)
        {
            if (request.QueryString["width"] != null && request.QueryString["height"] != null)
            {
                var dwidth =
                    double.Parse(
                        request.QueryString["width"].Replace(",",
                                                             CultureInfo.InvariantCulture.NumberFormat.
                                                                 NumberDecimalSeparator).Replace(".",
                                                                                                 CultureInfo.
                                                                                                     InvariantCulture.
                                                                                                     NumberFormat.
                                                                                                     NumberDecimalSeparator),
                        CultureInfo.InvariantCulture);
                var dheight =
                    double.Parse(
                        request.QueryString["height"].Replace(",",
                                                              CultureInfo.InvariantCulture.NumberFormat.
                                                                  NumberDecimalSeparator).Replace(".",
                                                                                                  CultureInfo.
                                                                                                      InvariantCulture.
                                                                                                      NumberFormat.
                                                                                                      NumberDecimalSeparator),
                        CultureInfo.InvariantCulture);
                var withCorner = Convert.ToBoolean(request.QueryString["WithCorner"]);
                buffer = ResizingGraphicsFile(buffer, dwidth, dheight, withCorner);
            }
            return buffer;
        }

        public static byte[] ResizingGraphicsFile(byte[] buffer, double dwidth, double dheight, bool withCorner)
        {
            var bmp = ResizingGraphicsImage(buffer, dwidth, dheight, withCorner);
            using (var stream = new MemoryStream())
            {
                bmp.Save(stream, ImageFormat.Jpeg);
                stream.Flush();
                buffer = stream.ToArray();
            }
            return buffer;
        }

        public static Bitmap ResizingGraphicsImage(byte[] buffer, double dwidth, double dheight, bool withCorner)
        {
            Image img;
            using (var memoryStream = new MemoryStream(buffer))
            {
                img = Image.FromStream(memoryStream);
            }

            return ResizubgGraphicsImage(dwidth, dheight, withCorner, img);
        }

        public static Bitmap ResizubgGraphicsImage(double dwidth, double dheight, bool withCorner, Image img)
        {
            var width = Convert.ToInt32(dwidth * img.HorizontalResolution / 2.54);
            var height = Convert.ToInt32(dheight * img.VerticalResolution / 2.54);
            var bmp = new Bitmap(img, width, height);
            bmp.SetResolution(img.HorizontalResolution, img.VerticalResolution);
            if (withCorner)
            {
                using (var g = Graphics.FromImage(bmp))
                {
                    var point1 = new Point(width / 3 * 2, height);
                    var point2 = new Point(width, height - (width / 3));
                    var point3 = new Point(width, height);
                    Point[] curvePoints =
                        {
                            point1,
                            point2,
                            point3,
                        };
                    g.FillPolygon(new SolidBrush(Color.White), curvePoints);
                }
            }

            return bmp;
        }
    }
}