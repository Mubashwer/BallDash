// Copyright (c) 2010-2013 SharpDX - Alexandre Mutel
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using SharpDX;
using SharpDX.Toolkit;
using System;
using System.Collections.Generic;
using Windows.UI.Input;
using Windows.UI.Core;
using Windows.Devices.Sensors;

namespace Project
{
    using System.Diagnostics;
    // Use this namespace here in case we need to use Direct3D11 namespace as well, as this
    // namespace will override the Direct3D11.
    using SharpDX.Toolkit.Graphics;
    using SharpDX.Toolkit.Input;

    public class LabGame : Game
    {
        private GraphicsDeviceManager graphicsDeviceManager;
        public List<GameObject> gameObjects;
        private Stack<GameObject> addedGameObjects;
        private Stack<GameObject> removedGameObjects;
        private KeyboardManager keyboardManager;
        public KeyboardState keyboardState;
        public Player player;
        public AccelerometerReading accelerometerReading;
        public GameInput input;
        public int score;
        public MainPage mainPage;
        public MazeSolver solver;

        public Dictionary<Point, GameObject> tiles = new Dictionary<Point, GameObject>();
        public Map CurrentMap { get; set; }

        // TASK 4: Use this to represent difficulty
        public float difficulty;

        // Represents the camera's position and orientation
        public Camera camera;

        // Graphics assets
        public Assets assets;

        // Random number generator
        public Random random;

        // World boundaries that indicate where the edge of the screen is for the camera.
        public float boundaryLeft;
        public float boundaryRight;
        public float boundaryTop;
        public float boundaryBottom;

        public bool started = false;
        /// <summary>
        /// Initializes a new instance of the <see cref="LabGame" /> class.
        /// </summary>
        public LabGame(MainPage mainPage)
        {
            // Creates a graphics manager. This is mandatory.
            graphicsDeviceManager = new GraphicsDeviceManager(this);

            // Setup the relative directory to the executable directory
            // for loading contents with the ContentManager
            Content.RootDirectory = "Content";

            // Create the keyboard manager
            keyboardManager = new KeyboardManager(this);
            assets = new Assets(this);
            random = new Random();
            input = new GameInput();

            // Set boundaries.
            boundaryLeft = 0f; //-4.5f;
            boundaryRight = 50f; //4.5f;
            boundaryTop = 50f; //4;
            boundaryBottom = 0f;//-4.5f;

            // Initialise event handling.
            input.gestureRecognizer.Tapped += Tapped;
            input.gestureRecognizer.ManipulationStarted += OnManipulationStarted;
            input.gestureRecognizer.ManipulationUpdated += OnManipulationUpdated;
            input.gestureRecognizer.ManipulationCompleted += OnManipulationCompleted;

            this.mainPage = mainPage;

            score = 0;
            difficulty = 1;
        }

        protected override void LoadContent()
        {
            // Initialise game object containers.
            gameObjects = new List<GameObject>();
            addedGameObjects = new Stack<GameObject>();
            removedGameObjects = new Stack<GameObject>();

            // Create game objects.
            player = new Player(this, "Phong", new Vector3(6f, 6f, 0));
            gameObjects.Add(player);
            camera = new Camera(this);

            var basicMap = new TextMap("testMap.txt");
            ChangeMap(basicMap);

            // SOLVER TEST
            
            solver = new MazeSolver(this, basicMap);
            solver.SolveMaze();
            
            /*foreach (var path in paths.Values)
            {
                Debug.WriteLine("STARTPATH");
                foreach(var unit in path)
                    Debug.WriteLine(unit);
                Debug.WriteLine("ENDPATH");
            }*/



            // map test
            Debug.WriteLine("First map:\n{0}", CurrentMap.ToString());

            // Create an input layout from the vertices

            base.LoadContent();
        }

        public void ChangeMap(Map map) {
            CurrentMap = map;
            boundaryRight = CurrentMap.Width * Map.WorldUnitWidth;
            boundaryTop = CurrentMap.Height * Map.WorldUnitHeight;
            LoadFloor(CurrentMap);
        }

        private void LoadFloor(Map map)
        {
            var width = Map.WorldUnitWidth;
            var height = Map.WorldUnitHeight;
            for (int i = 0; i < map.Width; i++)
            {
                for (int j = 0; j < map.Height; j++)
                {
                    var x = (i * width) + width / 2;
                    var y = (j * height) + height / 2;
                    var z = 0f;

                    Map.UnitType unitType = map[i, j];
                    if (unitType == Map.UnitType.PlayerStart)
                    {
                        var startObject = new FloorUnitGameObject(this, "Phong", new Vector3(x, y, z));
                        gameObjects.Add(startObject);
                        tiles[new Point(i, j)] = startObject;
                    }
                    if (unitType == Map.UnitType.PlayerEnd)
                    {
                        var endObject = new FloorUnitGameObject(this, "Phong", new Vector3(x, y, z));
                        endObject.IsEndObject = true;
                        gameObjects.Add(endObject);
                        tiles[new Point(i, j)] = endObject;
                    }
                    else if (unitType == Map.UnitType.Floor) {
                        var floorObject = new FloorUnitGameObject(this, "Phong", new Vector3(x, y, z));
                        gameObjects.Add(floorObject);
                        tiles[new Point(i, j)] = floorObject;
                    }
                    else if (unitType == Map.UnitType.Wall) {
                        z = -width / 2.0f;
                        var wallObject = new WallGameObject(this, "Phong", new Vector3(x, y, z));
                        gameObjects.Add(wallObject);
                        tiles[new Point(i, j)] = wallObject;
                    }
                }
            }
        }

