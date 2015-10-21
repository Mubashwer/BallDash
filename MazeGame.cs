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

namespace Project {
    using System.Diagnostics;
    using Menus;
    // Use this namespace here in case we need to use Direct3D11 namespace as well, as this
    // namespace will override the Direct3D11.
    using SharpDX.Toolkit.Graphics;
    using SharpDX.Toolkit.Input;

    public class MazeGame : Game {
        private GraphicsDeviceManager graphicsDeviceManager;
        private KeyboardManager keyboardManager;
        private Stack<GameObject> addedGameObjects;
        private Stack<GameObject> removedGameObjects;

        public List<GameObject> GameObjects { get; set; }
        public KeyboardState KeyboardState { get; set; }
        public Player Player { get; set; }
        public AccelerometerReading AccelerometerReading { get; set; }
        public GameInput Input { get; set; }
        public GamePage GameOverlayPage { get; set; }
        public MazeSolver MazeSolver { get; set; }
        public GraphicCache GraphicCache { get; set; }
        public GameSettings GameSettings { get; set; }

        public Dictionary<Point, GameObject> Tiles { get; set; }
        public Map CurrentMap { get; set; }

        // Represents the camera's position and orientation
        public Camera Camera { get; set; }

        // Graphics assets
        public Assets Assets { get; set; }

