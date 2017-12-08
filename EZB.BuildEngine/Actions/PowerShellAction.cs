using System;

namespace EZB.BuildEngine.Actions
{
    class PowerShellAction : IAction
    {
        public PowerShellAction(string cmdLine)
        {
            _processAction = new ProcessAction("powershell.exe", cmdLine, true);
        }

        public void Execute()
        {
            _processAction.Execute();
        }

        private ProcessAction _processAction;
    }
}
