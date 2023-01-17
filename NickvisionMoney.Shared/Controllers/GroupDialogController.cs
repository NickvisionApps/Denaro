using NickvisionMoney.Shared.Helpers;
using NickvisionMoney.Shared.Models;
using System.Collections.Generic;

namespace NickvisionMoney.Shared.Controllers;

/// <summary>
/// Statuses for when a group is validated
/// </summary>
public enum GroupCheckStatus
{
    Valid = 0,
    EmptyName,
    NameExists
}

/// <summary>
/// A controller for a GroupDialog
/// </summary>
public class GroupDialogController
{
    private string _originalName;
    private readonly List<string> _existingNames;

    /// <summary>
    /// The localizer to get translated strings from
    /// </summary>
    public Localizer Localizer { get; init; }
    /// <summary>
    /// The group represented by the controller
    /// </summary>
    public Group Group { get; init; }
    /// <summary>
    /// Whether or not the dialog was accepted (response)
    /// </summary>
    public bool Accepted { get; set; }

    /// <summary>
    /// Creates a GroupDialogController
    /// </summary>
    /// <param name="group">The Group object represented by the controller</param>
    /// <param name="existingNames">The list of existing group names</param>
    /// <param name="localizer">The Localizer of the app</param>
    internal GroupDialogController(Group group, List<string> existingNames, Localizer localizer)
    {
        _originalName = group.Name;
        _existingNames = existingNames;
        Localizer = localizer;
        Group = (Group)group.Clone();
        Accepted = false;
    }

    /// <summary>
    /// Creates a GroupDialogController
    /// </summary>
    /// <param name="id">The id of the new group</param>
    /// <param name="existingNames">The list of existing group names</param>
    /// <param name="localizer">The Localizer of the app</param>
    internal GroupDialogController(uint id, List<string> existingNames, Localizer localizer)
    {
        _originalName = "";
        _existingNames = existingNames;
        Localizer = localizer;
        Group = new Group(id);
        Accepted = false;
    }

    /// <summary>
    /// Updates the Group object
    /// </summary>
    /// <param name="name">The new name for the group</param>
    /// <param name="description">The new description for the group</param>
    /// <returns>GroupCheckStatus</returns>
    public GroupCheckStatus UpdateGroup(string name, string description)
    {
        if(string.IsNullOrEmpty(name))
        {
            return GroupCheckStatus.EmptyName;
        }
        if(name != _originalName && _existingNames.Contains(name))
        {
            return GroupCheckStatus.NameExists;
        }
        Group.Name = name;
        Group.Description = description;
        return GroupCheckStatus.Valid;
    }
}
