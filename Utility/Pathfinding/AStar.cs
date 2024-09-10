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
        /// Slightly modifed A*
        /// <para>Cardinal directions only, no diagonals</para>
        /// <para>Supports pathfinding for tile traversability</para>
        /// </summary>
        public class AStar : GridBase
        {
            /// <summary> </summary>
            /// <param name="BaseClearArea">Size of the grid(world units)</param>
            /// <param name="GridScale">How many path units are in 1 world unit</param>
            /// <param name="SearchRange">How far should pathfinding search by default(world units)</param>
            /// <param name="MaxSegmentCount">After how many path units should the path make new "node" in the output. (0 for no limit)</param>
            public AStar(GraphicsPath BaseClearArea, int GridScale = 20, float SearchRange = 20f, int MaxSegmentCount = 40) : base(BaseClearArea, GridScale, SearchRange, MaxSegmentCount)
            {
            }

            public override List<PointF>? GetPath(PointF From, PointF To, float SearchRange = 0)
            {
                var rawPath = GetRawPath(From, To, SearchRange, false);
                if (rawPath == null)
                {
                    return null;
                }

                var path = new List<PointF>();
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
                    if (!CheckDirectLine(currentpos.Value, nextpos.Value))
                    {
                        currentpos = nextpos;
                        path.Add(GetWorldPosition(currentpos.Value));
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
            public override LinkedList<PointF>? GetRawPath(PointF From, PointF To, float SearchRange = 0, bool WorldSpace = true)
            {
                From = GetGridPosition(From);
                To = GetGridPosition(To);
                if (From.X < 0 || From.Y < 0 || From.X > GridSize.Width || From.Y > GridSize.Height)
                {
                    Utility.ConsoleLog($"Pathfinding couln't start due 'From'({From}) point being outside of the grid", InfoType.Error);
                    return null;
                }
                else if (To.X < 0 || To.Y < 0 || To.X > GridSize.Width || To.Y > GridSize.Width)
                {
                    Utility.ConsoleLog($"Pathfinding couln't start due 'From'({To}) point being outside of the grid", InfoType.Error);
                    return null;
                }
                if (Grid[(int)From.X, (int)From.Y] == PathOccupancy.Blocked || Grid[(int)To.X, (int)To.Y] == PathOccupancy.Blocked)
                {
                    Utility.ConsoleLog($"{(Grid[(int)From.X, (int)From.Y] == PathOccupancy.Blocked ? $"Pathfinding couldn't start due to the 'From' point being inacessible [{From.X},{From.Y}] = {Grid[(int)From.X, (int)From.Y]}" : $"Pathfinding couldn't start due to the 'To' point being inacessible [{To.X},{To.Y}] = {Grid[(int)To.X, (int)To.Y]}")}", Petrosik.Enums.InfoType.Error);
                    return null;
                }
                if (SearchRange == 0)
                {
                    SearchRange = this.SearchRange * GridScale;
                }
                else
                {
                    SearchRange *= GridScale;
                }

                HashSet<PointF> Checked = new();
                List<Node> ToCheck = new() { new(From, 0, null) };
                Node last = null;
                while (ToCheck.Count > 0 && SearchRange > -1)
                {
                    Node currentpos = ToCheck[0];
                    ToCheck.RemoveAt(0);
                    if (currentpos.Pos.Equals(To))
                    {
                        last = currentpos;
                        break;
                    }
                    float dist;
                    (bool, PointF)[] nextpos = new (bool, PointF)[]
                    {
               (currentpos.Pos.X - 1 >= 0, new PointF(currentpos.Pos.X - 1, currentpos.Pos.Y)),
               (currentpos.Pos.Y - 1 >= 0, new PointF(currentpos.Pos.X, currentpos.Pos.Y - 1)),
               (currentpos.Pos.X + 1 < Grid.GetLength(0), new PointF(currentpos.Pos.X + 1, currentpos.Pos.Y)),
               (currentpos.Pos.Y + 1 < Grid.GetLength(1), new PointF(currentpos.Pos.X, currentpos.Pos.Y + 1))
                    };
                    for (int i = 0; i < 4; i++)
                    {
                        if (!Checked.Contains(nextpos[i].Item2) && nextpos[i].Item1 && Grid[(int)nextpos[i].Item2.X, (int)nextpos[i].Item2.Y] != PathOccupancy.Blocked)
                        {
                            dist = nextpos[i].Item2.Distance(To) + (int)Grid[(int)nextpos[i].Item2.X, (int)nextpos[i].Item2.Y];
                            if (ToCheck.Count > 0 && ToCheck[0].D > dist)
                            {
                                ToCheck.Insert(0, new(nextpos[i].Item2, dist, currentpos));
                            }
                            else
                            {
                                ToCheck.Add(new(nextpos[i].Item2, dist, currentpos));
                            }
                        }
                    }

                    Checked.Add(currentpos.Pos);
                    SearchRange--;
                }

                LinkedList<PointF> result = new LinkedList<PointF>();
                if (last == null || last.Parent == null)
                {
                    Utility.ConsoleLog($"{(SearchRange <= 0 ? "Pathfinding ran out of SearchRange" : "Pathfinding couln't find a path for {From} - {To}")}", Petrosik.Enums.InfoType.Warn);
                    return null;
                }
                while (last.Parent != null)
                {
                    if (WorldSpace)
                    {
                        result.AddLast(GetWorldPosition(last.Pos));
                    }
                    else
                    {
                        result.AddLast(new PointF(last.Pos.X + 0.5f, last.Pos.Y + 0.5f));
                    }
                    last = last.Parent;
                }
                if (WorldSpace)
                {
                    result.AddLast(GetWorldPosition(From));
                }
                else
                {
                    result.AddLast(new PointF(From.X + 0.5f, From.Y + 0.5f));
                }

                return result;
            }
            public bool CheckDirectLine(PointF from, PointF to)
            {
                var dx = Math.Abs(to.X - from.X);
                var dy = Math.Abs(to.Y - from.Y);
                int sx = from.X < to.X ? 1 : -1;
                int sy = from.Y < to.Y ? 1 : -1;
                var err = dx - dy;

                while (from.X != to.X || from.Y != to.Y)
                {
                    if (Grid[(int)from.X, (int)from.Y] == PathOccupancy.Blocked)
                    {
                        return false;
                    }
                    var e2 = 2 * err;

                    // Move horizontally or vertically based on error
                    if (e2 > -dy)
                    {
                        err -= dy;
                        from.X += sx;
                    }
                    else if (e2 < dx)
                    {
                        err += dx;
                        from.Y += sy;
                    }
                }
                return true;
            }
        }
        internal class Node
        {
            internal PointF Pos;
            internal float D;
            internal Node Parent;
            internal Node(PointF pos, float d, Node parent)
            {
                Pos = pos;
                D = d;
                Parent = parent;
            }
            public override string ToString()
            {
                return $"{Pos} - {D} | {(Parent != null ? $"{Parent.Pos} - {D}" : "null")}";
            }
        }
    }
}