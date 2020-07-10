using Eir.DevTools;
using Hl7.Fhir.Model;
using System;
using System.Collections.Generic;
using System.Text;

#if FHIR_R4
namespace Eir.FhirKhit.R4
#elif FHIR_R3
namespace Eir.FhirKhit.R3
#endif
{
    public class ElementNode
    {
        /// <summary>
        /// All names that this element is known by (i.e. value[x], valueInteger, etc)
        /// </summary>
        public List<String> Names { get; } = new List<string>();

        public String PathName { get; }

        public ElementDefinition Element { get; set; }
        public List<ElementNode> Children { get; } = new List<ElementNode>();
        public List<ElementSlice> Slices { get; } = new List<ElementSlice>();

        public ElementNode(String pathName)
        {
            this.PathName = pathName;
        }

        public ElementNode(ElementDefinition element)
        {
            this.PathName = element.ElementId.LastPathPart().Split(':')[0];
            this.Element = element;
            this.Names.Add(element.ElementId.LastPathPart());
        }

        public bool TryGetChild(String name, out ElementNode child)
        {
            child = null;
            foreach (ElementNode c in this.Children)
            {
                if (c.PathName == name)
                {
                    child = c;
                    return true;
                }
            }
            return false;
        }


        public bool TryGetSlice(String name, out ElementSlice slice)
        {
            slice = null;
            foreach (ElementSlice s in this.Slices)
            {
                if (s.SliceName == name)
                {
                    slice = s;
                    return true;
                }
            }
            return false;
        }
    }
}
