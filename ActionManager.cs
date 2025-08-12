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
        // TODO - this should get state data as a parameter.
        List<INeuroAction> GetPossibleActions() 
        { 
            throw new NotImplementedException(); 
        }
        void ExecuteAction(INeuroAction action)
        {
            throw new NotImplementedException();
        }
    }
}
