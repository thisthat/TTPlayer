using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HundredMilesSoftware.UltraID3Lib;

namespace TTPlayer.Classes
{
    static class Utility
    {

        //GET ID3 TAG
        static public string getTagTitle(string path)
        {
            UltraID3 u = new UltraID3();
            try
            {
                u.Read(path);
            }
            catch (Exception e) { }
            if (u != null || u.Title.Trim() == "")
            {
                return getFileName(path);
            }
            else
            {
                return u.Title;
            }
        }



        static public UltraID3 getTag(string path)
        {
            UltraID3 u = new UltraID3();
            try
            {
                u.Read(path);
            }
            catch (Exception e) {}
            return u;
        }

        static public string getFileName(string path)
        {
            return path.Split('\\').Last();
        }
    }
}
