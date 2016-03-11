﻿namespace Apex.AI.Teaching
{
    using UnityEngine;

    public sealed class MyGrid : Grid
    {
        private readonly MyPathfinder _pathfinder = new MyPathfinder();

        public override Path FindPath(Vector3 start, Vector3 destination)
        {
            return _pathfinder.FindPath(this, start, destination);
        }
    }
}