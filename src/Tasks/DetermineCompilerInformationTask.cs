using System;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Finite.Cpp.Sdk
{
    /// <summary>
    /// Determines compiler information given a set of search directories and
    /// names of compilers.
    /// </summary>
    public class DetermineCompilerInformationTask : Task
    {
        /// <summary>
        /// Gets or sets the directories to search in for compilers.
        /// </summary>
        [Required]
        public ITaskItem[] SearchDirectories { get; set; } = null!;

        /// <summary>
        /// Gets or sets the names of compiler binaries to search for.
        /// </summary>
        public ITaskItem[] CompilerNames { get; set; } = null!;

        /// <summary>
        /// Gets the detected compiler information.
        /// </summary>
        [Output]
        public ITaskItem[] DetectedCompilers { get; private set; } = null!;

        /// <inheritdoc/>
        public override bool Execute()
        {
            throw new NotImplementedException();
        }
    }
}
