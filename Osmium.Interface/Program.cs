using Osmium.Core;

namespace Osmium.Interface
{
    internal class Program
    {
        static void Main()
        {
            WaitForCommandAndReact();
        }

        static void WaitForCommandAndReact()
        {
            string input = Console.ReadLine();
            switch (input)
            {
                default:
                    WaitForCommandAndReact();
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

        public static void PrintPosition(Position position, PieceOptions pieceOptions, BackgroundOptions backgroundOptions)
        {
            string output = "";
            for (int rank = 7; rank >= 0; rank--)
            {
                for (int file = 0; file < 8; file++)
                {
                    if (position.board[rank, file] is null)
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
                            PieceOptions.Ascii => position.board[rank, file].ToString(),
                            PieceOptions.Unicode => unicodePieces[position.board[rank, file].ToChar()],
                            PieceOptions.UnicodeInverted => unicodePieces[new Piece(position.board[rank, file].type, !position.board[rank, file].isWhite).ToChar()],
                            _ => throw new Exception()
                        };
                }
                output += rank > 0 ? "\n" : "";
            }
            Console.WriteLine(output);
        }

        static bool IsSquareWhite(int rank, int file)
            => (rank + file) % 2 != 0;

        static string GetSquareShadeString(int rank, int file, bool invert)
            => IsSquareWhite(rank, file) ? "░░" : "▒▒";
    }
}
