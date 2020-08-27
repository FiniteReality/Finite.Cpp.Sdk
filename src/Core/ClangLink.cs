using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Finite.Cpp.Build.Tasks
{
    /// <summary>
    /// Links one or more C/C++ object files into an application or library.
    /// </summary>
    public class ClangLink : ToolTask
    {
        /// <summary>
        /// Gets or sets whether debugging symbols are enabled.
        /// </summary>
        [Required]
        public bool EnableDebugSymbols { get; set; }

        /// <summary>
        /// Gets or sets the library type if <see cref="OutputType"/> is a
        /// library.
        /// </summary>
        public string LibraryType { get; set; } = null!;

        /// <summary>
        /// Gets or sets the libraries to link to.
        /// </summary>
        public ITaskItem[] LinkLibraries { get; set; } = null!;

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
        /// Gets or sets the output file directory after linking, if
        /// <see cref="OutputFile"/> is <c>null</c>.
        /// </summary>
        public string OutputDirectory { get; set; } = null!;

        /// <summary>
        /// Gets or sets the output file after linking.
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
        /// Gets or sets the input file to link.
        /// </summary>
        [Required]
        public ITaskItem[] SourceFiles { get; set; } = null!;

        /// <inheritdoc />
        protected override string ToolName => "clang";

        /// <inheritdoc />
        protected override string GenerateCommandLineCommands()
        {
            if (OutputFile == null)
            {
                if (OutputDirectory == null)
                {
                    Log.LogError(
                        "Either OutputFile or OutputDirectory needs to be " +
                        "set");
                    return null!;
                }

                OutputFile = new TaskItem(
                    Path.ChangeExtension(
                        Path.Combine(
                            OutputDirectory,
                            Path.GetFileName(SourceFiles[0].ItemSpec)),
                        OutputFileExtension));

                Log.LogMessage($"OutputFile = {OutputFile}");
            }

            var builder = new CommandLineBuilder();

            switch (OutputType)
            {
                case "library" when LibraryType == "shared":
                    builder.AppendSwitch("-shared");
                    builder.AppendSwitch("-fPIC");
                    break;
                case "library" when LibraryType == "static":
                    builder.AppendSwitch("-static");
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

            builder.AppendFileNamesIfNotNull(SourceFiles, " ");

            if (LinkLibraries != null)
            {
                foreach (var directory in LinkLibraries)
                {
                    if (directory == null)
                        continue;

                    var fullPath = directory.ItemSpec;

                    builder.AppendSwitchIfNotNull("-L",
                        Path.GetDirectoryName(fullPath));
                    builder.AppendSwitchIfNotNull("-l:",
                        Path.GetFileName(fullPath));
                }
            }

            if (EnableDebugSymbols)
                builder.AppendSwitch("--debug");

            if (Optimize && OptimizeLevel > 0)
                builder.AppendSwitchIfNotNull(
                    $"--optimize=", OptimizeLevel.ToString());

            builder.AppendSwitchIfNotNull("--output=", OutputFile);
            builder.AppendSwitchIfNotNull("-rpath ", "$ORIGIN");

            return builder.ToString();
        }

        /// <inheritdoc/>
        protected override string GenerateFullPathToTool()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                foreach (var location in PathHelper.GetLinuxSearchPaths())
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
                $"{nameof(ClangLink)} is not implemented for " +
                $"{RuntimeInformation.OSDescription}.");
        }
    }
}