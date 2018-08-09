All Tiles in the Game are in (integer,integer) locations
so each tile is of 1 by 1 size

Given the above...

The JSON file with floors should contain 
-2 values for corner1 that are both integers
-2 values for corner2 that are both integers
-these values indicate the corner tile and not the literal corner of the entire space
-given the above if you have a with proper corner1 and corner2 values its impossible to not have a visual tile

The JSON file with walls should contain
-2 values for point1 that are both not integers (starting at .5 and increasing or decreasing by 1)
-2 values for point2 that are both not integers (starting at .5 and increasing or decreasing by 1)
-these values are the points where the wall lies strictly inbetween these 2 points