using System;

namespace DragonIdle
{
    /// <summary>
    /// 大きな数値を日本語の位取り（万・億・兆…）で読みやすく整形する。
    /// 放置ゲームでは桁がすぐ伸びるので、表示は常にここを通す。
    /// </summary>
    public static class NumberFormat
    {
        static readonly string[] Units =
        {
            "", "万", "億", "兆", "京", "垓", "秭", "穣", "溝", "澗", "正", "載", "極"
        };

        /// <summary>ゴールドなどの通貨表示。1万未満はそのまま、それ以上は「1.23億」形式。</summary>
        public static string Gold(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value)) return "0";
            if (value < 0) return "-" + Gold(-value);
            if (value < 10000) return Math.Floor(value).ToString("#,0");

            int unit = 0;
            double v = value;
            while (v >= 10000.0 && unit < Units.Length - 1)
            {
                v /= 10000.0;
                unit++;
            }

            // 桁が上がりきってもまだ大きい場合は指数表記に逃がす。
            if (v >= 10000.0) return value.ToString("0.00e+0");

            string body;
            if (v >= 1000.0) body = v.ToString("0");
            else if (v >= 100.0) body = v.ToString("0.#");
            else body = v.ToString("0.##");
            return body + Units[unit];
        }

        /// <summary>毎秒の生産量。細かい値でも0に見えないよう小数を残す。</summary>
        public static string Rate(double perSecond)
        {
            if (perSecond > 0.0 && perSecond < 10.0) return perSecond.ToString("0.##");
            return Gold(perSecond);
        }

        /// <summary>倍率を「+120%」のように表示する。</summary>
        public static string Percent(double multiplierMinusOne)
        {
            return (multiplierMinusOne >= 0 ? "+" : "") + (multiplierMinusOne * 100.0).ToString("0.#") + "%";
        }

        /// <summary>経過秒数を「3時間24分」形式にする。放置報酬の表示用。</summary>
        public static string Duration(double seconds)
        {
            if (seconds < 60) return Math.Floor(seconds).ToString("0") + "秒";
            int total = (int)Math.Floor(seconds);
            int days = total / 86400;
            int hours = (total % 86400) / 3600;
            int minutes = (total % 3600) / 60;
            if (days > 0) return days + "日" + hours + "時間";
            if (hours > 0) return hours + "時間" + minutes + "分";
            return minutes + "分";
        }
    }
}
