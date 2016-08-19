using Apache.Phoenix;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Garuda.Data.Internal
{
    class GarudaResultSet
    {
        public List<Frame> Frames { get; set; }

        public Signature Signature { get; set; }

        public ulong UpdateCount { get; set; }

        public GarudaResultSet(Signature sig, Frame firstFrame, ulong updateCount)
        {
            this.Signature = sig;
            this.Frames = new List<Frame>();
            this.UpdateCount = updateCount;
            this.Frames.Add(firstFrame);
        }
    }
}
