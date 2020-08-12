using System;
using System.Collections.Generic;
using System.Text;

#if FHIR_R4
namespace Eir.FhirKhit.R4
#elif FHIR_R3
namespace Eir.FhirKhit.R3
#endif
{
    public class Modified<T>
        where T : class
    {
        public bool ModifiedFlag { get; set; }
        public T Item { get; set; }

        public Modified(T item)
        {
            this.Item = item;
        }
    }
}
