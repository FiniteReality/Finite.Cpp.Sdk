# Finite.Cpp.Sdk #

An MSBuild SDK allowing for (semi) cross-platform compilation of C/C++
projects.

## License ##

Copyright (c) 2018 FiniteReality under the MIT license. See the LICENSE file in
the root directory of the project for more information.

## TODO ##
- Detect standards support in the various compilers
- Implement compilation in the various compilers
- Implement detection of more compiler types:
  - MSVC toolchain
  - Xcode toolchain
  - Other Windows/OSX compilers
  - Intel C/C++ compiler?
- Add compiler options in CompilerOptions.cs
- Add linker options in LinkerOptions.cs
- Finish writing targets/props files
- Add test projects
  - Building a C/C++ project
    - Executables and binaries
  - Wrappers for standard C/C++ API
    - Trig functions have analogues in .NET, easy to test
  - More complex wrappers (e.g. libcurl, OpenSSL, FreeType)?

## Considerations ##
- Split Compiler.cs into different types of compiler? e.g.:
  - CCompiler
  - CPlusPlusCompiler
  - AssemblyCompiler - technically not a compiler but might be easier to
    consider as one?
- Make linkers a separate type?
  - Some might have it separated? Need to find examples
  - How will they be detected?
- Support LLVM IR compilation for "platform independent" packages?
  - Will need LLVM set up to compile for target platform
    - Might make set-up costly
  - Would prevent platform-specific operations
    - e.g. using Windows IOCP vs epoll for I/O
    - Would make consumption of packages easier
  - How will `dotnet publish` work?
- MSBuild targets consideration:
  - Target extensions are somewhat hardcoded in MSBuild
    - Hard to overwrite correctly
    - Impossible to remove (condition is `'$(TargetExt)' != ''`)
      - Harder to make OSX/Linux style executables this way
  - NuGet support needs some extra thinking:
    - Single package containing multiple architectures?
    - Split-packages like .NET Core for large distributions of packages?
  - How to reference a C/C++ project from a C# project?
    - Csc will try to reference native files/executales and complain
    - Current workaround: reference as content and copy to output dir
  - How to reference a C/C++ project from another?
    - This should be easier, using the same approach as C# does for C# projects
  - How to reference a C# project from a C/C++ project?
    - Probably not necessary to start off with
  - Project file extension?
    - `vcxproj` is supported by the MSBuild toolchain/.NET Core SDK/NuGet
      tasks, hardcoded in places
      - This makes it harder to use another extension
    - IDEs have existing support for `vcxproj`
      - Visual Studio could break if we re-used the extension
      - Other IDEs?
    - `cxxproj` could be a viable alternative
      - Might need to add some work-arounds to .NET Core to support it?
      - Different file type does not break IDE support
        - Can simplify existing `vcxproj` structure without issues