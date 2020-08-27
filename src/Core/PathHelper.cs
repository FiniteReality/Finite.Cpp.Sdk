using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Finite.Cpp.Build.Tasks
{
    /// <summary>
    /// Helpers for reading the PATH environment variable.
    /// </summary>
    internal static class PathHelper
    {
        /// <summary>
        /// Parses the current PATH entries and returns them.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> containing each path entry.
        /// </returns>
        public static IEnumerable<string> GetCurrentPathEntries()
        {
            var environment = Environment.GetEnvironmentVariable("PATH");

            if (string.IsNullOrEmpty(environment))
                return Array.Empty<string>();

            // TODO: what separator does OSX use?
#if NETFRAMEWORK
            return environment.Split(new char[]{ ';' },
                StringSplitOptions.RemoveEmptyEntries);
#else
            return environment.Split(':',
                StringSplitOptions.RemoveEmptyEntries);
#endif
        }

        /// <summary>
        /// Returns the well-known paths used to search for Linux binaries, as
        /// well as any locations specified by the PATH environment variable.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> containing each path entry to
        /// search.
        /// </returns>
        public static IEnumerable<string> GetLinuxSearchPaths()
        {
            Debug.Assert(RuntimeInformation.IsOSPlatform(OSPlatform.Linux));

            yield return "/usr/local/bin";
            yield return "/usr/bin";
            yield return "/bin";
            yield return "/usr/local/sbin";
            yield return "/usr/sbin";
            yield return "/sbin";

            foreach (var path in GetCurrentPathEntries())
                yield return path;
        }
    }
}
