// See https://aka.ms/new-console-template for more information
//Console.WriteLine("Hello, World!");
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Transactions;
using static System.Runtime.InteropServices.JavaScript.JSType;

class Program
{
    static void Main(string[] args)
    {
        List<Transaction> transactions = new List<Transaction>();
        List<Account> Acc = new List<Account>();
        List<InterestRule> interestRates = new List<InterestRule>();

        decimal interestRate = 0;
        Console.WriteLine("Welcome to AwesomeGIC Bank!");

        while (true)
        {
            Console.WriteLine("\nWhat would you like to do?");
            Console.WriteLine("[T] Input transactions");
            Console.WriteLine("[I] Define interest rules");
            Console.WriteLine("[P] Print statement");
            Console.WriteLine("[Q] Quit");

            Console.Write("Your choice: ");
            string choice = Console.ReadLine()?.ToUpper();

            switch (choice)
            {
                case "T":
                    InputTransactions(transactions, Acc);
                    break;
                case "I":
                    DefineInterestRules(interestRates);
                    break;
                case "P":
                    PrintStatement(transactions, Acc, interestRates);
                    break;
                case "Q":
                    Console.WriteLine("Thank you for using AwesomeGIC Bank! Goodbye.");
                    return;
                default:
                    Console.WriteLine("Invalid choice. Please try again.");
                    break;
            }
        }
    }

