using iLabPlus.Models.Clases;

namespace iLabPlus.Helpers
{

    public interface IFunctionsCrypto
    {
        byte[] EncryptAES(string input);
    }

    public class FunctionsCryptoWrapper : IFunctionsCrypto
    {
        public byte[] EncryptAES(string input)
        {
            return FunctionsCrypto.EncryptAES(input);
        }
    }
}
