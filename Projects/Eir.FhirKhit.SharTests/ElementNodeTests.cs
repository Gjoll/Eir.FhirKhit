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
            items.Add(CreateEDef("A.1", "A.1"));
            items.Add(CreateEDef("B", "B"));
            items.Add(CreateEDef("C", "C"));
            items.Add(CreateEDef("C.1", "C.1"));
            items.Add(CreateEDef("C.2", "C.2"));
            ElementLoader loader = new ElementLoader();
            ElementNode e = loader.Create(items);

            Assert.True(e.Children.Count == 3);

            Assert.True(e.Children[0].Id == "A");
            Assert.True(e.Children[0].Children.Count == 1);
            Assert.True(e.Children[0].Children[0].Id == "A.1");

            Assert.True(e.Children[1].Id == "B");
            Assert.True(e.Children[1].Children.Count == 0);

            Assert.True(e.Children[2].Id == "C");
            Assert.True(e.Children[2].Children.Count == 2);
            Assert.True(e.Children[2].Children[0].Id == "C.1");
            Assert.True(e.Children[2].Children[1].Id == "C.2");
        }


        [Fact(DisplayName = "ElementNode.LoadTest2")]
        void LoadTest2()
        {
            List<ElementDefinition> items = new List<ElementDefinition>();
            items.Add(CreateEDef("A", "A"));
            items.Add(CreateEDef("A", "A:Slice"));
            //items.Add(CreateEDef("A.1", "A:Slice.1"));
            items.Add(CreateEDef("A.1", "A.1"));

            ElementLoader loader = new ElementLoader();
            ElementNode e = loader.Create(items);

            Assert.True(e.Children.Count == 3);

            Assert.True(e.Children[0].Id == "A");
            Assert.True(e.Children[0].Children.Count == 1);
            Assert.True(e.Children[0].Children[0].Id == "A.1");

            Assert.True(e.Slices.Count == 1);
            Assert.True(e.Slices[0].ElementNode.Children.Count == 1);
            Assert.True(e.Slices[0].ElementNode.Children[0].Id == "A:Slice.1");
        }
    }
}
