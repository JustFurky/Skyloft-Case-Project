using UnityEngine;

namespace SkyloftGame.UI
{
    public interface IButton
    {
        Sprite Icon         { get; set; }
        string Label        { get; set; }
        bool   Interactable { get; set; }
    }
}
