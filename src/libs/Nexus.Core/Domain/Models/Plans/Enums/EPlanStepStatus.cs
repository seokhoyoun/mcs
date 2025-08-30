namespace Nexus.Core.Domain.Models.Plans.Enums
{
    public enum EPlanStepStatus
    {
        Pending = 0,      // ��� ��
        Dispatched,   // �Ҵ��
        InProgress,   // ���� ��
        Completed,    // �Ϸ�
        Failed,       // ����
        Skipped       // �ǳʶ�
    }
}