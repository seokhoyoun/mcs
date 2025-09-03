namespace Nexus.Gateway.Services.Commands
{
    public class CreateLotCommand
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Priority { get; set; } = 0;
        public string Purpose { get; set; }
        public string EvalNo { get; set; }
        public string PartNo { get; set; }
        public int Qty { get; set; } = 0;
        public string Option { get; set; }
        public string Line { get; set; }
        public List<string> CassetteIds { get; set; }
        public List<CreateLotStepCommand> Steps { get; set; }

        public CreateLotCommand(string id, string name, int priority, string purpose, string evalNo, string partNo, int qty, string option, string line, List<string> cassetteIds, List<CreateLotStepCommand> steps)
        {
            Id = id;
            Name = name;
            Priority = priority;
            Purpose = purpose;
            EvalNo = evalNo;
            PartNo = partNo;
            Qty = qty;
            Option = option;
            Line = line;
            CassetteIds = cassetteIds;
            Steps = steps;
        }
    }
}