using Osmium.Core;

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
    }
}
