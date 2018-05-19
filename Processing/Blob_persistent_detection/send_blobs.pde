////////////////////
// Send blobs
////////////////////


void sendBlobs() {
  
  for (int i = 0; i < blobList.size(); i++) {
    for (int j = i+1; j < blobList.size(); j++) {
      Rectangle aBB = blobList.get(i).getBoundingBox();
      Rectangle bBB = blobList.get(j).getBoundingBox();
      
      if (!aBB.intersects(bBB)) {
        aBB.active = true;
        bBB.active = true;
      }
    }
  }
  
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
      sendMidiMessage(b);
    }
  }
}