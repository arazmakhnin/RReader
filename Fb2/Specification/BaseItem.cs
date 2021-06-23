using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Fb2.Specification
{
    public class BaseItem
    {
        public IReadOnlyCollection<BaseItem> Items { get; }

        public BaseItem(IReadOnlyCollection<BaseItem> items)
        {
            Items = items;
        }
    }

    public class Title : BaseItem
    {
        public Title(IReadOnlyCollection<BaseItem> items) : base(items) { }
    }

    public class Section : BaseItem
    {
        public Section(IReadOnlyCollection<BaseItem> items) : base(items) { }
    }

    public class Paragraph : BaseItem
    {
        public Paragraph(IReadOnlyCollection<BaseItem> items) : base(items) { }
    }

    public class Strong : BaseItem
    {
        public Strong(IReadOnlyCollection<BaseItem> items) : base(items) { }
    }

    public class EmptyLine : BaseItem
    {
        public EmptyLine() : base(Array.Empty<BaseItem>()) { }
    }

    public class Text : BaseItem
    {
        public string Value { get; }

        public Text(string value) : base(Array.Empty<BaseItem>())
        {
            Value = value;
        }
    }

    public static class ItemsMapper
    {
        public static Dictionary<string, Func<IReadOnlyCollection<BaseItem>, BaseItem>> Map = new()
        {
            ["body"] = e => new Body(e),
            ["section"] = e => new Section(e),
            ["title"] = e => new Title(e),
            ["p"] = e => new Paragraph(e),
            ["strong"] = e => new Strong(e),
            ["empty-line"] = e => new EmptyLine()
        };
    }
}
