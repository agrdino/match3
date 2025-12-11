namespace _Scripts
{
    public class Definition
    {
        public const int BOARD_WIDTH = 10;
        public const int BOARD_HEIGHT = 10;

        public const float MOVE_TIME_PER_UNIT = 0.05f;
    }

    public enum EGemType
    {
        None,
        Yellow,
        Green,
        Red,
        Blue,
        Orange,
    }

    public enum EGridPositionType
    {
        None,
        Gem,
    }
    
    public enum EGridPositionState
    {
        Free,
        Busy,
    }
}