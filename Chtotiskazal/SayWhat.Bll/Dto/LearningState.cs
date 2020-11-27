using System;
using System.Collections.Generic;
using System.Linq;
using SayWhat.MongoDAL.Examples;

// ReSharper disable MemberCanBePrivate.Global

 namespace SayWhat.Bll.Dto
 {
  
     public enum LearningState
     {
         New = 0,
         Familiar = 1,
         Known = 2,
         NotSure = 3,
         PreLearned = 4,
         Done = 5,
     }
 }