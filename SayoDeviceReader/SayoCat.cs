using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
// TODO: Add key config support, add more keys.
namespace SayoDeviceReader
{
    

    internal class SayoCat
    {
        // See https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwindowpos
        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private const uint TOPMOST_FLAGS = 0x0002 | 0x0001;


        public static uint TargetFramerate = 60;
        public bool isRunning = true;
        private static byte[] keyConfig = {0, 1, 6, 7};
        private unsafe SayoDevice* sayoHandle;
        private static VideoMode rendererMode = new VideoMode(new Vector2u(1189, 669));
        private RenderWindow mainRenderer;

        // Textures
        private static Texture[] rightArmTextures = {new Texture(".\\sprites\\right_up.png"), new Texture(".\\sprites\\right_down_key1.png"), new Texture(".\\sprites\\right_down_key2.png"), new Texture(".\\sprites\\right_down_bothkeys.png") };
        private static Texture[] leftArmTextures = {
            new Texture(".\\sprites\\left_up.png"),
            new Texture(".\\sprites\\left_down_key1.png"),
            new Texture(".\\sprites\\left_down_key2.png"),
            new Texture(".\\sprites\\left_down_bothkeys.png")
        };

        // Sprites
        private static Sprite catBase = new Sprite(new Texture(".\\sprites\\catBase.png"));
        private static Sprite catLeftArm = new Sprite(leftArmTextures[0]);
        private static Sprite catRightArm = new Sprite(rightArmTextures[0]);
        private static Sprite[] keyboardKeys = {
            new Sprite(new Texture(".\\sprites\\left_key1_mask.png")),
            new Sprite(new Texture(".\\sprites\\left_key2_mask.png")),
            new Sprite(new Texture(".\\sprites\\right_key2_mask.png")),
            new Sprite(new Texture(".\\sprites\\right_key1_mask.png"))
            

        };

        // Colors
        private Color keyFullyActivated = new Color(255, 29, 72);
        private Color keyActivated = new Color(0, 255, 255);

        public unsafe SayoCat(SayoDevice* sayoHandle)
        {
            this.sayoHandle = sayoHandle;

            // Create renderer on another thread. (Not much can be created in this constructor if we want it to be on a seperate thread.)
            Task.Run(RenderingThread);
            
        }
        private void RenderingThread()
        {
            // Setup renderer.
            mainRenderer = new RenderWindow(rendererMode, "Sayo Cat");
            mainRenderer.SetMaximumSize(new Vector2u(594, 334));
            SetWindowPos(mainRenderer.NativeHandle, HWND_TOPMOST, 0, 0, 0, 0, TOPMOST_FLAGS);

            // Setup the rendering window.
            mainRenderer.SetFramerateLimit(TargetFramerate);

            // Handle any events.
            mainRenderer.Closed += MainRenderer_Closed;
            mainRenderer.Resized += MainRender_Resized;

            // Main rendering loop.
            while(mainRenderer.IsOpen)
            {
                isRunning = true;
                // Fire any events that occured.
                mainRenderer.DispatchEvents();

                // Clean up previous frame.
                mainRenderer.Clear(Color.White);

                // Draw logic.
                DrawCat();

                // Update the display.
                mainRenderer.Display();

                // Sleep the framerate.
                Thread.Sleep((int)(1000 / TargetFramerate));
            }

            isRunning = false;
            Console.WriteLine("Renderer has finished.");
        }

        private void DrawCat()
        {
            // Draw the base sprite.
            mainRenderer.Draw(catBase);

            // Calculate both the colour of the keys and the arm state.
            byte armState = 0;
            for (int currentKey = 0; currentKey < keyConfig.Length; currentKey++)
            {
                bool isLeft = currentKey < ( (keyConfig.Length / 2) );
                byte keyState;
                unsafe
                {
                    keyState = sayoHandle->analogKeyStates[keyConfig[currentKey]];
                }
                double percentage = Math.Clamp((double)keyState / 79.0, 0, 1);
                
                if(percentage > 0.01)
                {
                    armState += (isLeft) ? (byte)(0x10 * (currentKey + 1)) : (byte)(0x1 + currentKey - 2 );
                    keyboardKeys[currentKey].Color = LerpColor(keyActivated, keyFullyActivated, percentage);
                    continue;
                }

                keyboardKeys[currentKey].Color = Color.White;
            }

            catLeftArm.Texture = leftArmTextures[armState & 0x0F]; // Masks 0000 1111
            catRightArm.Texture = rightArmTextures[armState >> 4]; // Shifts 1111 1111 -> 0000 1111

            // Draw keys & arms.
            for (int i = 0; i < keyboardKeys.Length; i++)
            {
                mainRenderer.Draw(keyboardKeys[i]);
            }
            mainRenderer.Draw(catLeftArm);
            mainRenderer.Draw(catRightArm);

        }

        private Color LerpColor(Color startingColor, Color endingColor, double amount)
        {
            byte r = (byte) (startingColor.R + (endingColor.R - startingColor.R) * amount);
            byte g = (byte)(startingColor.G + (endingColor.G - startingColor.G) * amount);
            byte b = (byte)(startingColor.B + (endingColor.B - startingColor.B) * amount);

            Color finalColor = new Color(r, g, b);
            return finalColor;
        }

        private void MainRenderer_Closed(object? sender, EventArgs e)
        {
            // Incase any future use requires more than just closing the renderer.
            mainRenderer.Close();
        }

        private void MainRender_Resized(object? sender, SizeEventArgs e)
        {
        }
    }
}
