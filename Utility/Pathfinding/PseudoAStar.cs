namespace Petrosik
{
    namespace Pathfinding
    {
        using Enums;
        using Petrosik.Utility;
        using System;
        using System.Collections.Generic;
        using System.Drawing;
        using System.Drawing.Drawing2D;

        /// <summary>
        /// Modified A* algorithm, faster in some cases but unstable and sometimes doesn't return any path. Good for open space not mazes
        /// <para>Cardinal directions only, no diagonals</para>
        /// <para>Desn't support pathfinding for tile traversability</para>
        /// </summary>
        public class PseudoAStar : GridBase
        {
            /// <summary> </summary>
            /// <param name="BaseClearArea">Size of the grid(world units)</param>
            /// <param name="GridScale">How many path units are in 1 world unit</param>
            /// <param name="SearchRange">How far should pathfinding search by default(world units)</param>
            /// <param name="MaxSegmentCount">After how many path units should the path make new "node" in the output. (0 for no limit)</param>
            internal PseudoAStar(GraphicsPath BaseClearArea, int GridScale = 20, float SearchRange = 20f, int MaxSegmentCount = 40) : base(BaseClearArea, GridScale, SearchRange, MaxSegmentCount)
            {
            }
            /// <summary>
            /// Checks path in direct line if it's acessible
            /// </summary>
            /// <param name="from"></param>
            /// <param name="to"></param>
            /// <returns></returns>
            public bool CheckDirectPath(PointF from, PointF to)
            {
                var from1 = GetGridPosition(from);
                var to1 = GetGridPosition(to);
                float distance = from1.Distance(to1);
                var direction = from1.Direction(to1);
                try
                {
                    for (float j = 0; j < distance; j += 1f / GridScale)
                    {
                        var destination1 = new PointF(to1.X + direction.X * j, to1.Y + direction.Y * j);
                        destination1 = new PointF(destination1.X, destination1.Y).Round(0);
                        if (Grid[(int)destination1.X, (int)destination1.Y] == PathOccupancy.Blocked)
                        {
                            return false;
                        }
                    }
                }
                catch
                {
                    return false;
                }
                return true;
            }
            public override List<PointF>? GetPath(PointF from, PointF to, float searchrange = 0)
            {
                if (searchrange == 0)
                {
                    searchrange = SearchRange * GridScale;
                }
                else
                {
                    searchrange *= GridScale;
                }
                var gridto = GetGridPosition(to);
                if (Grid == null || gridto.X > Grid.GetLength(0) || gridto.Y > Grid.GetLength(1) || from.Distance(to) > searchrange / GridScale || Grid[(int)gridto.X, (int)gridto.Y] == PathOccupancy.Blocked)
                {
                    Utility.ConsoleLog("Grid is null", InfoType.Error);
                    return new();
                }
                var path = new List<PointF>();
                var rawPath = GetRawPath(GetGridPosition(from), gridto, searchrange);
                if (rawPath == null || rawPath.Count == 0)
                {
                    return path;
                }

                var currentpos = rawPath.First;
                var nextpos = rawPath.First;
                var i = 0;

                while (true)
                {
                    //shifting to next on path
                    nextpos = nextpos.Next;
                    if (rawPath.Count == 1 || rawPath.Last.Value == nextpos.Value)
                    {
                        path.Add(GetWorldPosition(rawPath.Last.Value));
                        break;
                    }
                    //checking if direct path is accesible
                    float distance = currentpos.Value.Distance(nextpos.Value);
                    var direction = nextpos.Value.Direction(currentpos.Value);
                    for (float j = 0; j < distance; j += 0.5f)
                    {
                        var destination1 = new PointF(currentpos.Value.X + direction.X * j, currentpos.Value.Y + direction.Y * j).Round(0);
                        if (Grid[(int)destination1.X, (int)destination1.Y] == PathOccupancy.Blocked)
                        {
                            currentpos = nextpos;
                            path.Add(GetWorldPosition(currentpos.Value));
                            break;
                        }
                    }
                    //segmenting the path
                    if (i > MaxSegmentCount && MaxSegmentCount != 0)
                    {
                        currentpos = nextpos;
                        path.Add(GetWorldPosition(currentpos.Value));
                        i = 0;
                    }

                    i++;
                }
                return path;
            }
            /// <summary>
            /// Do not use this! use GetPath instead. this is method for testing
            /// </summary>
            /// <param name="from"></param>
            /// <param name="to"></param>
            /// <param name="searchrange"></param>
            /// <returns></returns>
            [Obsolete("This method is for testing, use GetPath instead")]
            internal PathOccupancy[,] GetRawPathT(PointF from, PointF to, float searchrange, out int[,] numb)
            {
                numb = null;
                var result = new PathOccupancy[Grid.GetLength(0), Grid.GetLength(1)];
                if (from.X >= Grid.GetLength(0) || from.X < 0 || from.Y >= Grid.GetLength(1) || from.Y < 0 || to.X >= Grid.GetLength(0) || to.X < 0 || to.Y >= Grid.GetLength(1) || to.Y < 0)
                {
                    Utility.ConsoleLog("outisde of grid", InfoType.Error);
                    return result;
                }
                for (int y = 0; y < result.GetLength(1); y++)
                {
                    for (int x = 0; x < result.GetLength(0); x++)
                    {
                        result[x, y] = Grid[x, y];
                    }
                }

                var numberpath = new int[result.GetLength(0), result.GetLength(1)];
                if (Grid[(int)to.X, (int)to.Y] == PathOccupancy.Blocked)
                {
                    Utility.ConsoleLog($"Target is on blocked tile", InfoType.Warn);
                    return result;
                }
                PointF currentpos = new();
                List<PointF> posiblepos = new() { from };
                int i;
                int pathdiff = 0;
                var maxmoves = searchrange + (searchrange / 2);
                for (i = 1; i < maxmoves + 1; i++)
                {
                    currentpos = posiblepos[0];
                    pathdiff += (int)Grid[(int)currentpos.X, (int)currentpos.Y] * 1;
                    numberpath[(int)currentpos.X, (int)currentpos.Y] = pathdiff;
                    if (currentpos.Distance(to) < 1f / GridScale)
                    {
                        break;
                    }
                    if (!PrefDirCheck(currentpos, to, numberpath, out var moveto))
                    {
                        if (currentpos.X - 1 >= 0 && Grid[(int)currentpos.X - 1, (int)currentpos.Y] != PathOccupancy.Blocked && numberpath[(int)currentpos.X - 1, (int)currentpos.Y] == 0)
                        {
                            posiblepos.Add(new((int)currentpos.X - 1, (int)currentpos.Y));
                        }
                        if (currentpos.Y - 1 >= 0 && Grid[(int)currentpos.X, (int)currentpos.Y - 1] != PathOccupancy.Blocked && numberpath[(int)currentpos.X, (int)currentpos.Y - 1] == 0)
                        {
                            posiblepos.Add(new((int)currentpos.X, (int)currentpos.Y - 1));
                        }
                        if (currentpos.X + 1 < Grid.GetLength(0) && Grid[(int)currentpos.X + 1, (int)currentpos.Y] != PathOccupancy.Blocked && numberpath[(int)currentpos.X + 1, (int)currentpos.Y] == 0)
                        {
                            posiblepos.Add(new((int)currentpos.X + 1, (int)currentpos.Y));
                        }
                        if (currentpos.Y + 1 < Grid.GetLength(1) && Grid[(int)currentpos.X, (int)currentpos.Y + 1] != PathOccupancy.Blocked && numberpath[(int)currentpos.X, (int)currentpos.Y + 1] == 0)
                        {
                            posiblepos.Add(new((int)currentpos.X, (int)currentpos.Y + 1));
                        }
                    }
                    else if (!posiblepos.Contains(new(currentpos.X + moveto.X, currentpos.Y + moveto.Y)))
                    {
                        posiblepos.Insert(0, new(currentpos.X + moveto.X, currentpos.Y + moveto.Y));
                    }
                    posiblepos.Remove(currentpos);
                }

                if (i >= maxmoves)
                {
                    Utility.ConsoleLog($"(T)Ran out of moves ({from} - {to} [{Grid[(int)from.X, (int)from.Y]},{Grid[(int)to.X, (int)to.Y]}], Range: {searchrange} ,CurrentPos: {currentpos}) M:[{i},{maxmoves}]", Petrosik.Enums.InfoType.Warn);
                }

                numb = new int[result.GetLength(0), result.GetLength(1)];
                for (int y = 0; y < result.GetLength(1); y++)
                {
                    for (int x = 0; x < result.GetLength(0); x++)
                    {
                        numb[x, y] = numberpath[x, y];
                    }
                }

                for (int j = 0; j < maxmoves; j++)
                {
                    int y = (int)currentpos.Y;
                    int x = (int)currentpos.X;
                    int smallestAdjacent = numberpath[x, y];
                    PointF dir = new();
                    if (y - 1 >= 0 && numberpath[x, y - 1] != 0)
                    {
                        if (numberpath[x, y - 1] < smallestAdjacent)
                        {
                            smallestAdjacent = numberpath[x, y - 1];
                            dir = Down;
                        }
                    }
                    if (y + 1 < numberpath.GetLength(1) && numberpath[x, y + 1] != 0)
                    {
                        if (numberpath[x, y + 1] < smallestAdjacent)
                        {
                            smallestAdjacent = numberpath[x, y + 1];
                            dir = Up;
                        }
                    }
                    if (x - 1 >= 0 && numberpath[x - 1, y] != 0)
                    {
                        if (numberpath[x - 1, y] < smallestAdjacent)
                        {
                            smallestAdjacent = numberpath[x - 1, y];
                            dir = Left;
                        }
                    }
                    if (x + 1 < numberpath.GetLength(0) && numberpath[x + 1, y] != 0)
                    {
                        if (numberpath[x + 1, y] < smallestAdjacent)
                        {
                            dir = Right;
                        }
                    }

                    result[x, y] = PathOccupancy.Path;
                    numberpath[x, y] = 0;
                    if (currentpos == from || dir == new PointF())
                    {
                        break;
                    }
                    currentpos = new(currentpos.X + dir.X, currentpos.Y + dir.Y);
                }

                return result;
            }
            public override LinkedList<PointF>? GetRawPath(PointF from, PointF to, float searchrange, bool WorldSpace = true)
            {
                LinkedList<PointF> result = new();
                var numberpath = new int[Grid.GetLength(0), Grid.GetLength(1)];
                from = GetGridPosition(from);
                to = GetGridPosition(to);
                if (from.X < 0 || from.Y < 0 || from.X > GridSize.Width || from.Y > GridSize.Height)
                {
                    Utility.ConsoleLog($"Pathfinding couln't start due 'From'({from}) point being outside of the grid", InfoType.Error);
                    return null;
                }
                else if (to.X < 0 || to.Y < 0 || to.X > GridSize.Width || to.Y > GridSize.Width)
                {
                    Utility.ConsoleLog($"Pathfinding couln't start due 'From'({to}) point being outside of the grid", InfoType.Error);
                    return null;
                }
                if (Grid[(int)to.X, (int)to.Y] == PathOccupancy.Blocked || Grid[(int)from.X, (int)from.Y] == PathOccupancy.Blocked)
                {
                    return null;
                }
                PointF currentpos = new();
                List<PointF> posiblepos = new() { from };
                int i;
                int pathdiff = 0;
                var maxmoves = searchrange + (searchrange / 2);
                for (i = 1; i < maxmoves + 1; i++)
                {
                    if (posiblepos.Count < 1)
                    {
                        break;
                    }
                    currentpos = posiblepos[0];
                    pathdiff += (int)Grid[(int)currentpos.X, (int)currentpos.Y] * 1;
                    numberpath[(int)currentpos.X, (int)currentpos.Y] = pathdiff;
                    if (currentpos.Distance(to) < 1f / GridScale)
                    {
                        break;
                    }
                    if (!PrefDirCheck(currentpos, to, numberpath, out var moveto))
                    {
                        if (currentpos.X - 1 >= 0 && Grid[(int)currentpos.X - 1, (int)currentpos.Y] != PathOccupancy.Blocked && numberpath[(int)currentpos.X - 1, (int)currentpos.Y] == 0)
                        {
                            posiblepos.Add(new((int)currentpos.X - 1, (int)currentpos.Y));
                        }
                        if (currentpos.Y - 1 >= 0 && Grid[(int)currentpos.X, (int)currentpos.Y - 1] != PathOccupancy.Blocked && numberpath[(int)currentpos.X, (int)currentpos.Y - 1] == 0)
                        {
                            posiblepos.Add(new((int)currentpos.X, (int)currentpos.Y - 1));
                        }
                        if (currentpos.X + 1 < Grid.GetLength(0) && Grid[(int)currentpos.X + 1, (int)currentpos.Y] != PathOccupancy.Blocked && numberpath[(int)currentpos.X + 1, (int)currentpos.Y] == 0)
                        {
                            posiblepos.Add(new((int)currentpos.X + 1, (int)currentpos.Y));
                        }
                        if (currentpos.Y + 1 < Grid.GetLength(1) && Grid[(int)currentpos.X, (int)currentpos.Y + 1] != PathOccupancy.Blocked && numberpath[(int)currentpos.X, (int)currentpos.Y + 1] == 0)
                        {
                            posiblepos.Add(new((int)currentpos.X, (int)currentpos.Y + 1));
                        }
                    }
                    else if (!posiblepos.Contains(new(currentpos.X + moveto.X, currentpos.Y + moveto.Y)))
                    {
                        posiblepos.Insert(0, new(currentpos.X + moveto.X, currentpos.Y + moveto.Y));
                    }
                    posiblepos.Remove(currentpos);
                }

                if (i >= maxmoves)
                {
                    Utility.ConsoleLog($"Ran out of moves ({from} - {to} [{Grid[(int)from.X, (int)from.Y]},{Grid[(int)to.X, (int)to.Y]}], Range: {searchrange} ,CurrentPos: {currentpos}) M:[{i},{maxmoves}]", Petrosik.Enums.InfoType.Warn);
                }

                for (int j = 0; j < maxmoves; j++)
                {
                    int y = (int)currentpos.Y;
                    int x = (int)currentpos.X;
                    int smallestAdjacent = numberpath[x, y];
                    PointF dir = new();
                    if (y - 1 >= 0 && numberpath[x, y - 1] != 0)
                    {
                        if (numberpath[x, y - 1] < smallestAdjacent)
                        {
                            smallestAdjacent = numberpath[x, y - 1];
                            dir = Down;
                        }
                    }
                    if (y + 1 < numberpath.GetLength(1) && numberpath[x, y + 1] != 0)
                    {
                        if (numberpath[x, y + 1] < smallestAdjacent)
                        {
                            smallestAdjacent = numberpath[x, y + 1];
                            dir = Up;
                        }
                    }
                    if (x - 1 >= 0 && numberpath[x - 1, y] != 0)
                    {
                        if (numberpath[x - 1, y] < smallestAdjacent)
                        {
                            smallestAdjacent = numberpath[x - 1, y];
                            dir = Left;
                        }
                    }
                    if (x + 1 < numberpath.GetLength(0) && numberpath[x + 1, y] != 0)
                    {
                        if (numberpath[x + 1, y] < smallestAdjacent)
                        {
                            dir = Right;
                        }
                    }
                    if (WorldSpace)
                    {
                        result.AddFirst(GetWorldPosition(new PointF(x, y)));
                    }
                    else
                    {
                        result.AddFirst(new PointF(x + 0.5f, y + 0.5f));
                    }
                    numberpath[x, y] = 0;
                    if (currentpos == from || dir == new PointF())
                    {
                        break;
                    }
                    currentpos = new(currentpos.X + dir.X, currentpos.Y + dir.Y);
                }

                return result;
            }
            public bool PrefDirCheck(PointF currentpos, PointF to, int[,] numberpath, out PointF prefdir)
            {
                var dir = currentpos.Direction(to);
                prefdir = new();
                if (dir.Distance(Right) <= 1f && currentpos.X - 1 >= 0 && Grid[(int)currentpos.X - 1, (int)currentpos.Y] != PathOccupancy.Blocked && numberpath[(int)currentpos.X - 1, (int)currentpos.Y] == 0)
                {
                    prefdir = Left;
                    return true;
                }
                else if (dir.Distance(Up) <= 1f && currentpos.Y - 1 >= 0 && Grid[(int)currentpos.X, (int)currentpos.Y - 1] != PathOccupancy.Blocked && numberpath[(int)currentpos.X, (int)currentpos.Y - 1] == 0)
                {
                    prefdir = Down;
                    return true;
                }
                else if (dir.Distance(Left) < 1f && currentpos.X + 1 < Grid.GetLength(0) && Grid[(int)currentpos.X + 1, (int)currentpos.Y] != PathOccupancy.Blocked && numberpath[(int)currentpos.X + 1, (int)currentpos.Y] == 0)
                {
                    prefdir = Right;
                    return true;
                }
                else if (dir.Distance(Down) < 1f && currentpos.Y + 1 < Grid.GetLength(1) && Grid[(int)currentpos.X, (int)currentpos.Y + 1] != PathOccupancy.Blocked && numberpath[(int)currentpos.X, (int)currentpos.Y + 1] == 0)
                {
                    prefdir = Up;
                    return true;
                }
                return false;
            }
        }
    }
}