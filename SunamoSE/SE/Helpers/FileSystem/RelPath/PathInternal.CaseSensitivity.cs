namespace SunamoSE.SE.Helpers.FileSystem.RelPath;

public partial class PathInternal
{
    public static StringComparison StringComparison =>
        IsCaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

    public static bool IsCaseSensitive => true;
}