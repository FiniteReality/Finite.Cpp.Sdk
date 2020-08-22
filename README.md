# Finite.Cpp.Sdk #

An MSBuild SDK allowing for (semi) cross-platform compilation of C/C++
projects.

## License ##

Copyright (c) 2018 FiniteReality under the MIT license. See the LICENSE file in
the root directory of the project for more information.

## TODO ##
- Finish writing targets/props files
- Add test projects
  - Building a C/C++ project
    - Executables and binaries
  - Wrappers for standard C/C++ API
    - Trig functions have analogues in .NET, easy to test
  - More complex wrappers (e.g. libcurl, OpenSSL, FreeType)?

## Considerations ##
- MSBuild targets consideration:
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
