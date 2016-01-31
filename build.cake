#tool "xunit.runner.console"
#addin "Cake.Slack"
#addin "Cake.Kudu"
#addin "nuget:https://www.myget.org/F/wcomab/api/v2?package=Cake.Git&prerelease"

///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument<string>("target", "Default");
var configuration = Argument<string>("configuration", "Release");

///////////////////////////////////////////////////////////////////////////////
// GLOBAL VARIABLES
///////////////////////////////////////////////////////////////////////////////

var solutions = GetFiles("./**/*.sln");
var solutionPaths = solutions.Select(solution => solution.GetDirectory());
var slackWebHookUrl = EnvironmentVariable("slackWebHookUrl");
var gitCommit = GitLogTip("./");
Action<Func<string>> postSlackMessage = (Kudu.IsRunningOnKudu && !string.IsNullOrWhiteSpace(slackWebHookUrl))
        ? new Action<Func<string>>(
                message=>{
                    Slack.Chat.PostMessage(
                        channel:"#azure",
                        text:string.Format(
                            "`[{0}, {1}/{2}]`\r\n{3}",
                            Kudu.WebSite.Name,
                            Kudu.SCM.Branch,
                            Kudu.SCM.CommitId,
                            message()
                            ),
                        messageSettings:new SlackChatMessageSettings { IncomingWebHookUrl = slackWebHookUrl }
                    );
                }
          )
        : new Action<Func<string>>(message=>{});

Action<Exception> postSlackException = exception=>postSlackMessage(()=>string.Format("```{0}```", exception));

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(() =>
{
    // Executed BEFORE the first task.
    Information("{0:yyyy-MM-dd HH:mm:ss} Running tasks...", DateTime.UtcNow);
    postSlackMessage(()=>string.Format(
        "build started beacuse {0} (\"{2}\") by {1}...",
        gitCommit.Sha,
        gitCommit.Author.Name,
        gitCommit.MessageShort
    ));
});

Teardown(() =>
{
    // Executed AFTER the last task.
    Information("{0:yyyy-MM-dd HH:mm:ss} Finished running tasks.", DateTime.UtcNow);
    postSlackMessage(()=>"build finished.");
});

///////////////////////////////////////////////////////////////////////////////
// TASK DEFINITIONS
///////////////////////////////////////////////////////////////////////////////

Task("Clean")
    .ReportError(postSlackException)
    .Does(() =>
{
    // Clean solution directories.
    foreach(var path in solutionPaths)
    {
        Information("Cleaning {0}", path);
        CleanDirectories(path + "/**/bin/" + configuration);
        CleanDirectories(path + "/**/obj/" + configuration);
    }
});

Task("Restore")
    .ReportError(postSlackException)
    .Does(() =>
{
    // Restore all NuGet packages.
    foreach(var solution in solutions)
    {
        Information("Restoring {0}...", solution);
        NuGetRestore(solution);
    }
});

Task("Build")
    .IsDependentOn("Clean")
    .IsDependentOn("Restore")
    .ReportError(postSlackException)
    .Does(() =>
{
    // Build all solutions.
    foreach(var solution in solutions)
    {
        Information("Building {0}", solution);
        MSBuild(solution, settings =>
            settings.SetPlatformTarget(PlatformTarget.MSIL)
                .WithTarget("Build")
                .SetConfiguration(configuration));
    }
});

Task("Run-Unit-Tests")
    .IsDependentOn("Build")
    .ReportError(postSlackException)
    .Does(() =>
{
    XUnit2("./src/**/bin/" + configuration + "/*.Tests.dll", new XUnit2Settings {
        NoAppDomain = true
        });
});

///////////////////////////////////////////////////////////////////////////////
// TARGETS
///////////////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Run-Unit-Tests");

///////////////////////////////////////////////////////////////////////////////
// EXECUTION
///////////////////////////////////////////////////////////////////////////////

RunTarget(target);
