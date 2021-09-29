using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minimalist.Reactive.Concurrency
{
    public static class Scheduler
    {
        /// <summary>
        /// Normalizes the specified <see cref="TimeSpan"/> value to a positive value.
        /// </summary>
        /// <param name="timeSpan">The <see cref="TimeSpan"/> value to normalize.</param>
        /// <returns>The specified TimeSpan value if it is zero or positive; otherwise, <see cref="TimeSpan.Zero"/>.</returns>
        public static TimeSpan Normalize(TimeSpan timeSpan) => timeSpan.Ticks < 0 ? TimeSpan.Zero : timeSpan;
    }
}
