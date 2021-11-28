using System;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualStudio.Shell;

namespace Finite.Cpp.Sdk.VisualStudio
{
    [PackageRegistration(AllowsBackgroundLoading = true, UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("Finite.Cpp.Sdk", "Cross-platform C/C++ SDK", productId: "memes")]
    [Guid(PackageGuid)]
    public sealed class CppSdkPackage : AsyncPackage
    {
        public const string PackageGuid = "780D1924-1B9B-4C3C-82AF-704A61F855D4";

        protected override async System.Threading.Tasks.Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync();
        }
    }
}
