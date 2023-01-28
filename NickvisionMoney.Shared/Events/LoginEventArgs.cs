using System;

namespace NickvisionMoney.Shared.Events;

/// <summary>
/// Event args for when a login is needed
/// </summary>
public class LoginEventArgs : EventArgs
{
    /// <summary>
    /// The title of the account to login
    /// </summary>
    public string Title { get; init; }
    /// <summary>
    /// The password of the login
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Constructs a LoginEventArgs
    /// </summary>
    /// <param name="title">The title of the account to login</param>
    public LoginEventArgs(string title)
    {
        Title = title;
        Password = null;
    }
}