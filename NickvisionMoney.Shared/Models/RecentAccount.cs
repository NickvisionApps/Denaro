using System;

namespace NickvisionMoney.Shared.Models;

/// <summary>
/// A model of a recent account
/// </summary>
public class RecentAccount : IEquatable<RecentAccount>
{
    /// <summary>
    /// The path of the recent account
    /// </summary>
    public string Path { get; set; }
    /// <summary>
    /// The name of the recent account
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// The type of the recent account
    /// </summary>
    public AccountType Type { get; set; }

    /// <summary>
    /// Constructs a RecentAccount
    /// </summary>
    /// <param name="path">The path of the recent account</param>
    public RecentAccount(string path = "null")
    {
        Path = path;
        Name = System.IO.Path.GetFileNameWithoutExtension(Path);
        Type = AccountType.Checking;
    }

    /// <summary>
    /// Gets whether or not an object is equal to this RecentAccount
    /// </summary>
    /// <param name="obj">The object to compare</param>
    /// <returns>True if equals, else false</returns>
    public override bool Equals(object? obj)
    {
        if (obj is RecentAccount toCompare)
        {
            return Path == toCompare.Path;
        }
        return false;
    }

    /// Gets whether or not an object is equal to this RecentAccount
    /// </summary>
    /// <param name="obj">The RecentAccount? object to compare</param>
    /// <returns>True if equals, else false</returns>
    public bool Equals(RecentAccount? other) => Equals(other);

    /// <summary>
    /// Compares two RecentAccount objects by ==
    /// </summary>
    /// <param name="a">The first RecentAccount object</param>
    /// <param name="b">The second RecentAccount object</param>
    /// <returns>True if a == b, else false</returns>
    public static bool operator ==(RecentAccount? a, RecentAccount? b) => a?.Path == b?.Path;

    /// <summary>
    /// Compares two RecentAccount objects by !=
    /// </summary>
    /// <param name="a">The first RecentAccount object</param>
    /// <param name="b">The second RecentAccount object</param>
    /// <returns>True if a != b, else false</returns>
    public static bool operator !=(RecentAccount? a, RecentAccount? b) => a?.Path != b?.Path;
}
