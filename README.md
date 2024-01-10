# Image Preprocessing for OCR Enhancement

This C# project is developed in Visual Studio and utilizes the SkiaSharp library for image preprocessing, particularly enhancing images for Optical Character Recognition (OCR) tasks. It's designed for clients who need to process images for clearer text recognition, making it ideal for applications where accurate text extraction from images is crucial.

## Features

- **Blurring**: Implemented in the `blurring` class, this feature applies Gaussian blur to reduce image noise while preserving edges, crucial for maintaining text integrity during processing.
- **Thresholding and Image Improvement**: The `thresholding` class includes adaptive thresholding for binarization, which is essential for OCR tasks. Additional functionalities like image sharpening, contrast enhancement, and edge detection are included but optional, based on the clarity of the input image.
- **Rescaling**: Reduces image size for lower computational costs and faster processing without significant loss of crucial details.
- **Multiple Processing Steps**: The program can be customized to include or exclude certain preprocessing steps based on the quality and requirements of the input image.

## Getting Started

### Prerequisites

- Visual Studio 2019 or later.
- SkiaSharp library installed (can be done via NuGet in Visual Studio).

### Installation and Setup

1. Clone the repository.
2. Open the solution file in Visual Studio.
3. Ensure SkiaSharp is installed via NuGet Package Manager.
4. Build and run the project.

### Usage

1. Place the image to be processed in an accessible directory.
2. Modify the `imagePath` variable in the `Main` method to the path of your image.
3. Run the program. Processed images will be saved at 'OCR_new\bin\Debug\net6.0'.

### Code Example

```csharp
string imagePath = "path_to_your_image.jpg";
SKBitmap inputBitmap = LoadImage(imagePath);
