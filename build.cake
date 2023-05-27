#addin nuget:?package=Cake.FileHelpers&version=6.1.3

const string appId = "org.nickvision.money";
const string projectName = "NickvisionMoney";
const string shortName = "denaro";

var sep = System.IO.Path.DirectorySeparatorChar;

var target = Argument("target", "Run");
var configuration = Argument("configuration", "Debug");
var ui = Argument("ui", EnvironmentVariable("NICK_UI", ""));
var projectSuffix = "";

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .WithCriteria(c => HasArgument("rebuild"))
    .Does(() =>
{
    CleanDirectory($".{sep}{projectName}.{projectSuffix}{sep}bin{sep}{configuration}");
});

Task("Build")
    .IsDependentOn("Clean")
    .Does(() =>
{
    DotNetBuild($".{sep}{projectName}.{projectSuffix}{sep}{projectName}.{projectSuffix}.csproj", new DotNetBuildSettings
    {
        Configuration = configuration
    });
});

Task("Run")
    .IsDependentOn("Build")
    .Does(() =>
{
    DotNetRun($".{sep}{projectName}.{projectSuffix}{sep}{projectName}.{projectSuffix}.csproj", new DotNetRunSettings
    {
        Configuration = configuration,
        NoBuild = true
    });
});

Task("Publish")
    .Does(() =>
{
    var selfContained = Argument("self-contained", false) || HasArgument("self-contained") || HasArgument("sc");
    var runtime = Argument("runtime", "");
    if (string.IsNullOrEmpty(runtime))
    {
        runtime = "linux-";
        runtime += System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture.ToString().ToLower();
    }
    var outDir = EnvironmentVariable("NICK_BUILDDIR", "_nickbuild");
    CleanDirectory(outDir);
    var prefix = Argument("prefix", "/usr");
    var libDir = string.IsNullOrEmpty(prefix) ? "lib" : $"{prefix}{sep}lib";
    var publishDir = $"{outDir}{libDir}{sep}{appId}";
    var exitCode = 0;
    Information($"Publishing {projectName}.{projectSuffix} ({runtime})...");
    DotNetPublish($".{sep}{projectName}.{projectSuffix}{sep}{projectName}.{projectSuffix}.csproj", new DotNetPublishSettings
    {
        Configuration = "Release",
        SelfContained = selfContained,
        OutputDirectory = publishDir,
        Sources = Argument("sources", "").Split(" "),
        Runtime = runtime,
        HandleExitCode = code => {
            exitCode = code;
            return false;
        }
    });
    if (exitCode != 0)
    {
        throw new Exception($"Publishing failed with exit code {exitCode}.");
    }

    FinishPublishLinux(outDir, prefix, libDir, selfContained);

    if (projectSuffix == "GNOME")
    {
        PostPublishGNOME(outDir, prefix, libDir);
    }
});

Task("Install")
    .Does(() =>
{
    var buildDir = EnvironmentVariable("NICK_BUILDDIR", "_nickbuild");
    var destDir = Argument("destdir", "/");
    CopyDirectory(buildDir, destDir);
});

Task("FlatpakSourcesGen")
    .Does(() =>
{
    StartProcess("flatpak-dotnet-generator.py", new ProcessSettings {
        Arguments = $"{projectName}.{projectSuffix}{sep}nuget-sources.json {projectName}.{projectSuffix}{sep}{projectName}.{projectSuffix}.csproj"
    });
});

Task("DocsGeneratePot")
    .Does(() =>
{
    StartProcess("sh", new ProcessSettings {
        Arguments = $"-c \"itstool -o po{sep}{shortName}.pot yelp{sep}C{sep}*.page\"",
        WorkingDirectory = $"{projectName}.Shared{sep}Docs"
    });
});

Task("DocsUpdatePo")
    .Does(() =>
{
    foreach (var lang in FileReadLines($".{sep}{projectName}.Shared{sep}Docs{sep}po{sep}LINGUAS"))
    {
        StartProcess("msgmerge", new ProcessSettings {
            Arguments = $"-U .{sep}{projectName}.Shared{sep}Docs{sep}po{sep}{lang}.po .{sep}{projectName}.Shared{sep}Docs{sep}po{sep}{shortName}.pot"
        });
    }
});

