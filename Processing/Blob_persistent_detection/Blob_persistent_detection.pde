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

PImage src, preProcessedImage, processedImage, contoursImage, blended;
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
int blobLife = 60;

// Control vars
ControlP5 cp5;
int buttonColor;
int buttonBgColor;

void setup() {
  frameRate(10); //<>// //<>// //<>//
  
  background(11);
  
  kinect = new KinectPV2(this);
  kinect.enableDepthImg(true);
  kinect.enableInfraredLongExposureImg(true);
  kinect.init();
  
  //opencv = new OpenCV(this, 640, 480);
  opencv = new OpenCV(this, 512, 424);
  
  // Contours list
  contours = new ArrayList<Contour>();
  // Blobs list
  blobList = new ArrayList<Blob>();
  
  size(1080, 480, P3D); // 840
  
  // Init Controls
  cp5 = new ControlP5(this);
  initControls();
  
  // Set thresholding
  toggleAdaptiveThreshold(useAdaptiveThreshold);
  
  // Image background subtraction mask 
  bckgSub = new PImage(512,424);
  
  initMidiSender();
}

void updateMask(PImage input) {
  bckgSub.blend(input, 0, 0, input.width, input.height, 0, 0, input.width, input.height, ADD);
  println("updating mask");
}

void draw() {
  // Load the new frame of our camera in to OpenCV
  PImage opencvInput = kinect.getDepthImage(); //<>// //<>//
  opencv.loadImage(opencvInput);
  src = opencv.getInput().copy(); //<>// //<>//
  
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
  //opencv.invert();
  
  // Reduce noise - Dilate and erode to close holes
  opencv.dilate();
  opencv.erode();
  
  // Blur
  opencv.blur(blurSize);
  
  if (updateBackgroundMask) {
    updateMask(opencv.getOutput().copy());
    updateBackgroundMask = false;
  }
  
  // subtract Background mask from the opencv image
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
  
  sendBlobs();
}

///////////////////////
// Display Functions
///////////////////////

void displayImages() {

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