using System;
using System.Collections.Generic;

namespace EZB.BuildEngine
{
    public class Engine
    {
        public Engine()
        {
        }

        public Build CreateBuild(string profilePath)
        {
            Profile.Profile profile = new Profile.Profile(profilePath);

            return new Build(profile);
        }
    }

    public enum BuildAction
    {
        Build,
        Rebuild,
        Clean
    }

    public class Build
    {
        public void Execute(BuildAction action)
        {
            Prepare();

            ResolveStage(action, _profile.PreBuild);
            ResolveStage(action, _profile.Build);
            ResolveStage(action, _profile.PostBuild);

            ExecuteActions();
        }

        internal Build(Profile.Profile profile)
        {
            _profile = profile;
        }

        private void Prepare()
        {
            if (_environment == null)
            {
                _environment = new Environment();
                _environment.Discover();
            }

            _actions = new List<Actions.IAction>();
        }

        private void ResolveStage(BuildAction action, Profile.Stage stage)
        {
            if (stage == null)
                return;

            if (stage.Type != Profile.StageType.Build &&
                stage.Type != Profile.StageType.PostClean &&
                action == BuildAction.Clean)
            {
                return;
            }

            if (action == BuildAction.Rebuild && stage.Type == Profile.StageType.Build)
            {
                foreach (Profile.Item item in stage.Items)
                    ResolveItem(BuildAction.Clean, stage, item);

                foreach (Profile.Item item in stage.Items)
                    ResolveItem(BuildAction.Build, stage, item);
            }
            else
            {
                foreach (Profile.Item item in stage.Items)
                    ResolveItem(action, stage, item);
            }
        }

        private void ResolveItem(BuildAction action, Profile.Stage stage, Profile.Item item)
        {
            if (stage == null || item == null)
                return;

            Actions.IAction resolvedAction = null;

            if (item.Type == Profile.ItemType.Solution || item.Type == Profile.ItemType.Project)
                resolvedAction = new Actions.MSBuildAction(item.Path, action == BuildAction.Clean);
            else if (item.Type == Profile.ItemType.BatchScript || item.Type == Profile.ItemType.ShellCommand)
                resolvedAction = new Actions.ShellAction(item.Path);
            else if (item.Type == Profile.ItemType.PowerShellScript)
                resolvedAction = new Actions.PowerShellAction(item.Path);

            if (resolvedAction == null)
                throw new ApplicationException($"Don't know how to execute action of type {item.Type}");

            _actions.Add(resolvedAction);
        }

        private void ExecuteActions()
        {
            if (_actions == null || _actions.Count == 0)
                return;

            foreach (Actions.IAction action in _actions)
                action.Execute();
        }

        private Profile.Profile _profile;
        private Environment _environment;
        private List<Actions.IAction> _actions;
    }
}
