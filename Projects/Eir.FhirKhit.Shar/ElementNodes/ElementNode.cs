using Eir.DevTools;
using Hl7.Fhir.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

#if FHIR_R4
namespace Eir.FhirKhit.R4
#elif FHIR_R3
namespace Eir.FhirKhit.R3
#endif
{
    [DebuggerDisplay("{this.ElementId}")]
    public class ElementNode
    {
        /// <summary>
        /// All names that this element is known by (i.e. value[x], valueInteger, etc)
        /// </summary>
        public List<String> Names { get; } = new List<string>();

        public String NodeName { get; }

        public String ElementId => this.Element == null ? this.NodeName : this.Element.ElementId;

        public ElementDefinition Element { get; set; }
        public List<ElementNode> Children { get; } = new List<ElementNode>();
        public List<ElementSlice> Slices { get; } = new List<ElementSlice>();

        public ElementNode(String pathName)
        {
            this.NodeName = pathName;
        }

        public ElementNode(ElementDefinition element)
        {
            this.NodeName = element.ElementId.LastPathPart().Split(':')[0];
            this.Element = element;
            this.Names.Add(element.ElementId.LastPathPart());
        }

        /// <summary>
        /// Drill down to search for child.
        /// This assumes that path is a period seperated list, and that path[0] is the name
        /// of the current node).
        /// </summary>
        /// <returns></returns>
        public bool TryGetNode(String path, out ElementNode child)
        {
            child = this;
            String[] pathParts = path.Split('.');
            if (pathParts[0] != this.ElementId)
                return false;
            return TryGetChild(pathParts.Skip(1), out child);
        }

        /// <summary>
        /// Drill down to search for child.
        /// This assumes that path is a period seperated list, and that path[0] is the name
        /// of a child node (not name of current node).
        /// </summary>
        /// <returns></returns>
        public bool TryGetChild(String path, out ElementNode child) => TryGetChild(path.Split('.'), out child);

        /// <summary>
        /// Drill down to search for child.
        /// This assumes that pathParts[0] is the name
        /// of a child node (not name of current node).
        /// </summary>
        /// <returns></returns>
        public bool TryGetChild(IEnumerable<String> pathParts, out ElementNode child)
        {
            child = null;
            ElementNode working = this;
            foreach (String pathPart in pathParts)
            {
                String[] nameParts = pathPart.Split(':');
                if (working.TryGetImmediateChild(nameParts[0], out working) == false)
                    return false;
                switch (nameParts.Length)
                {
                    case 1:
                        break;

                    case 2:
                        if (working.TryGetSlice(nameParts[1], out ElementSlice slice) == false)
                            return false;
                        working = slice.ElementNode;
                        break;

                    default:
                        throw new NotImplementedException($"Invalid path name {pathPart}");
                }
            }
            child = working;
            return true;
        }

        /// <summary>
        /// Try to get immediate child.
        /// </summary>
        public bool TryGetImmediateChild(String name, out ElementNode child)
        {
            child = null;
            foreach (ElementNode c in this.Children)
            {
                if (c.NodeName == name)
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
