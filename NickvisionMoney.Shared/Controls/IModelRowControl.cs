using System;

namespace NickvisionMoney.Shared.Controls;

/// <summary>
/// A contract for a model row control
/// </summary>
/// <typeparam name="T">The model type</typeparam>
public interface IModelRowControl<T>
{
    /// <summary>
    /// The Id of the model T the row represents
    /// </summary>
    public uint Id { get; }

    /// <summary>
    /// Occurs when the edit button on the row is clicked 
    /// </summary>
    public event EventHandler<uint>? EditTriggered;
    /// <summary>
    /// Occurs when the delete button on the row is clicked 
    /// </summary>
    public event EventHandler<uint>? DeleteTriggered;

    /// <summary>
    /// Shows the row
    /// </summary>
    public void Show();
    
    /// <summary>
    /// Hides the row
    /// </summary>
    public void Hide();

    /// <summary>
    /// Updates the row based on the new model
    /// </summary>
    /// <param name="newModel">The new model T</param>
    public void UpdateRow(T newModel);
}
