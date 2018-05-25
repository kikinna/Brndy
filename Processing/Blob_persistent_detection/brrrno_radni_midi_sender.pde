import themidibus.*;

MidiBus midiBus;

void initMidiSender() {
  // For windows we need to install and set Loop Internal MIDI as MIDI output
  MidiBus.list();
  midiBus = new MidiBus(this, 0, "LoopBe Internal MIDI");
}

void sendMidiMessage(Blob b) { 
  Rectangle rect = b.getBoundingBox();
  print("Sending", "id:", b.id % 128, "x:", rect.x, "y:", rect.y, "r:", "\n");
  midiBus.sendControllerChange(0, b.id % 128, 128-rect.x);
  midiBus.sendControllerChange(1, b.id % 128, rect.y);
}

void sendMidiDead(Blob b) {
  print("Sending", "id:", b.id % 128, "I am dead!", "\n");
  midiBus.sendControllerChange(2, b.id % 128, 0);
}