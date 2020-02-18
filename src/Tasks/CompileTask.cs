using System;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Finite.Cpp.Sdk
{
    /// <summary>
    /// Compiles an input C or C++ source file into an object file suitable for
    /// linking.
    /// </summary>
    public class CompileTask : Task
    {
        /// <summary>
        /// Gets or sets the compiler to use, along with any required
        /// parameters necessary to support the input file.
        /// </summary>
        [Required]
        public ITaskItem Compiler { get; set; } = null!;

        /// <summary>
        /// Gets or sets the include directories to use while compiling the
        /// input file.
        /// </summary>
        [Required]
        public ITaskItem[] IncludeDirectories { get; set; } = null!;

        /// <summary>
        /// Gets or sets the input file to compile.
        /// </summary>
        [Required]
        public ITaskItem InputFile { get; set; } = null!;

        /// <summary>
        /// Gets or sets the output file name to use.
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
