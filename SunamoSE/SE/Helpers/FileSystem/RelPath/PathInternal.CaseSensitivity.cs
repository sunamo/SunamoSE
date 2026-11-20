namespace SunamoSE.SE.Helpers.FileSystem.RelPath;

/// <summary>
/// Represents the path internal type.
/// </summary>
public partial class PathInternal
{
    /// <summary>
    /// Gets the string comparison.
    /// </summary>
    public static StringComparison StringComparison =>
        IsCaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

    /// <summary>
    /// Gets the is case sensitive.
    /// </summary>
    public static bool IsCaseSensitive => true;
}