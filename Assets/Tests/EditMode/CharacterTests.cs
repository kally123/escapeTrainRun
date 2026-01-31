using NUnit.Framework;
using UnityEngine;
using EscapeTrainRun.Characters;

namespace EscapeTrainRun.Tests.EditMode
{
    /// <summary>
    /// Unit tests for Character system.
    /// </summary>
    [TestFixture]
    public class CharacterTests
    {
        #region CharacterRarity Tests

        [Test]
        public void CharacterRarity_HasFiveValues()
        {
            // Arrange
            var values = System.Enum.GetValues(typeof(CharacterRarity));

            // Assert
            Assert.AreEqual(5, values.Length);
        }

        [Test]
        public void CharacterRarity_HasExpectedOrder()
        {
            // Assert
            Assert.AreEqual(0, (int)CharacterRarity.Common);
            Assert.AreEqual(1, (int)CharacterRarity.Uncommon);
            Assert.AreEqual(2, (int)CharacterRarity.Rare);
            Assert.AreEqual(3, (int)CharacterRarity.Epic);
            Assert.AreEqual(4, (int)CharacterRarity.Legendary);
        }

        #endregion

        #region UnlockType Tests

        [Test]
        public void UnlockType_HasExpectedValues()
        {
            // Assert
            Assert.AreEqual(0, (int)UnlockType.Coins);
            Assert.AreEqual(1, (int)UnlockType.Distance);
            Assert.AreEqual(2, (int)UnlockType.TotalCoins);
            Assert.AreEqual(3, (int)UnlockType.GamesPlayed);
            Assert.AreEqual(4, (int)UnlockType.HighScore);
            Assert.AreEqual(5, (int)UnlockType.Special);
        }

        #endregion

        #region CharacterAbility Tests

        [Test]
        public void CharacterAbility_HasNineValues()
        {
            // Arrange
            var values = System.Enum.GetValues(typeof(CharacterAbility));

            // Assert
            Assert.AreEqual(9, values.Length);
        }

        [Test]
        public void CharacterAbility_NoneIsZero()
        {
            // Assert
            Assert.AreEqual(0, (int)CharacterAbility.None);
        }

        [Test]
        public void CharacterAbility_AllAbilitiesAreDefined()
        {
            // Arrange
            var abilities = new CharacterAbility[]
            {
                CharacterAbility.None,
                CharacterAbility.StartWithShield,
                CharacterAbility.ExtendedPowerUps,
                CharacterAbility.DoubleJump,
                CharacterAbility.EnhancedMagnet,
                CharacterAbility.ScoreBoost,
                CharacterAbility.BeginnerFriendly,
                CharacterAbility.PowerSlide,
                CharacterAbility.NinjaDodge
            };

            // Assert
            Assert.AreEqual(9, abilities.Length);
        }

        #endregion

        #region DefaultCharacters Tests

        [Test]
        public void DefaultCharacters_HasEightCharacters()
        {
            // Assert
            Assert.AreEqual(8, DefaultCharacters.Characters.Length);
        }

        [Test]
        public void DefaultCharacters_TimmyIsFirst()
        {
            // Arrange
            var timmy = DefaultCharacters.Characters[0];

            // Assert
            Assert.AreEqual("timmy", timmy.Id);
            Assert.AreEqual("Timmy", timmy.Name);
            Assert.IsTrue(timmy.IsUnlockedByDefault);
        }

        [Test]
        public void DefaultCharacters_AllHaveUniqueIds()
        {
            // Arrange
            var ids = new System.Collections.Generic.HashSet<string>();

            // Act & Assert
            foreach (var character in DefaultCharacters.Characters)
            {
                Assert.IsFalse(ids.Contains(character.Id), $"Duplicate ID: {character.Id}");
                ids.Add(character.Id);
            }
        }

        [Test]
        public void DefaultCharacters_AllHaveValidAbilities()
        {
            // Assert
            foreach (var character in DefaultCharacters.Characters)
            {
                Assert.IsTrue(System.Enum.IsDefined(typeof(CharacterAbility), character.Ability),
                    $"Character {character.Id} has invalid ability");
            }
        }

        [Test]
        public void DefaultCharacters_AllHavePositiveModifiers()
        {
            // Assert
            foreach (var character in DefaultCharacters.Characters)
            {
                Assert.Greater(character.SpeedModifier, 0f,
                    $"Character {character.Id} has invalid speed modifier");
                Assert.Greater(character.CoinMultiplier, 0f,
                    $"Character {character.Id} has invalid coin multiplier");
                Assert.Greater(character.ScoreMultiplier, 0f,
                    $"Character {character.Id} has invalid score multiplier");
            }
        }

