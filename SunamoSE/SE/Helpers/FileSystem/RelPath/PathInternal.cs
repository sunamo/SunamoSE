namespace SunamoSE.SE.Helpers.FileSystem.RelPath;

public partial class PathInternal
{
    public const char DirectorySeparatorChar = '\\';

    public const char AltDirectorySeparatorChar = '/';

    // \\
    public const int UncPrefixLength = 2;

    // \\?\UNC\, \\.\UNC\
    public const int UncExtendedPrefixLength = 8;

    public const char VolumeSeparatorChar = ':';

    public const int DevicePrefixLength = 4;

    public static bool EndsInDirectorySeparator(ReadOnlySpan<char> path)
    {
        return EndsInDirectorySeparator2(path);
    }

    public static bool EndsInDirectorySeparator2(ReadOnlySpan<char> path)
    {
        return path.Length > 0 && IsDirectorySeparator(path[path.Length - 1]);
    }

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

    public static bool IsDeviceUNC(ReadOnlySpan<char> path)
    {
        return path.Length >= UncExtendedPrefixLength
               && IsDevice(path)
               && IsDirectorySeparator(path[7])
               && path[4] == 'U'
               && path[5] == 'N'
               && path[6] == 'C';
    }

    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsDirectorySeparator(char character)
    {
        return character == DirectorySeparatorChar || character == AltDirectorySeparatorChar;
    }

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

    public static bool IsValidDriveChar(char value)
    {
        return (value >= 'A' && value <= 'Z') || (value >= 'a' && value <= 'z');
    }
}
