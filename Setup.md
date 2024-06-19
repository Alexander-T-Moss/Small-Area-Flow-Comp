# Setting Up The Script
SAFC officialy supports the mainstream slicers derived from Slic3r, these being [PrusaSlicer](https://github.com/prusa3d/PrusaSlicer), [SuperSlicer](https://github.com/supermerill/SuperSlicer), [BambuStudio](https://github.com/bambulab/BambuStudio) and [OrcaSlicer](https://github.com/SoftFever/OrcaSlicer)

1. Download the [latest release](https://github.com/Alexander-T-Moss/Small-Area-Flow-Comp/releases) for your specific platform by clicking on the link in the `Assets` dropdown menu.

2. Once downloaded extract/uncompress the .zip file, then inside that folder, clicking through `artifacts` > `SmallAreaFlowComp-vX.X.X-platform` you will find the `SmallAreaFlowComp` application file, you will need to place this file in a folder where it will have permission to read and write to files in the same folder and read and write files that are where your slicer of choice is

> Unsure of what permissions the `SmallAreaFlowComp` file has?
> - On Windows: `Right Click` > `Properties` > `Security` then `Read`, `Write`, `Modify` and `Read & Execute` need to be set to **Allow**
> - On MacOS: Select `SmallAreaFlowComp` then `File` > `Get Info` and under `Sharing & Permissions` by your account, it should have `Read & Write` **Privilege**

3. Now the script is in a suitable location on your PC, in your slicer, you need to paste the path of the script into the post-process section, relavent information for each supported slicer is below:

### PrusaSlicer/SuperSlicer:
- Navigate to `Post-processing scripts` via `Print Settings` > `Output options`
- Paste the path to the script into the `Post-processing scripts` text field

![ps_post_process](/Screenshots/ps_post_process.png)

### BambuStudio/OrcaSlicer Pre V2.0.0 Beta:
- Navigate to `Post-processing scripts` via `Others` (*Advanced settings need to be on*)
- Paste the path to the script into the `Post-processing scripts` text field

![bambustudio_post_process](/Screenshots/bambustudio_post_process.png)

> Verify Everything Is Working As Expected:
> - Slice a model and export the file, if all is working correctly then a `model.txt` and `log.txt` file will be created in the same folder as the script (if they don't already exist)
> - You won't see any changes to the gcode unless you re-import the sliced G-code file back into your slicer, as the script is ran when exporting the sliced file initially

## Help! Somthing Is Not Right!
Got hung up at a step, or something isn't acting as expected? Headover to the [troubleshooting guide](https://github.com/Alexander-T-Moss/Small-Area-Flow-Comp/blob/main/Troubleshooting.md)
