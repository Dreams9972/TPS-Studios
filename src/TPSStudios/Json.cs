using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace TPSStudios;

internal static class Json
{
	public static string Ecrire(object o)
	{
		StringBuilder stringBuilder = new StringBuilder();
		E(o, stringBuilder, 0);
		stringBuilder.Append("\n");
		return stringBuilder.ToString();
	}

	private static void Ind(StringBuilder b, int n)
	{
		b.Append(' ', n * 2);
	}

	private static void Chaine(string s, StringBuilder b)
	{
		b.Append('"');
		foreach (char c in s)
		{
			if (c == '"')
			{
				b.Append("\\\"");
			}
			else if (c == '\\')
			{
				b.Append("\\\\");
			}
			else if (c == '\n')
			{
				b.Append("\\n");
			}
			else if (c == '\r')
			{
				b.Append("\\r");
			}
			else if (c == '\t')
			{
				b.Append("\\t");
			}
			else if (c < ' ')
			{
				int num = c;
				b.Append("\\u" + num.ToString("x4"));
			}
			else
			{
				b.Append(c);
			}
		}
		b.Append('"');
	}

	private static void E(object o, StringBuilder b, int n)
	{
		if (o == null)
		{
			b.Append("null");
		}
		else if (o is string s)
		{
			Chaine(s, b);
		}
		else if (o is bool)
		{
			b.Append(((bool)o) ? "true" : "false");
		}
		else if (o is int || o is long || o is short || o is byte || o is double || o is float || o is decimal)
		{
			b.Append(Convert.ToString(o, CultureInfo.InvariantCulture));
		}
		else if (o is IDictionary<string, object> dictionary)
		{
			if (dictionary.Count == 0)
			{
				b.Append("{}");
				return;
			}
			b.Append("{\n");
			int num = 0;
			foreach (KeyValuePair<string, object> item in dictionary)
			{
				Ind(b, n + 1);
				Chaine(item.Key, b);
				b.Append(": ");
				E(item.Value, b, n + 1);
				if (++num < dictionary.Count)
				{
					b.Append(",");
				}
				b.Append("\n");
			}
			Ind(b, n);
			b.Append("}");
		}
		else if (o is IEnumerable enumerable)
		{
			List<object> list = new List<object>();
			foreach (object item2 in enumerable)
			{
				list.Add(item2);
			}
			if (list.Count == 0)
			{
				b.Append("[]");
				return;
			}
			b.Append("[\n");
			for (int i = 0; i < list.Count; i++)
			{
				Ind(b, n + 1);
				E(list[i], b, n + 1);
				if (i < list.Count - 1)
				{
					b.Append(",");
				}
				b.Append("\n");
			}
			Ind(b, n);
			b.Append("]");
		}
		else
		{
			Chaine(o.ToString(), b);
		}
	}
}
