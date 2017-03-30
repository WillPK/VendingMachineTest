using System;

namespace VendingMachine
{
    public class VendingMachine
    {
        // in a production application, the PricePerCan and MaximumCapacity must be injected somehow as they can change
        private const int MaximumCapacity = 25;
        private const double PricePerCan = 0.5;
        private readonly ICustomerAccountRepository _repo;

        public VendingMachine(ICustomerAccountRepository repo)
        {
            _repo = repo;
            Capacity = MaximumCapacity;
        }

        public int Capacity { get; private set; }

        public void Vend(string cardNumber, int pin, int count = 1)
        {
            if (count > Capacity)
            {
                throw new ApplicationException("Exceeded remaining cans.");
            }

            var customerAccount = _repo.Find(cardNumber, pin);

            if (customerAccount == null)
            {
                throw new ApplicationException("Customer account not found.");

            }
            if (customerAccount.Balance < PricePerCan)
            {
                throw new ApplicationException("Balance is too low.");
            }

            // note: this should probably transfer to another account rather than just withdrawing...!
            _repo.Withdraw(customerAccount, PricePerCan);
        }
    }
}
