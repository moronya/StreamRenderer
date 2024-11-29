public class ScreenRenderer
{
    private int Width { get; set; }
    private int Height { get; set; }
    private char[,] ScreenBuffer;
    private ConsoleColor[,] ColorBuffer;
    private bool IsInitialized;

    public void ProcessStream(byte[] stream)
    {
        int index = 0;

        while (index < stream.Length)
        {
            byte command = stream[index++];

            switch (command)
            {
                case 0x1:
                    ProcessScreenSetup(stream, ref index);
                    break;
                case 0x2:
                    ProcessDrawCharacter(stream, ref index);
                    break;
                case 0x3:
                    ProcessDrawLine(stream, ref index);
                    break;
                case 0x4:
                    ProcessRenderText(stream, ref index);
                    break;
                case 0x5:
                    ProcessCursorMovement(stream, ref index);
                    break;
                case 0x6:
                    ProcessDrawAtCursor(stream, ref index);
                    break;
                case 0x7:
                    ClearScreen();
                    break;
                case 0xFF:
                    return;
                default:
                    throw new InvalidOperationException($"Unknown command byte: {command}");
            }
        }
    }

    private void ProcessScreenSetup(byte[] stream, ref int index)
    {
        if (stream.Length - index < 3)
            throw new InvalidOperationException("Invalid Screen Setup data.");

        Width = stream[index++];
        Height = stream[index++];
        byte colorMode = stream[index++]; // Currently unused but reserved for future.

        ScreenBuffer = new char[Height, Width];
        ColorBuffer = new ConsoleColor[Height, Width];
        ClearScreen();
        IsInitialized = true;
    }

    private void ProcessDrawCharacter(byte[] stream, ref int index)
    {
        if (!IsInitialized || stream.Length - index < 4)
            throw new InvalidOperationException("Invalid Draw Character data or uninitialized screen.");

        int x = stream[index++];
        int y = stream[index++];
        ConsoleColor color = (ConsoleColor)stream[index++];
        char character = (char)stream[index++];

        if (x < Width && y < Height)
        {
            ScreenBuffer[y, x] = character;
            ColorBuffer[y, x] = color;
        }
    }

    private void ProcessDrawLine(byte[] stream, ref int index)
    {
        if (!IsInitialized || stream.Length - index < 6)
            throw new InvalidOperationException("Invalid Draw Line data or uninitialized screen.");

        int x1 = stream[index++];
        int y1 = stream[index++];
        int x2 = stream[index++];
        int y2 = stream[index++];
        ConsoleColor color = (ConsoleColor)stream[index++];
        char character = (char)stream[index++];

        int dx = Math.Abs(x2 - x1), sx = x1 < x2 ? 1 : -1;
        int dy = -Math.Abs(y2 - y1), sy = y1 < y2 ? 1 : -1;
        int err = dx + dy, e2;

        while (true)
        {
            if (x1 >= 0 && x1 < Width && y1 >= 0 && y1 < Height)
            {
                ScreenBuffer[y1, x1] = character;
                ColorBuffer[y1, x1] = color;
            }

            if (x1 == x2 && y1 == y2) break;

            e2 = 2 * err;
            if (e2 >= dy) { err += dy; x1 += sx; }
            if (e2 <= dx) { err += dx; y1 += sy; }
        }
    }

    private void ProcessRenderText(byte[] stream, ref int index)
    {
        if (!IsInitialized || stream.Length - index < 3)
            throw new InvalidOperationException("Invalid Render Text data or uninitialized screen.");

        int x = stream[index++];
        int y = stream[index++];
        ConsoleColor color = (ConsoleColor)stream[index++];

        while (index < stream.Length && stream[index] != 0xFF)
        {
            if (x >= 0 && x < Width && y >= 0 && y < Height)
            {
                ScreenBuffer[y, x] = (char)stream[index];
                ColorBuffer[y, x] = color;
            }
            x++;
            index++;
        }
    }

    private void ProcessCursorMovement(byte[] stream, ref int index)
    {
        if (!IsInitialized || stream.Length - index < 2)
            throw new InvalidOperationException("Invalid Cursor Movement data or uninitialized screen.");

        Console.SetCursorPosition(stream[index++], stream[index++]);
    }

    private void ProcessDrawAtCursor(byte[] stream, ref int index)
    {
        if (!IsInitialized || stream.Length - index < 2)
            throw new InvalidOperationException("Invalid Draw At Cursor data or uninitialized screen.");

        char character = (char)stream[index++];
        ConsoleColor color = (ConsoleColor)stream[index++];

        int x = Console.CursorLeft;
        int y = Console.CursorTop;

        if (x < Width && y < Height)
        {
            ScreenBuffer[y, x] = character;
            ColorBuffer[y, x] = color;
        }
    }

    private void ClearScreen()
    {
        if (!IsInitialized) return;

        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                ScreenBuffer[y, x] = ' ';
                ColorBuffer[y, x] = ConsoleColor.Black;
            }
        }
    }

    public void Render()
    {
        Console.Clear();

        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                Console.ForegroundColor = ColorBuffer[y, x];
                Console.Write(ScreenBuffer[y, x]);
            }
            Console.WriteLine();
        }
    }
}