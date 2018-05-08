/**
 * Image Filtering
 * This sketch will help us to adjust the filter values to optimize blob detection
 * 
 * Persistence algorithm by Daniel Shifmann:
 * http://shiffman.net/2011/04/26/opencv-matching-faces-over-time/
 *
 * @author: Jordi Tost (@jorditost)
 * @url: https://github.com/jorditost/ImageFiltering/tree/master/ImageFilteringWithBlobPersistence
 *
 * University of Applied Sciences Potsdam, 2014
 *
 * It requires the ControlP5 Processing library:
 * http://www.sojamo.de/libraries/controlP5/
 */
 
import gab.opencv.*;
import java.awt.Rectangle;
import processing.video.*;
import controlP5.*;
import KinectPV2.*;

OpenCV opencv;
Capture video;
KinectPV2 kinect;

PImage src, preProcessedImage, processedImage, contoursImage, initImage, blended;
PImage bckgSub;

boolean updateBackgroundMask = false;

ArrayList<Contour> contours;

// List of detected contours parsed as blobs (every frame)
ArrayList<Contour> newBlobContours;

// List of my blob objects (persistent)
ArrayList<Blob> blobList;


// Number of blobs detected over all time. Used to set IDs.
int blobCount = 0;

float contrast = 1.35;
int brightness = 0;
int threshold = 75;
boolean useAdaptiveThreshold = false; // use basic thresholding
int thresholdBlockSize = 489;
int thresholdConstant = 45; // 45
int blobSizeThreshold = 80; // 20
int blobSizeMax = 200;
int blurSize = 5; //4 

// Control vars
ControlP5 cp5;
int buttonColor;
int buttonBgColor;

void setup() {
  frameRate(10); //<>//
  
  kinect = new KinectPV2(this);
  kinect.enableDepthImg(true);
  kinect.enableInfraredLongExposureImg(true);
  kinect.init();
  
  ////opencv = new OpenCV(this, 640, 480);
  opencv = new OpenCV(this, 512, 424);
  //opencv.startbckgSub(5, 3, 0.5);
  contours = new ArrayList<Contour>();
  
  // Blobs list
  blobList = new ArrayList<Blob>();
  
  size(1080, 480, P3D); // 840
  
  // Init Controls
  cp5 = new ControlP5(this);
  initControls();
  
  // Set thresholding
  toggleAdaptiveThreshold(useAdaptiveThreshold);
  
  initImage = new PImage(); // kinect.getDepthImage().width, kinect.getDepthImage().height
  bckgSub = new PImage(512,424);
}

void keyPressed() {
  if (key == 'i') {
    //opencv.loadImage(kinect.getInfraredLongExposureImage());
    initImage = kinect.getDepthImage();
    //initImage = opencv.getInput().copy();
    println("done");
    
    //opencv.startbckgSub(5, 3, 0.5);
    
    initImage.loadPixels();
    println(initImage.get(80, 80));
    
  }
  
  if (key == '+') {
    updateBackgroundMask = true;
  }
}

void updateMask(PImage input) {
  bckgSub.blend(input, 0, 0, input.width, input.height, 0, 0, input.width, input.height, ADD);
  println("updating mask");
  //image(bckgSub, 0, 0, bckgSub.width/2, bckgSub.height/2);
}

