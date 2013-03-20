//###################################################################################################
/*
    Copyright (c) since 2012 - Paul Freund 
    
    Permission is hereby granted, free of charge, to any person
    obtaining a copy of this software and associated documentation
    files (the "Software"), to deal in the Software without
    restriction, including without limitation the rights to use,
    copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the
    Software is furnished to do so, subject to the following
    conditions:
    
    The above copyright notice and this permission notice shall be
    included in all copies or substantial portions of the Software.
    
    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
    EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
    OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
    NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
    HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
    WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
    FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
    OTHER DEALINGS IN THE SOFTWARE.
*/
//###################################################################################################


namespace Backend.Data
{
    public static class DefaultColors
    {
         public static string FrameForeground           =		"#FFFFFFFF";
         public static string FrameBackground           =		"#FF4F4F4F";
         public static string FrameSecondary	        =	   	"#FF3C3C3C";

         public static string ContentForeground         =		"#FF000000";
         public static string ContentBackground         =		"#FFFFFFFF";
         public static string ContentSecondary		    =		"#FFE9E9E9";
         public static string ContentPopout             =		"#FFCCCCCC";
         public static string ContentEnabled	        =       "#FF42B417";
         public static string ContentDisabled	        =       "#FFA6A6A6";

         public static string ContactListBackground     =		"#FFF4F4F4";
         public static string ContactListForeground     =		"#FF4F4F4F";
         public static string ContactListSelected       =		"#FFFFFFFF";

         public static string HighlightForeground       =       "#FFFFFFFF";
         public static string HighlightImportant        =       "#FFF53D00";
         public static string HighlightWarning          =       "#FFFF8000";
         public static string HighlightRequest          =       "#FF33CCFF"; 

         public static string StatusAvailable           =		"#FF4ABD1E";
         public static string StatusAway                =		"#FFFFA10B";
         public static string StatusDnd                 =		"#FFDC4D3C";
         public static string StatusOffline             =		"#FFB9B9B9"; 
    }

    public class Colors : IStore<string>
    {
        public Colors() : base() 
        {
            SetDefault("FrameForeground", DefaultColors.FrameForeground);
            SetDefault("FrameBackground", DefaultColors.FrameBackground);
            SetDefault("FrameSecondary", DefaultColors.FrameSecondary);

            SetDefault("ContentForeground", DefaultColors.ContentForeground);
            SetDefault("ContentBackground", DefaultColors.ContentBackground);
            SetDefault("ContentSecondary", DefaultColors.ContentSecondary);
            SetDefault("ContentPopout", DefaultColors.ContentPopout);
            SetDefault("ContentEnabled", DefaultColors.ContentEnabled);
            SetDefault("ContentDisabled", DefaultColors.ContentDisabled);

            SetDefault("ContactListBackground", DefaultColors.ContactListBackground);
            SetDefault("ContactListForeground", DefaultColors.ContactListForeground);
            SetDefault("ContactListSelected", DefaultColors.ContactListSelected);

            SetDefault("HighlightForeground", DefaultColors.HighlightForeground);
            SetDefault("HighlightImportant", DefaultColors.HighlightImportant);
            SetDefault("HighlightWarning", DefaultColors.HighlightWarning);
            SetDefault("HighlightRequest", DefaultColors.HighlightRequest);

            SetDefault("StatusAvailable", DefaultColors.StatusAvailable);
            SetDefault("StatusAway", DefaultColors.StatusAway);
            SetDefault("StatusDnd", DefaultColors.StatusDnd);
            SetDefault("StatusOffline", DefaultColors.StatusOffline);
        }
 

        public string FrameForeground	    { get { return this["FrameForeground"]; } set { this["FrameForeground"] = value; } }
        public string FrameBackground		{ get { return this["FrameBackground"]; } set { this["FrameBackground"] = value; } }
        public string FrameSecondary		{ get { return this["FrameSecondary"]; } set { this["FrameSecondary"] = value; } }

        public string ContentForeground		{ get { return this["ContentForeground"]; } set { this["ContentForeground"] = value; } }
        public string ContentBackground		{ get { return this["ContentBackground"]; } set { this["ContentBackground"] = value; } }
        public string ContentSecondary		{ get { return this["ContentSecondary"]; } set { this["ContentSecondary"] = value; } }
        public string ContentPopout		    { get { return this["ContentPopout"]; } set { this["ContentPopout"] = value; } }
        public string ContentEnabled        { get { return this["ContentEnabled"]; } set { this["ContentEnabled"] = value; } }
        public string ContentDisabled       { get { return this["ContentDisabled"]; } set { this["ContentDisabled"] = value; } }

        public string ContactListBackground	{ get { return this["ContactListBackground"]; } set { this["ContactListBackground"] = value; } }
        public string ContactListForeground	{ get { return this["ContactListForeground"]; } set { this["ContactListForeground"] = value; } }
        public string ContactListSelected	{ get { return this["ContactListSelected"]; } set { this["ContactListSelected"] = value; } }

        public string HighlightForeground   { get { return this["HighlightForeground"]; } set { this["HighlightForeground"] = value; } }
        public string HighlightImportant    { get { return this["HighlightImportant"]; } set { this["HighlightImportant"] = value; } }
        public string HighlightWarning      { get { return this["HighlightWarning"]; } set { this["HighlightWarning"] = value; } }
        public string HighlightRequest      { get { return this["HighlightRequest"]; } set { this["HighlightRequest"] = value; } }

        public string StatusAvailable	    { get { return this["StatusAvailable"]; } set { this["StatusAvailable"] = value; } }
        public string StatusAway		    { get { return this["StatusAway"]; } set { this["StatusAway"] = value; } }
        public string StatusDnd		        { get { return this["StatusDnd"]; } set { this["StatusDnd"] = value; } }
        public string StatusOffline		    { get { return this["StatusOffline"]; } set { this["StatusOffline"] = value; } }
    }
}