Task("DocsUpdateYelp")
    .Does(() =>
{
    foreach (var lang in FileReadLines($".{sep}{projectName}.Shared{sep}Docs{sep}po{sep}LINGUAS"))
    {
        StartProcess("msgfmt", new ProcessSettings {
            Arguments = $"{lang}.po -o {lang}.mo",
            WorkingDirectory = $"{projectName}.Shared{sep}Docs{sep}po"
        });
        CreateDirectory($".{sep}{projectName}.Shared{sep}Docs{sep}yelp{sep}{lang}");
        StartProcess("sh", new ProcessSettings {
            Arguments = $"-c \"itstool -m po{sep}{lang}.mo -o yelp{sep}{lang}{sep} yelp{sep}C{sep}*.page\"",
            WorkingDirectory = $"{projectName}.Shared{sep}Docs"
        });
        DeleteFile($"{projectName}.Shared{sep}Docs{sep}po{sep}{lang}.mo");
        CreateDirectory($".{sep}{projectName}.Shared{sep}Docs{sep}yelp{sep}{lang}{sep}figures");
        CopyDirectory($".{sep}{projectName}.Shared{sep}Docs{sep}yelp{sep}C{sep}figures", $".{sep}{projectName}.Shared{sep}Docs{sep}yelp{sep}{lang}{sep}figures");
    }
});

Task("DocsUpdateHtml")
    .Does(() =>
{
    foreach (var lang in FileReadLines($".{sep}{projectName}.Shared{sep}Docs{sep}po{sep}LINGUAS"))
    {
        CreateDirectory($"{projectName}.Shared{sep}Docs{sep}html{sep}{lang}");
        StartProcess("yelp-build", new ProcessSettings {
            Arguments = $"html -o html{sep}{lang}{sep} yelp{sep}{lang}{sep}",
            WorkingDirectory = $"{projectName}.Shared{sep}Docs"
        });
        if (lang == "C")
        {
            continue;
        }
        DeleteDirectory($"{projectName}.Shared{sep}Docs{sep}html{sep}{lang}{sep}figures", new DeleteDirectorySettings {
            Force = true,
            Recursive = true
        });
        DeleteFiles($"{projectName}.Shared{sep}Docs{sep}html{sep}{lang}{sep}*.css");
        DeleteFiles($"{projectName}.Shared{sep}Docs{sep}html{sep}{lang}{sep}*.js");
        ReplaceRegexInFiles($"{projectName}.Shared{sep}Docs{sep}html{sep}{lang}{sep}*.html", "href=\".*.css\"", "href=\"../C/C.css\"");
        ReplaceRegexInFiles($"{projectName}.Shared{sep}Docs{sep}html{sep}{lang}{sep}*.html", "src=\"highlight.pack.js\"", "src=\"../C/highlight.pack.js\"");
        ReplaceRegexInFiles($"{projectName}.Shared{sep}Docs{sep}html{sep}{lang}{sep}*.html", "src=\"yelp.js\"", "src=\"../C/yelp.js\"");
        ReplaceRegexInFiles($"{projectName}.Shared{sep}Docs{sep}html{sep}{lang}{sep}*.html", $"src=\"figures/{shortName}.png\"", $"src=\"../C/figures/{shortName}.png\"");
    }
});

//////////////////////////////////////////////////////////////////////
// FUNCTIONS
//////////////////////////////////////////////////////////////////////

