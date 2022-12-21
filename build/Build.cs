using System.Linq;
using Nuke.Common;
using Nuke.Common.CI.AzurePipelines;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities.Collections;

class Build : NukeBuild
{
    public static int Main () => Execute<Build>(x => x.Pack);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;
    [Solution]
    readonly Solution Solution;

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            RootDirectory.GlobDirectories("**/bin", "**/obj").ForEach(FileSystemTasks.DeleteDirectory);
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            DotNetTasks.DotNetRestore(s => s.SetProjectFile(Solution.GetProject("fishbus")));
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetTasks.DotNetBuild(s => 
                s.SetProjectFile(Solution.GetProject("fishbus"))
                    .EnableNoLogo()
                    .EnableNoRestore()
                    .EnableNoConsoleLogger()
                    .SetConfiguration(Configuration));
        });
    
    Target Test => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            var testProject = Solution.GetProject("FishbusTests");
            DotNetTasks.DotNetTest(x => x
                .SetProjectFile(testProject)
                .SetConfiguration(Configuration)
                .SetLoggers("xunit")
                .SetDataCollector("XPlat Code Coverage"));
            
            var testResults = testProject?.Directory.GlobFiles("**/TestResults/TestResults.xml")
                .Select(x => x.ToString());
            
            AzurePipelines.Instance?.PublishTestResults(
                "Fishbus unit test results",
                AzurePipelinesTestResultsType.XUnit,
                testResults);
        });
    
    Target Pack => _ => _
        .DependsOn(Test)
        .Executes(() =>
        {
            DotNetTasks.DotNetPack(s => 
                s.SetProject(Solution.GetProject("fishbus"))
                    .SetConfiguration(Configuration));
        });

}
