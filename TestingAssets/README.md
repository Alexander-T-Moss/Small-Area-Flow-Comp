# Prerequisites

This script won't be a magic bullet to turn the worst of top layers into perfect top layers, so you will only really see improvements if your EM is well-tuned to begin with and your normal-sized top layers print well!

If you want some pointers for improving your top surfaces in general, these are the settings I've been using while testing:

```
Top Solid Infill Speed: 80 mm/s
Top Solid Infill Width: 0.4 mm
Top Solid Infill Accel: 10,000 mm/s^2
Top Fill Pattern: Monotonic Lines (Unconnected although the flow comp may compensate for connected lines!)
Top/Bottom Layer Count: 5
```

However, feel free to use your own settings during the testing (the ones above are an optional guide)



# More Cool G-Code

![demo_animation](/Screenshots/demo_animation.gif)



# BETA Test Objectives

During this beta test programme, there are 3 main objectives that need testing

1. Checking the **executables/scripts run on a variety of platforms** and operating systems
2. The **script actually works** and makes noticeable changes (hopefully positive ones)
3. Error proofing in the **script is robust** enough to handle, and **correctly process a variety of gcode**



# How Do I Know The Script Is Working?

Assuming the slicer doesn't throw any errors, if you search the gcode file for `old value` it should show modified gcode lines

The two main indicators of proper flow on smaller infill areas are visual and tactile. The latter is a much better indicator as on small infill areas, the pattern of extrusion lines can often be quite misleading, so if the surface feels smooth, despite looking rough, then the flow is correctly compensated.

I would really like to see photos of what prints you get in [this channel](link.to.channel), extra brownie points for the best (and worst) results!



# Test STL Files

There is nothing special about these files for the most part, they just have some top surfaces of various sizes that test the script well. You can print anything you want with the script, it would be pretty bad if it only worked with these STLs



### Modified_Voron_Filament_Card.stl

This file is a slight edit of the [voron filament card](https://github.com/VoronDesign/Voron-2/blob/Voron2.4/STLs/Test_Prints/Filament_Card.stl), it is made slightly thicker to mitigate any first layer over squish/extrusion travelling through the print affecting top surfaces



### centre_brace_8x_wago.stl

This is a file from my [custom V0 project](https://github.com/Alexander-T-Moss/Hex-Zero) (go check it out, *wink wink*), I recommend printing it with 2 perimeters as will make lots of long thin infill areas that are normally quite difficult to print well



### cw1_extruder_body.stl

This is the body of the [CW1 extruder](https://docs.vorondesign.com/hardware.html#mini-stealthburner), I used this in my testing initially as was shown on ellis3dp and I thought it would make a good point to start testing
