namespace SunamoSE.SE;

/// <summary>
/// Represents the thread helper type.
/// </summary>
public class ThreadHelper
{
    /// <summary>
    /// Need dispatcher.
    /// </summary>
    /// <param name="typeName">The type name.</param>
    /// <returns>The need dispatcher result.</returns>
    public static bool NeedDispatcher(string typeName)
    {
        return typeName == "UIElementCollection";
    }

    /// <summary>
    /// Sleep.
    /// </summary>
    /// <param name="milliseconds">The milliseconds.</param>
    public static
    async Task
 Sleep(int milliseconds)
    {
        await Task.Delay(milliseconds);
    }
}
