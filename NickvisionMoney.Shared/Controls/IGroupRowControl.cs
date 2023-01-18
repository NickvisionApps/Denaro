using NickvisionMoney.Shared.Models;
using System;
using System.Globalization;

namespace NickvisionMoney.Shared.Controls;

/// <summary>
/// A contract for a group row control
/// </summary>
public interface IGroupRowControl : IModelRowControl<Group>
{
    /// <summary>
    /// Whether or not the filter checkbox is checked
    /// </summary>
    public bool FilterChecked { get; set; }

    /// <summary>
    /// Occurs when the filter checkbox is changed on the row
    /// </summary>
    public event EventHandler<(uint Id, bool Filter)>? FilterChanged;

    /// <summary>
    /// Updates the row based on the new Group model
    /// </summary>
    /// <param name="group">The new Group model</param>
    /// <param name="cultureAmount">The culture to use for displaying amount strings</param>
    /// <param name="cultureDate">The culture to use for displaying date strings</param>
    void IModelRowControl<Group>.UpdateRow(Group group, CultureInfo cultureAmount, CultureInfo cultureDate) => UpdateRow(group, cultureAmount, cultureDate, true);

    /// <summary>
    /// Updates the row based on the new Group model
    /// </summary>
    /// <param name="group">The new Group model</param>
    /// <param name="cultureAmount">The culture to use for displaying amount strings</param>
    /// <param name="cultureAmount">The culture to use for displaying date strings</param>
    /// <param name="filterActive">Whether or not the filter checkbox is active</param>
    public void UpdateRow(Group group, CultureInfo cultureAmount, CultureInfo cultureDate, bool filterActive);
}
