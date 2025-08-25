namespace Nexus.Gateway.Services.Commands
{
    public class CreateLotStepCommand
    {
        public int No { get; set; }
        public int LoadingType { get; set; }
        public string Chipset { get; set; } 
        public string DpcType { get; set; }
        public string PGM { get; set; }
        public int PlanPercent { get; set; }

        public CreateLotStepCommand(int no, int loadingType, string chipset, string dpcType, string pGM, int planPercent)
        {
            No = no;
            LoadingType = loadingType;
            Chipset = chipset;
            DpcType = dpcType;
            PGM = pGM;
            PlanPercent = planPercent;
        }
    }
}