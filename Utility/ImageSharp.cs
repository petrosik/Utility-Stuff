namespace Petrosik
{
    namespace ImageSharpUtility
    {
        using SixLabors.ImageSharp;
        using SixLabors.ImageSharp.Drawing;
        using SixLabors.ImageSharp.Drawing.Processing;
        using SixLabors.ImageSharp.PixelFormats;
        using SixLabors.ImageSharp.Processing;
        using System;

        internal class ImageSharp
        {
            /// <summary>
            /// Tints the image to the color, weight cannot be more than 1 otherwise it inverses the colors
            /// </summary>
            /// <param name="img"></param>
            /// <param name="color"></param>
            /// <param name="weight">0 to 1, 0 original color only, 1 tint color only</param>
            /// <returns></returns>
            public static Image Tint(Image<Rgba32> img, Rgba32 color, float weight = 1f)
            {
                Image<Rgba32> filteredImage = img.Clone();

                for (int y = 0; y < filteredImage.Height; y++)
                {
                    for (int x = 0; x < filteredImage.Width; x++)
                    {
                        int[] pcolor = new int[] { filteredImage[x, y].R, filteredImage[x, y].G, filteredImage[x, y].B };
                        pcolor[0] = (int)(color.R * weight + pcolor[0] * Math.Abs(1f - weight));
                        pcolor[1] = (int)(color.G * weight + pcolor[1] * Math.Abs(1f - weight));
                        pcolor[2] = (int)(color.B * weight + pcolor[2] * Math.Abs(1f - weight));
                        filteredImage[x, y] = new Rgba32((byte)pcolor[0], (byte)pcolor[1], (byte)pcolor[2], filteredImage[x, y].A);
                    }
                }

                return filteredImage;
            }
        }
        public static class Extensions
        {
            /// <summary>
            /// Tints the image to the color, weight cannot be more than 1 otherwise it inverses the colors
            /// </summary>
            /// <param name="img"></param>
            /// <param name="color"></param>
            /// <param name="weight">0 to 1, 0 original color only, 1 tint color only</param>
            /// <returns></returns>
            public static Image Tint1(this Image img, Rgba32 color, float weight = 1f)
            {
                return ImageSharp.Tint((Image<Rgba32>)img, color, weight);
            }
            /// <summary>
            /// Tints the image to the color, weight cannot be more than 1 otherwise it inverses the colors
            /// </summary>
            /// <param name="context"></param>
            /// <param name="color"></param>
            /// <param name="weight">0 to 1, 0 original color only, 1 tint color only</param>
            /// <returns></returns>
            public static IImageProcessingContext Tint(this IImageProcessingContext context, Rgba32 color, float weight = 1f)
            {
                Size size = context.GetCurrentSize();
                var pcolor = color.ToScaledVector4();
                context.ProcessPixelRowsAsVector4(row =>
                {
                    for (int i = 0; i < size.Height; i++)
                    {
                        var pixel = row[i];
                        pixel = 
                        new(
                            pcolor[0] * weight + pixel[0] * Math.Abs(1f - weight),
                            pcolor[1] * weight + pixel[1] * Math.Abs(1f - weight),
                            pcolor[2] * weight + pixel[2] * Math.Abs(1f - weight),
                            pixel.W
                           );
                        row[i] = pixel;
                    }
                });

                return context;
            }
            /// <summary>
            /// Rounds off edges of the image
            /// </summary>
            /// <param name="context"></param>
            /// <returns></returns>
            public static IImageProcessingContext ConvertToAvatar(this IImageProcessingContext context)
            {
                Size size = context.GetCurrentSize();

                context.SetGraphicsOptions(new GraphicsOptions()
                {
                    Antialias = true,

                    // Enforces that any part of this shape that has color is punched out of the background
                    AlphaCompositionMode = PixelAlphaCompositionMode.DestOut
                });

                var rect = new RectangularPolygon(0, 0, size.Width, size.Height);
                var path = rect.Clip(new EllipsePolygon(size.Width / 2, size.Height / 2, size.Width / 2 - 1));

                // Mutating in here as we already have a cloned original
                // use any color (not Transparent), so the corners will be clipped
                context = context.Fill(Color.Red, path);
                return context;
            }
            /// <summary>
            /// Should cut out the shapes(paths) from the original image
            /// </summary>
            /// <param name="context"></param>
            /// <param name="paths"></param>
            /// <returns></returns>
            public static IImageProcessingContext Cutout(this IImageProcessingContext context, IPath[] paths)
            {
                context.SetGraphicsOptions(new GraphicsOptions()
                {
                    Antialias = true,

                    // Enforces that any part of this shape that has color is punched out of the background
                    //AlphaCompositionMode = PixelAlphaCompositionMode.DestOut
                    AlphaCompositionMode = PixelAlphaCompositionMode.DestIn
                });
                Size size = context.GetCurrentSize();
                var rect = new RectangularPolygon(0, 0, size.Width, size.Height);
                var path = rect.Clip(paths);

                // Mutating in here as we already have a cloned original
                // use any color (not Transparent), so the corners will be clipped
                context = context.Fill(Color.Red, path);
                return context;
            }
            /// <summary>
            /// Should keep the shapes(paths) from the original image
            /// </summary>
            /// <param name="context"></param>
            /// <param name="paths"></param>
            /// <returns></returns>
            public static IImageProcessingContext Select(this IImageProcessingContext context, IPath[] paths)
            {
                context.SetGraphicsOptions(new GraphicsOptions()
                {
                    Antialias = true,

                    // Enforces that any part of this shape that has color is punched out of the background
                    AlphaCompositionMode = PixelAlphaCompositionMode.DestOut
                });
                Size size = context.GetCurrentSize();
                var rect = new RectangularPolygon(0, 0, size.Width, size.Height);
                var path = rect.Clip(paths);

                // Mutating in here as we already have a cloned original
                // use any color (not Transparent), so the corners will be clipped
                context = context.Fill(Color.Red, path);
                return context;
            }
        }
    }
}
