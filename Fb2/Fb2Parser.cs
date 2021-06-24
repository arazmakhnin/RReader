using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml.Linq;
using Fb2.Specification;

namespace Fb2
{
    public static class Fb2Parser
    {
        public static FictionBook Load(string content)
        {
            var doc = XDocument.Parse(content);

            if (doc.Root?.Name.LocalName != "FictionBook")
            {
                throw new Fb2ParseException("Root element is not FictionBook");
            }

            var ignoredTags = new List<string>();
            var w = Stopwatch.StartNew();
            var bookElements = new List<BaseItem>();
            Parse(doc.Root.Nodes(), bookElements, ignoredTags);
            w.Stop();

            return new FictionBook(bookElements, new LoadInfo(ignoredTags, w.Elapsed));
        }

        public static FictionBook LoadFile(string fileName)
        {
            var text = File.ReadAllText(fileName);
            return Load(text);
        }

        private static void Parse(
            IEnumerable<XNode> nodes,
            ICollection<BaseItem> result,
            ICollection<string> ignoredTags)
        {
            foreach (var node in nodes)
            {
                if (node is XText text)
                {
                    result.Add(new Text(text.Value));
                    continue;
                }

                if (node is XElement element)
                {
                    if (ItemsMapper.Map.TryGetValue(element.Name.LocalName.ToLower(), out var create))
                    {
                        var openTag = create();
                        openTag.TagType = element.IsEmpty ? TagType.SelfClose : TagType.Open;
                        result.Add(openTag);

                        if (openTag.TagType != TagType.SelfClose)
                        {
                            Parse(element.Nodes(), result, ignoredTags);

                            var closeTag = create();
                            closeTag.TagType = TagType.Close;
                            result.Add(closeTag);
                        }
                    }
                    else
                    {
                        if (!ignoredTags.Contains(element.Name.LocalName))
                        {
                            ignoredTags.Add(element.Name.LocalName);
                        }
                    }
                }
            }
        }
    }
}
