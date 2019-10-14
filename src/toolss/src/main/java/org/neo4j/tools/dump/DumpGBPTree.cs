using System;

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
namespace Neo4Net.tools.dump
{

	using Neo4Net.Index.@internal.gbptree;
	// import org.neo4j.index.internal.gbptree.TreePrinter;
	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.scheduler.JobSchedulerFactory.createInitializedScheduler;

	/// <summary>
	/// For now only dumps header, could be made more useful over time.
	/// </summary>
	public class DumpGBPTree
	{
		 /// <summary>
		 /// Dumps stuff about a <seealso cref="GBPTree"/> to console in human readable format.
		 /// </summary>
		 /// <param name="args"> arguments. </param>
		 /// <exception cref="IOException"> on I/O error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void main(String[] args) throws java.io.IOException
		 public static void Main( string[] args )
		 {
			  if ( args.Length == 0 )
			  {
					Console.Error.WriteLine( "File argument expected" );
					Environment.Exit( 1 );
			  }
			  Console.WriteLine( " Deprecated tool. You should no longer use this utility." );
			  //File file = new File( args[0] );
			  //System.out.println( "Dumping " + file.getAbsolutePath() );
			  // TreePrinter.printHeader( new DefaultFileSystemAbstraction(), createInitializedScheduler(), file, System.out );

		 }
	}

}