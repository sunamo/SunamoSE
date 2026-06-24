namespace SunamoSE.SE;

public class ThreadHelper
{
    public static bool NeedDispatcher(string typeName)
    {
        return typeName == "UIElementCollection";
    }

    public static
    async Task
 Sleep(int milliseconds)
    {
        await Task.Delay(milliseconds);
    }
}
