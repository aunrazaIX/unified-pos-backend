namespace UnifiedPOS.Domain.Enums
{
    public enum UserRole { Owner, Admin, Manager, Cashier }
    public enum CartStatus { Open, Held, CheckedOut }
    public enum SaleStatus { Pending, Completed, Voided }
    public enum TenderType { Cash, Card }
}
