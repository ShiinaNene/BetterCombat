using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;

using DuckGame;
using Microsoft.Win32;
using Microsoft.Xna.Framework;


namespace BetterCombat
{
    public class BetterCombat : Mod
    {
        public static bool SwapMouseButtons = false;
        protected override void OnPostInitialize()
        {
            base.OnPostInitialize();

            try
            {
                var swapMouseButtons = Registry.CurrentUser.OpenSubKey(@"Control Panel\Mouse\").GetValue("SwapMouseButtons").ToString();
                if (Convert.ToInt32(swapMouseButtons) == 1)
                {
                    SwapMouseButtons = true;
                }
            }
            catch { }
            

            var form = Program.main.GetForm();
            form?.BeginInvoke(new EventHandler((sender, e) =>
            {
                form.Cursor = Cursors.Cross;
            }));
            var reflectionFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;
            (typeof(Game).GetField("updateableComponents", reflectionFlags).GetValue(MonoMain.instance) as List<IUpdateable>).Add(new ModUpdate());

        }
    }
    internal static class Extensions
    {
        internal static Form GetForm(this Main main)
        {
            var reflectionFlags = BindingFlags.Instance | BindingFlags.NonPublic;
            var mainClassType = main.GetType().BaseType.BaseType;
            var hostField = mainClassType.GetField("host", reflectionFlags);
            var hostInstance = hostField.GetValue(main);
            var windowInstance = hostInstance.GetType().GetField("gameWindow", reflectionFlags).GetValue(hostInstance);
            return (Form)windowInstance.GetType().GetField("mainForm", reflectionFlags).GetValue(windowInstance);
        }
    }
}
