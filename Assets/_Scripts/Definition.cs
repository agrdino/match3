namespace _Scripts
{
    public class Definition
    {
        public const int BOARD_WIDTH = 10;
        public const int BOARD_HEIGHT = 10;

        public const float MOVE_TIME_PER_UNIT = 0.1f;
        public const float MIN_SWIPE_DISTENCE = 1f;

        public const float DELAY_TO_SHUFFLE = 1;
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
    
    public enum EPositionState
    {
        Free,
        Busy,
    }

    public enum ESwipeDirection
    {
        Cancel,
        Left,
        Right,
        Up,
        Down
    }
    
    public enum EBoardState
    {
        Creating,
        Free,
        PreShuffle,
        Shuffling,
    }
}