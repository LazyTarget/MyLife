namespace MyLife.Models
{
    public enum FilterGroupRule
    {
        AND,
        OR,
    }

    public enum FilterOperator
    {
        None,
        LessThan,
        LessOrEqualsThan,
        Equals,
        GreaterOrEqualsThan,
        GreaterThan,
    }
}