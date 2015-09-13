using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using TerrariaApi.Server;
using Terraria;


namespace CSharpScript
{
    [ApiVersion(1,21)]
    public class CSharpScriptPlugin : TerrariaPlugin
    {
        public override string Author { get { return "Ancientgods and magnusi"; } }
        public override string Name { get { return "CSharpScript"; } }
        public override Version Version { get { return new Version(0,1); } }
        public CSharpScriptPlugin(Main game)
            : base(game)
        {
        }
        public override void Initialize()
        {
            ScriptLoader.Initialize();
        }
    }
}
