using Fb2.Specification;

namespace TextPaint
{
    public readonly struct ReadingInfo
    {
        public int ItemIndex { get; }
        public int LineIndex { get; }

        public ReadingInfo(int itemIndex, int lineIndex)
        {
            ItemIndex = itemIndex;
            LineIndex = lineIndex;
        }
    }
}
