---
typora-copy-images-to: ./Screenshots
---

# DISCLAIMER - PLEASE READ
**Use this script at your own risk**. It has been tested to a point where I feel it is safe for public testing, but your milage may vary. Until indicated on this readme otherwise, I strongly recommend **only printing test pieces** you don't mind if corrupt gcode causes it to fail! Flow maths is hard, and this is just a bodge for a larger issue with current slicer flow maths. With that disclaimer out the way, lets dive right in!



# What Is The Goal Here?

Chances are, you've found that small areas of solid infill appear to be overextruded, despite the rest of a print looking like it has a well dialed in EM/Flow. It is suspected that the main culprit for this is the model slicers use to determine the cross-section of an extrusion line. 

So, to combat this, I have created a script to modify the flow of extrusion lines inversely proportional to the length of the extrusion line (*shorter the extrusion, the less flow it should have*). Does this work then? Well that's where you and the testing of this script comes in!

From preliminary testing, I have noticed some improvements in my small area solid infill, but this is a sample size of 1, me, so like all good scientific hypothesis testing, we need more samples to confirm whether of not this script actually works as expected!



# How Do I Use This Script

The first thing you need is the script for your relavent platform (`win`, `osx_intel`, `osx_arm`, `linux`), so download the one you need from the latest release. When saving it on your computer, the file path to the script **cannot have spaces in it**, so any folders it goes in cannot have spaces, then follow the steps for your relavent slicer below:



### PrusaSlicer/SuperSlicer

---

1. Navigate to `Post-processing scripts` via `Print Settings >> Output options` 
2. Paste the path to your script in the text field (see screenshot below, file path will differ)

![image-20230901093740545](/Users/alexanderthorunnarson-moss/Documents/GitHub/Small-Area-Flow-Comp/Screenshots/image-20230901093740545.png)



### BambuStudio/OrcaSlicer

---

1. Navigate to `Post-processing scripts` via `Others >> Scroll down to the bottom` (*Advanced settings need to be on*) 
2. Paste the path to your script in the text field (see screenshot below, file path will differ)

![image-20230901093729295](/Users/alexanderthorunnarson-moss/Documents/GitHub/Small-Area-Flow-Comp/Screenshots/image-20230901093729295.png)
