using System.Threading.Tasks;
using Moq;
using NUnit.Framework;

namespace VendingMachine.Tests
{
    [TestFixture]
    public class VendingMachineTests
    {
        VendingMachine _vendingMachine;
        Mock<ICustomerAccountRepository> _customerAccountRepository;
        const int InvalidPin = 1234;
        const int ValidPin = 5678;
        const string CashCardNumber1 = "1234567890";
        const string CashCardNumber2 = "2345678901";
        const double PricePerCan = 0.5;

        [SetUp]
        public void SetUp()
        {
            _customerAccountRepository = new Mock<ICustomerAccountRepository>();
            _vendingMachine = new VendingMachine(_customerAccountRepository.Object);
        }

        [Test]
        public void ShouldHaveAnInitialCapacityOf25()
        {
            Assert.IsTrue(_vendingMachine.Capacity == 25);
        }

        [Test]
        public void ShouldNotVendMoreThan25Cans()
        {
            Assert.Catch(() => _vendingMachine.Vend(CashCardNumber1, 0, 26), "Exceeded remaining cans");
        }

        [Test]
        public void ShouldNotVendWhenPinSuppliedIsInvalid()
        {
            Assert.Catch(() =>
            {
                _customerAccountRepository.Setup(r => r.Find(CashCardNumber1, InvalidPin)).Returns((CustomerAccount)null);

                _vendingMachine.Vend(CashCardNumber1, InvalidPin, 26);
            }, "Customer account not found.");
        }

        [TestCase(0)]
        [TestCase(0.1)]
        [TestCase(0.2)]
        [TestCase(0.4)]
        public void ShouldNotVendWhenLessThan50PAvailable(double balance)
        {
            Assert.Catch(() =>
            {
                _customerAccountRepository
                .Setup(r => r.Find(CashCardNumber1, ValidPin))
                .Returns(new CustomerAccount()
                    {
                        Balance = balance
                });

                _vendingMachine.Vend(CashCardNumber1, ValidPin);
            }, "Balance too low.");
        }
        
        [Test]
        public void ShouldUpdateAccountBalanceWhenBoughtACanSuccessfully()
        {
            var customerAccount = new CustomerAccount()
            {
                Balance = 10
            };

            _customerAccountRepository
               .Setup(r => r.Find(CashCardNumber1, ValidPin))
               .Returns(customerAccount);

            _customerAccountRepository
               .Setup(r => r.Withdraw(customerAccount, PricePerCan));

            // act
            _vendingMachine.Vend(CashCardNumber1, ValidPin);

            // assert
            _customerAccountRepository.VerifyAll();
        }

        [Test]
        public void ShouldUpdateTwoConcurrentTransactions()
        {
            var customerAccount = new CustomerAccount()
            {
                Balance = 10
            };

            _customerAccountRepository
               .Setup(r => r.Find(CashCardNumber1, ValidPin))
               .Returns(customerAccount);

            _customerAccountRepository
               .Setup(r => r.Find(CashCardNumber2, ValidPin))
               .Returns(customerAccount);

            _customerAccountRepository
               .Setup(r => r.Withdraw(customerAccount, PricePerCan));
            
            // act
            Parallel.Invoke(
                () => _vendingMachine.Vend(CashCardNumber1, ValidPin),
                () => _vendingMachine.Vend(CashCardNumber2, ValidPin));

            // assert
            _customerAccountRepository
                .Verify(r => r.Withdraw(customerAccount, PricePerCan), Times.Exactly(2));

            _customerAccountRepository.VerifyAll();
        }
    }
}
