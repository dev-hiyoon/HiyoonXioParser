using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HiyoonXioParser
{
    class XIOData
    {
        public String Id { get; set; }
        public String Name { get; set; }
        public String Value { get; set; }
        public int Length { get; set; }

        public XIOData(String id, String name, String value, int length)
        {
            this.Id = id;
            this.Name = name;
            this.Value = String.Format("{0}{1}{2}", "[", value, "]");
            this.Length = length;
        }
    }
}
