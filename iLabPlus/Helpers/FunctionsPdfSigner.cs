using iText.Signatures;
using iText.Commons.Bouncycastle.Cert;
using System.IO;
using Org.BouncyCastle.Pkcs;
using iText.Bouncycastle.X509;
using iText.Bouncycastle.Crypto;
using iText.Kernel.Pdf;
using System;
using Microsoft.AspNetCore.Authorization;

namespace iLabPlus.Models.Clases
{
    [Authorize]
    public class FunctionsPdfSigner
    {
        public IExternalSignature _privateSignature;
        public IX509Certificate[] _signChain;
        //byte[] _signatureBytes;

        public class PDFSignParameters
        {
            public string       Reason { get; set; }
            public string       Location { get; set; }
            public ImageType    Image { get; set; }
            // Otros parámetros según sea necesario
        }
        public class ImageType
        {
            public float WidthRatio { get; set; }
            public float HeigthRatio { get; set; }
            public float X { get; set; }
            public float Y { get; set; }

            public Stream Data { get; set; }
        }

        public FunctionsPdfSigner(String certFile, string keyPassword )
        {
            try
            {
                Pkcs12Store pkcs12Store = null;

                byte[] certBytes = File.ReadAllBytes(certFile);
                using (MemoryStream ms = new MemoryStream(certBytes))
                {
                    pkcs12Store = new Pkcs12StoreBuilder().Build();
                    pkcs12Store.Load(ms, keyPassword.ToCharArray());
                }

                string alias = null;
                foreach (string tAlias in pkcs12Store.Aliases)
                {
                    if (pkcs12Store.IsKeyEntry(tAlias))
                    {
                        alias = tAlias;
                        break;
                    }
                }


                var pk = pkcs12Store.GetKey(alias).Key;
                var ce = pkcs12Store.GetCertificateChain(alias);

                _signChain = new IX509Certificate[ce.Length];

                for (int k = 0; k < ce.Length; ++k)
                    _signChain[k] = new X509CertificateBC(ce[k].Certificate);
                _privateSignature = new PrivateKeySignature(new PrivateKeyBC(pk), "SHA-512");

            }
            catch (Exception ex)
            {
                var Error = ex.Message;
                var ErrorInner = ex.InnerException;

            }

        }



        public void SignPDF(Stream input, Stream output, PDFSignParameters p)
        {
            try
            {
                PdfReader reader = new PdfReader(input);
                StampingProperties properties = new StampingProperties();
                var signer = new iText.Signatures.PdfSigner(reader, output, properties);

                PdfSignatureAppearance sap = signer.GetSignatureAppearance().SetReason(p.Reason).SetLocation(p.Location);
                //if (p.Image != null)
                //{
                //    var img = iText.IO.Image.ImageDataFactory.Create(new BinaryReader(p.Image.Data).ReadBytes((int)p.Image.Data.Length));

                //    sap.SetSignatureGraphic(img);
                //    sap.SetLayer2Text(string.Empty);
                //    sap.SetPageRect(new iText.Kernel.Geom.Rectangle(p.Image.X, p.Image.Y, img.GetWidth() / p.Image.WidthRatio, img.GetHeight() / p.Image.HeigthRatio));
                //    sap.SetImage(img);
                //}

                //signer.SignExternalContainer(_privateSignature, 8190);

                //var tsa_client = new TSAClientBouncyCastle("", null, null, 8192, "sha256");
                //signer.Timestamp(tsa_client, "SignatureTimeStamp");


                signer.SignDetached(_privateSignature, _signChain, null, null, null, 0, iText.Signatures.PdfSigner.CryptoStandard.CMS);


            }
            catch (Exception ex)
            {
                var Error = ex.Message;
                var ErrorInner = ex.InnerException;
                // Manejo del error

            }


        }


    }
}
