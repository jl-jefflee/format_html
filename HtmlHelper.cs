using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace magic.html
{
    public class HtmlHelper
    {
        public static Char[] Whitespaces = new Char[]{'\r','\n','\t',' '};

        public static Boolean IsWhitespace(Char c)
        {
            foreach (Char ws in Whitespaces)
            {
                if (ws == c)
                {
                    return true;
                }
            }
            return false;
        }
        public static Boolean IsWhitespace(String str)
        {
            foreach (Char c in str)
            {
                if (!IsWhitespace(c))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 判断是否为html有效标示符（只包含字母，数字，下划线，中线）
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static Boolean IsHtmlToken(String str)
        {
            for(int i=0;i<str.Length;i++)
            {
                if(!char.IsLetterOrDigit(str[i])&&str[i]!='_'&&str[i]!='-')
                {
                    return false;
                }
            }
            return true;
        }
        public static String GetHtmlToken(String str)
        {
            String token = "";
            foreach(var c in str)
            {
                if (!char.IsLetterOrDigit(c) && c != '_' && c != '-')
                {
                    continue;
                }
                else
                {
                    token += c.ToString();
                }
            }


            return token;
        }

        public static String FormatHtml(String html)
        {
            FormatHtml formatHtml = new FormatHtml(html);
            return formatHtml.Format();
        }

        public static String FormatHtml(String html, FormatSetting setting)
        {
            FormatHtml formatHtml = new FormatHtml(html, setting);
            return formatHtml.Format();
        }

        /// <summary>
        /// 过滤掉无用的控制字符
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static String FilterControlChar(String html,out List<char> filters)
        {
            /*
00000000 	0 	00 	NUL(null) 	空字符
00000001 	1 	01 	SOH(start of headling) 	标题开始
00000010 	2 	02 	STX (start of text) 	正文开始
00000011 	3 	03 	ETX (end of text) 	正文结束
00000100 	4 	04 	EOT (end of transmission) 	传输结束
00000101 	5 	05 	ENQ (enquiry) 	请求
00000110 	6 	06 	ACK (acknowledge) 	收到通知
00000111 	7 	07 	BEL (bell) 	响铃
00001000 	8 	08 	BS (backspace) 	退格
00001001 	9 	09 	HT (horizontal tab) 	水平制表符
00001010 	10 	0A 	LF (NL line feed, new line) 	换行键
00001011 	11 	0B 	VT (vertical tab) 	垂直制表符
00001100 	12 	0C 	FF (NP form feed, new page) 	换页键
00001101 	13 	0D 	CR (carriage return) 	回车键
00001110 	14 	0E 	SO (shift out) 	不用切换
00001111 	15 	0F 	SI (shift in) 	启用切换
00010000 	16 	10 	DLE (data link escape) 	数据链路转义
00010001 	17 	11 	DC1 (device control 1) 	设备控制1
00010010 	18 	12 	DC2 (device control 2) 	设备控制2
00010011 	19 	13 	DC3 (device control 3) 	设备控制3
00010100 	20 	14 	DC4 (device control 4) 	设备控制4
00010101 	21 	15 	NAK (negative acknowledge) 	拒绝接收
00010110 	22 	16 	SYN (synchronous idle) 	同步空闲
00010111 	23 	17 	ETB (end of trans. block) 	传输块结束
00011000 	24 	18 	CAN (cancel) 	取消
00011001 	25 	19 	EM (end of medium) 	介质中断
00011010 	26 	1A 	SUB (substitute) 	替补
00011011 	27 	1B 	ESC (escape) 	溢出
00011100 	28 	1C 	FS (file separator) 	文件分割符
00011101 	29 	1D 	GS (group separator) 	分组符
00011110 	30 	1E 	RS (record separator) 	记录分离符
00011111 	31 	1F 	US (unit separator) 	单元分隔符
00100000 	32 	20 	(space) 	空格 */
            StringBuilder sb = new StringBuilder();
            filters = new List<char>();
            foreach(char c in html)
            {
                int i=(int)c;
                if((i>=0&&i<=8)||i==11||i==12||(i>=14&&i<=31))
                {
                    filters.Add(c);
                }
                else
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }

        
    }
}
