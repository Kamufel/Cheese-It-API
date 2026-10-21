namespace CheeseIt.Api.Domain;

/// <summary>
/// Production stages of a cheese batch, in the order they are passed through.
/// The numeric order matters: a batch can only move forward to the next stage.
/// </summary>
public enum BatchStatus
{
    Curdling = 0,
    Pressing = 1,
    Brining = 2,
    Maturing = 3,
    ReadyForSale = 4
}
