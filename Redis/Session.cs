using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redis
{
    public partial class Session
    {
        public int Id { get; set; }
        public int UserType { get; set; }
        public int UserId { get; set; }
        public string Token { get; set; }
        public System.DateTimeOffset StartDate { get; set; }
        public System.DateTimeOffset EndDate { get; set; }
        public bool IsLocked { get; set; }
        public bool IsDeleted { get; set; }
    }
}
