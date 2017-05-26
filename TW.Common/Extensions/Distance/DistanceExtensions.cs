namespace TW.Common.Extensions.Distance
{
    using System;
    using SharpDX;
    using EloBuddy;
    using EloBuddy.SDK;

    public static partial class DistanceExtensions
    {
        #region Distance

        public static float Distance(this Obj_AI_Base target1, GameObject target2, bool squared = false)
        {
            return Distance(target1.ServerPosition.To2D(), target2.Position.To2D(), squared);
        }

        public static float Distance(this Obj_AI_Base target, Vector3 pos, bool squared = false)
        {
            return Distance(target.ServerPosition.To2D(), pos.To2D(), squared);
        }

        public static float Distance(this Obj_AI_Base target, Vector2 pos, bool squared = false)
        {
            return Distance(target.ServerPosition.To2D(), pos, squared);
        }

        public static float Distance(this Obj_AI_Base target1, Obj_AI_Base target2, bool squared = false)
        {
            return Distance(target1.ServerPosition.To2D(), target2.ServerPosition.To2D(), squared);
        }

        public static float Distance(this GameObject target1, Obj_AI_Base target2, bool squared = false)
        {
            return Distance(target1.Position.To2D(), target2.ServerPosition.To2D(), squared);
        }

        public static float Distance(this GameObject target, Vector3 pos, bool squared = false)
        {
            return Distance(target.Position.To2D(), pos.To2D(), squared);
        }

        public static float Distance(this GameObject target, Vector2 pos, bool squared = false)
        {
            return Distance(target.Position.To2D(), pos, squared);
        }

        public static float Distance(this GameObject target1, GameObject target2, bool squared = false)
        {
            return Distance(target1.Position.To2D(), target2.Position.To2D(), squared);
        }

        public static float Distance(this Vector3 pos, Obj_AI_Base target, bool squared = false)
        {
            return Distance(pos.To2D(), target.ServerPosition.To2D(), squared);
        }

        public static float Distance(this Vector3 pos, GameObject target, bool squared = false)
        {
            return Distance(pos.To2D(), target.Position.To2D(), squared);
        }

        public static float Distance(this Vector3 pos1, Vector2 pos2, bool squared = false)
        {
            return Distance(pos1.To2D(), pos2, squared);
        }

        public static float Distance(this Vector3 pos1, Vector3 pos2, bool squared = false)
        {
            return Distance(pos1.To2D(), pos2.To2D(), squared);
        }

        public static float Distance(this Vector2 pos, Obj_AI_Base target, bool squared = false)
        {
            return Distance(pos, target.ServerPosition.To2D(), squared);
        }

        public static float Distance(this Vector2 pos, GameObject target, bool squared = false)
        {
            return Distance(pos, target.Position.To2D(), squared);
        }

        public static float Distance(this Vector2 pos1, Vector3 pos2, bool squared = false)
        {
            return Distance(pos1, pos2.To2D(), squared);
        }

        public static float Distance(this Vector2 pos1, Vector2 pos2, bool squared = false)
        {
            if (squared)
            {
                return Vector2.DistanceSquared(pos1, pos2);
            }
            else
            {
                return Vector2.Distance(pos1, pos2);
            }
        }

        public static float Distance(this Vector2 point, Vector2 segmentStart, Vector2 segmentEnd, bool squared = false)
        {
            var a =
                Math.Abs((segmentEnd.Y - segmentStart.Y) * point.X - (segmentEnd.X - segmentStart.X) * point.Y +
                         segmentEnd.X * segmentStart.Y - segmentEnd.Y * segmentStart.X);
            return (squared ? a.Pow() : a) / segmentStart.Distance(segmentEnd, squared);
        }

        public static float Distance(this Vector3 point, Vector2 segmentStart, Vector2 segmentEnd, bool squared = false)
        {
            return point.To2D().Distance(segmentStart, segmentEnd, squared);
        }

        public static float DistanceSquared(this Vector2 pos1, Vector2 pos2)
        {
            return Vector2.DistanceSquared(pos1, pos2);
        }

        public static float DistanceSquared(this Vector3 pos1, Vector3 pos2)
        {
            return DistanceSquared(pos1.To2D(), pos2.To2D());
        }

        public static float DistanceSquared(this Vector2 pos1, Vector3 pos2)
        {
            return DistanceSquared(pos1, pos2.To2D());
        }

        public static float DistanceSquared(this Vector3 pos1, Vector2 pos2)
        {
            return DistanceSquared(pos1.To2D(), pos2);
        }

        public static bool IsInRange(this Vector2 source, Vector2 target, float range)
        {
            return source.Distance(target, true) < range.Pow();
        }

        public static bool IsInRange(this Vector2 source, Vector3 target, float range)
        {
            return IsInRange(source, target.To2D(), range);
        }

        public static bool IsInRange(this Vector2 source, GameObject target, float range)
        {
            return IsInRange(source, target.Position.To2D(), range);
        }

        public static bool IsInRange(this Vector2 source, Obj_AI_Base target, float range)
        {
            return IsInRange(source, target.ServerPosition.To2D(), range);
        }

        public static bool IsInRange(this Vector3 source, Vector2 target, float range)
        {
            return IsInRange(source.To2D(), target, range);
        }

        public static bool IsInRange(this Vector3 source, Vector3 target, float range)
        {
            return IsInRange(source.To2D(), target, range);
        }

        public static bool IsInRange(this Vector3 source, GameObject target, float range)
        {
            return IsInRange(source.To2D(), target, range);
        }

        public static bool IsInRange(this Vector3 source, Obj_AI_Base target, float range)
        {
            return IsInRange(source.To2D(), target, range);
        }

        public static bool IsInRange(this GameObject source, Vector2 target, float range)
        {
            return IsInRange(source.Position.To2D(), target, range);
        }

        public static bool IsInRange(this GameObject source, Vector3 target, float range)
        {
            return IsInRange(source.Position.To2D(), target, range);
        }

        public static bool IsInRange(this GameObject source, GameObject target, float range)
        {
            return IsInRange(source.Position.To2D(), target, range);
        }

        public static bool IsInRange(this GameObject source, Obj_AI_Base target, float range)
        {
            return IsInRange(source.Position.To2D(), target, range);
        }

        public static bool IsInRange(this Obj_AI_Base source, Vector2 target, float range)
        {
            return IsInRange(source.ServerPosition.To2D(), target, range);
        }

        public static bool IsInRange(this Obj_AI_Base source, Vector3 target, float range)
        {
            return IsInRange(source.ServerPosition.To2D(), target, range);
        }

        public static bool IsInRange(this Obj_AI_Base source, GameObject target, float range)
        {
            return IsInRange(source.ServerPosition.To2D(), target, range);
        }

        public static bool IsInRange(this Obj_AI_Base source, Obj_AI_Base target, float range)
        {
            return IsInRange(source.ServerPosition.To2D(), target, range);
        }

        #endregion

        #region Vector Conversions

        /// <summary>
        /// Changes Vector2 to Vector3 by setting Z equal to the height of the land at the specified coordinates.
        /// </summary>
        public static Vector3 To3DWorld(this Vector2 vector)
        {
            return new Vector3(vector.X, vector.Y, NavMesh.GetHeightForPosition(vector.X, vector.Y));
        }

        /// <summary> 
        /// Converts a World point to it's Grid equivalent.
        /// </summary>
        public static Vector2 WorldToGrid(this Vector3 vector)
        {
            return WorldToGrid(vector.To2D());
        }

        /// <summary> 
        /// Converts a World point to it's Grid equivalent.
        /// </summary>
        public static Vector2 WorldToGrid(this Vector2 vector)
        {
            return NavMesh.WorldToGrid(vector.X, vector.Y);
        }

        /// <summary> 
        /// Converts a Grid point to it's World equivalent.
        /// </summary>
        public static Vector3 GridToWorld(this Vector3 vector)
        {
            return GridToWorld(vector.To2D());
        }

        /// <summary> 
        /// Converts a Grid point to it's World equivalent.
        /// </summary>
        public static Vector3 GridToWorld(this Vector2 vector)
        {
            return NavMesh.GridToWorld((short)vector.X, (short)vector.Y);
        }

        /// <summary> 
        /// Converts a World point to it's Minimap equivalent.
        /// </summary>
        public static Vector2 WorldToMinimap(this Vector3 vector)
        {
            return TacticalMap.WorldToMinimap(vector);
        }

        /// <summary> 
        /// Converts a Minimap point to it's World equivalent.
        /// </summary>
        public static Vector3 MinimapToWorld(this Vector2 vector)
        {
            return TacticalMap.MinimapToWorld(vector.X, vector.Y);
        }

        /// <summary> 
        /// Converts a point to it's NavMeshCell equivalent.
        /// </summary>
        public static NavMeshCell ToNavMeshCell(this Vector3 vector)
        {
            return ToNavMeshCell(vector.To2D());
        }

        /// <summary> 
        /// Converts a point to it's NavMeshCell equivalent.
        /// </summary>
        public static NavMeshCell ToNavMeshCell(this Vector2 vector)
        {
            var gridCoords = vector.WorldToGrid();
            return NavMesh.GetCell((short)gridCoords.X, (short)gridCoords.Y);
        }

        /// <summary> 
        /// Finds the angle between to points and retuns it as a radian.
        /// </summary>
        public static float AngleBetween(this Vector3 vector3, Vector3 toVector3)
        {
            var magnitudeA = Math.Sqrt((vector3.X * vector3.X) + (vector3.Y * vector3.Y) + (vector3.Z * vector3.Z));
            var magnitudeB =
                Math.Sqrt((toVector3.X * toVector3.X) + (toVector3.Y * toVector3.Y) + (toVector3.Z * toVector3.Z));

            var dotProduct = (vector3.X * toVector3.X) + (vector3.Y * toVector3.Y) + (vector3.Z + toVector3.Z);
            return (float)Math.Acos(dotProduct / magnitudeA * magnitudeB);
        }

        #endregion

        #region Vector Extending

        /// <summary> 
        /// Returns a position that runs from the source position to the targets position with the length of the range specified.
        /// </summary>
        public static Vector2 Extend(this Vector3 source, GameObject target, float range)
        {
            return source.To2D().Extend(target.Position.To2D(), range);
        }

        /// <summary> 
        /// Returns a position that runs from the source position to the targets position with the length of the range specified.
        /// </summary>
        public static Vector2 Extend(this Vector3 source, Obj_AI_Base target, float range)
        {
            return source.To2D().Extend(target.ServerPosition.To2D(), range);
        }

        /// <summary> 
        /// Returns a position that runs from the source position to the target position with the length of the range specified.
        /// </summary>
        public static Vector2 Extend(this Vector3 source, Vector3 target, float range)
        {
            return source.To2D().Extend(target.To2D(), range);
        }

        /// <summary> 
        /// Returns a position that runs from the source position to the target position with the length of the range specified.
        /// </summary>
        public static Vector2 Extend(this Vector3 source, Vector2 target, float range)
        {
            return source.To2D().Extend(target, range);
        }

        /// <summary> 
        /// Returns a position that runs from the source position to the targets position with the length of the range specified.
        /// </summary>
        public static Vector2 Extend(this Vector2 source, GameObject target, float range)
        {
            return source.Extend(target.Position.To2D(), range);
        }

        /// <summary> 
        /// Returns a position that runs from the source position to the targets position with the length of the range specified.
        /// </summary>
        public static Vector2 Extend(this Vector2 source, Obj_AI_Base target, float range)
        {
            return source.Extend(target.ServerPosition.To2D(), range);
        }

        /// <summary> 
        /// Returns a position that runs from the source position to the target position with the length of the range specified.
        /// </summary>
        public static Vector2 Extend(this Vector2 source, Vector3 target, float range)
        {
            return source.Extend(target.To2D(), range);
        }

        /// <summary> 
        /// Returns a position that runs from the source position to the target position with the length of the range specified.
        /// </summary>
        public static Vector2 Extend(this Vector2 source, Vector2 target, float range)
        {
            return source + range * (target - source).Normalized();
        }

        /// <summary>
        /// Changes Vector2 to Vector3 by adding the height parameter as it's Z axis.
        /// </summary>
        public static Vector3 To3D(this Vector2 vector, int height = 0)
        {
            return new Vector3(vector.X, vector.Y, height);
        }

        #endregion

        public static Vector3 GetMissileFixedYPosition(this MissileClient target)
        {
            var pos = target.Position;
            return new Vector3(pos.X, pos.Y, pos.Z - 100);
        }
    }
}