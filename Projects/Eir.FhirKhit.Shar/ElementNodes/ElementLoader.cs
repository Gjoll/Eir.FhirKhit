using Eir.DevTools;
using Hl7.Fhir.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if FHIR_R4
namespace Eir.FhirKhit.R4
#elif FHIR_R3
namespace Eir.FhirKhit.R3
#endif
{
    public class ElementLoader
    {
        IConversionInfo info;

        public ElementLoader(IConversionInfo info = null)
        {
            this.info = info;
        }

        void Info(string className, string method, string msg)
        {
            if (this.info != null)
                this.info.ConversionInfo(className, method, msg);
        }

        void Warn(string className, string method, string msg)
        {
            if (this.info != null)
                this.info.ConversionWarn(className, method, msg);
        }

        void Error(string className, string method, string msg)
        {
            if (this.info != null)
                this.info.ConversionError(className, method, msg);
        }

        public ElementNode Create(IEnumerable<ElementDefinition> items)
        {
            const String fcn = "Create";

            Int32 itemIndex = 0;
            ElementNode head = new ElementNode(new ElementDefinition
            {
                Path = "",
                ElementId = ""
            });
            if (items != null)
            {
                Load(0, head, items.ToArray(), ref itemIndex);
                if (itemIndex != items.Count())
                {
                    this.Error(this.GetType().Name, fcn, $"Loader error. Unconsumed elements leftover....");
                    return null;
                }
            }
            return head;
        }

        /// <summary>
        /// Wildly recursive. Be carefull!
        /// </summary>
        void Load(Int32 pathDepth,
            ElementNode head,
            ElementDefinition[] loadItems,
            ref Int32 itemIndex)
        {
            while (itemIndex < loadItems.Length)
            {
                ElementDefinition loadItem = loadItems[itemIndex];
                if (Load(pathDepth, head, loadItem) == false)
                    return;
                itemIndex += 1;
            }
        }

        bool SameBegin(Int32 depth,
            ElementPath basePath,
            ElementPath path)
        {
            if (basePath.Nodes.Count < depth)
                return false;
            if (path.Nodes.Count < depth)
                return false;
            for (Int32 i = 0; i < depth; i++)
            {
                ElementPath.Node baseNode = basePath.Nodes[i];
                ElementPath.Node pathNode = path.Nodes[i];
                bool Same()
                {
                    if (baseNode.Name != pathNode.Name)
                        return false;
                    if ((baseNode.Slice == null) && (pathNode.Slice == null))
                        return true;
                    if ((baseNode.Slice != null) || (pathNode.Slice != null))
                        return false;
                    if (baseNode.Slice != pathNode.Slice)
                        return false;
                    return true;
                }
                if (Same() == false)
                    return false;
            }

            return true;
        }

        bool Load(Int32 pathDepth,
                ElementNode head,
                ElementDefinition loadItem)
        {
            ElementPath p = new ElementPath(loadItem.ElementId);

            ElementNode newNode = null;
            while (pathDepth < p.Nodes.Count)
            {
                ElementPath.Node pathNode = p.Nodes[pathDepth];
                newNode = this.GetNode(head, pathNode);
                head = newNode;
                pathDepth += 1;
            }

            newNode.Element = loadItem;
            return true;
        }

        ElementNode GetNode(ElementNode head, ElementPath.Node pathNode)
        {
            if (head.TryGetChild(pathNode.Name, out ElementNode childNode) == false)
            {
                childNode = new ElementNode(pathNode.Name);
                head.Children.Add(childNode);
            }

            if (String.IsNullOrEmpty(pathNode.Slice) == false)
            {
                if (childNode.TryGetSlice(pathNode.Slice, out ElementSlice slice) == false)
                {
                    slice = new ElementSlice(pathNode.Slice, pathNode.Name);
                    childNode.Slices.Add(slice);
                }
                childNode = slice.ElementNode;
            }

            return childNode;
        }
    }
}
