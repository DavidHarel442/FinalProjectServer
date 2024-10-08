using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectServer
{
    internal class CaptchaGenerator
    {// taken from claude
     // this is the class that will generate the captcha
        /// <summary>
        /// Stores the current CAPTCHA text for validation.
        /// </summary>
        private string currentCaptcha;

        /// <summary>
        /// Random number generator for CAPTCHA creation.
        /// </summary>
        private readonly Random random;

        /// <summary>
        /// Initializes a new instance of the CaptchaGenerator class.
        /// </summary>
        public CaptchaGenerator()
        {
            random = new Random();
        }

        /// <summary>
        /// Generates a random CAPTCHA text.
        /// </summary>
        /// <returns></returns>A string containing the generated CAPTCHA text.
        public string GenerateCaptchaText()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            // Generate a 5-character random string from the chars set
            currentCaptcha = new string(Enumerable.Repeat(chars, 5)
                .Select(s => s[random.Next(s.Length)]).ToArray());
            return currentCaptcha;
        }

        /// <summary>
        /// Generates a CAPTCHA image based on the provided text.
        /// </summary>
        /// <param name="captchaText">The text to be rendered in the CAPTCHA image.</param>
        /// <returns></returns>A byte array representing the PNG image of the CAPTCHA.
        public byte[] GenerateCaptchaImage(string captchaText)
        {
            using (var bitmap = new Bitmap(130, 40))
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.Clear(Color.White);

                // Add background noise
                using (var brush = new HatchBrush(HatchStyle.SmallConfetti, Color.LightGray, Color.White))
                {
                    graphics.FillRectangle(brush, new Rectangle(0, 0, bitmap.Width, bitmap.Height));
                }

                // Add text
                using (var font = new Font("Arial", 18, FontStyle.Bold))
                {
                    graphics.DrawString(captchaText, font, Brushes.Black, 5, 5);
                }

                // Add foreground noise (random lines)
                for (int i = 0; i < 50; i++)
                {
                    int x1 = random.Next(bitmap.Width);
                    int y1 = random.Next(bitmap.Height);
                    int x2 = random.Next(bitmap.Width);
                    int y2 = random.Next(bitmap.Height);
                    graphics.DrawLine(Pens.Gray, x1, y1, x2, y2);
                }

                // Distort the image
                using (var copy = (Bitmap)bitmap.Clone())
                {
                    for (int y = 0; y < bitmap.Height; y++)
                    {
                        for (int x = 0; x < bitmap.Width; x++)
                        {
                            // Apply sine wave distortion to x and y coordinates
                            int newX = (int)(x + (Math.Sin(y / 8.0) * 4));
                            int newY = (int)(y + (Math.Cos(x / 8.0) * 4));
                            if (newX >= 0 && newX < bitmap.Width && newY >= 0 && newY < bitmap.Height)
                            {
                                bitmap.SetPixel(x, y, copy.GetPixel(newX, newY));
                            }
                        }
                    }
                }

                // Convert the bitmap to a byte array
                using (var memoryStream = new MemoryStream())
                {
                    bitmap.Save(memoryStream, ImageFormat.Png);
                    return memoryStream.ToArray();
                }
            }
        }

        /// <summary>
        /// Validates the user's input against the current CAPTCHA.
        /// </summary>
        /// <param name="userInput">The user's input to be validated.</param>
        /// <returns>True if the input matches the CAPTCHA, false otherwise.</returns>
        public bool ValidateCaptcha(string userInput)
        {
            // Compare user input with the current CAPTCHA, ignoring case
            return string.Equals(userInput, currentCaptcha, StringComparison.OrdinalIgnoreCase);
        }
    }
}

