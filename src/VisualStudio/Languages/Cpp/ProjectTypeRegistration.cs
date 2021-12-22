using Finite.Cpp.Sdk.VisualStudio;
using Microsoft.VisualStudio.Packaging;
using Microsoft.VisualStudio.ProjectSystem.VS;
using Microsoft.VisualStudio.Shell;

[assembly: ProjectTypeRegistration(
    projectTypeGuid: ProjectType.CPlusPlus,
    displayName: "C++",
    displayProjectFileExtensions: "C++ Project Files (*.cxxproj);*.cxxproj",
    defaultProjectExtension: "cxxproj",
    language: "CPlusPlus",
    resourcePackageGuid: CppSdkPackage.PackageGuid,
    Capabilities = Capabilities.ProjectTypes.CPlusPlus,
    PossibleProjectExtensions = "cxxproj",
    NewProjectRequireNewFolderVsTemplate = true)]
[assembly: ProvideEditorFactoryMapping("{f6819a78-a205-47b5-be1c-675b3c7f0b8e}", ".cxxproj")]
