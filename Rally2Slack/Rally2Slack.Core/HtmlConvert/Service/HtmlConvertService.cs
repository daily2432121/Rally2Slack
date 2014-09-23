using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Windows.Forms;
using HtmlRenderer;


namespace Rally2Slack.Core.HtmlConvert.Service
{
    
    public class HtmlConvertService
    {
        public Image ConvertToJpeg(string html)
        {
            try
            {
                //HtmlRender
                Image image = HtmlRender.RenderToImageGdiPlus(html);
                return image;

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return null;
            }

        }

        public string ConvertToJpegFromUrl(string url, string imageSaveFolder)
        {
            try
            {
                var bytes = ToBitMapArray(new Uri(url));
                var stamp = DateTime.Now.ToString("o").Replace(":", "_");
                var root = HostingEnvironment.MapPath("~");
                string name = @"Kanban_" + stamp + ".jpg";
                using (Image img = Image.FromStream(new MemoryStream(bytes)))
                {

                    img.Save(root + @"Upload\" + name, ImageFormat.Png);
                }
                return name;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return null;
            }


        }


        /// <summary>
        /// Convert url to bitmap byte array
        /// </summary>
        /// <param name="url">Url to browse</param>
        /// <param name="width">width of page (if page contains frame, you need to pass this params)</param>
        /// <param name="height">heigth of page (if page contains frame, you need to pass this params)</param>
        /// <param name="htmlToManipulate">function to manipulate dom</param>
        /// <param name="timeout">in milliseconds, how long can you wait for page response?</param>
        /// <returns>bitmap byte[]</returns>
        /// <example>
        /// byte[] img = new Uri("http://www.uol.com.br").ToImage();
        /// </example>
        [STAThread]
        public byte[] ToBitMapArray(Uri url, int? width = null, int? height = null, Action<HtmlDocument> htmlToManipulate = null, int timeout = -1)
        {
            byte[] toReturn = null;

            var waiter = new ManualResetEvent(false);
            Bitmap screenshot = null;

            // Spin up an STA thread to do the web browser work:
            var staThread = new Thread(() =>
            {
                WebBrowser browser = new WebBrowser() { ScrollBarsEnabled = false };
                browser.DocumentCompleted += (sender, e) =>
                {
                    if (htmlToManipulate != null) htmlToManipulate(browser.Document);
                    browser.ClientSize = new Size(width ?? browser.Document.Body.ScrollRectangle.Width, height ?? browser.Document.Body.ScrollRectangle.Bottom);
                    browser.ScrollBarsEnabled = false;
                    browser.BringToFront();
                    using (Bitmap bmp = new Bitmap(browser.Document.Body.ScrollRectangle.Width, browser.Document.Body.ScrollRectangle.Bottom))
                    {
                        browser.DrawToBitmap(bmp, browser.Bounds);
                        Debug.WriteLine("Height:"+browser.Bounds.Height);
                        Debug.WriteLine("Width:" + browser.Bounds.Height);
                        toReturn = (byte[]) new ImageConverter().ConvertTo(bmp, typeof (byte[]));
                    }

                    waiter.Set(); // Signal the web service thread we're done.
                };
                browser.Navigate(url);
            });
            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start();

            var t = TimeSpan.FromSeconds(10);
            waiter.WaitOne(t); // Wait for the STA thread to finish.
            return toReturn;
        }
    }
}
