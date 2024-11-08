﻿// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using X500;
using System.Diagnostics;
using System.Globalization;
using System.Numerics;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System;
using System.Collections.Generic;
using iLabPlus.FacturaE.Xml;

namespace iLabPlus.FacturaE.XAdES;

/// <summary>
/// XAdES Extension Methods
/// </summary>
internal static class XAdESExtensions
{
    private static readonly string PolicyIdentifier = "http://www.facturae.es/politica_de_firma_formato_facturae/politica_de_firma_formato_facturae_v3_1.pdf";
    private static readonly string PolicyResource   = "Nemesis365.FacturaE.Policies.politica_de_firma_formato_facturae_v3_1.pdf";
    private static readonly byte[] PolicyHash       = Assembly.GetExecutingAssembly().GetManifestResourceStream(PolicyResource).ComputeSHA1Hash();

    private static readonly Encoding          s_encoding       = new UTF8Encoding(false);
    private static readonly XmlSerializer     s_serializer     = new(typeof(QualifyingPropertiesType));
    private static readonly XmlWriterSettings s_writerSettings = new() { Encoding = s_encoding };

    internal static SignedPropertiesType CreateSignedProperties(
        this QualifyingPropertiesType properties,
        XAdESSignedXml                signedXml)
    {
        var id = XsdSchemas.FormatId(signedXml.Signature.Id, "SignedProperties");

        properties.SignedProperties = new SignedPropertiesType { Id = id };

        return properties.SignedProperties;
    }

    internal static SignedSignaturePropertiesType CreateSignedSignatureProperties(this SignedPropertiesType properties)
    {
        properties.SignedSignatureProperties = new SignedSignaturePropertiesType();

        return properties.SignedSignatureProperties;
    }

    internal static SignedSignaturePropertiesType SetSigningTime(this SignedSignaturePropertiesType properties)
    {
        properties.SigningTime          = DateTime.Now;
        properties.SigningTimeSpecified = true;

        return properties;
    }

    internal static SignedSignaturePropertiesType SetSignerRole(
        this SignedSignaturePropertiesType properties,
        ClaimedRole                        signerRole)
    {
        properties.SignerRole = new SignerRoleType { ClaimedRoles = new List<ClaimedRole> { signerRole } };

        return properties;
    }

    internal static SignedSignaturePropertiesType SetSigningCertificate(
        this SignedSignaturePropertiesType properties,
        X509Certificate2                   certificate)
    {
        Debug.Assert(certificate is not null);

        properties.SigningCertificate = new CertIDType[]
        {
            new CertIDType
            {
                CertDigest = new DigestAlgAndValueType
                {
                    DigestMethod = new DigestMethodType { Algorithm = SignedXml.XmlDsigSHA1Url },
                    DigestValue  = certificate.RawData.ComputeSHA1Hash()
                }
                , IssuerSerial = new X509IssuerSerialType
                {
                    X509IssuerName   = X500.X500DistinguishedName.Format(DistinguishedNameFormat.RFC2253, certificate.IssuerName.RawData),
                    X509SerialNumber = BigInteger.Parse(certificate.SerialNumber, NumberStyles.HexNumber).ToString()
                }
            }
        };

        return properties;
    }

    internal static SignaturePolicyIdentifierType SetSignaturePolicyIdentifier(this SignedSignaturePropertiesType properties)
    {
        properties.SignaturePolicyIdentifier = new SignaturePolicyIdentifierType
        {
            Item = new SignaturePolicyIdType
            {
                SigPolicyId = new ObjectIdentifierType
                {
                    Identifier  = new IdentifierType { Value = PolicyIdentifier },
                    Description = "Política de Firma FacturaE v3.1"
                }
                ,
                SigPolicyHash = new DigestAlgAndValueType
                {
                    DigestMethod = new DigestMethodType { Algorithm = SignedXml.XmlDsigSHA1Url },
                    DigestValue  = PolicyHash
                }
            }
        };

        return properties.SignaturePolicyIdentifier;
    }

    internal static string ToXml(this QualifyingPropertiesType properties)
    {
        var buffer = new StringBuilder();

        using (var writer = XmlWriter.Create(buffer, s_writerSettings))
        {
            s_serializer.Serialize(writer, properties, XsdSchemas.XadesSerializerNamespaces);
        }

        return buffer.ToString();
    }

    internal static XmlDocument ToXmlDocument(this QualifyingPropertiesType properties)
    {
        var document = new XmlDocument { PreserveWhitespace = true };

        document.LoadXml(properties.ToXml());

        return document;
    }
}
