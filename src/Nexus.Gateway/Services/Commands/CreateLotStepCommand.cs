namespace Nexus.Gateway.Services.Commands
{
    public class CreateLotStepCommand
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int LoadingType { get; set; }
        public string Chipset { get; set; } 
        public string DpcType { get; set; }
        public string PGM { get; set; }
        public int PlanPercent { get; set; }

        public CreateLotStepCommand(string id, string name, int loadingType, string chipset, string dpcType, string pGM, int planPercent)
        {
            Id = id;
            Name = name;
            LoadingType = loadingType;
            Chipset = chipset;
            DpcType = dpcType;
            PGM = pGM;
            PlanPercent = planPercent;
        }
    }
}