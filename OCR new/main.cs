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
        string imagePath = "sabset.jpg";
        SKBitmap inputBitmap = LoadImage(imagePath);

        if (inputBitmap == null)
        {
            Console.WriteLine("Image could not be loaded.");
            return;
        }

        blurring blur = new blurring();
        thresholding thresh = new thresholding();

        // Start preprocessing steps
        // Resize the image if necessary - this can help with OCR accuracy if the text is too small


       SKBitmap resizedImage =thresh.Resize(inputBitmap, 0.6f, 0.6f);

      
        // Convert to grayscale - OCR works better on grayscale images
  //      SKBitmap grayscaleImage = thresh.ConvertToGrayscale(inputBitmap);


       
        // Applying a median blur can help reduce noise while preserving edges
       SKBitmap medianBlurred = blur.GaussianBlur(resizedImage, 1.2f);

    
     //   SKBitmap sharpenedImage = thresh.SharpenImage(medianBlurred,  1f);

    //   SKBitmap contrastEnhancedImage = thresh.EnhanceContrast(medianBlurred, 1.2f);

   //     SKBitmap edgeDetectedImage = thresh.SobelEdgeDetection(medianBlurred);


        // Apply adaptive thresholding to create a clear, binary image
        SKBitmap thresholdedImage = thresh.AdaptiveThreshold(medianBlurred, blockSize: 51, c: 15);

        // Apply erosion to thin out the text
    //    SKBitmap erodedImage = thresh.Erode(thresholdedImage, 1);
        // Save the final image
        SaveImage(thresholdedImage, "finalPlz.jpg");

        Console.WriteLine("All operations completed.");
        Console.WriteLine("All operations completed.");

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
            using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
            using (var stream = System.IO.File.OpenWrite(path))
            {
                data.SaveTo(stream);
            }
        }

    }

}