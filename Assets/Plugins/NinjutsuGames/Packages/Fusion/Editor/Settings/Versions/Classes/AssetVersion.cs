using System;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Editor
{
    [Serializable]
    internal class AssetVersion
    {
        public static readonly AssetVersion None = new AssetVersion();

        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField] private int major;
        [SerializeField] private int minor;
        [SerializeField] private int patch;

        // PROPERTIES: ----------------------------------------------------------------------------

        public int Major => major;
        public int Minor => minor;
        public int Patch => patch;

        public bool Empty => major == default &&
                             minor == default &&
                             patch == default;

        // CONSTRUCTORS: --------------------------------------------------------------------------

        public AssetVersion()
        {
        }

        public AssetVersion(string version) : this()
        {
            string[] versions = version.Split('.');
            if (versions.Length != 3) return;

            bool majorSuccess = int.TryParse(versions[0], out int majorValue);
            bool minorSuccess = int.TryParse(versions[1], out int minorValue);
            bool patchSuccess = int.TryParse(versions[2], out int patchValue);

            if (!majorSuccess || !minorSuccess || !patchSuccess) return;

            major = majorValue;
            minor = minorValue;
            patch = patchValue;
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public bool IsOlderThan(AssetVersion otherVersion)
        {
            if(otherVersion == null) return false;
            
            if (Major > otherVersion.Major) return false;
            if (Major != otherVersion.Major) return true;

            if (Minor > otherVersion.Minor) return false;
            if (Minor != otherVersion.Minor) return true;

            return Patch < otherVersion.Patch;
        }

        public bool IsNewerThan(AssetVersion otherVersion)
        {
            if(otherVersion == null) return false;

            if (Major > otherVersion.Major) return true;
            if (Major < otherVersion.Major) return false;

            if (Minor > otherVersion.Minor) return true;
            if (Minor < otherVersion.Minor) return false;

            return Patch > otherVersion.Patch;
        }

        // TO STRING: -----------------------------------------------------------------------------

        public override string ToString() => $"{Major}.{Minor}.{Patch}";
    }
}