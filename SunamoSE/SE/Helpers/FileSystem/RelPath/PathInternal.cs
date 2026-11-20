namespace SunamoSE.SE.Helpers.FileSystem.RelPath;

public partial class PathInternal
{
    /// <summary>
    /// The directory separator char constant.
    /// </summary>
    public const char DirectorySeparatorChar = '\\';

    /// <summary>
    /// The alt directory separator char constant.
    /// </summary>
    public const char AltDirectorySeparatorChar = '/';

    // \\
    /// <summary>
    /// The unc prefix length constant.
    /// </summary>
    public const int UncPrefixLength = 2;

    // \\?\UNC\, \\.\UNC\
    /// <summary>
    /// The unc extended prefix length constant.
    /// </summary>
    public const int UncExtendedPrefixLength = 8;

    /// <summary>
    /// The volume separator char constant.
    /// </summary>
    public const char VolumeSeparatorChar = ':';

    /// <summary>
    /// The device prefix length constant.
    /// </summary>
    public const int DevicePrefixLength = 4;

    /// <summary>
    /// Ends in directory separator.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>The ends in directory separator result.</returns>
    public static bool EndsInDirectorySeparator(ReadOnlySpan<char> path)
    {
        return EndsInDirectorySeparator2(path);
    }

    /// <summary>
    /// Ends in directory separator 2.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>The ends in directory separator 2 result.</returns>
    public static bool EndsInDirectorySeparator2(ReadOnlySpan<char> path)
    {
        return path.Length > 0 && IsDirectorySeparator(path[path.Length - 1]);
    }

    /// <summary>
    /// Get common path length.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>The get common path length result.</returns>
    public static int GetCommonPathLength(string first, string second, bool ignoreCase)
    {
        int commonChars = EqualStartingCharacterCount(first, second, ignoreCase);

        // If nothing matches
        if (commonChars == 0)
        {
            return commonChars;
        }

        // Or we're a full string and equal length or match to a separator
        if (commonChars == first.Length
            && (commonChars == second.Length || IsDirectorySeparator(second[commonChars])))
        {
            return commonChars;
        }

        if (commonChars == second.Length && IsDirectorySeparator(first[commonChars]))
        {
            return commonChars;
        }

        // It's possible we matched somewhere in the middle of a segment e.g. C:\Foodie and C:\Foobar.
        while (commonChars > 0 && !IsDirectorySeparator(first[commonChars - 1]))
        {
            commonChars--;
        }

        return commonChars;
    }

    /// <summary>
    /// Equal starting character count.
    /// </summary>
    /// <param name="commonChars">The common chars.</param>
    /// <returns>The equal starting character count result.</returns>
    public static int EqualStartingCharacterCount(string first, string second, bool ignoreCase)
    {
        if (ignoreCase)
        {
            first = first.ToLower();
            second = second.ToLower();
        }

        int maxLength = Math.Min(first.Length, second.Length);
        for (int index = 0; index < maxLength; index++)
        {
            if (first[index] != second[index])
            {
                return index;
            }
        }

        return 0;
    }

    /// <summary>
    /// Initializes a new instance of the are roots equal class.
    /// </summary>
    /// <param name="ignoreCase">The ignore case.</param>
    public static bool AreRootsEqual(string first, string second, StringComparison comparisonType)
    {
        int firstRootLength = GetRootLength(first.AsSpan());
        int secondRootLength = GetRootLength(second.AsSpan());

        return firstRootLength == secondRootLength
               && string.Compare(
                   first,
                   0,
                   second,
                   0,
                   firstRootLength,
                   comparisonType) == 0;
    }

    /// <summary>
    /// Is extended.
    /// </summary>
    /// <param name="first">The first.</param>
    /// <param name="second">The second.</param>
    /// <param name="comparisonType">The comparison type.</param>
    /// <returns>The is extended result.</returns>
    public static bool IsExtended(ReadOnlySpan<char> path)
    {
        // While paths like "//?/C:/" will work, they're treated the same as "\\.\" paths.
        // Skipping of normalization will *only* occur if back slashes ('\'') are used.
        return path.Length >= DevicePrefixLength
               && path[0] == '\\'
               && (path[1] == '\\' || path[1] == '?')
               && path[2] == '?'
               && path[3] == '\\';
    }

