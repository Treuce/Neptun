using Dna;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml;
using static Dna.FrameworkDI;

namespace Neptun
{
	public class RichTextBoxHelper : DependencyObject
	{
		public static string GetDocumentXaml(DependencyObject obj)
		{
			return (string)obj.GetValue(DocumentXamlProperty);
		}

		public static void SetDocumentXaml(DependencyObject obj, string value)
		{
			obj.SetValue(DocumentXamlProperty, value);
		}

		public static readonly DependencyProperty DocumentXamlProperty =
			DependencyProperty.RegisterAttached(
				"DocumentXaml",
				typeof(string),
				typeof(RichTextBoxHelper),
				new FrameworkPropertyMetadata
				{
					BindsTwoWayByDefault = true,
					PropertyChangedCallback = (obj, e) =>
					{
						var richTextBox = (RichTextBox)obj;

						// Parse the XAML to a document (or use XamlReader.Parse())
						var xaml = GetDocumentXaml(richTextBox);
						//var xaml = XamlReader.Parse()
						var doc = new FlowDocument();
						var range = new TextRange(doc.ContentStart, doc.ContentEnd);
						//var asd = new MemoryStream(Encoding.UTF8.GetBytes(xaml));
						//Debugger.Break();
						try
						{
							range.Load(new MemoryStream(Encoding.UTF8.GetBytes(xaml)), DataFormats.Xaml);
						}
						catch (Exception ex)
						{
							Logger.LogErrorSource(ex.Message);
							Debugger.Break();
						}
						//range.Load(asd, DataFormats.Xaml);
						// Set the document
						richTextBox.Document = doc;

						// When the document changes update the source
						range.Changed += (obj2, e2) =>
							{
								if (richTextBox.Document == doc)
								{
									MemoryStream buffer = new MemoryStream();
									range.Save(buffer, DataFormats.Xaml);
									SetDocumentXaml(richTextBox,
										Encoding.UTF8.GetString(buffer.ToArray()));
								}
							};
					}
				});
	}

	public static class TextBlockHelper
	{
		#region FormattedText Attached dependency property

		public class TextPart
		{
			public String mType = String.Empty;
			public Inline mInline = null;
			public InlineCollection mChildren = null;

			public TextPart() { }
			public TextPart(String t, Inline inline, InlineCollection col)
			{
				mType = t;
				mInline = inline;
				mChildren = col;
			}
		}

		private static Regex mRegex = new Regex(@"<(?<Span>/?[^>]*)>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		private static Regex mSpanRegex = new Regex("(?<Key>[^\\s=]+)=\"(?<Val>[^\\s\"]*)\"", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		public static string GetFormattedText(DependencyObject obj)
		{
			return (string)obj.GetValue(FormattedTextProperty);
		}

		public static void SetFormattedText(DependencyObject obj, string value)
		{
			obj.SetValue(FormattedTextProperty, value);
		}

		public static readonly DependencyProperty FormattedTextProperty =
			DependencyProperty.RegisterAttached("FormattedText",
												typeof(string),
												typeof(TextBlockHelper),
												new UIPropertyMetadata("", FormattedTextChanged));

		private static void FormattedTextChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			TextBlock textBlock = sender as TextBlock;
			FormatText(e.NewValue as string, new TextPart("TextBlock", null, textBlock.Inlines));
		}

