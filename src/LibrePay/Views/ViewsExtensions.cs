using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace LibrePay.Views
{
    public static class ViewsExtensions
    {
        public static Task<bool> ColorToAsync(this VisualElement self, Color fromColor, Color toColor,
            Action<Color> callback = null, uint length = 250, Easing easing = null)
        {
            Color Transform(double t)
            {
                return Color.FromRgba(
                    fromColor.R + t * (toColor.R - fromColor.R)
                    , fromColor.G + t * (toColor.G - fromColor.G)
                    , fromColor.B + t * (toColor.B - fromColor.B)
                    , fromColor.A + t * (toColor.A - fromColor.A)
                );
            }

            return AnimateAsync(self, "ColorTo", Transform, callback, length, easing: easing);
        }

        public static Animation ColorTo(
            this VisualElement self
            , Color fromColor
            , Color toColor
            , Action<Color> callback
            , Easing easing = null
        )
        {
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));

            Color Transform(double t)
            {
                return Color.FromRgba(
                    fromColor.R + t * (toColor.R - fromColor.R)
                    , fromColor.G + t * (toColor.G - fromColor.G)
                    , fromColor.B + t * (toColor.B - fromColor.B)
                    , fromColor.A + t * (toColor.A - fromColor.A)
                );
            }

            return new Animation(d => callback(Transform(d)), easing: easing);
        }

        public static void CancelColorTo(this VisualElement self)
        {
            self.AbortAnimation("ColorTo");
        }

        public static Task<bool> RunAnimationAsync(this VisualElement element, string name, Animation animation,
            uint rate = 16, uint length = 250, Easing easing = null)
        {
            var taskCompletionSource = new TaskCompletionSource<bool>();

            animation.Commit(element, name, rate, length, easing, (v, c) => taskCompletionSource.SetResult(c));

            return taskCompletionSource.Task;
        }

        public static Task<bool> AnimateAsync<T>(this VisualElement element, string name, Func<double, T> transform,
            Action<T> callback = null, uint rate = 16, uint length = 250, Easing easing = null)
        {
            easing = easing ?? Easing.Linear;
            callback = callback ?? (_ => { });
            var taskCompletionSource = new TaskCompletionSource<bool>();

            element.Animate(name, transform, callback, rate, length, easing,
                (v, c) => taskCompletionSource.SetResult(c));

            return taskCompletionSource.Task;
        }
    }
}
