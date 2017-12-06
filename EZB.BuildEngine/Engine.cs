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
        void Execute(BuildAction action)
        {
            // ...
        }

        internal Build(Profile.Profile profile)
        {
            _profile = profile;
        }

        private Profile.Profile _profile;
    }
}
