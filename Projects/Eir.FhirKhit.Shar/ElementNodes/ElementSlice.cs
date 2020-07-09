﻿using System;
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
        public String SliceName { get; }
        public ElementNode ElementNode { get; set; }

        public ElementSlice(String sliceName, String elementName)
        {
            this.SliceName = sliceName;
            this.ElementNode = new ElementNode(elementName);
        }
    }
}
