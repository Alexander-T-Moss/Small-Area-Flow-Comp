Hey fellow 3DP Enthusiast who has fallen down the rabbit hole of tuning their 3D printer, I appreciate you spending time here helping me test some pretty cool stuff (if I do say so myself). I have spent way too long working on this, and all I ask in return is for 5 mins of your time to read through this documentation! Past me thanks you (and assumes you will read this page, let's not disappoint him) 

---

# DISCLAIMER - PLEASE READ
Use this script at your own risk. It has been tested to a point where I feel it is safe for public testing, but your mileage may vary. I personally recommend only printing test pieces you don't mind having print issues with! Not just those found here, but play around. Failure is how we learn, please fail in new and exciting ways for me.



# Cool Looking G-Code

![demo_cw1_body](/Screenshots/demo_cw1_body.png)

The example above used very extreme parameters to show the flow compensation, you will not see this extreme in your gcode (*the areas that are more blue have less flow*) This screenshot very clearly shows the script at work



# What Is The Goal Here?

Chances are, you've found that small areas of solid infill appear to be over-extruded, despite the rest of a print looking like it has a well-dialled-in EM/Flow, otherwise why would you be reading this? Currently, there isn't a good understanding of why this happens, so this is an attempt at a brute-force approach to treat the symptom.

I've created a script to modify the flow of extrusion lines inversely proportional to the length of the extrusion line (*shorter the extrusion, the less flow it should have*). Does this work then? Well, that's where you and the testing of this script comes in!

From preliminary testing, I have noticed some improvements in my small area solid infill, but this is a sample size of 1, me, so like all good scientific hypothesis testing, I need more samples to confirm whether or not this script actually works as expected!




# What Is The Script Actually Doing

This screenshot shows what is going on really well. The flow is visualized as the more blue, the less flow (and the more red, the more flow). As you can see, where the infill lines get shorter, the lines get bluer, thus indicating they have less flow (and in theory shouldn't show signs of over-extrusion)

![demo_square](/Screenshots/demo_square.png)



# How Do I Use This Script

The first thing you need is the script for your relevant platform (`win`, `osx_intel`, `osx_arm`, `linux`), so download the one you need from the latest release. When saving it on your computer, the file path to the script **cannot have spaces in it**, so any folders it goes in cannot have spaces, then follow the steps for your relevant slicer below:



## PrusaSlicer/SuperSlicer

1. Navigate to `Post-processing scripts` via `Print Settings >> Output options` 
2. Paste the path to your script in the text field (*see screenshot below, file path will differ*)

![ps_post_process](/Screenshots/ps_post_process.png)



## BambuStudio/OrcaSlicer

1. Navigate to `Post-processing scripts` via `Others >> Scroll down to the bottom` (*Advanced settings need to be on*) 
2. Paste the path to your script in the text field (*see screenshot below, file path will differ*)

![bambustudio_post_process](/Screenshots/bambustudio_post_process.png)

### Don't See Your Slicer? 

Ping me (*alexandertmoss*) on Discord and I'll see what can be done :)





# HELP! It's Not Working 

To help diagnose the issues quicker, please provide the information (*if possible*) below for your issue on the [channel in VORONDesign](link.to.channel). (*You'll get quicker responses there than if you submit an issue on GitHub*)



## Slicer Issue When Exporting G-Code?  

Slicers only run the script when exporting the gcode, not when it's slicing the file. If possible to help me diagnose the issue, please provide the following information:

1. A screenshot (*or ideally copy/paste of the error text in a code block*) of the error the slicer throws
2. If a terminal window pops up, a copy of the error in that window (*won't always show up*)



## Issue With G-Code When Printing?  

**The chances are the bug you've found has already been squashed, so check the release notes of any new versions beforehand!**

These issues include when a print fails or doesn't print as expected. If possible to help me diagnose the issue quicker, please provide the following information:

1. A copy of the gcode that has issues with it
2. A copy of the STL you sliced (*including mentioning what slicer you used*)
3. A photo of the print that failed (*if it didn't even start printing, then obviously I don't need a photo of your empty build plate*)



# Credits

**Weaslus**: Proof Reader, Hypeman & Pestered Me Into Making This

**Donut Man**: They Were Here Too (Also A Hypeman, Well I'm Not Sure If He's Old Enough To Be A Hypeman, Maybe A Hypeboy)

