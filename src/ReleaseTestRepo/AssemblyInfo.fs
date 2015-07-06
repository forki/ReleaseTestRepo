namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("ReleaseTestRepo")>]
[<assembly: AssemblyProductAttribute("ReleaseTestRepo")>]
[<assembly: AssemblyDescriptionAttribute("Just a repo to test releasing")>]
[<assembly: AssemblyVersionAttribute("1.0")>]
[<assembly: AssemblyFileVersionAttribute("1.0")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "1.0"
