﻿using Hl7.Fhir.Model;
using System;
using System.Collections.Generic;
using System.Text;

#if FHIR_R4
namespace Eir.FhirKhit.R4
#elif FHIR_R3
namespace Eir.FhirKhit.R3
#endif
{
#if FHIR_R4
    public static class FhirKhitVersion
    {
        public const String FhirVersion = "4.0.0";
        public const FHIRVersion FhirVersionEnum = FHIRVersion.N4_0_1;
    }
#elif FHIR_R3
    public static class FhirKhitVersion
    {
        public const String FhirVersion = "3.0.1";
    }
#endif
}
