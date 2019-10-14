using System.Collections.Generic;
using System.Text;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4j Enterprise Edition. The included source
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
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.Kernel.impl.query
{

	using Strings = Neo4Net.Helpers.Strings;
	using QuerySnapshot = Neo4Net.Kernel.api.query.QuerySnapshot;
	using AnyValue = Neo4Net.Values.AnyValue;
	using PrettyPrinter = Neo4Net.Values.utils.PrettyPrinter;
	using MapValue = Neo4Net.Values.@virtual.MapValue;

	internal class QueryLogFormatter
	{
		 private QueryLogFormatter()
		 {
		 }

		 internal static void FormatPageDetails( StringBuilder result, QuerySnapshot query )
		 {
			  result.Append( query.PageHits() ).Append(" page hits, ");
			  result.Append( query.PageFaults() ).Append(" page faults - ");
		 }

		 internal static void FormatAllocatedBytes( StringBuilder result, QuerySnapshot query )
		 {
			  long? bytes = query.AllocatedBytes();
			  if ( bytes != null )
			  {
					result.Append( bytes ).Append( " B - " );
			  }
		 }

		 internal static void FormatDetailedTime( StringBuilder result, QuerySnapshot query )
		 {
			  result.Append( "(planning: " ).Append( TimeUnit.MICROSECONDS.toMillis( query.CompilationTimeMicros() ) );
			  long? cpuTime = TimeUnit.MICROSECONDS.toMillis( query.CpuTimeMicros() );
			  if ( cpuTime != null )
			  {
					result.Append( ", cpu: " ).Append( cpuTime );
			  }
			  result.Append( ", waiting: " ).Append( TimeUnit.MICROSECONDS.toMillis( query.WaitTimeMicros() ) );
			  result.Append( ") - " );
		 }

		 internal static void FormatMapValue( StringBuilder result, MapValue @params, ICollection<string> obfuscate )
		 {
			  result.Append( '{' );
			  if ( @params != null )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String[] sep = new String[]{""};
					string[] sep = new string[]{ "" };
					@params.Foreach((key, value) =>
					{
					 result.Append( sep[0] ).Append( key ).Append( ": " );

					 if ( obfuscate.Contains( key ) )
					 {
						  result.Append( "******" );
					 }
					 else
					 {
						  result.Append( FormatAnyValue( value ) );
					 }
					 sep[0] = ", ";
					});
			  }
			  result.Append( "}" );
		 }

		 internal static string FormatAnyValue( AnyValue value )
		 {
			  PrettyPrinter printer = new PrettyPrinter( "'" );
			  value.WriteTo( printer );
			  return printer.Value();
		 }

		 internal static void FormatMap( StringBuilder result, IDictionary<string, object> @params )
		 {
			  FormatMap( result, @params, Collections.emptySet() );
		 }

		 internal static void FormatMap( StringBuilder result, IDictionary<string, object> @params, ICollection<string> obfuscate )
		 {
			  result.Append( '{' );
			  if ( @params != null )
			  {
					string sep = "";
					foreach ( KeyValuePair<string, object> entry in @params.SetOfKeyValuePairs() )
					{
						 result.Append( sep ).Append( entry.Key ).Append( ": " );

						 if ( obfuscate.Contains( entry.Key ) )
						 {
							  result.Append( "******" );
						 }
						 else
						 {
							  FormatValue( result, entry.Value );
						 }
						 sep = ", ";
					}
			  }
			  result.Append( "}" );
		 }

		 private static void FormatValue( StringBuilder result, object value )
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: if (value instanceof java.util.Map<?,?>)
			  if ( value is IDictionary<object, ?> )
			  {
					FormatMap( result, ( IDictionary<string, object> ) value, Collections.emptySet() );
			  }
			  else if ( value is string )
			  {
					result.Append( '\'' ).Append( value ).Append( '\'' );
			  }
			  else
			  {
					result.Append( Strings.prettyPrint( value ) );
			  }
		 }
	}

}