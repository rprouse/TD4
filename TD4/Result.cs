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
internal class Result
{
    public bool Success { get; set; }
    public byte[] Opcodes { get; set; }

    private Result(bool success, byte[] opcodes)
    {
        Success = success;
        Opcodes = opcodes;
    }

    public static Result CreateSuccess(byte[] opcodes) =>
        new Result(true, opcodes);

    public static Result CreateFailure() => 
        new Result(false, new byte[0]);
}
