using UnityEngine;

namespace Coffee.UISoftMask.Internal.AssetModification
{
    internal interface IComponentModifier
    {
        bool isModified { get; }
        bool ModifyComponent(GameObject root, bool dryRun);
        string Report();
    }
}
