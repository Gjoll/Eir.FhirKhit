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
        void Load(String baseId,
            ElementNode head,
            ElementDefinition[] loadItems,
            ref Int32 itemIndex)
        {
            //const String fcn = "Load";

            while (itemIndex < loadItems.Length)
            {
                ElementDefinition loadItem = loadItems[itemIndex];

                if (loadItem.ElementId.StartsWith(baseId) == false)
                    return;
                String loadId = loadItem.ElementId.Substring(baseId.Length);

                if ((baseId.Length > 0) && (loadId[0] != '.'))
                    return;
                itemIndex += 1;
                String[] parts = loadId.Split('.');
                ElementNode currentNode = head;
                foreach (String part in parts)
                {
                    String[] sliceParts = part.Split(':');

                    ElementNode newNode = this.GetNode(currentNode, fullId);
                    switch (sliceParts.Length)
                    {
                        case 1:
                            break;

                        case 2:
                            {
                                ElementSlice s = this.GetSlice(currentNode, sliceParts[1], sliceParts[0]);
                                newNode = s.ElementNode;
                            }
                            break;

                        default:
                            throw new Exception($"Invalid Element path part {part}");
                    }
                    currentNode = newNode;
                }
                if (currentNode.Element != null)
                    throw new Exception($"Duplicate Element {baseId}");
                currentNode.Element = loadItem;
            }
        }

        ElementNode GetNode(ElementNode head, String name)
        {
            if (head.TryGetChild(name, out ElementNode childNode) == false)
            {
                childNode = new ElementNode(name);
                head.Children.Add(childNode);
            }
            return childNode;
        }

        ElementSlice GetSlice(ElementNode head, String sliceName, String elementName)
        {
            if (head.TryGetSlice(sliceName, out ElementSlice slice) == false)
            {
                slice = new ElementSlice(sliceName, elementName);
                head.Slices.Add(slice);
            }
            return slice;
        }
    }
}
