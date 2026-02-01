namespace EscapeTrainRun.Environment
{
    /// <summary>
    /// Defines the available game environment themes.
    /// Each theme has unique visuals, obstacles, and collectibles.
    /// </summary>
    public enum ThemeType
    {
        /// <summary>
        /// Train environment - running through train cars.
        /// Obstacles: Luggage, carts, gift boxes.
        /// Collectibles: Golden tickets, lunch boxes, travel stamps.
        /// </summary>
        Train = 0,

        /// <summary>
        /// Bus environment - hopping between buses.
        /// Obstacles: Backpacks, hanging straps, sports gear.
        /// Collectibles: Bus tokens, mystery backpacks, gold stars.
        /// </summary>
        Bus = 1,

        /// <summary>
        /// Ground/Playground environment - running through parks.
        /// Obstacles: Benches, branches, sprinklers, friendly dogs.
        /// Collectibles: Golden stars, treasure chests, golden leaves.
        /// </summary>
        Ground = 2,

        /// <summary>
        /// Park environment - alternative outdoor theme.
        /// Alias for Ground theme for compatibility.
        /// </summary>
        Park = 2
    }
}
