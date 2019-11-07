using System;
using System.Collections.Generic;
using System.Text;

/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.tools.console.input
{
	using Cli = io.airlift.airline.Cli;
	using Help = io.airlift.airline.Help;


	using NullOutputStream = Neo4Net.Io.NullOutputStream;
	using Neo4Net.Kernel.impl.util;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.ArrayUtil.join;

	public class ConsoleUtil
	{
		 public static readonly Listener<PrintStream> NoPrompt = @out =>
		 { // Do nothing
		 };

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static Neo4Net.kernel.impl.util.Listener<java.io.PrintStream> staticPrompt(final String prompt)
		 public static Listener<PrintStream> StaticPrompt( string prompt )
		 {
			  return @out => @out.print( prompt );
		 }

		 public static readonly PrintStream NullPrintStream = new PrintStream( NullOutputStream.NULL_OUTPUT_STREAM );

		 public static string[] TokenizeStringWithQuotes( string @string )
		 {
			  List<string> result = new List<string>();
			  @string = @string.Trim();
			  bool inside = @string.StartsWith( "\"", StringComparison.Ordinal );
			  StringTokenizer quoteTokenizer = new StringTokenizer( @string, "\"" );
			  while ( quoteTokenizer.hasMoreTokens() )
			  {
					string token = quoteTokenizer.nextToken();
					token = token.Trim();
					if ( token.Length == 0 )
					{
						 // Skip it
					}
					else if ( inside )
					{
						 // Don't split
						 result.Add( token );
					}
					else
					{
						 SplitAndKeepEscapedSpaces( token, false, result );
					}
					inside = !inside;
			  }
			  return result.ToArray();
		 }

		 private static void SplitAndKeepEscapedSpaces( string @string, bool preserveEscapes, ICollection<string> into )
		 {
			  StringBuilder current = new StringBuilder();
			  for ( int i = 0; i < @string.Length; i++ )
			  {
					char ch = @string[i];
					if ( ch == ' ' )
					{
						 bool isGluedSpace = i > 0 && @string[i - 1] == '\\';
						 if ( !isGluedSpace )
						 {
							  into.Add( current.ToString() );
							  current = new StringBuilder();
							  continue;
						 }
					}

					if ( preserveEscapes || ch != '\\' )
					{
						 current.Append( ch );
					}
			  }
			  if ( current.Length > 0 )
			  {
					into.Add( current.ToString() );
			  }
		 }

		 public static Stream OneCommand( params string[] args )
		 {
			  string @string = join( args, " " );
			  return new MemoryStream( @string.GetBytes() );
		 }

		 public static string AirlineHelp<T1>( Cli<T1> cli )
		 {
			  StringBuilder builder = new StringBuilder();
			  Help.help( cli.Metadata, Collections.emptyList(), builder );
			  return builder.ToString();
		 }

		 private ConsoleUtil()
		 {
		 }
	}

}