// Error Codes:                                 //
// 1 - Fed GCode Already Parsed                 //
// 2 - Unable to make/edit temp gcode file      //
// 3 - Wrong format E Lengths in model.txt      //
// 4 - Wrong format Flow Comp in model.txt      //
// 5 - Issue loading model.txt file             //
// 6 - Issue with line parts count in model.txt //
// 7 - First/Last flow model values incorrect   //
// 8 - Not enough flow model points (min. 3)    //
// 9 - Flow comp applied to negative number     //
// 10 - Issue creating model.txt file           //
//                                              //
// 999 - Check Log, Unknown Error               //

using System.Numerics;
using System.Text.RegularExpressions;
using MathNet.Numerics.Interpolation;

partial class Program
{   
    // Version Variables
    string FlowCompScriptVer = "V0.7.1";
    string FlowModelVer = "V0.2.1";
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
        Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

        // Make sure the script is aware of its current directory
        Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

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
        Regex gcodeLineOfInterest = MyRegex1(); // Contains either XYZ followed by number
        Regex extrusionMovePattern = MyRegex(); // E Followed by a non negative decimal

        // Initiate FlowMaths and copy of Program
        string? model = Array.Find(args, s => s.EndsWith(".txt"));
        FlowMaths flowMaths = new(errorLogger, model != null ? model: "model.txt");
        Program program = new();

        // Get gcode file path from arguments and create temp gcode file
        string slicerGcodeFilePath = args[^1];
        string tempGCodeFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tempScriptGCode.gcode");

        // Assume initial tool position is at 0,0 (sould be updated before any extrusion moves anyway)
        Vector2 previousToolPos = new(0, 0);
        bool adjustingFlow = false;
        double extrusionLength = -1f;
        
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
                        for (int i = 0; i < gcodeLineSegments.Length; i++)
                        {
                            // Check if the segment is an extrusion move we want to modify
                            if (extrusionMovePattern.IsMatch(gcodeLineSegments[i]))
                            {
                                // Try convert e value and update newFlowVal and oldFlowVal
                                if(double.TryParse(gcodeLineSegments[i][1..], out oldFlowVal))
                                {
                                    extrusionLength = flowMaths.CalcExtrusionLength(currentToolPos, previousToolPos);

                                    if(extrusionLength < flowMaths.maxModifiedLength() && extrusionLength > 0)
                                    {
                                        newFlowVal = flowMaths.ModifyFlow(extrusionLength, oldFlowVal);
                                        gcodeLineSegments[i] = "E" + newFlowVal.ToString("N5");
                                    }
                                }
                                else
                                    errorLogger.AddToLog($"Unable to convert {gcodeLineSegments[i][1..]} to double");
                            }
                        }

                        // Modify E value if it's been changed (doesn't modify retraction stuff)
                        if (oldFlowVal > 0 && oldFlowVal != newFlowVal)
                            GCodeLine = string.Join(' ', gcodeLineSegments) + $"; Old Flow Value: {oldFlowVal}   Length: {extrusionLength:N5}";
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

    [GeneratedRegex("^E(?:0\\.\\d+|\\.\\d+|[1-9]\\d*|\\d+\\.\\d+)$")]
    private static partial Regex MyRegex();
    [GeneratedRegex("[XYE][\\d]+(?:\\.\\d+)?")]
    private static partial Regex MyRegex1();
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
            File.AppendAllText(logFilePath, $"\n{DateTime.Now}: {msg}");
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
    private List<double> eLengths = new(), flowComps = new();
    private CubicSpline flowModel;
    private string[] defaultModel = {"0, 0",
                                     "0.2, 0.4444",
                                     "0.4, 0.6145",
                                     "0.6, 0.7059",
                                     "0.8, 0.7619",
                                     "1.5, 0.8571",
                                     "2, 0.8889",
                                     "3, 0.9231",
                                     "5, 0.9520",
                                     "10, 1"};

