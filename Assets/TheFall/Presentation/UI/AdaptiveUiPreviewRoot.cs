using System;
using UnityEngine.UIElements;

namespace TheFall.Presentation.UI
{
    [UxmlElement]
    public partial class AdaptiveUiPreviewRoot : VisualElement
    {
        private static readonly string[] PreviewClasses =
        {
            "preview-desktop",
            "preview-phone-landscape",
            "preview-tablet-landscape",
        };

        private AdaptiveUiProfile _previewProfile = AdaptiveUiProfile.PhoneLandscape;

        public AdaptiveUiPreviewRoot()
        {
            AddToClassList("adaptive-preview-root");
            UseAuthoringLayout();
        }

        [UxmlAttribute]
        public AdaptiveUiProfile PreviewProfile
        {
            get => _previewProfile;
            set
            {
                _previewProfile = value;
                UseAuthoringLayout();
            }
        }

        public void UseAuthoringLayout()
        {
            AddToClassList("authoring-preview-root");
            AddToClassList("screen-root");
            AdaptiveUiFoundation.ApplyProfileClass(this, _previewProfile);

            foreach (var className in PreviewClasses)
            {
                RemoveFromClassList(className);
            }

            switch (_previewProfile)
            {
                case AdaptiveUiProfile.Desktop:
                    AddToClassList("preview-desktop");
                    break;
                case AdaptiveUiProfile.PhoneLandscape:
                    AddToClassList("preview-phone-landscape");
                    break;
                case AdaptiveUiProfile.TabletLandscape:
                    AddToClassList("preview-tablet-landscape");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(_previewProfile));
            }
        }

        public void UseSceneLayout()
        {
            RemoveFromClassList("authoring-preview-root");
            RemoveFromClassList("screen-root");
            AdaptiveUiFoundation.RemoveProfileClasses(this);
            foreach (var className in PreviewClasses)
            {
                RemoveFromClassList(className);
            }
        }
    }
}
