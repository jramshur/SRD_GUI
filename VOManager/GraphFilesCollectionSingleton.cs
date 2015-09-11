using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GraphLib;




namespace VOManager
{
   public  class GraphFileCollectionSingleton
    {
       private static GraphFileCollectionSingleton instance = new GraphFileCollectionSingleton();

       private GraphFileCollectionSingleton() { }

       public static GraphFileCollectionSingleton getInstance()
       {
           return instance;
       }

       public Dictionary<ushort, List<string>> channelFileMap = new Dictionary<ushort, List<string>>();
    }
}
