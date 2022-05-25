using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TOW_Core.Utilities
{
    /// <summary>
    /// This class is used for storing constants for heroes that don't really have a 
    /// place anywhere else in the code base but are still important for gameplay
    /// mechanics.
    /// </summary>
    public class HeroConstants
    {
        /// <summary>
        /// Heroes under this age (exclusive) will be vampires.
        /// </summary>
        public static readonly int VAMPIRE_MAX_AGE = 26;
    }
}
