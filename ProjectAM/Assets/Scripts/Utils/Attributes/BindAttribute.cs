using System;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class BindAttribute : Attribute
{
    public string Name { get; }

    public BindAttribute(string name = null)
    {
        Name = name;
    }
}