using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AssetInventory
{
    [Serializable]
    public sealed class RunActionStep : ActionStep
    {
        public RunActionStep()
        {
            List<CustomAction> actions = DBAdapter.DB.Query<CustomAction>("select * from CustomAction order by Name");
            List<Tuple<string, ParameterValue>> options = new List<Tuple<string, ParameterValue>>();
            foreach (CustomAction action in actions)
            {
                options.Add(new Tuple<string, ParameterValue>(action.Name, new ParameterValue(action.Id)));
            }

            Key = "RunAction";
            Name = "Run Action";
            Description = "Run another custom action.";
            Category = ActionCategory.Misc;
            Parameters.Add(new StepParameter
            {
                Name = "Action",
                Description = "Action to run.",
                Type = StepParameter.ParamType.Int,
                ValueList = StepParameter.ValueType.Custom,
                Options = options
            });
        }

        public override async Task Run(List<ParameterValue> parameters)
        {
            CustomAction action = DBAdapter.DB.Find<CustomAction>(parameters[0].intValue);
            await AI.Actions.RunUserAction(action);
        }
    }
}