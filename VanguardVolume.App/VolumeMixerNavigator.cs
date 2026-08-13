using System.Diagnostics;
using System.Windows.Automation;

namespace VanguardVolume.App;

internal static class VolumeMixerNavigator
{
    public static void ScrollToMixer()
    {
        try
        {
            var mixer = AutomationElement.RootElement
                .FindAll(TreeScope.Descendants, Condition.TrueCondition)
                .Cast<AutomationElement>()
                .FirstOrDefault(element => string.Equals(
                    element.Current.Name,
                    "Volume mixer",
                    StringComparison.OrdinalIgnoreCase));

            if (mixer?.TryGetCurrentPattern(ScrollItemPattern.Pattern, out var pattern) == true)
            {
                ((ScrollItemPattern)pattern).ScrollIntoView();
            }
        }
        catch (ElementNotAvailableException)
        {
            Trace.TraceWarning("The Windows volume mixer closed before it could be selected.");
        }
    }
}
