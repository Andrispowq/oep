using System;
using System.Globalization;

namespace OEP;

public sealed class Program
{
    public static class StateMachine
    {
        public static List<IWeatherCondition> Conditions { get; private set; }
        public static List<AtmosphereLayer> Layers { get; private set; }

        private static bool initialised = false;

        public static void Init(List<IWeatherCondition> conditions, List<AtmosphereLayer> layers)
        {
            if (initialised) throw new Exception("Already initialised");
            initialised = true;

            Conditions = conditions;
            Layers = layers;
        }

        public static void Start()
        {
            if (!initialised) throw new Exception("Not initialised");

            int index = 0;
            while (!CheckOver())
            {
                var condition = Conditions[index];
                foreach (var layer in Layers)
                {
                    layer.HandleWithCondition(condition);
                }

                HandleCreatedLayers();

                Console.WriteLine("Printing round...");
                PrintState();
                Console.WriteLine("Printed round\n\n\n");

                index = NextCondition(index);
            }
        }

        private static int NextCondition(int index)
        {
            return (index + 1) % Conditions.Count;
        }

        private static void HandleCreatedLayers()
        {
            List<AtmosphereLayer> added = new();
            List<AtmosphereLayer> removed = new();

            for (int i = 0; i < Layers.Count; i++)
            {
                var layer = Layers[i];

                if(layer.FloatsUp)
                {
                    var toAdd = layer.FindSuitableAfter(i);
                    if (toAdd != null)
                    {
                        toAdd.Thickness += layer.Thickness;
                    }

                    removed.Add(layer);
                }

                if(layer.CreatedLayer != null)
                {
                    var newLayer = layer.CreatedLayer;
                    var toAdd = newLayer.FindSuitableAfter(i);
                    if (toAdd != null)
                    {
                        toAdd.Thickness += newLayer.Thickness;
                    }
                    else
                    {
                        if(!newLayer.FloatsUp)
                        {
                            added.Add(newLayer);
                        }
                    }

                    layer.CreatedLayer = null;
                }
            }

            foreach(var remove in removed)
            {
                Layers.Remove(remove);
            }
            foreach(var add in added)
            {
                Layers.Add(add);
            }
        }

        private static void PrintState()
        {
            foreach(var layer in Layers)
            {
                Console.WriteLine(layer);
            }    
        }

        private static bool CheckOver()
        {
            int ozone = 0, oxygen = 0, co2 = 0;
            foreach(var layer in Layers)
            {
                if (layer is OzoneLayer)
                    ozone++;
                else if (layer is OxygenLayer)
                    oxygen++;
                else if (layer is CO2Layer)
                    co2++;
            }

            return ozone == 0 || oxygen == 0 || co2 == 0;
        }
    }

    public abstract record AtmosphereLayer
    {
        public double Thickness { get; set; }
        public AtmosphereLayer? CreatedLayer { get; set; } = null;

        public bool FloatsUp => Thickness < 0.5;

        public AtmosphereLayer(double thickness) => Thickness = thickness;

        public abstract AtmosphereLayer? FindSuitableAfter(int index);
        public abstract void HandleWithCondition(IWeatherCondition condition);
    }

    public sealed record OzoneLayer : AtmosphereLayer
    {
        public OzoneLayer(double thickness) : base(thickness) { }

        public override AtmosphereLayer? FindSuitableAfter(int index)
        {
            for(int i = index + 1; i < StateMachine.Layers.Count; i++)
            {
                var layer = StateMachine.Layers[i];
                if(layer is OzoneLayer && !layer.FloatsUp)
                {
                    return layer;
                }
            }

            return null;
        }

        public override void HandleWithCondition(IWeatherCondition condition)
        {
            condition.HandleLayer(this);
        }
    }

    public sealed record OxygenLayer : AtmosphereLayer
    {
        public OxygenLayer(double thickness) : base(thickness) { }

        public override AtmosphereLayer? FindSuitableAfter(int index)
        {
            for (int i = index + 1; i < StateMachine.Layers.Count; i++)
            {
                var layer = StateMachine.Layers[i];
                if (layer is OxygenLayer && !layer.FloatsUp)
                {
                    return layer;
                }
            }

            return null;
        }

        public override void HandleWithCondition(IWeatherCondition condition)
        {
            condition.HandleLayer(this);
        }
    }

    public sealed record CO2Layer : AtmosphereLayer
    {
        public CO2Layer(double thickness) : base(thickness) { }

