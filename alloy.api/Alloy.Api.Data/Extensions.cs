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
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;

namespace Alloy.Api.Data
{
    public static class StringExtensions
    {
        public static string Key(this String str)
        {
            if (str == null)
                return "";

            int x = str.IndexOf('=');
            if (x > -1)
                return str.Substring(0, x).Trim();
            
            return str.Trim();
        }

        public static string Modifier(this String str)
        {
            return str.Key().LastSuffix();
        }

        public static string Value(this String str)
        {
            if (str == null)
                return "";
            
            int x = str.IndexOf('=');
            if (x > -1)
                return str.Substring(x + 1).Trim();
            
            return str.Trim();
        }

        public static string Prefix(this String str)
        {
            if (str == null)
                return "";
            int x = str.IndexOf('.');
            if (x > -1)
                return str.Substring(0, x).Trim();
            
            return str.Trim();
        }

        public static string Suffix(this String str)
        {
            if (str == null)
                return "";
            int x = str.IndexOf('.');
            if (x > -1)
                return str.Substring(x + 1).Trim();

            return str.Trim();
        }

        public static string LastSuffix(this String str)
        {
            if (str == null)
                return "";
            int x = str.LastIndexOf('.');
            if (x > -1)
                return str.Substring(x + 1).Trim();
            
            return str.Trim();
        }

        public static string Inner(this String str)
        {
            if (str == null)
                str = "";

            int x = str.IndexOf('[');
            int y = str.IndexOf(']');
            if (x > -1 && y > x)
                str = str.Substring(x + 1, y - x - 1);
            
            return str.Trim();
        }
        public static string Field(this String str, int index)
        {
            string r = "";
            string[] a = str.Split('|');
            if (index < a.Length)
                r = a[index].Trim();
            return r;
        }
        public static string Token(this String str, int index)
        {
            string r = "";
            string[] a = str.Split(new char[] { ' ', ',', '\t', '|' });
            if (index < a.Length)
                r = a[index].Trim();
            return r;
        }

        public static string Pluck(this String str, char lead, char trail)
        {
            string r = "";
            int x = str.LastIndexOf(lead);
            int y = str.LastIndexOf(trail, x);
            if (x > -1 && y > x)
                r = str.Substring(x + 1, y - x - 1);
            else if (x > -1)
                r = str.Substring(x + 1);
            else
                r = str.Substring(0, y);
            return r;

        }
        public static string[] Find(this string[] list, string target)
        {
            List<string> result = new List<string>();
            foreach (string s in list)
                if (s.StartsWith(target))
                    result.Add(s);
            return result.ToArray();
        }

        public static string FindOne(this string[] list, string target)
        {
            string result = "";
            foreach (string s in list)
                if (s.StartsWith(target))
                {
                    result = s;
                    break;
                }
            return result;
        }

        public static string Substitute(this string input, char target, int val)
        {
            if (input == null)
                return "";
            
            /*
             *      Replace "#" sequence with leading-zero value
             */
            string result = input;
            string match = "";
            string replacement = "";
            char[] array = result.ToCharArray();

            //find sequence of ?? and build format string
            for (int i = 0; i < array.Length; i++)
            {
                char c = array[i];
                if (c == target)
                {
                    match += c;
                    replacement += '0';
                }
                else
                {
                    if (match.Length > 0)
                        break;
                }
            }

            //replace sequence of ## with zero padded value
            if (!string.IsNullOrEmpty(match))
            {
                string format = "{0:" + replacement + "}";
                result = input.Replace(match, String.Format(format, val));
            }

            return result;
        }

        // public static string Key(this Alloy.Api.Data.Entity item)
        // {
        //     Type T = item.GetType();
        //     if (T.BaseType != typeof(Alloy.Api.Data.Entity))
        //         T = T.BaseType;

        //     return String.Format("{0}|{1}", T.Name, item.Id);
        // }
        
        public static string RemoveRange(this string item)
        {
            int x = item.IndexOf('[');
            int y = item.IndexOf(']');
            if (x < 0 || y < 0 || y - x < 2)
                return item;
            else
            {
                return item.Substring(0, x);
            }
        }

