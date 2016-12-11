﻿namespace Logger.Tests
{
    public class PersonFlat
    {
        public string Name { get; set; }
        public string LastName;
        public double AmountFlat { get; set; }
        public int AccountNumber;
    }
    public class Account
    {
        public double Amount;
        public int AccountNumber { get; set; }
    }
    public class AmountPerson
    {
        public string Name;
        public string LastName { get; set; }
        public Account Account { get; set; }
    }
}