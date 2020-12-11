namespace SayWhat.MongoDAL
{
    public static class CollectionTools
    {
        public static void AddValuesInplace(this int[] targetArray, int[] other)
        {
            for (int i = 0; i < targetArray.Length; i++) 
                targetArray[i] += other[i];
        }
        
        public static int[] Sum(this int[] targetArray, int[] other)
        {
            if (other == null)
                return targetArray;
            if (targetArray == null)
                return other;
            int[] res = new int[targetArray.Length];
            for (int i = 0; i < targetArray.Length; i++) 
                res[i] = targetArray[i] + other[i];
            return res;
        }
        
        public static void SetLowLimitInplace(this int[] targetArray, int lowLimit)
        {
            for (int i = 0; i < targetArray.Length; i++)
            {
                if (targetArray[i] < lowLimit)
                    targetArray[i] = lowLimit;
            }
        }
    }
}