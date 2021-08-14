using System.Collections.Generic;

namespace FileManager.Models
{
    internal class WorkMenuList
    {
        public string Name { get; set; }
        public string Descriptions { get; set; }
        public ICollection<WorkSpaceData> File { get; set; }
        
    }
}
