/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace S3.Vm.Console.Extensions
{
    public static class StringExtensions
    {
        //check for string value
        public static bool HasValue(this string s)
        {
            return (!String.IsNullOrWhiteSpace(s));
        }

        //check for presence of array values
        public static bool IsEmpty(this object[] o)
        {
            return (o == null || o.Length == 0);
        }

        public static bool IsNotEmpty(this object[] o)
        {
            return (o != null && o.Length > 0);
        }

        //expands a range string (i.e. [1-3,5,7,10-12]) into an int list
        public static int[] ExpandRange(this string s)
        {
            s = s.Replace("[", "").Replace("]", "");
            List<int> list = new List<int>();
            string[] sections = s.Split(',');
            foreach (string section in sections)
            {
                //Console.WriteLine(section);
                string[] token = section.Split('-');
                int x = 0, y = 0;
                if (Int32.TryParse(token[0], out x))
                {
                    if (token.Length > 1)
                    {
                        if (!Int32.TryParse(token[1], out y))
                            y = x;
                    }
                    else
                    {
                        y = x;
                    }
                    for (int i = x; i <= y; i++)
                    {
                        //Console.WriteLine(i);
                        list.Add(i);
                    }
                }
            }
            return list.ToArray();
        }

        //extracts string from brackets [].
        public static string Inner(this string s)
        {
            if (s == null)
                s = "";

            int x = s.IndexOf('[');
            if (x > -1)
            {
                int y = s.IndexOf(']', x);
                if (x > -1 && y > -1)
                    s = s.Substring(x + 1, y - x - 1);
            }
            return s.Trim();
        }

        public static string Tag(this string s)
        {
            if (s.HasValue())
            {
                int x = s.IndexOf("#");
                if (x >= 0)
                    return s.Substring(x + 1);
            }
            return "";
        }
        //strips hashtag+ from string
        public static string Untagged(this string s)
        {
            if (s.HasValue())
            {
                int x = s.IndexOf("#");
                if (x >= 0)
                    return s.Substring(0, x);
            }
            return s;
        }

        public static string ExtractBefore(this string s, string target)
        {
            int x = s.IndexOf(target);
            return (x > -1)
                ? s.Substring(0, x)
                : s;
        }
        public static string ExtractAfter(this string s, string target)
        {
            int x = s.IndexOf(target);
            return (x > -1)
                ? s.Substring(x + 1)
                : s;
        }

        //Note: this assumes a guid string (length > 16)
        public static string ToSwitchName(this string s)
        {
            return String.Format("sw#{0}..{1}", s.Substring(0, 8), s.Substring(s.Length - 8));
        }

        public static string ToAbbreviatedHex(this string s)
        {
            return (s.Length > 8)
                ? String.Format("{0}..{1}", s.Substring(0, 4), s.Substring(s.Length - 4))
                : s;
        }

        //System.Reflection not fully baked in dotnet core 1.0.0
        // public static void ReplaceString(this object obj, string pattern, string val)
        // {
        //     Type t = obj.GetType();
        //     foreach (PropertyInfo pi in t.GetRuntimeProperties())
        //     {
        //         if (pi.SetMethod != null && pi.PropertyType == typeof(String))
        //             pi.SetValue(obj, pi.GetValue(obj).ToString().Replace(pattern, val));
        //     }
        // }

        // public static object Clone(this object obj)
        // {
        //     Type t = obj.GetType();
        //     Object o = Activator.CreateInstance(t);
        //     foreach (PropertyInfo pi in t.GetRuntimeProperties())
        //     {
        //         if (pi.SetMethod != null && (pi.PropertyType.IsValueType || pi.PropertyType == typeof(String)))
        //             pi.SetValue(o, pi.GetValue(obj));
        //     }
        //     return o;
        // }
    }

}