        [Test]
        public void DefaultCharacters_OnlyTimmyIsUnlockedByDefault()
        {
            // Arrange
            int unlockedCount = 0;

            // Act
            foreach (var character in DefaultCharacters.Characters)
            {
                if (character.IsUnlockedByDefault)
                {
                    unlockedCount++;
                    Assert.AreEqual("timmy", character.Id);
                }
            }

            // Assert
            Assert.AreEqual(1, unlockedCount);
        }

        [Test]
        public void DefaultCharacters_PricesIncreaseWithRarity()
        {
            // Arrange - find characters of each rarity
            int commonPrice = 0;
            int uncommonPrice = 0;
            int rarePrice = 0;
            int epicPrice = 0;
            int legendaryPrice = 0;

            foreach (var character in DefaultCharacters.Characters)
            {
                switch (character.Rarity)
                {
                    case CharacterRarity.Common:
                        commonPrice = Mathf.Max(commonPrice, character.UnlockPrice);
                        break;
                    case CharacterRarity.Uncommon:
                        if (uncommonPrice == 0) uncommonPrice = character.UnlockPrice;
                        break;
                    case CharacterRarity.Rare:
                        if (rarePrice == 0) rarePrice = character.UnlockPrice;
                        break;
                    case CharacterRarity.Epic:
                        if (epicPrice == 0) epicPrice = character.UnlockPrice;
                        break;
                    case CharacterRarity.Legendary:
                        legendaryPrice = character.UnlockPrice;
                        break;
                }
            }

            // Assert - higher rarity should cost more
            Assert.LessOrEqual(commonPrice, uncommonPrice);
            Assert.Less(uncommonPrice, rarePrice);
            Assert.Less(rarePrice, epicPrice);
            Assert.Less(epicPrice, legendaryPrice);
        }

        #endregion

        #region RarityColor Tests

        [Test]
        public void GetRarityColor_ReturnsNonBlackColors()
        {
            // Arrange
            var rarities = System.Enum.GetValues(typeof(CharacterRarity));

            // Assert
            foreach (CharacterRarity rarity in rarities)
            {
                Color color = DefaultCharacters.GetRarityColor(rarity);
                Assert.AreNotEqual(Color.black, color, $"Rarity {rarity} returned black color");
            }
        }

        [Test]
        public void GetRarityName_ReturnsNonEmptyStrings()
        {
            // Arrange
            var rarities = System.Enum.GetValues(typeof(CharacterRarity));

            // Assert
            foreach (CharacterRarity rarity in rarities)
            {
                string name = DefaultCharacters.GetRarityName(rarity);
                Assert.IsNotEmpty(name, $"Rarity {rarity} returned empty name");
            }
        }

        [Test]
        public void GetRarityColor_LegendaryIsGold()
        {
            // Act
            Color color = DefaultCharacters.GetRarityColor(CharacterRarity.Legendary);

            // Assert - Gold should have high R, high G, low B
            Assert.Greater(color.r, 0.7f);
            Assert.Greater(color.g, 0.6f);
            Assert.Less(color.b, 0.5f);
        }

        #endregion

        #region CharacterDefinition Tests

        [Test]
        public void CharacterDefinition_TimmyHasBeginnerFriendlyAbility()
        {
            // Arrange
            var timmy = DefaultCharacters.Characters[0];

            // Assert
            Assert.AreEqual(CharacterAbility.BeginnerFriendly, timmy.Ability);
        }

        [Test]
        public void CharacterDefinition_NinjaIsLegendary()
        {
            // Arrange
            CharacterDefinition? ninja = null;
            foreach (var c in DefaultCharacters.Characters)
            {
                if (c.Id == "ninja_nick")
                {
                    ninja = c;
                    break;
                }
            }

            // Assert
            Assert.IsNotNull(ninja);
            Assert.AreEqual(CharacterRarity.Legendary, ninja.Value.Rarity);
        }

        [Test]
        public void CharacterDefinition_AllHaveDescriptions()
        {
            // Assert
            foreach (var character in DefaultCharacters.Characters)
            {
                Assert.IsNotEmpty(character.Description, 
                    $"Character {character.Id} has no description");
                Assert.IsNotEmpty(character.AbilityDescription, 
                    $"Character {character.Id} has no ability description");
            }
        }

        #endregion
    }
}
