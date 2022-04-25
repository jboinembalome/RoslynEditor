using RoslynCodeEditLib.Enums;

namespace RoslynCodeEditLib.Models
{
    /// <summary>
    /// This class is just a small utility type class that helps recovering
    /// the Fold Name when creating a new fold from start to end offset.
    /// </summary>
    public class FoldLine
    {
        public int Offset { get; set; }
        public string Name { get; set; }
        public FoldType TypeOfFold { get; set; }
    }
}
