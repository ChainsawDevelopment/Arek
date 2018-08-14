#r "tools/FAKE.Core/tools/FakeLib.dll"

open Fake

let source = __SOURCE_DIRECTORY__

let runMsbuild (project: string) (target: string) =
    let setParams (p: MSBuildParams) = 
        { p with
            Verbosity = Some(MSBuildVerbosity.Minimal)
            Targets = [ target ]
            Properties = 
            [
                ( "Configuration", "Debug" )
            ]
        }

    MSBuildHelper.build setParams project

Target "NuGetRestore" (fun _ ->
    let setParams (p: RestorePackageParams) = p
         
    RestoreMSSolutionPackages setParams (source @@ "GitLabNotifier.sln")
)

Target "Build" (fun _ ->
    runMsbuild (source @@ "GitLabNotifier.sln") "Build"
)

"NuGetRestore"
==> "Build"

RunTargetOrDefault "Build"