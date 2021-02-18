
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using NUnit.Framework;



namespace Tests
{
    public class PathFinderTest
    {
        // A Test behaves as an ordinary method
        [Test]
        public void TestBasicPathFinding()
        {
            PathFinder pf = new PathFinder();
            var map = CreateMap();

            Vector2 start = new Vector2(1, 1);
            Vector2 end = new Vector2(4, 2);
            pf.Init(start,end, map,1);
            var result = pf.Start();
            Assert.AreEqual(4,result.Count);


        }
        // A Test behaves as an ordinary method
        [Test]
        public void TestBasicPathFindingWithObstacles()
        {
            List<Vector2> obs = new List<Vector2>()
            {
                new Vector2(2, 1),
                new Vector2(2, 2),
            };
            
            PathFinder pf = new PathFinder();
            var map = CreateMap();
            map = map.Except(obs).ToList();
            
            Vector2 start = new Vector2(1, 1);
            Vector2 end = new Vector2(4, 2);
            pf.Init(start,end, map,1);
            var result = pf.Start();
            Assert.AreEqual(5,result.Count);


        }
        [Test]
        public void TestLongPathFindingWithObstacles()
        {
            List<Vector2> obstacles = new List<Vector2>();
            obstacles.Add(new Vector2(2,1));
            obstacles.Add(new Vector2(2, 2));
            obstacles.Add(new Vector2(2, 3));
            obstacles.Add(new Vector2(3, 3));
            obstacles.Add(new Vector2(3, 4));

            obstacles.Add(new Vector2(5, 7));
            obstacles.Add(new Vector2(6, 7));
            
            PathFinder pf = new PathFinder();
            var map = CreateMap();
            map = map.Except(obstacles).ToList();
            
            Vector2 start = new Vector2(1, 1);
            Vector2 end = new Vector2(8, 8);
            pf.Init(start,end, map,1);
            var result = pf.Start();
            Assert.AreEqual(5,5);


        }
        private static List<Vector2> CreateMap()
        {
            List<Vector2> map = new List<Vector2>();
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    Vector2 node;
                    if (j % 2 == 0)
                    {
                        node = new Vector2(i, j);
                    }
                    else
                    {
                        node = new Vector2(i + 0.0f, j);
                    }

                    map.Add(node);
                }
            }

            return map;
        }
    }
}
