using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finite.Cpp.Sdk.VisualStudio.Configuration
{
    internal sealed partial class ProjectConfigurationDimensionProvider
    {
        private record Dimension(string Name, string MSBuildProperty, string[] Value);

        private static readonly Dimension ConfigurationDimension
            = new("Configuration", "Configurations", new[] { "Debug" });
        private static readonly Dimension PlatformDimension
            = new("Platform", "Platforms", new[] { "Native" });
    }
}
