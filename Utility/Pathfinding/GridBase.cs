namespace Petrosik
{
    namespace Pathfinding
    {
        using Enums;
        using System;
        using System.Collections.Generic;
        using System.Collections.Immutable;
        using System.Drawing;
        using System.Drawing.Drawing2D;
        using System.Linq;
        using System.Text.Json;
        using System.Text.Json.Serialization;
        public class GridBase
        {
            /// <summary>
            /// x: 0, y: 1
            /// </summary>
            internal static readonly PointF Up = new(0f, 1f);
            /// <summary>
            /// x: -1, y: 0
            /// </summary>
            internal static readonly PointF Down = new(0f, -1f);
            /// <summary>
            /// x: -1, y: 0
            /// </summary>
            internal static readonly PointF Left = new(-1f, 0f);
            /// <summary>
            /// x: 1, y: 0
            /// </summary>
            internal static readonly PointF Right = new(1f, 0f);
            /// <summary>
            /// How many path units are in 1 world unit
            /// </summary>
            public int GridScale = 20;
            /// <summary>
            /// How far should pathfinding search by default(world units) 
            /// </summary>
            public float SearchRange = 20;
            /// <summary>
            /// After how many path units should the path make new "node" in the output. (0 for no limit)
            /// </summary>
            public int MaxSegmentCount = 40;
            /// <summary>
            /// Size of the current grid
            /// </summary>
            public Size GridSize { get; protected set; }
            public PathOccupancy[,] Grid;
            public ImmutableList<ObstacleObject> LoadedObstacles { get { return loadedObstacles.ToImmutableList(); } }
            private List<ObstacleObject> loadedObstacles = new();

            /// <summary>
            /// 
            /// </summary>
            /// <param name="BaseClearArea">Size of the grid(world units)</param>
            /// <param name="GridScale">How many path units are in 1 world unit</param>
            /// <param name="SearchRange">How far should pathfinding search by default(world units)</param>
            /// <param name="MaxSegmentCount">After how many path units should the path make new "node" in the output. (0 for no limit)</param>
            internal GridBase(GraphicsPath BaseClearArea, int GridScale = 20, float SearchRange = 20f, int MaxSegmentCount = 40)
            {
                this.GridScale = GridScale > 1 ? GridScale : 1;
                this.SearchRange = SearchRange;
                this.MaxSegmentCount = MaxSegmentCount;
                var bounds = BaseClearArea.GetBounds();
                AddObstacle(new(PathOccupancy.Clear, BaseClearArea));
            }
            private GridBase() { }
            private void BuildGrid()
            {
                GridSize = new(loadedObstacles.ConvertAll(x => (int)Math.Ceiling(x.Shape.GetBounds().Width)).OrderByDescending(x => x).First() * this.GridScale,
                               loadedObstacles.ConvertAll(x => (int)Math.Ceiling(x.Shape.GetBounds().Height)).OrderByDescending(x => x).First() * this.GridScale);
                Grid = new PathOccupancy[GridSize.Width, GridSize.Height];
                for (int i = 0; i < loadedObstacles.Count; i++)
                {
                    for (int y = 0; y < GridSize.Height; y++)
                    {
                        for (int x = 0; x < GridSize.Width; x++)
                        {
                            if (loadedObstacles[i].Shape.IsVisible(x, y))
                            {
                                Grid[x, y] = loadedObstacles[i].Type;
                            }
                        }
                    }
                }
            }
            public void AddObstacle(ObstacleObject Obstacle)
            {
                var scalematrix = new Matrix();
                scalematrix.Scale(GridScale, GridScale);
                Obstacle.Shape.Transform(scalematrix);
                loadedObstacles.Add(Obstacle);
                BuildGrid();
            }
            public void RemoveObstacle(ObstacleObject Obstacle)
            {
                loadedObstacles.Remove(Obstacle);
                BuildGrid();
            }
            public void RemoveObstacleAt(int Index)
            {
                loadedObstacles.RemoveAt(Index);
                BuildGrid();
            }
            /// <summary>
            /// Clears all obstacles
            /// <para>Also removes what you set as BaseClearArea when creating this GridBase</para>
            /// </summary>
            public void ClearObstalces()
            {
                loadedObstacles.Clear();
                BuildGrid();
            }
            /// <summary>
            /// Moves obstacle on <paramref name="Index"/> Up or Down in the list based on <paramref name="Offset"/>
            /// </summary>
            /// <param name="Index"></param>
            /// <param name="Offset">+ Moves Down, - Moves Up</param>
            /// <exception cref="IndexOutOfRangeException">Occurs when Index + Offset is outside of range of the obstacle list</exception>
            public void MoveObstalce(int Index, int Offset = 1)
            {
                if (Index + Offset < 0 || Index + Offset > loadedObstacles.Count - 1)
                {
                    throw new IndexOutOfRangeException("Index + Offset is outside of range of the obstacle list");
                }
                (loadedObstacles[Index + Offset], loadedObstacles[Index]) = (loadedObstacles[Index], loadedObstacles[Index + Offset]);
                BuildGrid();
            }
            /// <summary>
            /// Adds single pixel worth obstacle
            /// <para>Use Sparingly, will slow down grid building</para>
            /// </summary>
            /// <param name="X"></param>
            /// <param name="Y"></param>
            /// <param name="Type"></param>
            public void AddSingle(int X, int Y, PathOccupancy Type)
            {
                GraphicsPath path = new GraphicsPath();
                path.AddRectangle(new RectangleF(X, Y, 1f / GridScale, 1f / GridScale));
                AddObstacle(new(Type, path));
            }
            /// <summary>
            /// Returns color interpretation of the tile
            /// </summary>
            /// <param name="Tile"></param>
            /// <returns></returns>
            public Color GetPathColor(PathOccupancy Tile)
            {
                switch (Tile)
                {
                    case PathOccupancy.Blocked:
                        return Color.Red;
                    case PathOccupancy.Clear:
                        return Color.DarkGreen;
                    case PathOccupancy.LowP:
                        return Color.Green;
                    case PathOccupancy.MediumP:
                        return Color.Yellow;
                    case PathOccupancy.HighP:
                        return Color.Magenta;
                    default:
                        return Color.White;
                }
            }
            /// <summary>
            /// Returns position on the grid from world position
            /// </summary>
            /// <param name="Pos"></param>
            /// <returns></returns>
            public PointF GetGridPosition(PointF Pos)
            {
                return new PointF((float)Math.Floor(Pos.X / (1f / GridScale)), (float)Math.Floor(Pos.Y / (1f / GridScale)));
            }
            /// <summary>
            /// Returns position in world from grid position
            /// </summary>
            ///  <param name="Pos"></param>
            /// <returns></returns>
            public PointF GetWorldPosition(PointF Pos)
            {
                return new PointF(Pos.X * (1f / GridScale) + (1f / GridScale / 2), Pos.Y * (1f / GridScale) + (1f / GridScale / 2));
            }
            /// <summary>
            /// Exports Obstacles into Json format
            /// </summary>
            /// <returns></returns>
            public string ExportPaths()
            {
                List<ObstacleObjectExport> l = new();
                foreach (var obstacle in loadedObstacles)
                {
                    l.Add(new(obstacle.Type, obstacle.Shape.PathPoints.Select(x => new float[] { x.X, x.Y }).ToArray(), obstacle.Shape.PathTypes));
                }
                return JsonSerializer.Serialize(l);
            }
            /// <summary>
            /// Inports Obstacles from Json text
            /// </summary>
            /// <param name="text"></param>
            /// <exception cref="ArgumentException">Happens when text is not Json format</exception>
            public void InportPaths(string text)
            {
                var l = JsonSerializer.Deserialize<List<ObstacleObjectExport>>(text);
                if (l == null)
                {
                    throw new ArgumentException($"Inported text is not readable Json format. '{text}'");
                }
                loadedObstacles = new();
                foreach (var obstacle in l)
                {
                    loadedObstacles.Add(new(obstacle.Type, new(obstacle.Points.Select(x => new PointF(x[0], x[1])).ToArray(), obstacle.PointTypes)));
                }
                BuildGrid();
            }
            /// <summary>
            /// Returns List of coordinates for path to target, searchrange is 
            /// </summary>
            /// <param name="From">World's Units</param>
            /// <param name="To">World's Units</param>
            /// <param name="SearchRange">How far to search in world's units (0 for default limit)</param>
            /// <returns></returns>
            public virtual List<PointF>? GetPath(PointF From, PointF To, float SearchRange = 0)
            {
                throw new NotImplementedException("Someone didn't implemented this");
            }
            /// <summary>
            /// Simple Raw Pathing method
            /// </summary>
            /// <param name="From">World's Units</param>
            /// <param name="To">World's Units</param>
            /// <param name="SearchRange">How far to search in world's units (0 for default limit)</param>
            /// <param name="WorldSpace">Should the positions be converted to world positions</param>
            /// <returns></returns>
            public virtual LinkedList<PointF>? GetRawPath(PointF From, PointF To, float SearchRange = 0, bool WorldSpace = true)
            {
                throw new NotImplementedException("Someone didn't implemented this");
            }
        }
        public struct ObstacleObject
        {
            public PathOccupancy Type;
            public GraphicsPath Shape;
            internal ObstacleObject(PathOccupancy Type, GraphicsPath Shape)
            {
                this.Type = Type;
                this.Shape = Shape;
            }
            /// <summary>
            /// Do not use this one, it will just throw a error
            /// </summary>
            /// <exception cref="InvalidOperationException"></exception>
            [Obsolete]
            public ObstacleObject() => throw new InvalidOperationException("Default constructor is not allowed.");
            public override string ToString()
            {
                return $"{Type} | {Shape.PointCount} points, {Shape.GetBounds()}";
            }
            public override int GetHashCode()
            {
                return HashCode.Combine(Type.GetHashCode(), Shape.GetHashCode());
            }
        }

        [Serializable]
        internal struct ObstacleObjectExport
        {
            [JsonInclude] public PathOccupancy Type;
            [JsonInclude] public float[][] Points;
            [JsonInclude] public byte[] PointTypes;
            public ObstacleObjectExport(PathOccupancy Type, float[][] Points, byte[] PointTypes)
            {
                this.Type = Type;
                this.Points = Points;
                this.PointTypes = PointTypes;
            }
            public override string ToString()
            {
                return $"{Type} | {Points.Length}x points";
            }
        }
    }
}


