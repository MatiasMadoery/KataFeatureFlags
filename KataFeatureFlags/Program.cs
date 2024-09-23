using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KataFeatureFlags
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var flag = new FeatureFlag("testFlag", new List<string> { "a", "b", "c"});

            var roundRobinEngine = new RoundRobinDecisionEngine();

            var randomWeightEngine = new RandomWeightDecisionEngine();

            var manager = new FeatureFlagManager();


            manager.AddConstraint("testFlag", new Dictionary<int, string> { { 1, "a" }, { 3, "b" } });


            //Test Round Robin Engine
            //Always "a"
            Console.WriteLine(manager.GetFeatureFlagValue(1, flag, roundRobinEngine));
            //Random "a" , "b", "c"
            Console.WriteLine(manager.GetFeatureFlagValue(2, flag, roundRobinEngine));
            //Always "b"
            Console.WriteLine(manager.GetFeatureFlagValue(3, flag, roundRobinEngine));

            //Test Random Weights Engine
            var weights = new List<double> { 0.5, 0.3, 0.2 };
            Console.WriteLine(manager.GetFeatureFlagValue(4, flag, randomWeightEngine,weights));

            Console.ReadKey();

        }

        public class FeatureFlag
        {
            public string Name { get; }
            public List<string> Options { get; }

            public FeatureFlag(string name, List<string> options)
            {
                Name = name;
                Options = options;
            }
        }


        public class RoundRobinDecisionEngine
        {
            private int index = 0;

            public string GetNextOption(FeatureFlag flag)
            {
                var option = flag.Options[index];
                index = (index + 1) % flag.Options.Count;
                return option;
            }
        }


        public class RandomWeightDecisionEngine
        {
            public string GetNextOption(FeatureFlag flag, List<double> weights)
            {
                var cumulativeWeights = new List<double> { 0.0 };

                for (int i = 0; i < weights.Count; i++)
                {
                    cumulativeWeights.Add(cumulativeWeights[i] + weights[i]);
                }

                var randomValue = new Random().NextDouble();
                var selectedIndex = cumulativeWeights.FindIndex(cw => cw > randomValue) - 1;

                return flag.Options[selectedIndex];
            }
        }


        public class FeatureFlagManager
        {
            private readonly Dictionary<string, string> previusDecisions = new Dictionary<string, string>();
            private readonly Dictionary<string, Dictionary<int, string>> constraints = new Dictionary<string, Dictionary<int, string>>();


            public void AddConstraint(string flagName, Dictionary<int, string> userConstraints)
            {
                constraints[flagName] = userConstraints;
            }

            public string GetFeatureFlagValue(int clienteId, FeatureFlag flag, RoundRobinDecisionEngine engine)
            {
                if (constraints.ContainsKey(flag.Name) && constraints[flag.Name].ContainsKey(clienteId))
                {
                    return constraints[flag.Name][clienteId];   
                }


                var key = $"{clienteId}-{flag.Name}";

                if (!previusDecisions.ContainsKey(key))
                {
                    previusDecisions[key] = engine.GetNextOption(flag);
                }

                return previusDecisions[key];
            }

            public string GetFeatureFlagValue(int clienteId, FeatureFlag flag, RandomWeightDecisionEngine engine, List<double> weights) 
            {
                if (constraints.ContainsKey(flag.Name) && constraints[flag.Name].ContainsKey(clienteId))
                {
                    return constraints[flag.Name][clienteId];
                }


                var key = $"{clienteId}-{flag.Name}";

                if (!previusDecisions.ContainsKey(key))
                {
                    previusDecisions[key] = engine.GetNextOption(flag, weights);
                }

                return previusDecisions[key];
            }
        }

    }
}
