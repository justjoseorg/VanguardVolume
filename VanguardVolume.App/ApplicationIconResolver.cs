using System.Drawing;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Interop;
using MediaBrushes = System.Windows.Media.Brushes;
using MediaPen = System.Windows.Media.Pen;

namespace VanguardVolume.App;

internal static class ApplicationIconResolver
{
    private static readonly Dictionary<string, ImageSource> Cache = new(StringComparer.OrdinalIgnoreCase);

    public static ImageSource GetIcon(MixerTarget target)
    {
        if (Cache.TryGetValue(target.Id, out var icon))
        {
            return icon;
        }

        if (target.IsMaster)
        {
            icon = CreateMasterVolumeIcon();
            Cache[target.Id] = icon;
            return icon;
        }

        if (target.Id.StartsWith("process:", StringComparison.OrdinalIgnoreCase))
        {
            var executablePath = target.Id["process:".Length..];
            if (File.Exists(executablePath))
            {
                using var extractedIcon = Icon.ExtractAssociatedIcon(executablePath);
                if (extractedIcon is not null)
                {
                    icon = CreateImageSource(extractedIcon);
                    Cache[target.Id] = icon;
                    return icon;
                }
            }
        }

        icon = CreateImageSource(SystemIcons.Application);
        Cache[target.Id] = icon;
        return icon;
    }

    private static ImageSource CreateImageSource(Icon icon)
    {
        var image = Imaging.CreateBitmapSourceFromHIcon(
            icon.Handle,
            System.Windows.Int32Rect.Empty,
            BitmapSizeOptions.FromWidthAndHeight(32, 32));
        image.Freeze();
        return image;
    }

    private static ImageSource CreateMasterVolumeIcon()
    {
        var drawing = new DrawingGroup();
        drawing.Children.Add(new GeometryDrawing(
            MediaBrushes.White,
            null,
            Geometry.Parse("M2,12 L8,12 L15,5 L15,27 L8,20 L2,20 Z")));
        drawing.Children.Add(new GeometryDrawing(
            null,
            new MediaPen(MediaBrushes.DeepSkyBlue, 2.5),
            Geometry.Parse("M19,10 C24,14 24,18 19,22")));
        drawing.Children.Add(new GeometryDrawing(
            null,
            new MediaPen(MediaBrushes.DeepSkyBlue, 2.5),
            Geometry.Parse("M23,6 C31,13 31,19 23,26")));

        var image = new DrawingImage(drawing);
        image.Freeze();
        return image;
    }
}
