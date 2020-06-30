using System;
using System.Collections.Generic;
using System.Text;

namespace Eir.FhirKhit
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