        public static List<string> ExpandRange(this string item)
        {
            List<string> result = new List<string>();
            int x = item.IndexOf('[');
            int y = item.IndexOf(']');
            if (x < 0 || y < 0 || y - x < 2)
                result.Add(item);
            else
            {
                string r = item.Substring(x, y - x + 1);
                foreach (int i in ToRangeList(r))
                    result.Add(item.Replace(r, i.ToString()));
            }

            return result;
        }

        //Get Range from a full name with range, e.g. EX2.USER?[1-2] gives 1,2
        public static List<int> GetRange(this string input)
        {
            List<int> list = new List<int>();

            int i = input.LastIndexOf('[');

            if (i < 0)
            {
                return list;
            }
            else
            {
                input = input.Substring(i);
                return ToRangeList(input);
            }
        }

        public static List<int> ToRangeList(this string input)
        {
            if (input == null)
                input = "";
            List<int> list = new List<int>();
            input = input.Replace("[", "").Replace("]", "");
            string[] parts = input.Split(',');
            foreach (string p in parts)
            {
                if (p.IndexOf('-') > 0)
                {
                    string[] range = p.Split('-');
                    int i = 0, j = 0;
                    Int32.TryParse(range.First(), out i);
                    Int32.TryParse(range.Last(), out j);
                    for (int k = i; k < j + 1; k++)
                        list.Add(k);
                }
                else
                {
                    int m = 0;
                    if (Int32.TryParse(p, out m))
                        list.Add(m);
                }
            }
            return list;
        }

        public static string ToRange(this List<int> list)
        {
            string r = "";
            int[] nums = list.Distinct().OrderBy(i => i).ToArray();
            int c = 0, sp = 0, ep = 0;
            while (c < nums.Length)
            {
                bool skip = (nums[c] - ep == 1);
                //initialize
                if (sp == 0)
                {
                    sp = nums[c];
                    skip = true;
                }

                //skip increments
                if (skip)
                {
                    ep = nums[c];
                }
                else //add range when there is a break in sequence
                {
                    r += sp.ToString();
                    if (ep > sp)
                    {
                        r += "-" + ep.ToString();
                    }

                    r += ",";
                    
                    //reset start point
                    sp = nums[c];
                    ep = sp;
                }
                c += 1;
            }

            //add last number
            r += sp.ToString();
            if (ep > sp)
            {
                r += "-" + ep.ToString();
            }

            return r;
        }

        public static string ToLines(this string[] s)
        {
            return string.Join("\r\n", s);
        }

        public static string[] Lines(this string s)
        {
            if (s == null)
                s = "";
            return System.Text.RegularExpressions.Regex.Split(s, "\r\n|\r|\n");
        }

        public static string[] Terms(this string s)
        {
            return Regex.Matches(s, "(?<match>[^\\s\"]+)|(?<match>\"[^\"]*\")")
                                .Cast<Match>()
                                .Select(m => m.Groups["match"].Value.Trim('"'))
                                .ToArray();
        }

        public static string[] NumericSort(this string[] s)
        {
            AlphaNumericStringComparer comparer = new AlphaNumericStringComparer();
            List<string> list = s.ToList();
            list.Sort(comparer);
            return list.ToArray();
        }

        public static string ToRelativeTime(this DateTime dt)
        {
            DateTime now = DateTime.UtcNow;
            TimeSpan ts = now.Subtract(dt);
            string fmt = (dt.Year == now.Year) ? "M/d HH:mm" : "M/d/yy HH:mm";
            string result = dt.ToString(fmt);
            
            if (ts.TotalSeconds < 0)
            {
                ts = dt.Subtract(DateTime.UtcNow);
                if (ts.Days == 0)
                {
                    if (ts.Hours > 0)
                    {
                        result = string.Format("in {0}h {1}m", ts.Hours, ts.Minutes);
                    }
                    else if (ts.Minutes > 0)
                    {
                        result = String.Format("in {0}m", ts.Minutes);
                    }
                    else 
                    { 
                        result = String.Format("in {0}s", ts.Seconds);
                    }
                }
            }
            else
            {
                if (ts.TotalDays < 1)
                    result = String.Format("{0}h ago", ts.Hours);
                if (ts.TotalHours < 1)
                    result = String.Format("{0}m ago", ts.Minutes);
                if (ts.TotalMinutes < 1)
                    result = string.Format("{0}s ago", ts.Seconds);
            }
            return result;
        }

