using NeuroSdk;
using NeuroSdk.Actions;
using NeuroSdk.Json;
using NeuroSdk.Websocket;
using NeuroValet.ViewsParsers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuroValet.Actions
{
    internal class StoryAction : NeuroActionS<int>
    {
        private readonly string _choiceText;
        private readonly int _choiceIndex;

        public StoryAction(string choiceText, int choiceIndex)
        {
            _choiceText = choiceText;
            _choiceIndex = choiceIndex;
        }

        public override string Name => "story_decision_" + _choiceIndex;
        protected override string Description => _choiceText;

        protected override JsonSchema Schema => new()
        {
            Type = JsonSchemaType.Object,
            Required = new List<string> { "index" },
            Properties = new Dictionary<string, JsonSchema>
            {
                ["index"] = QJS.Type(JsonSchemaType.Integer)
            }
        };

        protected override void Execute(int? choiceIndex)
        {
            // TODO - need to implement pressing a key in the MouseSimulator. 
            throw new NotImplementedException();
        }

        // TODO do I even need this choiceIndex param? if the action window saves the action object I might already have it in the _choiceIndex field.
        protected override ExecutionResult Validate(ActionJData actionData, out int? choiceIndex)
        {
            choiceIndex = actionData.Data?["index"]?.Value<int>();

            if (!choiceIndex.HasValue)
            {
                return ExecutionResult.Failure(NeuroSdkStrings.ActionFailedMissingRequiredParameter.Format("index"));
            }
            else if (StoryViewParser.Instance.HasAction(choiceIndex))
            {
                return ExecutionResult.Success();
            }
            else
            {
                return ExecutionResult.Failure(NeuroSdkStrings.ActionFailedInvalidParameter.Format("index"));
            }
        }
    }
}
