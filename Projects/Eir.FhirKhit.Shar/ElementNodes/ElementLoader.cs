﻿using Eir.DevTools;
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
            if (snapShotItems != null)
            {
                Load(0, head, snapShotItems.ToArray(), ref itemIndex);
                ProcessTypeNodes(head);
                if (itemIndex != snapShotItems.Count())
                {
                    this.Error(this.GetType().Name, fcn, $"Loader error. Unconsumed elements leftover....");
                    return null;
                }
            }
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
                if (n.NodeName.EndsWith("[x]"))
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
            String baseName = baseNode.NodeName.Substring(0, baseNode.NodeName.Length - 3);

            while (index < head.Children.Count)
            {
                ElementNode n = head.Children[index];
                if (n.NodeName.StartsWith(baseName) == false)
                    return;

                String code = String.Empty;
                bool match = false;
                foreach (ElementDefinition.TypeRefComponent type in baseNode.Element.Type)
                {
                    if (n.NodeName == $"{baseName}{type.Code}")
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
            while (pathDepth < p.Nodes.Count)
            {
                ElementPath.Node pathNode = p.Nodes[pathDepth];
                newNode = this.GetNode(head, pathNode);
                head = newNode;
                pathDepth += 1;
            }
            newNode.Element = loadItem;
        }

        ElementNode GetNode(ElementNode head, ElementPath.Node pathNode)
        {
            if (head.TryGetImmediateChild(pathNode.Name, out ElementNode childNode) == false)
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
