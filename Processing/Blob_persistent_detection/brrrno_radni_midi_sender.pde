import themidibus.*;

MidiBus midiBus;

void initMidiSender() {
  // For windows we need to install and set Loop Internal MIDI as MIDI output
  MidiBus.list();
  midiBus = new MidiBus(this, 0, "LoopBe Internal MIDI");
}

void sendMidiMessage(Blob b) { 
  Rectangle rect = b.getBoundingBox();
  float x_out = 127 - map(rect.x, 0, 512, 0, 127);
  float y_out = map(rect.y, 0, 424, 0, 127);
  print("Sending", "id:", b.id % 128, "x:", x_out, "y:", rect.y, "r:", "\n");
  midiBus.sendControllerChange(0, b.id % 128, int(x_out));
  midiBus.sendControllerChange(1, b.id % 128, int(y_out));
}

void sendMidiDead(Blob b) {
  print("Sending", "id:", b.id % 128, "I am dead!", "\n");
  midiBus.sendControllerChange(2, b.id % 128, 0);
}
