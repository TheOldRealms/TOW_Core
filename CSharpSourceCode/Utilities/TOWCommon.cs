using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace TOW_Core.Utilities
{
    public static class TOWCommon
    {
        private static Random _random = new Random();
        /// <summary>
        /// Print a message to the MB2 message window.
        /// </summary>
        /// <param name="text">The text that you want to print to the console.</param>
        public static void Say(string text)
        {
            InformationManager.DisplayMessage(new InformationMessage(text, new Color(134, 114, 250)));
        }

        public static void Log(string message, LogLevel severity)
        {
            var logger = LogManager.GetCurrentClassLogger();
            logger.Log(severity, message);
        }

        public static string GetRandomScene()
        {
            var filterednames = new List<string>();
            string pickedname = "towmm_menuscene_01";
            var path = BasePath.Name + "Modules/TOW_EnvironmentAssets/SceneObj/";
            if (Directory.Exists(path))
            {
                var dirnames = Directory.GetDirectories(path);
                filterednames = dirnames.Where(x =>
                {
                    string[] s = x.Split('/');
                    var name = s[s.Length - 1];
                    if (name.StartsWith("towmm_")) return true;
                    else return false;
                }).ToList();
            }
            if (filterednames.Count > 0)
            {
                var index = _random.Next(0, filterednames.Count);
                pickedname = filterednames[index];
                string[] s = pickedname.Split('/');
                pickedname = s[s.Length - 1];

            }
            return pickedname;
        }
    }
}
