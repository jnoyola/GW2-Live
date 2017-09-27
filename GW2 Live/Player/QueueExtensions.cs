using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GW2_Live.Player
{
    static class QueueExtensions
    {
        public static T PeekOrDefault<T>(this Queue<T> queue)
        {
            if (queue.Count == 0)
            {
                return default(T);
            }

            return queue.Peek();
        }
    }
}
