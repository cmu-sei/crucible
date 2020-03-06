/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using NLog;

namespace Bond.Infrastructure.Code
{
    internal class FileSearch
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();
        internal readonly ConcurrentBag<FileInfo> AllFiles;

        internal FileSearch()
        {
            this.AllFiles = new ConcurrentBag<FileInfo>();
        }

        internal void WalkDirectoryTree(DirectoryInfo dr, string searchName)
        {
            FileInfo[] files = null;
            try
            {
                files = dr.GetFiles(searchName + ".*");
            }
            catch (Exception ex)
            {
                _log.Trace(ex);
            }

            if (files == null) 
                return;
            
            foreach (var fi in files)
            {
                AllFiles.Add(fi);
            }

            var subDirs = dr.GetDirectories();

            Parallel.ForEach(subDirs, dir => WalkDirectoryTree(dir, searchName));
        }
    }
}
