using UnityEngine;

namespace TheFall.Presentation.Table
{
    [DisallowMultipleComponent]
    public sealed class PrototypeSeatView : MonoBehaviour
    {
        public int LogicalIndex { get; private set; }

        public int TeamIndex { get; private set; }

        public float AnchorAngleDegrees { get; private set; }

        public string DisplayName { get; private set; }

        public bool IsLocal { get; private set; }

        public bool IsHandPrivate { get; private set; }

        public bool IsActive { get; private set; }

        public Transform HandAnchor { get; private set; }

        public Transform CapturedPileAnchor { get; private set; }

        public void Configure(
            PrototypeSeatLayout layout,
            bool isActive,
            Transform handAnchor,
            Transform capturedPileAnchor)
        {
            LogicalIndex = layout.LogicalIndex;
            TeamIndex = layout.TeamIndex;
            AnchorAngleDegrees = layout.AnchorAngleDegrees;
            DisplayName = layout.DisplayName;
            IsLocal = layout.IsLocal;
            IsHandPrivate = layout.IsHandPrivate;
            IsActive = isActive;
            HandAnchor = handAnchor;
            CapturedPileAnchor = capturedPileAnchor;
        }
    }
}
