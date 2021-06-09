using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
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

            return new FictionBook(Parse(doc.Root.Nodes()));
        }

        public static FictionBook LoadFile(string fileName)
        {
            var text = File.ReadAllText(fileName);
            return Load(text);
        }

        private static IReadOnlyCollection<BaseItem> Parse(IEnumerable<XNode> nodes)
        {
            var result = new List<BaseItem>();
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
                        result.Add(create(Parse(element.Nodes())));
                    }
                }
            }

            return result;
        }
    }
}
