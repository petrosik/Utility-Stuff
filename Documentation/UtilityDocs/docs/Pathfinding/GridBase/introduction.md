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