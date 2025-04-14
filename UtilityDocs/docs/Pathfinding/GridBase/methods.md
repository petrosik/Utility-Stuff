# Methods
### AddObstacle   
Adds obstacle object to the list that creates the grid  
**The Method:**  
```
void AddObstacle(ObstacleObject Obstacle)
```
`Obstacle` - Obstacle that should be added
<details> <summary><b>Raw Code:</b></summary>
``` 
var scalematrix = new Matrix();
scalematrix.Scale(GridScale, GridScale);
Obstacle.Shape.Transform(scalematrix);
loadedObstacles.Add(Obstacle);
BuildGrid();
``` 
</details>

---
### RemoveObstacle    
Removes obstacle object from the list that creates the grid  
**The Method:**  
```
void RemoveObstacle(ObstacleObject Obstacle)
```
`Obstacle` - Obstacle that should be removed from the list
<details> <summary><b>Raw Code:</b></summary>
``` 
loadedObstacles.Remove(Obstacle);
BuildGrid();
``` 
</details>

---
### RemoveObstacleAt   
Removes obstacle object at position on the list that creates the grid  
**The Method:**  
``` 
void RemoveObstacleAt(int Index)
```
`Index` - At what position should be removed
<details> <summary><b>Raw Code:</b></summary>
``` 
loadedObstacles.RemoveAt(Index);
BuildGrid();
``` 
</details>

---
### ClearObstalces
Clears all obstacles  
!!! warning
    Also removes what you set as BaseClearArea when creating this GridBase

**The Method:**  
```  
void ClearObstalces()  
```

<details> <summary><b>Raw Code:</b></summary>
``` 
loadedObstacles.Clear();
BuildGrid();
``` 
</details>

---
### MoveObstalce  
Moves obstacle on Index Up or Down in the list based on Offset
!!! tip
    \+ Moves Down, - Moves Up  
**The Method:**  
```
void MoveObstalce(int Index, int Offset = 1)
```
`Index` - Index what positon should be moved  
`Offset` (Default: 1) - How much should it be moved  
<details> <summary><b>Raw Code:</b></summary>
``` 
if (Index + Offset < 0 || Index + Offset > loadedObstacles.Count - 1)
{
    throw new IndexOutOfRangeException("Index + Offset is outside of range of the obstacle list");
}
(loadedObstacles[Index + Offset], loadedObstacles[Index]) = (loadedObstacles[Index], loadedObstacles[Index + Offset]);
BuildGrid();
``` 
</details>

---
### AddSingle  
Adds single pixel worth obstacle
!!! warning 
    Use Sparingly, will slow down grid building  
**The Method:**  
```
void AddSingle(int X, int Y, PathOccupancy Type)
```
`X` - x Grid position  
`Y` - y Grid position
`Type` - What type the object should be
<details> <summary><b>Raw Code:</b></summary>
``` 
GraphicsPath path = new GraphicsPath();
path.AddRectangle(new RectangleF(X, Y, 1f / GridScale, 1f / GridScale));
AddObstacle(new(Type, path));
``` 
</details>

---
### GetPathColor
Returns color interpretation of the tile   
**The Method:**  
```
Color GetPathColor(PathOccupancy Tile)
```
`Tile` - Tile to get the color for
!!! tip
    PathOccupancy.Blocked => Color.Red <span class="color-box" style="background-color:#FF0000;"></span>  
    PathOccupancy.Clear => Color.DarkGreen <span class="color-box" style="background-color:#006400;"></span>   
    PathOccupancy.LowP => Color.Green <span class="color-box" style="background-color:#008000;"></span>  
    PathOccupancy.MediumP => Color.Yellow <span class="color-box" style="background-color:#FFFF00;"></span>  
    PathOccupancy.HighP => Color.Magenta <span class="color-box" style="background-color:#FF00FF;"></span>  
    PathOccupancy.Path => Color.White <span class="color-box" style="background-color:#FFFFFF;"></span>  

<details> <summary><b>Raw Code:</b></summary>
``` 
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
``` 
</details>

---
### GetGridPosition  
Returns position on the grid from world position  
**The Method:**  
```
PointF GetGridPosition(PointF Pos)
```
`Pos` - World position
<details> <summary><b>Raw Code:</b></summary>
``` 
return new PointF((float)Math.Floor(Pos.X / (1f / GridScale)), (float)Math.Floor(Pos.Y / (1f / GridScale)));
``` 
</details>

---
### GetWorldPosition  
Returns position in world from grid position  
**The Method:**  
```
PointF GetWorldPosition(PointF Pos)
```
`Pos` - Grid position
<details> <summary><b>Raw Code:</b></summary>
```  
return new PointF(Pos.X * (1f / GridScale) + (1f / GridScale / 2), Pos.Y * (1f / GridScale) + (1f / GridScale / 2));
``` 
</details>

---
### ExportPaths 
Exports Obstacles into Json format  
**The Method:**  
```
string ExportPaths()
```
<details> <summary><b>Raw Code:</b></summary>
``` 
List<ObstacleObjectExport> l = new();
foreach (var obstacle in loadedObstacles)
{
    l.Add(new(obstacle.Type, obstacle.Shape.PathPoints.Select(x => new float[] { x.X, x.Y }).ToArray(), obstacle.Shape.PathTypes));
}
return JsonSerializer.Serialize(l);
``` 
</details>

---
### InportPaths  
Inports Obstacles from Json text  
**The Method:**  
```
void InportPaths(string text)
```
`text` - Text that gets inported
<details> <summary><b>Raw Code:</b></summary>
``` 
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
``` 
</details>

---
### GetPath  
Returns List of coordinates for path to target, searchrange is  
**The Method:**  
```  
virtual List<PointF>? GetPath(PointF From, PointF To, float SearchRange = 0)
``` 
`From` - World's Units  
`To` - World's Units  
`SearchRange` (default: 0) - How far to search in world's units (0 for default limit)  
<details> <summary><b>Raw Code:</b></summary>
``` 
throw new NotImplementedException("Someone didn't implemented this");
``` 
</details>

---
### GetRawPath  
Simple Raw Pathing method   

**The Method:**  
``` 
virtual LinkedList<PointF>? GetRawPath(PointF From, PointF To, float SearchRange = 0, bool WorldSpace = true)  
```
`From` - World's Units  
`To` - World's Units  
`SearchRange` (default: 0) - How far to search in world's units (0 for default limit)  
`WorldSpace` (default: true) - Should the positions be converted to world positions  

<details> <summary><b>Raw Code:</b></summary>
``` 
throw new NotImplementedException("Someone didn't implemented this");
``` 
</details>
---