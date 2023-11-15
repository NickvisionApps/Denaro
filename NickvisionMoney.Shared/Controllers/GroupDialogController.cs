﻿using Nickvision.Aura;
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
    private readonly string _originalName;
    private readonly string _originalRGBA;
    private readonly List<string> _existingNames;

    /// <summary>
    /// The group represented by the controller
    /// </summary>
    public Group Group { get; init; }
    /// <summary>
    /// Whether or not the dialog is editing a group
    /// </summary>
    public bool IsEditing { get; init; }
    /// <summary>
    /// Whether to use native digits
    /// </summary>
    public bool UseNativeDigits => Configuration.Current.UseNativeDigits;

    /// <summary>
    /// Gets the AppInfo object
    /// </summary>
    public AppInfo AppInfo => Aura.Active.AppInfo;
    /// <summary>
    /// Whether or not the group color has changed
    /// </summary>
    public bool HasColorChanged => _originalRGBA != Group.RGBA;

    /// <summary>
    /// Creates a GroupDialogController
    /// </summary>
    /// <param name="group">The Group object represented by the controller</param>
    /// <param name="existingNames">The list of existing group names</param>
    /// <param name="groupDefaultColor">A default color for the group</param>
    internal GroupDialogController(Group group, List<string> existingNames, string groupDefaultColor)
    {
        _originalName = group.Name;
        _originalRGBA = group.RGBA;
        _existingNames = existingNames;
        Group = (Group)group.Clone();
        IsEditing = true;
        if (string.IsNullOrWhiteSpace(Group.RGBA))
        {
            Group.RGBA = groupDefaultColor;
        }
    }

    /// <summary>
    /// Creates a GroupDialogController
    /// </summary>
    /// <param name="id">The id of the new group</param>
    /// <param name="existingNames">The list of existing group names</param>
    /// <param name="groupDefaultColor">A default color for the group</param>
    internal GroupDialogController(uint id, List<string> existingNames, string groupDefaultColor)
    {
        _originalName = "";
        _originalRGBA = groupDefaultColor;
        _existingNames = existingNames;
        Group = new Group(id);
        IsEditing = false;
        //Set Defaults For New Group
        Group.RGBA = groupDefaultColor;
    }

    /// <summary>
    /// Updates the Group object
    /// </summary>
    /// <param name="name">The new name for the group</param>
    /// <param name="description">The new description for the group</param>
    /// <param name="rgba">The new rgba for the group</param>
    /// <returns>GroupCheckStatus</returns>
    public GroupCheckStatus UpdateGroup(string name, string description, string rgba)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return GroupCheckStatus.EmptyName;
        }
        if (name != _originalName && _existingNames.Contains(name))
        {
            return GroupCheckStatus.NameExists;
        }
        Group.Name = name;
        Group.Description = description;
        Group.RGBA = rgba;
        return GroupCheckStatus.Valid;
    }
}
