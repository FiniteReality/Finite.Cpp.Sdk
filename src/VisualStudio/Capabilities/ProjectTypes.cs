using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ProjectSystem;

namespace Finite.Cpp.Sdk.VisualStudio
{
    internal static class Capabilities
    {
        internal static class ProjectType
        {
            public const string FiniteCppSdk = nameof(FiniteCppSdk);
            public const string CPlusPlus = nameof(CPlusPlus);
        }

        internal static class ProjectTypes
        {
            public const string FiniteCppSdk =
                $"{ProjectCapabilities.HandlesOwnReload}; " +
                $"{ProjectCapabilities.OpenProjectFile}; " +
                $"{ProjectCapabilities.PreserveFormatting}; " +
                $"{ProjectCapabilities.ProjectConfigurationsDeclaredDimensions}; " +
                $"{ProjectType.FiniteCppSdk}";

            public const string CPlusPlus =
                $"{FiniteCppSdk}; " +
                $"{ProjectType.CPlusPlus}";
        }
    }
}
