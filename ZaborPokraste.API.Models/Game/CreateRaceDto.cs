namespace ZaborPokraste.API.Models.Game
{
    public class CreateRaceDto
    {
        public CreateRaceDto(string map = null)
        {
            Map = map ?? "test";
        }

        public string Map { get; set; }
    }
}