# Grid Base
Base for pathfinding classes contains grid map thats generated from paths thats also stored with it, and other default settings.

## Fields 
- **readonly PointF Up**  
Default: `new(0f, 1f)`  
- **readonly PointF Down**  
Default: `new(0f, -1f)`   
- **readonly PointF Left**  
Default: `new(-1f, 0f)`  
- **readonly PointF Right**  
Default: `new(1f, 0f)`  
- **int GridScale**  
How many path units are in 1 world unit  
Default: `20`  
- **float SearchRange**  
How far should pathfinding search by default(world units)  
Default: `20`  
- **int MaxSegmentCount**  
After how many path units should the path make new "node" in the output. (0 for no limit)
Default: `40`  
- **Size GridSize**  
Size of the current grid
- **[PathOccupancy](../../enums.md#pathoccupancy)[,] Grid**
- **ImmutableList&lt;[ObstacleObject](./obstacleobject.md)> LoadedObstacles**  
Allows only `get` and returns internal list with all obstacles that contribute to generation of Grid
## Methods overview
- **[AddObstacle](./methods.md#addobstacle)([ObstacleObject](./obstacleobject.md) Obstacle)**  
Adds obstacle object to the list that creates the grid
- **[RemoveObstacle](./methods.md#removeobstacle)([ObstacleObject](./obstacleobject.md) Obstacle)**  
Removes obstacle object from the list that creates the grid
- **[RemoveObstacleAt](./methods.md#removeobstacleat)(int Index)**  
Removes obstacle object at position on the list that creates the grid
- **[ClearObstalces](./methods.md#clearobstalces)()**  
Clears all obstacles
- **[MoveObstalce](./methods.md#moveobstalce)(int Index, int Offset = 1)**  
Moves obstacle on Index Up or Down in the list based on Offset
- **[AddSingle](./methods.md#addsingle)(int X, int Y, [PathOccupancy](../../enums.md#pathoccupancy) Type)**  
Adds single pixel worth obstacle
- **[GetPathColor](./methods.md#getpathcolor)([PathOccupancy](../../enums.md#pathoccupancy) Tile)**  
Returns color interpretation of the tile
- **[GetGridPosition](./methods.md#getgridposition)(PointF Pos)**  
Returns position on the grid from world position
- **[GetWorldPosition](./methods.md#getworldposition)(PointF Pos)**  
Returns position in world from grid position
- **[ExportPaths](./methods.md#exportpaths)()**  
Exports Obstacles into Json format
- **[InportPaths](./methods.md#inportpaths)(string text)**  
Inports Obstacles from Json text
- **[GetPath](./methods.md#getpath)(PointF From, PointF To, float SearchRange = 0)**  
Returns List of coordinates for path to target, searchrange is 
- **[GetRawPath](./methods.md#getrawpath)(PointF From, PointF To, float SearchRange = 0, bool WorldSpace = true)**  
Simple Raw Pathing method