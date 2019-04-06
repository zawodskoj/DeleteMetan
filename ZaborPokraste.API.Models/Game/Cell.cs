namespace ZaborPokraste.API.Models.Game
{
    public class Cell
    {
        public Location Location { get; set; }
        public CellType Type { get; set; }
    }

    public enum CellType
    {
        Empty,
        Rock,
        DangerousArea,
        Pit
    }
}