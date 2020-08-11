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
            LoadSnapshotNodes(0, head, snapShotItems.ToArray(), ref itemIndex);
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
        /// Load snapshot nodes into hierarchical list.
        /// </summary>
        void LoadSnapshotNodes(Int32 pathDepth,
            ElementNode head,
            ElementDefinition[] loadItems,
            ref Int32 itemIndex)
        {
            while (itemIndex < loadItems.Length)
            {
                ElementDefinition loadItem = loadItems[itemIndex];
                SnapshotNode(pathDepth, head, loadItem);
                itemIndex += 1;
            }
        }

        void SnapshotNode(Int32 pathDepth,
                ElementNode head,
                ElementDefinition loadItem)
        {
            /*
             * Type nodes (valueReferrence) may have been created already with empty Element. If so, put
             * element into existing type node if same elementId.
             */
            if (this.nodes.TryGetValue(loadItem.ElementId, out var existingNode) == true)
            {
                if (existingNode.Element != null)
                    throw new Exception($"Node {loadItem.ElementId} already populated");
                existingNode.Element = loadItem;
                return;
            }

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

            void AddTypeNodes()
            {
                String elementName = loadItem.ElementId;
                if (elementName.EndsWith("[x]") == false)
                    return;
                String baseName = elementName.Substring(0, elementName.Length - 3);
                foreach (ElementDefinition.TypeRefComponent elementType in loadItem.Type)
                {
                    String typeName = $"{baseName}{elementType.Code}";
                    ElementNode typeNode = new ElementNode(typeName);
                    newNode.ElementTypes.Add(elementType.Code, typeNode);
                    this.nodes.Add(typeNode.ElementId, typeNode);
                }

            }

            AddTypeNodes();
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

            StringBuilder sb = new StringBuilder();
            foreach (ElementNode node in this.nodes.Values)
                node.Dump(sb);
            File.WriteAllText(@"c:\Temp\scr.txt", sb.ToString());
            throw new Exception($"Can not find snapshot node matching differential {differentialItem.ElementId}");
        }
    }
}
