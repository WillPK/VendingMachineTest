namespace VendingMachine
{
    public interface ICustomerAccountRepository
    {
        CustomerAccount Find(string cardNumber, int pin);
        void Withdraw(CustomerAccount customerAccount, double amount);
    }
}
