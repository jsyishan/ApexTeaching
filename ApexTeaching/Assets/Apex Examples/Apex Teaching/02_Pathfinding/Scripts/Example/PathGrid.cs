﻿namespace Apex.AI.Teaching
{
    using UnityEngine;

    public sealed class PathGrid : MonoBehaviour
    {
        public static PathGrid instance;

        private readonly Pathfinder _pathfinder = new Pathfinder();

        public int cellSize = 2;
        public Vector2 gridSize = Vector2.one * 100f;
        public LayerMask obstaclesLayer;
        public bool allowCornerCutting;

        private PathCell[,] _cells;

        public PathCell[,] cells
        {
            get { return _cells; }
        }

        private void OnEnable()
        {
            if (instance != null)
            {
                Debug.LogWarning(this.ToString() + " another PathGrid has already registered, destroying the old one");
                Destroy(instance, 0.01f);
            }

            instance = this;

            var startX = Mathf.CeilToInt(gridSize.x * -0.5f);
            var xSteps = Mathf.FloorToInt(gridSize.x / cellSize);

            var startZ = Mathf.CeilToInt(gridSize.y * -0.5f);
            var zSteps = Mathf.FloorToInt(gridSize.y / cellSize);

            _cells = new PathCell[xSteps, zSteps];

            for (int x = 0; x < xSteps; x++)
            {
                for (int z = 0; z < zSteps; z++)
                {
                    var xPos = startX + (x * this.cellSize);
                    var zPos = startZ + (z * this.cellSize);
                    _cells[x, z] = new PathCell(new Vector3(xPos, 0f, zPos), this.cellSize, x, z);
                }
            }

            var colliders = FindObjectsOfType<Collider>();
            for (int i = 0; i < colliders.Length; i++)
            {
                var collider = colliders[i];
                var layer = 1 << collider.gameObject.layer;
                if ((obstaclesLayer & layer) == 0)
                {
                    colliders[i] = null;
                    continue;
                }

                var cell = GetCell(collider.transform.position);
                if (cell != null && !cell.blocked)
                {
                    cell.blocked = true;
                }
            }

            for (int x = 0; x < xSteps; x++)
            {
                for (int z = 0; z < zSteps; z++)
                {
                    var cell = _cells[x, z];
                    if (cell.blocked)
                    {
                        continue;
                    }

                    var cellBounds = cell.bounds;
                    for (int i = 0; i < colliders.Length; i++)
                    {
                        var coll = colliders[i];
                        if (coll == null)
                        {
                            continue;
                        }

                        if (cellBounds.Intersects(coll.bounds))
                        {
                            cell.blocked = true;
                            break;
                        }
                    }
                }
            }

            IdentifyCellNeighbours();
        }

        private void IdentifyCellNeighbours()
        {
            var xLength = _cells.GetLength(0);
            var zLength = _cells.GetLength(1);
            for (int x = 0; x < xLength; x++)
            {
                for (int z = 0; z < zLength; z++)
                {
                    var cell = _cells[x, z];
                    var xi = cell.xIndex;
                    var zi = cell.zIndex;

                    if (xi > 0)
                    {
                        cell.AddNeighbour(_cells[xi - 1, zi]);
                    }

                    if (xi < xLength - 1)
                    {
                        cell.AddNeighbour(_cells[xi + 1, zi]);
                    }

                    if (zi > 0)
                    {
                        cell.AddNeighbour(_cells[xi, zi - 1]);
                    }

                    if (zi < zLength - 1)
                    {
                        cell.AddNeighbour(_cells[xi, zi + 1]);
                    }

                    if (!allowCornerCutting)
                    {
                        continue;
                    }

                    if (xi > 0 && zi > 0)
                    {
                        cell.AddNeighbour(_cells[xi - 1, zi - 1]);
                    }

                    if (xi > 0 && zi < zLength - 1)
                    {
                        cell.AddNeighbour(_cells[x - 1, zi + 1]);
                    }

                    if (xi < xLength - 1 && zi > 0)
                    {
                        cell.AddNeighbour(_cells[x + 1, zi - 1]);
                    }

                    if (xi < xLength - 1 && zi < zLength - 1)
                    {
                        cell.AddNeighbour(_cells[x + 1, zi + 1]);
                    }
                }
            }
        }

        public PathCell GetCell(Vector3 position)
        {
            var xLength = _cells.GetLength(0);
            var zLength = _cells.GetLength(1);
            for (int x = 0; x < xLength; x++)
            {
                for (int z = 0; z < zLength; z++)
                {
                    var cell = _cells[x, z];
                    if (cell.Contains(new Vector3(position.x, cell.position.y, position.z)))
                    {
                        return cell;
                    }
                }
            }

            return null;
        }

        public PathCell GetNearestWalkableCell(Vector3 position)
        {
            var shortest = float.MaxValue;
            PathCell closest = null;

            var xLength = _cells.GetLength(0);
            var zLength = _cells.GetLength(1);
            for (int x = 0; x < xLength; x++)
            {
                for (int z = 0; z < zLength; z++)
                {
                    var cell = _cells[x, z];
                    if (cell.blocked)
                    {
                        continue;
                    }

                    var distance = (cell.position - position).sqrMagnitude;
                    if (distance < shortest)
                    {
                        shortest = distance;
                        closest = cell;
                    }
                }
            }

            return closest;
        }

        public Path FindPath(Vector3 start, Vector3 destination)
        {
            return _pathfinder.FindPath(this, start, destination);
        }
    }
}