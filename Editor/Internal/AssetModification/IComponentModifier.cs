using UnityEngine;

namespace Coffee.UISoftMaskInternal.AssetModification
{
    internal interface IComponentModifier
    {
        bool isModified { get; }
        bool ModifyComponent(GameObject root, bool dryRun);
        string Report();
    }
}
