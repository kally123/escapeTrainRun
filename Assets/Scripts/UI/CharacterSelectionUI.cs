using UnityEngine;
using UnityEngine.UI;
using EscapeTrainRun.Characters;
using EscapeTrainRun.Core;
using EscapeTrainRun.Events;

namespace EscapeTrainRun.UI
{
    /// <summary>
    /// UI for character selection screen.
    /// Displays all characters, their stats, and unlock status.
    /// </summary>
    public class CharacterSelectionUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform characterListContainer;
        [SerializeField] private GameObject characterCardPrefab;
        [SerializeField] private CharacterPreviewUI previewUI;

        [Header("Selected Character Display")]
        [SerializeField] private Image selectedPortrait;
        [SerializeField] private Text selectedName;
        [SerializeField] private Text selectedDescription;
        [SerializeField] private Text selectedStats;
        [SerializeField] private Text selectedAbility;
        [SerializeField] private Image rarityBorder;

        [Header("Buttons")]
        [SerializeField] private Button selectButton;
        [SerializeField] private Button unlockButton;
        [SerializeField] private Text unlockButtonText;
        [SerializeField] private Button backButton;

        [Header("Audio")]
        [SerializeField] private AudioClip navigateSound;
        [SerializeField] private AudioClip selectSound;
        [SerializeField] private AudioClip unlockSound;
        [SerializeField] private AudioClip errorSound;

        // State
        private CharacterManager characterManager;
        private CharacterData selectedCharacter;
        private CharacterCard[] characterCards;

        private void OnEnable()
        {
            GameEvents.OnCharacterUnlocked += OnCharacterUnlocked;
        }

        private void OnDisable()
        {
            GameEvents.OnCharacterUnlocked -= OnCharacterUnlocked;
        }

        private void Start()
        {
            characterManager = CharacterManager.Instance;

            SetupButtons();
            PopulateCharacterList();
            SelectInitialCharacter();
        }

        private void SetupButtons()
        {
            if (selectButton != null)
            {
                selectButton.onClick.AddListener(OnSelectButtonClicked);
            }

            if (unlockButton != null)
            {
                unlockButton.onClick.AddListener(OnUnlockButtonClicked);
            }

            if (backButton != null)
            {
                backButton.onClick.AddListener(OnBackButtonClicked);
            }
        }

        private void PopulateCharacterList()
        {
            if (characterManager?.Database == null || characterListContainer == null)
            {
                return;
            }

            // Clear existing cards
            foreach (Transform child in characterListContainer)
            {
                Destroy(child.gameObject);
            }

            // Create cards for each character
            var characters = characterManager.Database.GetAllCharacters();
            var cardList = new System.Collections.Generic.List<CharacterCard>();

            foreach (var character in characters)
            {
                if (characterCardPrefab != null)
                {
                    var cardObj = Instantiate(characterCardPrefab, characterListContainer);
                    var card = cardObj.GetComponent<CharacterCard>();

                    if (card != null)
                    {
                        bool isUnlocked = characterManager.IsCharacterUnlocked(character.CharacterId);
                        card.Initialize(character, isUnlocked, OnCharacterCardClicked);
                        cardList.Add(card);
                    }
                }
            }

            characterCards = cardList.ToArray();
        }

        private void SelectInitialCharacter()
        {
            // Select currently equipped character
            if (characterManager?.CurrentCharacter != null)
            {
                SelectCharacterForPreview(characterManager.CurrentCharacter);
            }
        }

        #region Character Selection

        private void OnCharacterCardClicked(CharacterData character)
        {
            PlaySound(navigateSound);
            SelectCharacterForPreview(character);
        }

        private void SelectCharacterForPreview(CharacterData character)
        {
            if (character == null) return;

            selectedCharacter = character;
            UpdateCharacterDisplay(character);
            UpdateButtons(character);

            // Update preview
            if (previewUI != null)
            {
                previewUI.ShowCharacter(character);
            }

            // Highlight selected card
            if (characterCards != null)
            {
                foreach (var card in characterCards)
                {
                    card.SetSelected(card.Character == character);
                }
            }
        }

