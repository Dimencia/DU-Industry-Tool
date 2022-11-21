using Krypton.Toolkit;

namespace DU_Industry_Tool
{
    // Derivates to toggle visibility based on set text
    public class KLabel : KryptonLabel
    {
        public void SetText(string value = null, bool visible = true)
        {
            if (value != null)
            {
                this.Values.Text = value;
            }
            Visible = visible;
        }

        public void Hide(string value = null)
        {
            SetText(value, false);
        }

        public override string Text
        {
            get => this.Values.Text;
            set => SetText(value);
        }
    }

    public class KLinkLabel : KryptonLinkLabel
    {
        public void SetText(string value = null, bool visible = true)
        {
            if (value != null)
            {
                this.Values.Text = value;
            }
            Visible = visible;
        }

        public void Hide(string value = null)
        {
            SetText(value, false);
        }

        public override string Text
        {
            get => this.Values.Text;
            set => SetText(value);
        }
    }
}
