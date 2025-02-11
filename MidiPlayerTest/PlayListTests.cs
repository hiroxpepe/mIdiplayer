﻿
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

using MidiPlayer;

namespace MidiPlayerTest {
    [TestClass()]
    public class PlayListTests {
#nullable enable
        [TestMethod()]
        public void NextTest1() {
            PlayList target = new();
            target.Add("file1.mid");
            target.Add("file2.mid");
            target.Add("file3.mid");
            AreEqual("file1.mid", target.Next);
            AreEqual("file2.mid", target.Next);
            AreEqual("file3.mid", target.Next);
            AreEqual("file1.mid", target.Next);
            AreEqual("file2.mid", target.Next);
            target.Add("file4.mid");
            AreEqual("file3.mid", target.Next);
            AreEqual("file4.mid", target.Next);
            AreEqual("file1.mid", target.Next);
            AreEqual("file2.mid", target.Next);
            AreEqual("file3.mid", target.Next);
            AreEqual("file4.mid", target.Next);
        }
    }
}
