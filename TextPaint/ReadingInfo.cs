using Fb2.Specification;

namespace TextPaint
{
    public class ReadingInfo
    {
        public FictionBook Book { get; }

        public ReadingInfo(FictionBook book)
        {
            Book = book;
        }
    }
}