void draw() {
  // Load the new frame of our camera in to OpenCV
  
  PImage opencvInput = kinect.getDepthImage();
  opencv.loadImage(opencvInput);
  src = opencv.getInput().copy(); //<>//

  
  ///////////////////////////////
  // <1> PRE-PROCESS IMAGE
  // - Grey channel 
  // - Brightness / Contrast
  ///////////////////////////////
  
  // Gray channel
  //opencv.gray();
  
  //opencv.brightness(brightness);
  opencv.contrast(contrast);
  
  // Save snapshot for display
  preProcessedImage = opencv.getOutput().copy();
  
  ///////////////////////////////
  // <2> PROCESS IMAGE
  // - Threshold
  // - Noise Supression
  ///////////////////////////////
  
  
  // Adaptive threshold - Good when non-uniform illumination
  if (useAdaptiveThreshold) {
    
    // Block size must be odd and greater than 3
    if (thresholdBlockSize%2 == 0) thresholdBlockSize++;
    if (thresholdBlockSize < 3) thresholdBlockSize = 3;
    
    opencv.adaptiveThreshold(thresholdBlockSize, thresholdConstant);
    
  // Basic threshold - range [0, 255]
  } else {
    opencv.threshold(threshold);
  }

  // Invert (black bg, white blobs)
  opencv.invert();
  
  // Reduce noise - Dilate and erode to close holes
  opencv.dilate();
  opencv.erode();
  
  // Blur
  opencv.blur(blurSize);
  
  if (updateBackgroundMask) {
    updateMask(opencv.getOutput().copy());
    updateBackgroundMask = false;
  }
  
  PImage opencvImg = opencv.getOutput();
  opencvImg.blend(bckgSub, 0, 0, bckgSub.width, bckgSub.height, 0, 0, bckgSub.width, bckgSub.height, SUBTRACT);
  opencv.loadImage(opencvImg);
  
  opencv.erode();
  opencv.erode();
  opencv.dilate();
  opencv.dilate();
  
  // Save snapshot for display
  processedImage = opencv.getOutput();
  
  ///////////////////////////////
  // <3> FIND CONTOURS  
  ///////////////////////////////
  
  detectBlobs();
  // Passing 'true' sorts them by descending area.
  //contours = opencv.findContours(true, true);
  
  // Save snapshot for display
  contoursImage = opencv.getInput().copy();
  
  // Draw
  pushMatrix();
    
    // Leave space for ControlP5 sliders
    //translate(width-src.width, 0);
    translate(300, 0);
    
    // Display images
    displayImages();
    
    // Display contours in the lower right window
    pushMatrix();
      scale(0.5);
      translate(src.width, src.height);
      
      // Contours
      //displayContours();
      //displayContoursBoundingBoxes();
      
      // Blobs
      displayBlobs();
      
    popMatrix(); 
    
  popMatrix();
  //opencv.loadImage(kinect.getDepthImage());
  //initImage = opencv.getInput();
  
  sendBlobs();
}

///////////////////////
// Display Functions
///////////////////////

void displayImages() {

  
  //image(initImage, 0, 0);
  pushMatrix();
  fill(0);
  rect(0, 0, width, height);
  
  scale(0.5);
  image(src, 0, 0);
  image(preProcessedImage, src.width, 0);
  image(bckgSub, src.width*2 + 10, 0);
  image(processedImage, 0, src.height);
  image(src, src.width, src.height);
  //image(opencv.getInput(), 0, 0, src.width*2, src.height*2);
  popMatrix();
  
  stroke(255);
  fill(255);
  textSize(12);
  text("Source", 10, 25); 
  text("Pre-processed Image", src.width/2 + 10, 25); 
  text("Processed Image", 10, src.height/2 + 25); 
  text("Tracked Points", src.width/2 + 10, src.height/2 + 25);
}

void displayBlobs() {
  
  for (Blob b : blobList) {
    strokeWeight(1);
    b.display();
  }
}

void displayContours() {
  
  // Contours
  for (int i=0; i<contours.size(); i++) {
  
    Contour contour = contours.get(i);
    
    noFill();
    stroke(0, 255, 0);
    strokeWeight(3);
    contour.draw();
  }
}

