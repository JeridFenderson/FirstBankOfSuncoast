using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;

namespace FirstBankOfSuncoast
{
    class Program
    {
        class Transaction
        {
            public int Amount { get; set; }
            public bool Checking { get; set; }
            public bool Deposit { get; set; }
            public DateTime Date { get; set; }
        }


        static void Banner(string message)
        {
            Console.WriteLine("\n~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            Console.WriteLine(message);
            Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\n");
        }


        static string Menu()
        {
            Console.WriteLine("\nADD - Deposit money to checking or savings");
            Console.WriteLine("REMOVE - Withdraw money from checking or savings");
            Console.WriteLine("TRANSFER - Transfer money between checking and savings");
            Console.WriteLine("VIEW - View checking history, saving history, or total balances");
            Console.WriteLine("EXIT - Exit The First Bank of Suncoast's application\n");
            Console.Write("What would you like to do? ");
            var choice = Console.ReadLine().Trim().ToUpper();
            Console.WriteLine();
            return choice;
        }


        static int DetermineFundAmount(string verb)
        {
            Console.Write($"\nHow much money would you like to {verb}? ");
            var numberToBeParsed = Console.ReadLine();
            var parsedNumber = 0;
            bool wasThisParsable = int.TryParse(numberToBeParsed, out parsedNumber);
            while (!wasThisParsable)
            {
                Console.Write("\nNot a number. Please enter a number only: ");
                numberToBeParsed = Console.ReadLine();
                wasThisParsable = int.TryParse(numberToBeParsed, out parsedNumber);
                while (parsedNumber < 0)
                {
                    Console.Write("\nPlease enter a positive number only: ");
                    numberToBeParsed = Console.ReadLine();
                    wasThisParsable = int.TryParse(numberToBeParsed, out parsedNumber);
                }
            }
            return parsedNumber;
        }


        static bool DetermineAccountType(string verbing)
        {
            Console.WriteLine($"\nForm which account will you be {verbing} funds?");
            Console.Write("'CHECKING' or 'SAVINGS': ");
            var accountType = Console.ReadLine().Trim().ToUpper();
            while (accountType != "CHECKING" && accountType != "SAVINGS")
            {
                Console.WriteLine("\nPlease enter a valid account type!");
                Console.Write("'CHECKING' or 'SAVINGS': ");
                accountType = Console.ReadLine().Trim().ToUpper();
            }
            if (accountType == "CHECKING")
                return true;
            else
                return false;
        }


        static List<Transaction> AddTransaction(List<Transaction> transactions, bool accountType, int addAmount)
        {
            var newTransaction = new Transaction
            {
                Checking = accountType,
                Amount = addAmount,
                Deposit = true,
                Date = new DateTime()
            };
            transactions.Add(newTransaction);
            Console.Write($"\n${newTransaction.Amount} successfully added to your ");
            if (newTransaction.Checking)
                Console.WriteLine("checking");
            else
                Console.WriteLine("savings");
            return transactions;
        }


        static List<Transaction> RemoveTransaction(List<Transaction> transactions, bool accountType, int removeAmount)
        {
            var checkingAccountTransactions = transactions.Where(transaction => transaction.Checking);
            var savingsAccountTransactions = transactions.Where(transaction => !transaction.Checking);
            var currentAccountTotal = 0;
            var newTransaction = new Transaction
            {
                Checking = accountType,
                Amount = removeAmount,
                Deposit = false,
                Date = new DateTime()
            };
            if (newTransaction.Checking)
                currentAccountTotal = View(checkingAccountTransactions, newTransaction.Checking, false);
            else
                currentAccountTotal = View(savingsAccountTransactions, newTransaction.Checking, false);

            if (currentAccountTotal < newTransaction.Amount)
            {
                Console.WriteLine("\nInsufficient funds. Transaction declined");
            }
            else
            {

                transactions.Add(newTransaction);
                Console.Write($"\n${newTransaction.Amount} successfully removed from your ");
                if (newTransaction.Checking)
                    Console.WriteLine("checking");
                else
                    Console.WriteLine("savings");
            }
            return transactions;
        }

        static void TransferTransaction(List<Transaction> transactions)
        {
            var currentTransactionsAmount = transactions.Count();
            var isChecking = DetermineAccountType("transferring");
            var transferAmount = DetermineFundAmount("transfer");
            RemoveTransaction(transactions, isChecking, transferAmount);
            var newTransactionsAmount = transactions.Count();

            if (newTransactionsAmount != currentTransactionsAmount)
                AddTransaction(transactions, !isChecking, transferAmount);
        }


        static void ViewTransactions(List<Transaction> transactions)
        {
            Console.WriteLine("\nCHECKING - Checking account with transaction history");
            Console.WriteLine("SAVINGS - Savings account with transaction history");
            Console.WriteLine("TOTAL - Summary of balances in both accounts");
            Console.Write("\nWhich would you like to view? ");
            var choice = Console.ReadLine().Trim().ToUpper();
            switch (choice)
            {
                case "CHECKING":
                    View(transactions.Where(transaction => transaction.Checking).OrderByDescending(transaction => transaction.Date), true, true);
                    break;
                case "SAVINGS":
                    View(transactions.Where(transaction => !transaction.Checking).OrderByDescending(transaction => transaction.Date), false, true);
                    break;
                case "TOTAL":
                    View(transactions.Where(transaction => transaction.Checking).OrderByDescending(transaction => transaction.Date), true, false);
                    View(transactions.Where(transaction => !transaction.Checking).OrderByDescending(transaction => transaction.Date), false, false);
                    break;
                default:
                    Console.WriteLine("\nThat was not an option\n");
                    break;
            }

        }


        static int View(IEnumerable<Transaction> transactions, bool isChecking, bool calledFromViewIndividualAccount)
        {
            var accountTotal = 0;

            if (calledFromViewIndividualAccount)
            {
                foreach (var transaction in transactions)
                {
                    if (transaction.Deposit)
                        Console.WriteLine($"\n{transaction.Date} - You added ${transaction.Amount}");
                    else
                        Console.WriteLine($"\n{transaction.Date} - You removed ${transaction.Amount}");
                }
            }
            foreach (var transaction in transactions)
            {
                if (transaction.Deposit)
                    accountTotal += transaction.Amount;
                else
                    accountTotal -= transaction.Amount;
            }

            if (isChecking)
                Console.WriteLine($"\nYou have ${accountTotal} in your checking\n");
            else
                Console.WriteLine($"\nYou have ${accountTotal} in your savings\n");
            return accountTotal;
        }


        static void WriteFile(List<Transaction> transactions)
        {
            var fileWriter = new StreamWriter("transactions.csv");
            var csvWriter = new CsvWriter(fileWriter, CultureInfo.InvariantCulture);
            csvWriter.WriteRecords(transactions);
            fileWriter.Close();
        }


        static void Main(string[] args)
        {
            Banner("    Welcome to The First Bank of Suncoast!");

            TextReader fileReader;
            if (File.Exists("transactions.csv"))
                fileReader = new StreamReader("transactions.csv");
            else
                fileReader = new StringReader("");

            var csvReader = new CsvReader(fileReader, CultureInfo.InvariantCulture);
            var transactions = csvReader.GetRecords<Transaction>().ToList();
            fileReader.Close();

            var choice = Menu();
            while (choice != "EXIT")
            {
                switch (choice)
                {
                    case "ADD":
                        AddTransaction(transactions, DetermineAccountType("adding"), DetermineFundAmount("add"));
                        WriteFile(transactions);
                        choice = Menu();
                        break;
                    case "REMOVE":
                        RemoveTransaction(transactions, DetermineAccountType("removing"), DetermineFundAmount("remove"));
                        WriteFile(transactions);
                        choice = Menu();
                        break;
                    case "TRANSFER":
                        TransferTransaction(transactions);
                        choice = Menu();
                        WriteFile(transactions);
                        break;
                    case "VIEW":
                        ViewTransactions(transactions);
                        choice = Menu();
                        break;
                    default:
                        Console.WriteLine("\nThat's not a menu option, please try again\n");
                        choice = Menu();
                        break;
                }
            }
            Banner("Thank you for using The First Bank of Suncoast!");
        }
    }
}
