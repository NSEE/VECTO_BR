using System;

namespace VectoObjects
{
    public interface IComponentDeclaration
    {
        SignatureType_v2_0 Signature { get; set; }
    }

    public interface IComponentDataDeclaration
    {
#pragma warning disable IDE1006 // Naming Styles
        string id { get; set; }
#pragma warning restore IDE1006 // Naming Styles
        string Manufacturer { get; set; }
        string Model { get; set; }
        string AppVersion { get; set; }
        string CertificationNumber { get; set; }
        DateTime Date { get; set; }
    }

    public interface ISignatureType
    {
        ReferenceType Reference { get; set; }
    }

    public partial class SignatureType_v2_0 : ISignatureType
    {
    }

    public partial class SignatureType_v1_0 : ISignatureType
    {
    }

    public static class ComponentDeclarationExtensions
    {
        public static ISignatureType GetSignature(this IComponentDeclaration component)
        {
            return component.Signature;
        }

        public static bool IsEmptySignature(this ISignatureType signature)
        {
            return signature.Reference.DigestValue.Length == 0;
        }
    }
}