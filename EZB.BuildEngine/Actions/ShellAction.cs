using System;

namespace EZB.BuildEngine.Actions
{
    class ShellAction : IAction
    {
        public ShellAction(string cmdLine)
        {
            _processAction = new ProcessAction("cmd.exe", "/c " + cmdLine, true);
        }

        public void Execute()
        {
            _processAction.Execute();
        }

        private ProcessAction _processAction;
    }
}
