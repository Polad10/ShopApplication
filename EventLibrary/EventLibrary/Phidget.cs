using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Phidgets;
using Phidgets.Events;

namespace EventLibrary
{
    public class MyPhidget
    {
        //fields
        public static RFID myRfid { get; private set; }
        //methods
        public static void Start()      
        {
            myRfid = new RFID();
            myRfid.open();
            myRfid.waitForAttachment(3000);
            myRfid.Antenna = true;
            myRfid.LED = true;
        }
        public static void AddToTagEvent(TagEventHandler d)
        {
            myRfid.Tag += d;
        }
    }
}
