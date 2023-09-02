using System.Numerics;
using System.Globalization;

class Program
{   
    static void Main(string[] args)
    {
        // Set up the log file
        ErrorLogger errorLogger = new ErrorLogger("log.txt");

        // Flags that are checked for in slicer gcode
        string[] slicerInfillFlags = { ";TYPE:Solid infill", ";TYPE:Top solid infill", "; FEATURE: Top surface", "; FEATURE: Internal solid infill", "; FEATURE: Bottom surface", ";TYPE:Internal solid infill",";TYPE:Top surface", ";TYPE:Bottom surface"};
        string[] slicerGenericFlags = { ";TYPE:" , "; FEATURE:"};

        // Forces script to interpert , and . as thousands and decimal respectively
        System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

        // Initiate objects
        FlowMaths flowMaths = new();
        Program program = new Program();

        // Load variable arguments passed to the script
        for(int i = 0; i < args.Length - 1; i++)
        {
            try
            {
                if(args[i][0] == 'L')  
                {
                    int userMaxModifiedLength = Convert.ToInt32(args[i].Substring(1));

                    // Check userMaxModifiedLength isn't negative
                    if(userMaxModifiedLength < 0)
                        errorLogger.AddToLog($"{args[i]} Is Below 0, Using Default Value");

                    // Warn if userMaxModifiedLength seems quite long
                    else if(userMaxModifiedLength > 20)
                    {
                        flowMaths.maxModifiedLength = Convert.ToInt32(args[i].Substring(1));
                        errorLogger.AddToLog($"{args[i]} Seems Very High, Are You Sure This Is Correct?");
                    }

                    else
                        flowMaths.maxModifiedLength = Convert.ToInt32(args[i].Substring(1));    
                }

                else if(args[i][0] == 'F')  
                {
                    int userMinFlowPercent = Convert.ToInt32(args[i].Substring(1));

                    // Check userMinFlowPercent is a percentage (between 0 and 100 inclusive)
                    if (userMinFlowPercent < 0 || userMinFlowPercent > 100)
                        errorLogger.AddToLog($"{args[i]} Is Out Of Range For minFlowPercent, Please Use A Value Between 0 and 100, Inclusive, Using Default Value For MinFlowPercent");

                    else
                        flowMaths.minFlowPercent = MathF.Round((float)userMinFlowPercent / 100f, 2);
                }
                
                else if(args[i][0] == 'D')
                {
                    Int32 userFlowDropOff = Convert.ToInt32(args[i].Substring(1));

                    // Check userFlowDropOff is a multiple of 2
                    if(userFlowDropOff % 2 != 0)   
                        errorLogger.AddToLog($"{args[i]} Needs To Be A Multiple Of 2, Using Default Value");

                    // Check userFlowDropOff is a positive multiple of 2
                    else if(userFlowDropOff < 2)   
                        errorLogger.AddToLog($"{args[i]} Needs To Be A Positive Multiple Of 2, Using Default Value");
                    
                    else
                        flowMaths.flowDropOff = userFlowDropOff;
                }
            }
            catch(FormatException)
            {
                errorLogger.AddToLog($"{args[i]} Parameter Not In Correct format, Using Default Value!");
            }
        }


        // Get gcode file path from arguments
        string slicerGcodeFilePath = args[^1];
        List<string> gcodeLines = new();

        // Load gcode file
        try
        {
           gcodeLines = File.ReadAllLines(slicerGcodeFilePath).ToList();
        }
        catch (FileNotFoundException)
        {
            errorLogger.AddToLog("Script unable to find gcode file");
            errorLogger.AddToLog($"Tried searching for file at: {slicerGcodeFilePath}");
        }

        // Check script loads gcode as expected
        bool alreadyParsed = false;

        if (gcodeLines.Count() < 1)
            errorLogger.AddToLog("Loaded Gcode file appears to be empty"); 

        else if(gcodeLines[0].Contains("; File Parsed By Flow Comp Script"))
        {
            errorLogger.AddToLog("Script Already Parsed By Flow Comp Script");
            alreadyParsed = true;
        }

        bool adjustingFlow = false;
        Vector2 previousToolPos = new(0, 0);
        
        if(!alreadyParsed)
        {
            // Loop through every line of gcode
            for (int index = 0; index < gcodeLines.Count(); index++)
            {
                // Update current tool position
                Vector2 currentToolPos = flowMaths.UpdateToolPos(gcodeLines[index], previousToolPos);

                // Check if it's reading infill gcode that needs modified
                if (slicerInfillFlags.Contains(gcodeLines[index]))
                    adjustingFlow = true;

                else if (adjustingFlow)
                {
                    foreach (string genericFlag in slicerGenericFlags)
                    {
                        if (gcodeLines[index].Contains(genericFlag))
                            adjustingFlow = false;
                    }
                }


                // If it's set to adjusting the flow
                if (adjustingFlow)
                {
                    double oldFlowVal = -1f;
                    double newFlowVal = -1f;

                    string[] gcodeLineSegments = gcodeLines[index].Trim().Split(' ');

                    // Loop through each segment of gcode
                    for (int i = 0; i < gcodeLineSegments.Count(); i++)
                    {
                        // Check for padding spaces
                        if(gcodeLineSegments[i].Length > 0)
                        {
                            // Check if the segment begins with E (extrusion gcode)
                            if (gcodeLineSegments[i][0] == 'E')
                            {
                                // Converts whatever is after the E (and if it fails, catches the error)
                                try
                                {
                                    if(gcodeLineSegments[i][1] != '-')
                                        oldFlowVal = Convert.ToDouble('0' + gcodeLineSegments[i].Substring(1));
                                }
                                catch (Exception FormatException)
                                {
                                    errorLogger.AddToLog($"Soft Error: {FormatException.Message}");
                                }

                                // Check if E value isn't a deretraction or wipe
                                if (oldFlowVal > 0f)
                                {
                                    newFlowVal = flowMaths.ModifyFlow(flowMaths.CalcExtrusionLength(currentToolPos, previousToolPos), oldFlowVal);
                                    gcodeLineSegments[i] = "E" + newFlowVal.ToString("N5");
                                }
                            }
                        }
                    }

                    // Modify E value if it's been changed (doesn't modify retraction stuff)
                    if (oldFlowVal > 0 && oldFlowVal != newFlowVal)
                        gcodeLines[index] = String.Join(' ', gcodeLineSegments) + "; Old Flow Value: " + oldFlowVal + " tool at: X" + currentToolPos.X + " Y" + currentToolPos.Y + " was at: X" + previousToolPos.X + " Y" + previousToolPos.Y;
                }

                // Update previous tool position
                previousToolPos = currentToolPos;
            }

            // Rewrite all the gcode back to the slicer
            gcodeLines.Insert(0, program.scriptGcodeHeader(flowMaths));
            File.WriteAllLines(slicerGcodeFilePath, gcodeLines);
            errorLogger.AddToLog("File Parsed Successfully (I Hope)");
            Console.WriteLine("Terminal Should Close In 5 Seconds");
            Thread.Sleep(5000); 
        }
    }

