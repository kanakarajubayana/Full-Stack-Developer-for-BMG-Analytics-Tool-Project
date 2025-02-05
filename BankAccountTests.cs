using NUnit.Framework.Legacy;

namespace BMGNunitTest
{
    class BankAccountTests
    {
        [Test]
        public void Deposit_ShouldIncreaseBalance()
        {
            BankAccount account = new BankAccount("Test User");
            account.Deposit(100);
            ClassicAssert.AreEqual(100, account.Balance);
        }

        [Test]
        public void Withdraw_ShouldDecreaseBalance()
        {
            BankAccount account = new BankAccount("Test User");
            account.Deposit(100);
            account.Withdraw(50);
            ClassicAssert.AreEqual(50, account.Balance);
        }

        [Test]
        public void Withdraw_ShouldNotAllowOverdraw()
        {
            BankAccount account = new BankAccount("Test User");
            account.Deposit(50);
            account.Withdraw(100);
            ClassicAssert.AreEqual(50, account.Balance);
        }

        [Test]
        public void ApplyInterest_ShouldIncreaseBalance()
        {
            BankAccount account = new BankAccount("Test User");
            account.Deposit(100);
            account.SetInterestRate(10);
            account.ApplyInterest();
            ClassicAssert.AreEqual(110, account.Balance);
        }
    }
}