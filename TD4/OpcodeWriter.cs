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
using Spectre.Console;

internal static class OpcodeWriter
{
    public static void Output(byte[] opcodes)
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
}