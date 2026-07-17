using SecureLearning.Client.Localization;
using TMPro;
using UnityEngine;

namespace SecureLearning.Client.UI
{
    /// <summary>Attach next to a TMP_Text; re-renders it in the active language whenever the flag toggle fires.</summary>
    [RequireComponent(typeof(TMP_Text))]
    public class LocalizedText : MonoBehaviour
    {
        [SerializeField] private string key;

        private TMP_Text label;
        private LocalizationService localizationService;
        private LanguageToggleController controller;

        public void Initialize(LocalizationService service, LanguageToggleController languageToggleController)
        {
            localizationService = service;
            controller = languageToggleController;
            label = GetComponent<TMP_Text>();
            controller.LanguageChanged += Render;
            Render(controller.Current);
        }

        private void OnDestroy()
        {
            if (controller != null)
            {
                controller.LanguageChanged -= Render;
            }
        }

        private void Render(Language language)
        {
            label.text = localizationService.Translate(key, language);
        }
    }
}