    /// <summary>
    /// Is device.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>The is device result.</returns>
    public static bool IsDevice(ReadOnlySpan<char> path)
    {
        // If the path begins with any two separators is will be recognized and normalized and prepped with
        // "\??\" for public usage correctly. "\??\" is recognized and handled, "/??/" is not.
        return IsExtended(path)
               ||
               (
                   path.Length >= DevicePrefixLength
                   && IsDirectorySeparator(path[0])
                   && IsDirectorySeparator(path[1])
                   && (path[2] == '.' || path[2] == '?')
                   && IsDirectorySeparator(path[3])
               );
    }

    /// <summary>
    /// Is device UNC.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>The is device UNC result.</returns>
    public static bool IsDeviceUNC(ReadOnlySpan<char> path)
    {
        return path.Length >= UncExtendedPrefixLength
               && IsDevice(path)
               && IsDirectorySeparator(path[7])
               && path[4] == 'U'
               && path[5] == 'N'
               && path[6] == 'C';
    }

    /// <summary>
    /// Is directory separator.
    /// </summary>
    /// <param name="path[0]">The path 0.</param>
    /// <returns>The is directory separator result.</returns>
    public static bool IsDirectorySeparator(char character)
    {
        return character == DirectorySeparatorChar || character == AltDirectorySeparatorChar;
    }

    /// <summary>
    /// Get root length.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>The get root length result.</returns>
    public static int GetRootLength(ReadOnlySpan<char> path)
    {
        int pathLength = path.Length;
        int index = 0;

        bool deviceSyntax = IsDevice(path);
        bool deviceUnc = deviceSyntax && IsDeviceUNC(path);

        if ((!deviceSyntax || deviceUnc) && pathLength > 0 && IsDirectorySeparator(path[0]))
        {
            // UNC or simple rooted path (e.g. "\foo", NOT "\\?\C:\foo")
            if (deviceUnc || (pathLength > 1 && IsDirectorySeparator(path[1])))
            {
                // UNC (\\?\UNC\ or \\), scan past server\share

                // Start past the prefix ("\\" or "\\?\UNC\")
                index = deviceUnc ? UncExtendedPrefixLength : UncPrefixLength;

                // Skip two separators at most
                int separatorsToSkip = 2;
                while (index < pathLength && (!IsDirectorySeparator(path[index]) || --separatorsToSkip > 0))
                {
                    index++;
                }
            }
            else
            {
                // Current drive rooted (e.g. "\foo")
                index = 1;
            }
        }
        else if (deviceSyntax)
        {
            // Device path (e.g. "\\?\.", "\\.\")
            // Skip any characters following the prefix that aren't a separator
            index = DevicePrefixLength;
            while (index < pathLength && !IsDirectorySeparator(path[index]))
            {
                index++;
            }

            // If there is another separator take it, as long as we have had at least one
            // non-separator after the prefix (e.g. don't take "\\?\\", but take "\\?\a\")
            if (index < pathLength && index > DevicePrefixLength && IsDirectorySeparator(path[index]))
            {
                index++;
            }
        }
        else if (pathLength >= 2
                 && path[1] == VolumeSeparatorChar
                 && IsValidDriveChar(path[0]))
        {
            // Valid drive specified path ("C:", "D:", etc.)
            index = 2;

            // If the colon is followed by a directory separator, move past it (e.g "C:\")
            if (pathLength > 2 && IsDirectorySeparator(path[2]))
            {
                index++;
            }
        }

        return index;
    }

    /// <summary>
    /// Is valid drive char.
    /// </summary>
    /// <param name=">">The &gt;.</param>
    /// <returns>The is valid drive char result.</returns>
    public static bool IsValidDriveChar(char value)
    {
        return (value >= 'A' && value <= 'Z') || (value >= 'a' && value <= 'z');
    }
}