        public static string ToDayTime(this DateTime dt)
        {
            return (dt.Date == DateTime.UtcNow.Date)
                ? dt.ToString("HH:mm")
                : dt.ToString("M/d HH:mm");
        }

        public static string ToRelativeAndActualTime(this DateTime dt)
        {
            return dt.AddDays(1).CompareTo(DateTime.UtcNow) < 0 ? dt.ToDayTime() : String.Format("{0} [{1}]", dt.ToRelativeTime(), dt.ToDayTime());
        }

        public static int ToDurationSeconds(this string time)
        {
            int result = 0;
            int[] m = new int[] { 1, 60, 3600, 86400, 604800 };
            int j = -1, x = -1, y = -1;

            if (time == null)
                return 0;

            //normalize input
            time = time.ToLower().Trim();
            if (time.HasValue() && !time.Contains(":"))
            {
                char c = time.ToCharArray().Last();
                switch (c)
                {
                    case 'm':
                        time += "0s";
                        break;

                    case 'h':
                        time += "0m0s";
                    break;

                    case 'd':
                        time += "0h0m0s";
                        break;

                    case 'w':
                        time += "0d0h0m0s";
                        break;
                }
            }

            //interate and process segmented digits
            char[] chars = time.ToCharArray();
            for (int i = chars.Length - 1; i >= 0; i--)
            {
                if (Char.IsDigit(chars[i]))
                {
                    if (y == -1)
                    {
                        y = i;
                        j++;
                    }
                    x = i;
                }
                else
                {
                    if (y != -1)
                    {
                        result += Int32.Parse(time.Substring(x, y-x+1)) * ((j < m.Length) ? m[j] : 0);
                        y = -1;
                    }
                }
            }
            if (y != -1)
            {
                result += Int32.Parse(time.Substring(x, y-x+1)) * ((j < m.Length) ? m[j] : 0);
                y = -1;
            }
            return result;
        }

        public static string ToDurationString(this int seconds)
        {
            string result = "";
            char[] suffix = new char[] { 'w', 'd', 'h', 'm', 's' };
            int[] m = new int[] { 604800, 86400, 3600, 60, 1 };
            for (int i = 0; i < m.Length; i++)
            {
                int d = seconds / m[i];
                if (d > 0)
                {
                    result += String.Format("{0}{1} ", d, suffix[i]);
                    seconds -= d * m[i];
                }
            }
            if (!result.HasValue())
            {
                result = "0s";
            }

            return result;
        }

        public static string StripTrailingDigits(this string s)
        {
            char[] a = s.ToCharArray();
            int i = a.Length;
            while (i > 0)
            {
                if (!Char.IsDigit(a[i-1]))
                {
                    break;
                }
                i--;
            }
            return s.Substring(0, i);
        }

        public static bool HasValue(this string s)
        {
            return !String.IsNullOrWhiteSpace(s); // (s == null) ? false : s.Length > 0;
        }

        public static string Pathify(this string s)
        {
            string result = "";
            char[] invalidchars = System.IO.Path.GetInvalidFileNameChars();
            foreach (char c in s.ToCharArray())
                if (invalidchars.Contains(c))
                    result += "-";
                else
                    result += c;
            return result;
        }

        public static string UrlSanitize(this string s)
        {
            s = s.Replace(" ", "_");
            s = Regex.Replace(s, @"[^\w-\+]", "", RegexOptions.None);

            return s;
        }

        public static string ReplaceWithCase(this string s, string target, string replacement)
        {
            string result = s;
            
            int x = s.ToLower().IndexOf(target.ToLower());
            if (x > -1)
            {
                //prep arrays
                int y = x + target.Length;
                char[] originalChars = s.ToCharArray();
                char[] newChars = new char[s.Length + replacement.Length - target.Length];
                char[] replChars = replacement.ToCharArray();
                
                //fill prefix
                for (int i = 0; i < x; i++)
                {
                    newChars[i] = originalChars[i];
                }

                //fill new chars, with matching case
                for (int i = 0; i < replChars.Length; i++)
                {
                    newChars[x] = replacement[i];
                    if (i < target.Length && Char.IsLetter(newChars[x]))
                    {
                        //make new char Upper, then combine original char's case
                        newChars[x] = (char)(((newChars[x] | 0x20) ^ 0x20) | (originalChars[x] & 0x20));
                    }
                    x += 1;
                }

                //fill suffix
                for (int i = y; i < originalChars.Length; i++)
                    newChars[x++] = originalChars[i];
                
                //01234567890
                //jam.test   
                //jeff.test  

                result = new String(newChars);
            }

            return result;
        }

