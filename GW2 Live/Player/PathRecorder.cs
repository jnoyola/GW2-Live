using System;
using System.Threading.Tasks;
using GW2_Live.GameInterface;
using GW2_Live.UI;

namespace GW2_Live.Player
{
    class PathRecorder
    {
        private const float Pi = (float)Math.PI;
        private const float MinDistanceSquared = 30.48f * 30.48f;
        private const float MinAngleDelta = Pi / 4;

        private MumbleHandler mumble;
        private MapView mapView;
        private bool shouldStop;

        public PathRecorder(MumbleHandler mumble, MapView mapView)
        {
            this.mumble = mumble;
            this.mapView = mapView;
        }

        public void Record()
        {
            mapView.Plan.RemoveAllRoutePoints();
            Task.Run(RecordPath);
        }

        public void StopRecording()
        {
            shouldStop = true;
        }

        private async Task RecordPath()
        {
            float lastRecordedX = mumble.GetX();
            float lastRecordedY = mumble.GetY();
            float lastRecordedAngle = mumble.GetAngle();

            float lastX = lastRecordedX;
            float lastY = lastRecordedY;
            float lastAngle = lastRecordedAngle;

            mapView.Plan.AddRoutePoint(mumble.GetPercentX(), mumble.GetPercentY());
            mapView.Invalidate();

            while (!shouldStop)
            {
                float x = mumble.GetX();
                float y = mumble.GetY();
                float angle = mumble.GetAngle();

                if (ShouldMark(lastRecordedX, lastRecordedY, lastRecordedAngle, lastX, lastY, lastAngle, x, y, angle))
                {
                    mapView.Plan.AddRoutePoint(mumble.GetPercentX(), mumble.GetPercentY());
                    mapView.Invalidate();

                    lastRecordedX = x;
                    lastRecordedY = y;
                    lastRecordedAngle = angle;
                }

                lastX = x;
                lastY = y;
                lastAngle = angle;

                await Task.Delay(100);
            }
        }

        private bool ShouldMark(float lastRecordedX, float lastRecordedY, float lastRecordedAngle, float lastX, float lastY, float lastAngle, float x, float y, float angle)
        {
            float dX = x - lastRecordedX;
            float dY = y - lastRecordedY;
            float dAngle = (angle - lastRecordedAngle + Pi) % (2 * Pi) - Pi;

            // Mark if we've traveled sufficient distance since the last recorded point.
            bool shouldMark = dX * dX + dY * dY > MinDistanceSquared;

            // Or moved some amount and turned a sufficient angle.
            shouldMark |= (dX != 0 || dY != 0) && Math.Abs(dAngle) > MinAngleDelta;

            // Or stood still here for the first time.
            shouldMark |= x == lastX && y == lastY && (dX != 0 && dY != 0);

            return shouldMark;
        }
    }
}
