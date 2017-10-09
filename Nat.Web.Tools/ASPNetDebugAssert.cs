using System.Diagnostics;

namespace Nat.Web.Tools
{
    public class ASPNetDebugAssert : System.Diagnostics.TraceListener
    {
        public override void Write(object o)
        {
            BreakInCode();
        }

        public override void Write(string message, string category)
        {
            BreakInCode();
        }

        public override void Write(object o, string category)
        {
            BreakInCode();
        }

        public override void WriteLine(object o)
        {
            BreakInCode();
        }

        public override void WriteLine(string message, string category)
        {
            BreakInCode();
        }

        public override void WriteLine(object o, string category)
        {
            BreakInCode();
        }

        public override void Write(string message)
        {
            BreakInCode();
        }

        public override void WriteLine(string message)
        {
            BreakInCode();
        }

        private static void BreakInCode()
        {
            BooleanSwitch assertSwitch = new BooleanSwitch("BreakOnAssert", "");
            if (assertSwitch.Enabled)
            {
                if (Debugger.IsAttached)
                    Debugger.Break();
                else
                    Debugger.Launch();
            }
        }
    }
}