using System;
using System.Collections.Generic;
using System.Text;

#if FHIR_R4
namespace Eir.FhirKhit.R4
#elif FHIR_R3
namespace Eir.FhirKhit.R3
#endif
{
    public class ElementPath
    {
        public class Node
        {
            public String Name { get; set; }
            public String Slice { get; set; }
        }

        public List<Node> Nodes { get; } = new List<Node>();

        public ElementPath()
        {
        }

        public ElementPath(String id)
        {
            if (String.IsNullOrEmpty(id) == false)
            {
                foreach (String part in id.Split('.'))
                {
                    String[] s = part.Split(':');
                    Node n = new Node();
                    n.Name = s[0];
                    if (s.Length > 1)
                        n.Slice = s[1];
                    this.Nodes.Add(n);
                }
            }
        }


        public override string ToString()
        {
            if (this.Nodes.Count == 0)
                return String.Empty;
            StringBuilder sb = new StringBuilder();

            void NodeStr(Node n)
            {
                sb.Append(n.Name);
                if (String.IsNullOrEmpty(n.Slice) == false)
                    sb.Append(n.Slice);
            }

            NodeStr(this.Nodes[0]);
            for (Int32 i = 1; i < this.Nodes.Count; i++)
            {
                sb.Append(".");
                NodeStr(this.Nodes[i]);
            }

            return sb.ToString();
        }
    }
}
