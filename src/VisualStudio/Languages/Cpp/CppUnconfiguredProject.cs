using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.ProjectSystem;


namespace Finite.Cpp.Sdk.VisualStudio.Languages.Cpp
{
    [Export]
    [AppliesTo(Capabilities.ProjectTypes.FiniteCppSdk)]
    internal sealed class CppUnconfiguredProject
    {
    }
}
