using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ahmazedotnet
{
    public static class ArrayExtension
    {
        static Random rand = new Random();

        public static void Shuffle<T>(this T[] deck) 
        {  
            int n = deck.Length;            
            while (n > 1) 
            {
                n--;                         
                int k = rand.Next(0, n+1);  
                T tmp = deck[k];
                deck[k] = deck[n];
                deck[n] = tmp;
            }
        }

    }
}
