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
        /// Gets or sets the additional output files after compilation.
        /// </summary>
        [Output]
        public ITaskItem[] OutputFiles { get; set; } = null!;

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

        /// <inheritdoc/>
        protected override int ExecuteTool(string pathToTool, string responseFileCommands, string commandLineCommands)
        {
            var result = base.ExecuteTool(pathToTool, responseFileCommands, commandLineCommands);

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                OutputFiles = new[]
                {
                    new TaskItem(Path.ChangeExtension(OutputFile.ItemSpec, "lib"),
                        new Dictionary<string, string>
                        {
                            ["CopyToOutput"] = "true"
                        }),
                    new TaskItem(Path.ChangeExtension(OutputFile.ItemSpec, "pdb"),
                        new Dictionary<string, string>
                        {
                            ["CopyToOutput"] = "true"
                        }),
                    new TaskItem(Path.ChangeExtension(OutputFile.ItemSpec, "ilk")),
                    new TaskItem(Path.ChangeExtension(OutputFile.ItemSpec, "exp"))
                };
            }

            return result;
        }

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
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        builder.AppendSwitch("-mdll");
                    else
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

            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                builder.AppendSwitch("-fvisibility=hidden");

            builder.AppendFileNamesIfNotNull(SourceFiles, " ");

            if (LinkLibraries != null)
            {
                foreach (var linkLibrary in LinkLibraries)
                {
                    if (linkLibrary == null)
                        continue;

                    builder.AppendSwitchIfNotNull("-L",
                        Path.GetDirectoryName(linkLibrary.ItemSpec));

                    /*
                      N.B. Since we're passed the output, we need to rename to
                      .lib on windows.

                      Additionally, the presence/lack of a colon in `-l` is
                      important - this parameter is effectively passed directly
                      to the system linker which has different behaviour on
                      windows versus linux: the colon tells linux that we're
                      looking for a specific filename, while Windows expects
                      that we're passing a filename anyway, so a colon would
                      cause it to look for that in the filename.
                    */
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        builder.AppendSwitchIfNotNull("-l",
                            Path.GetFileName(
                                Path.ChangeExtension(
                                    linkLibrary.ItemSpec, "lib")));
                    else
                        builder.AppendSwitchIfNotNull("-l:",
                            Path.GetFileName(linkLibrary.ItemSpec));
                }
            }

            if (EnableDebugSymbols)
                builder.AppendSwitch("--debug");

            if (Optimize && OptimizeLevel > 0)
                builder.AppendSwitchIfNotNull(
                    $"--optimize=", OptimizeLevel.ToString());

            builder.AppendSwitchIfNotNull("--output=", OutputFile);

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
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
            }


            throw new NotImplementedException(
                $"{nameof(ClangLink)} is not implemented for " +
                $"{RuntimeInformation.OSDescription}.");
        }
    }
}
