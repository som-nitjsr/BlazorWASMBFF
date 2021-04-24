using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorWASMBFF.Shared
{
   public class UserProfile
    {
        public bool IsAuthenticated { get; set; }
        public string UserName { get; set; }
        public string Role { get; set; }
    }
}
