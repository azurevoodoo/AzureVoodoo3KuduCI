$ProgressPreference='SilentlyContinue'
$CAKE_LOG_PATH = "$env:DEPLOYMENT_TARGET\default.htm"
$CAKE_BUILDSTATUS_PATH = "$env:DEPLOYMENT_TARGET\buildstatus.svg"

if (!(Test-Path $env:DEPLOYMENT_TARGET))
{
    EXIT 404
}

if (!(Test-Path .\tools))
{
    md tools
}


'<html><body><img src="buildstatus.svg" /><pre>'|Set-Content $CAKE_LOG_PATH -Encoding UTF8 -PassThru

Invoke-WebRequest https://dist.nuget.org/win-x86-commandline/latest/nuget.exe -OutFile Tools\nuget.exe|Add-Content $CAKE_LOG_PATH -Encoding UTF8 -PassThru
SET NUGET_EXE=Tools\nuget.exe

Remove-Item .\tools\* -Recurse -Force -Exclude packages.config,nuget.exe|Add-Content $CAKE_LOG_PATH -Encoding UTF8 -PassThru
Set-Location .\tools
.\nuget.exe install Cake -ExcludeVersion -Verbosity Detailed|Add-Content $CAKE_LOG_PATH -Encoding UTF8 -PassThru
Set-Location ..


.\Tools\Cake\Cake.exe -version|Add-Content $CAKE_LOG_PATH -Encoding UTF8 -PassThru
.\Tools\Cake\Cake.exe build.cake -Configuration=debug|Add-Content $CAKE_LOG_PATH -Encoding UTF8 -PassThru
if ($LASTEXITCODE -eq 0)
{
    "Build passing!"
    '<svg xmlns="http://www.w3.org/2000/svg" width="92" height="20"><linearGradient id="b" x2="0" y2="100%"><stop offset="0" stop-color="#bbb" stop-opacity=".1"/><stop offset="1" stop-opacity=".1"/></linearGradient><mask id="a"><rect width="92" height="20" rx="3" fill="#fff"/></mask><g mask="url(#a)"><path fill="#555" d="M0 0h39v20H0z"/><path fill="#97CA00" d="M39 0h53v20H39z"/><path fill="url(#b)" d="M0 0h92v20H0z"/></g><g fill="#fff" text-anchor="middle" font-family="DejaVu Sans,Verdana,Geneva,sans-serif" font-size="11"><text x="19.5" y="15" fill="#010101" fill-opacity=".3">Kudu</text><text x="19.5" y="14">Kudu</text><text x="64.5" y="15" fill="#010101" fill-opacity=".3">passing</text><text x="64.5" y="14">passing</text></g></svg>' | Set-Content $CAKE_BUILDSTATUS_PATH -Encoding UTF8
}
else
{
    "Build failed!"
    '<svg xmlns="http://www.w3.org/2000/svg" width="80" height="20"><linearGradient id="b" x2="0" y2="100%"><stop offset="0" stop-color="#bbb" stop-opacity=".1"/><stop offset="1" stop-opacity=".1"/></linearGradient><mask id="a"><rect width="80" height="20" rx="3" fill="#fff"/></mask><g mask="url(#a)"><path fill="#555" d="M0 0h39v20H0z"/><path fill="#e05d44" d="M39 0h41v20H39z"/><path fill="url(#b)" d="M0 0h80v20H0z"/></g><g fill="#fff" text-anchor="middle" font-family="DejaVu Sans,Verdana,Geneva,sans-serif" font-size="11"><text x="19.5" y="15" fill="#010101" fill-opacity=".3">Kudu</text><text x="19.5" y="14">Kudu</text><text x="58.5" y="15" fill="#010101" fill-opacity=".3">failed</text><text x="58.5" y="14">failed</text></g></svg>' | Set-Content $CAKE_BUILDSTATUS_PATH -Encoding UTF8
}

"</pre></body></html>"|Add-Content $CAKE_LOG_PATH -Encoding UTF8 -PassThru

#Regardless what happens report ok
exit 0