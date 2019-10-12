/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Neo4Net.Csv.Reader
{
	/// <summary>
	/// Configuration options around reading CSV data, or similar.
	/// </summary>
	public interface Configuration
	{
		 /// <summary>
		 /// TODO: Our intention is to flip this to false (which means to comply with RFC4180) at some point
		 /// because of how it better complies with common expectancy of behavior. It may be least disruptive
		 /// to do this when changing major version of the product.
		 /// </summary>

		 /// <summary>
		 /// Character to regard as quotes. Quoted values can contain newline characters and even delimiters.
		 /// </summary>
		 char QuotationCharacter();

		 /// <summary>
		 /// Data buffer size.
		 /// </summary>
		 int BufferSize();

		 /// <summary>
		 /// Whether or not fields are allowed to have newline characters in them, i.e. span multiple lines.
		 /// </summary>
		 bool MultilineFields();

		 /// <summary>
		 /// Whether or not strings should be trimmed for whitespaces.
		 /// </summary>
		 bool TrimStrings();

		 /// <returns> {@code true} for treating empty strings, i.e. {@code ""} as null, instead of an empty string. </returns>
		 bool EmptyQuotedStringsAsNull();

		 /// <summary>
		 /// Adds a default implementation returning <seealso cref="DEFAULT_LEGACY_STYLE_QUOTING"/>, this to not requiring
		 /// any change to other classes using this interface.
		 /// </summary>
		 /// <returns> whether or not the parsing will interpret <code>\"</code> (see <seealso cref="quotationCharacter()"/>)
		 /// as an inner quote. Reason why this is configurable is that this interpretation conflicts with
		 /// "standard" RFC for CSV parsing, see https://tools.ietf.org/html/rfc4180. This also makes it impossible
		 /// to enter some combinations of characters, e.g. <code>"""abc\"""</code>, when expecting <code>"abc\"</code>. </returns>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default boolean legacyStyleQuoting()
	//	 {
	//		  return DEFAULT_LEGACY_STYLE_QUOTING;
	//	 }
	}

	public static class Configuration_Fields
	{
		 public const bool DEFAULT_LEGACY_STYLE_QUOTING = true;
		 public const int KB = 1024;
		 public static readonly int Mb = KB * KB;
		 public static readonly int DefaultBufferSize_4mb = 4 * Mb;
		 public static readonly Configuration Default = new Default();
	}

	 public class Configuration_Default : Configuration
	 {
		  public override char QuotationCharacter()
		  {
				return '"';
		  }

		  public override int BufferSize()
		  {
				return Configuration_Fields.DefaultBufferSize_4mb;
		  }

		  public override bool MultilineFields()
		  {
				return false;
		  }

		  public override bool EmptyQuotedStringsAsNull()
		  {
				return false;
		  }

		  public override bool TrimStrings()
		  {
				return false;
		  }
	 }

	 public class Configuration_Overridden : Configuration
	 {
		  internal readonly Configuration Defaults;

		  public Configuration_Overridden( Configuration defaults )
		  {
				this.Defaults = defaults;
		  }

		  public override char QuotationCharacter()
		  {
				return Defaults.quotationCharacter();
		  }

		  public override int BufferSize()
		  {
				return Defaults.bufferSize();
		  }

		  public override bool MultilineFields()
		  {
				return Defaults.multilineFields();
		  }

		  public override bool EmptyQuotedStringsAsNull()
		  {
				return Defaults.emptyQuotedStringsAsNull();
		  }

		  public override bool TrimStrings()
		  {
				return Defaults.trimStrings();
		  }

		  public override bool LegacyStyleQuoting()
		  {
				return Defaults.legacyStyleQuoting();
		  }
	 }

}