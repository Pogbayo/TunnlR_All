
namespace TunnlR.Domain
{
    public abstract class BaseEntity
    {
        public Guid Id { get; set; } = default!;

        public bool IsCreated { get; set; } = true;           
        public bool IsDeleted { get; set; } = false;        

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }             
        public Guid? CreatedBy { get; set; }                  
        public Guid? UpdatedBy { get; set; }                 

        public void MarkAsDeleted() => IsDeleted = true;
    }
}
