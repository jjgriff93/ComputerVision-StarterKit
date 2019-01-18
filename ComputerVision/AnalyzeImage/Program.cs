using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ImageAnalyze
{
    class Program
    {
        // subscriptionKey = "0123456789abcdef0123456789ABCDEF"
        private const string subscriptionKey = "24fd7acc5423450c932d719cec2bf98a";

        // localImagePath = @"C:\Documents\LocalImage.jpg"
        private const string localImagePath = @"C:\Users\jamesgr\Desktop\cognitive-services-vision-csharp-sdk-quickstarts\ComputerVision\Mount Rushmore.jpg";

        private const string remoteImageUrl =
            "https://m.media-amazon.com/images/M/MV5BMTc0MDMyMzI2OF5BMl5BanBnXkFtZTcwMzM2OTk1MQ@@._V1_UX214_CR0,0,214,317_AL_.jpg";

        // Specify the features to return
        private static readonly List<VisualFeatureTypes> features =
            new List<VisualFeatureTypes>()
        {
            VisualFeatureTypes.Categories,
            VisualFeatureTypes.Description,
            VisualFeatureTypes.Faces,
            VisualFeatureTypes.ImageType,
            VisualFeatureTypes.Tags,
            VisualFeatureTypes.Adult,
            VisualFeatureTypes.Color
        };

        static void Main(string[] args)
        {
            ComputerVisionClient computerVision = new ComputerVisionClient(
                new ApiKeyServiceClientCredentials(subscriptionKey),
                new System.Net.Http.DelegatingHandler[] { });

            // You must use the same region as you used to get your subscription
            // keys. For example, if you got your subscription keys from westus,
            // replace "westcentralus" with "westus".
            //
            // Free trial subscription keys are generated in the westcentralus
            // region. If you use a free trial subscription key, you shouldn't
            // need to change the region.

            // Specify the Azure region
            computerVision.Endpoint = "https://westeurope.api.cognitive.microsoft.com";

            Console.WriteLine("Hello Azure Brownbag attendees. Prepare for the greatest demo you've ever seen.");
            Console.WriteLine("Press ENTER to begin analysing your images");
            Console.ReadLine();

            Console.Clear();
            Console.WriteLine("Images being analysed ...\n");
            var t1 = AnalyzeRemoteAsync(computerVision, remoteImageUrl);
            var t2 = AnalyzeLocalAsync(computerVision, localImagePath);

            Task.WhenAll(t1, t2).Wait(10000);

            Console.WriteLine("\nPress ENTER to exit");
            Console.ReadLine();
        }

        // Analyze a remote image
        private static async Task AnalyzeRemoteAsync(
            ComputerVisionClient computerVision, string imageUrl)
        {
            if (!Uri.IsWellFormedUriString(imageUrl, UriKind.Absolute))
            {
                Console.WriteLine(
                    "\nInvalid remoteImageUrl:\n{0} \n", imageUrl);
                return;
            }

            ImageAnalysis analysis =
                await computerVision.AnalyzeImageAsync(imageUrl, features);
            DisplayResults(analysis, imageUrl);
        }

        // Analyze a local image
        private static async Task AnalyzeLocalAsync(
            ComputerVisionClient computerVision, string imagePath)
        {
            if (!File.Exists(imagePath))
            {
                Console.WriteLine(
                    "\nUnable to open or read localImagePath:\n{0} \n", imagePath);
                return;
            }

            using (Stream imageStream = File.OpenRead(imagePath))
            {
                ImageAnalysis analysis = await computerVision.AnalyzeImageInStreamAsync(
                    imageStream, features);
                DisplayResults(analysis, imagePath);
            }
        }

        // Display the relevant information detected for the images
        private static void DisplayResults(ImageAnalysis analysis, string imageUri)
        {
            string resultsOutput = $"\nImage result for {imageUri}\n==================\n\n";

            if (analysis.Description.Captions != null)
            {
                resultsOutput += "DESCRIPTION: " + analysis.Description.Captions[0].Text + "\n";
            }

            if (analysis.ImageType != null)
            {
                resultsOutput += "IMAGE TYPE: ";
                if (analysis.ImageType.ClipArtType > 0.7)
                {
                    resultsOutput += "I think this is clip art.\n";
                }
                else if (analysis.ImageType.LineDrawingType > 0.7)
                {
                    resultsOutput += "I think this is a drawing.\n";
                }
                else
                {
                    resultsOutput += "This isn't clipart or a drawing.\n";
                }
            }

            if (analysis.Faces != null && analysis.Faces.Count > 0)
            {
                resultsOutput += "FACES: " + analysis.Faces.Count + " faces detected\n";
                int faceCount = 1;
                foreach (var face in analysis.Faces)
                {
                    resultsOutput += $"    Face no. {faceCount} is {face.Gender}, their age is {face.Age} and it is located in coordinates X{face.FaceRectangle.Left} Y{face.FaceRectangle.Top}\n";
                    faceCount++;
                }
            }

            if (analysis.Adult != null)
            {
                if (analysis.Adult.IsAdultContent)
                {
                    resultsOutput += $"ADULT: This image contains adult content. Adult score: {analysis.Adult.AdultScore}\n";
                }
                else
                {
                    resultsOutput += $"ADULT: This image does not contain adult content. Adult score: {analysis.Adult.AdultScore}\n";
                }
            }

            if (analysis.Categories != null)
            {
                resultsOutput += "CATEGORIES: " + analysis.Categories.Count + " categories detected: ";
                foreach (var category in analysis.Categories)
                {
                    if (category.Detail == null)
                    {
                        resultsOutput += $"{category.Name}, ";
                    }
                    else
                    {
                        resultsOutput += $"{category.Name} with details: ";
                        if (category.Detail.Celebrities != null)
                        {
                            if (category.Detail.Celebrities.Count > 0)
                            {
                                foreach (var celebrity in category.Detail.Celebrities)
                                {
                                    resultsOutput += $" celebrity: {celebrity.Name},";
                                }
                            }
                        }
                        if (category.Detail.Landmarks != null)
                        {
                            if (category.Detail.Landmarks.Count > 0)
                            {
                                foreach (var landmark in category.Detail.Landmarks)
                                {
                                    resultsOutput += $" landmark: {landmark.Name},";
                                }
                            }
                        }
                    }
                }
            }

            if (analysis.Tags != null)
            {
                resultsOutput += $"\nTAGS: {analysis.Tags.Count} tags detected: ";
                int tagNo = 1;
                foreach (var tag in analysis.Tags)
                {
                    if (tagNo < analysis.Tags.Count)
                    {
                        resultsOutput += $"{tag.Name}, ";
                    }
                    else
                    {
                        resultsOutput += tag.Name;
                    }
                    tagNo++;
                }
            }

            if (analysis.Color != null)
            {
                if (analysis.Color.IsBWImg)
                {
                    resultsOutput += "\nCOLOUR: This image is black & white";
                }
                else
                {
                    resultsOutput += $"\nCOLOUR: the dominant foreground colour is {analysis.Color.DominantColorForeground} with an accent of {analysis.Color.AccentColor} & " +
                    $"background of {analysis.Color.DominantColorBackground}.";
                }
            }

            Console.WriteLine(resultsOutput);
        }
    }
}