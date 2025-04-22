using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace WeatherApplication
{
    public static class VisualElementExtensions
    {
        // Extension method for color transition
        public static async Task ToColor(this VisualElement element, Color toColor, uint duration)
        {
            var currentColor = element.BackgroundColor; // Get the current background color
            await element.ColorTo(currentColor, toColor, duration); // Call the color transition logic
        }

        // Helper method to calculate and apply the color transition
        public static async Task ColorTo(this VisualElement element, Color fromColor, Color toColor, uint duration)
        {
            var startTime = DateTime.Now; // Start time of the transition

            // Loop to create the color transition effect
            while (true)
            {
                var elapsed = (DateTime.Now - startTime).TotalMilliseconds;
                var ratio = Math.Min(elapsed / duration, 1); // Ensure ratio doesn't exceed 1

                // Extract the RGB components using the ToRgb method
                byte fromR, fromG, fromB;
                fromColor.ToRgb(out fromR, out fromG, out fromB);

                byte toR, toG, toB;
                toColor.ToRgb(out toR, out toG, out toB);

                // Interpolate the colors
                var r = (int)(fromR + ratio * (toR - fromR));
                var g = (int)(fromG + ratio * (toG - fromG));
                var b = (int)(fromB + ratio * (toB - fromB));

                // Apply the color interpolation on the main thread
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    element.BackgroundColor = Color.FromRgb(r, g, b);
                });

                if (ratio >= 1) break; // Stop the loop once the transition is complete

                await Task.Delay(16); // Wait 16ms for smoother transition (roughly 60fps)
            }
        }
    }
}