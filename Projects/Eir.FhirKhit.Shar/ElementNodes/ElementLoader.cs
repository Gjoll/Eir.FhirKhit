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

            itemIndex = 0;
            if (differentialItems != null)
                LoadDifferentialNodes(0, head, differentialItems.ToArray(), ref itemIndex);
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

            newNode.Element = loadItem;
        }






        /// <summary>
        /// Load differential nodes.
        /// </summary>
        void LoadDifferentialNodes(Int32 pathDepth,
            ElementNode head,
            ElementDefinition[] loadItems,
            ref Int32 itemIndex)
        {
            while (itemIndex < loadItems.Length)
            {
                ElementDefinition loadItem = loadItems[itemIndex];
                DifferentialNode(pathDepth, head, loadItem);
                itemIndex += 1;
            }
        }

        void DifferentialNode(Int32 pathDepth,
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

            newNode.DiffElement = loadItem;
        }

        ElementNode GetNode(ElementNode head,
            ElementPath.Node pathNode,
            ref String elementId)
        {
            if (String.IsNullOrEmpty(elementId) == false)
                elementId += ".";
            elementId += $"{pathNode.Name}";
            if (head.TryGetImmediateType(pathNode.Name, out String typeCode, out ElementNode childNode) == false)
            {
                childNode = new ElementNode(elementId);
                head.Children.Add(childNode);
            }
            else
            {
                if (String.IsNullOrEmpty(typeCode) == false)
                {
                    if (childNode.ElementTypes.TryGetValue(typeCode, out ElementNode typeNode) == false)
                    {
                        typeNode = new ElementNode(elementId);
                        childNode.ElementTypes.Add(typeCode, typeNode);
                    }
                    childNode = typeNode;
                }
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

            return childNode;
        }


        //$void LinkDifferentialItem(ElementNode head,
        //    ElementDefinition differentialItem)
        //{
        //    if (this.nodes.TryGetValue(differentialItem.ElementId, out ElementNode differentialNode) == true)
        //    {
        //        if (differentialNode.DiffElement != null)
        //            throw new Exception("Differential item {differentialItem.ElementId} already linked");
        //        differentialNode.DiffElement = differentialItem;
        //        return;
        //    }

        //    StringBuilder sb = new StringBuilder();
        //    foreach (ElementNode node in this.nodes.Values)
        //        node.Dump(sb);
        //    File.WriteAllText(@"c:\Temp\scr.txt", sb.ToString());
        //    throw new Exception($"Can not find snapshot node matching differential {differentialItem.ElementId}");
        //}
    }
}
