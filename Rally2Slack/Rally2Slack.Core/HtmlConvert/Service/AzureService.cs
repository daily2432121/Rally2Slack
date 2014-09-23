using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Rally2Slack.Core.HtmlConvert.Service
{
    public class AzureService
    {
        public string Upload(Image img, string fileName)
        {
            StorageCredentials sc = new StorageCredentials("kanbanimages", "uQ6uECREYvii9I1yDicadX0ftY3AdIBQstsRtbtdV2tgxHhOi6zxIh4EyNxQozWiNjQmRXQigXJtiMZNyn9A4Q==");    
            CloudStorageAccount acc = new CloudStorageAccount(sc,false);
            CloudBlobClient bc = acc.CreateCloudBlobClient();
            CloudBlobContainer bcon = bc.GetContainerReference("kanbanimage");
            CloudBlockBlob cbb = bcon.GetBlockBlobReference(fileName + DateTime.Now.ToString("o").Replace(":", "_")+".jpg");
            cbb.Properties.ContentType = @"image\jpg";
            using (MemoryStream ms = new MemoryStream())
            {
                img.Save(ms, ImageFormat.Jpeg);
                byte[] b = ms.GetBuffer();
                cbb.UploadFromByteArray(b,0,b.Count());
            }
            //BlobContainerPermissions bpc =new BlobContainerPermissions();
            //var shared60days = new SharedAccessBlobPolicy()
            //{
            //    SharedAccessExpiryTime = DateTime.UtcNow.AddDays(60),
            //    Permissions = SharedAccessBlobPermissions.Read
            //};
            //bpc.SharedAccessPolicies.Add("sharedKanban", shared60days);
            //bpc.PublicAccess = BlobContainerPublicAccessType.Blob;
            //bcon.SetPermissions(bpc);
            var path =cbb.Uri.AbsoluteUri;
            //string sas = bcon.GetSharedAccessSignature(shared60days, "sharedKanban");
            return path;
        }

        
    }
}
