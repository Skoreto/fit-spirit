using DataAccess.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Class;

namespace WebApplication1.Class
{
    public class ProfilePhotoHelper
    {
        
        //public static void AddProfilePhoto(FitnessCentreUser user, HttpPostedFileBase profilePhoto)
        //{
        //    if (profilePhoto != null)
        //    {
        //        if (profilePhoto.ContentType == "image/jpeg" || profilePhoto.ContentType == "image/png")
        //        {
        //            Image image = Image.FromStream(profilePhoto.InputStream);

        //            if (image.Height > 200 || image.Width > 200)
        //            {
        //                Image smallImage = ImageHelper.ScaleImage(image, 200, 200);     // zmenšení fotografie
        //                Bitmap b = new Bitmap(smallImage);

        //                Guid guid = Guid.NewGuid();     // vytvoření jména fotografie
        //                string imageName = guid.ToString() + ".jpg";

        //                b.Save(Server.MapPath("~/uploads/profilePhoto/" + imageName), ImageFormat.Jpeg);    // uložení fotografie; formát jpg

                        
        //                smallImage.Dispose();   // vyčištění po e-disposable objektech
        //                b.Dispose();

        //                user.ProfilePhotoName = imageName;   // vyplnění parametru názvu fotografie
        //            }
        //            else
        //            {
        //                profilePhoto.SaveAs(Server.MapPath("~/uploads/profilePhoto/") + profilePhoto.FileName);     // uložení v případě, že fotografii není potřeba zmenšovat
        //                user.ProfilePhotoName = profilePhoto.FileName;   // TomSko chybělo vyplnění parametru názvu fotografie
        //            }
        //        }
        //    }
        //}

    }
}