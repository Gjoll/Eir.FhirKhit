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
                Load("", head, items.ToArray(), ref itemIndex);
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
        void Load(String elementId,
            ElementNode head,
            ElementDefinition[] loadItems,
            ref Int32 itemIndex)
        {
            const String fcn = "Load";

            while (itemIndex < loadItems.Length)
            {
                ElementDefinition loadItem = loadItems[itemIndex];

                if (loadItem.ElementId.StartsWith(elementId) == false)
                    return;
                String loadId = loadItem.ElementId.Substring(elementId.Length);

                if ((elementId.Length > 0) && (loadId[0] != '.'))
                    return;

                itemIndex += 1;

                String[] parts = loadId.Split('.');

                if (parts.Length > 1)
                    throw new Exception($"Invalid Element {loadId}");

                String[] sliceParts = parts[0].Split(':');

                switch (sliceParts.Length)
                {
                    case 1:
                        {
                            if (head.TryGetChild(sliceParts[0], out ElementNode childNode) == true)
                            {
                                if (childNode.Element != null)
                                    throw new Exception($"Duplicate Element {elementId}");
                                childNode.Element = loadItem;
                            }
                            else
                            {
                                childNode = new ElementNode(loadItem);
                                head.Children.Add(childNode);
                            }
                            Load(loadItem.ElementId, childNode, loadItems, ref itemIndex);
                        }
                        break;

                    case 2:
                        {
                            if (head.TryGetChild(sliceParts[0], out ElementNode childNode) == false)
                                throw new Exception($"Missing base element in slice node {parts[0]}");

                            if (head.TryGetSlice(sliceParts[1], out ElementSlice slice) == true)
                            {
                                if (slice.ElementNode.Element != null)
                                    throw new Exception($"Duplicate Element {elementId}");
                                slice.ElementNode.Element = loadItem;
                            }
                            else
                            {
                                slice = new ElementSlice
                                {
                                    ElementNode = new ElementNode(loadItem)
                                };
                                childNode.Slices.Add(slice);
                            }
                            Load(loadItem.ElementId, slice.ElementNode, loadItems, ref itemIndex);
                        }
                        break;

                    default:
                        throw new Exception($"Internal error. Invalid slice...");
                }
            }
        }

        //public bool Add(ElementNode head,
        //    ElementDefinition item)
        //{
        //    return Add(head, new ElementDefinition[] { item });
        //}

        //public bool Add(ElementNode head,
        //    IEnumerable<ElementDefinition> items)
        //{
        //    const String fcn = "Add";

        //    Int32 itemIndex = 0;
        //    Load("", head.DefaultSlice, items.ToArray(), ref itemIndex);
        //    if (itemIndex != items.Count())
        //    {
        //        this.Error(this.GetType().Name, fcn, $"Loader error. Unconsumed elements leftover....");
        //        return false;
        //    }
        //    return true;
        //}
    }
}