		public static void FormatText(String s, TextPart root)
		{
			int len = s.Length;
			int lastIdx = 0;
			List<TextPart> parts = new List<TextPart>();
			parts.Add(root);
			Match m = mRegex.Match(s);
			while (m.Success)
			{
				String tag = m.Result("${Span}");
				if (tag.StartsWith("/"))
				{
					String prevStr = s.Substring(lastIdx, m.Index - lastIdx);
					TextPart part = parts.Last();
					if (!String.IsNullOrEmpty(prevStr))
					{
						if (part.mChildren != null)
						{
							part.mChildren.Add(new Run(prevStr));
						}
						else if (part.mInline is Run)
						{
							(part.mInline as Run).Text = prevStr;
						}
					}
					if (!tag.Substring(1).Equals(part.mType, StringComparison.InvariantCultureIgnoreCase))
					{
						Logger.LogDebugSource("Mismatched End Tag '" + tag.Substring(1) + "' (expected </" + part.mType + ">) at position " + m.Index.ToString() + " in String '" + s + "'");
					}
					if (parts.Count > 1)
					{
						parts.RemoveAt(parts.Count - 1);
						TextPart parentPart = parts.Last();
						if (parentPart.mChildren != null)
						{
							parentPart.mChildren.Add(part.mInline);
						}
					}
				}
				else
				{
					TextPart prevPart = parts.Last();
					String prevStr = s.Substring(lastIdx, m.Index - lastIdx);
					if (!String.IsNullOrEmpty(prevStr))
					{
						if (prevPart.mChildren != null)
						{
							prevPart.mChildren.Add(new Run(prevStr));
						}
						else if (prevPart.mInline is Run)
						{
							(prevPart.mInline as Run).Text = prevStr;
						}
					}

					bool hasAttributes = false;
					TextPart part = new TextPart();
					if (tag.StartsWith("bold", StringComparison.InvariantCultureIgnoreCase))
					{
						part.mType = "BOLD";
						part.mInline = new Bold();
						part.mChildren = (part.mInline as Bold).Inlines;
					}
					else if (tag.StartsWith("underline", StringComparison.InvariantCultureIgnoreCase))
					{
						part.mType = "UNDERLINE";
						part.mInline = new Underline();
						part.mChildren = (part.mInline as Underline).Inlines;
					}
					else if (tag.StartsWith("italic", StringComparison.InvariantCultureIgnoreCase))
					{
						part.mType = "ITALIC";
						part.mInline = new Italic();
						part.mChildren = (part.mInline as Italic).Inlines;
					}
					else if (tag.StartsWith("linebreak", StringComparison.InvariantCultureIgnoreCase))
					{
						part.mType = "LINEBREAK";
						part.mInline = new LineBreak();
					}
					else if (tag.StartsWith("span", StringComparison.InvariantCultureIgnoreCase))
					{
						hasAttributes = true;
						part.mType = "SPAN";
						part.mInline = new Span();
						part.mChildren = (part.mInline as Span).Inlines;
					}
					else if (tag.StartsWith("run", StringComparison.InvariantCultureIgnoreCase))
					{
						hasAttributes = true;
						part.mType = "RUN";
						part.mInline = new Run();
					}
					else if (tag.StartsWith("hyperlink", StringComparison.InvariantCultureIgnoreCase))
					{
						hasAttributes = true;
						part.mType = "HYPERLINK";
						part.mInline = new Hyperlink();
						part.mChildren = (part.mInline as Hyperlink).Inlines;
					}

					if (hasAttributes && part.mInline != null)
					{
						Match m2 = mSpanRegex.Match(tag);
						while (m2.Success)
						{
							String key = m2.Result("${Key}");
							String val = m2.Result("${Val}");
							if (key.Equals("FontWeight", StringComparison.InvariantCultureIgnoreCase))
							{
								FontWeight fw = FontWeights.Normal;
								try
								{
									fw = (FontWeight)new FontWeightConverter().ConvertFromString(val);
								}
								catch (Exception)
								{
									fw = FontWeights.Normal;
								}
								part.mInline.FontWeight = fw;
							}
							else if (key.Equals("FontSize", StringComparison.InvariantCultureIgnoreCase))
							{
								double fs = part.mInline.FontSize;
								if (Double.TryParse(val, out fs))
								{
									part.mInline.FontSize = fs;
								}
							}
							else if (key.Equals("FontStretch", StringComparison.InvariantCultureIgnoreCase))
							{
								FontStretch fs = FontStretches.Normal;
								try
								{
									fs = (FontStretch)new FontStretchConverter().ConvertFromString(val);
								}
								catch (Exception)
								{
									fs = FontStretches.Normal;
								}
								part.mInline.FontStretch = fs;
							}
							else if (key.Equals("FontStyle", StringComparison.InvariantCultureIgnoreCase))
							{
								FontStyle fs = FontStyles.Normal;
								try
								{
									fs = (FontStyle)new FontStyleConverter().ConvertFromString(val);
								}
								catch (Exception)
								{
									fs = FontStyles.Normal;
								}
								part.mInline.FontStyle = fs;
							}
							else if (key.Equals("FontFamily", StringComparison.InvariantCultureIgnoreCase))
							{
								if (!String.IsNullOrEmpty(val))
								{
									FontFamily ff = new FontFamily(val);
									if (Fonts.SystemFontFamilies.Contains(ff))
									{
										part.mInline.FontFamily = ff;
									}
								}
							}
							else if (key.Equals("Background", StringComparison.InvariantCultureIgnoreCase))
							{
								Brush b = part.mInline.Background;
								try
								{
									b = (Brush)new BrushConverter().ConvertFromString(val);
								}
								catch (Exception)
								{
									b = part.mInline.Background;
								}
								part.mInline.Background = b;
							}
							else if (key.Equals("Foreground", StringComparison.InvariantCultureIgnoreCase))
							{
								Brush b = part.mInline.Foreground;
								try
								{
									b = (Brush)new BrushConverter().ConvertFromString(val);
								}
								catch (Exception)
								{
									b = part.mInline.Foreground;
								}
								part.mInline.Foreground = b;
							}
							else if (key.Equals("ToolTip", StringComparison.InvariantCultureIgnoreCase))
							{
								part.mInline.ToolTip = val;
							}
							else if (key.Equals("Text", StringComparison.InvariantCultureIgnoreCase) && part.mInline is Run)
							{
								(part.mInline as Run).Text = val;
							}
							else if (key.Equals("NavigateUri", StringComparison.InvariantCultureIgnoreCase) && part.mInline is Hyperlink)
							{
								(part.mInline as Hyperlink).NavigateUri = new Uri(val);
							}
							m2 = m2.NextMatch();
						}
					}

					if (part.mInline != null)
					{
						if (tag.TrimEnd().EndsWith("/"))
						{
							if (prevPart.mChildren != null)
							{
								prevPart.mChildren.Add(part.mInline);
							}
						}
						else
						{
							parts.Add(part);
						}
					}
				}
				lastIdx = m.Index + m.Length;
				m = m.NextMatch();
			}
			if (lastIdx < (len - 1))
			{
				root.mChildren.Add(new Run(s.Substring(lastIdx)));
			}
		}
		#endregion
	}
}
