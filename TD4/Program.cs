using Spectre.Console;

if (args.Length != 1)
{
    AnsiConsole.WriteLine("[cyan]Usage:[/] td4 <source.asm>");
    return -1;
}

if (!File.Exists(args[0]))
{
    AnsiConsole.WriteLine($"[red]File {0} does not exist[/]");
    return -2;
}

var parser = new Parser(args[0]);
var result = parser.Parse();
if(!result.Success)
{
    return -1;
}
OpcodeWriter.Output(result.Opcodes);
return 0;
