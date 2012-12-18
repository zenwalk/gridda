using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;

namespace GRIDDA
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                PrintHelpCommand();
                Console.WriteLine("Press any key to continue.....");
                Console.ReadKey();

                return;
            }
            else
            {
                Dictionary<String, String> arguments = InterpretCommandLineArguments(args);

                foreach (KeyValuePair<String, String> kvp in arguments)
                {
                    //Console.WriteLine("Found " + kvp.Key + " / " + kvp.Value);
                }

                if (arguments.ContainsKey("HELP") || arguments.ContainsKey("H"))
                {
                    PrintHelpCommand();
                    Console.WriteLine("Press any key to exit.....");
                    Console.ReadKey();
                    return;
                }
                else if (arguments.ContainsKey("GUI"))
                {
                    GUI gui = new GUI();
                    gui.ShowDialog();
                }
                // Both
                else if (arguments.ContainsKey("ALL"))
                {
                    if (arguments.ContainsKey("SHAPEFILE") && arguments.ContainsKey("GRID") && arguments.ContainsKey("OUTDIR") && arguments.ContainsKey("DATADIR") && arguments.ContainsKey("TIMEUNIT"))
                    {
                        // Check output directory exists or can be created
                        if (!Directory.Exists(arguments["OUTDIR"]))
                        {
                            try
                            {
                                Directory.CreateDirectory(arguments["OUTDIR"]);
                            }
                            catch (Exception)
                            {
                                throw new Exception("Failure creating output directory ( " + arguments["OUTDIR"] + " )");
                            }
                        }

                        // Set Grid
                        GriddedDataDetails gridDetails = new GriddedDataDetails();
                        if (arguments.ContainsKey("GRID"))
                        {
                            try
                            {
                                gridDetails = InterpretGridString(arguments["GRID"]);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Error");
                                Console.WriteLine(e.Message);
                                return;
                            }
                        }

                        // Create weight file
                        Delineator delineator = new Delineator(arguments["SHAPEFILE"], gridDetails, arguments["OUTDIR"]);
                        delineator.Delineate(arguments.ContainsKey("BOUNDARY"), arguments.ContainsKey("AREA"));

                        // Extract data for weight file
                        Extractor extractor = new Extractor(arguments["DATADIR"], delineator.getWeightFile(), gridDetails, (TimeUnit)Enum.Parse(typeof(TimeUnit), arguments["TIMEUNIT"]));
                        extractor.Extract(arguments["OUTDIR"]);
                        extractor.ProduceStatPlots(new Size(2048, 2048), arguments["OUTDIR"], arguments.ContainsKey("PLOTTIME"), arguments.ContainsKey("STATS"), arguments.ContainsKey("PLOTSTAT"));
                    }
                    else
                    {
                        Console.WriteLine("Mode all (-ALL) requires arguments for shapefile location (-SHAPEFILE), data directory (-DATADIR), data time unit (-TIMEUNIT), grid details (-GRID) and output directory (-OUTDIR)");
                        return;
                    }
                }
                // Just get grid cells
                else if (arguments.ContainsKey("DELINEATE"))
                {
                    if (arguments.ContainsKey("SHAPEFILE") && arguments.ContainsKey("GRID") && arguments.ContainsKey("OUTDIR"))
                    {

                        // Check output directory exists or can be created
                        if (!Directory.Exists(arguments["OUTDIR"]))
                        {
                            try
                            {
                                Directory.CreateDirectory(arguments["OUTDIR"]);
                            }
                            catch (Exception)
                            {
                                throw new Exception("Failure creating output directory ( " + arguments["OUTDIR"] + " )");
                            }
                        }

                        // Set Grid
                        GriddedDataDetails gridDetails = new GriddedDataDetails();
                        if (arguments.ContainsKey("GRID"))
                        {
                            try
                            {
                                gridDetails = InterpretGridString(arguments["GRID"]);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Error");
                                Console.WriteLine(e.Message);
                                return;
                            }
                        }

                        // Create weight file
                        Delineator delineator = new Delineator(arguments["SHAPEFILE"], gridDetails, arguments["OUTDIR"]);
                        delineator.Delineate(arguments.ContainsKey("BOUNDARY"), arguments.ContainsKey("AREA"));
                    }
                    else
                    {
                        Console.WriteLine("Mode cell calculation (-DELINEATE) requires arguments for shapefile location (-SHAPEFILE), grid details (-GRID) and output weight file name (-WEIGHTFILE)");
                        return;
                    }
                }
                // Extract data
                else if (arguments.ContainsKey("EXTRACT"))
                {
                    if (arguments.ContainsKey("WEIGHTFILE") && arguments.ContainsKey("DATADIR") && arguments.ContainsKey("TIMEUNIT") && arguments.ContainsKey("OUTDIR"))
                    {

                        // Check output directory exists or can be created
                        if (!Directory.Exists(arguments["OUTDIR"]))
                        {
                            try
                            {
                                Directory.CreateDirectory(arguments["OUTDIR"]);
                            }
                            catch (Exception)
                            {
                                throw new Exception("Failure creating output directory ( " + arguments["OUTDIR"] + " )");
                            }
                        }

                        // Set Grid
                        GriddedDataDetails gridDetails = new GriddedDataDetails();
                        if (arguments.ContainsKey("GRID"))
                        {
                            try
                            {
                                gridDetails = InterpretGridString(arguments["GRID"]);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Error");
                                Console.WriteLine(e.Message);
                                return;
                            }
                        }

                        // Extract data for weight file
                        Extractor extractor = new Extractor(arguments["DATADIR"], arguments["WEIGHTFILE"], gridDetails, (TimeUnit)Enum.Parse(typeof(TimeUnit), arguments["TIMEUNIT"]));
                        extractor.Extract(arguments["OUTDIR"]);
                        extractor.ProduceStatPlots(new Size(2048, 2048), arguments["OUTDIR"], arguments.ContainsKey("PLOTTIME"), arguments.ContainsKey("STATS"), arguments.ContainsKey("PLOTSTAT"));
                    }
                    else
                    {
                        Console.WriteLine("Mode cell calculation (-EXTRACT) requires arguments for output weight file name (-WEIGHTFILE), data directory (-DATADIR), data time unit (-TIMEUNIT), grid details (-GRID) and output directory (-OUTDIR)");
                        return;
                    }
                }
                else
                {
                    Console.WriteLine("Incorrect arguments");
                    PrintHelpCommand();
                }

            }                
        }

        static GriddedDataDetails InterpretGridString(String gridString)
        {
            String[] tokens = gridString.Split(',');

            if (tokens.Length == 6)
            {
                decimal lat = decimal.Parse(tokens[0]);
                decimal lon = decimal.Parse(tokens[1]);
                decimal xRes = decimal.Parse(tokens[2]);
                decimal yRes = decimal.Parse(tokens[3]);
                int cols = int.Parse(tokens[4]);
                int rows = int.Parse(tokens[5]);

                return new GriddedDataDetails(lat, lon, xRes, yRes, cols, rows);
            }
            else
            {
                throw new Exception("Grid string incorrect format.\n\tRequired: lat,lon,xRes,yRes,cols,rows.\n\tExample: 111.975, -44.025, 0.05, 0.05, 861, 681");
            }
        }

        static GriddedDataDetails SelectAWAPGrid()
        {
            return new GriddedDataDetails(111.975m, -44.025m, 0.05m, 0.05m, 861, 681);
        }

        static GriddedDataDetails SelectAccessGrid()
        {
            return new GriddedDataDetails(146.531m, -42.1875m, 0.5625m, 0.375m, 640, 481);
        }

        static Dictionary<String, String> InterpretCommandLineArguments(String[] args)
        {
            Dictionary<String,String> dictionary = new Dictionary<String,String>();

            foreach (String s in args)
            {
                if (s[0] == '-')
                {
                    int equals = s.IndexOf('=');

                    if (equals > 0)
                    {
                        dictionary.Add(s.Substring(1, equals - 1).ToUpper(), s.Substring(equals + 1));
                    }
                    else
                    {
                        dictionary.Add(s.Substring(1).ToUpper(), s.Substring(1).ToUpper());
                    }

                }
                else
                {
                    Console.WriteLine("Ignoring >> " + s);
                }
            }

            return dictionary;
        }

        static void PrintHelpCommand()
        {
            Console.WriteLine("\n***GRIDDA: Gridded Data Aggregator***");
            Console.WriteLine("");

            Console.WriteLine("GRIDDA performs two key functions, shapefile delineation and gridded data extraction.");
            Console.WriteLine("");

            Console.WriteLine("Modes:");
            Console.WriteLine("-ALL                        Perform both delineation and extraction");
            Console.WriteLine("-DELINEATE                  Only perform shapefile delineation");
            Console.WriteLine("-EXTRACT                    Only perform data extraction");
            Console.WriteLine("-GUI                        Graphical user interface");
            Console.WriteLine("");

            Console.WriteLine("Arguments:");
            Console.WriteLine("ALL mode requires the combined arguments of -DELINEATE and -EXTRACT,");
            Console.WriteLine("excluding -WEIGHTFILE");
            Console.WriteLine("");
                
            Console.WriteLine("DELINEATE");
            Console.WriteLine("-SHAPEFILE=<path>       Path to shapefile");
            Console.WriteLine("-OUTDIR=<path>          Directory to place output");
            Console.WriteLine("-GRID=a,b,c,d,e,f       Where a and b are longitude and latitude");
            Console.WriteLine("                        of the lower left hand corner of the grid,");
            Console.WriteLine("                        c and d are cell width and height,");
            Console.WriteLine("                        e and f are number of columns and rows.");
            Console.WriteLine("");

            Console.WriteLine("EXTRACT");
            Console.WriteLine("-WEIGHTFILE=<path>      File containing cell weights");
            Console.WriteLine("-DATADIR=<path>         File containing gridded data");
            Console.WriteLine("-TIMEUNIT=<hr|da|mo>    Time unit of gridded data");
            Console.WriteLine("-OUTDIR=<path>          Directory to place output");
            Console.WriteLine("-GRID=a,b,c,d,e,f       Where a and b are longitude and latitude");
            Console.WriteLine("                        of the lower left hand corner of the grid,");
            Console.WriteLine("                        c and d are cell width and height,");
            Console.WriteLine("                        e and f are number of columns and rows.");
            Console.WriteLine("");

            Console.WriteLine("Optional Arguments");
            Console.WriteLine("-BOUNDARY           Produce images of catchment boundaries");
            Console.WriteLine("-AREA               Produce images of catchment area");
            Console.WriteLine("-STATS              Produce timeseries statistics");
            Console.WriteLine("-PLOTTIME           Produce timeseries plots");
            Console.WriteLine("-PLOTSTAT           Produce timeseries statistic plots");
            Console.WriteLine("");
        }
    }
}
