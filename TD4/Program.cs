/*
  | instruction | bit7-bit4 | bit3-bit0 |
  | ----------- | --------- | --------- |
  | ADD A, Im   | 0000      | Im        |
  | MOV A, B    | 0001      | 0000      |
  | IN A        | 0010      | 0000      |
  | MOV A, Im   | 0011      | Im        |
  | MOV B, A    | 0100      | 0000      |
  | ADD B, Im   | 0101      | Im        |
  | IN B        | 0110      | 0000      |
  | MOV B, Im   | 0111      | Im        |
  |             | 1000      |           |
  | OUT B       | 1001      | 0000      |
  |             | 1010      |           |
  | OUT Im      | 1011      | Im        |
  |             | 1100      |           |
  |             | 1101      |           |
  | JZ Im       | 1110      | Im        |
  | JMP Im      | 1111      | Im        |
 */
using System.Globalization;
using Spectre.Console;

if (args.Length != 1)
{
    Console.WriteLine("Usage: td4 <source.asm>");
    return -1;
}

if (!File.Exists(args[0]))
{
    Console.WriteLine($"File {0} does not exist");
    return -2;
}

var result = ParseSource(args[0]);
if(!result.Success)
{
    return -1;
}
WriteOutput(result.Opcodes);
return 0;

static Result ParseSource(string filename)
{
    var CODES = new Dictionary<string, int>
    {
        { "adda,",  0b0000_0000 },
        { "mova,b", 0b0001_0000 },
        { "ina",    0b0010_0000 },
        { "mova,",  0b0011_0000 },
        { "movb,a", 0b0100_0000 },
        { "addb,",  0b0101_0000 },
        { "inb",    0b0110_0000 },
        { "movb,",  0b0111_0000 },
        { "ins8",   0b1000_0000 },
        { "outb",   0b1001_0000 },
        { "insa",   0b1010_0000 },
        { "out",    0b1011_0000 },
        { "insc",   0b1100_0000 },
        { "insd",   0b1101_0000 },
        { "jz",     0b1110_0000 },
        { "jmp",    0b1111_0000 },
    };

    try
    {
        string[] source = File.ReadAllText(filename)
            .Split('\n')
            .Select(x => x.Trim())
            .ToArray();

        string[] asm = source
            .Select(x => x.Replace(" ", "").ToLowerInvariant())
            .ToArray();

        var opcodes = new byte[16];
        var index = 0;
        bool error = false;

        for (int line = 1; line <= asm.Length; line++)
        {
            string code = asm[line - 1];
            if (code.StartsWith("#") || string.IsNullOrEmpty(code))
            {
                // comment or blank line
                continue;
            }

            if (index == 16)
            {
                // We only have 16 bytes of ROM
                Console.WriteLine($"ROM full on line {line}");
                error = true;
                break;
            }

            var split = code.Split("0x");
            byte immediate = 0;
            if (split.Length > 2 ||
                !CODES.ContainsKey(split[0]) ||
                (split.Length == 2 &&
                  byte.TryParse(split[1], NumberStyles.HexNumber, null as IFormatProvider, out immediate) &&
                  immediate > 0xF))
            {
                error = true;
                SyntaxError(line);
                continue;
            }

            var op = CODES[split[0]];
            opcodes[index++] = (byte)(op | immediate);
        }
        if (error) return Result.CreateFailure();

        return Result.CreateSuccess(opcodes);
    }
    catch (Exception ex)
    {
        AnsiConsole.WriteLine($"[red]Error: {ex}[/]");
        return Result.CreateFailure();
    }
}

static void WriteOutput(byte[] opcodes)
{
    var table = new Table();
    table.AddColumns(
        new TableColumn("").Centered(),
        new TableColumn("").Centered(),
        new TableColumn("").Centered(),
        new TableColumn("").Centered()
    );
    table.NoBorder();
    for (int i = 0; i < opcodes.Length;)
    {
        table.AddRow(new[]
        {
            CreatePanel(opcodes[i++]),
            CreatePanel(opcodes[i++]),
            CreatePanel(opcodes[i++]),
            CreatePanel(opcodes[i++]),
        });
    }
    AnsiConsole.Write(table);
}

static Panel CreatePanel(byte opcode)
{
    string binary = Convert.ToString(opcode, 2).PadLeft(8, '0');
    var markup = binary.Reverse()
        .Select(c => c == '1' ? "[green]1[/]" : "[red]0[/]")
        .ToArray();
    var output = new Markup(string.Join("", markup));

    var panel = new Panel(output);
    panel.BorderColor(Color.Aqua);
    return panel;
}

static void SyntaxError(int line) =>
    AnsiConsole.WriteLine($"[red]Syntax error on line {line}[/]");
