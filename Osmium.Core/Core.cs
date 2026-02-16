namespace Osmium.Core
{
    public class Vector2
    {
        public int file, rank; // which file = x, which rank = y

        public Vector2(int p_file, int p_rank)
        {
            file = p_file;
            rank = p_rank;
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
            => new(a.file + b.file, a.rank + b.rank);

        public static Vector2 operator -(Vector2 a, Vector2 b)
            => new(a.file - b.file, a.rank - b.rank);

        public static bool operator ==(Vector2 a, Vector2 b)
            => a.file == b.file && a.rank == b.rank;

        public static bool operator !=(Vector2 a, Vector2 b)
            => !(a == b);

        public Vector2 DeepCopy()
            => new(file, rank);

        public static Vector2 FromString(string str) // assuming a string in the format of e4 (for example)
            => new(str[0] - 'a', str[1] - '0' - 1);

        public override string ToString()
            => (char)('a' + file) + (rank + 1).ToString();
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

        public static bool operator ==(Piece a, Piece b)
            => a.type == b.type && a.isWhite == b.isWhite;

        public static bool operator !=(Piece a, Piece b)
            => !(a == b);

        public Piece DeepCopy()
            => new(type, isWhite);
    }

    public class Position
    {
        Piece?[,] board = new Piece?[8, 8];
        bool whiteToMove;
        Vector2? enPassantSquare;
        int halfmoveClock;
        int fullmoves;

        public Position(Piece?[,] p_board, bool p_whiteToMove, Vector2? p_enPassantSquare, int p_halfmoveClock, int p_fullmoves)
        {
            board = p_board;
            whiteToMove = p_whiteToMove;
            enPassantSquare = p_enPassantSquare;
            halfmoveClock = p_halfmoveClock;
            fullmoves = p_fullmoves;
        }

        public static Position FromFEN(string fen)
        {
            var fields = fen.Split(' ');
            // 0th (1st) field = piece placement data
            Piece?[,] board = new Piece?[8, 8];
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
                        board[rank, file] = Piece.FromChar(ch);
                        file++;
                    }
                }
            }
            // 1st (2nd) field = active color
            bool whiteToMove = fields[1] == "w";
            // 2nd (3rd) field = castling availability

            // 3rd (4th) field = en passant target square
            Vector2? enPassantSquare = fields[3] == "-" ? null : Vector2.FromString(fields[3]);
            // 4th (5th) field = halfmove clock used for the fifty move rule
            int halfmoveClock = int.Parse(fields[4]);
            // 5th (6th) field = fullmove number
            int fullmoves = int.Parse(fields[5]);
            //
            return new(board, whiteToMove, enPassantSquare, halfmoveClock, fullmoves);
        }

        public Position DeepCopy()
        {
            Piece?[,] newBoard = new Piece?[8, 8];
            for (int rank = 0; rank < 8; rank++)
            {
                for (int file = 0; file < 8; file++)
                    newBoard[rank, file] = board[rank, file];
            }
            bool newWhiteToMove = whiteToMove;
            Vector2? newEnPassantSquare = enPassantSquare is null ? null : enPassantSquare.DeepCopy();
            int newHalfmoveClock = halfmoveClock;
            int newFullmoves = fullmoves;
            return new(newBoard, newWhiteToMove, newEnPassantSquare, newHalfmoveClock, newFullmoves);
        }

        public void MakeMove(Move move)
        {
            var piece = board[move.from.rank, move.from.file];
            if (piece is null)
                return; // just pack it up man
            board[move.from.rank, move.from.file] = null;
            board[move.to.rank, move.to.file] = piece.DeepCopy();
        }
    }

    public class Move
    {
        public Vector2 from, to;

        public Move(Vector2 p_from, Vector2 p_to)
        {
            from = p_from;
            to = p_to;
        }
    }
}
