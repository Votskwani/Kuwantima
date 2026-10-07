namespace Kuwantima.Sandbox.Models
{
    /// <summary>Sample row for the DataGrid page — same fleet-tracking flavor as the Inputs/Toggles demo content.</summary>
    public sealed record Vehicle(string Id, string Site, string Status, int FuelPercent);
}
