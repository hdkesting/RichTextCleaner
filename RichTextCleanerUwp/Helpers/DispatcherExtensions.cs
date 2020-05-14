using System;
using System.Runtime.CompilerServices;
using Windows.UI.Core;

namespace RichTextCleanerUwp.Helpers
{
    public static class DispatcherExtensions
    {
        /// <summary>
        /// Switches the execution to the UI thread.
        /// </summary>
        /// <param name="dispatcher">The dispatcher.</param>
        /// <remarks>Inspired by https://medium.com/criteo-labs/switching-back-to-the-ui-thread-in-wpf-uwp-in-modern-c-5dc1cc8efa5e</remarks>
        /// <returns></returns>
        public static SwitchToUiAwaitable SwitchToUi(this CoreDispatcher dispatcher)
        {
            return new SwitchToUiAwaitable(dispatcher);
        }

        public struct SwitchToUiAwaitable : INotifyCompletion
        {
            private readonly CoreDispatcher _dispatcher;

            public SwitchToUiAwaitable(CoreDispatcher dispatcher)
            {
                _dispatcher = dispatcher;
            }

            public SwitchToUiAwaitable GetAwaiter()
            {
                return this;
            }

            public void GetResult()
            {
                // no action needed
            }

            public bool IsCompleted => _dispatcher.HasThreadAccess;

            public async void OnCompleted(Action continuation)
            {
                await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => continuation());
            }
        }
    }
}
