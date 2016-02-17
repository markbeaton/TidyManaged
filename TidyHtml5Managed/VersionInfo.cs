namespace TidyManaged
{
    /// <summary>
    /// VersionInfo class
    /// </summary>
    public class VersionInfo
    {
        #region Private Variables
        private const string MIN_SUPPORTED_VERSION = "5.0.0";

        private const int MAJOR = 0;
        private const int MINOR = 1;
        private const int PATCH = 2;
        
        private int[] _version;
        #endregion

        #region Properties

        /// <summary>
        /// Major version
        /// </summary>
        public int VersionMajor
        {
            get { return _version[MAJOR]; }
        }

        /// <summary>
        /// Minor version
        /// </summary>
        public int VersionMinor
        {
            get { return _version[MINOR]; }
        }

        /// <summary>
        /// Patch version
        /// </summary>
        public int VersionPatch
        {
            get { return _version[PATCH]; }
        }

        /// <summary>
        /// The underlying Tidy HTML5 library is a supported version or not
        /// </summary>
        public bool IsSupportedVersion
        {
            get { return IsGreaterThanOrEquals(MinSupported); }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Instantiate version object
        /// </summary>
        /// <param name="sVersion">Version number in string</param>
        public VersionInfo(string sVersion)
        {
            _version = new int[3];

            if (!string.IsNullOrEmpty(sVersion))
            {
                string[] vArr = sVersion.Split('.');
                if (vArr.Length > 0)
                    int.TryParse(vArr[0], out _version[MAJOR]);
                if (vArr.Length > 1)
                    int.TryParse(vArr[1], out _version[MINOR]);
                if (vArr.Length > 2)
                    int.TryParse(vArr[2], out _version[PATCH]);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Check whether current version is greater than or equals the compared version
        /// </summary>
        /// <param name="versionToCompare">Compared version</param>
        /// <returns></returns>
        public bool IsGreaterThanOrEquals(VersionInfo versionToCompare)
        {
            if (_version[MAJOR] > versionToCompare.VersionMajor)
                return true;
            else if (_version[MAJOR] == versionToCompare.VersionMajor)
            {
                if (_version[MINOR] > versionToCompare.VersionMinor)
                    return true;
                else if (_version[MINOR] == versionToCompare.VersionMinor)
                {
                    return _version[PATCH] >= versionToCompare.VersionPatch;
                }
            }

            return false;
        }

        #endregion

        #region Static

        /// <summary>
        /// Minimum supported version of underlying Tidy HTML5 library
        /// </summary>
        public static VersionInfo MinSupported
        {
            get { return new VersionInfo(MIN_SUPPORTED_VERSION); }
        }
        
        #endregion
    }
}