void displayContoursBoundingBoxes() {
  
  for (int i=0; i<contours.size(); i++) {
    
    Contour contour = contours.get(i);
    Rectangle r = contour.getBoundingBox();
    
    if (//(contour.area() > 0.9 * src.width * src.height) ||
        (r.width < blobSizeThreshold || r.height < blobSizeThreshold || r.width > blobSizeMax || r.height > blobSizeMax))
      continue;
    
    stroke(255, 0, 0);
    fill(255, 0, 0, 150);
    strokeWeight(2);
    rect(r.x, r.y, r.width, r.height);
  }
}

////////////////////
// Send blobs
////////////////////
void sendBlobs() {
  println(blobList.size());
  
  for (int i = 0; i < blobList.size(); i++) {
    for (int j = i+1; j < blobList.size(); j++) {
      Blob a = blobList.get(i);
      Rectangle aBB = a.getBoundingBox();
      Blob b = blobList.get(j);
      Rectangle bBB = b.getBoundingBox();
      
      // TODO 
      //a.active = true;
      //b.active = true;
      
      if (bBB.contains(aBB)) {
        a.active = false;
      } else if (aBB.contains(bBB)) {
        b.active = false;
      }
    }
  }
  
  for (Blob b : blobList) {
    if (b.active) {
      println("id:", b.id);
    }
  }
}




////////////////////
// Blob Detection
////////////////////

void detectBlobs() {
  
  // Contours detected in this frame
  // Passing 'true' sorts them by descending area.
  contours = opencv.findContours(true, true);
  
  newBlobContours = getBlobsFromContours(contours);
  
  //println(contours.length);
  
  // Check if the detected blobs already exist are new or some has disappeared. 
  
  // SCENARIO 1 
  // blobList is empty
  if (blobList.isEmpty()) {
    // Just make a Blob object for every face Rectangle
    for (int i = 0; i < newBlobContours.size(); i++) {
      println("+++ New blob detected with ID: " + blobCount);
      blobList.add(new Blob(this, blobCount, newBlobContours.get(i)));
      blobCount++;
    }
  
  // SCENARIO 2 
  // mame nove bloby
  // We have fewer Blob objects than face Rectangles found from OpenCV in this frame
  } else if (blobList.size() <= newBlobContours.size()) {
    boolean[] used = new boolean[newBlobContours.size()];
    // Match existing Blob objects with a Rectangle
    for (Blob b : blobList) {
       // Find the new blob newBlobContours.get(index) that is closest to blob b
       // set used[index] to true so that it can't be used twice
       float record = 50000;
       int index = -1;
       for (int i = 0; i < newBlobContours.size(); i++) {
         float d = dist(newBlobContours.get(i).getBoundingBox().x, newBlobContours.get(i).getBoundingBox().y, b.getBoundingBox().x, b.getBoundingBox().y);
         //float d = dist(blobs[i].x, blobs[i].y, b.r.x, b.r.y);
         if (d < record && !used[i]) {
           record = d;
           index = i;
         } 
       }
       // Update Blob object location
       used[index] = true;
       b.update(newBlobContours.get(index));
    }
    // Add any unused blobs
    for (int i = 0; i < newBlobContours.size(); i++) {
      if (!used[i]) {
        
        // do not add the countour if it has intersection with existing blobs
        boolean intersects = false;
        for(Blob b : blobList) {
          intersects = intersects || b.getBoundingBox().intersects(newBlobContours.get(i).getBoundingBox());
        }
        if (intersects) { continue; }
        
        println("+++ New blob detected with ID: " + blobCount);
        blobList.add(new Blob(this, blobCount, newBlobContours.get(i)));
        //blobList.add(new Blob(blobCount, blobs[i].x, blobs[i].y, blobs[i].width, blobs[i].height));
        blobCount++;
      }
    }
  
  // SCENARIO 3 
  // We have more Blob objects than blob Rectangles found from OpenCV in this frame
  } else {
    // All Blob objects start out as available
    for (Blob b : blobList) {
      b.available = true;
    } 
    // Match Rectangle with a Blob object
    for (int i = 0; i < newBlobContours.size(); i++) {
      // Find blob object closest to the newBlobContours.get(i) Contour
      // set available to false
       float record = 50000;
       int index = -1;
       for (int j = 0; j < blobList.size(); j++) {
         Blob b = blobList.get(j);
         float d = dist(newBlobContours.get(i).getBoundingBox().x, newBlobContours.get(i).getBoundingBox().y, b.getBoundingBox().x, b.getBoundingBox().y);
         //float d = dist(blobs[i].x, blobs[i].y, b.r.x, b.r.y);
         if (d < record && b.available) {
           record = d;
           index = j;
         } 
       }
       // Update Blob object location
       Blob b = blobList.get(index);
       b.available = false;
       b.update(newBlobContours.get(i));
       
       // do not add the countour if it has intersection with existing blobs
        //boolean intersects = false;
        //for(Blob otherB : blobList) {
        //  intersects = intersects || otherB.getBoundingBox().intersects(newBlobContours.get(i).getBoundingBox());
        //}
        //if (intersects) { b.delete = true; }
        
    } 
    // Start to kill any left over Blob objects
    for (Blob b : blobList) {
      if (b.available) {
        b.countDown();
        if (b.dead()) {
          b.delete = true;
        } 
      }
    } 
  }
  
  // Delete any blob that should be deleted
  for (int i = blobList.size()-1; i >= 0; i--) {
    Blob b = blobList.get(i);
    if (b.delete) {
      blobList.remove(i);
    } 
  }
}

