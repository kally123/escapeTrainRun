using System.Collections.Generic;
using UnityEngine;
using EscapeTrainRun.Core;
using EscapeTrainRun.Events;

namespace EscapeTrainRun.Characters
{
    /// <summary>
    /// Manages character selection, unlocking, and persistence.
    /// Central hub for all character-related operations.
    /// </summary>
    public class CharacterManager : MonoBehaviour
    {
        public static CharacterManager Instance { get; private set; }

        [Header("Character Database")]
        [SerializeField] private CharacterDatabase characterDatabase;

        [Header("Default Character")]
        [SerializeField] private string defaultCharacterId = "timmy";

        // State
        private CharacterData currentCharacter;
        private HashSet<string> unlockedCharacters = new HashSet<string>();

        // Events
        public event System.Action<CharacterData> OnCharacterChanged;
        public event System.Action<CharacterData> OnCharacterUnlocked;

        // Properties
        public CharacterData CurrentCharacter => currentCharacter;
        public CharacterDatabase Database => characterDatabase;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            ServiceLocator.Register(this);
            LoadUnlockedCharacters();
        }

        private void Start()
        {
            InitializeDefaultCharacter();
        }

        #region Initialization

        private void InitializeDefaultCharacter()
        {
            // Load saved character selection
            string savedCharacterId = PlayerPrefs.GetString("SelectedCharacter", defaultCharacterId);

            // Try to select saved character
            var character = characterDatabase?.GetCharacter(savedCharacterId);

            if (character != null && IsCharacterUnlocked(savedCharacterId))
            {
                SelectCharacter(character);
            }
            else
            {
                // Fall back to default
                character = characterDatabase?.GetCharacter(defaultCharacterId);
                if (character != null)
                {
                    SelectCharacter(character);
                }
            }
        }

        private void LoadUnlockedCharacters()
        {
            unlockedCharacters.Clear();

            if (characterDatabase == null) return;

            // Add default unlocked characters
            foreach (var character in characterDatabase.GetAllCharacters())
            {
                if (character.IsUnlockedByDefault)
                {
                    unlockedCharacters.Add(character.CharacterId);
                }
            }

            // Load saved unlocks
            string savedUnlocks = PlayerPrefs.GetString("UnlockedCharacters", "");
            if (!string.IsNullOrEmpty(savedUnlocks))
            {
                string[] ids = savedUnlocks.Split(',');
                foreach (string id in ids)
                {
                    if (!string.IsNullOrEmpty(id))
                    {
                        unlockedCharacters.Add(id);
                    }
                }
            }

            Debug.Log($"[CharacterManager] Loaded {unlockedCharacters.Count} unlocked characters");
        }

        private void SaveUnlockedCharacters()
        {
            string saveData = string.Join(",", unlockedCharacters);
            PlayerPrefs.SetString("UnlockedCharacters", saveData);
            PlayerPrefs.Save();
        }

        #endregion

        #region Character Selection

        /// <summary>
        /// Selects a character for gameplay.
        /// </summary>
        public bool SelectCharacter(CharacterData character)
        {
            if (character == null)
            {
                Debug.LogWarning("[CharacterManager] Cannot select null character");
                return false;
            }

            if (!IsCharacterUnlocked(character.CharacterId))
            {
                Debug.LogWarning($"[CharacterManager] Character {character.CharacterId} is not unlocked");
                return false;
            }

            currentCharacter = character;

            // Save selection
            PlayerPrefs.SetString("SelectedCharacter", character.CharacterId);
            PlayerPrefs.Save();

            // Notify listeners
            OnCharacterChanged?.Invoke(character);
            GameEvents.TriggerCharacterSelected(character);

            Debug.Log($"[CharacterManager] Selected character: {character.DisplayName}");
            return true;
        }

        /// <summary>
        /// Selects a character by ID.
        /// </summary>
        public bool SelectCharacter(string characterId)
        {
            var character = characterDatabase?.GetCharacter(characterId);
            return character != null && SelectCharacter(character);
        }

        #endregion

        #region Character Unlocking

        /// <summary>
        /// Checks if a character is unlocked.
        /// </summary>
        public bool IsCharacterUnlocked(string characterId)
        {
            return unlockedCharacters.Contains(characterId);
        }

        /// <summary>
        /// Attempts to unlock a character with coins.
        /// </summary>
        public bool TryUnlockCharacter(CharacterData character)
        {
            if (character == null) return false;

            if (IsCharacterUnlocked(character.CharacterId))
            {
                Debug.Log($"[CharacterManager] Character {character.CharacterId} already unlocked");
                return true;
            }

            // Check unlock requirements
            if (!CanUnlockCharacter(character, out string reason))
            {
                Debug.Log($"[CharacterManager] Cannot unlock {character.CharacterId}: {reason}");
                return false;
            }

            // Spend coins if required
            if (character.UnlockRequirement == UnlockType.Coins)
            {
                var saveManager = ServiceLocator.Get<SaveManager>();
                if (saveManager == null || !saveManager.SpendCoins(character.UnlockPrice))
                {
                    Debug.Log($"[CharacterManager] Not enough coins to unlock {character.CharacterId}");
                    return false;
                }
            }

            // Unlock the character
            UnlockCharacter(character);
            return true;
        }

