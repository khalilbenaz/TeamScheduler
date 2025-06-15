namespace TeamScheduler.Core.Enums
{
    public enum MissionType
    {
        Regular,
        OneTime,
        Project,
        Emergency,
        Training,
        Audit,
        Support
    }

    public enum PlanningStatus
    {
        Draft,
        InProgress,
        Validated,
        Modified,
        Cancelled
    }

    public enum SkillCategory
    {
        Technical,
        Functional,
        Language,
        Certification,
        License,
        Other
    }

    public enum LeaveType
    {
        AnnualLeave,
        SickLeave,
        MaternityLeave,
        PaternityLeave,
        PersonalLeave,
        Training,
        RTT,
        ExceptionalLeave,
        Other
    }

    public enum LeaveStatus
    {
        Pending,
        Approved,
        Rejected,
        Cancelled
    }

    public enum AssignmentType
    {
        NewTeam,
        TeamChange,
        ClientAssignment,
        MissionEnd,
        Leave,
        Training,
        Other
    }

    public enum PresenceStatus
    {
        Present,
        Absent,
        OnLeave,
        PartialDay,
        Remote
    }
}
