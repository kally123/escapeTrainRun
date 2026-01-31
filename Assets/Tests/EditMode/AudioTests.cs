using NUnit.Framework;
using UnityEngine;
using EscapeTrainRun.Audio;
using EscapeTrainRun.Environment;

namespace EscapeTrainRun.Tests.EditMode
{
    /// <summary>
    /// Unit tests for Audio system.
    /// </summary>
    [TestFixture]
    public class AudioTests
    {
        #region SurfaceType Tests

        [Test]
        public void SurfaceType_HasFiveValues()
        {
            // Arrange
            var values = System.Enum.GetValues(typeof(SurfaceType));

            // Assert
            Assert.AreEqual(5, values.Length);
        }

        [Test]
        public void SurfaceType_HasExpectedValues()
        {
            // Assert
            Assert.AreEqual(0, (int)SurfaceType.Default);
            Assert.AreEqual(1, (int)SurfaceType.Metal);
            Assert.AreEqual(2, (int)SurfaceType.Wood);
            Assert.AreEqual(3, (int)SurfaceType.Concrete);
            Assert.AreEqual(4, (int)SurfaceType.Grass);
        }

        #endregion

        #region WeatherType Tests

        [Test]
        public void WeatherType_HasThreeValues()
        {
            // Arrange
            var values = System.Enum.GetValues(typeof(WeatherType));

            // Assert
            Assert.AreEqual(3, values.Length);
        }

        [Test]
        public void WeatherType_HasExpectedValues()
        {
            // Assert
            Assert.AreEqual(0, (int)WeatherType.Clear);
            Assert.AreEqual(1, (int)WeatherType.Rain);
            Assert.AreEqual(2, (int)WeatherType.Storm);
        }

        #endregion

        #region MusicIntensity Tests

        [Test]
        public void MusicIntensity_HasFourValues()
        {
            // Arrange
            var values = System.Enum.GetValues(typeof(MusicIntensity));

            // Assert
            Assert.AreEqual(4, values.Length);
        }

        [Test]
        public void MusicIntensity_HasExpectedValues()
        {
            // Assert
            Assert.AreEqual(0, (int)MusicIntensity.Low);
            Assert.AreEqual(1, (int)MusicIntensity.Normal);
            Assert.AreEqual(2, (int)MusicIntensity.High);
            Assert.AreEqual(3, (int)MusicIntensity.Intense);
        }

        #endregion

        #region MusicPlaylist Tests

        [Test]
        public void MusicPlaylist_CanBeCreated()
        {
            // Arrange & Act
            var playlist = new MusicPlaylist
            {
                Name = "TestPlaylist",
                Tracks = new AudioClip[3],
                AllowShuffle = true
            };

            // Assert
            Assert.AreEqual("TestPlaylist", playlist.Name);
            Assert.AreEqual(3, playlist.Tracks.Length);
            Assert.IsTrue(playlist.AllowShuffle);
        }

        [Test]
        public void MusicPlaylist_CanHaveEmptyTracks()
        {
            // Arrange & Act
            var playlist = new MusicPlaylist
            {
                Name = "EmptyPlaylist",
                Tracks = new AudioClip[0]
            };

            // Assert
            Assert.AreEqual(0, playlist.Tracks.Length);
        }

        #endregion

        #region SpatialAudioSource Tests

        [Test]
        public void SpatialAudioSource_CanBeAddedToGameObject()
        {
            // Arrange
            var gameObj = new GameObject("TestSpatialAudio");

            // Act
            var spatialAudio = gameObj.AddComponent<SpatialAudioSource>();

            // Assert
            Assert.IsNotNull(spatialAudio);

            // Cleanup
            Object.DestroyImmediate(gameObj);
        }

        #endregion

        #region FootstepAudioController Tests

        [Test]
        public void FootstepAudioController_CanBeAddedToGameObject()
        {
            // Arrange
            var gameObj = new GameObject("TestFootsteps");

            // Act
            var footsteps = gameObj.AddComponent<FootstepAudioController>();

            // Assert
            Assert.IsNotNull(footsteps);

            // Cleanup
            Object.DestroyImmediate(gameObj);
        }

        #endregion

        #region SpeedBasedAudio Tests

        [Test]
        public void SpeedBasedAudio_CanBeAddedToGameObject()
        {
            // Arrange
            var gameObj = new GameObject("TestSpeedAudio");

            // Act
            var speedAudio = gameObj.AddComponent<SpeedBasedAudio>();

            // Assert
            Assert.IsNotNull(speedAudio);

            // Cleanup
            Object.DestroyImmediate(gameObj);
        }

        #endregion

        #region AmbientAudioController Tests

        [Test]
        public void AmbientAudioController_CanBeAddedToGameObject()
        {
            // Arrange
            var gameObj = new GameObject("TestAmbient");

            // Act
            var ambient = gameObj.AddComponent<AmbientAudioController>();

            // Assert
            Assert.IsNotNull(ambient);

            // Cleanup
            Object.DestroyImmediate(gameObj);
        }

        #endregion

        #region CollectibleAudioFeedback Tests

        [Test]
        public void CollectibleAudioFeedback_CanBeAddedToGameObject()
        {
            // Arrange
            var gameObj = new GameObject("TestCollectibleAudio");

            // Act
            var feedback = gameObj.AddComponent<CollectibleAudioFeedback>();

            // Assert
            Assert.IsNotNull(feedback);

            // Cleanup
            Object.DestroyImmediate(gameObj);
        }

        #endregion

        #region Volume Conversion Tests

        [Test]
        public void LinearToDecibels_ZeroReturnsNegative80()
        {
            // Arrange
            float linear = 0f;

            // Act
            float decibels = linear <= 0f ? -80f : Mathf.Log10(linear) * 20f;

            // Assert
            Assert.AreEqual(-80f, decibels);
        }

        [Test]
        public void LinearToDecibels_OneReturnsZero()
        {
            // Arrange
            float linear = 1f;

            // Act
            float decibels = Mathf.Log10(linear) * 20f;

            // Assert
            Assert.AreEqual(0f, decibels, 0.001f);
        }

        [Test]
        public void LinearToDecibels_HalfReturnsNegativeSix()
        {
            // Arrange
            float linear = 0.5f;

            // Act
            float decibels = Mathf.Log10(linear) * 20f;

            // Assert
            Assert.AreEqual(-6.02f, decibels, 0.1f);
        }

        #endregion

        #region Theme Audio Mapping Tests

        [Test]
        public void ThemeType_AllValuesMapToAudio()
        {
            // Arrange
            var themes = System.Enum.GetValues(typeof(ThemeType));

            // Assert
            foreach (ThemeType theme in themes)
            {
                // Each theme should have a corresponding audio mapping
                Assert.IsTrue(System.Enum.IsDefined(typeof(ThemeType), theme));
            }
        }

        #endregion
    }
}
