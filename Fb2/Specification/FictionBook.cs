using System;
using System.Collections.Generic;
using System.Linq;

namespace Fb2.Specification
{
    public class FictionBook : BaseItem
    {
        public LoadInfo LoadInfo { get; }

        public FictionBook(IReadOnlyCollection<BaseItem> items, LoadInfo loadInfo) : base(items)
        {
            LoadInfo = loadInfo;
        }
    }

    public class LoadInfo
    {
        public IReadOnlyCollection<string> IgnoredTags { get; }
        public TimeSpan LoadTime { get; }

        public LoadInfo(IReadOnlyCollection<string> ignoredTags, TimeSpan loadTime)
        {
            IgnoredTags = ignoredTags;
            LoadTime = loadTime;
        }

        public override string ToString()
        {
            var lines = new[]
            {
                "Time: " + LoadTime,
                IgnoredTags.Any() ? "Ignored tags:" : "No ignored tags"
            }.Concat(IgnoredTags.Select(t => $"  {t}"));

            return string.Join(Environment.NewLine, lines);
        }
    }
}
