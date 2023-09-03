// Error Codes:                            //
// 1 - Fed GCode Already Parsed            //
// 2 - Unable to make/edit temp gcode file //
//                                         //
// 999 - Check Log, Unknown Error          //

using System.Numerics;
using System.Globalization;
using System.Text.RegularExpressions;

class Program
{   
    // Version Variables
    string FlowCompScriptVer = "V0.6.0";
    string FlowModelVer = "V0.1.1";
    string ErrorLoggerVer = "V0.0.1";

    // Create header for top of gcode file
    public string ScriptGcodeHeader(FlowMaths flowMaths)
    {
        string header = $"; File Parsed By Flow Comp Script\n; Script Ver. {FlowCompScriptVer}\n; Flow Model Ver. {FlowModelVer}\n; Logger Ver. {ErrorLoggerVer}";
        Console.WriteLine(header + flowMaths.ReturnFlowModelParameters());
        return header + flowMaths.ReturnFlowModelParameters();
    } 

    // Main bit of code
    static void Main(string[] args)
    {
        // Forces script to interpert , and . as thousands seperator and decimal respectively
        System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

        // Set up the log file
        ErrorLogger errorLogger = new ErrorLogger("log.txt");

        // Flags that are checked for in slicer gcode (for ares to modify)
        string[] slicerInfillFlags = { ";TYPE:Solid infill",
                                       ";TYPE:Top solid infill",
                                       ";TYPE:Internal solid infill",
                                       ";TYPE:Top surface", 
                                       ";TYPE:Bottom surface",
                                       "; FEATURE: Top surface",
                                       "; FEATURE: Internal solid infill",
                                       "; FEATURE: Bottom surface"};

        // Flags that are checked for change in extrusion type
        string[] slicerGenericFlags = { ";TYPE:" , "; FEATURE:"};

        // Regex pattern to check if gcode lines and segments
        Regex gcodeLineOfInterest = new Regex(@"[XYE][\d]+(?:\.\d+)?"); // Contains either XYZ followed by number
        Regex extrusionMovePattern = new Regex(@"^E(?:0\.\d+|\.\d+|[1-9]\d*|\d+\.\d+)$");

        // Initiate FlowMaths and copy of Program
        FlowMaths flowMaths = new(errorLogger);
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

        // Get gcode file path from arguments and create temp gcode file
        string slicerGcodeFilePath = args[^1];
        string tempGCodeFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tempScriptGCode.gcode");

        // Assume initial tool position is at 0,0 (sould be updated before any extrusion moves anyway)
        Vector2 previousToolPos = new(0, 0);
        bool adjustingFlow = false;
        
        // Start Reading GCode File
        try{
            using (StreamReader reader = new(slicerGcodeFilePath))
            using (StreamWriter writer = new(tempGCodeFilePath))
            {
                // Add header to temp gcode file
                writer.WriteLine(program.ScriptGcodeHeader(flowMaths));

                string? GCodeLine;
                while ((GCodeLine = reader.ReadLine()) != null)
                {
                    // Check if gcode file has alread been parsed
                    if(GCodeLine.Trim() == "; File Parsed By Flow Comp Script")
                        Environment.Exit(1);

                    // Check if reading gcode that needs flow comp (see flags above)
                    if (slicerInfillFlags.Contains(GCodeLine))
                        adjustingFlow = true;

                    else if (adjustingFlow)
                    {
                        foreach (string genericFlag in slicerGenericFlags)
                        {
                            if (GCodeLine.Contains(genericFlag))
                                adjustingFlow = false;
                        }
                    }

                    // Update current tool position
                    Vector2 currentToolPos = flowMaths.UpdateToolPos(GCodeLine, previousToolPos);

                    if (adjustingFlow && gcodeLineOfInterest.IsMatch(GCodeLine))
                    {
                        double oldFlowVal = -1f;
                        double newFlowVal = -1f;

                        // Break GCodeLine Into It's Segments
                        string[] gcodeLineSegments = GCodeLine.Trim().Split(' ');

                        // Loop through each segment of gcode
                        for (int i = 0; i < gcodeLineSegments.Count(); i++)
                        {
                            // Check if the segment is an extrusion move we want to modify
                            if (extrusionMovePattern.IsMatch(gcodeLineSegments[i]))
                            {
                                // Try convert e value and update newFlowVal and oldFlowVal
                                if(double.TryParse(gcodeLineSegments[i].Substring(1), out oldFlowVal))
                                {
                                    newFlowVal = flowMaths.ModifyFlow(flowMaths.CalcExtrusionLength(currentToolPos, previousToolPos), oldFlowVal);
                                    gcodeLineSegments[i] = "E" + newFlowVal.ToString("N5");
                                }
                                else
                                    errorLogger.AddToLog($"Unable to convert {gcodeLineSegments[i].Substring(1)} to double");
                            }
                        }

                        // Modify E value if it's been changed (doesn't modify retraction stuff)
                        if (oldFlowVal > 0 && oldFlowVal != newFlowVal)
                        {
                            //GCodeLine = $"\n; Old Flow Value: {oldFlowVal}\n; Tool at: X{currentToolPos.X} Y{currentToolPos.Y}\n; Was at: X{previousToolPos.X} Y{previousToolPos.Y}\n\n" + String.Join(' ', gcodeLineSegments) +"\n";
                            GCodeLine = String.Join(' ', gcodeLineSegments) + $"; Old Flow Value: {oldFlowVal}";
                        }
                    }
                    previousToolPos = currentToolPos;

                    // Write GCode Line (modifed or not) to temp gcode file                    
                    writer.WriteLine(GCodeLine);
                }
            }
            File.Copy(tempGCodeFilePath, slicerGcodeFilePath, true);
        }

        catch (Exception ex)
        {
            errorLogger.AddToLog($"An error occurred: {ex.Message}");
            Environment.Exit(999);
        }

        finally
        {
            errorLogger.AddToLog("Deleting Temp Gcode File");
            File.Delete(tempGCodeFilePath);
        }
    }
}

class ErrorLogger
{
    // ErrorLogger variables
    private readonly string scriptDirectory, logFilePath;

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
            // Add text to the log file at logFilePath
            File.AppendAllText(logFilePath, $"{DateTime.Now}: {msg}");
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
    private ErrorLogger errorLogger;
    public FlowMaths(ErrorLogger _errorLogger)
    {
        errorLogger = _errorLogger;
    }

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

                // Go through each segment of the gcode line
                foreach (string segment in gcodeSegments)
                {
                    try{
                        // Check segment isn't blank
                        if(segment.Length != 0)
                        {
                            // Try update X coordinate
                            if (segment[0] == 'X' || segment[0] == 'x')
                                xPos = (float)Convert.ToDouble(segment.Substring(1));

                            // Try update Y coordinate
                            else if (segment[0] == 'Y' || segment[0] == 'y')
                                yPos = (float)Convert.ToDouble(segment.Substring(1));
                            
                            // If it finds a comment, stops reading the segments
                            else if(segment[0] == ';')
                                break;
                        }
                    }

                    catch(Exception ex){
                        errorLogger.AddToLog($"Tried converting {segment.Substring(1)} to double but got error: {ex.Message}");
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