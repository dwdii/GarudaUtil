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

        public GarudaResultSet(Signature sig, Frame firstFrame)
        {
            this.Signature = sig;
            this.Frames = new List<Frame>();
            this.Frames.Add(firstFrame);
        }
    }
}
