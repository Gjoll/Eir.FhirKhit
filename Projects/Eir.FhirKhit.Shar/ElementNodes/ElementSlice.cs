using System;
using System.Collections.Generic;
using System.Text;

#if FHIR_R4
namespace Eir.FhirKhit.R4
#elif FHIR_R3
namespace Eir.FhirKhit.R3
#endif
{
    public class ElementSlice
    {
        public String Slice => ElementNode.Element.SliceName();
        public ElementNode ElementNode { get; set; }
    }
}
