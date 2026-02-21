using System.ComponentModel.Design;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Runtime.Intrinsics;

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
        public static readonly Vector2 one = new(1, 1);

        public static readonly Vector2[] orthogonalDirections = [up, right, down, left];
        public static readonly Vector2[] diagonalDirections = [one, right + down, -one, left + up];
        public static readonly Vector2[] allDirections = [up, one, right, right + down, down, -one, left, left + up];
        public static readonly Vector2[] hippogonalDirections = [new(1, 2), new(2, 1), new(2, -1), new(1, -2), new(-1, -2), new(-2, -1), new(-2, 1), new(-1, 2)];

        public static Vector2 operator -(Vector2 v)
            => new(-v.file, -v.rank);

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

        public bool IsInBounds()
            => rank >= 0 && rank < 8 && file >= 0 && file < 8;
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
                _ => throw new Exception()
            };
        }

        public static bool operator ==(Piece a, Piece b)
            => a.type == b.type && a.isWhite == b.isWhite;

        public static bool operator !=(Piece a, Piece b)
            => !(a == b);

        public char ToChar()
        {
            char ch = type switch
            {
                Type.Pawn => 'p',
                Type.Bishop => 'b',
                Type.Knight => 'n',
                Type.Rook => 'r',
                Type.Queen => 'q',
                Type.King => 'k',
                _ => throw new Exception()
            };
            return isWhite ? char.ToUpper(ch) : ch;
        }

        public override string ToString()
            => ToChar().ToString();

        public Piece DeepCopy()
            => new(type, isWhite);
    }

    public class Position
    {
        public Piece?[,] board = new Piece?[8, 8];
        bool whiteToMove;
        CastlingAvailability castlingAvailability;
        Vector2? enPassantSquare;
        int halfmoveClock;
        int fullmoves;

        [Flags]
        public enum CastlingAvailability : byte
        {
            None = 0,
            WhiteKingside = 1,
            WhiteQueenside = 2,
            BlackKingside = 4,
            BlackQueenside = 8
        }

        public static CastlingAvailability CastlingRightsFromString(string str)
        {
            var output = CastlingAvailability.None;
            if (str == "-")
                return output;
            foreach (var ch in str.ToCharArray())
            {
                output |= ch switch
                {
                    'K' => CastlingAvailability.WhiteKingside,
                    'Q' => CastlingAvailability.WhiteQueenside,
                    'k' => CastlingAvailability.BlackKingside,
                    'q' => CastlingAvailability.BlackQueenside,
                    _ => throw new Exception(),
                };
            }
            return output;
        }

        public static string CastlingRightsToString(CastlingAvailability castlingAvailability)
        {
            string[] options = ["-", "K", "Q", "KQ", "k", "Kk", "Qk", "KQk", "q", "Kq", "Qq", "KQq", "kq", "Kkq", "Qkq", "KQkq"];
            return options[(int)castlingAvailability];
        }

        public Position(Piece?[,] p_board, bool p_whiteToMove, CastlingAvailability p_castlingAvailability, Vector2? p_enPassantSquare, int p_halfmoveClock, int p_fullmoves)
        {
            board = p_board;
            whiteToMove = p_whiteToMove;
            castlingAvailability = p_castlingAvailability;
            enPassantSquare = p_enPassantSquare;
            halfmoveClock = p_halfmoveClock;
            fullmoves = p_fullmoves;
        }

        public static readonly Position startingPosition = FromFEN("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");

        public static Position FromFEN(string fen)
        {
            var fields = fen.Split(' ');
            // 0th (1st) field = piece placement data
            Piece?[,] board = new Piece?[8, 8];
            var ranks = fields[0].Split('/');
            for (int rank = 7; rank >= 0; rank--)
            {
                int file = 0;
                foreach (var ch in ranks[7 - rank].ToCharArray())
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
            var castlingAvailability = CastlingRightsFromString(fields[2]);
            // 3rd (4th) field = en passant target square
            Vector2? enPassantSquare = fields[3] == "-" ? null : Vector2.FromString(fields[3]);
            // 4th (5th) field = halfmove clock used for the fifty move rule
            int halfmoveClock = int.Parse(fields[4]);
            // 5th (6th) field = fullmove number
            int fullmoves = int.Parse(fields[5]);
            //
            return new(board, whiteToMove, castlingAvailability, enPassantSquare, halfmoveClock, fullmoves);
        }

        public Position DeepCopy()
        {
            Piece?[,] newBoard = new Piece?[8, 8];
            for (int rank = 0; rank < 8; rank++)
            {
                for (int file = 0; file < 8; file++)
                    newBoard[rank, file] = GetPiece(rank, file)?.DeepCopy();
            }
            bool newWhiteToMove = whiteToMove;
            var newCastlingAvailability = castlingAvailability;
            Vector2? newEnPassantSquare = enPassantSquare is null ? null : enPassantSquare.DeepCopy();
            int newHalfmoveClock = halfmoveClock;
            int newFullmoves = fullmoves;
            return new(newBoard, newWhiteToMove, newCastlingAvailability, newEnPassantSquare, newHalfmoveClock, newFullmoves);
        }

        public override string ToString()
            => ToFEN();

        public string ToFEN()
        {
            string output = "";
            // 0th (1st) field = piece placement data
            for (int rank = 7; rank >= 0; rank--)
            {
                int consecutiveEmptySquares = 0;
                for (int file = 0; file < 8; file++)
                {
                    if (GetPiece(rank, file) is null)
                        consecutiveEmptySquares++;
                    else
                    {
                        if (consecutiveEmptySquares != 0)
                            output += consecutiveEmptySquares.ToString();
                        output += GetPiece(rank, file)?.ToString();
                    }
                }
                output += consecutiveEmptySquares == 0 ? "" : consecutiveEmptySquares.ToString();
                output += rank == 0 ? " " : "/";
            }
            // 1st (2nd) field = active color
            output += (whiteToMove ? "w" : "b") + " ";
            // 2nd (3rd) field = castling availability
            output += CastlingRightsToString(castlingAvailability) + " ";
            // 3rd (4th) field = en passant target square
            output += (enPassantSquare is null ? "-" : enPassantSquare.ToString()) + " ";
            // 4th (5th) field = halfmove clock used for the fifty move rule
            output += halfmoveClock.ToString() + " ";
            // 5th (6th) field = fullmove number
            output += fullmoves.ToString();
            //
            return output;
        }

        public Piece? GetPiece(int rank, int file)
            => board[rank, file];

        public Piece? GetPiece(Vector2 v)
            => GetPiece(v.rank, v.file);

        public void SetPiece(int rank, int file, Piece? piece)
            => board[rank, file] = piece is null ? null : piece.DeepCopy();

        public void SetPiece(Vector2 v, Piece? piece)
            => SetPiece(v.rank, v.file, piece);

        public void MakeMove(Move move)
        {
            var piece = GetPiece(move.from);
            if (piece is null)
                return; // just pack it up man
            SetPiece(move.from, null);
            SetPiece(move.to, piece);
        }

        // move legality and move generation:

        public Piece? Raycast(Vector2 origin, Vector2 direction)
        {
            Vector2 pos = origin.DeepCopy();
            while (true)
            {
                pos += direction;
                if (!pos.IsInBounds())
                    return null;
                var piece = GetPiece(pos);
                if (piece is not null)
                    return piece.DeepCopy();
            }
        }

        public Vector2? RaycastForHitpoint(Vector2 origin, Vector2 direction)
        {
            Vector2 pos = origin.DeepCopy();
            while (true)
            {
                pos += direction;
                if (!pos.IsInBounds())
                    return null;
                var piece = GetPiece(pos);
                if (piece is not null)
                    return pos;
            }
        }

        public bool IsPieceCheckingKing(Vector2 attacker, Vector2 king)
        {
            // we assume that the provided king location is correct
            var piece = GetPiece(attacker.rank, attacker.file);
            if (piece is null)
                return false;
            return piece.type switch
            {
                // son 😭😭😭 im crine 😭😭😭 this is NOT good code 😭😭😭
                Piece.Type.Pawn => IsPawnCheckingKing(attacker, piece.isWhite, king),
                Piece.Type.Bishop => IsBishopCheckingKing(attacker, king),
                Piece.Type.Knight => IsKnightCheckingKing(attacker, king),
                Piece.Type.Rook => IsRookCheckingKing(attacker, king),
                Piece.Type.Queen => IsRookCheckingKing(attacker, king) || IsBishopCheckingKing(attacker, king),
                Piece.Type.King => IsKingCheckingKing(attacker, king),
                _ => throw new Exception()
            };
        }

        bool IsPawnCheckingKing(Vector2 pawn, bool pawnColor, Vector2 king)
        {
            Vector2 displacement = king - pawn;
            Vector2 forward = pawnColor ? Vector2.up : Vector2.down;
            return displacement == forward + Vector2.left || displacement == forward + Vector2.right;
        }

        bool IsBishopCheckingKing(Vector2 bishop, Vector2 king)
        {
            // again, we assume that the provided king location is correct
            Vector2 displacement = king - bishop;
            if (Math.Abs(displacement.rank) != Math.Abs(displacement.file))
                return false;
            Vector2 direction = new(displacement.file > 0 ? 1 : -1, displacement.rank > 0 ? 1 : -1);
            return RaycastForHitpoint(bishop, direction) == king;                
        }

        bool IsKnightCheckingKing(Vector2 knight, Vector2 king)
        {
            // again, we assume that the provided king location is correct
            Vector2 displacement = king - knight;
            if (Math.Abs(displacement.rank) == 2 && Math.Abs(displacement.file) == 1)
                return true;
            else if (Math.Abs(displacement.rank) == 1 && Math.Abs(displacement.file) == 2)
                return true;
            else return false;
        }

        bool IsRookCheckingKing(Vector2 rook, Vector2 king)
        {
            // again, we assume that the provided king location is correct
            if (rook.rank == king.rank)
                return RaycastForHitpoint(rook, (rook.file > king.file) ? Vector2.left : Vector2.right) == king;
            else if (rook.file == king.file)
                return RaycastForHitpoint(rook, (rook.rank > king.rank) ? Vector2.down : Vector2.up) == king;
            else return false;
        }

        bool IsKingCheckingKing(Vector2 attacker, Vector2 king)
        {
            // son 😭😭😭
            Vector2 displacement = king - attacker;
            return Math.Abs(displacement.rank) <= 1 && Math.Abs(displacement.file) <= 1;
        }

        public bool IsKingInCheck(bool kingColor)
        {
            // find king
            var king = FindPiece(new(Piece.Type.King, kingColor));
            if (king is null)
                throw new Exception();
            //
            for (int rank = 0; rank < 8; rank++)
            {
                for (int file = 0; file < 8; file++)
                {
                    Piece? piece = GetPiece(rank, file);
                    if (piece is null || piece.isWhite == kingColor)
                        continue; // nothing worth examining rn on this square
                    if (IsPieceCheckingKing(new(file, rank), king))
                        return true;
                }
            }
            return false;
        }

        Vector2? FindPiece(Piece target)
        {
            Vector2 result;
            int startRank = target.isWhite ? 0 : 7;
            int rankIncrement = target.isWhite ? 1 : -1;
            for (int rank = startRank; rank >= 0 && rank < 8; rank += rankIncrement)
            {
                for (int file = 0; file < 8; file++)
                {
                    Piece? piece = GetPiece(rank, file);
                    if (piece is null || piece != target)
                        continue;
                    result = new(file, rank);
                    return result;
                }
            }
            return null;
        }

        List<Move> GetMovesAlongRay(Vector2 origin, Vector2 direction, bool attackerColor)
        {
            List<Move> result = [];
            Vector2 pos = origin.DeepCopy();
            while (true)
            {
                pos += direction;
                if (!pos.IsInBounds())
                    return result;
                var piece = GetPiece(pos);
                if (piece is null || piece.isWhite != attackerColor)
                    result.Add(new(origin.DeepCopy(), pos.DeepCopy()));
                if (piece is not null)
                    return result;
            }
        }

        public List<Move> GetAllLegalMoves()
        {
            List<Move> result = [];
            // get all moves not acknowledging checks
            for (int rank = 0; rank < 8; rank++)
            {
                for (int file = 0; file < 8; file++)
                {
                    var piece = GetPiece(rank, file);
                    if (piece is null || piece.isWhite != whiteToMove)
                        continue;
                    result.AddRange(piece.type switch
                    {
                        Piece.Type.Pawn => GetPawnMoves(new(file, rank), whiteToMove),
                        Piece.Type.Bishop => GetRiderMoves(new(file, rank), whiteToMove, Vector2.diagonalDirections),
                        Piece.Type.Knight => GetLeaperMoves(new(file, rank), whiteToMove, Vector2.hippogonalDirections),
                        Piece.Type.Rook => GetRiderMoves(new(file, rank), whiteToMove, Vector2.orthogonalDirections),
                        Piece.Type.Queen => GetRiderMoves(new(file, rank), whiteToMove, Vector2.allDirections),
                        Piece.Type.King => GetLeaperMoves(new(file, rank), whiteToMove, Vector2.allDirections),
                        _ => throw new Exception()
                    });
                }
            }
            // filter out every move that'd leave the king in check
            for (int i = result.Count - 1; i >= 0; i--)
            {
                var move = result[i].DeepCopy();
                var positionAfterMove = this.DeepCopy();
                positionAfterMove.MakeMove(move);
                if (positionAfterMove.IsKingInCheck(whiteToMove))
                    result.RemoveAt(i);
            }
            //
            return result;
        }

        List<Move> GetPawnMoves(Vector2 pawn, bool pawnColor)
        {
            List<Move> result = [];
            Vector2 forward = pawnColor ? Vector2.up : Vector2.down;
            // push 1 square forward
            if (GetPiece(pawn + forward) is null)
            {
                result.Add(new(pawn, pawn + forward));
                // push 2 squares forward
                if (pawn.rank == (pawnColor ? 1 : 6) && GetPiece(pawn + forward + forward) is null)
                    result.Add(new(pawn, pawn + forward + forward));
            }
            // captures
            if ((pawn + forward + Vector2.left).IsInBounds())
            {
                var leftCapturePiece = GetPiece(pawn + forward + Vector2.left);
                if ((leftCapturePiece is not null && leftCapturePiece.isWhite != pawnColor) || (enPassantSquare is not null && enPassantSquare == pawn + forward + Vector2.left))
                    result.Add(new(pawn, pawn + forward + Vector2.left));
            }
            if ((pawn + forward + Vector2.right).IsInBounds())
            {
                var rightCapturePiece = GetPiece(pawn + forward + Vector2.right);
                if ((rightCapturePiece is not null && rightCapturePiece.isWhite != pawnColor) || (enPassantSquare is not null && enPassantSquare == pawn + forward + Vector2.right))
                    result.Add(new(pawn, pawn + forward + Vector2.left));
            }
            //
            return result;
        }

        List<Move> GetRiderMoves(Vector2 rider, bool riderColor, Vector2[] directions) // generalized method for rooks, bishops and queens
        {
            List<Move> result = [];
            foreach (var direction in directions)
                result.AddRange(GetMovesAlongRay(rider, direction, riderColor));
            return result;
        }

        List<Move> GetLeaperMoves(Vector2 leaper, bool leaperColor, Vector2[] directions) // generalized method for knights and kings
        {
            List<Move> result = [];
            foreach (var direction in directions)
            {
                if (!(leaper + direction).IsInBounds())
                    continue;
                var piece = GetPiece(leaper + direction);
                if (piece is null || piece.isWhite != leaperColor)
                    result.Add(new(leaper, leaper + direction));
            }
            return result;
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

        public Move DeepCopy()
            => new(from.DeepCopy(), to.DeepCopy());
    }
}
