using Nop.Core;
using System;

namespace SaljiDalje.Core.Data
{
    
    public class CostumerPictureAttachmentMapping : BaseEntity
    {
        public string FileName { get; set; }
        public string FileType { get; set; }
        public long FileSize { get; set; }
        public string Guid { get; set; }
        public bool Deleted { get; set; }
        public DateTime CreatedOn { get; set; }
        
        public byte[] PictureData { get; set; }
        
        public int UserId { get; set; }
    }
}
