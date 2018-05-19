
//////////////////////////
// CONTROL P5 Functions
//////////////////////////

void initControls() {
  // Slider for bbrightnes
  //cp5.addSlider("brightness")
  //   .setLabel("brightness")
  //   .setPosition(20,20)
  //   .setRange(0.0,6.0)
  //   ;

  // Slider for contrast
  cp5.addSlider("contrast")
     .setLabel("contrast")
     .setPosition(20,40)
     .setRange(0.0,6.0)
     ;
     
  // Slider for threshold
  cp5.addSlider("threshold")
     .setLabel("threshold")
     .setPosition(20,60)
     .setRange(0,255)
     ;
  
  // Toggle to activae adaptive threshold
  cp5.addToggle("toggleAdaptiveThreshold")
     .setLabel("use adaptive threshold")
     .setSize(10,10)
     .setPosition(20,100)
     ;
     
  // Slider for adaptive threshold block size
  cp5.addSlider("thresholdBlockSize")
     .setLabel("a.t. block size")
     .setPosition(20,130)
     .setRange(1,700)
     ;
     
  // Slider for adaptive threshold constant
  cp5.addSlider("thresholdConstant")
     .setLabel("a.t. constant")
     .setPosition(20,150)
     .setRange(-100,100)
     ;
  
  // Slider for blur size
  cp5.addSlider("blurSize")
     .setLabel("blur size")
     .setPosition(20,190)
     .setRange(1,20)
     ;
         
  // Update background mask
  cp5.addButton("updateBckgMask")
    .setLabel("Update background mask")
    .setPosition(20, 240)
    .setSize(200, 30)
    ;
      
  // Remove background mask
  cp5.addButton("removeBckgMask")
    .setLabel("Remove background mask")
    .setPosition(20, 280)
    .setSize(200, 20)
    ;
    
  // Set blob life
  cp5.addSlider("blobLife")
     .setLabel("Blob life")
     .setPosition(20,330)
     .setSize(128, 15)
     .setRange(0,127)
     ;
     
  // Slider for minimum blob size
  cp5.addSlider("blobSizeThreshold")
     .setLabel("min blob size")
     .setPosition(20,370)
     .setSize(128, 15)
     .setRange(0,100)
     ;

  // Slider for minimum blob size
  cp5.addSlider("blobSizeMax")
     .setLabel("Max blob size")
     .setPosition(20, 390)
     .setSize(128, 15)
     .setRange(60, 400)
     .setValue(200)
     ;


  // Store the default background color, we gonna need it later
  buttonColor = cp5.getController("contrast").getColor().getForeground();
  buttonBgColor = cp5.getController("contrast").getColor().getBackground();
}

void updateBckgMask(int theValue) {
  updateBackgroundMask = true;
}

void removeBckgMask(int value) {
  bckgSub = new PImage(512,424);
}

void toggleAdaptiveThreshold(boolean theFlag) {
  
  useAdaptiveThreshold = theFlag;
  
  if (useAdaptiveThreshold) {
    
    // Lock basic threshold
    setLock(cp5.getController("threshold"), true);
       
    // Unlock adaptive threshold
    setLock(cp5.getController("thresholdBlockSize"), false);
    setLock(cp5.getController("thresholdConstant"), false);
       
  } else {
    
    // Unlock basic threshold
    setLock(cp5.getController("threshold"), false);
       
    // Lock adaptive threshold
    setLock(cp5.getController("thresholdBlockSize"), true);
    setLock(cp5.getController("thresholdConstant"), true);
  }
}

void setLock(Controller theController, boolean theValue) {
  
  theController.setLock(theValue);
  
  if (theValue) {
    theController.setColorBackground(color(100,150));
    theController.setColorForeground(color(100,100));
  
  } else {
    theController.setColorBackground(color(buttonBgColor));
    theController.setColorForeground(color(buttonColor));
  }
}