    // Create header for top of gcode file
    public string scriptGcodeHeader(FlowMaths flowMaths)
    {
        string header = "; File Parsed By Flow Comp Script\n; Script Ver. V0.5.1\n; Flow Model Ver. V0.1.1\n; Logger Ver. V0.0.1";
        Console.WriteLine(header + flowMaths.ReturnFlowModelParameters());
        return header + flowMaths.ReturnFlowModelParameters();
    } 
}

class ErrorLogger
{
    // ErrorLogger variables
    private string scriptDirectory, logFilePath;

    // Constructor
    public ErrorLogger(string logFileName)
    {
        scriptDirectory = AppDomain.CurrentDomain.BaseDirectory;
        logFilePath = Path.Combine(scriptDirectory, logFileName);

        // Try to create or open the log file for writing
        try
        {
            using (StreamWriter writer = File.AppendText(logFilePath))
            {
                // Write a message to the log file
                writer.WriteLine($"Log file accessed at: {DateTime.Now}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating or writing to the log file: {ex.Message}");
        }
    }

    public void AddToLog(string msg)
    {
        try
        {
            // Create or open the log file for appending
            using (StreamWriter writer = File.AppendText(logFilePath))
            {
                // Write the log entry to the log file
                writer.WriteLine($"{DateTime.Now}: {msg}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error appending to the log file: {ex.Message}");
        }
    }
}


// Class to handle all the flow maths etc.
class FlowMaths
{
    // Length at which extrusion modifer is no longer applied (argument e.g. L17)
    public int maxModifiedLength = 17;
    
    // Min percentage flow is reduced to (argument e.g. F30)
    public double minFlowPercent = 0.3f; 

    // How exponential the flow drop off is (multiple of 2) (argument e.g. D12)
    public int flowDropOff = 12;

    // Retursn the parameters used by the flow model
    public string ReturnFlowModelParameters()
    {
        string msg = $"\n; MaxModifiedLength: {maxModifiedLength}\n; MinFlowPercent: " + minFlowPercent.ToString("N2") + $"\n; FlowDropOff: {flowDropOff}\n";
        return msg;
    }

    // The maths doing the flow compensation
    private double flowCompModel(double extrusionLength)
    {
        if(extrusionLength == 0 || extrusionLength > maxModifiedLength)
            return 1;

        double magicNumber = (minFlowPercent-1) * Math.Pow(maxModifiedLength, -1 * flowDropOff);
        return magicNumber * Math.Pow(extrusionLength-maxModifiedLength, flowDropOff) + 1;
    }

    // Applies flow compensation model
    public double ModifyFlow(double extrusionLength, double eValue)
    {
        return Math.Round(eValue * flowCompModel(extrusionLength), 5);
    }

    // Returns double value of distance between two Vector2's (endPos, startPos)
    public double CalcExtrusionLength(Vector2 endPos, Vector2 startPos)
    {
        return Vector2.Distance(endPos, startPos);
    }

    // Create a 2D Vector for toolhead XY position from GCode Line
    // If gcodeLine doesn't contain an X or Y coord, it's flagged as -999
    private Vector2 vectorPos(string gcodeLine)
    {
        float xPos = -999, yPos = -999;

        // Check gcode line isn't a blank line
        if (gcodeLine.Length != 0)
        {
            // Check the gcode line isn't just a comment
            if (gcodeLine[0] != ';')
            {
                string[] gcodeSegments = gcodeLine.Trim().Split(' ');

                foreach (string segment in gcodeSegments)
                {
                    // Check segment isn't blank
                    if(segment.Length != 0)
                    {
                        if (segment[0] == 'X' || segment[0] == 'x')
                            xPos = (float)Convert.ToDouble(segment.Substring(1));

                        else if (segment[0] == 'Y' || segment[0] == 'y')
                            yPos = (float)Convert.ToDouble(segment.Substring(1));
                    }
                }
            }
        }

        return new(xPos, yPos);
    }

    // Creates a 2D Vector for toolhead's new XY position from a GCode Line
    public Vector2 UpdateToolPos(string gcodeLine, Vector2 previousToolPos)
    {
        Vector2 toolPos = vectorPos(gcodeLine);

        if (toolPos.X == -999)
            toolPos.X = previousToolPos.X;

        if (toolPos.Y == -999)
            toolPos.Y = previousToolPos.Y;

        return toolPos;
    }
}