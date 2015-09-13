using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSScriptLibrary;
using csscript;
using Terraria;

namespace CSharpScript
{
    public abstract class Script : IDisposable
    {
        public virtual string Name
        {
            get
            {
                return "None";
            }
        }
        public virtual Version Version
        {
            get
            {
                return new Version(1, 0);
            }
        }
        public virtual string Author
        {
            get
            {
                return "None";
            }
        }
        public virtual string Description
        {
            get
            {
                return "None";
            }
        }
        public virtual bool Enabled
        {
            get;
            set;
        }

        public int Order
        {
            get;
            set;
        }

        protected Main Game
        {
            get;
            private set;
        }

        protected Script()
        {
            this.Order = 1;
        }

        ~Script()
        {
            this.Dispose(false);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        public abstract void Initialize();
    }
}
