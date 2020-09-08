using System;
using System.Collections.Generic;
using System.Text;

#if FHIR_R4
namespace Eir.FhirKhit.R4
#elif FHIR_R3
namespace Eir.FhirKhit.R3
#endif
{
    public enum SaveType
    {
        Json,
        Xml
    }

    public static class SaveTypeExtensions
    {
        public static String ToFileExtension(this SaveType saveType)
        {
            switch (saveType)
            {
                case SaveType.Json: return ".json";
                case SaveType.Xml: return ".xml";
                default: throw new NotImplementedException();
            }
        }
    }
}
