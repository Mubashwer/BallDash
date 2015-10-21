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

using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using SharpDX;

namespace Project.Menus {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    // TASK 4: Instructions Page
    public sealed partial class MainMenu
    {
        private MainPage parent;
        public GamePage GamePage { get; set; }
        public MainMenu(MainPage parent)
        {
            this.parent = parent;
            InitializeComponent();
        }

        // TASK 2: Starts the game.  Not that it seems easier to simply move the game.Run(this) command to this function,
        // however this seems to result in a reduction in texture quality on some machines.  Not sure why this is the case
        // but this is an easy workaround.  Not we are also making the command button invisible after it is clicked
        private void StartGame(object sender, RoutedEventArgs e) {
            GamePage = new GamePage(parent);
            parent.Children.Add(GamePage);
            parent.Children.Remove(this);
            parent.Game.Started = true;
            parent.Game.GameOverlayPage = GamePage;
        }

        private void LoadInstructions(object sender, RoutedEventArgs e)
        {
            parent.Children.Add(new Instructions(parent));
            parent.Children.Remove(this);
 
        }

        private void LoadSettings(object sender, RoutedEventArgs e) {
            parent.Children.Add(new Settings(parent));
            parent.Children.Remove(this);
        }
    }
}