        public static string Truncate(this string value, int maxLength)
        {
            return Truncate(value, maxLength, String.Empty);
        }
        public static string Truncate(this string value, int maxLength, string trailingString)
        {
            if (string.IsNullOrEmpty(value)) return value;

            if(value.Length <= maxLength)
            {
                return value;
            }
            else
            {
                return value.Substring(0, maxLength) + trailingString;
            }
        }

        public static bool Matches(this string value, string match)
        {
            const string variable = "{VARIABLE}";

            string[] tokens = value.Trim().Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            match = match.Trim();

            List<string> searchterms = new List<string>();
            string searchTerm = String.Empty;

            foreach(string token in tokens)
            {
                if(token == variable)
                {
                    if(!String.IsNullOrEmpty(searchTerm))
                    {
                        searchterms.Add(searchTerm.Remove(searchTerm.Length - 1));
                        searchTerm = String.Empty;
                    }

                    searchterms.Add(token);
                }
                else
                {
                    searchTerm += token + ' ';
                }
            }

            if(tokens.Last() != variable)
            {
                searchterms.Add(searchTerm.Remove(searchTerm.Length - 1));
            }

            bool variablePrevious = false;
            foreach(string term in searchterms)
            {
                if(term == variable)
                {
                    variablePrevious = true;
                    continue;
                }

                if(variablePrevious)
                {
                    int index = match.IndexOf(term);

                    if(index >= 0)
                    {
                        match = match.Substring((match.IndexOf(term) + term.Length));
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    if(match.StartsWith(term))
                    {
                        match = match.Substring((match.IndexOf(term) + term.Length));
                    }
                    else
                    {
                        return false;
                    }
                }

                variablePrevious = false;
            }

            return true;
        }

        public static string FilterToPrefixedTokens(this string input)
        {
            List<string> tokens = input.Split(' ').ToList();
            List<string> results = new List<string>();

            foreach (string token in tokens)
            {
                results.Add(token.StripTrailingDigits());
            }
            return String.Join(" ", results.Distinct());
        }

        public static string FilterToBaseTokens(this string input)
        {
            List<string> tokens = input.Split(' ').ToList();
            List<string> results = new List<string>();

            foreach (string token in tokens)
            {
                results.Add(token.StripTrailingDigits().Suffix());
            }
            return String.Join(" ", results.Distinct());
        }        

        public static int ParseChassisNumber(this string name)
        {
            string r = "";
            int i = 0, result = 0;
            int lastchar = 0x41;
            int chassis = 0;

            //return any digits from end of name
            char[] ca = name.ToUpper().ToCharArray();
            for (i = 0; i < ca.Length; i++)
            {
                if (Char.IsDigit(ca[i]))
                {
                    r += ca[i];
                }
                else
                {
                    if (!String.IsNullOrEmpty(r))
                        break;
                    lastchar = (int)ca[i];
                }
            }
            chassis = lastchar - 0x41;
            Int32.TryParse(r, out result);
            result += chassis * 16;
            return result;
        }
    }

    public static class IntegerExtensions
    {
        public static string FormatBytes(this int size)
        {
            return ((long)size).FormatBytes();
        }
        public static string FormatBytes(this long size)
        {
            string[] tags = new string[] { "", "KB", "MB", "GB", "TB", "EB", "PB" };
            int count = 0;
            double s = size;
            while (s > 1024)
            {
                s /= 1024;
                count += 1;
            }

            int magnitude = (s == 0.0) ? 0 : (int)Math.Floor(Math.Log10(Math.Abs(s))) + 1;
            int digits = Math.Max(2 - magnitude, 0);
            return s.ToString("f" + digits.ToString()) + tags[count];
            //return String.Format("{0:N0}{1}", s, tags[count]);
        }        
    }

