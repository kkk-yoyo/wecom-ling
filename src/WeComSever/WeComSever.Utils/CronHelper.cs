using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WeComSever.Utils;

public class CronHelper
{
	public static readonly List<CronPreset> Presets = new List<CronPreset>
	{
		new CronPreset("每天凌晨3点", "0 3 * * *", "每天凌晨 03:00 执行"),
		new CronPreset("每分钟", "* * * * *", "每分钟执行一次"),
		new CronPreset("每5分钟", "*/5 * * * *", "每5分钟执行一次"),
		new CronPreset("每10分钟", "*/10 * * * *", "每10分钟执行一次"),
		new CronPreset("每15分钟", "*/15 * * * *", "每15分钟执行一次"),
		new CronPreset("每30分钟", "*/30 * * * *", "每30分钟执行一次"),
		new CronPreset("每小时", "0 * * * *", "每小时整点执行"),
		new CronPreset("每2小时", "0 */2 * * *", "每2小时执行一次"),
		new CronPreset("每6小时", "0 */6 * * *", "每6小时执行一次"),
		new CronPreset("每12小时", "0 */12 * * *", "每12小时执行一次"),
		new CronPreset("每天凌晨", "0 0 * * *", "每天凌晨 00:00 执行"),
		new CronPreset("每天早上6点", "0 6 * * *", "每天早上 06:00 执行"),
		new CronPreset("每天中午12点", "0 12 * * *", "每天中午 12:00 执行"),
		new CronPreset("每天晚上6点", "0 18 * * *", "每天晚上 18:00 执行"),
		new CronPreset("每天晚上11点", "0 23 * * *", "每天晚上 23:00 执行"),
		new CronPreset("每周一凌晨", "0 0 * * 1", "每周一凌晨 00:00 执行"),
		new CronPreset("每月1号凌晨", "0 0 1 * *", "每月1号凌晨 00:00 执行")
	};

	public static bool IsValid(string expression)
	{
		if (string.IsNullOrWhiteSpace(expression))
		{
			return false;
		}
		try
		{
			string[] array = expression.Trim().Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			if (array.Length != 5)
			{
				return false;
			}
			if (!ValidateField(array[0], 0, 59))
			{
				return false;
			}
			if (!ValidateField(array[1], 0, 23))
			{
				return false;
			}
			if (!ValidateField(array[2], 1, 31))
			{
				return false;
			}
			if (!ValidateField(array[3], 1, 12))
			{
				return false;
			}
			if (!ValidateField(array[4], 0, 6))
			{
				return false;
			}
			return true;
		}
		catch
		{
			return false;
		}
	}

	private static bool ValidateField(string field, int min, int max)
	{
		if (field == "*")
		{
			return true;
		}
		if (field.StartsWith("*/"))
		{
			if (int.TryParse(field.Substring(2), out var result))
			{
				if (result >= 1)
				{
					return result <= max;
				}
				return false;
			}
			return false;
		}
		if (field.Contains("-"))
		{
			string[] array = field.Split('-');
			if (array.Length == 2 && int.TryParse(array[0], out var result2) && int.TryParse(array[1], out var result3))
			{
				if (result2 >= min && result3 <= max)
				{
					return result2 <= result3;
				}
				return false;
			}
			return false;
		}
		if (field.Contains(","))
		{
			string[] array2 = field.Split(',');
			for (int i = 0; i < array2.Length; i++)
			{
				if (!int.TryParse(array2[i].Trim(), out var result4) || result4 < min || result4 > max)
				{
					return false;
				}
			}
			return true;
		}
		if (int.TryParse(field, out var result5))
		{
			if (result5 >= min)
			{
				return result5 <= max;
			}
			return false;
		}
		return false;
	}