        public override AtmosphereLayer? FindSuitableAfter(int index)
        {
            for (int i = index + 1; i < StateMachine.Layers.Count; i++)
            {
                var layer = StateMachine.Layers[i];
                if (layer is CO2Layer && !layer.FloatsUp)
                {
                    return layer;
                }
            }

            return null;
        }

        public override void HandleWithCondition(IWeatherCondition condition)
        {
            condition.HandleLayer(this);
        }
    }

    public interface IWeatherCondition
    {
        public void HandleLayer(OzoneLayer layer);
        public void HandleLayer(OxygenLayer layer);
        public void HandleLayer(CO2Layer layer);
    }

    public sealed class StormyCondition : IWeatherCondition
    {
        public void HandleLayer(OzoneLayer layer)
        {
            layer.CreatedLayer = null;
        }

        public void HandleLayer(OxygenLayer layer)
        {
            double removed = layer.Thickness * 0.5;
            layer.Thickness -= removed;
            layer.CreatedLayer = new OzoneLayer(removed);
        }

        public void HandleLayer(CO2Layer layer)
        {
            layer.CreatedLayer = null;
        }
    }

    public sealed class SunnyCondition : IWeatherCondition
    {
        public void HandleLayer(OzoneLayer layer)
        {
            layer.CreatedLayer = null;
        }

        public void HandleLayer(OxygenLayer layer)
        {
            double removed = layer.Thickness * 0.05;
            layer.Thickness -= removed;
            layer.CreatedLayer = new OzoneLayer(removed);
        }

        public void HandleLayer(CO2Layer layer)
        {
            double removed = layer.Thickness * 0.05;
            layer.Thickness -= removed;
            layer.CreatedLayer = new OxygenLayer(removed);
        }
    }

    public sealed class OtherCondition : IWeatherCondition
    {
        public void HandleLayer(OzoneLayer layer)
        {
            double removed = layer.Thickness * 0.05;
            layer.Thickness -= removed;
            layer.CreatedLayer = new OxygenLayer(removed);
        }

        public void HandleLayer(OxygenLayer layer)
        {
            double removed = layer.Thickness * 0.15;
            layer.Thickness -= removed;
            layer.CreatedLayer = new CO2Layer(removed);
        }

        public void HandleLayer(CO2Layer layer)
        {
            layer.CreatedLayer = null;
        }
    }

    static void Main(string[] args)
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

        if (args.Length != 1)
        {
            throw new Exception($"No filename provided on {args}");
        }

        try
        {
            var filename = args[0];
            var contents = File.ReadAllLines(filename);

            if(contents == null || contents.Length < 2)
            {
                throw new Exception($"Bad file {contents}");
            }

            var conditions_s = contents[0];
            if (conditions_s.Count() == 0)
            {
                throw new Exception($"No conditions");
            }

            var conditions = new List<IWeatherCondition>();
            foreach (var c in conditions_s)
            {
                IWeatherCondition condition;
                switch (c)
                {
                    case 'm': condition = new OtherCondition(); break;
                    case 'n': condition = new SunnyCondition(); break;
                    case 'z': condition = new StormyCondition(); break;
                    default: throw new Exception($"Unknown condition {c}");
                }

                conditions.Add(condition);
            }

            var layers = new List<AtmosphereLayer>();
            for (var i = 1; i < contents.Length; i++)
            {
                var line = contents[i];
                var tokens = line.Split(" ");
                if (tokens.Length != 2)
                {
                    throw new Exception($"Bad line {line}");
                }

                var character = tokens[0];
                if (character.Length != 1)
                {
                    throw new Exception($"Bad identifier {character}");
                }

                double thickness;
                if (!double.TryParse(tokens[1], out thickness))
                {
                    throw new Exception($"Bad thickness {tokens[1]}, {line}");
                }

                AtmosphereLayer layer;
                switch (character[0])
                {
                    case 'z': layer = new OzoneLayer(thickness); break;
                    case 'x': layer = new OxygenLayer(thickness); break;
                    case 's': layer = new CO2Layer(thickness); break;
                    default: throw new Exception($"Unknown layer {character[0]}");
                }

                layers.Add(layer);
            }

            StateMachine.Init(conditions, layers);
            StateMachine.Start();
        }
        catch (Exception e)
        {
            Console.WriteLine($"An exception occourred: {e.Message}");
        }
    }
}

