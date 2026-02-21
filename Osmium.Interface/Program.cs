using Osmium.Core;

namespace Osmium.Interface
{
    internal class Program
    {
        static void Main()
        {
            PrettyPrinter.Print(Position.startingPosition);
            PrettyPrinter.Print(Position.FromFEN("k7/8/8/8/8/8/8/R6K w - - 0 1"));
            PregameLoop();
        }

        static void PregameLoop()
        {
            string input = Console.ReadLine();
            switch (input)
            {
                default:
                    break;
            }
        }
    }

    public class PrettyPrinter
    {
        static readonly Dictionary<char, char> unicodePieces = new()
        {
            { 'p', '♙'},
            { 'b', '♗'},
            { 'n', '♘' },
            { 'r', '♖' },
            { 'q', '♕' },
            { 'k', '♔' },
            { 'P', '♟'},
            { 'B', '♝'},
            { 'N', '♞' },
            { 'R', '♜' },
            { 'Q', '♛' },
            { 'K', '♚' }
        };

        public enum PieceOptions
        {
            Ascii,
            Unicode,
            UnicodeInverted
        }

        public enum BackgroundOptions
        {
            Simple,
            Shaded,
            ShadedInverted
        }

        public static void Print(Position position, PieceOptions pieceOptions, BackgroundOptions backgroundOptions)
        {
            string output = "";
            for (int rank = 7; rank >= 0; rank--)
            {
                output += (rank + 1).ToString() + " ";
                for (int file = 0; file < 8; file++)
                {
                    if (position.GetPiece(rank, file) is null)
                        output += backgroundOptions switch
                        {
                            BackgroundOptions.Simple => ". ",
                            BackgroundOptions.Shaded => GetSquareShadeString(rank, file, false),
                            BackgroundOptions.ShadedInverted => GetSquareShadeString(rank, file, true),
                            _ => throw new Exception()
                        };
                    else
                        output += "." + pieceOptions switch
                        {
                            PieceOptions.Ascii => position.GetPiece(rank, file)?.ToString(),
                            PieceOptions.Unicode => unicodePieces[position.GetPiece(rank, file).ToChar()],
                            PieceOptions.UnicodeInverted => unicodePieces[new Piece(position.GetPiece(rank, file).type, !position.GetPiece(rank, file).isWhite).ToChar()],
                            _ => throw new Exception()
                        };
                }
                output += "\n";
            }
            output += "  a b c d e f g h ";
            Console.WriteLine(output);
        }

        public static void Print(Position position)
            => Print(position, PieceOptions.Ascii, BackgroundOptions.ShadedInverted);

        static bool IsSquareWhite(int rank, int file)
            => (rank + file) % 2 != 0;

        static string GetSquareShadeString(int rank, int file, bool invert)
            => (IsSquareWhite(rank, file) ^ invert) ? "░░" : "▒▒";
    }
}
