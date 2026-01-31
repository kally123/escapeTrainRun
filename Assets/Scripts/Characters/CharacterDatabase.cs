using System.Collections.Generic;
using UnityEngine;

namespace EscapeTrainRun.Characters
{
    /// <summary>
    /// ScriptableObject database containing all playable characters.
    /// </summary>
    [CreateAssetMenu(fileName = "CharacterDatabase", menuName = "Escape Train Run/Character Database")]
    public class CharacterDatabase : ScriptableObject
    {
        [Header("Characters")]
        [SerializeField] private List<CharacterData> characters = new List<CharacterData>();

        /// <summary>
        /// Gets the total number of characters.
        /// </summary>
        public int CharacterCount => characters.Count;

        /// <summary>
        /// Gets a character by ID.
        /// </summary>
        public CharacterData GetCharacter(string characterId)
        {
            if (string.IsNullOrEmpty(characterId)) return null;

            foreach (var character in characters)
            {
                if (character != null && character.CharacterId == characterId)
                {
                    return character;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets a character by index.
        /// </summary>
        public CharacterData GetCharacter(int index)
        {
            if (index < 0 || index >= characters.Count) return null;
            return characters[index];
        }

        /// <summary>
        /// Gets all characters in the database.
        /// </summary>
        public IEnumerable<CharacterData> GetAllCharacters()
        {
            return characters;
        }

        /// <summary>
        /// Gets characters filtered by rarity.
        /// </summary>
        public List<CharacterData> GetCharactersByRarity(CharacterRarity rarity)
        {
            var result = new List<CharacterData>();

            foreach (var character in characters)
            {
                if (character != null && character.Rarity == rarity)
                {
                    result.Add(character);
                }
            }

            return result;
        }

        /// <summary>
        /// Gets characters that have a specific ability.
        /// </summary>
        public List<CharacterData> GetCharactersByAbility(CharacterAbility ability)
        {
            var result = new List<CharacterData>();

            foreach (var character in characters)
            {
                if (character != null && character.PrimaryAbility == ability)
                {
                    result.Add(character);
                }
            }

            return result;
        }

        /// <summary>
        /// Checks if a character ID exists in the database.
        /// </summary>
        public bool HasCharacter(string characterId)
        {
            return GetCharacter(characterId) != null;
        }

        /// <summary>
        /// Gets the index of a character.
        /// </summary>
        public int GetCharacterIndex(string characterId)
        {
            for (int i = 0; i < characters.Count; i++)
            {
                if (characters[i] != null && characters[i].CharacterId == characterId)
                {
                    return i;
                }
            }
            return -1;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Editor-only: Adds a character to the database.
        /// </summary>
        public void AddCharacter(CharacterData character)
        {
            if (character != null && !characters.Contains(character))
            {
                characters.Add(character);
            }
        }

        /// <summary>
        /// Editor-only: Removes a character from the database.
        /// </summary>
        public void RemoveCharacter(CharacterData character)
        {
            characters.Remove(character);
        }

        private void OnValidate()
        {
            // Remove null entries
            characters.RemoveAll(c => c == null);

            // Check for duplicate IDs
            var ids = new HashSet<string>();
            foreach (var character in characters)
            {
                if (character != null)
                {
                    if (ids.Contains(character.CharacterId))
                    {
                        Debug.LogWarning($"[CharacterDatabase] Duplicate character ID: {character.CharacterId}");
                    }
                    ids.Add(character.CharacterId);
                }
            }
        }
#endif
    }
}
