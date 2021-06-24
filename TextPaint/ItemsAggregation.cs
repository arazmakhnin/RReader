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
        private bool _lookingForStartLine;

        public IReadOnlyCollection<DrawingItem> Items => _items;
        public bool EndOfPage { get; private set; }
        public int CurrentLineIndex { get; private set; }
        public float TextHeight { get; private set; }

        public ItemsAggregation(int lookForLine, float maxHeight)
        {
            _lookForLine = lookForLine;
            _maxHeight = maxHeight;
            _items = new List<DrawingItem>();
            _lookingForStartLine = true;
        }

        public bool TryAdd(DrawingItem item)
        {
            var wasAdded = false;
            if (!_lookingForStartLine || CurrentLineIndex >= _lookForLine)
            {
                _lookingForStartLine = false;

                if (TextHeight + item.GetHeight > _maxHeight)
                {
                    EndOfPage = true;
                    return false;
                }

                Debug.WriteLine(item switch{ DrawingText t => t.Text, _ => "=="});
                _items.Add(item);
                wasAdded = true;
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

            return wasAdded;
        }

        public void StartNewItem()
        {
            CurrentLineIndex = 0;
        }
    }
}
