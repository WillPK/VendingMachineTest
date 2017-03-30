using System.Collections.Generic;

namespace VendingMachine
{
    public class CustomerAccount
    {
        public List<CashCard> CashCards { get; set; }

        public double Balance { get; set; }
    }
}
