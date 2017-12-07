using System;

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

            ExecuteStage(action, _profile.PreBuild);
            ExecuteStage(action, _profile.Build);
            ExecuteStage(action, _profile.PostBuild);
        }

        internal Build(Profile.Profile profile)
        {
            _profile = profile;
        }

        private void ExecuteStage(BuildAction action, Profile.Stage stage)
        {
            if (stage == null)
                return;

            if (stage.Type != Profile.StageType.Build && action == BuildAction.Clean)
                return;

            foreach (Profile.Item item in stage.Items)
            {
                ExecuteItem(action, stage, item);
            }
        }

        private void ExecuteItem(BuildAction action, Profile.Stage stage, Profile.Item item)
        {
            if (item == null)
                return;

            // ...
        }

        private void Prepare()
        {
            // ...
        }

        private Profile.Profile _profile;

        private string _msBuildPath;
    }
}
