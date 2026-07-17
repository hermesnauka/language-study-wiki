using SecureLearning.Client.Localization;
using UnityEngine;
using UnityEngine.UI;

namespace SecureLearning.Client.UI
{
    /// <summary>
    /// Scene adapter for US-1.1's flag-toggle button: wires a UGUI Button click to
    /// <see cref="LanguageToggleController.ToggleAsync"/> and swaps the flag sprite plus
    /// every registered <see cref="LocalizedText"/> label when the language changes.
    /// The actual GameObject/Button/Sprite wiring happens in the Unity Editor scene —
    /// this only assumes the references are assigned there.
    /// </summary>
    public class FlagToggleView : MonoBehaviour
    {
        [SerializeField] private Button toggleButton;
        [SerializeField] private Image flagImage;
        [SerializeField] private Sprite englishFlagSprite;
        [SerializeField] private Sprite polishFlagSprite;

        private LanguageToggleController controller;

        public LanguageToggleController Controller => controller;

        public void Initialize(LanguageToggleController languageToggleController)
        {
            controller = languageToggleController;
            controller.LanguageChanged += OnLanguageChanged;
            toggleButton.onClick.AddListener(OnToggleClicked);
            OnLanguageChanged(controller.Current);
        }

        private void OnDestroy()
        {
            if (controller != null)
            {
                controller.LanguageChanged -= OnLanguageChanged;
            }
        }

        private void OnToggleClicked()
        {
            _ = controller.ToggleAsync();
        }

        private void OnLanguageChanged(Language language)
        {
            flagImage.sprite = language == Language.English ? englishFlagSprite : polishFlagSprite;
        }
    }
}
