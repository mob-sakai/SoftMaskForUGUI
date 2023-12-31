using System.Text;

namespace Coffee.UISoftMask.Internal.AssetModification
{
    internal interface ITextModifier
    {
        public bool ModifyText(StringBuilder sb, string text);
    }
}
