using System;
using UnityEngine;
using RPG.Dialogue;
using TMPro;
using UnityEngine.UI;

namespace RPG.UI
{
    public class DialogueUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _aiText;
        [SerializeField] private TextMeshProUGUI _conversantNameText;
        [SerializeField] private Button _nextButton;
        [SerializeField] private Button _quitButton;
        [SerializeField] private GameObject _aiResponse;
        [SerializeField] private Transform _choiceRoot;
        [SerializeField] private GameObject _choicePrefab;
        
        private PlayerConversant _playerConversant;

        private void Start()
        {
            _playerConversant = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerConversant>();
            _playerConversant.OnConversationUpdated += UpdateUI;
            _nextButton.onClick.AddListener(() => _playerConversant.Next());
            _quitButton.onClick.AddListener(() => _playerConversant.Quit());
            
            UpdateUI();
        }

        private void UpdateUI()
        {
            gameObject.SetActive(_playerConversant.IsDialogueActive());
            if (!_playerConversant.IsDialogueActive())
            {
                return;
            }
            
            _conversantNameText.SetText(_playerConversant.GetCurrentConversantName());
            _aiResponse.SetActive(!_playerConversant.IsChoosing());
            _choiceRoot.gameObject.SetActive(_playerConversant.IsChoosing());

            if (_playerConversant.IsChoosing())
            {
                BuildChoiceList();
            }
            else
            {
                _aiText.SetText(_playerConversant.GetText());
                _nextButton.gameObject.SetActive(_playerConversant.HasNext());
            }
        }

        private void BuildChoiceList()
        {
            foreach (Transform choice in _choiceRoot)
            {
                Destroy(choice.gameObject);
            }

            foreach (var choiceNode in _playerConversant.GetChoices())
            {
                var choiceGameObject = Instantiate(_choicePrefab, _choiceRoot);
                var choiceText = choiceGameObject.GetComponentInChildren<TextMeshProUGUI>();
                choiceText.SetText(choiceNode.GetText());
                var choiceButton = choiceGameObject.GetComponent<Button>();
                choiceButton.onClick.AddListener(() =>
                {
                    _playerConversant.SelectChoice(choiceNode);
                });
            }
        }
    }
}