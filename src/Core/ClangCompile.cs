using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Finite.Cpp.Build.Tasks
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
        /// Gets or sets the language to compile <see cref="SourceFile"/> as.
        /// </summary>
        [Required]
        public string Language { get; set; } = null!;

        /// <summary>
        /// Gets or sets the language version to compile
        /// <see cref="SourceFile"/> as.
        /// </summary>
        [Required]
        public string LanguageVersion { get; set; } = null!;

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
        /// Gets or sets the output file after compilation.
        /// </summary>
        [Required, Output]
        public ITaskItem OutputFile { get; set; } = null!;

        /// <summary>
        /// Gets or sets the files written when this task is executed.
        /// </summary>
        [Output]
        public ITaskItem[] FileWrites { get; set; } = null!;


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

        /// <inheritdoc/>
        protected override int ExecuteTool(string pathToTool,
            string responseFileCommands, string commandLineCommands)
        {
            Log.LogMessage($"OutputFile = {OutputFile}");

            var directory = Path.GetDirectoryName(OutputFile.ItemSpec);
            if (!string.IsNullOrEmpty(directory))
                _ = Directory.CreateDirectory(directory);

            var result = base.ExecuteTool(pathToTool, responseFileCommands, commandLineCommands);

            FileWrites = new[] { OutputFile };

            return result;
        }

        /// <inheritdoc />
        protected override string GenerateCommandLineCommands()
        {
            var builder = new CommandLineBuilder();

            if (IncludeDirectories != null)
            {
                foreach (var includeDirectory in IncludeDirectories)
                {
                    builder.AppendSwitchIfNotNull("--include-directory=",
                        includeDirectory);
                }
            }

            switch (OutputType)
            {
                case "library" when LibraryType == "shared":
                    if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
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
                    "--optimize=", OptimizeLevel.ToString());

            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                builder.AppendSwitch("-fvisibility=hidden");

            builder.AppendSwitch("--compile");

            // the languages we care about all have lowercase names:
            // https://github.com/llvm/llvm-project/blob/main/clang/include/clang/Driver/Types.def
            builder.AppendSwitchIfNotNull("--language=",
                Language.ToLowerInvariant());

            builder.AppendSwitchIfNotNull("--std=",
                LanguageVersion.ToLowerInvariant());

            builder.AppendFileNameIfNotNull(SourceFile);
            builder.AppendSwitchIfNotNull("--output=", OutputFile);

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
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                foreach (var install in ToolLocationHelper.GetFoldersInVSInstalls())
                {
                    var fullPath = Path.Combine(install, "VC", "Tools", "Llvm", "bin", $"{ToolName}.exe");

                    Log.LogMessage(
                        $"Searching {fullPath} for {ToolName} executable");

                    if (File.Exists(fullPath))
                        return fullPath;
                }

                foreach (var location in PathHelper.GetCurrentPathEntries())
                {
                    var fullPath = Path.Combine(location, $"{ToolName}.exe");

                    Log.LogMessage(
                        $"Searching {fullPath} for {ToolName} executable");

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
    }
}
