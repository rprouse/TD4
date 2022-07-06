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
  | OUT B       | 1001      | 0000      |
  | OUT Im      | 1011      | Im        |
  | JZ Im       | 1110      | Im        |
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

try
{
    string[] source = File.ReadAllText(args[0])
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

        if (index == 16 && !code.StartsWith("#") && !string.IsNullOrEmpty(code))
        {
            Console.WriteLine($"ROM full on line {line}");
            error = true;
            break;
        }
        else if (code.StartsWith("adda,0x"))
        {
            if (TryParseImmediate(code, 7, out byte immediate))
            {
                opcodes[index++] = immediate;
            }
            else
            {
                error = true;
                SyntaxError(line);
            }
        }
        else if (code == "mova,b")
        {
            opcodes[index++] = 0b0001_0000;
        }
        else if (code == "ina")
        {
            opcodes[index++] = 0b0010_0000;
        }
        else if (code.StartsWith("mova,0x"))
        {
            if (TryParseImmediate(code, 7, out byte immediate))
            {
                opcodes[index++] = (byte)(0b0011_0000 | immediate);
            }
            else
            {
                error = true;
                SyntaxError(line);
            }
        }
        else if (code == "movb,a")
        {
            opcodes[index++] = 0b0100_0000;
        }
        else if (code.StartsWith("addb,0x"))
        {
            if (TryParseImmediate(code, 7, out byte immediate))
            {
                opcodes[index++] = (byte)(0b0101_0000 | immediate);
            }
            else
            {
                error = true;
                SyntaxError(line);
            }
        }
        else if (code == "inb")
        {
            opcodes[index++] = 0b0110_0000;
        }
        else if (code.StartsWith("movb,0x"))
        {
            if (TryParseImmediate(code, 7, out byte immediate))
            {
                opcodes[index++] = (byte)(0b0111_0000 | immediate);
            }
            else
            {
                error = true;
                SyntaxError(line);
            }
        }
        else if (code == "outb")
        {
            opcodes[index++] = 0b1001_0000;
        }
        else if (code.StartsWith("out0x"))
        {
            if (TryParseImmediate(code, 5, out byte immediate))
            {
                opcodes[index++] = (byte)(0b1011_0000 | immediate);
            }
            else
            {
                error = true;
                SyntaxError(line);
            }
        }
        else if (code.StartsWith("jz0x"))
        {
            if (TryParseImmediate(code, 4, out byte immediate))
            {
                opcodes[index++] = (byte)(0b1110_0000 | immediate);
            }
            else
            {
                error = true;
                SyntaxError(line);
            }
        }
        else if (code.StartsWith("jmp0x"))
        {
            if (TryParseImmediate(code, 5, out byte immediate))
            {
                opcodes[index++] = (byte)(0b1111_0000 | immediate);
            }
            else
            {
                error = true;
                SyntaxError(line);
            }
        }
        else if (!code.StartsWith("#") && !string.IsNullOrEmpty(code))
        {
            error = true;
            SyntaxError(line);
        }
    }
    if (error) return 1;

    var table = new Table();
    table.AddColumns(
        new TableColumn("").Centered(),
        new TableColumn("").Centered(),
        new TableColumn("").Centered(),
        new TableColumn("").Centered()
    );
    table.NoBorder();
    for (int i = 0; i < opcodes.Length; )
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

    return 0;
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex}");
    return -99;
}

static bool TryParseImmediate(string code, int startIndex, out byte immediate)
{
    var imstr = code.Substring(startIndex);
    return byte.TryParse(imstr, NumberStyles.HexNumber, null as IFormatProvider, out immediate) && immediate <= 0xF;
}

static Panel CreatePanel(byte opcode)
{
    string binary = Convert.ToString(opcode, 2).PadLeft(8, '0');
    string output = new string(binary.Reverse().ToArray());

    return new Panel(output);
}

static void SyntaxError(int line)
{
    Console.WriteLine($"Syntax error on line {line}");
}