namespace NickvisionMoney.Shared.Models;

/// <summary>
/// A model of a transaction
/// </summary>
public class Group
{
    /// <summary>
    /// The id of the group
    /// </summary>
    public uint Id { get; init; }
    /// <summary>
    /// The name of the group
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// The description of the group
    /// </summary>
    public string Description { get; set; }
    /// <summary>
    /// The balance of the group
    /// </summary>
    public decimal Balance { get; set; }

    /// <summary>
    /// Constructs a group
    /// </summary>
    /// <param name="id">The id of the group</param>
    public Group(uint id)
    {
        Id = id;
        Name = "";
        Description = "";
        Balance = 0m;
    }

    /// <summary>
    /// Gets whether or not an object is equal to this Group
    /// </summary>
    /// <param name="obj">The object to compare</param>
    /// <returns>True if equals, else false</returns>
    public override bool Equals(object? obj)
    {
        if (obj is Group toCompare)
        {
            return Id == toCompare.Id;
        }
        return false;
    }

    /// <summary>
    /// Compares two Group objects by ==
    /// </summary>
    /// <param name="a">The first Group object</param>
    /// <param name="b">The second Group object</param>
    /// <returns>True if a == b, else false</returns>
    public static bool operator ==(Group a, Group b) => a.Name == b.Name;

    /// <summary>
    /// Compares two Group objects by !=
    /// </summary>
    /// <param name="a">The first Group object</param>
    /// <param name="b">The second Group object</param>
    /// <returns>True if a != b, else false</returns>
    public static bool operator !=(Group a, Group b) => a.Name != b.Name;

    /// <summary>
    /// Compares two Group objects by >
    /// </summary>
    /// <param name="a">The first Group object</param>
    /// <param name="b">The second Group object</param>
    /// <returns>True if a > b, else false</returns>
    public static bool operator <(Group a, Group b) => a.Id < b.Id;

    /// <summary>
    /// Compares two Group objects by <
    /// </summary>
    /// <param name="a">The first Group object</param>
    /// <param name="b">The second Group object</param>
    /// <returns>True if a < b, else false</returns>
    public static bool operator >(Group a, Group b) => a.Id > b.Id;
}
