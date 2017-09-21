using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GW2_Live.Player
{
    public static class MathUtils
    {
        public static float GetDistSqr(float x1, float y1, float x2, float y2)
        {
            float dx = x2 - x1;
            float dy = y2 - y1;
            return dx * dx + dy * dy;
        }

        public static double GetAngleDiff(float xFrom, float yFrom, float xTo, float yTo)
        {
            double aFrom = Math.Atan2(yFrom, xFrom);
            double aTo = Math.Atan2(yTo, xTo);

            double da = aTo - aFrom;
            if (da > Math.PI)
            {
                da -= 2 * Math.PI;
            }
            else if (da < -Math.PI)
            {
                da += 2 * Math.PI;
            }

            return da;
        }
    }
}
