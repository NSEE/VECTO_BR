using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("VectoInputManagerTests")]

namespace VectoObjects
{
    /// <summary>
    /// This is a test implementation of IComponentDeclaration used for unit testing.
    /// Marked as internal to restrict its visibility to the testing project.
    /// </summary>
    internal class TestComponentDeclaration : IComponentDeclaration
    {
        public SignatureType_v2_0 Signature { get; set; }

        [System.ComponentModel.DescriptionAttribute("P561 - [m²]")]
        [System.ComponentModel.DataAnnotations.RegularExpressionAttribute("[-]?([1-9][0-9]*|0)\\.[0-9]{2}")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Xml.Serialization.XmlElementAttribute("DeltaCdxA_CFD")]
        public decimal DeltaCdxA_CFDValue { get; set; }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        public bool DeltaCdxA_CFDValueSpecified { get; set; }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public System.Nullable<decimal> DeltaCdxA_CFD
        {
            get
            {
                if (this.DeltaCdxA_CFDValueSpecified)
                {
                    return this.DeltaCdxA_CFDValue;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                this.DeltaCdxA_CFDValue = value.GetValueOrDefault();
                this.DeltaCdxA_CFDValueSpecified = value.HasValue;
            }
        }

        [System.Xml.Serialization.XmlElementAttribute("Value")]
        public decimal Value { get; set; }
    }

    internal class Test_Object
    {
        public Test_Nested NestedObject { get; set; }
        public List<Test_Nested> ObjectList { get; set; } = new List<Test_Nested>();
        public decimal[] DecimalArray { get; set; }
        public bool BoolProp { get; set; }
        public string StringProp { get; set; }
        public int IntProp { get; set; }
        public decimal DecimalProp { get; set; }
        public Test_Enum EnumProp { get; set; }
        public Test_Enum EnumProp2 { get; set; }
    }

    internal class Test_Nested
    {
        public string Name { get; set; }
        public int Value { get; set; }
    }

    internal enum Test_Enum
    {
        OptionA,
        OptionB,
        OptionC
    }

}