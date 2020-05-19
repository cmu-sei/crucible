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

namespace Foreman.Core.Infrastructure.Comparers
{
    public class AlphaNumericStringComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            int r = 0, ai = 0, bi = 0, am = 0, bm = 0;
            int an = 0, bn = 0;
            bool af = false, bf = false;
            var a = x.ToCharArray();
            var b = y.ToCharArray();
            
            //iterate through each character
            while (ai < a.Length && bi < b.Length && ai == bi && r == 0)
            {
                af = false;
                bf = false;

                if (char.IsDigit(a[ai]))
                {
                    //run to end of number in a
                    am = ai;
                    do
                    {
                        ai += 1;
                    } while (ai < a.Length && char.IsDigit(a[ai]));
                    int.TryParse(new string(a, am, ai - am), out an);
                    af = true;
                }
                else
                {
                    //grab char value
                    an = a[ai];
                    ai += 1;
                }

                if (Char.IsDigit(b[bi]))
                {
                    //run to end of number in b
                    bm = bi;
                    do
                    {
                        bi += 1;
                    } while (bi < b.Length && char.IsDigit(b[bi]));
                    int.TryParse(new string(b, bm, bi - bm), out bn);
                    bf = true;
                }
                else
                {
                    //grab char value
                    bn = b[bi];
                    bi += 1;
                }

                if (af == bf)               //if both numbers or both character values
                    r = an.CompareTo(bn);
                else                        // return number < text
                    r = (af) ? -1 : 1;
            }

            if (r == 0)                     //if completely matching up to termination point, return shorter < longer
                r = x.Length.CompareTo(y.Length);

            return r;
        }
    }
}
