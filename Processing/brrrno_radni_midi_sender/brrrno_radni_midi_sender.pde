import themidibus.*;

final class Blob {
  int x;
  int y;
  int r;
  
  Blob(int x, int y, int r) {
    this.x = x;
    this.y = y;
    this.r = r;
  }
}

MidiBus midiBus;
Blob[] midiBlobs = new Blob[128];

boolean mouseDown = false;
int currentIndex = 0;

void setup() {
  size(512, 424);
  frameRate(10);
  // For windows we need to install and set Loop Internal MIDI as MIDI output
  MidiBus.list();
  midiBus = new MidiBus(this, 0, "LoopBe Internal MIDI");
  
  killAll();
}

void draw() {
  if (mouseDown == true) {
    midiBlobs[currentIndex].x = mouseX;
    midiBlobs[currentIndex].y = mouseY;
  }
  sendMidiMessages();
}

void mousePressed() {
  mouseDown = true;
  currentIndex = 25;//(int)random(1, 127);
  midiBlobs[currentIndex] = new Blob(mouseX - 1, mouseY - 1, (int)random(10, 30));
}

void mouseReleased() {
  mouseDown = false;
  currentIndex = 0;
  midiBlobs = new Blob[128];
}

void killAll() {
  for(int i = 0; i < 128; i++) {
      //print("Sending", "id:", i, "x:", midiBlobs[i].x, "y:", midiBlobs[i].y, "r:", midiBlobs[i].r, "\n");
     // midiBus.sendControllerChange(0, i, midiBlobs[i].x);
      //midiBus.sendControllerChange(1, i, midiBlobs[i].y);
      midiBus.sendControllerChange(2, i, 0);
  }
}

void sendMidiMessages() {
  for(int i = 0; i < midiBlobs.length; i++) {
    if (midiBlobs[i] != null) {
      float x_out = map(midiBlobs[i].x, 0, 512, 0, 127);
      print("Sending", "id:", i, "x:", int(x_out), "y:", midiBlobs[i].y, "r:", midiBlobs[i].r, "\n");
      midiBus.sendControllerChange(0, i, int(x_out));
      midiBus.sendControllerChange(1, i, midiBlobs[i].y);
      //midiBus.sendControllerChange(2, i, midiBlobs[i].r);
    }
  }
}
