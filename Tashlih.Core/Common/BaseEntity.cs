using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tashlih.Core.Common
{
    public abstract class BaseEntity
    {
        public long Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
    public abstract class AuditableEntity : BaseEntity
    {
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public abstract class SoftDeletableEntity : AuditableEntity
    {
        public DateTime? DeletedAt { get; set; }
    }
}
