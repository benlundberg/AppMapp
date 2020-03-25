using System;
using System.Collections.Generic;
using System.Text;

namespace MagicPattern.Core
{
    public class FieldInfo
    {
        public bool IsPrimaryKey { get; set; }
        public string MemberName { get; set; }
        public string JsonMemberName { get; set; }
        public ClassInfo ClassInfo { get; set; }
        public IList<object> Examples { get; set; }
    }
}
