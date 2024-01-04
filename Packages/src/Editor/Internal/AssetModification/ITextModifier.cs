using System.Text;

namespace Coffee.UISoftMaskInternal.AssetModification
{
    internal interface ITextModifier
    {
        bool ModifyText(StringBuilder sb, string text);
    }
}
