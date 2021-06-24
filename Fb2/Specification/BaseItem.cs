using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Fb2.Specification
{
    public class BaseItem
    {
        public TagType TagType { get; set; }
    }

    public class Title : BaseItem
    {
    }

    public class Section : BaseItem
    {
    }

    public class Paragraph : BaseItem
    {
    }

    public class Strong : BaseItem
    {
    }

    public class Emphasis : BaseItem
    {
    }

    public class EmptyLine : BaseItem
    {
    }

    public class Text : BaseItem
    {
        public string Value { get; }

        public Text(string value)
        {
            Value = value;
        }
    }

    public enum TagType
    {
        Open,
        Close,
        SelfClose
    }

    public static class ItemsMapper
    {
        public static Dictionary<string, Func<BaseItem>> Map = new()
        {
            ["body"] = () => new Body(),
            ["section"] = () => new Section(),
            ["title"] = () => new Title(),
            ["p"] = () => new Paragraph(),
            ["strong"] = () => new Strong(),
            ["emphasis"] = () => new Emphasis(),
            ["empty-line"] = () => new EmptyLine()
        };
    }
}
