using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MilitaryGame.Pathfinder
{
    public class Pathfinder : Singleton<Pathfinder>
    {
        [SerializeField] private Tilemap _mainTilemap;
        [SerializeField] private TileBase _groundTileBase;

        
        /// <summary>
        /// A* PATHFINDING ALGORITHM
        /// Finds a path from the start position to the end position using the A* algorithm.
        /// </summary>
        public List<Vector3Int> FindPath(Vector3Int start, Vector3Int end)
        {
            List<Vector3Int> path = new List<Vector3Int>();
            
            // Lists to keep track of open and closed sets of positions during the search.
            List<Vector3Int> openSet = new List<Vector3Int>();
            List<Vector3Int> closedSet = new List<Vector3Int>();
            openSet.Add(start);

            // Dictionary to store the relationship between positions in the path.
            Dictionary<Vector3Int, Vector3Int> cameFrom = new Dictionary<Vector3Int, Vector3Int>();
            
            // Dictionary to store the cost of reaching each position from the start position.
            Dictionary<Vector3Int, int> gScore = new Dictionary<Vector3Int, int>();
            gScore[start] = 0;

            while (openSet.Count > 0)
            {
                Vector3Int current = GetLowestFScore(openSet, gScore, end);

                // If the current position is the target, reconstruct the path and exit the loop.
                if (current == end)
                {
                    path = ReconstructPath(cameFrom, current);
                    break;
                }

                // Remove the current position from the open set and add it to the closed set.
                openSet.Remove(current);
                closedSet.Add(current);

                foreach (Vector3Int neighbor in GetNeighbors(current))
                {
                    // Skip if the neighbor is in the closed set or is not walkable.
                    if (closedSet.Contains(neighbor) || !IsWalkable(neighbor))
                        continue;

                    // Calculate the tentative cost to reach the neighbor from the start.
                    int tentativeGScore = gScore[current] + 1;

                    // Update the path and cost if this path is better than previous ones.
                    if (!gScore.ContainsKey(neighbor) || tentativeGScore < gScore[neighbor])
                    {
                        cameFrom[neighbor] = current;
                        gScore[neighbor] = tentativeGScore;

                        // If the neighbor is not in the open set, add it.
                        if (!openSet.Contains(neighbor))
                            openSet.Add(neighbor);
                    }
                }
            }

            return path;
        }

        /// <summary>
        /// Gets the node with the lowest F score from the open set based on the A* algorithm.
        /// </summary>
        /// <param name="openSet">The list of open nodes to consider.</param>
        /// <param name="gScore">The dictionary containing the G scores for nodes.</param>
        /// <param name="end">The target node for the heuristic estimate.</param>
        /// <returns>The node with the lowest F score.</returns>
        private Vector3Int GetLowestFScore(List<Vector3Int> openSet, Dictionary<Vector3Int, int> gScore, Vector3Int end)
        {
            int lowestFScore = int.MaxValue;
            Vector3Int lowestFScoreNode = Vector3Int.zero;

            foreach (Vector3Int node in openSet)
            {
                int fScore = gScore[node] + HeuristicCostEstimate(node, end);
                if (fScore < lowestFScore)
                {
                    lowestFScore = fScore;
                    lowestFScoreNode = node;
                }
            }

            return lowestFScoreNode;
        }

        /// <summary>
        /// Estimates the heuristic cost from the start node to the end node.
        /// </summary>
        private int HeuristicCostEstimate(Vector3Int start, Vector3Int end)
        {
            return Mathf.Abs(start.x - end.x) + Mathf.Abs(start.y - end.y);
        }

        /// <summary>
        /// Reconstructs the path based on the cameFrom dictionary and the current node.
        /// </summary>
        /// <param name="cameFrom">The dictionary containing the mapping of nodes to their predecessors.</param>
        /// <param name="current">The current node.</param>
        /// <returns>The reconstructed path as a list of nodes.</returns>
        private List<Vector3Int> ReconstructPath(Dictionary<Vector3Int, Vector3Int> cameFrom, Vector3Int current)
        {
            List<Vector3Int> path = new List<Vector3Int>();
            while (cameFrom.ContainsKey(current))
            {
                path.Add(current);
                current = cameFrom[current];
            }
            path.Reverse();
            return path;
        }

        /// <summary>
        /// Checks if the position is walkable based on the main tilemap and the specified ground tile.
        /// </summary>
        /// <param name="position">The position to check for walkability.</param>
        /// <returns>True if the position is walkable, otherwise false.</returns>
        private bool IsWalkable(Vector3Int position)
        {
            TileBase tile = _mainTilemap.GetTile(position);
            return tile == _groundTileBase; 
        }

        /// <summary>
        /// Gets the neighboring positions (up, down, left, right) of the specified position.
        /// </summary>
        /// <param name="position">The reference position.</param>
        /// <returns>A list of neighboring positions.</returns>
        private List<Vector3Int> GetNeighbors(Vector3Int position)
        {
            List<Vector3Int> neighbors = new List<Vector3Int>();
            neighbors.Add(new Vector3Int(position.x + 1, position.y, position.z)); // right
            neighbors.Add(new Vector3Int(position.x - 1, position.y, position.z)); // left
            neighbors.Add(new Vector3Int(position.x, position.y + 1, position.z)); // up
            neighbors.Add(new Vector3Int(position.x, position.y - 1, position.z)); // down
            
            return neighbors;
        }
    }
}