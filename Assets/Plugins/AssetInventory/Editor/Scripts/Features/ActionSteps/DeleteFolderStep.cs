using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;

namespace AssetInventory
{
    [Serializable]
    public sealed class DeleteFolderStep : ActionStep
    {
        public DeleteFolderStep()
        {
            Key = "DeleteFolder";
            Name = "Delete Folder";
            Description = "Delete the folder under the specified path.";
            Category = ActionCategory.FilesAndFolders;
            Parameters.Add(new StepParameter
            {
                Name = "Path",
                Description = "Path of a folder relative to the current project root.",
                DefaultValue = new ParameterValue("Assets/Temp"),
                ValueList = StepParameter.ValueType.Folder
            });
        }

        public override async Task Run(List<ParameterValue> parameters)
        {
            AssetDatabase.DeleteAsset(parameters[0].stringValue);
            await Task.Yield();
        }
    }
}
