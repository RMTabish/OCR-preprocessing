using SkiaSharp;

public class blurring
{
    public SKBitmap AveragingBlur(SKBitmap inputBitmap, int kernelSize)
    {
        SKBitmap blurredBitmap = new SKBitmap(inputBitmap.Width, inputBitmap.Height);
        SKPaint paint = new SKPaint
        {
            ImageFilter = SKImageFilter.CreateBlur(kernelSize, kernelSize)
        };

        using (SKCanvas canvas = new SKCanvas(blurredBitmap))
        {
            canvas.Clear(SKColors.Transparent);
            canvas.DrawBitmap(inputBitmap, 0, 0, paint);
        }

        return blurredBitmap;
    }


    public SKBitmap GaussianBlur(SKBitmap inputBitmap, float sigma)
    {
        SKBitmap blurredBitmap = new SKBitmap(inputBitmap.Width, inputBitmap.Height);
        SKPaint paint = new SKPaint
        {
            ImageFilter = SKImageFilter.CreateBlur(sigma, sigma)
        };

        using (SKCanvas canvas = new SKCanvas(blurredBitmap))
        {
            canvas.Clear(SKColors.Transparent);
            canvas.DrawBitmap(inputBitmap, 0, 0, paint);
        }

        return blurredBitmap;
    }


    public SKBitmap MedianBlurOptimized(SKBitmap inputBitmap, int kernelSize)
    {
        // Ensure that the kernel size is odd to have a central pixel
        if (kernelSize % 2 == 0) throw new ArgumentException("Kernel size must be odd.");

        int width = inputBitmap.Width;
        int height = inputBitmap.Height;
        SKBitmap outputBitmap = new SKBitmap(width, height, inputBitmap.ColorType, inputBitmap.AlphaType);

        int edge = kernelSize / 2;
        int[] histogram = new int[256];
        int medianIndex = (kernelSize * kernelSize) / 2;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Array.Clear(histogram, 0, histogram.Length);

                // Initialize histogram for the current window
                for (int i = -edge; i <= edge; i++)
                {
                    for (int j = -edge; j <= edge; j++)
                    {
                        int pixelValue = GetPixelValue(inputBitmap, x + j, y + i);
                        histogram[pixelValue]++;
                    }
                }

                int medianValue = FindMedian(histogram, medianIndex);
                outputBitmap.SetPixel(x, y, new SKColor((byte)medianValue, (byte)medianValue, (byte)medianValue));
            }
        }

        return outputBitmap;
    }

    private int GetPixelValue(SKBitmap bitmap, int x, int y)
    {
        x = Math.Max(0, Math.Min(bitmap.Width - 1, x));
        y = Math.Max(0, Math.Min(bitmap.Height - 1, y));
        var pixel = bitmap.GetPixel(x, y);
        // Use the luminance formula for grayscale conversion
        return (int)(0.299 * pixel.Red + 0.587 * pixel.Green + 0.114 * pixel.Blue);
    }

    private int FindMedian(int[] histogram, int medianIndex)
    {
        int count = 0;
        for (int i = 0; i < histogram.Length; i++)
        {
            count += histogram[i];
            if (count > medianIndex)
            {
                return i;
            }
        }
        return 0;
    }


    public SKBitmap BilateralBlur(SKBitmap inputBitmap, int diameter, float sigmaColor, float sigmaSpace)
    {
        int width = inputBitmap.Width;
        int height = inputBitmap.Height;
        SKBitmap outputBitmap = new SKBitmap(width, height);

        // Pre-compute Gaussian distance weights.
        float[] spaceWeights = new float[diameter * diameter];
        float gaussSpaceCoeff = -0.5f / (sigmaSpace * sigmaSpace);
        int radius = diameter / 2;
        int index = 0;
        for (int i = -radius; i <= radius; i++)
        {
            for (int j = -radius; j <= radius; j++)
            {
                float distance = i * i + j * j;
                spaceWeights[index++] = (float)Math.Exp(distance * gaussSpaceCoeff);
            }
        }

        // Apply bilateral filter.
        float gaussColorCoeff = -0.5f / (sigmaColor * sigmaColor);
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float sum = 0;
                float normFactor = 0;
                SKColor centerPixel = inputBitmap.GetPixel(x, y);

                // Sum over the window.
                index = 0;
                for (int i = -radius; i <= radius; i++)
                {
                    for (int j = -radius; j <= radius; j++)
                    {
                        int neighborX = Math.Max(0, Math.Min(x + j, width - 1));
                        int neighborY = Math.Max(0, Math.Min(y + i, height - 1));
                        SKColor neighborPixel = inputBitmap.GetPixel(neighborX, neighborY);

                        float colorDistance = (centerPixel.Red - neighborPixel.Red) * (centerPixel.Red - neighborPixel.Red) +
                                              (centerPixel.Green - neighborPixel.Green) * (centerPixel.Green - neighborPixel.Green) +
                                              (centerPixel.Blue - neighborPixel.Blue) * (centerPixel.Blue - neighborPixel.Blue);

                        float weight = spaceWeights[index++] * (float)Math.Exp(colorDistance * gaussColorCoeff);

                        sum += weight * (neighborPixel.Red + neighborPixel.Green + neighborPixel.Blue);
                        normFactor += weight;
                    }
                }

                // Normalize the sum.
                byte newVal = (byte)(sum / (3 * normFactor));
                outputBitmap.SetPixel(x, y, new SKColor(newVal, newVal, newVal));
            }
        }

        return outputBitmap;
    }

}