using BepInEx.Logging;
using NeuroSdk.Actions;
using NeuroValet.StateData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuroValet.ViewsParsers
{
    /// <summary>
    /// Game is UI-based, and uses 'Views' to handle all the different screens and interactions.
    /// Each view is responsible for a specific part of the game, such as city-actions, market, dialogue and more
    /// Each IViewParser is responsible for parsing the data from a specific view, identifying possible actions, and executing them
    /// </summary>
    internal interface IViewParser
    {
        List<INeuroAction> GetPossibleActions(GameStateData stateData, ManualLogSource logger);
    }
}
