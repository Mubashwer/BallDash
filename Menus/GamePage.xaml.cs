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
    public sealed partial class GamePage
    {
        private MainPage parent;
        private MainMenu menu;
       
        public GamePage(MainPage parent)
        {
            this.parent = parent;
            InitializeComponent();
        }
        public void UpdateStats(string stats) {
            txtStats.Text = stats;
        }

        // Button for Hint
        private void ChangeHint(object sender, RoutedEventArgs e)
        {
            if (!parent.Game.MazeSolver.Enabled)
            {
                parent.Game.MazeSolver.Enabled = true;
            }
            else
            {
                parent.Game.MazeSolver.Enabled = false;
            }
        }

        private void Back(object sender, RoutedEventArgs e)
        {
            parent.Game.Exit();
            parent.Game.GraphicsDevice.Clear(Color.Black);
            parent.Game.Dispose();
            parent.Children.Add(new MainMenu(parent));
            parent.Children.Remove(this);
        }

    }
}
