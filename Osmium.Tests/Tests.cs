using Osmium.Core;
using System.Runtime.Intrinsics;

namespace Osmium.Tests
{
    public class CoreTests
    {
        [Fact]
        public void Vector2FromString_SampleSquare()
        {
            var u = Vector2.FromString("e4");
            var v = new Vector2(4, 3);
            Assert.True(u == v);
        }

        [Fact]
        public void Vector2FromString_AllSquares()
        {
            for (int rank = 1; rank <= 8; rank++)
            {
                for (char file = 'a'; file <= 'h'; file++)
                {
                    string squareName = file.ToString() + rank.ToString();
                    var u = Vector2.FromString(squareName);
                    var v = new Vector2(file - 'a', rank - 1);
                    Assert.True(u == v);
                }
            }
        }

        [Fact]
        public void Vector2ToString_AllSquares()
        {
            for (int rank = 0; rank < 8; rank++)
            {
                for (int file = 0; file <= 8; file++)
                {
                    string squareName = (char)('a' + file) + (rank + 1).ToString();
                    var v = new Vector2(file, rank);
                    Assert.Equal(squareName, v.ToString());
                }
            }
        }

        [Fact]
        public void PieceFromChar_AllPieces()
        {
            char[] chars = ['p', 'b', 'n', 'r', 'q', 'k'];
            Piece.Type[] pieceTypes = [Piece.Type.Pawn, Piece.Type.Bishop, Piece.Type.Knight, Piece.Type.Rook, Piece.Type.Queen, Piece.Type.King];
            // white pieces
            for (int i = 0; i < 6; i++)
            {
                var p = new Piece(pieceTypes[i], true);
                var q = Piece.FromChar(char.ToUpper(chars[i]));
                Assert.True(p == q);
            }
            // black pieces
            for (int i = 0; i < 6; i++)
            {
                var p = new Piece(pieceTypes[i], false);
                var q = Piece.FromChar(chars[i]);
                Assert.True(p == q);
            }
        }

        [Fact]
        public void CastlingRightsFromString_AllOptions()
        {
            string[] options = ["-", "K", "Q", "KQ", "k", "Kk", "Qk", "KQk", "q", "Kq", "Qq", "KQq", "kq", "Kkq", "Qkq", "KQkq"];
            for (int i = 0; i < options.Length; i++)
            {
                var c = Position.CastlingRightsFromString(options[i]);
                Assert.Equal(i, (int)c);
            }
        }

        [Fact]
        public void StartingPositionToFen()
        {
            Assert.Equal("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1", Position.startingPosition.ToFEN());
        }

        [Fact]
        public void Raycast_RookSample()
        {
            var position = Position.FromFEN("k7/8/8/8/8/8/8/R7 w - - 0 1");
            Vector2 attacker = new(0, 0);
            Assert.Equal("R", position.GetPiece(0, 0).ToString());
            //
            Assert.Equal("k", position.Raycast(attacker, Vector2.up).ToString());
            Assert.True(position.Raycast(attacker, Vector2.up) == new Piece(Piece.Type.King, false));
            // de facto two ways of expressing the same thing
        }

        [Fact]
        public void Raycast_BishopSample()
        {
            var position = Position.FromFEN("7k/8/8/8/8/8/8/B7 w - - 0 1");
            Vector2 attacker = new(0, 0);
            Assert.Equal("B", position.GetPiece(0, 0).ToString());
            //
            Assert.Equal("k", position.Raycast(attacker, Vector2.one).ToString());
            Assert.True(position.Raycast(attacker, Vector2.one) == new Piece(Piece.Type.King, false));
            // de facto two ways of expressing the same thing
        }

        [Fact]
        public void Raycast_WallHit()
        {
            var position = Position.FromFEN("8/8/8/8/8/8/8/8 w - - 0 1");
            Vector2 origin = new(1, 1);
            foreach (var direction in Vector2.allDirections)
                Assert.True(position.Raycast(origin, direction) is null);
        }

        [Fact]
        public void IsPieceCheckingKing_Samples()
        {
            var position = Position.FromFEN("r1bqkb1r/pppQ1ppp/5n2/8/4P3/8/PPP2PPP/RNB1KB1R b KQkq - 0 6");
            Vector2 attacker = new(3, 6);
            Vector2 king = new(4, 7);
            Assert.True(position.IsPieceCheckingKing(attacker, king));
            //
            position = Position.FromFEN("rnbqkbnr/pppp1p1p/6p1/4Q3/4P3/8/PPPP1PPP/RNB1KBNR b KQkq - 0 3");
            attacker = new(4, 4);
            king = new(4, 7);
            Assert.True(position.IsPieceCheckingKing(attacker, king));
            //
            position = Position.FromFEN("rnbqkbnr/ppNp2pp/8/4pp2/8/8/PPPPPPPP/R1BQKBNR b KQkq - 0 3");
            attacker = new(2, 6);
            king = new(4, 7);
            Assert.True(position.IsPieceCheckingKing(attacker, king));
            //
            position = Position.FromFEN("8/8/3k1p2/4p1pp/5K2/P7/1PP5/8 w - - 0 1");
            attacker = new(6, 4);
            king = new(5, 3);
            Assert.True(position.IsPieceCheckingKing(attacker, king));
            attacker = new(4, 4);
            Assert.True(position.IsPieceCheckingKing(attacker, king));
            //
            position = Position.FromFEN("8/8/6k1/7P/7K/8/8/8 w - - 0 1");
            attacker = new(7, 4);
            king = new(6, 5);
            Assert.True(position.IsPieceCheckingKing(attacker, king));
            //
            position = Position.FromFEN("r1bqkbnr/ppppp1pp/2n2p2/7Q/4P3/8/PPPP1PPP/RNB1KBNR b KQkq - 2 2");
            attacker = new(7, 4);
            king = new(4, 7);
            Assert.True(position.IsPieceCheckingKing(attacker, king));
        }

        [Fact]
        public void IsKingInCheck_Samples()
        {
            var position = Position.FromFEN("r1bqkbnr/ppppp1pp/2n2p2/7Q/4P3/8/PPPP1PPP/RNB1KBNR w KQkq - 3 3");
            Assert.True(position.IsKingInCheck(false));
            //
            position = Position.FromFEN("1rbqkbnr/pppppppp/2n5/7Q/4P3/8/PPPP1PPP/RNB1KBNR w KQk - 3 3");
            Assert.False(position.IsKingInCheck(false));
        }

        [Fact]
        public void GetAllLegalMoves_StartingPositionMoveCount()
        {
            Assert.Equal(20, Position.startingPosition.GetAllLegalMoves().Count);
        }

        [Fact]
        public void GetAllLegalMoves_AccountsForCheck()
        {
            var position = Position.FromFEN("r1bqkbnr/pppppppp/2n5/7Q/4P3/8/PPPP1PPP/RNB1KBNR b KQkq - 2 2");
            Assert.Equal(19, position.GetAllLegalMoves().Count);
        }
    }
}
