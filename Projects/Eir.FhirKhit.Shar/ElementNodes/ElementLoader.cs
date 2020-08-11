using Eir.DevTools;
using Hl7.Fhir.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
        Dictionary<String, ElementNode> nodes = new Dictionary<string, ElementNode>();

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

        public ElementNode Create(StructureDefinition sDef)
        {
            return this.Create(sDef.Snapshot.Element, sDef.Differential.Element);
        }


        public ElementNode Create(IEnumerable<ElementDefinition> snapShotItems,
            IEnumerable<ElementDefinition> differentialItems)
        {
            const String fcn = "Create";

            Int32 itemIndex = 0;
            ElementNode head = new ElementNode(new ElementDefinition
            {
                Path = "",
                ElementId = ""
            });
            Load(0, head, snapShotItems.ToArray(), ref itemIndex);
            ProcessTypeNodes(head);
            if (itemIndex != snapShotItems.Count())
            {
                this.Error(this.GetType().Name, fcn, $"Loader error. Unconsumed elements leftover....");
                return null;
            }

            if (differentialItems != null)
                LinkDifferentialItems(head, differentialItems);
            return head;
        }

        /// <summary>
        /// Moves appropriate nodes into ElementTypes
        /// </summary>
        /// <param name="head"></param>
        void ProcessTypeNodes(ElementNode head)
        {
            Int32 index = 0;
            while (index < head.Children.Count)
            {
                ElementNode n = head.Children[index];
                if (n.ElementId.EndsWith("[x]"))
                    ProcessTypeNodes(head, index);

                // Recursively call on children.
                ProcessTypeNodes(n);

                // Recursively call on all slices.
                foreach (ElementSlice slice in n.Slices)
                    ProcessTypeNodes(slice.ElementNode);

                index += 1;
            }
        }

        void ProcessTypeNodes(ElementNode head, Int32 index)
        {
            ElementNode baseNode = head.Children[index++];
            String baseName = baseNode.ElementId.Substring(0, baseNode.ElementId.Length - 3);

            while (index < head.Children.Count)
            {
                ElementNode n = head.Children[index];
                if (n.ElementId.StartsWith(baseName) == false)
                    return;

                String code = String.Empty;
                bool match = false;
                foreach (ElementDefinition.TypeRefComponent type in baseNode.Element.Type)
                {
                    if (n.ElementId.LastPathPart() == $"{baseName}{type.Code}")
                    {
                        code = type.Code;
                        match = true;
                        break;
                    }
                }

                if (match == false)
                    return;
                baseNode.ElementTypes.Add(code, n);
                head.Children.RemoveAt(index);
            }
        }

        /// <summary>
        /// Load ElementDefinition into hierarchical list.
        /// </summary>
        void Load(Int32 pathDepth,
            ElementNode head,
            ElementDefinition[] loadItems,
            ref Int32 itemIndex)
        {
            while (itemIndex < loadItems.Length)
            {
                ElementDefinition loadItem = loadItems[itemIndex];
                Load(pathDepth, head, loadItem);
                itemIndex += 1;
            }
        }

        void Load(Int32 pathDepth,
                ElementNode head,
                ElementDefinition loadItem)
        {
            ElementPath p = new ElementPath(loadItem.ElementId);

            ElementNode newNode = null;
            String elementId = head.ElementId;
            while (pathDepth < p.Nodes.Count)
            {
                ElementPath.Node pathNode = p.Nodes[pathDepth];
                newNode = this.GetNode(head, pathNode, ref elementId);
                head = newNode;
                pathDepth += 1;
            }

            Debug.Assert(newNode.ElementId == loadItem.ElementId);
            newNode.Element = loadItem;
            this.nodes.Add(newNode.ElementId, newNode);
            //foreach (KeyValuePair<String, ElementNode> type in newNode)
            //{

            //}
        }

        ElementNode GetNode(ElementNode head,
            ElementPath.Node pathNode,
            ref String elementId)
        {
            if (String.IsNullOrEmpty(elementId) == false)
                elementId += ".";
            elementId += $"{pathNode.Name}";
            if (head.TryGetImmediateChild(pathNode.Name, out ElementNode childNode) == false)
            {
                childNode = new ElementNode(elementId);
                head.Children.Add(childNode);
            }

            if (String.IsNullOrEmpty(pathNode.Slice) == false)
            {
                elementId += $":{pathNode.Slice}";
                if (childNode.TryGetSlice(pathNode.Slice, out ElementSlice slice) == false)
                {
                    slice = new ElementSlice(pathNode.Slice, elementId);
                    childNode.Slices.Add(slice);
                }
                childNode = slice.ElementNode;
            }

            Debug.Assert(childNode.ElementId == elementId);
            return childNode;
        }

        void LinkDifferentialItems(ElementNode head,
            IEnumerable<ElementDefinition> differentialItems)
        {
            foreach (ElementDefinition differentialItem in differentialItems)
                LinkDifferentialItem(head, differentialItem);
        }

        void LinkDifferentialItem(ElementNode head,
            ElementDefinition differentialItem)
        {
            if (this.nodes.TryGetValue(differentialItem.ElementId, out ElementNode snapshotNode) == true)
            {
                if (snapshotNode.DiffElement != null)
                    throw new Exception("Differential item {differentialItem.ElementId} already linked");
                snapshotNode.DiffElement = differentialItem;
                return;
            }

            if (head.TryGetAlias(differentialItem.ElementId, out ElementNode child) == true)
            {
                if (snapshotNode.DiffElement != null)
                    throw new Exception("Differential item {differentialItem.ElementId} already linked");
                snapshotNode.DiffElement = differentialItem;
                return;
            }

            StringBuilder sb = new StringBuilder();
            foreach (ElementNode node in this.nodes.Values)
                node.Dump(sb);
            File.WriteAllText(@"c:\Temp\scr.txt", sb.ToString());
            throw new Exception($"Can not find snapshot node matching differential {differentialItem.ElementId}");
        }
    }
}
