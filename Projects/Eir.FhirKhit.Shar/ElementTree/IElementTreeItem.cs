using Hl7.Fhir.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Linq;
using Eir.FhirKhit;

#if FHIR_R4
namespace Eir.FhirKhit.R4
#elif FHIR_R3
namespace Eir.FhirKhit.R3
#endif
{
    public interface IElementTreeItem
    {
        String Name { get; }
    }
}
