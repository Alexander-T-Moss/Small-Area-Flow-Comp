# Troubleshooting Common Issues
Here you will hopefully find solutions for issues you are having with the SAFC script. Most issues can be catagorized into two groups which is the format this guide follows:

# Issue With Running The Script
Here are common issues that prevent the script from running

## Incorrect Path In The Slicer
This is often the case when your slicer throws an error similar to "Failed starting the script" when trying to export the gcode

> - Make sure on windows, you include the `.exe` extension for the `SmallAreaFlowComp.exe` file in the path
> - If the path contains spaces, then either place the script where this won't be the case, or enclose the path in quotation marks `"path/to/the script/SmallAreaFlowComp.exe"`
> - The script has not got the correct file permissions, refer to setup guide for what the correct permissions are

## OSX Arm 64 Artifact Runtime Crash
With MacOS, the script needs correct code signing, this issue was solved [here](https://github.com/Alexander-T-Moss/Small-Area-Flow-Comp/issues/7)
> Run `codesign --force --deep -s - ./SmallAreaFlowComp` in a terminal to give the script the correct signiture



# Issue With Prints From G-Code Processed By The Script
Issues where the script seems to be running, but is doing unexpected things with the gcode

## Excessive Volumetric Flow Values

If your printer starts acting strange or throwing random errors like `Move exceeds maximum extrusion`, import the gcode into your slicer and check if the volumetric flow values are stupidly high
> - Make sure you have relative extrusion enabled in your slicer

# Can't Find Your Specific Issue?
Reach out to me on Discord _@alexandertmoss_ or create an `Issues` ticket for this repo, if I'm awake I will aim to respond promptly. My timezone is often **UTC+0** or **UTC+1**
