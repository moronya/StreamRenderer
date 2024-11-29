using System;
using System.Text;



// Usage example
class Program
{
    static void Main()
    {
        byte[] binaryStream =
        {
            0x1, 20, 10, 0x01,      // Screen setup: 20x10, color mode 1
            0x2, 5, 5, 12, (byte)'A', // Draw character 'A' at (5, 5) with color index 12
            0x3, 2, 2, 10, 8, 14, (byte)'*', // Draw line from (2,2) to (10,8) with color 14 and '*'
            0x4, 1, 1, 10, (byte)'H', (byte)'e', (byte)'l', (byte)'l', (byte)'o', // Render text "Hello" at (1,1)
            0xFF                    // End of file
        };

        ScreenRenderer renderer = new ScreenRenderer();
        renderer.ProcessStream(binaryStream);
        renderer.Render();
    }
}
