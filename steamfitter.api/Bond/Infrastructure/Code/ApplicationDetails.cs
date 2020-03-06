/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

using System.IO;
using System.Reflection;

namespace Bond.Infrastructure.Code
{
    /// <summary>
    /// Configuration object
    /// </summary>
    public static class ApplicationDetails
    {
        /// <summary>
        /// Returns current exe name
        /// </summary>
        internal static string Name => Assembly.GetEntryAssembly().GetName().Name;

        /// <summary>
        /// Returns current exe version
        /// </summary>
        internal static string Version => Assembly.GetEntryAssembly().GetName().Version.ToString();

        /// <summary>
        /// The standard string renderer for app name and version
        /// </summary>
        internal static string VersionString => $"{Name} {Version}";

        /// <summary>
        /// Returns installed exe path
        /// </summary>
        internal static string InstalledPath
        {
            get
            {
                try
                {
                    return Path.GetDirectoryName(Assembly.GetEntryAssembly().CodeBase)?.Replace("file:\\", "");
                }
                catch
                {
                    return Assembly.GetEntryAssembly().Location;
                }
            }
        }
    }
}
