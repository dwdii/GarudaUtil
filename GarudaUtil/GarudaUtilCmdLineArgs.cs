using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GarudaUtil
{
    class GarudaUtilCmdLineArgs
    {
        public string ConnectionString { get; private set; }

        public bool ShowException { get; private set; }

        public GarudaUtilCmdLineArgs(string[] args)
        {
            ParseArgs(args);
        }

        private void ParseArgs(string[] args)
        {
            for(int i = 0; i < args.Length; i++)
            {
                switch(args[i].ToLower())
                {
                    case ("-connectionstring"):
                        if(args.Length > i + 1)
                        {
                            this.ConnectionString = args[i + 1];
                        }
                        
                        break;

                    case ("-connectionstringfile"):
                        if (args.Length > i + 1)
                        {
                            this.ConnectionString = System.IO.File.ReadAllText(args[i + 1]);
                        }
                        break;

                    case ("-showexception"):
                        this.ShowException = true;
                        break;
                }
            }
        }
    }
}
