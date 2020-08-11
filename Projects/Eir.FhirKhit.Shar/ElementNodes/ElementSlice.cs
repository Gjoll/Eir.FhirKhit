using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

#if FHIR_R4
namespace Eir.FhirKhit.R4
#elif FHIR_R3
namespace Eir.FhirKhit.R3
#endif
{
    [DebuggerDisplay("{this.ElementNode.NodeName}:{this.SliceName}")]
    public class ElementSlice
    {
        public String SliceName { get; }
        public ElementNode ElementNode { get; set; }

        public ElementSlice(String sliceName, String pathName)
        {
            this.SliceName = sliceName;
            this.ElementNode = new ElementNode(pathName);
        }
        public void Dump(StringBuilder sb, String margin = "")
        {
            void Line(String s) => sb.AppendLine($"{margin} {s}");

            Line($"Slice: '{this.SliceName}'");
            this.ElementNode.Dump(sb, $"{margin}  ");
        }
    }
}
