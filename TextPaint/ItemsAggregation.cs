using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace TextPaint
{
    public class ItemsAggregation
    {
        private readonly int _lookForLine;
        private readonly float _maxHeight;
        private readonly List<DrawingItem> _items;

        public IReadOnlyCollection<DrawingItem> Items => _items;
        public bool EndOfPage { get; private set; }
        public int CurrentLineIndex { get; private set; }
        public float TextHeight { get; private set; }

        public ItemsAggregation(int lookForLine, float maxHeight)
        {
            _lookForLine = lookForLine;
            _maxHeight = maxHeight;
            _items = new List<DrawingItem>();
        }

        public void Add(DrawingItem item)
        {
            if (CurrentLineIndex >= _lookForLine)
            {
                //if (item is DrawingText t && t.Text == "")

                if (TextHeight + item.GetHeight > _maxHeight)
                {
                    EndOfPage = true;
                    return;
                }

                //Debug.WriteLine(item switch{ DrawingText t => t.Text, _ => "=="});
                _items.Add(item);
                if (item is LineBreak lineBreak)
                {
                    TextHeight += lineBreak.Paint.TextSize;
                }

                if (item is EmptyLine emptyLine)
                {
                    TextHeight += emptyLine.Size;
                }
            }

            if (item is LineBreak)
            {
                CurrentLineIndex++;
            }
        }
    }
}
