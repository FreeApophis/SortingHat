using System.Collections.Generic;
using System.Text;

namespace SortingHat.API
{
    class IdentifyFileTypeFactory
    {
        public IdentifyFileType Create(string rule)
        {
            if (rule.StartsWith("basic"))
            {
                return new BasicFileTypeIdentifier(rule);
            }

            return null;
        }
    }
}
