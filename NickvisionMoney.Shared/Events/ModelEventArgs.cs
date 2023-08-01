namespace NickvisionMoney.Shared.Events;

/// <summary>
/// Event args for wrapping models
/// </summary>
public class ModelEventArgs<T>
{
    /// <summary>
    /// The model of the event
    /// </summary>
    public T Model { get; set; }
    /// <summary>
    /// The position index representing the model
    /// </summary>
    public int? Position { get; set; }
    /// <summary>
    /// Whether or not the model is active
    /// </summary>
    public bool Active { get; set; }

    /// <summary>
    /// Constructs a ModelEventArgs
    /// </summary>
    /// <param name="model">The model of the event</param>
    /// <param name="position">The position index representing the model</param>
    /// <param name="active">Whether or not the model is active</param>
    public ModelEventArgs(T model, int? position = null, bool active = false)
    {
        Model = model;
        Position = position;
        Active = false;
    }
}