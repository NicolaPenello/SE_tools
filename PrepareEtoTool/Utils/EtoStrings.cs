using System;
using System.Collections.Generic;
using System.Text;

namespace EtoUtils
{
    internal enum EtoProjects
    {
        // obsolete enum
        OBSOLETE = 0x80,
        ACTIVE = 0x00,

        // obsolete projects
        ECOBRZ = OBSOLETE | 1,
        MBGEN = OBSOLETE | 2,
        UNFLRLE = OBSOLETE | 3,

        // unexpected project
        UNEXPECTED = 0x00,

        // active projects
        AMICO = ACTIVE | 1,
        BCWC = ACTIVE | 2,
        AQUACENTR = BCWC,
        LEOGEN2 = ACTIVE | 3,
        TRMCHLR = ACTIVE | 4,
        UNFLRLE_L = ACTIVE | 5,
        UNFLRTSA = ACTIVE | 6
    }

    internal class EtoStrings
    {
        // directories
        public const string APP_DIR = "app";
        public const string DMMMB_DIR = "dmMmb";
        public const string H_DIR = "h";
        public const string APPLANG_DIR = "applang";
        public const string LANGPACKS_DIR = "langpacks";

        // files
        public const string HEADER_C = "header.c";
        public const string MAKEFILE = "makefile";
        public const string APPLANG_MAK = "applang.mak";
        public const string LANG_H = "lang.h";

        // suffixes
        public const string STRDB_C = "strdb.c";
        public const string STRL_H = "strl.h";
        public const string LPK_SFX = "lpk";

        // active projects
        public const string AMICO = "amico";
        public const string BCWC = "bcwc";
        public const string AQUACENTR = "aquacentr";
        public const string LEOGEN2 = "leogen2";
        public const string TRMCHLR = "trmchlr";
        public const string UNFLRLE_L = "unflrle_l";
        public const string UNFLRTSA = "unflrtsa";

        // obsolete projects
        public const string ECOBRZ = "ecobrz";
        public const string MBGEN = "mbGen";
        public const string UNFLRLE = "unflrle";

        public static EtoProjects ToEnum(string appName)
        {
            if (appName.Equals(AMICO)) return EtoProjects.AMICO;
            else if (appName.Equals(BCWC)) return EtoProjects.BCWC;
            else if (appName.Equals(AQUACENTR)) return EtoProjects.AQUACENTR;
            else if (appName.Equals(LEOGEN2)) return EtoProjects.LEOGEN2;
            else if (appName.Equals(TRMCHLR)) return EtoProjects.TRMCHLR;
            else if (appName.Equals(UNFLRLE_L)) return EtoProjects.UNFLRLE_L;
            else if (appName.Equals(UNFLRTSA)) return EtoProjects.UNFLRTSA;
            else if (appName.Equals(ECOBRZ)) return EtoProjects.ECOBRZ;
            else if (appName.Equals(MBGEN)) return EtoProjects.MBGEN;
            else if (appName.Equals(UNFLRLE)) return EtoProjects.UNFLRLE;
            else return EtoProjects.UNEXPECTED;
        }
    }
    internal class CommandLineCommands
    {
        public const string NMAKE = "nmake";
        public const string CLEAN = "clean";
        public const string LPK = "lpk";
        public const string LANG = "lang";
    }
}
