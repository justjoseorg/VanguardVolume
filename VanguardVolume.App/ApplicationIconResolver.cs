using System.Drawing;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Interop;

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

        if (!target.IsMaster && target.Id.StartsWith("process:", StringComparison.OrdinalIgnoreCase))
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

        icon = CreateImageSource(target.IsMaster ? SystemIcons.Information : SystemIcons.Application);
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
}
