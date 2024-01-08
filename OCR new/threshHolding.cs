using SkiaSharp;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class thresholding
{
    public SKBitmap SharpenImage(SKBitmap original, float amount)
    {
        SKBitmap sharpenedBitmap = new SKBitmap(original.Width, original.Height, SKColorType.Bgra8888, SKAlphaType.Premul);

        using (var canvas = new SKCanvas(sharpenedBitmap))
        {
            canvas.Clear(SKColors.White);

            for (int y = 1; y < original.Height - 1; y++)
            {
                for (int x = 1; x < original.Width - 1; x++)
                {
                    SKColor originalColor = original.GetPixel(x, y);
                    float edgeDetection = (
                        GetIntensity(original.GetPixel(x - 1, y - 1)) +
                        GetIntensity(original.GetPixel(x, y - 1)) +
                        GetIntensity(original.GetPixel(x + 1, y - 1)) +
                        GetIntensity(original.GetPixel(x - 1, y)) - 8 * GetIntensity(originalColor) +
                        GetIntensity(original.GetPixel(x + 1, y)) +
                        GetIntensity(original.GetPixel(x - 1, y + 1)) +
                        GetIntensity(original.GetPixel(x, y + 1)) +
                        GetIntensity(original.GetPixel(x + 1, y + 1))
                    ) * amount;

                    int r = ClampColor(originalColor.Red + (int)edgeDetection);
                    int g = ClampColor(originalColor.Green + (int)edgeDetection);
                    int b = ClampColor(originalColor.Blue + (int)edgeDetection);

                    SKColor newColor = new SKColor((byte)r, (byte)g, (byte)b);
                    sharpenedBitmap.SetPixel(x, y, newColor);
                }
            }
        }

        return sharpenedBitmap;
    }

    private static float GetIntensity(SKColor color)
    {
        return 0.299f * color.Red + 0.587f * color.Green + 0.114f * color.Blue;
    }

    private static int ClampColor(int value)
    {
        return Math.Max(0, Math.Min(255, value));
    }

    public SKBitmap SobelEdgeDetection(SKBitmap inputBitmap)
    {
        int width = inputBitmap.Width;
        int height = inputBitmap.Height;
        SKBitmap edgeBitmap = new SKBitmap(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);

        // Define the Sobel kernel for horizontal and vertical edge detection
        int[,] xSobel = { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } };
        int[,] ySobel = { { -1, -2, -1 }, { 0, 0, 0 }, { 1, 2, 1 } };

        for (int y = 1; y < height - 1; y++)
        {
            for (int x = 1; x < width - 1; x++)
            {
                float xGradient = 0;
                float yGradient = 0;

                // Apply the Sobel kernel to the current pixel neighborhood
                for (int dy = -1; dy <= 1; dy++)
                {
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        SKColor neighborColor = inputBitmap.GetPixel(x + dx, y + dy);
                        int neighborIntensity = (neighborColor.Red + neighborColor.Green + neighborColor.Blue) / 3;

                        xGradient += neighborIntensity * xSobel[dy + 1, dx + 1];
                        yGradient += neighborIntensity * ySobel[dy + 1, dx + 1];
                    }
                }

                // Compute the magnitude of the gradient
                int gradientMagnitude = (int)Math.Sqrt(xGradient * xGradient + yGradient * yGradient);
                gradientMagnitude = Math.Min(255, gradientMagnitude); // Cap the values at 255

                // Set the edge color as white if the gradient magnitude is high, black otherwise
                SKColor edgeColor = gradientMagnitude > 128 ? SKColors.White : SKColors.Black;
                edgeBitmap.SetPixel(x, y, edgeColor);
            }
        }

        return edgeBitmap;
    }

    public SKBitmap SimpleThreshold(SKBitmap original, byte threshold)
    {
        // Create a new bitmap with the same dimensions as the original.
        SKBitmap thresholded = new SKBitmap(original.Width, original.Height, SKColorType.Bgra8888, SKAlphaType.Premul);

        // Iterate over all pixels to apply the threshold.
        for (int y = 0; y < original.Height; y++)
        {
            for (int x = 0; x < original.Width; x++)
            {
                // Get the pixel value (assuming it is already in a grayscale format).
                SKColor originalColor = original.GetPixel(x, y);
                byte intensity = (byte)(0.2126 * originalColor.Red + 0.7152 * originalColor.Green + 0.0722 * originalColor.Blue);

                // Apply the threshold.
                SKColor newColor = intensity >= threshold ? SKColors.White : SKColors.Black;
                thresholded.SetPixel(x, y, newColor);
            }
        }

        return thresholded;
    }
    public SKBitmap AdaptiveThreshold(SKBitmap original, int blockSize, int c)
    {
        int width = original.Width;
        int height = original.Height;
        SKBitmap thresholded = new SKBitmap(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);

        int[,] integralImage = BuildIntegralImage(original);

        int halfBlockSize = blockSize / 2;

        // Using parallel for loop for y-axis
        Parallel.For(0, height, y =>
        {
            for (int x = 0; x < width; x++)
            {
                int x1 = Math.Max(0, x - halfBlockSize);
                int x2 = Math.Min(width - 1, x + halfBlockSize);
                int y1 = Math.Max(0, y - halfBlockSize);
                int y2 = Math.Min(height - 1, y + halfBlockSize);

                int area = (x2 - x1) * (y2 - y1);
                int sum = integralImage[y2, x2] - integralImage[y1, x2] - integralImage[y2, x1] + integralImage[y1, x1];

                SKColor originalColor = original.GetPixel(x, y);
                byte intensity = (byte)(0.2126 * originalColor.Red + 0.7152 * originalColor.Green + 0.0722 * originalColor.Blue);
                byte localMean = (byte)(sum / area);

                SKColor newColor = intensity <= (localMean - c) ? SKColors.Black : SKColors.White;

                lock (thresholded)
                {
                    thresholded.SetPixel(x, y, newColor);
                }
            }
        });

        return thresholded;
    }

    private int[,] BuildIntegralImage(SKBitmap bitmap)
    {
        int width = bitmap.Width;
        int height = bitmap.Height;
        int[,] integralImage = new int[height, width];

        for (int y = 0; y < height; y++)
        {
            int sum = 0;
            for (int x = 0; x < width; x++)
            {
                SKColor color = bitmap.GetPixel(x, y);
                sum += (byte)(0.2126 * color.Red + 0.7152 * color.Green + 0.0722 * color.Blue);
                if (y == 0)
                    integralImage[y, x] = sum;
                else
                    integralImage[y, x] = integralImage[y - 1, x] + sum;
            }
        }

        return integralImage;
    }
    public SKBitmap OtsuThreshold(SKBitmap original)
    {
        int width = original.Width;
        int height = original.Height;
        SKBitmap thresholded = new SKBitmap(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);

        // Histogram array
        int[] histogram = new int[256];
        int total = width * height;

        // Compute the histogram
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                SKColor color = original.GetPixel(x, y);
                int luminance = (int)(0.2126 * color.Red + 0.7152 * color.Green + 0.0722 * color.Blue);
                histogram[luminance]++;
            }
        }

        // Total mean level
        int sum = 0;
        for (int t = 0; t < 256; t++) sum += t * histogram[t];

        int sumB = 0;
        int wB = 0;
        int wF = 0;
        float varMax = 0;
        int threshold = 0;

        for (int t = 0; t < 256; t++)
        {
            wB += histogram[t];               // Weight Background
            if (wB == 0) continue;

            wF = total - wB;                  // Weight Foreground
            if (wF == 0) break;

            sumB += t * histogram[t];

            float mB = sumB / (float)wB;      // Mean Background
            float mF = (sum - sumB) / (float)wF; // Mean Foreground

            // Calculate Between Class Variance
            float varBetween = (float)wB * (float)wF * (mB - mF) * (mB - mF);

            // Check if new maximum found
            if (varBetween > varMax)
            {
                varMax = varBetween;
                threshold = t;
            }
        }

        // Apply the threshold
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                SKColor color = original.GetPixel(x, y);
                int luminance = (int)(0.2126 * color.Red + 0.7152 * color.Green + 0.0722 * color.Blue);
                SKColor newColor = luminance > threshold ? SKColors.White : SKColors.Black;
                thresholded.SetPixel(x, y, newColor);
            }
        }

        return thresholded;
    }


    public  SKBitmap Resize(SKBitmap original, float scaleX, float scaleY)
    {
        int newWidth = (int)(original.Width * scaleX);
        int newHeight = (int)(original.Height * scaleY);
        SKBitmap resized = original.Resize(new SKImageInfo(newWidth, newHeight), SKFilterQuality.High);
        return resized;
    }

    public  SKBitmap ConvertToGrayscale(SKBitmap original)
    {
        SKBitmap grayscale = new SKBitmap(original.Width, original.Height, SKColorType.Gray8, SKAlphaType.Premul);
        using (SKCanvas canvas = new SKCanvas(grayscale))
        {
            canvas.Clear(SKColors.White);
            SKPaint paint = new SKPaint
            {
                ColorFilter = SKColorFilter.CreateColorMatrix(new float[]
                {
                    0.2126f, 0.7152f, 0.0722f, 0, 0,
                    0.2126f, 0.7152f, 0.0722f, 0, 0,
                    0.2126f, 0.7152f, 0.0722f, 0, 0,
                    0, 0, 0, 1, 0
                })
            };
            canvas.DrawBitmap(original, 0, 0, paint);
        }
        return grayscale;
    }

    public  SKBitmap Erode(SKBitmap original, int kernelSize)
    {
        SKBitmap eroded = new SKBitmap(original.Width, original.Height, SKColorType.Bgra8888, SKAlphaType.Premul);
        int radius = kernelSize / 2;

        for (int y = 0; y < original.Height; y++)
        {
            for (int x = 0; x < original.Width; x++)
            {
                SKColor minVal = SKColors.White;
                for (int dy = -radius; dy <= radius; dy++)
                {
                    for (int dx = -radius; dx <= radius; dx++)
                    {
                        int px = x + dx;
                        int py = y + dy;
                        if (px >= 0 && px < original.Width && py >= 0 && py < original.Height)
                        {
                            SKColor pixelColor = original.GetPixel(px, py);
                            if (pixelColor.Red < minVal.Red)
                            {
                                minVal = pixelColor;
                            }
                        }
                    }
                }
                eroded.SetPixel(x, y, minVal);
            }
        }

        return eroded;
    }

    public  SKBitmap Dilate(SKBitmap original, int kernelSize)
    {
        SKBitmap dilated = new SKBitmap(original.Width, original.Height, SKColorType.Bgra8888, SKAlphaType.Premul);
        int radius = kernelSize / 2;

        for (int y = 0; y < original.Height; y++)
        {
            for (int x = 0; x < original.Width; x++)
            {
                SKColor maxVal = SKColors.Black;
                for (int dy = -radius; dy <= radius; dy++)
                {
                    for (int dx = -radius; dx <= radius; dx++)
                    {
                        int px = x + dx;
                        int py = y + dy;
                        if (px >= 0 && px < original.Width && py >= 0 && py < original.Height)
                        {
                            SKColor pixelColor = original.GetPixel(px, py);
                            if (pixelColor.Red > maxVal.Red)
                            {
                                maxVal = pixelColor;
                            }
                        }
                    }
                }
                dilated.SetPixel(x, y, maxVal);
            }
        }

        return dilated;
    }
    public  SKBitmap ApplyThreshold(SKBitmap original, int threshold)
    {
        SKBitmap thresholded = new SKBitmap(original.Width, original.Height, SKColorType.Gray8, SKAlphaType.Premul);
        for (int y = 0; y < original.Height; y++)
        {
            for (int x = 0; x < original.Width; x++)
            {
                byte pixelValue = original.GetPixel(x, y).Red; // Assuming the image is grayscale
                thresholded.SetPixel(x, y, pixelValue > threshold ? SKColors.White : SKColors.Black);
            }
        }
        return thresholded;
    }

    public SKBitmap EnhanceContrast(SKBitmap original, float contrast)
    {
        // Create a new empty bitmap with the same properties
        SKBitmap enhanced = new SKBitmap(original.Width, original.Height, original.ColorType, original.AlphaType);

        // Calculate contrast correction factor
        float correctionFactor = (259 * (contrast + 255)) / (255 * (259 - contrast));

        using (SKCanvas canvas = new SKCanvas(enhanced))
        {
            canvas.Clear(SKColors.Transparent);

            // Scan through each pixel row
            for (int y = 0; y < original.Height; y++)
            {
                // Scan through each pixel column
                for (int x = 0; x < original.Width; x++)
                {
                    // Get the original color
                    SKColor originalColor = original.GetPixel(x, y);

                    // Apply contrast formula for each channel
                    float red = Trim(contrast * (originalColor.Red / 255.0f - 0.5f) + 0.5f);
                    float green = Trim(contrast * (originalColor.Green / 255.0f - 0.5f) + 0.5f);
                    float blue = Trim(contrast * (originalColor.Blue / 255.0f - 0.5f) + 0.5f);

                    // Set the new color to the enhanced bitmap
                    enhanced.SetPixel(x, y, new SKColor((byte)(red * 255), (byte)(green * 255), (byte)(blue * 255)));
                }
            }
        }

        return enhanced;
    }

    private static float Trim(float value)
    {
        if (value < 0) return 0;
        if (value > 1) return 1;
        return value;
    }

}
