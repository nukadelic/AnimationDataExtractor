# Unity Animation Clip data extractor
Extract curve data to a scriptable object asset that can be accessed and edited in runtime 

Here is an example component that uses the extracted data 

![GIF or MP4 ?](https://i.imgur.com/jDq7Pmm.gif)  
( 'Strut Walking' animation and the 'Y Bot' model from mixamo )  

### Usage 
* Find the editor window in `Tools` > `Animation Data Extractor`
* Select your animation clip
* Use the simple string removal ( by default its `mixamorig:` ) as well as filters to remove clutter 
* Press Extract button to save scriptable object asset into the `Assets/Exported` folder with the same name as the animation clip 
  
  
Preview allows to compare data from the animation curves and keyframes to the extracted data | Keyframe preview 
--- | ---
![_](https://i.imgur.com/PriNzP4.png) | ![_](https://i.imgur.com/0IPofEG.png)
