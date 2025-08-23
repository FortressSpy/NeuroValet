using BepInEx.Logging;

using static NeuroValet.ActionManager;

namespace NeuroValet.ViewsParsers
{
    /// <summary>
    /// Game is UI-based, and uses 'Views' to handle all the different screens and interactions.
    /// Each view is responsible for a specific part of the game, such as city-actions, market, dialogue and more
    /// Each IViewParser is responsible for parsing the data from a specific view, identifying possible actions, and executing them
    /// </summary>
    internal interface IViewParser
    {
        PossibleActions GetPossibleActions(ManualLogSource logger);
        bool IsViewRelevant();
    }
}