	public static DateTime? GetNextOccurrence(string expression, DateTime from)
	{
		if (!IsValid(expression))
		{
			return null;
		}
		try
		{
			string[] array = expression.Trim().Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			string minuteField = array[0];
			string text = array[1];
			string text2 = array[2];
			string text3 = array[3];
			string text4 = array[4];
			DateTime dateTime = from.AddMinutes(1.0);
			dateTime = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, 0);
			DateTime dateTime2 = from.AddYears(1);
			while (dateTime < dateTime2)
			{
				if (Matches(dateTime, minuteField, text, text2, text3, text4))
				{
					return dateTime;
				}
				dateTime = (FieldMatches(text3, dateTime.Month, 1, 12) ? ((FieldMatches(text2, dateTime.Day, 1, 31) && WeekdayMatches(text4, dateTime)) ? (FieldMatches(text, dateTime.Hour, 0, 23) ? dateTime.AddMinutes(1.0) : new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, 0, 0).AddHours(1.0)) : new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 0, 0, 0).AddDays(1.0)) : new DateTime(dateTime.Year, dateTime.Month, 1, 0, 0, 0).AddMonths(1));
			}
			return null;
		}
		catch
		{
			return null;
		}
	}

	private static bool Matches(DateTime time, string minuteField, string hourField, string dayField, string monthField, string weekdayField)
	{
		if (FieldMatches(minuteField, time.Minute, 0, 59) && FieldMatches(hourField, time.Hour, 0, 23) && FieldMatches(dayField, time.Day, 1, 31) && FieldMatches(monthField, time.Month, 1, 12))
		{
			return WeekdayMatches(weekdayField, time);
		}
		return false;
	}

	private static bool FieldMatches(string field, int value, int min, int max)
	{
		if (field == "*")
		{
			return true;
		}
		if (field.StartsWith("*/"))
		{
			if (int.TryParse(field.Substring(2), out var result) && result > 0)
			{
				return (value - min) % result == 0;
			}
			return false;
		}
		if (field.Contains("-"))
		{
			string[] array = field.Split('-');
			if (array.Length == 2 && int.TryParse(array[0], out var result2) && int.TryParse(array[1], out var result3))
			{
				if (value >= result2)
				{
					return value <= result3;
				}
				return false;
			}
			return false;
		}
		if (field.Contains(","))
		{
			string[] array2 = field.Split(',');
			for (int i = 0; i < array2.Length; i++)
			{
				if (int.TryParse(array2[i].Trim(), out var result4) && result4 == value)
				{
					return true;
				}
			}
			return false;
		}
		if (int.TryParse(field, out var result5))
		{
			return result5 == value;
		}
		return false;
	}

	private static bool WeekdayMatches(string field, DateTime time)
	{
		int dayOfWeek = (int)time.DayOfWeek;
		return FieldMatches(field, dayOfWeek, 0, 6);
	}

	public static string GetDescription(string expression)
	{
		if (string.IsNullOrWhiteSpace(expression))
		{
			return "无效的表达式";
		}
		CronPreset cronPreset = Presets.FirstOrDefault((CronPreset p) => p.Expression == expression);
		if (cronPreset != null && cronPreset.Name != "自定义")
		{
			return cronPreset.Description;
		}
		if (!IsValid(expression))
		{
			return "无效的表达式";
		}
		try
		{
			string[] array = expression.Trim().Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			string text = array[0];
			string text2 = array[1];
			string text3 = array[2];
			string text4 = array[3];
			string text5 = array[4];
			StringBuilder stringBuilder = new StringBuilder();
			if (text == "*" && text2 == "*" && text3 == "*" && text4 == "*" && text5 == "*")
			{
				return "每分钟执行";
			}
			if (text.StartsWith("*/"))
			{
				stringBuilder.Append("每" + text.Substring(2) + "分钟");
			}
			else if (text != "*")
			{
				stringBuilder.Append("第" + text + "分");
			}
			if (text2.StartsWith("*/"))
			{
				if (stringBuilder.Length > 0)
				{
					stringBuilder.Append(", ");
				}
				stringBuilder.Append("每" + text2.Substring(2) + "小时");
			}
			else if (text2 != "*")
			{
				if (stringBuilder.Length > 0)
				{
					stringBuilder.Append(", ");
				}
				stringBuilder.Append(text2 + "时");
			}
			if (text3 != "*")
			{
				if (stringBuilder.Length > 0)
				{
					stringBuilder.Append(", ");
				}
				stringBuilder.Append("每月" + text3 + "号");
			}
			if (text4 != "*")
			{
				if (stringBuilder.Length > 0)
				{
					stringBuilder.Append(", ");
				}
				stringBuilder.Append(text4 + "月");
			}
			if (text5 != "*")
			{
				if (stringBuilder.Length > 0)
				{
					stringBuilder.Append(", ");
				}
				stringBuilder.Append("周" + GetWeekdayName(text5));
			}
			if (stringBuilder.Length == 0)
			{
				stringBuilder.Append("按计划执行");
			}
			return stringBuilder.ToString();
		}
		catch
		{
			return "无效的表达式";
		}
	}

	private static string GetWeekdayName(string weekday)
	{
		string[] array = new string[7] { "日", "一", "二", "三", "四", "五", "六" };
		if (int.TryParse(weekday, out var result) && result >= 0 && result <= 6)
		{
			return array[result];
		}
		return weekday;
	}

	public static string FormatNextOccurrence(DateTime? nextRun)
	{
		if (!nextRun.HasValue)
		{
			return "无法计算";
		}
		TimeSpan timeSpan = nextRun.Value - DateTime.Now;
		if (timeSpan.TotalSeconds <= 0.0)
		{
			return "即将执行...";
		}
		if (timeSpan.TotalDays >= 1.0)
		{
			return $"{(int)timeSpan.TotalDays}天 {timeSpan.Hours}小时 {timeSpan.Minutes}分钟后";
		}
		if (timeSpan.TotalHours >= 1.0)
		{
			return $"{timeSpan.Hours}小时 {timeSpan.Minutes}分钟 {timeSpan.Seconds}秒后";
		}
		if (timeSpan.TotalMinutes >= 1.0)
		{
			return $"{timeSpan.Minutes}分钟 {timeSpan.Seconds}秒后";
		}
		return $"{timeSpan.Seconds}秒后";
	}
}