        /// <summary>
        /// Checks if a character can be unlocked.
        /// </summary>
        public bool CanUnlockCharacter(CharacterData character, out string reason)
        {
            reason = "";

            if (character == null)
            {
                reason = "Invalid character";
                return false;
            }

            if (IsCharacterUnlocked(character.CharacterId))
            {
                reason = "Already unlocked";
                return true;
            }

            var saveManager = ServiceLocator.Get<SaveManager>();
            if (saveManager == null)
            {
                reason = "Save system unavailable";
                return false;
            }

            switch (character.UnlockRequirement)
            {
                case UnlockType.Coins:
                    if (saveManager.TotalCoins < character.UnlockPrice)
                    {
                        reason = $"Need {character.UnlockPrice - saveManager.TotalCoins} more coins";
                        return false;
                    }
                    return true;

                case UnlockType.Distance:
                    if (saveManager.TotalDistanceRun < character.UnlockPrice)
                    {
                        reason = $"Run {character.UnlockPrice - saveManager.TotalDistanceRun}m more";
                        return false;
                    }
                    return true;

                case UnlockType.TotalCoins:
                    if (saveManager.TotalCoinsCollected < character.UnlockPrice)
                    {
                        reason = $"Collect {character.UnlockPrice - saveManager.TotalCoinsCollected} more coins total";
                        return false;
                    }
                    return true;

                case UnlockType.GamesPlayed:
                    if (saveManager.GamesPlayed < character.UnlockPrice)
                    {
                        reason = $"Play {character.UnlockPrice - saveManager.GamesPlayed} more games";
                        return false;
                    }
                    return true;

                case UnlockType.HighScore:
                    if (saveManager.GetHighScore() < character.UnlockPrice)
                    {
                        reason = $"Reach score of {character.UnlockPrice}";
                        return false;
                    }
                    return true;

                case UnlockType.Special:
                    reason = "Special unlock required";
                    return false;

                default:
                    reason = "Unknown unlock type";
                    return false;
            }
        }

        /// <summary>
        /// Forces a character to be unlocked (for rewards, events, etc.)
        /// </summary>
        public void UnlockCharacter(CharacterData character)
        {
            if (character == null) return;

            unlockedCharacters.Add(character.CharacterId);
            SaveUnlockedCharacters();

            OnCharacterUnlocked?.Invoke(character);
            GameEvents.TriggerCharacterUnlocked(character);

            Debug.Log($"[CharacterManager] Unlocked character: {character.DisplayName}");
        }

        #endregion

        #region Queries

        /// <summary>
        /// Gets all unlocked characters.
        /// </summary>
        public List<CharacterData> GetUnlockedCharacters()
        {
            var unlocked = new List<CharacterData>();

            if (characterDatabase == null) return unlocked;

            foreach (var character in characterDatabase.GetAllCharacters())
            {
                if (IsCharacterUnlocked(character.CharacterId))
                {
                    unlocked.Add(character);
                }
            }

            return unlocked;
        }

        /// <summary>
        /// Gets all locked characters.
        /// </summary>
        public List<CharacterData> GetLockedCharacters()
        {
            var locked = new List<CharacterData>();

            if (characterDatabase == null) return locked;

            foreach (var character in characterDatabase.GetAllCharacters())
            {
                if (!IsCharacterUnlocked(character.CharacterId))
                {
                    locked.Add(character);
                }
            }

            return locked;
        }

        /// <summary>
        /// Gets the total number of characters.
        /// </summary>
        public int GetTotalCharacterCount()
        {
            return characterDatabase?.CharacterCount ?? 0;
        }

        /// <summary>
        /// Gets the number of unlocked characters.
        /// </summary>
        public int GetUnlockedCharacterCount()
        {
            return unlockedCharacters.Count;
        }

        /// <summary>
        /// Gets unlock progress as a percentage.
        /// </summary>
        public float GetUnlockProgress()
        {
            int total = GetTotalCharacterCount();
            if (total == 0) return 0f;

            return (float)unlockedCharacters.Count / total;
        }

        #endregion

        #region Character Stats Application

        /// <summary>
        /// Gets the current character's speed modifier.
        /// </summary>
        public float GetSpeedModifier()
        {
            return currentCharacter?.SpeedModifier ?? 1f;
        }

        /// <summary>
        /// Gets the current character's coin multiplier.
        /// </summary>
        public float GetCoinMultiplier()
        {
            return currentCharacter?.CoinMultiplier ?? 1f;
        }

        /// <summary>
        /// Gets the current character's score multiplier.
        /// </summary>
        public float GetScoreMultiplier()
        {
            return currentCharacter?.ScoreMultiplier ?? 1f;
        }

        /// <summary>
        /// Gets power-up duration bonus from current character.
        /// </summary>
        public float GetPowerUpDurationBonus(Collectibles.PowerUpType type)
        {
            return currentCharacter?.GetPowerUpDurationBonus(type) ?? 0f;
        }

        /// <summary>
        /// Checks if current character has a specific ability.
        /// </summary>
        public bool HasAbility(CharacterAbility ability)
        {
            return currentCharacter?.PrimaryAbility == ability;
        }

        /// <summary>
        /// Gets the current character's ability strength.
        /// </summary>
        public float GetAbilityStrength()
        {
            return currentCharacter?.AbilityStrength ?? 1f;
        }

        #endregion

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
            ServiceLocator.Unregister<CharacterManager>();
        }
    }
}
