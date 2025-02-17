using System.Collections.Generic;
using DF.Models.Boosters;
using DF.Models.Diamonds;
using DF.Models.Miners;

namespace DF
{
    public static class Const
    {
        public const float DefaultSkipTime = 15;
        
        public static class Items
        {
            public static class Types
            {
                public const string Currency = "Currency";
                public const string Boosters = "Boosters";
            }

            public static class Currency
            {
                public const string Brilliants = "Brilliants";
            }

            public static class Boosters
            {
                public const string First = "First";
                public const string Second = "Second";
                public const string Third = "Third";
                public const string Fourth = "Fourth";
                
                public static BoosterDefinition FirstDefinition => new(First, 10, new List<BuffDefinition>()
                    { new(BuffType.MinerSpeed, 5) });

                public static BoosterDefinition SecondDefinition => new(Second, 12, new List<BuffDefinition>() 
                    { new(BuffType.MineDuration, 5), });

                public static BoosterDefinition ThirdDefinition => new(Third, 14, new List<BuffDefinition>()
                { new(BuffType.ProcessingDiamondSpeed, 5), });
                
                public static BoosterDefinition FourthDefinition => new(Fourth, 15, new List<BuffDefinition>()
                {
                    new(BuffType.MinerSpeed, 2), 
                    new(BuffType.MineDuration, 2),
                    new(BuffType.ProcessingDiamondSpeed, 2),
                });
            }
        }

        public static class Miners
        {
            public const string First = "First";
            public const string Second = "Second";
            public const string Third = "Third";
            public const string Fourth = "Fourth";
            
            public static MinerSpecification JuniorSpecification => new(5, 3);
            public static MinerSpecification MiddleSpecification => new(6, 2);
            public static MinerSpecification SeniorSpecification => new(7, 1);

            public static MinerDefinition FirstDefinition => new(JuniorSpecification, First, 20);
            public static MinerDefinition SecondDefinition => new(MiddleSpecification, Second, 20);
            public static MinerDefinition ThirdDefinition => new(SeniorSpecification, Third, 20);
            public static MinerDefinition FourthDefinition => new(SeniorSpecification, Fourth, 15);
        }

        public static class Diamonds
        {
            public const string Small = "Small";
            public const string Middle = "Middle";
            public const string Big = "Big";

            public static DiamondDefinition SmallDefinition => new(Small, 1, 1);
            public static DiamondDefinition MiddleDefinition => new(Middle, 2, 1);
            public static DiamondDefinition BigDefinition => new(Big, 3, 1);
        }
    }
}