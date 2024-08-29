namespace iLabPlus.FacturaE
{
    internal static class TaxExtensions
    {
        internal static bool IsTaxWithheld(this TaxTypeCodeType taxType)
        {
            return (taxType == TaxTypeCodeType.PersonalIncomeTax);
        }
    }
}
