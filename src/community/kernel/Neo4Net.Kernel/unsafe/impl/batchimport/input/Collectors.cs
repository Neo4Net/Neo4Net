/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
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
namespace Neo4Net.@unsafe.Impl.Batchimport.input
{

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.io.NullOutputStream.NULL_OUTPUT_STREAM;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.@unsafe.impl.batchimport.input.BadCollector.BAD_RELATIONSHIPS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.@unsafe.impl.batchimport.input.BadCollector.COLLECT_ALL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.@unsafe.impl.batchimport.input.BadCollector.DEFAULT_BACK_PRESSURE_THRESHOLD;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.@unsafe.impl.batchimport.input.BadCollector.DUPLICATE_NODES;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.@unsafe.impl.batchimport.input.BadCollector.EXTRA_COLUMNS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.@unsafe.impl.batchimport.input.BadCollector.NO_MONITOR;

	/// <summary>
	/// Common implementations of <seealso cref="Collector"/>
	/// </summary>
	public class Collectors
	{
		 private Collectors()
		 {
		 }

		 public static Collector SilentBadCollector( long tolerance )
		 {
			  return SilentBadCollector( tolerance, COLLECT_ALL );
		 }

		 public static Collector SilentBadCollector( long tolerance, int collect )
		 {
			  return BadCollector( NULL_OUTPUT_STREAM, tolerance, collect );
		 }

		 public static Collector BadCollector( Stream @out, long unlimitedTolerance )
		 {
			  return BadCollector( @out, unlimitedTolerance, COLLECT_ALL, false );
		 }

		 public static Collector BadCollector( Stream @out, long tolerance, int collect )
		 {
			  return new BadCollector( @out, tolerance, collect, DEFAULT_BACK_PRESSURE_THRESHOLD, false, NO_MONITOR );
		 }

		 public static Collector BadCollector( Stream @out, long unlimitedTolerance, int collect, bool skipBadEntriesLogging )
		 {
			  return new BadCollector( @out, unlimitedTolerance, collect, DEFAULT_BACK_PRESSURE_THRESHOLD, skipBadEntriesLogging, NO_MONITOR );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static System.Func<java.io.OutputStream,Collector> badCollector(final int tolerance)
		 public static System.Func<Stream, Collector> BadCollector( int tolerance )
		 {
			  return badCollector( tolerance, COLLECT_ALL );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static System.Func<java.io.OutputStream,Collector> badCollector(final int tolerance, final int collect)
		 public static System.Func<Stream, Collector> BadCollector( int tolerance, int collect )
		 {
			  return @out => BadCollector( @out, tolerance, collect, false );
		 }

		 public static int Collect( bool skipBadRelationships, bool skipDuplicateNodes, bool ignoreExtraColumns )
		 {
			  return ( skipBadRelationships ? BAD_RELATIONSHIPS : 0 ) | ( skipDuplicateNodes ? DUPLICATE_NODES : 0 ) | ( ignoreExtraColumns ? EXTRA_COLUMNS : 0 );
		 }
	}

}