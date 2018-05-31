# RecyclAR
RecyclAR is an Augmented Reality(AR) game about a robot who is tasked to vacuum and process all sorts of wastes that are being brought from various waste sources in the real world. Players are to play as the robot, discover the waste sources around their vicinity, and continuously clean up and recycle an endless supply of wastes released from these waste sources. 

At the current phase of RecyclAR, there are 7 types of objects being released from any given waste source; these are: plastic wastes, metal wastes, organic wastes, electronic wastes, hazardous wastes, inert wastes and animals.Players must properly identify and handle these objects with the right interactions. Failure to do so will result in failure of the robot's task (Game Over). Here are the player controls:
Swipe up - Electronic waste recycling
Swipe right - Metal waste recycling
Swipe down - Plastic waste recycling
Swipe left - Organic waste recycling
Hold touch for a set duration - Hazardous waste pulverization
Reposition physically - Inert waste disposal
Single Tap - Animal rescue
For all touch interactions, players must initiate their touch on the object they want to be handling.

Another failure condition is if the player misses an interaction with an object, which results in either having those objects vacuummed by the robot or having them slide past the player. In both cases, they are considered as an improper disposal of waste or neglect of animal care.

Players are given different levels of challenges. In the first level, players only need to focus on one waste resource and a few types of wastes. As the level of challenge progress, more types of wastes will need to be dealt with and more waste sources will be responsible for.  

Developed and designed by Yao F. Chen, as a code practice for firstborn coporation.

future changes:
	-option to reset anchors in case tracking goes bad
	-Add distance check on anchors/waste sources (if they are too far from user's current position, prompt user t reset/re-initiate anchors) 
	-test if tracked planes will disappear, if it does, manage the display
	-make use of the detected planes
	-add more types of wastes and map to more controls
	-consider adding buffer to prevent player from instantly losing.
	-add audio