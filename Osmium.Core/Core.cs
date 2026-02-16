namespace Osmium.Core
{
    public class Vector2
    {
        public int x;
        public int y;

        public Vector2(int p_x, int p_y)
        {
            x = p_x;
            y = p_y;
        }

        public static readonly Vector2 up = new(0, 1);
        public static readonly Vector2 right = new(1, 0);
        public static readonly Vector2 down = new(0, -1);
        public static readonly Vector2 left = new(-1, 0);

        public static readonly Vector2[] orthogonalDirections = [up, right, down, left];
        public static readonly Vector2[] diagonalDirections = [up + right, right + down, down + left, left + up];
        public static readonly Vector2[] allDirections = [up, up + right, right, right + down, down, down + left, left, left + up];
        public static readonly Vector2[] hippogonalDirections = [new(1, 2), new(2, 1), new(2, -1), new(1, -2), new(-1, -2), new(-2, -1), new(-2, 1), new(-1, 2)];

        public static Vector2 operator +(Vector2 a, Vector2 b)
            => new(a.x + b.x, a.y + b.y);

        public static Vector2 operator -(Vector2 a, Vector2 b)
            => new(a.x - b.x, a.y - b.y);

        public override string ToString()
            => $"({x}, {y})";
    }

    public class Piece
    {
        public Type type;
        public bool isWhite;

        public enum Type
        {
            Pawn,
            Bishop,
            Knight,
            Rook,
            Queen,
            King
        }

        public Piece(Type p_type, bool p_isWhite)
        {
            type = p_type;
            isWhite = p_isWhite;
        }      
        
        public static Piece FromChar(char ch)
        {
            bool isWhite = char.IsUpper(ch);
            return char.ToLower(ch) switch
            {
                'p' => new(Type.Pawn, isWhite),
                'b' => new(Type.Bishop, isWhite),
                'n' => new(Type.Knight, isWhite),
                'r' => new(Type.Rook, isWhite),
                'q' => new(Type.Queen, isWhite),
                'k' => new(Type.King, isWhite),
                _ => throw new Exception(),
            };
        }
    }

    public class Position
    {
        Piece?[,] board = new Piece?[8, 8];
        bool whiteToMove;
        Vector2? enPassantSquare;
        int halfmoveClock;
        int fullmoves;

        public static Position FromFEN(string fen)
        {
            Position result = new();
            //
            var fields = fen.Split(' ');
            // 0th field = piece placement data
            var ranks = fields[0].Split('/');
            for (int rank = 0; rank < 8; rank++)
            {
                var chars = ranks[rank].ToCharArray();
                int file = 0;
                foreach (var ch in chars)
                {
                    if (char.IsDigit(ch))
                        file += ch - '0'; // effectively converts ch to an int
                    else
                    {
                        result.board[rank, file] = Piece.FromChar(ch);
                        file++;
                    }
                }
            }
            //
            return result;
        }
    }
}
