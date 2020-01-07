//========================================================================================
// String拡張メソッド.
// Auther: Akihiro Imada
//========================================================================================
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatLauncher
{
    public static class StringExtension
    {
        /// <summary>
        // レーベンシュタイン距離を計算する. 
        // https://smdn.jp/programming/netfx/tips/get_levenshtein_distance/
        /// </summary>
        public static int LevenshteinDistance( this string s, string t, bool isCaseSensitive )
        {
            if ( s == null )
                throw new ArgumentNullException( "s" );
            if ( t == null )
                throw new ArgumentNullException( "t" );

            if ( s.Length == 0 )
                return t.Length;
            if ( t.Length == 0 )
                return s.Length;

            var d = new int[s.Length + 1, t.Length + 1];

            for ( var i = 0; i <= s.Length; i++ )
            {
                d[i, 0] = i;
            }

            for ( var j = 0; j <= t.Length; j++ )
            {
                d[0, j] = j;
            }

            if ( !isCaseSensitive )
            {
                s = s.ToLower();
                t = t.ToLower();
            }

            for ( var j = 1; j <= t.Length; j++ )
            {
                for ( var i = 1; i <= s.Length; i++ )
                {
                    if ( s[i - 1] == t[j - 1] )
                    {
                        d[i, j] = d[i - 1, j - 1];
                    }
                    else
                    {
                        d[i, j] = Math.Min(
                            Math.Min( d[i - 1, j] + 1, d[i, j - 1] + 1 ),
                            d[i - 1, j - 1] + 1 );
                    }
                }
            }

            return d[s.Length, t.Length];
        }

        /// <summary>
        /// 標準化レーベンシュタイン距離を計算する.
        /// </summary>
        public static float NormalizedLevenshteinDistance( this string s, string t, bool isCaseSensitive )
        {
            int maxLen = Math.Max( s.Length, t.Length );
            return (( float )s.LevenshteinDistance( t, isCaseSensitive )) / maxLen;
        }
    }
}
