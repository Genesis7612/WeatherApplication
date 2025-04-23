using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace WeatherApplication
{
    public static class VisualElementExtensions
    {
        
        public static async Task ToColor(this VisualElement element, Color toColor, uint duration)
        {
            var currentColor = element.BackgroundColor; 
            await element.ColorTo(currentColor, toColor, duration); 
        }

        
        public static async Task ColorTo(this VisualElement element, Color fromColor, Color toColor, uint duration)
        {
            var startTime = DateTime.Now; 

            
            while (true)
            {
                var elapsed = (DateTime.Now - startTime).TotalMilliseconds;
                var ratio = Math.Min(elapsed / duration, 1); 

                
                byte fromR, fromG, fromB;
                fromColor.ToRgb(out fromR, out fromG, out fromB);

                byte toR, toG, toB;
                toColor.ToRgb(out toR, out toG, out toB);

                
                var r = (int)(fromR + ratio * (toR - fromR));
                var g = (int)(fromG + ratio * (toG - fromG));
                var b = (int)(fromB + ratio * (toB - fromB));

                
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    element.BackgroundColor = Color.FromRgb(r, g, b);
                });

                if (ratio >= 1) break; 

                await Task.Delay(16); 
            }
        }
    }
}