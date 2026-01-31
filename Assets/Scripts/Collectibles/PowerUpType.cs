namespace EscapeTrainRun.Collectibles
{
    /// <summary>
    /// Defines the available power-up types in the game.
    /// Each power-up provides a unique temporary ability.
    /// </summary>
    public enum PowerUpType
    {
        /// <summary>
        /// Magnet - Attracts nearby coins to the player.
        /// Duration: 10 seconds.
        /// Visual: Blue sparkle trail.
        /// </summary>
        Magnet = 0,

        /// <summary>
        /// Shield - Protects from one collision with an obstacle.
        /// Duration: Until hit.
        /// Visual: Bubble around player.
        /// </summary>
        Shield = 1,

        /// <summary>
        /// Speed Boost - Doubles speed and makes player invincible.
        /// Duration: 5 seconds.
        /// Visual: Speed lines, flying animation.
        /// </summary>
        SpeedBoost = 2,

        /// <summary>
        /// Star Power - Player flies above all obstacles.
        /// Duration: 8 seconds.
        /// Visual: Golden wings, star particles.
        /// </summary>
        StarPower = 3,

        /// <summary>
        /// Multiplier - Doubles all score earned.
        /// Duration: 15 seconds.
        /// Visual: Score text glows gold.
        /// </summary>
        Multiplier = 4,

        /// <summary>
        /// Mystery Box - Gives a random reward instantly.
        /// Duration: Instant.
        /// Visual: Confetti explosion.
        /// </summary>
        MysteryBox = 5
    }
}