    static void InputTransactions(List<Transaction> transactions, List<Account> Acc)
    {
        Console.Write("Please enter transaction details in <Date> <Account> <Type> <Amount> format (or enter blank to go back to main menu): ");

        try
        {
            string TransactionsData = Console.ReadLine();

            string DateFormat = DateTime.ParseExact(TransactionsData.Split(' ')[0].ToString(), "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None).ToString();

            //int count = transactions.FindAll(t => t.Date == DateTime.ParseExact(TransactionsData.Split(' ')[0].ToString(), "yyyyMMdd", null).ToString()).Count + 1;
            int count = transactions.FindAll(t => t.Date == TransactionsData.Split(' ')[0].ToString()).Count + 1;

            //if (count==1 && TransactionsData.Split(' ')[2] == "W")
            //{
            //    Console.WriteLine("An account's balance should not be less than 0. Therefore, the first transaction on an account should not be a withdrawal");
            //    return;
            //}


            transactions.Add(new Transaction
            {
                Date = TransactionsData.Split(' ')[0],
                TxnId = $"{TransactionsData.Split(' ')[0]}-{count:D2}",
                Type = TransactionsData.Split(' ')[2],
                Amount = Convert.ToDecimal(TransactionsData.Split(' ')[3].ToString())
            });

            Acc.Add(new Account { AccName = TransactionsData.Split(' ')[1], Txns = transactions[0] });

            Console.WriteLine("Transaction added successfully.");
            PrintTxnStatement(transactions, Acc);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Date should be in YYYYMMdd format");
        }

    }

    static void DefineInterestRules(List<InterestRule> interestRates)
    {
        Console.Write("Please enter interest rules details in <Date> <RuleId> <Rate in %> format (or enter blank to go back to main menu): ");
        string interestRateRule = Console.ReadLine();
        var RuleFound = interestRates.Find(Rule => Rule.Date == interestRateRule.Split(' ')[0]);
        if (RuleFound != null)
            interestRates.Remove(RuleFound);



        if (Decimal.Parse(interestRateRule.Split(' ')[2]) < 0 || Decimal.Parse(interestRateRule.Split(' ')[2]) > 100)
        {
            Console.WriteLine("Interest rate should be greater than 0 and less than 100");
        }
        else
        {
            interestRates.Add(new InterestRule
            {
                Date = interestRateRule.Split(' ')[0],
                RuleId = interestRateRule.Split(' ')[1],
                Rate = decimal.Parse(interestRateRule.Split(' ')[2])
            });
        }


        Console.WriteLine("| Date     | RuleId       | Rate (%) |");
        Console.WriteLine("------------------------------------------------");


        foreach (var interestRate in interestRates)
        {
            Console.WriteLine($"| {interestRate.Date} | {interestRate.RuleId} | {interestRate.Rate}");
        }

    }
    static void PrintTxnStatement(List<Transaction> transactions, List<Account> Acc)
    {
        if (transactions.Count == 0)
        {
            Console.WriteLine("No transactions to display.");
            return;
        }
        IEnumerable<Account> filteredAccList = Acc.GroupBy(AccountDetails => AccountDetails.AccName)
                                               .Select(group => group.First());

        foreach (var Accnt in filteredAccList)
        {
            Console.WriteLine("\nAccount: " + Accnt.AccName);

            Console.WriteLine("| Date     | Txn Id      | Type | Amount   |");
            Console.WriteLine("------------------------------------------------");

            decimal balance = 0;
            foreach (var transaction in transactions)
            {
                Console.WriteLine($"| {transaction.Date} | {transaction.TxnId} | {transaction.Type}   | {Math.Abs(transaction.Amount):F2} | ");
                balance += transaction.Amount;
            }

            Console.WriteLine("------------------------------------------------");
            Console.WriteLine($"Current Balance: {balance:F2}");
        }
    }
    static void PrintStatement(List<Transaction> transactions, List<Account> Acc, List<InterestRule> interestRateRules)
    {
        if (transactions.Count == 0)
        {
            Console.WriteLine("No transactions to display.");
            return;
        }

        Console.Write("Please enter account and month to generate the statement <Account> <Year><Month> (or enter blank to go back to main menu): ");
        string AccountData = Console.ReadLine();

        var AccFound = Acc.Find(txn => txn.AccName == AccountData.Split(' ')[0]);


        //IEnumerable<Transaction> filteredTxns = transactions.Any(txns => txns.Date.Substring(5, 6) = AccountData.Split(' ')[1].Substring(5, 6));

        List<Transaction> CurrMonthtransactions = new List<Transaction>();

        Console.WriteLine("\nAccount: " + AccFound.AccName.ToString());
        Console.WriteLine("----------------------------------------------------------------------");
        Console.WriteLine("| Date     | Txn Id           | Type |   Amount   | Balance   |");
        Console.WriteLine("----------------------------------------------------------------------");


        decimal balance = 0;
        string yyyymm = string.Empty;
        foreach (var transaction in transactions)
        {
            yyyymm = transaction.Date.Substring(0, 6);

            if (yyyymm == AccountData.Split(' ')[1].Substring(0, 6))
            {
                if (transaction.Type == "D")
                    balance += transaction.Amount;
                else if (transaction.Type == "W")
                    balance -= transaction.Amount;

                transaction.Balance = balance;
                Console.WriteLine($"{transaction.Date}     {transaction.TxnId}	 {transaction.Type}	 {transaction.Amount:C}  {balance:C}");
                CurrMonthtransactions.Add(transaction);
            }
        }
        decimal CalcInterest = 0;
        string IntTxnDate = string.Empty;
        int EOM = 0;
        calculateInterest(CurrMonthtransactions, yyyymm, interestRateRules, ref CalcInterest, ref IntTxnDate, ref EOM);

        Console.WriteLine($"{EOM}     {" "}	 {"I"}	 {CalcInterest:C}  {balance + CalcInterest:C}");
        Console.WriteLine("---------------------------------------------------------------------");
        //Console.WriteLine($"Balance before interest: {balance:C}");

        //decimal interest = balance * interestRate;
        //Console.WriteLine($"Interest (based on {interestRate * 100}% rate): {interest:C}");
        //Console.WriteLine($"Final Balance: {balance + interest:C}");
    }

    static void calculateInterest(List<Transaction> transactions, string yyyymm, List<InterestRule> interestRateRules, ref decimal CalcInterest, ref string IntTxnDate, ref int EOM)
    {
        int days = 0;

        decimal AnnualizedInterest = 0;
        decimal EODBalance = 0;
        decimal totalInterest = 0;
        decimal RuleRate = 0;
        int cnt = 0;
        List<InterestRule> OldRule = new List<InterestRule>();

        foreach (var txns in transactions)
        {
            if (interestRateRules != null && interestRateRules.Count > 0)
            {
                foreach (var rule in interestRateRules)
                {

                    if (int.Parse(rule.Date.Substring(6)) >= int.Parse(txns.Date.Substring(6)))
                    {
                        EODBalance = txns.Balance;
                    }


                    if (rule.Date.Substring(0, 6) == yyyymm && (cnt == 0))
                    {
                        days = int.Parse(rule.Date.Substring(6)) - 1;
                        AnnualizedInterest += EODBalance * rule.Rate * days;
                        OldRule.Add(rule);
                        interestRateRules.Remove(rule);
                        cnt++;

                    }
                    else if ((rule.Date.Substring(0, 6) == yyyymm))// && (cnt == 1))
                    {

                        days = int.Parse(rule.Date.Substring(6)) - int.Parse(OldRule[cnt - 1].Date.Substring(6)) + 1;

                        AnnualizedInterest += EODBalance * rule.Rate * days;
                        interestRateRules.Remove(rule);
                        RuleRate = rule.Rate;
                        cnt++;
                    }
                    break;
                }
            }
            else
            {
                IntTxnDate = txns.Date;
                int Month = int.Parse(yyyymm.Substring(4));
                int EOTxnM = int.Parse(yyyymm + txns.Date.Substring(6));
                int Totaldays = DateTime.DaysInMonth(int.Parse(txns.Date.Substring(0, 4)), Month);
                days = (int.Parse(yyyymm + Totaldays) - EOTxnM) + 1;
                EOM = int.Parse(yyyymm + Totaldays);
                EODBalance = txns.Balance;
                AnnualizedInterest += EODBalance * RuleRate * days;
            }
        }

        totalInterest = Math.Abs(AnnualizedInterest / 365);
        CalcInterest = totalInterest;

    }
}

class InterestRule
{
    public string Date { get; set; }
    public string RuleId { get; set; }
    public Decimal Rate { get; set; }
}
class Account
{
    public string AccName { get; set; }
    public Transaction Txns { get; set; }
}

class Transaction
{
    public string Date { get; set; }
    public string TxnId { get; set; }
    public string Type { get; set; }
    public decimal Amount { get; set; }
    public decimal Balance { get; set; }
}
