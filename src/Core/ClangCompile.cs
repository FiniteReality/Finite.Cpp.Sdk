using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Finite.Cpp.Sdk
{
    /// <summary>
    /// Compiles an input C or C++ source file into an object file suitable for
    /// linking.
    /// </summary>
    public class ClangCompile : ToolTask
    {
        /// <summary>
        /// Gets or sets whether debugging symbols are enabled.
        /// </summary>
        [Required]
        public bool EnableDebugSymbols { get; set; }

        /// <summary>
        /// Gets or sets the include directories to use.
        /// </summary>
        public ITaskItem[] IncludeDirectories { get; set; } = null!;

        /// <summary>
        /// Gets or sets the library type if <see cref="OutputType"/> is a
        /// library.
        /// </summary>
        public string LibraryType { get; set; } = null!;

        /// <summary>
        /// Gets or sets whether optimizations are enabled.
        /// </summary>
        /// <value></value>
        [Required]
        public bool Optimize { get; set; }

        /// <summary>
        /// Gets or sets the optimization level, where 0 is disabled. If
        /// <see cref="Optimize"/> is <c>false</c>, the value of this option is
        /// ignored.
        /// </summary>
        [Required]
        public int OptimizeLevel { get; set; }

        /// <summary>
        /// Gets or sets the output file directory after compilation, if
        /// <see cref="OutputFile"/> is <c>null</c>.
        /// </summary>
        public string OutputDirectory { get; set; } = null!;

        /// <summary>
        /// Gets or sets the output file after compilation.
        /// </summary>
        [Output]
        public ITaskItem OutputFile { get; set; } = null!;

        /// <summary>
        /// Gets or sets the output file extension, if <see cref="OutputFile"/>
        /// is <c>null</c>.
        /// </summary>
        public string OutputFileExtension { get; set; } = null!;

        /// <summary>
        /// Gets or sets the output file type.
        /// </summary>
        [Required]
        public string OutputType { get; set; } = null!;

        /// <summary>
        /// Gets or sets the input file to compile.
        /// </summary>
        [Required]
        public ITaskItem SourceFile { get; set; } = null!;


        /// <inheritdoc />
        protected override string ToolName => "clang";

        /// <inheritdoc />
        protected override string GenerateCommandLineCommands()
        {
            if (OutputFile == null && OutputFileExtension != null)
            {
                OutputFile = new TaskItem(
                    Path.ChangeExtension(
                        Path.Combine(
                            OutputDirectory,
                            Path.GetFileName(SourceFile.ItemSpec)),
                        OutputFileExtension));

                Log.LogMessage($"OutputFile = {OutputFile}");
            }

            var builder = new CommandLineBuilder();

            builder.AppendSwitchIfNotNull("--include-directory",
                IncludeDirectories, " ");

            switch (OutputType)
            {
                case "library" when LibraryType == "shared":
                    builder.AppendSwitch("-fPIC");
                    break;
                case "library" when LibraryType == "static":
                    //builder.AppendSwitch("-static");
                    break;

                case "library":
                    Log.LogError($"Unknown library type {LibraryType}");
                    return null!;

                case "exe":
                    break;

                default:
                    Log.LogError($"Unknown output type {OutputType}");
                    return null!;
            }

            if (EnableDebugSymbols)
                builder.AppendSwitch("--debug");

            if (Optimize && OptimizeLevel > 0)
                builder.AppendSwitchIfNotNull(
                    $"--optimize=", OptimizeLevel.ToString());

            builder.AppendSwitch("--compile");
            builder.AppendFileNameIfNotNull(SourceFile);
            builder.AppendSwitchIfNotNull("--output=", OutputFile);

            return builder.ToString();
        }

        /// <inheritdoc/>
        protected override string GenerateFullPathToTool()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                foreach (var location in GetLinuxSearchPaths())
                {
                    var fullPath = Path.Combine(location, ToolName);

                    Log.LogMessage(
                        $"Searching {fullPath} for {ToolName} executable");

                    // N.B. this doesn't check the file is executable...
                    if (File.Exists(fullPath))
                        return fullPath;
                }

                Log.LogError($"Could not find {ToolName} executable");
                return null!;
            }


            throw new NotImplementedException(
                $"{nameof(ClangCompile)} is not implemented for " +
                $"{RuntimeInformation.OSDescription}.");
        }

        private static IEnumerable<string> GetLinuxSearchPaths()
        {
            yield return "/usr/local/bin";
            yield return "/usr/bin";
            yield return "/bin";
            yield return "/usr/local/sbin";
            yield return "/usr/sbin";
            yield return "/sbin";

            var additionalPaths = Environment.GetEnvironmentVariable("PATH")
                ?.Split(':', StringSplitOptions.RemoveEmptyEntries)
                ?? Array.Empty<string>();

            foreach (var path in additionalPaths)
                yield return path;
        }
    }
}
