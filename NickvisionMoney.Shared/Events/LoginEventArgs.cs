using System;

namespace NickvisionMoney.Shared.Events;

/// <summary>
/// Event args for when a login is needed
/// </summary>
public class LoginEventArgs : EventArgs
{
    /// <summary>
    /// The password of the login
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Constructs a LoginEventArgs
    /// </summary>
    public LoginEventArgs() => Password = null;
}