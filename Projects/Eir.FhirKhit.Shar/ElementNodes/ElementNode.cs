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
        public List<String> Names { get; }  = new List<string>();

        public String Id => this.Element.ElementId;

        public ElementDefinition Element { get; set; }
        public List<ElementNode> Children { get; } = new List<ElementNode>();
        public List<ElementSlice> Slices { get; } = new List<ElementSlice>();

        public ElementNode(ElementDefinition element)
        {
            this.Element = element;
            this.Names.Add(element.ElementId.LastPathPart());
        }

        public bool TryGetChild(String name, out ElementNode child)
        {
            String fullName = "";
            if (this.Element.ElementId.Length > 0)
            {
                fullName += $"{this.Element.ElementId}.";
            }
            fullName += name;
            child = null;
            foreach (ElementNode c in this.Children)
            {
                if (c.Element.ElementId == fullName)
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
                if (s.Slice == name)
                {
                    slice = s;
                    return true;
                }
            }
            return false;
        }
    }
}
