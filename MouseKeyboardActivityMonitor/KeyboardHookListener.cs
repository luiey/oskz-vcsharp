using System;
using System.Windows.Forms;
using MouseKeyboardActivityMonitor.WinApi;

namespace MouseKeyboardActivityMonitor
{
    /// <summary>
    /// This class monitors all keyboard activities and provides appropriate events.
    /// </summary>
    public class KeyboardHookListener : BaseHookListener
    {
        /// <summary>
        /// Initializes a new instance of <see cref="KeyboardHookListener"/>.
        /// </summary>
        /// <param name="hooker">Depending on this parameter the listener hooks either application or global keyboard events.</param>
        /// <remarks>Hooks are not active after instantiation. You need to use either <see cref="BaseHookListener.Enabled"/> property or call <see cref="BaseHookListener.Start"/> method.</remarks>
        public KeyboardHookListener(Hooker hooker)
            : base(hooker)
        {
        }

        /// <summary>
        /// This method processes the data from the hook and initiates event firing.
        /// </summary>
        /// <param name="wParam">The first Windows Messages parameter.</param>
        /// <param name="lParam">The second Windows Messages parameter.</param>
        /// <returns>
        /// True - The hook will be passed along to other applications.
        /// <para>
        /// False - The hook will not be given to other applications, effectively blocking input.
        /// </para>
        /// </returns>
        protected override bool ProcessCallback(int wParam, IntPtr lParam)
        {
            KeyEventArgsExt e = KeyEventArgsExt.FromRawData(wParam, lParam, IsGlobal);

            // not allow
            Keys[] notAllowedKeys = {  Keys.LControlKey, Keys.RControlKey, Keys.LMenu, Keys.RMenu, Keys.LWin, Keys.RWin, Keys.Apps, Keys.Alt, Keys.Control, Keys.Tab, Keys.F1, Keys.F2, Keys.F3, Keys.F4, Keys.F5, Keys.F6, Keys.F7, Keys.F8, Keys.F9, Keys.F10, Keys.F11, Keys.F12 };

            Console.WriteLine(" * Oh! : " + e.KeyData);

            if ((Array.IndexOf(notAllowedKeys, e.KeyData) != -1) || e.Control || e.Alt || (e.Shift && e.Alt))
            {
                Console.WriteLine(" * Block1 : " + e.KeyData);
                return e.Handled;
            }
            else if (e.KeyData.ToString() == "Tab, Shift" || e.KeyData.ToString() == "Shift, Tab")
            {
                // not allow modifier with tab!
                Console.WriteLine(" * Block2 : " + e.KeyData);
                return e.Handled;
            }
            else 
            {
                // for debug purpose
                string st = e.KeyData.ToString();
                string[] s = st.Split(',');
                Console.WriteLine("st:" + st);
                Console.WriteLine("s:" + s.Length);

                foreach (var keyData in s)
                {
                    Console.WriteLine(" * keyData : [" + keyData + "]");
                }

                // allow
                InvokeKeyDown(e);
                InvokeKeyPress(wParam, lParam);
                InvokeKeyUp(e);
                return !e.Handled;
            }
        }

        /// <summary>
        /// Returns the correct hook id to be used for <see cref="HookNativeMethods.SetWindowsHookEx"/> call.
        /// </summary>
        /// <returns>WH_KEYBOARD (0x02) or WH_KEYBOARD_LL (0x13) constant.</returns>
        protected override int GetHookId()
        {
            return IsGlobal ? 
                GlobalHooker.WH_KEYBOARD_LL : 
                AppHooker.WH_KEYBOARD;
        }

        /// <summary>
        /// Occurs when a key is preseed. 
        /// </summary>
        public event KeyEventHandler KeyDown;

        private void InvokeKeyDown(KeyEventArgsExt e)
        {
            KeyEventHandler handler = KeyDown;
            if (handler == null || e.Handled || !e.IsKeyDown) { return; }
            handler(this, e);
        }

        /// <summary>
        /// Occurs when a key is pressed.
        /// </summary>
        /// <remarks>
        /// Key events occur in the following order: 
        /// <list type="number">
        /// <item>KeyDown</item>
        /// <item>KeyPress</item>
        /// <item>KeyUp</item>
        /// </list>
        ///The KeyPress event is not raised by noncharacter keys; however, the noncharacter keys do raise the KeyDown and KeyUp events. 
        ///Use the KeyChar property to sample keystrokes at run time and to consume or modify a subset of common keystrokes. 
        ///To handle keyboard events only in your application and not enable other applications to receive keyboard events, 
        ///set the <see cref="KeyPressEventArgs.Handled"/> property in your form's KeyPress event-handling method to <b>true</b>. 
        /// </remarks>
        public event KeyPressEventHandler KeyPress;

        private void InvokeKeyPress(int wParam, IntPtr lParam)
        {
            InvokeKeyPress(KeyPressEventArgsExt.FromRawData(wParam, lParam, IsGlobal));
        }

        private void InvokeKeyPress(KeyPressEventArgsExt e)
        {
            KeyPressEventHandler handler = KeyPress;
            if (handler == null || e.Handled || e.IsNonChar) { return; }
            handler(this, e);
        }

        /// <summary>
        /// Occurs when a key is released. 
        /// </summary>
        public event KeyEventHandler KeyUp;

        private void InvokeKeyUp(KeyEventArgsExt e)
        {
            KeyEventHandler handler = KeyUp;
            if (handler == null || e.Handled || !e.IsKeyUp) { return; }
            handler(this, e);
        }


        /// <summary>
        /// Method to be used from <see cref="Dispose"/> and finalizer.
        /// Override this method to release subclass sepcific references.
        /// </summary>
        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                KeyPress = null;
                KeyDown = null;
                KeyUp = null;
            }

            base.Dispose(isDisposing);
        }
    }
}