private void FinishPublishLinux(string outDir, string prefix, string libDir, bool selfContained)
{
    var binDir = string.IsNullOrEmpty(prefix) ? $"{outDir}/bin" : $"{outDir}{prefix}/bin";
    CreateDirectory(binDir);
    CopyFileToDirectory($"./{projectName}.Shared/{appId}.in", binDir);
    ReplaceTextInFiles($"{binDir}/{appId}.in", "@EXEC@", selfContained ? $"{libDir}/{appId}/{projectName}.{projectSuffix}" : $"dotnet {libDir}/{appId}/{projectName}.{projectSuffix}.dll");
    MoveFile($"{binDir}/{appId}.in", $"{binDir}/{appId}");
    StartProcess("chmod", new ProcessSettings{
        Arguments = $"+x {binDir}/{appId}"
    });

    var shareDir = string.IsNullOrEmpty(prefix) ? $"{outDir}/share" : $"{outDir}{prefix}/share";

    var iconsScalableDir = $"{shareDir}{sep}icons{sep}hicolor{sep}scalable{sep}apps";
    CreateDirectory(iconsScalableDir);
    CopyFileToDirectory($".{sep}{projectName}.Shared{sep}Resources{sep}{appId}.svg", iconsScalableDir);
    CopyFileToDirectory($".{sep}{projectName}.Shared{sep}Resources{sep}{appId}-devel.svg", iconsScalableDir);
    var iconsSymbolicDir = $"{shareDir}{sep}icons{sep}hicolor{sep}symbolic{sep}apps";
    CreateDirectory(iconsSymbolicDir);
    CopyFileToDirectory($".{sep}{projectName}.Shared{sep}Resources{sep}{appId}-symbolic.svg", iconsSymbolicDir);

    var desktopDir = $"{shareDir}/applications";
    CreateDirectory(desktopDir);
    CopyFileToDirectory($"./{projectName}.Shared/{appId}.desktop.in", desktopDir);
    ReplaceTextInFiles($"{desktopDir}/{appId}.desktop.in", "@EXEC@", $"{prefix}/bin/{appId}");
    MoveFile($"{desktopDir}/{appId}.desktop.in", $"{desktopDir}/{appId}.desktop");

    var metainfoDir = $"{shareDir}/metainfo";
    CreateDirectory(metainfoDir);
    CopyFileToDirectory($"./{projectName}.Shared/{appId}.metainfo.xml.in", metainfoDir);
    ReplaceTextInFiles($"{metainfoDir}/{appId}.metainfo.xml.in", "@PROJECT@", $"{projectName}.{projectSuffix}");
    MoveFile($"{metainfoDir}/{appId}.metainfo.xml.in", $"{metainfoDir}/{appId}.metainfo.xml");

    var mimeDir = $"{shareDir}/mime/packages";
    CreateDirectory(mimeDir);
    CopyFileToDirectory($"./{projectName}.Shared/{appId}.extension.xml", mimeDir);
}

private void PostPublishGNOME(string outDir, string prefix, string libDir)
{
    var shareDir = string.IsNullOrEmpty(prefix) ? $"{outDir}{sep}share" : $"{outDir}{prefix}{sep}share";

    CreateDirectory($"{shareDir}{sep}{appId}");
    MoveFileToDirectory($"{outDir}{libDir}{sep}{appId}{sep}{appId}.gresource", $"{shareDir}{sep}{appId}");

    var servicesDir = $"{shareDir}{sep}dbus-1{sep}services";
    CreateDirectory(servicesDir);
    CopyFileToDirectory($".{sep}{projectName}.GNOME{sep}{appId}.service.in", servicesDir);
    ReplaceTextInFiles($"{servicesDir}{sep}{appId}.service.in", "@PREFIX@", $"{prefix}");
    MoveFile($"{servicesDir}{sep}{appId}.service.in", $"{servicesDir}{sep}{appId}.service");

    FileAppendLines($"{shareDir}{sep}applications{sep}{appId}.desktop" , new string[] { "\nDBusActivatable=true" });

    foreach (var dir in GetSubDirectories($".{sep}{projectName}.Shared{sep}Docs{sep}yelp"))
    {
        CopyDirectory(dir, $"{shareDir}{sep}help{sep}{dir.Segments[dir.Segments.Length-1]}{sep}{shortName}");
    }
}

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

var requiresUI = target switch
{
    "Clean" or "Build" or "Run" or "Publish" or "FlatpakSourcesGen" => true,
    _ => false
};
if (!requiresUI)
{
    RunTarget(target);
    return;
}

if (string.IsNullOrEmpty(ui))
{
    throw new Exception("UI is not set. Use --ui option or NICK_UI environment variable.");
}
projectSuffix = ui.ToLower() switch
{
    "gnome" => "GNOME",
    _ => ""
};
if (string.IsNullOrEmpty(projectSuffix))
{
    throw new Exception("Unknown UI. Possible values: gnome.");
}

RunTarget(target);