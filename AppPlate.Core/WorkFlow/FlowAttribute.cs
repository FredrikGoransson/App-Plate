namespace AppPlate.Core.WorkFlow
{
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class FlowAttribute : System.Attribute
    {
        public bool IsDefault { get; set; }
        public bool IsStartupPoint { get; set; }
    }
}