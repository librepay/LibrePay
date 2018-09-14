using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Input;
using Xamarin.Forms;

namespace BitcoinPOS_App.Behaviors
{
    /// <remarks>https://stackoverflow.com/a/51247520/4084610</remarks>
    public class LongPressBehavior : Behavior<Button>
    {
        private readonly object _syncObject = new object();

        //the timeout value for long press
        public int DurationInMs
        {
            get => (int) GetValue(DurationInMsProperty);
            set => SetValue(DurationInMsProperty, value);
        }

        //timer to track long press
        private Timer _timer;

        //whether the button was released after press
        private volatile bool _isReleased;

        /// <summary>
        /// Occurs when the associated button is long pressed.
        /// </summary>
        public event EventHandler LongPressed;

        public static readonly BindableProperty DurationInMsProperty = BindableProperty.Create(nameof(DurationInMs),
            typeof(int), typeof(LongPressBehavior), 1000);

        public static readonly BindableProperty CommandProperty = BindableProperty.Create(nameof(Command),
            typeof(ICommand), typeof(LongPressBehavior), default(ICommand));

        public static readonly BindableProperty CommandParameterProperty =
            BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(LongPressBehavior));

        /// <summary>
        /// Gets or sets the command parameter.
        /// </summary>
        public object CommandParameter
        {
            get => GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        /// <summary>
        /// Gets or sets the command.
        /// </summary>
        public ICommand Command
        {
            get => (ICommand) GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        protected override void OnAttachedTo(Button button)
        {
            base.OnAttachedTo(button);
            button.Pressed += Button_Pressed;
            button.Released += Button_Released;
        }

        protected override void OnDetachingFrom(Button button)
        {
            base.OnDetachingFrom(button);
            button.Pressed -= Button_Pressed;
            button.Released -= Button_Released;
        }

        /// <summary>
        /// DeInitializes and disposes the timer.
        /// </summary>
        private void DeInitializeTimer()
        {
            lock (_syncObject)
            {
                if (_timer == null)
                {
                    return;
                }

                _timer.Change(Timeout.Infinite, Timeout.Infinite);
                _timer.Dispose();
                _timer = null;
                Debug.WriteLine("Timer disposed...");
            }
        }

        /// <summary>
        /// Initializes the timer.
        /// </summary>
        private void InitializeTimer()
        {
            lock (_syncObject)
            {
                _timer = new Timer(Timer_Elapsed, null, DurationInMs, Timeout.Infinite);
            }
        }

        private void Button_Pressed(object sender, EventArgs e)
        {
            _isReleased = false;
            InitializeTimer();
        }

        private void Button_Released(object sender, EventArgs e)
        {
            _isReleased = true;
            DeInitializeTimer();
        }

        protected virtual void OnLongPressed()
        {
            var handler = LongPressed;
            handler?.Invoke(this, EventArgs.Empty);
            if (Command != null && Command.CanExecute(CommandParameter))
            {
                Command.Execute(CommandParameter);
            }
        }

        public LongPressBehavior()
        {
            _isReleased = true;
        }

        private void Timer_Elapsed(object state)
        {
            DeInitializeTimer();
            if (_isReleased)
            {
                return;
            }

            Device.BeginInvokeOnMainThread(OnLongPressed);
        }
    }
}
