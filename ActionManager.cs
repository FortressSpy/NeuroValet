using BepInEx.Logging;
using NeuroSdk.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuroValet
{
    /// <summary>
    /// This class is responsible for identifying possible actions in the game, and executing them.
    /// </summary>
    internal class ActionManager
    {
        internal struct PossibleActions
        {
            public List<INeuroAction> Actions;
            public string Context;
            public bool IsContextSilent;
        }

        private ManualLogSource logger;

        public ActionManager(ManualLogSource logger)
        {
            this.logger = logger;
        }

        // TODO - this should get state data as a parameter.
        public PossibleActions GetPossibleActions() 
        { 
            throw new NotImplementedException(); 
        }
        public void ExecuteAction(INeuroAction action)
        {
            throw new NotImplementedException();
        }
    }
}
