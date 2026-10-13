using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Cli.Help;
using Spectre.Console.Rendering;

namespace VectoInputManager.Commands;

internal class CustomHelpProvider(ICommandAppSettings settings)
    : HelpProvider(settings)
{
    public override IEnumerable<IRenderable> GetOptions(ICommandModel model, ICommandInfo? command)
    {
        // Add custom --ver option to the root command help
        if (command == null)
        {
            var r = base.GetOptions(model, command).ToList();
            r.Add(new Markup("    -a, -lic      Prints license\n"));
            r.Add(new Markup("    --ver         Prints version\n"));
            r.Add(new Markup("    --docs        Opens documentation\n"));
            return r;
        }
        else
        {
            return base.GetOptions(model, command);
        }
    }

    public override IEnumerable<IRenderable> GetUsage(ICommandModel model, ICommandInfo? command)
    {
        return base.GetUsage(model, command);
    }
}