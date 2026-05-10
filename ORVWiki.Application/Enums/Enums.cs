namespace ORVWiki.Application.Enums;

public enum EntityType
{
    Character,
    Constellation,
    Nebula,
    Dokkaebi,
    DemonKing,
    OuterGod,
    Worldline,
    Fable,
    Stigma,
    Attribute,
    Skill,
    Item,
    Location,
    Scenario,
    Arc,
    Event,
    Concept
}

public enum CharacterStatus
{
    Unknown,
    Alive,
    Dead,
    Transcended
}

public enum Gender
{
    Male,
    Female,
    Unknown
}

public enum ConstellationGrade
{
    Historical,
    Fable,
    Myth,
    StarStream
}

public enum DokkaebiRank
{
    Low,
    Intermediate,
    Great,
    Grand,
    Zen
}

public enum FableGrade
{
    Legendary,
    Mythical,
    Great,
    Divine
}

public enum AttributeRarity
{
    Common,
    Rare,
    Epic,
    Legend,
    Mythic
}

public enum SkillType
{
    Active,
    Passive,
    General
}

public enum ItemGrade
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary,
    Mythic,
    Divine
}

public enum ScenarioType
{
    Main,
    Sub,
    Hidden,
    Bounty,
    Disaster
}

public enum ScenarioDifficulty
{
    F,
    E,
    D,
    C,
    B,
    A,
    S,
    SS,
    SSS,
    Unknown
}

public enum EventImportance
{
    Minor,
    Major,
    Pivotal
}

public enum ConceptImpact
{
    Low,
    Medium,
    High,
    Core
}

public enum CharacterConstellationRel
{
    MainSponsor,
    Patron,
    Subscriber,
    Opposed
}

public enum EventCharacterRole
{
    Participant,
    Observer,
    Victim,
    Perpetrator,
    Mentor
}

public enum ScenarioOutcome
{
    Succeeded,
    Failed,
    Withdrew,
    Pending
}

public enum CommentReactionType
{
    Like,
    Dislike,
    Heart,
    Laugh,
    Sad,
    Star
}

public enum EditSuggestionStatus
{
    Pending,
    Approved,
    Rejected
}

public enum NotificationType
{
    CommentReply,
    EditApproved,
    EditRejected,
    Mention,
    System
}
