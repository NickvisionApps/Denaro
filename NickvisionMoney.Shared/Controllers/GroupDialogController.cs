using NickvisionMoney.Shared.Models;
using System.Collections.Generic;

namespace NickvisionMoney.Shared.Controllers;

public enum GroupCheckStatus
{
    Valid = 0,
    EmptyName,
    NameExists,
    EmptyDescription
}

public class GroupDialogController
{
    private List<string> _existingNames;

    public Group Group { get; init; }
    public bool Accepted { get; set; }

    public GroupDialogController(uint id, List<string> existingNames)
    {
        _existingNames = existingNames;
        Group = new Group(id);
        Accepted = false;
    }

    public GroupDialogController(Group group, List<string> existingNames)
    {
        _existingNames = existingNames;
        Group = group;
        Accepted = true;
    }

    public GroupCheckStatus UpdateGroup(string name, string description)
    {
        if(string.IsNullOrEmpty(name))
        {
            return GroupCheckStatus.EmptyName;
        }
        if(string.IsNullOrEmpty(description))
        {
            return GroupCheckStatus.EmptyDescription;
        }
        if(name != Group.Name && _existingNames.Contains(name))
        {
            return GroupCheckStatus.NameExists;
        }
        Group.Name = name;
        Group.Description = description;
        return GroupCheckStatus.Valid;
    }
}
