namespace Helpers
{
    public class ExpandData
    {
        public InfoExpand from { get; set; }
        
        public InfoExpand to { get; set; }
    }

    public class InfoExpand
    {
        public string clientId { get; set; }
        
        public bool isExpand { get; set; }
    }
}