ArrayList<Contour> getBlobsFromContours(ArrayList<Contour> newContours) {
  
  ArrayList<Contour> newBlobs = new ArrayList<Contour>();
  
  // Which of these contours are blobs?
  for (int i=0; i<newContours.size(); i++) {
    
    Contour contour = newContours.get(i);
    Rectangle r = contour.getBoundingBox();
    
    if (//(contour.area() > 0.9 * src.width * src.height) ||
        (r.width < blobSizeThreshold || r.height < blobSizeThreshold))
      continue;
    
    newBlobs.add(contour);
  }
  
  return newBlobs;
}

//////////////////////////
// CONTROL P5 Functions
//////////////////////////

void initControls() {
  // Slider for contrast
  cp5.addSlider("contrast")
     .setLabel("contrast")
     .setPosition(20,50)
     .setRange(0.0,6.0)
     ;
     
  // Slider for threshold
  cp5.addSlider("threshold")
     .setLabel("threshold")
     .setPosition(20,110)
     .setRange(0,255)
     ;
  
  // Toggle to activae adaptive threshold
  cp5.addToggle("toggleAdaptiveThreshold")
     .setLabel("use adaptive threshold")
     .setSize(10,10)
     .setPosition(20,144)
     ;
     
  // Slider for adaptive threshold block size
  cp5.addSlider("thresholdBlockSize")
     .setLabel("a.t. block size")
     .setPosition(20,180)
     .setRange(1,700)
     ;
     
  // Slider for adaptive threshold constant
  cp5.addSlider("thresholdConstant")
     .setLabel("a.t. constant")
     .setPosition(20,200)
     .setRange(-100,100)
     ;
  
  // Slider for blur size
  cp5.addSlider("blurSize")
     .setLabel("blur size")
     .setPosition(20,260)
     .setRange(1,20)
     ;
     
  // Slider for minimum blob size
  cp5.addSlider("blobSizeThreshold")
     .setLabel("min blob size")
     .setPosition(20,290)
     .setRange(0,60)
     ;
     
  // Store the default background color, we gonna need it later
  buttonColor = cp5.getController("contrast").getColor().getForeground();
  buttonBgColor = cp5.getController("contrast").getColor().getBackground();
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
    theController.setColorBackground(color(150,150));
    theController.setColorForeground(color(100,100));
  
  } else {
    theController.setColorBackground(color(buttonBgColor));
    theController.setColorForeground(color(buttonColor));
  }
}