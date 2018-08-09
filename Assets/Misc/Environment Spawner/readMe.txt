There are different types of Objects that we can spawn into the scene

MUST USE THE STREAMING ASSETS

NOTE: orderInLayer RANGE(−32,768 to 32,767)

*[TINT] To Easily Create Variation Every Single Object has a tint option
	NOTE: that the tint applies in different spots for different object types (EX: a bed object tint might just change sheet colors)
*[LAYER] To Have Flexibility over how SIMILAR objects show up each object has assigned a specific layer

NOTE: To Have Flexibility over how DIFFERENT objects show up each object type has a specific layer range
	these values are all in the same range so that objects of different types can both be above and below other objects of different types

FLOORS(0-100) < Walls(101-200)
walls never mess with obstacles or props
FLOORS < OBSTACLES
FLOORS < PROPS
...BUT... Obstacles and props can both be either above or below each other... so... between (301-500)
OBSTALCES will take even values
PROPS will take odd values

1. Floors(Squares) that spawn from one corner to another
	*there are multiple "floorTypes" and each type will be using a different sprite 
		---TYPES---
		-TILE
		-CARPET
		-WOOD
2. Walls(Strips) that spawn form one point to another
	*there are multiple "wallTypes" and each type will be using a different sprite 
		---TYPES---
		-UNBREAKABLE_WALL
		---WINDOW
		-WINDOW_ONE_WAY
		-WINDOW_TWO_WAY
		---DOOR
		-DOOR_ONE_WAY_REBOUND
		-DOOR_ONE_WAY_NON_REBOUNDING
		-DOOR_TWO_WAY_REBOUND
		-DOOR_TWO_WAY_NON_REBOUNDING
	*since these objects have 2 points 2 spawn they also have a variable that lets you read how the values are read in (given then some objects change functionality depending on what direction they face)
3. Obstacles that spawn at a position with a rotation
	*obstacles collide with player
4. Props that spawn at a position with a rotation
	*props dont collide with player