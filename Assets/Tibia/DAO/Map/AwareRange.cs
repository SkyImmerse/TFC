namespace Game.DAO
{
    public class AwareRange
    {
        public int Top;
        public int Right;
        public int Bottom;
        public int Left;

        public override string ToString()
        {
            return Left + "|" + Right + "|" + Top + "|" + Bottom;
        }


        public int Horizontal => Left + Right + 1;

        public int Vertical => Top + Bottom + 1;
    }
}