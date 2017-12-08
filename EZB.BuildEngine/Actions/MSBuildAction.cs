using System;

namespace EZB.BuildEngine.Actions
{
    class MSBuildAction : IAction
    {
        public MSBuildAction(string path, bool clean, Environment environment)
        {
            if (environment == null || string.IsNullOrEmpty(environment.MSBuildPath))
                throw new ApplicationException("Failed to locate the MSBuild executable");

            string msBuildVerb = clean ? "clean" : "build";
            _processAction = new ProcessAction(environment.MSBuildPath, "\"" + path + "\" /m /t:" + msBuildVerb, true);
        }

        public void Execute()
        {
            _processAction.Execute();
        }

        private ProcessAction _processAction;
    }
}