    public static class ObjectExtensions
    {
        public static bool Inherits(this Object obj, Type t)
        {
            Type b = obj.GetType();
            do
            {
                b = b.BaseType;
                //Console.WriteLine("Inheritance check t:{0} b:{1}", t.Name, b.Name);
            } while (b != typeof(Object) && b != t);
            
            return b == t;
        }

        // public static Type EntityType(this Object obj)
        // {
        //     Type t = obj.GetType();
        //     while (t.BaseType != typeof(Data.Entity) && t.BaseType != typeof(Object))
        //         t = t.BaseType;
        //     return t;
        // }

        // public static object AsEntity(this Object obj)
        // {
        //     Type t = obj.EntityType();
        //     Object o = Activator.CreateInstance(t);
        //     foreach (PropertyInfo pi in t.GetProperties())
        //     {
        //         if (pi.SetMethod != null && (pi.PropertyType.IsValueType || pi.PropertyType == typeof(String)))
        //             pi.SetValue(o, pi.GetValue(obj));
        //     }
        //     return o;
        // }

        public static object GetPropertyValue(this object o, string Name)
        {
            return o.GetType().GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Where(x => x.Name == Name).First().GetValue(o, null);
        }

        public static List<Object> NumericSort(this List<Object> list, string member)
        {
            list.Sort(new AlphaNumericComparer(member));
            return list;
        }
        // public static List<Entity> NumericEntitySort(this List<Entity> list, string member)
        // {
        //     list.Sort(new Data.AlphaNumericComparer(member));
        //     return list;
        // }
    }

    // public static class ContextExtensions
    // {        
    //     public static string GetTableName<T>(this DbContext context) where T : class
    //     {
    //         ObjectContext objectContext = ((IObjectContextAdapter)context).ObjectContext;
    //         return objectContext.GetTableName<T>();
    //     }

    //     public static string GetTableName<T>(this ObjectContext context) where T : class
    //     {
    //         string sql = context.CreateObjectSet<T>().ToTraceString();
    //         Regex regex = new Regex("FROM (?<table>.*) AS");
    //         Match match = regex.Match(sql);

    //         string table = match.Groups["table"].Value;

    //         //Parse out just the x in [y].[x] (y usually = dbo)
    //         table = table.Substring(table.LastIndexOf('[') + 1);
    //         table = table.Remove(table.Length - 1);

    //         return table;
    //     }

    //     public static IEnumerable<string> GetPrimaryKeys<T>(this DbContext context) where T : class
    //     {
    //         ObjectContext objectContext = ((IObjectContextAdapter)context).ObjectContext;
    //         ObjectSet<T> set = objectContext.CreateObjectSet<T>();
    //         IEnumerable<string> keyNames = set.EntitySet.ElementType.KeyMembers.Select(k => k.Name);
    //         return keyNames;
    //     }

    //     //Returns a Dictionary where the key is the name of the table and the Value is a list of property names in that table, which are dependant upon T
    //     public static Dictionary<string, List<string>> GetDependantRelationships<T>(this ViewContext context) where T : class
    //     {
    //         Dictionary<string, List<string>> dict = new Dictionary<string, List<string>>();

    //         var dbSets = context.GetType().GetProperties().Where(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeEventTemplate() == typeof(DbSet<>));

    //         foreach (var dbSet in dbSets)
    //         {
    //             dynamic set = dbSet.GetValue(context);

    //             Type type = typeof(ContextExtensions);
    //             MethodInfo method = type.GetMethod("GetForeignKeys");
    //             MethodInfo genericMethod = method.MakeGenericMethod(dbSet.PropertyType.BaseType.GetGenericArguments().First());

    //             KeyValuePair<string, List<string>> kvp = (KeyValuePair<string, List<string>>)genericMethod.Invoke(null, new object[] { set, context, typeof(T) });

    //             if(kvp.Value.Any())
    //             {
    //                 if(!dict.ContainsKey(kvp.Key))
    //                 {
    //                     dict.Add(kvp.Key, kvp.Value);
    //                 }
    //             }
    //         }

    //         return dict;
    //     }

    //     public static KeyValuePair<string, List<string>> GetForeignKeys<T>(this DbSet<T> set, DbContext context, Type typeToFind) where T : class
    //     {
    //         List<string> foreignKeyNames = new List<string>();

