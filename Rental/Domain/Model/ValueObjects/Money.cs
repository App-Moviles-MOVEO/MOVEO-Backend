namespace Moveo_backend.Rental.Domain.Model.ValueObjects;

public record Money
{
    public decimal Amount { get; }
    public string Currency { get; } = "USD";
    
    protected Money() 
    { 
        Currency = "USD";
    }

    public Money(decimal amount, string currency = "USD")
    {
        if (amount < 0)
            throw new ArgumentException("The amount cannot be negative", nameof(amount));

        Amount = amount;
        Currency = currency;
    }

    public static Money Zero(string currency = "USD") => new(0, currency);

    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("Cannot add different currencies.");
        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("Cannot subtract different currencies.");
        return new Money(Amount - other.Amount, Currency);
    }

    public override string ToString() => $"{Amount:F2} {Currency}";
}