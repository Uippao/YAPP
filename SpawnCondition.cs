namespace YAPP
{
    public class Types
    {
        public enum SpawnConditionType
        {
            None,
            PlayerCountAtLeast,
            PlayerCountAtMost
        }
    }
    
    public class SpawnCondition
    {
        public Types.SpawnConditionType Type { get; set; } = Types.SpawnConditionType.None;

        public int? IntValue { get; set; }
        public float? FloatValue { get; set; }
        public string StringValue { get; set; }
        public bool? BoolValue { get; set; }
        
        public ResolvedCondition Resolve()
        {
            return new ResolvedCondition
            {
                Type = Type,
                IntValue = IntValue ?? 0,
                FloatValue = FloatValue ?? 0f,
                StringValue = StringValue ?? string.Empty,
                BoolValue = BoolValue ?? false
            };
        }
    }
}