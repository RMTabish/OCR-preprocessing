using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;
using System;

class Program
{
    static void Main(string[] args)
    {
        // Load an image
        string imagePath = "ocrtest2.png"; 
        SKBitmap inputBitmap = LoadImage(imagePath);

        if (inputBitmap == null)
        {
            Console.WriteLine("Image could not be loaded.");
            return;
        }

        blurring blur = new blurring();

        // Apply Averaging Blur
        //SKBitmap averagingBlurred =blur.AveragingBlur(inputBitmap, 5);
        //SaveImage(averagingBlurred, "averaging_blurred.jpg");

        //// Apply Gaussian Blur
        //SKBitmap gaussianBlurred = GaussianBlur(inputBitmap, 5);
        //SaveImage(gaussianBlurred, "gaussian_blurred.jpg");

        //// Apply Median Blur
        SKBitmap medianBlurred = blur.MedianBlurOptimized(inputBitmap, 5);
        SaveImage(medianBlurred, "median_blurred.jpg");

        // Apply Bilateral Filter
        SKBitmap bilateralFiltered = blur.BilateralBlur(inputBitmap, 5, 75, 75); // Implementation needed
        SaveImage(bilateralFiltered, "bilateral_filtered.jpg");

        Console.WriteLine("All operations completed.");
    }

    static SKBitmap LoadImage(string path)
    {
        if (!System.IO.File.Exists(path))
        {
            return null;
        }

        using (var stream = new SKFileStream(path))
        {
            return SKBitmap.Decode(stream);
        }
    }

    static void SaveImage(SKBitmap bitmap, string path)
    {
        using (var image = SKImage.FromBitmap(bitmap))
        using (var data = image.Encode(SKEncodedImageFormat.Jpeg, 100))
        using (var stream = System.IO.File.OpenWrite(path))
        {
            data.SaveTo(stream);
        }
    }

}
