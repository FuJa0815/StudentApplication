namespace StudentApplication.Models;

public interface IIdentifiable<TKey> where TKey : IEquatable<TKey>
{
    public TKey Key { get; set; }
}