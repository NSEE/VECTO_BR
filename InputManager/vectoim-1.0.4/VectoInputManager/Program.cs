using System;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Cli;
using VectoInputManager.Commands;
using VectoInputManager.Core;

[assembly: InternalsVisibleTo("VectoInputManagerTests")]

Console.OutputEncoding = System.Text.Encoding.UTF8;

var services = new ServiceCollection();
services.ConfigureServices();

var firstArg = args.Length > 0 ? args[0].ToLower() : string.Empty;

if (firstArg == "--ver" || firstArg == "--version")
{
    AnsiConsole.MarkupLine($"[bold green]VECTO Input Manager[/]");
    AnsiConsole.MarkupLine($"[yellow]{Consts.AppVersion}[/]");
    return 0;
}

if (firstArg == "-a" || firstArg == "-lic" || firstArg == "--license")
{
    var exePath = AppContext.BaseDirectory;
    var lines = File.ReadAllText(Path.Combine(exePath, "LICENSE.txt"));
    AnsiConsole.MarkupLine(lines);
    return 0;
}

if (firstArg == "--docs" || firstArg == "-d" || firstArg == "--doc")
{
    var exePath = AppContext.BaseDirectory;
    var docUrl = Path.Combine(exePath, "html", "index.html");

    if (File.Exists(docUrl))
        docUrl = new Uri(docUrl).AbsoluteUri;
    else
        docUrl = Consts.HelpUrl;

    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
    {
        FileName = docUrl,
        UseShellExecute = true
    });

    return 0;
}

var app = new CommandApp(new TypeRegistrar(services));
app.Configure(config =>
{
    config.SetApplicationName("vectoim");
    config.ConfigureProgramCommands();
    config.SetHelpProvider(new CustomHelpProvider(config.Settings));
});


return app.Run(args);
