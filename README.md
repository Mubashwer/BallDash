# Ball Dash

Ball dash is a classic marble maze game designed for Windows. Your goal is to control the marble ball to solve the maze without falling into any holes.

### Version
1.0.0

### Description
* The game can be started by pressing the start button. You can go to the options page and disable touch and/or accelerometer and enable/disable debug mode. The instructions page will contain the instructions on how to play. Once the game is started, you can select different levels. The maze board can be tilted by tilting the tablet. The ball can be moved by arrow keys. If the screen is touched, the ball will attempt to move towards the point of touch. If the ball falls into a hole, the game will restart. If the ball touches the "Rainbow" tile, it will enable disco-like lights. The game is over once the ball reaches the green end tile. There is a "HINT" button on the game overlay page. If it is touched it can enable/disable maze solution. It highlights the shortest path from the player's position to the end tile by decreasing the brightness of everything except the tiles in the shortest path (which is generated via Djkistra's algorithm).

* Models are all geometric primitives which are included SharpDX library which use indexed vertex definition, vertex normals and texture for drawing.

* Customised phong shader was used to handle the graphics which supports textures and multiple lights. The Camera is placed above the maze board and faces the ball. It follows the ball and updates its position and target appropriately. Pinching of the screen controls the zooming of the camera. 

* The Priority Queue used in our implementation of Djkistra's Algorithm is copied directly from Satsuma .NET library (http://sourceforge.net/projects/satsumagraph/). The license is included above the class definition.

* The physics engine is homemade. It calculates the rough angle that the collision occurred on (a multiple of 45 degrees), and uses that value to calculate a change in X and Y velocity components, adding dampening as it does so.