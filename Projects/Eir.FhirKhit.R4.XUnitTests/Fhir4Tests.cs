using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using Xunit;

namespace Eir.FhirKhit.R4.XUnitTests
{
    public class Fhir4Tests
    {
        [Fact(DisplayName = "SnapshotTest1")]
        public async void SnapshotTest1()
        {
            String path = @"C:\Development\HL7\BreastRadiologyProfilesV3\Projects\build\input\profiles\StructureDefinition-AbnormalityArchitecturalDistortion.json";
            StructureDefinition sDef;
            switch (Path.GetExtension(path).ToUpper(CultureInfo.InvariantCulture))
            {
                case ".XML":
                    {
                        FhirXmlParser parser = new FhirXmlParser();
                        sDef = (StructureDefinition) parser.Parse<DomainResource>(File.ReadAllText(path));
                        break;
                    }

                case ".JSON":
                    {
                        FhirJsonParser parser = new FhirJsonParser();
                        sDef = (StructureDefinition) parser.Parse<DomainResource>(File.ReadAllText(path));
                        break;
                    }

                default:
                    throw new Exception($"Unknown extension for serialized fhir resource '{path}'");
            }

            await SnapshotCreator.CreateAsync(sDef);
            Debug.Assert(sDef.Snapshot != null);
            Debug.Assert(sDef.Snapshot.Element.Count > 0);
        }
    }
}
