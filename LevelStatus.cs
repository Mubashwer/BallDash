using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project {
    public class LevelStatus {
        /// <summary>
        /// Amount of time the current level attempt has been running for
        /// </summary>
        public TimeSpan Time { get; set; }
        /// <summary>
        /// The best time for this level
        /// </summary>
        public TimeSpan? BestTime { get; set; }
        /// <summary>
        /// Whether or not the hint system has been used for this level attempt
        /// </summary>
        public bool HintUsed { get; set; }
        /// <summary>
        /// The number of wall collision suffered during this level attempt
        /// </summary>
        public int Collisions { get; set; }
        /// <summary>
        /// The least number of wall collisions suffered during an attempt of this level
        /// </summary>
        public int? BestCollisions { get; set; }
    }
}
