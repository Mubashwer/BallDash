﻿// Copyright (c) 2010-2013 SharpDX - Alexandre Mutel
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
    using System.Linq;
    using Menus;
    // Use this namespace here in case we need to use Direct3D11 namespace as well, as this
    // namespace will override the Direct3D11.
    using SharpDX.Toolkit.Graphics;
    using SharpDX.Toolkit.Input;
    using Windows.ApplicationModel;
    using Windows.Storage;

    public class MazeGame : Game {
        private GraphicsDeviceManager graphicsDeviceManager;
        private KeyboardManager keyboardManager;
        private Stack<GameObject> addedGameObjects;
        private Stack<GameObject> removedGameObjects;

        public List<GameObject> GameObjects { get; set; }
        public List<Light> Lights { get; set; }
        public KeyboardState KeyboardState { get; set; }
        public Player Player { get; set; }
        public AccelerometerReading AccelerometerReading { get; set; }
        public GameInput Input { get; set; }
        public GamePage GameOverlayPage { get; set; }
        public MazeSolver MazeSolver { get; set; }
        public GraphicCache GraphicCache { get; set; }
        public GameSettings GameSettings { get; set; }
        public List<LevelInfo> AvailableLevels { get; private set; }
        public Dictionary<Point, GameObject> Tiles { get; set; }
        public LevelInfo CurrentLevel { get; set; }
        public LevelStatus CurrentLevelStatus { get; set; }
        private Stopwatch levelTimer = new Stopwatch();

        // Represents the camera's position and orientation
        public Camera Camera { get; set; }

        // Graphics assets
        public Assets Assets { get; set; }

        // Random number generator
        public Random RandomGenerator { get; set; }

        public event EventHandler AllLevelsComplete;


        // Checks if game has started, uses a lock for threadsafe concurrency
        private bool isStarted;
        private readonly object isStartedLock = new object();
        public bool IsStarted {
            get {
                lock (isStartedLock) {
                    return isStarted;
                }
            }
            set {
                lock (isStartedLock) {
                    isStarted = value;
                }
            }
        }


        public float DefaultLightHeight { get; set; }

        private bool _enableRainbowEffect = false;
        public bool RainbowModeOn {

            get { return _enableRainbowEffect; }
            set {
                _enableRainbowEffect = value;
                if (value) // Rainbow mode turns on three moving red, green and blue lights
                {
                    Lights[0].LightColor = Color.Red.ToColor4();
                    Lights[0].LightIntensity = 1.0f;
                    Lights[1].LightColor = Color.Green.ToColor4();
                    Lights[2].LightColor = Color.Blue.ToColor4();
                }
                else { // Use 1 white light only
                    Lights[0].LightColor = Color.White.ToColor4();
                    Lights[0].LightIntensity = 0.5f;
                    Lights[0].LightPosition = new Vector3(CurrentLevel.Map.Width / 2f * CurrentLevel.Map.MapUnitWidth,
                        CurrentLevel.Map.Height * CurrentLevel.Map.MapUnitHeight / 2f,
                        DefaultLightHeight);
                    Lights[1].LightColor = Color.Black.ToColor4();
                    Lights[2].LightColor = Color.Black.ToColor4();
                }

            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MazeGame" /> class.
        /// </summary>
        public MazeGame(GameSettings settings) {
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
            Input.gestureRecognizer.Tapped += OnTapped;
            Input.gestureRecognizer.ManipulationStarted += OnManipulationStarted;
            Input.gestureRecognizer.ManipulationUpdated += OnManipulationUpdated;
            Input.gestureRecognizer.ManipulationCompleted += OnManipulationCompleted;

            IsStarted = false;

        }

        private void LoadLevels() {
            this.AvailableLevels = new List<LevelInfo>();

            try {
                StorageFolder folder = Package.Current.InstalledLocation.GetFolderAsync("Maps").GetAwaiter().GetResult();
                IReadOnlyList<StorageFile> mapFiles = folder.GetFilesAsync().GetAwaiter().GetResult();

                foreach (StorageFile thisMapFile in mapFiles) {
                    IList<string> readLines = FileIO.ReadLinesAsync(thisMapFile).GetAwaiter().GetResult();
                    LevelInfo level = LevelInfo.CreateFromMapDefinition(readLines);
                    AvailableLevels.Add(level);
                }
            }
            catch (Exception e) {
                Debug.WriteLine("Could not open map, Error: {1}", e.ToString());
                return;
            }

            AvailableLevels.OrderBy(o => o.Index);
        }

        protected override void LoadContent() {
            // Initialise game object containers.
            GameObjects = new List<GameObject>();
            addedGameObjects = new Stack<GameObject>();
            removedGameObjects = new Stack<GameObject>();
            Tiles = new Dictionary<Point, GameObject>();
            Lights = new List<Light>();

            // load all levels
            LoadLevels();

            // Create an input layout from the vertices
            base.LoadContent();
        }

        private void SaveLevelStatus(LevelStatus status, string levelId) {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            // Only save the high score if the hint wasn't used
            if (!status.HintUsed) {
                // get previous scores to compare with
                LevelStatus previousStatus = GetStartingLevelStatus(levelId);

                // update collisions high score
                if ((previousStatus.BestCollisions == null) ||
                    (status.Collisions < previousStatus.BestCollisions)) {
                    localSettings.Values[string.Format("HighScore_{0}_BestCollisions", levelId)] = status.Collisions;
                }

                // update time high score
                if ((previousStatus.BestTime == null) ||
                    (status.Time < previousStatus.BestTime)) {
                    localSettings.Values[string.Format("HighScore_{0}_BestTime", levelId)] = (int)(status.Time.TotalMilliseconds + 0.5);
                }
            }
        }

        public void EraseAllHighScores() {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            var highScores = localSettings.Values.Where(o => o.Key.StartsWith("HighScore", StringComparison.OrdinalIgnoreCase));

            foreach (var score in highScores) {
                localSettings.Values.Remove(score.Key);
            }
        }

        public LevelStatus GetStartingLevelStatus(string levelId) {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            var levelStatus = new LevelStatus();

            int? bestTimeMs = localSettings.Values[string.Format("HighScore_{0}_BestTime", levelId)] as int?;
            if (bestTimeMs != null) {
                levelStatus.BestTime = new TimeSpan(0, 0, 0, 0, (int)bestTimeMs);
            }

            levelStatus.BestCollisions = localSettings.Values[string.Format("HighScore_{0}_BestCollisions", levelId)] as int?;

            return levelStatus;
        }

        private void PlayerCompletedLevel(object sender, EventArgs args) {
            lock (isStartedLock) {
                //stop the current stopwatch and record the current time
                levelTimer.Stop();
                TimeSpan levelTime = new TimeSpan(0, 0, 0, 0, (int)levelTimer.ElapsedMilliseconds);
                CurrentLevelStatus.Time = levelTime;
                SaveLevelStatus(CurrentLevelStatus, CurrentLevel.LevelID);


                // find the next level higher than this one and load it
                int currentLevelIndex = AvailableLevels.IndexOf(CurrentLevel);
                if (currentLevelIndex >= 0) {
                    int nextLevelIndex = currentLevelIndex + 1;
                    if (nextLevelIndex < AvailableLevels.Count) {
                        // another level exists, start it
                        LevelInfo nextLevel = AvailableLevels[nextLevelIndex];
                        // load the level
                        ChangeLevel(nextLevel);
                        // don't complete the rest of this update
                    }
                    else {
                        // there are no more levels, exit back to the main menu
                        if (AllLevelsComplete != null) {
                            IsStarted = false;
                            AllLevelsComplete(this, null);
                        }
                    }
                }
            }
        }


        public void ChangeLevel(LevelInfo level) {
            lock (isStartedLock) {
                CurrentLevel = level;
                ChangeMap(level.Map);
            }
        }

        private void ChangeMap(Map map) {
            lock (isStartedLock) {
                // clear out all existing assets
                GameObjects.Clear();
                addedGameObjects.Clear();
                removedGameObjects.Clear();
                Tiles.Clear();
                Lights.Clear();

                // Solve the maze
                // This precomputes the shortest path to the maze exit for every
                // possible map position
                MazeSolver = new MazeSolver(this, map);
                MazeSolver.SolveMaze();

                // Create player
                Vector2 playerPos = map.GetWorldCoordinates(((Vector2)map.StartPosition) + 0.5f);

                if (Player != null) {
                    Player.CompletedLevel -= PlayerCompletedLevel;
                    Player.Collision -= Player_Collision;
                }

                Player = new Player(this, "Phong", playerPos);
                Player.CompletedLevel += PlayerCompletedLevel;
                Player.Collision += Player_Collision;

                GameObjects.Add(Player);

                // Generate the game objects that make up the floor
                LoadFloor(map);
                DefaultLightHeight = CurrentLevel.Map.Width / -2f * CurrentLevel.Map.MapUnitWidth;
                AddEnvironmentLights();
                RainbowModeOn = false;

                // create camera
                Camera = new Camera(this);

                // reset the current level stats
                ResetLevelStatus();
            }
        }

        private void Player_Collision(object sender, EventArgs e) {
            if (CurrentLevelStatus != null) {
                // add a collision to the current level stats
                CurrentLevelStatus.Collisions++;
            }
        }

        public void ResetLevelStatus() {
            CurrentLevelStatus = GetStartingLevelStatus(CurrentLevel.LevelID);
            this.levelTimer.Restart();
        }

        public void StopGame() {
            lock (isStartedLock) {
                this.IsStarted = false;
                this.GraphicsDevice.Clear(Color.Black);
                this.GraphicsDevice.Present();
            }
        }

        // Add Red, Green and Blue environment lights with different rate of movements
        private void AddEnvironmentLights() {
            var rate = new Vector3(RandomGenerator.NextFloat(0.001f, 0.005f), RandomGenerator.NextFloat(0.001f, 0.005f), RandomGenerator.NextFloat(0.001f, 0.005f));
            var red = new Light(this, Color.Red.ToColor4(), 1f, rate);
            rate = new Vector3(RandomGenerator.NextFloat(0.001f, 0.005f), RandomGenerator.NextFloat(0.001f, 0.005f), RandomGenerator.NextFloat(0.001f, 0.005f));
            var green = new Light(this, Color.Green.ToColor4(), 1f, rate);
            rate = new Vector3(RandomGenerator.NextFloat(0.001f, 0.005f), RandomGenerator.NextFloat(0.001f, 0.005f), RandomGenerator.NextFloat(0.001f, 0.005f));
            var blue = new Light(this, Color.Blue.ToColor4(), 1f, rate);
            Lights.Add(red);
            Lights.Add(green);
            Lights.Add(blue);
        }

        // Move the Red, Green and Blue Lights in different directions when rainbow mode is activated
        private void UpdateLightPositions(GameTime gameTime) {
            if (!RainbowModeOn) return;
            var time = gameTime.TotalGameTime.TotalMilliseconds;
            Vector3 rate = Lights[0].Rate;
            var deltaY = CurrentLevel.Map.Height / 2f * CurrentLevel.Map.MapUnitHeight;
            var deltaX = CurrentLevel.Map.Width / 2f * CurrentLevel.Map.MapUnitWidth; 
            Lights[0].LightPosition = new Vector3(deltaX * (float)Math.Sin(time * rate.X) + deltaX, deltaY * (float)Math.Cos(time * rate.Y) + deltaY, DefaultLightHeight * (float)Math.Cos(time * rate.Z));
            rate = Lights[1].Rate;
            Lights[1].LightPosition = new Vector3(deltaX * (float)Math.Cos(time * rate.Y) + deltaX, deltaY * (float)Math.Sin(time * rate.X) + deltaY, DefaultLightHeight * (float)Math.Cos(time * rate.Z));
            rate = Lights[2].Rate;
            Lights[2].LightPosition = new Vector3(deltaX * (float)Math.Cos(time * rate.Y) + deltaX, deltaY * (float)Math.Cos(time * rate.Y) + deltaY, DefaultLightHeight * (float)Math.Cos(time * rate.Z));
        }

        private void LoadFloor(Map map) {
            var width = map.MapUnitWidth;
            var height = map.MapUnitHeight;

            // Add background floor below maze board twice the size of map and always draw it
            var bg = (new FloorUnitGameObject(this, "Phong", "black_floor.dds", new Vector3(width * map.Width / 2f, height * map.Height / 2f, 6), map.Width * 2 * width, map.Height * 2 * height));
            bg.MustDraw = true;
            GameObjects.Add(bg);

            for (int i = 0; i < map.Width; i++) {
                for (int j = 0; j < map.Height; j++) {
                    var x = (i * width) + width / 2;
                    var y = (j * height) + height / 2;
                    var z = 0f;

                    Map.UnitType unitType = map[i, j];

                    if (unitType.HasFlag(Map.UnitType.Floor)) {
                        string texture = "wooden_floor.dds";
                        if (unitType.HasFlag(Map.UnitType.PlayerStart)) {
                            texture = "wooden_floor.dds";
                        }
                        else if (unitType.HasFlag(Map.UnitType.PlayerEnd)) {
                            texture = "wooden_floor_exit.dds";
                        }
                        else if (unitType.HasFlag(Map.UnitType.Rainbow)) {
                            texture = "rainbow.dds";
                        }

                        var floorObject = new FloorUnitGameObject(this, "Phong", texture, new Vector3(x, y, z), width, height);

                        GameObjects.Add(floorObject);
                        Tiles[new Point(i, j)] = floorObject;
                    }
                    else if (unitType.HasFlag(Map.UnitType.Wall)) {
                        z = -width / 2.0f;
                        var wallObject = new WallGameObject(this, "Phong", new Vector3(x, y, z), width);
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
            lock (isStartedLock) {
                if (IsStarted) {
                    KeyboardState = keyboardManager.GetState();
                    FlushAddedAndRemovedGameObjects();
                    float deltaTime = (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000f;

                    // H toggles hint
                    if (KeyboardState.IsKeyPressed(Keys.H)) {
                        MazeSolver.Enabled = !MazeSolver.Enabled;
                    }

                    // O and P do camera zoom
                    if (KeyboardState.IsKeyPressed(Keys.O)) {
                        Camera.Zoom(1.1f);
                    }
                    if (KeyboardState.IsKeyPressed(Keys.P)) {
                        Camera.Zoom(0.9f);
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

                    MazeSolver.Hint();
                    UpdateLightPositions(gameTime);

                    if (CurrentLevelStatus != null) {
                        CurrentLevelStatus.Time = new TimeSpan(0, 0, 0, 0, (int)levelTimer.ElapsedMilliseconds);
                        if (MazeSolver != null) {
                            if (MazeSolver.Enabled) {
                                CurrentLevelStatus.HintUsed = true;
                            }
                        }
                        GameOverlayPage.UpdateStats(CurrentLevelStatus);
                    }
                }
            }
            // Handle base.Update
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime) {
            if (IsRunning) {
                // Clears the screen with the Color.Black
                GraphicsDevice.Clear(Color.Black);
                if (!IsStarted) return;
                for (int i = 0; i < GameObjects.Count; i++) {
                    GameObjects[i].Draw(gameTime);
                }
            }
            // Handle base.Draw
            base.Draw(gameTime);
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
        private void FlushAddedAndRemovedGameObjects() {
            while (addedGameObjects.Count > 0) { GameObjects.Add(addedGameObjects.Pop()); }
            while (removedGameObjects.Count > 0) { GameObjects.Remove(removedGameObjects.Pop()); }
        }

        public void OnManipulationStarted(GestureRecognizer sender, ManipulationStartedEventArgs args) {
            // Pass Manipulation events to the game objects.
            if (GameObjects != null) {
                foreach (var gameObject in GameObjects) {
                    gameObject.OnManipulationStarted(sender, args);
                }
            }
        }

        public void OnTapped(GestureRecognizer sender, TappedEventArgs args) {
            // Pass Manipulation events to the game objects.
            if (GameObjects != null) {
                foreach (var gameObject in GameObjects) {
                    gameObject.OnTapped(sender, args);
                }
            }
        }

        public void OnManipulationUpdated(GestureRecognizer sender, ManipulationUpdatedEventArgs args) {
            if (GameObjects != null) {
                // Pass Manipulation events to the game objects.
                foreach (var gameObject in GameObjects) {
                    gameObject.OnManipulationUpdated(sender, args);
                }
            }
            if (Camera != null) {
                Camera.Zoom(-args.Delta.Scale + 2);
            }
        }

        public void OnManipulationCompleted(GestureRecognizer sender, ManipulationCompletedEventArgs args) {
            if (GameObjects != null) {
                // Pass Manipulation events to the game objects.
                foreach (var gameObject in GameObjects) {
                    gameObject.OnManipulationCompleted(sender, args);
                }
            }
        }

    }
}