    // Constructor for flow maths
    public FlowMaths(ErrorLogger _errorLogger, string modelName)
    {
        // Assign errorLogger for later user by flow maths object
        errorLogger = _errorLogger;

        string eLengthPattern = @"^\d+(\.\d+)?$";
        string flowCompPattern = @"^(0(\.\d+)?|1(\.0+)?)$";

        string modelParametersPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, modelName);
        errorLogger.AddToLog($"Loading model {modelName}");

        try
        {
            // Create a model.txt file if it doesn't exist
            if(!File.Exists(modelParametersPath))
                createModel(modelParametersPath);

            using (StreamReader reader = new(modelParametersPath))
            {
                string? modelParameterLine;
                while ((modelParameterLine = reader.ReadLine()) != null)
                {
                    errorLogger.AddToLog(modelParameterLine);
                    string[] lineParts = modelParameterLine.Trim().Split(',');

                    // Check line has right amount of parts
                    if(lineParts.Length > 2 || lineParts.Length == 0)
                    {
                        errorLogger.AddToLog($"Incorrect format of parameter line in {modelName}");
                        Environment.Exit(6);
                    }

                    if(Regex.IsMatch(lineParts[0].Trim(), eLengthPattern))
                        eLengths.Add(Convert.ToDouble(lineParts[0].Trim()));
                    else
                    {
                        errorLogger.AddToLog($"Incorrect format of eLength in {modelName}: {lineParts[0].Trim()}");
                        Environment.Exit(3);
                    }

                    if(Regex.IsMatch(lineParts[1].Trim(), flowCompPattern))
                        flowComps.Add(Convert.ToDouble(lineParts[1].Trim()));
                    else
                    {
                        errorLogger.AddToLog($"Incorrect format of flowComp in {modelName}: {lineParts[1].Trim()}");
                        Environment.Exit(4);
                    }                 
                }
            }

            if(eLengths[0] != 0.0 || flowComps[^1] != 1.0f)
            {
                errorLogger.AddToLog("First E length must be 0.0 and last flowComp must be 1.0");
                Environment.Exit(7);
            }

            if(eLengths.Count() < 3)
            {
                errorLogger.AddToLog("Please specifiy atleast 3 flow model points");
                Environment.Exit(8);
            }

        }
        
        catch(Exception ex){
            errorLogger.AddToLog($"An error occured: {ex.Message}");
            Environment.Exit(999);
        }

        flowModel = CubicSpline.InterpolateNatural(eLengths, flowComps);
    }

    // Creates a default model.txt file
    private void createModel(string directory)
    {
        string modelName = Path.GetFileName(directory);
        try
        {
            errorLogger.AddToLog($"Creating {modelName} file (as one doesn't exist)");
            using (StreamWriter writer = File.AppendText(directory))
            {
                foreach(string modelPoint in defaultModel)
                {
                    writer.WriteLine(modelPoint);
                }
            }
            errorLogger.AddToLog($"Succesfully created {modelName} file");
        }
        catch (Exception ex)
        {
            errorLogger.AddToLog($"Error with creating {modelName} file: {ex.Message}");
            Environment.Exit(10);
        }
    }

    // Getter for longest length in eLengths
    public double maxModifiedLength()
    {
        return eLengths[^1];
    }

    // Returns the parameters used by the flow model
    public string ReturnFlowModelParameters()
    {
        string msg = "\n; Flow Comp Model Points:";
        for(int index = 0; index < eLengths.Count(); index++)
        {
            msg += $"\n; ({eLengths[index]}, {flowComps[index]})";
        }
        return msg + "\n";
    }

    // Returns flow multiplier value from flow model
    private double flowCompModel(double extrusionLength)
    {
        if(extrusionLength < 0.0)
        {
            errorLogger.AddToLog("Tried to apply flow comp to extrusion length < 0");
            return 1;
        }

        if(extrusionLength > eLengths[^1])
        {
            errorLogger.AddToLog($"Tried to apply flow comp to extrusion length > max flow comp length: {eLengths[^1]}");
            return 1;
        }

        return flowModel.Interpolate(extrusionLength);
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