    //         ObjectContext objectContext = ((IObjectContextAdapter)context).ObjectContext;
    //         ObjectSet<T> objectSet = objectContext.CreateObjectSet<T>();

    //         foreach (var navigationProperty in objectSet.EntitySet.ElementType.NavigationProperties)
    //         {
    //             if (navigationProperty.TypeUsage.EdmType.FullName == typeToFind.FullName && navigationProperty.ToEndMember.DeleteBehavior != OperationAction.Cascade)                    
    //             {
    //                 foreach (var foreignKey in navigationProperty.GetDependentProperties())
    //                 {
    //                     foreignKeyNames.Add(foreignKey.Name);
    //                 }
    //             }
    //         }

    //         return new KeyValuePair<string, List<string>>(objectContext.GetTableName<T>(), foreignKeyNames);
    //     }
    // }

    // public static class IListExtensions
    // {
    //     public static List<T> Search<T>(this IList<T> input, Entity root, string term)
    //     {
    //         return input.AsQueryable().Search(root, term).ToList();
    //     }

    //     public static string ToDelimitedString(this IEnumerable<string> list, string delimiter)
    //     {
    //         return string.Join(delimiter, list.ToArray());
    //     }

    //     public static void ReplaceValue(this IList<string> config, string target, string value)
    //     {
    //         if (String.IsNullOrEmpty(value))
    //             return;

    //         for (int i = 0; i < config.Count; i++)
    //             config[i] = config[i].Replace(target, value);

    //     }
    // }

    // public static class IQueryableExtensions
    // {
    //     public static ObjectQuery<T> ToObjectQuery<T>(this IQueryable<T> query)
    //     {
    //         ObjectQuery<T> objectQuery = (ObjectQuery<T>)(query.GetPropertyValue("InternalQuery").GetPropertyValue("ObjectQuery"));
    //         return objectQuery;
    //     }

    //     public static IQueryable<T> Search<T>(this IQueryable<T> input, Entity root, string term, StringComparison comparison = StringComparison.InvariantCultureIgnoreCase, bool sql = false, bool searchNavigationProperties = false)
    //     {
    //         Expression expression = null;
    //         ParameterExpression pe = Expression.Parameter(typeof(T), "o");

    //         if (root != null)
    //         {
    //             //search where parentid == parent.Id
    //             string propname = root.GetType().BaseType.Name + "Id";
    //             PropertyInfo pi = typeof(T).GetProperty(propname);
    //             if (pi != null)
    //             {
    //                 expression = Expression.Equal(Expression.Convert(Expression.Property(pe, pi.Name), root.Id.GetType()), Expression.Constant(root.Id));

    //             }
    //             else
    //             {
    //                 if (root.GetType().BaseType == typeof(T))
    //                     expression = Expression.Equal(Expression.Constant(1), Expression.Constant(0)); //searching noexistant property; so return nothing.
    //             }
    //         }

    //         if (!String.IsNullOrEmpty(term))
    //         {
    //             //Parse term for multiple search terms
    //             string[] terms = term.Terms();

    //             Expression matched = default(Expression<Func<T, bool>>);                
    //             System.Reflection.MethodInfo stringMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
    //             System.Reflection.MethodInfo intMethod = typeof(int).GetMethod("Equals", new[] { typeof(int) });
    //             System.Reflection.MethodInfo guidMethod = typeof(Guid).GetMethod("Equals", new[] { typeof(Guid) });

    //             foreach (PropertyInfo info in typeof(T).GetProperties())
    //             {
    //                 SearchAttribute sa = info.GetCustomAttribute<SearchAttribute>();

    //                 if (sa != null)
    //                 {
    //                     List<PropertyInfo> propertyInfos = new List<PropertyInfo>();
    //                     bool navigationProperty = false;

    //                     if (typeof(Entity).IsAssignableFrom(info.PropertyType))
    //                     {
    //                         if(searchNavigationProperties)
    //                         {
    //                             propertyInfos = info.PropertyType.GetProperties().ToList();
    //                             navigationProperty = true;
    //                         }                            
    //                         else
    //                         {
    //                             continue;
    //                         }
    //                     }
    //                     else
    //                     {
    //                         propertyInfos.Add(info);
    //                     }

