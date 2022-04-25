using System;

namespace RoslynEditorDarkTheme.Events
{
    public class SendTextEvent
    {
        public event EventHandler<string> OnTextReceived;

        public void SendText(string text)
        {
            TextReceived(text);
        }

        protected virtual void TextReceived(string text)
        {
            OnTextReceived?.Invoke(this, text);
        }
    }
}
