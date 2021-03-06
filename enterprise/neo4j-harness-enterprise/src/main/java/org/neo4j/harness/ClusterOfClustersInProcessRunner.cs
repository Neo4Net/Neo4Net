﻿using System;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
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
namespace Org.Neo4j.Harness
{

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.logging.FormattedLogProvider.toOutputStream;

	public class ClusterOfClustersInProcessRunner
	{

		 public static void Main( string[] args )
		 {
			  try
			  {
					Path clusterPath = Files.createTempDirectory( "causal-cluster" );
					Console.WriteLine( "clusterPath = " + clusterPath );

					CausalClusterInProcessBuilder.CausalCluster cluster = CausalClusterInProcessBuilder.Init().withCores(9).withReplicas(6).withLogger(toOutputStream(System.out)).atPath(clusterPath).withOptionalDatabases(Arrays.asList("foo", "bar", "baz")).build();

					Console.WriteLine( "Waiting for cluster to boot up..." );
					cluster.Boot();

					Console.WriteLine( "Press ENTER to exit ..." );
					//noinspection ResultOfMethodCallIgnored
					Console.Read();

					Console.WriteLine( "Shutting down..." );
					cluster.Shutdown();
			  }
			  catch ( Exception e )
			  {
					Console.WriteLine( e.ToString() );
					Console.Write( e.StackTrace );
					Environment.Exit( -1 );
			  }
			  Environment.Exit( 0 );
		 }

	}

}