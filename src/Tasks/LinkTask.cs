using System;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Finite.Cpp.Sdk
{
    /// <summary>
    /// Links one or more C/C++ object files into an application or library.
    /// </summary>
    public class LinkTask : Task
    {
        /// <summary>
        /// Gets or sets the libraries to link against while linking the input
        /// object files.
        /// </summary>
        public ITaskItem[] LinkLibraries { get; set; } = null!;

        /// <summary>
        /// Gets or sets the input files to link.
        /// </summary>
        [Required]
        public ITaskItem[] InputFiles { get; set; } = null!;

        /// <summary>
        /// Gets or sets the output file name.
        /// </summary>
        [Required]
        public string OutputFileName { get; set; } = null!;

        /// <summary>
        /// Gets the output file after compilation.
        /// </summary>
        [Output]
        public ITaskItem OutputFile { get; private set; } = null!;

        /// <inheritdoc/>
        public override bool Execute()
        {
            throw new NotImplementedException();
        }
    }
}