        // Random number generator
        public Random RandomGenerator { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MazeGame" /> class.
        /// </summary>
        public MazeGame(GamePage gameOverlayPage, GameSettings settings) {
            this.GameSettings = settings;
            // Creates a graphics manager. This is mandatory.
            graphicsDeviceManager = new GraphicsDeviceManager(this);
            GraphicCache = new GraphicCache();

            // Setup the relative directory to the executable directory
            // for loading contents with the ContentManager
            Content.RootDirectory = "Content";

            // Create the keyboard manager
            keyboardManager = new KeyboardManager(this);
            Assets = new Assets(this);
            RandomGenerator = new Random();
            Input = new GameInput();

            // Initialise event handling.
            Input.gestureRecognizer.Tapped += Tapped;
            Input.gestureRecognizer.ManipulationStarted += OnManipulationStarted;
            Input.gestureRecognizer.ManipulationUpdated += OnManipulationUpdated;
            Input.gestureRecognizer.ManipulationCompleted += OnManipulationCompleted;

            this.GameOverlayPage = gameOverlayPage;
        }

        protected override void LoadContent() {
            // Initialise game object containers.
            GameObjects = new List<GameObject>();
            addedGameObjects = new Stack<GameObject>();
            removedGameObjects = new Stack<GameObject>();
            Tiles = new Dictionary<Point, GameObject>();

            // Create game objects.
            Player = new Player(this, "Phong", new Vector3(6f, 6f, 0));
            GameObjects.Add(Player);
            Camera = new Camera(this);

            var basicMap = new TextMap("Maps\\testMap.txt");
            ChangeMap(basicMap);

            // Create an input layout from the vertices
            base.LoadContent();
        }


        public void ChangeMap(Map map) {
            CurrentMap = map;
            LoadFloor(CurrentMap);

            MazeSolver = new MazeSolver(this, map);
            MazeSolver.SolveMaze();

            // map test
            Debug.WriteLine("First map:\n{0}", CurrentMap.ToString());
        }

        private void LoadFloor(Map map) {
            var width = Map.WorldUnitWidth;
            var height = Map.WorldUnitHeight;
            for (int i = 0; i < map.Width; i++) {
                for (int j = 0; j < map.Height; j++) {
                    var x = (i * width) + width / 2;
                    var y = (j * height) + height / 2;
                    var z = 0f;

                    Map.UnitType unitType = map[i, j];
                    if (unitType == Map.UnitType.PlayerStart) {
                        var startObject = new FloorUnitGameObject(this, "Phong", new Vector3(x, y, z));
                        GameObjects.Add(startObject);
                        Tiles[new Point(i, j)] = startObject;
                    }
                    if (unitType == Map.UnitType.PlayerEnd) {
                        var endObject = new FloorUnitGameObject(this, "Phong", new Vector3(x, y, z));
                        endObject.IsEndObject = true;
                        GameObjects.Add(endObject);
                        Tiles[new Point(i, j)] = endObject;
                    }
                    else if (unitType == Map.UnitType.Floor) {
                        var floorObject = new FloorUnitGameObject(this, "Phong", new Vector3(x, y, z));
                        GameObjects.Add(floorObject);
                        Tiles[new Point(i, j)] = floorObject;
                    }
                    else if (unitType == Map.UnitType.Wall) {
                        z = -width / 2.0f;
                        var wallObject = new WallGameObject(this, "Phong", new Vector3(x, y, z));
                        GameObjects.Add(wallObject);
                        Tiles[new Point(i, j)] = wallObject;
                    }
                }
            }
        }

        protected override void Initialize() {
            Window.Title = "Ball Dash";

            base.Initialize();
        }

        protected override void Update(GameTime gameTime) {
            if (IsRunning) {
                KeyboardState = keyboardManager.GetState();
                flushAddedAndRemovedGameObjects();
                float deltaTime = (float)gameTime.ElapsedGameTime.Milliseconds / 1000f;
                float speed = 3f;

                if (KeyboardState.IsKeyDown(Keys.W)) {
                    Camera.cameraMoved = true;
                    Camera.pitch -= speed * deltaTime;
                }
                if (KeyboardState.IsKeyDown(Keys.S)) {
                    Camera.cameraMoved = true;
                    Camera.pitch += speed * deltaTime;
                }
                if (KeyboardState.IsKeyDown(Keys.A)) {
                    Camera.cameraMoved = true;
                    Camera.yaw -= speed * deltaTime;
                }
                if (KeyboardState.IsKeyDown(Keys.D)) {
                    Camera.cameraMoved = true;
                    Camera.yaw += speed * deltaTime;
                }
                if (KeyboardState.IsKeyDown(Keys.Q)) {
                    Camera.cameraMoved = true;
                    Camera.roll -= speed * deltaTime;
                }
                if (KeyboardState.IsKeyDown(Keys.E)) {
                    Camera.cameraMoved = true;
                    Camera.roll += speed * deltaTime;
                }

                if (GameSettings.AccelerometerEnabled && Input.accelerometer != null) {
                    AccelerometerReading = Input.accelerometer.GetCurrentReading();
                }
                else {
                    AccelerometerReading = null;
                }

                for (int i = 0; i < GameObjects.Count; i++) {
                    GameObjects[i].Update(gameTime);
                }

                // update the camera last
                Camera.Update();
            }
            MazeSolver.Hint();
            // Handle base.Update
            base.Update(gameTime);

        }

        protected override void Draw(GameTime gameTime) {
            if (IsRunning) {
                // Clears the screen with the Color.Black
                GraphicsDevice.Clear(Color.Black);

                for (int i = 0; i < GameObjects.Count; i++) {
                    GameObjects[i].Draw(gameTime);
                }
            }
            // Handle base.Draw
            base.Draw(gameTime);
        }
        // Count the number of game objects for a certain type.
        public int Count(GameObjectType type) {
            int count = 0;
            foreach (var obj in GameObjects) {
                if (obj.type == type) { count++; }
            }
            return count;
        }

        // Add a new game object.
        public void Add(GameObject obj) {
            if (!GameObjects.Contains(obj) && !addedGameObjects.Contains(obj)) {
                addedGameObjects.Push(obj);
            }
        }

        // Remove a game object.
        public void Remove(GameObject obj) {
            if (GameObjects.Contains(obj) && !removedGameObjects.Contains(obj)) {
                removedGameObjects.Push(obj);
            }
        }

        // Process the buffers of game objects that need to be added/removed.
        private void flushAddedAndRemovedGameObjects() {
            while (addedGameObjects.Count > 0) { GameObjects.Add(addedGameObjects.Pop()); }
            while (removedGameObjects.Count > 0) { GameObjects.Remove(removedGameObjects.Pop()); }
        }

        public void OnManipulationStarted(GestureRecognizer sender, ManipulationStartedEventArgs args) {
            // Pass Manipulation events to the game objects.

        }

        public void Tapped(GestureRecognizer sender, TappedEventArgs args) {
            // Pass Manipulation events to the game objects.
            foreach (var obj in GameObjects) {
                obj.Tapped(sender, args);
            }
        }

        public void OnManipulationUpdated(GestureRecognizer sender, ManipulationUpdatedEventArgs args) {

            // TODO: need to change

            /*camera.pos.Z = camera.pos.Z * args.Delta.Scale;
            // Update camera position for all game objects
            foreach (var obj in gameObjects)
            {
                if (obj.basicEffect != null) { obj.basicEffect.View = camera.View; }
                obj.OnManipulationUpdated(sender, args);
            }*/
        }

        public void OnManipulationCompleted(GestureRecognizer sender, ManipulationCompletedEventArgs args) {
        }

    }
}