    //                     foreach (PropertyInfo subInfo in propertyInfos)
    //                     {
    //                         if(navigationProperty)
    //                         {
    //                             sa = subInfo.GetCustomAttribute<SearchAttribute>();
    //                         }

    //                         System.Reflection.MethodInfo method = stringMethod;

    //                         if (sa != null && subInfo.PropertyType == typeof(string) || subInfo.PropertyType == typeof(int) || subInfo.PropertyType == typeof(Guid))
    //                         {
    //                             Expression propertyExpression = null;

    //                             foreach (string searchTerm in terms)
    //                             {
    //                                 Expression searchTermExpression = null;
    //                                 MemberExpression propertyAccess = Expression.MakeMemberAccess(pe, info);
    //                                 Type searchType = typeof(string);
    //                                 Expression nullExp = null;

    //                                 object searchTermObject = null;

    //                                 if(subInfo.PropertyType == typeof(string))
    //                                 {
    //                                     searchTermObject = searchTerm;
    //                                     nullExp = Expression.Constant(null, subInfo.PropertyType);
    //                                 }
    //                                 else if(subInfo.PropertyType == typeof(int))
    //                                 {
    //                                     int intSearchTerm = 0;

    //                                     if(Int32.TryParse(searchTerm, out intSearchTerm))
    //                                     {
    //                                         searchTermObject = intSearchTerm;
    //                                         method = intMethod;
    //                                         searchType = typeof(int);
    //                                         nullExp = Expression.Constant(0, subInfo.PropertyType);
    //                                     }
    //                                     else
    //                                     {
    //                                         continue;
    //                                     }
    //                                 }
    //                                 else if(subInfo.PropertyType == typeof(Guid))
    //                                 {
    //                                     Guid guidSearchTerm = Guid.Empty;

    //                                     if(Guid.TryParse(searchTerm, out guidSearchTerm))
    //                                     {
    //                                         searchTermObject = guidSearchTerm;
    //                                         method = guidMethod;
    //                                         searchType = typeof(Guid);
    //                                         nullExp = Expression.Constant(Guid.Empty, subInfo.PropertyType);
    //                                     }
    //                                     else
    //                                     {
    //                                         continue;
    //                                     }
    //                                 }

    //                                 if(navigationProperty)
    //                                 {
    //                                     propertyAccess = Expression.MakeMemberAccess(propertyAccess, subInfo);
    //                                 }

    //                                 if (sql)
    //                                 {
    //                                     searchTermExpression = Expression.Call(propertyAccess, method, Expression.Constant(searchTermObject));
    //                                 }
    //                                 else
    //                                 {
    //                                     searchTermExpression = Expression.Call(propertyAccess, "IndexOf", null, Expression.Constant(searchTermObject, searchType), Expression.Constant(comparison));
    //                                     searchTermExpression = Expression.GreaterThanOrEqual(searchTermExpression, Expression.Constant(0));
    //                                 }
                                    
    //                                 Expression nullCheck = Expression.NotEqual(propertyAccess, nullExp);
    //                                 searchTermExpression = Expression.Condition(nullCheck, searchTermExpression, Expression.Constant(false));

    //                                 if (propertyExpression == null)
    //                                     propertyExpression = searchTermExpression;
    //                                 else
    //                                     propertyExpression = Expression.And(propertyExpression, searchTermExpression);
    //                             }

    //                             if(propertyExpression != null)
    //                             {
    //                                 if (matched == null)
    //                                 {
    //                                     matched = propertyExpression;
    //                                 }                                                                            
    //                                 else
    //                                 {
    //                                     matched = Expression.Or(matched, propertyExpression);
    //                                 }                                        
    //                             }                                
    //                         }
    //                     }
    //                 }                                                                     
    //             }

    //             if (matched == null)
    //             {
    //                 matched = Expression.Equal(Expression.Constant(1), Expression.Constant(0)); //if term but no searchable fields, fail.
    //             }                    
                
    //             expression = (expression == null) ? matched : Expression.And(expression, matched);
    //         }

    //         return (expression == null) ? input : input.Where(Expression.Lambda<Func<T, bool>>(expression, pe));
    //     }

    // }
}