        private void UpdateCharacterDisplay(CharacterData character)
        {
            if (selectedPortrait != null && character.Portrait != null)
            {
                selectedPortrait.sprite = character.Portrait;
            }

            if (selectedName != null)
            {
                selectedName.text = character.DisplayName;
                selectedName.color = character.GetRarityColor();
            }

            if (selectedDescription != null)
            {
                selectedDescription.text = character.Description;
            }

            if (selectedStats != null)
            {
                selectedStats.text = character.GetStatsSummary();
            }

            if (selectedAbility != null)
            {
                if (character.PrimaryAbility != CharacterAbility.None)
                {
                    selectedAbility.text = $"<b>{GetAbilityName(character.PrimaryAbility)}</b>\n{character.AbilityDescription}";
                }
                else
                {
                    selectedAbility.text = "";
                }
            }

            if (rarityBorder != null)
            {
                rarityBorder.color = character.GetRarityColor();
            }
        }

        private void UpdateButtons(CharacterData character)
        {
            bool isUnlocked = characterManager.IsCharacterUnlocked(character.CharacterId);
            bool isSelected = characterManager.CurrentCharacter == character;

            // Select button
            if (selectButton != null)
            {
                selectButton.gameObject.SetActive(isUnlocked && !isSelected);
            }

            // Unlock button
            if (unlockButton != null)
            {
                unlockButton.gameObject.SetActive(!isUnlocked);

                if (!isUnlocked && unlockButtonText != null)
                {
                    unlockButtonText.text = GetUnlockButtonText(character);
                }
            }
        }

        private string GetUnlockButtonText(CharacterData character)
        {
            return character.UnlockRequirement switch
            {
                UnlockType.Coins => $"Unlock ({character.UnlockPrice} Coins)",
                UnlockType.Distance => $"Run {character.UnlockPrice}m",
                UnlockType.TotalCoins => $"Collect {character.UnlockPrice} Coins",
                UnlockType.GamesPlayed => $"Play {character.UnlockPrice} Games",
                UnlockType.HighScore => $"Score {character.UnlockPrice}",
                UnlockType.Special => "Special Event",
                _ => "Locked"
            };
        }

        private string GetAbilityName(CharacterAbility ability)
        {
            return ability switch
            {
                CharacterAbility.StartWithShield => "Shield Start",
                CharacterAbility.ExtendedPowerUps => "Power Extender",
                CharacterAbility.DoubleJump => "Double Jump",
                CharacterAbility.EnhancedMagnet => "Super Magnet",
                CharacterAbility.ScoreBoost => "Score Master",
                CharacterAbility.BeginnerFriendly => "Easy Mode",
                CharacterAbility.PowerSlide => "Power Slide",
                CharacterAbility.NinjaDodge => "Ninja Reflexes",
                _ => ""
            };
        }

        #endregion

        #region Button Handlers

        private void OnSelectButtonClicked()
        {
            if (selectedCharacter == null) return;

            if (characterManager.SelectCharacter(selectedCharacter))
            {
                PlaySound(selectSound);

                // Play character's select sound
                if (selectedCharacter.SelectSound != null && ServiceLocator.TryGet<AudioManager>(out var audio))
                {
                    audio.PlaySFX(selectedCharacter.SelectSound);
                }

                UpdateButtons(selectedCharacter);

                Debug.Log($"[CharacterSelectionUI] Selected: {selectedCharacter.DisplayName}");
            }
        }

        private void OnUnlockButtonClicked()
        {
            if (selectedCharacter == null) return;

            if (characterManager.TryUnlockCharacter(selectedCharacter))
            {
                PlaySound(unlockSound);
                Debug.Log($"[CharacterSelectionUI] Unlocked: {selectedCharacter.DisplayName}");
            }
            else
            {
                PlaySound(errorSound);

                // Show why unlock failed
                if (characterManager.CanUnlockCharacter(selectedCharacter, out string reason))
                {
                    Debug.Log($"[CharacterSelectionUI] Cannot unlock: {reason}");
                }
            }
        }

        private void OnBackButtonClicked()
        {
            // Return to main menu
            if (ServiceLocator.TryGet<GameManager>(out var gameManager))
            {
                gameManager.ReturnToMenu();
            }
        }

        #endregion

        #region Events

        private void OnCharacterUnlocked(CharacterData character)
        {
            // Refresh the unlocked character's card
            if (characterCards != null)
            {
                foreach (var card in characterCards)
                {
                    if (card.Character == character)
                    {
                        card.SetUnlocked(true);
                        break;
                    }
                }
            }

            // Update buttons if this is the selected character
            if (selectedCharacter == character)
            {
                UpdateButtons(character);
            }
        }

        #endregion

        private void PlaySound(AudioClip clip)
        {
            if (clip != null && ServiceLocator.TryGet<AudioManager>(out var audio))
            {
                audio.PlaySFX(clip);
            }
        }
    }
}
