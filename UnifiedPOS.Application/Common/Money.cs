
namespace UnifiedPOS.Application.Common
{
    public static class Money
    {
        public static decimal Round(decimal value) => decimal.Round(value, 2, MidpointRounding.AwayFromZero);
        public static void Validate(decimal value, bool allowZero = true)
        {
            if (value < 0 || (!allowZero && value == 0) || value > 100000000m || Round(value) != value)
                throw PosException.Invalid("Amount must be within range and have at most two decimal places.");
        }
    }
}
