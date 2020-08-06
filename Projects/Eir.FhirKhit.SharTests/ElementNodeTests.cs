using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Hl7.Fhir.Introspection;
using Hl7.Fhir.Model;
using Xunit;
using System.IO;
using System.Diagnostics;
using Eir.DevTools;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.DataCollection;

#if FHIR_R3
using Eir.FhirKhit.R3;
#elif FHIR_R4
using Eir.FhirKhit.R4;
#endif

#if FHIR_R3
namespace Eir.FhirKhit.R3.XUnitTests
#elif FHIR_R4
namespace Eir.FhirKhit.R4.XUnitTests
#endif
{
    public class ElementNodeTests
    {
        ElementDefinition CreateEDef(String path, String id)
        {
            return new ElementDefinition
            {
                Path = path,
                ElementId = id
            };
        }

        [Fact(DisplayName = "ElementNode.LoadTest1")]
        void LoadTest1()
        {
            List<ElementDefinition> items = new List<ElementDefinition>();
            items.Add(CreateEDef("A", "A"));
            items.Add(CreateEDef("A.1", "A.A1"));
            items.Add(CreateEDef("B", "B"));
            items.Add(CreateEDef("C", "C"));
            items.Add(CreateEDef("C.1", "C.C1"));
            items.Add(CreateEDef("C.2", "C.C2"));
            ElementLoader loader = new ElementLoader();
            ElementNode e = loader.Create(items, null);

            Assert.True(e.Children.Count == 3);

            Assert.True(e.Children[0].NodeName == "A");
            Assert.True(e.Children[0].Children.Count == 1);
            Assert.True(e.Children[0].Children[0].NodeName == "A1");

            Assert.True(e.Children[1].NodeName == "B");
            Assert.True(e.Children[1].Children.Count == 0);

            Assert.True(e.Children[2].NodeName == "C");
            Assert.True(e.Children[2].Children.Count == 2);
            Assert.True(e.Children[2].Children[0].NodeName == "C1");
            Assert.True(e.Children[2].Children[1].NodeName == "C2");
        }


        [Fact(DisplayName = "ElementNode.LoadTest2")]
        void LoadTest2()
        {
            List<ElementDefinition> items = new List<ElementDefinition>();
            items.Add(CreateEDef("A", "A"));
            items.Add(CreateEDef("A", "A:Slice"));
            items.Add(CreateEDef("A.1", "A:Slice.A1"));
            items.Add(CreateEDef("A.2", "A.A2"));
            items.Add(CreateEDef("B", "B"));

            ElementLoader loader = new ElementLoader();
            ElementNode e = loader.Create(items, null);

            Assert.True(e.Children.Count == 2);

            ElementNode child1 = e.Children[0];
            Assert.True(child1.NodeName == "A");
            Assert.True(child1.Children.Count == 1);
            Assert.True(child1.Children[0].NodeName == "A2");

            Assert.True(child1.Slices.Count == 1);
            Assert.True(child1.Slices[0].ElementNode.Children.Count == 1);
            Assert.True(child1.Slices[0].ElementNode.Children[0].NodeName == "A1");

            ElementNode child2 = e.Children[1];
            Assert.True(child2.Slices.Count == 0);
            Assert.True(child2.NodeName == "B");
        }
        [Fact(DisplayName = "ElementNode.LoadSubTypes")]
        void LoadSubTypes()
        {
            List<ElementDefinition> snapItems = new List<ElementDefinition>();
            snapItems.Add(CreateEDef("A", "A"));
            ElementDefinition a1 = CreateEDef("A.value[x]", "A.value[x]");
            a1.Type.Add(new ElementDefinition.TypeRefComponent
            {
                Code = "String"
            });
            a1.Type.Add(new ElementDefinition.TypeRefComponent
            {
                Code = "Range"
            });
            snapItems.Add(a1);

            snapItems.Add(CreateEDef("A.valueString", "A.valueString"));
            snapItems.Add(CreateEDef("A.valueRange", "A.valueRange"));
            snapItems.Add(CreateEDef("B", "B"));

            List<ElementDefinition> diffItems = new List<ElementDefinition>();
            diffItems.Add(CreateEDef("A.value[x]", "A.value[x]"));
            diffItems.Add(CreateEDef("A.valueString", "A.valueString"));
            diffItems.Add(CreateEDef("A.valueRange", "A.valueRange"));

            ElementLoader loader = new ElementLoader();
            ElementNode e = loader.Create(snapItems, diffItems);

            Assert.True(e.Children.Count == 2);

            ElementNode childA = e.Children[0];
            Assert.True(childA.NodeName == "A");
            Assert.True(childA.Children.Count == 1);
            Assert.True(childA.DiffElement == null);

            ElementNode childB = e.Children[1];
            Assert.True(childB.NodeName == "B");
            Assert.True(childB.Children.Count == 0);
            Assert.True(childB.DiffElement == null);

            ElementNode valueX = e.Children[0].Children[0];
            Assert.True(valueX.NodeName == "value[x]");
            Assert.True(valueX.Children.Count == 0);
            Assert.True(valueX.ElementTypes.Count == 2);
            Assert.True(valueX.DiffElement.ElementId == "A.value[x]");

            Assert.True(valueX.ElementTypes.TryGetValue("String", out ElementNode valueString)== true);
            Assert.True(valueString.NodeName == "valueString");
            Assert.True(valueString.DiffElement.ElementId == "A.valueString");

            Assert.True(valueX.ElementTypes.TryGetValue("Range", out ElementNode valueRange) == true);
            Assert.True(valueRange.NodeName == "valueRange");
            Assert.True(valueRange.DiffElement.ElementId == "A.valueRange");
        }

    }
}