        protected override void Initialize()
        {
            Window.Title = "Lab 4";
            

            base.Initialize();
        }

        protected override void Update(GameTime gameTime)
        {
            if (started)
            {
                keyboardState = keyboardManager.GetState();
                flushAddedAndRemovedGameObjects();
                float deltaTime = (float)gameTime.ElapsedGameTime.Milliseconds/1000f;
                float speed = 3f;

                if (keyboardState.IsKeyDown(Keys.W))
                {
                    camera.cameraMoved = true;
                    camera.pitch -= speed * deltaTime;
                }
                if (keyboardState.IsKeyDown(Keys.S))
                {
                    camera.cameraMoved = true;
                    camera.pitch += speed *deltaTime;
                }
                if (keyboardState.IsKeyDown(Keys.A))
                {
                    camera.cameraMoved = true;
                    camera.yaw -= speed * deltaTime;
                }  
                if (keyboardState.IsKeyDown(Keys.D))
                {
                    camera.cameraMoved = true;
                    camera.yaw += speed * deltaTime;
                }
                if (keyboardState.IsKeyDown(Keys.Q))
                {
                    camera.cameraMoved = true;
                    camera.roll -= speed * deltaTime;
                }
                if (keyboardState.IsKeyDown(Keys.E))
                {
                    camera.cameraMoved = true;
                    camera.roll += speed * deltaTime;
                }

                accelerometerReading = input.accelerometer.GetCurrentReading();
                for (int i = 0; i < gameObjects.Count; i++)
                {
                    gameObjects[i].Update(gameTime);
                }

                mainPage.UpdateScore(score);

                if (keyboardState.IsKeyDown(Keys.Escape))
                {
                    this.Exit();
                    this.Dispose();
                    App.Current.Exit();
                }

                // update the camera last
                camera.Update();
            }
            solver.Hint();
            // Handle base.Update
            base.Update(gameTime);

        }

        protected override void Draw(GameTime gameTime)
        {
            if (started)
            {
                // Clears the screen with the Color.CornflowerBlue
                GraphicsDevice.Clear(Color.CornflowerBlue);

                for (int i = 0; i < gameObjects.Count; i++)
                {
                    gameObjects[i].Draw(gameTime);
                }
            }
            // Handle base.Draw
            base.Draw(gameTime);
        }
        // Count the number of game objects for a certain type.
        public int Count(GameObjectType type)
        {
            int count = 0;
            foreach (var obj in gameObjects)
            {
                if (obj.type == type) { count++; }
            }
            return count;
        }

        // Add a new game object.
        public void Add(GameObject obj)
        {
            if (!gameObjects.Contains(obj) && !addedGameObjects.Contains(obj))
            {
                addedGameObjects.Push(obj);
            }
        }

        // Remove a game object.
        public void Remove(GameObject obj)
        {
            if (gameObjects.Contains(obj) && !removedGameObjects.Contains(obj))
            {
                removedGameObjects.Push(obj);
            }
        }

        // Process the buffers of game objects that need to be added/removed.
        private void flushAddedAndRemovedGameObjects()
        {
            while (addedGameObjects.Count > 0) { gameObjects.Add(addedGameObjects.Pop()); }
            while (removedGameObjects.Count > 0) { gameObjects.Remove(removedGameObjects.Pop()); }
        }

        public void OnManipulationStarted(GestureRecognizer sender, ManipulationStartedEventArgs args)
        {
            // Pass Manipulation events to the game objects.

        }

        public void Tapped(GestureRecognizer sender, TappedEventArgs args)
        {
            // Pass Manipulation events to the game objects.
            foreach (var obj in gameObjects)
            {
                obj.Tapped(sender, args);
            }
        }

        public void OnManipulationUpdated(GestureRecognizer sender, ManipulationUpdatedEventArgs args)
        {
            
            // TODO: need to change

            /*camera.pos.Z = camera.pos.Z * args.Delta.Scale;
            // Update camera position for all game objects
            foreach (var obj in gameObjects)
            {
                if (obj.basicEffect != null) { obj.basicEffect.View = camera.View; }
                obj.OnManipulationUpdated(sender, args);
            }*/
        }

        public void OnManipulationCompleted(GestureRecognizer sender, ManipulationCompletedEventArgs args)
        {
        }

    }
}
