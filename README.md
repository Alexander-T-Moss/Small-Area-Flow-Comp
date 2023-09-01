# DISCLAIMER - PLEASE READ
**Use this script at your own risk**. It has been tested to a point where I feel it is safe for public testing, but your milage may vary. Until indicated on this readme otherwise, I strongly recommend **only printing test pieces** you don't mind if corrupt gcode causes it to fail! Flow maths is hard, and this is just a bodge for a larger issue with current slicer flow maths. With that disclaimer out the way, lets dive right in!



# Information On Beta Testing

For information on the tests you can conduct, please follow the link [here](link.here). For information on the script in general, keep reading below (*including how to set it up*)

![Screen Recording 2023-09-01 at 14.03.28](/Users/alexanderthorunnarson-moss/Documents/GitHub/Small-Area-Flow-Comp/Screenshots/Screen Recording 2023-09-01 at 14.03.28.gif)

The example above was using very extreme parameters to show the flow compensation, you will not see this extreme



# What Is The Goal Here?

Chances are, you've found that small areas of solid infill appear to be overextruded, despite the rest of a print looking like it has a well dialed in EM/Flow. It is suspected that the main culprit for this is the model slicers use to determine the cross-section of an extrusion line. 

So, to combat this, I have created a script to modify the flow of extrusion lines inversely proportional to the length of the extrusion line (*shorter the extrusion, the less flow it should have*). Does this work then? Well that's where you and the testing of this script comes in!

From preliminary testing, I have noticed some improvements in my small area solid infill, but this is a sample size of 1, me, so like all good scientific hypothesis testing, we need more samples to confirm whether of not this script actually works as expected!



# What Is The Script Actually Doing

This screenshot shows what is going on really well. The flow is visualized as the more blue, the less flow (and the more red, the more flow). And as you can see, where the infill lines get shorter, the lines get more blue, thus indicating they have less flow (and in theory shouldn't show signs of over-extrusion)

![img](https://cdn.discordapp.com/attachments/1120959178408726568/1146559708455510056/image.png)



# How Do I Use This Script

The first thing you need is the script for your relavent platform (`win`, `osx_intel`, `osx_arm`, `linux`), so download the one you need from the latest release. When saving it on your computer, the file path to the script **cannot have spaces in it**, so any folders it goes in cannot have spaces, then follow the steps for your relavent slicer below:



### PrusaSlicer/SuperSlicer

---

1. Navigate to `Post-processing scripts` via `Print Settings >> Output options` 
2. Paste the path to your script in the text field (*see screenshot below, file path will differ*)

![image-20230901093740545](https://github.com/Alexander-T-Moss/Small-Area-Flow-Comp/blob/main/Screenshots/image-20230901093740545.png)



### BambuStudio/OrcaSlicer

---

1. Navigate to `Post-processing scripts` via `Others >> Scroll down to the bottom` (*Advanced settings need to be on*) 
2. Paste the path to your script in the text field (*see screenshot below, file path will differ*)

![image-20230901093729295](https://github.com/Alexander-T-Moss/Small-Area-Flow-Comp/blob/main/Screenshots/image-20230901093729295.png)

### Don't See Your Slicer? 

---

Ping me (*alexandertmoss*) on discord and I'll see what can be done :)





# HELP! It's Not Working 

To help diagnose the issues quicker, please provide the information (*if possible*) below for your issue on the [channel in VORONDesign](link.to.channel). (*You'll get quicker responses there than if you submit an issue on GitHub*)



### Slicer Issue When Exporting G-Code?  

---

Slicers only run the script when exporting the gcode, not when it's slicing the file. If possible to help us diagnose the issue, please provide the following information:

1. A screenshot (*or ideally copy/paste of the error text in a code block*) of the error the slicer throws
2. If a terminal windows pops up, a copy of the error in that window (*won't always show up*)



### Issue With G-Code When Printing?  

---

**The chances are the bug you've found has already been squashed, so check the release notes of any new versions beforehand!**

These issues include when a print fails or doesn't print as expected. If possible to help us diagnose the issue quicker, please provide the following information:

1. A copy of the gcode that has issues with it
2. A copy of the STL you sliced (*including mentioning what slicer you used*)
3. A photo of the print that failed (*if it didn't even start printing, then obviously we don't need a photo of your empty build plate*)
