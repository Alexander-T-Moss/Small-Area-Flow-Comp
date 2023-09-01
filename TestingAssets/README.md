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

# BETA Test Objectives

During this beta test programme, there are 3 main objectives that need testing

1. Checking the **executables/scripts run on a variety of platforms** and operating systems
2. The **script actually works** and makes noticeable changes (hopefully positive ones)
3. Error proofing in the **script is robust** enough to handle, and **correctly process a variety of gcode**



# How Do I Help In This BETA Test?

Obviously, to test the first objective, you just need to download a script and confirm it runs properly and modifies the gcode. To test the other objectives you just need to print something twice, once with, and once without the script and compare the models.

The two main indicators of proper flow on smaller infill areas are visual and tactile. The latter is a much better indicator as on small infill areas, the pattern of extrusion lines can often be quite misleading, so if the surface feels smooth, despite looking rough, then the flow is correctly compensated.

I would really like to see photos of what prints you get in [this channel](link.to.channel) (the channel is not just for posting issues) but obviously, you are under no obligation to do this, and if you just want to try it out independently, you are more than welcome to :)


# Test Print
Here is a photo of an initial test of the script


![test_print](/Screenshots/test_print.jpg)
