using System.Collections.Generic;
using System.Xml.Linq;

namespace Fb2.Specification
{
    public class FictionBook : BaseItem
    {
        public FictionBook(IReadOnlyCollection<BaseItem> items) : base(items)
        {
        }
    }
}
