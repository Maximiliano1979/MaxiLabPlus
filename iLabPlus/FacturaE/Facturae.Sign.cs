// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using iLabPlus.FacturaE.XAdES;
using iLabPlus.FacturaE.Xml;
using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace iLabPlus.FacturaE;

/// <summary>
/// Facturae extensions
/// </summary>
public partial class Facturae
{
    private static readonly XmlSerializer s_serializer = new(typeof(Facturae));
    private static readonly Encoding      s_encoding   = new UTF8Encoding(false);

    private static void XmlValidationEventHandler(object sender, ValidationEventArgs e)
    {
        if (e.Severity == XmlSeverityType.Error)
        {
            throw e.Exception;
        }
        else
        {
            Trace.Write($"Warning while validating XML '{e.Message}'");
        }
    }

    /// <summary>
    /// Validates the electronic invoice XML.
    /// </summary>
    /// <returns></returns>
    public Facturae Validate()
    {
        XmlDocument document = ToXmlDocument();

        document.Schemas.Add(XsdSchemas.BuildSchemaSet(document.NameTable));
        document.Validate(XmlValidationEventHandler);

        return this;
    }

    /// <summary>
    /// Signs the electronic invoice using the given certificate & RSA key.
    /// </summary>
    /// <param name="certificate">The certificate.</param>
    /// <param name="signerRole">Rol del "firmante" de la factura</param>
    /// <returns>The XAdES signature verifier.</returns>
    public XAdESSignatureVerifier Sign(X509Certificate2 certificate, ClaimedRole signerRole = ClaimedRole.Supplier)
    {

        //System.Diagnostics.Debug.WriteLine("**************************************** Sign : Entrando en la función Sign");

        if (certificate is null)
        {
            throw new ArgumentNullException(nameof(certificate), "certificate cannot be null");
        }

        var privateKey = certificate.GetRSAPrivateKey();
        if (privateKey is null)
        {
            throw new ArgumentNullException(nameof(certificate), "the certificate private key cannot be null");
        }
        var publicKey = certificate.GetRSAPublicKey();
        if (publicKey is null)
        {
            throw new ArgumentNullException(nameof(certificate), "the certificate public key cannot be null");
        }

        //System.Diagnostics.Debug.WriteLine("**************************************** Sign : 11111111");

        var document  = ToXmlDocument();
        var signedXml = new XAdESSignedXml(document);

        // Set the key to sign
        signedXml.SigningKey = privateKey;

        signedXml
            .SetSignatureInfo()
            .SetSignerRole(signerRole)          // XAdES Signer Role
            .SetKeyInfo(certificate, publicKey) // Key Info
            .ComputeSignature();                // Compute Signature

        // Import the signed XML node
        document.DocumentElement.AppendChild(document.ImportNode(signedXml.GetXml(), true));

        System.Diagnostics.Debug.WriteLine("**************************************** Sign : 22222222");

        var _XAdESSignatureVerifier = new XAdESSignatureVerifier(document);

        System.Diagnostics.Debug.WriteLine(_XAdESSignatureVerifier);

        return _XAdESSignatureVerifier;
    }

    /// <summary>
    /// Writes the electronic invoice to the given path.
    /// </summary>
    /// <param name="path">The file path.</param>
    /// <returns></returns>
    public Facturae WriteToFile(string path)
    {
        File.WriteAllText(path, ToXml());

        return this;
    }

    private string ToXml()
    {
        var settings = new XmlWriterSettings { Encoding = s_encoding, ConformanceLevel = ConformanceLevel.Auto };

        using var buffer = new MemoryStream();
        using (var writer = XmlWriter.Create(buffer, settings))
        {
            s_serializer.Serialize(writer, this, XsdSchemas.XadesSerializerNamespaces);
        }

        return s_encoding.GetString(buffer.GetBuffer());
    }

    private XmlDocument ToXmlDocument()
    {
        var document = new XmlDocument { PreserveWhitespace = true };

        document.LoadXml(ToXml());

        return document;
    }
}
