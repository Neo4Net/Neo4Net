using System;

/*
 * Copyright (c) 2002-2018 "Neo Technology,"
 * Network Engine for Objects in Lund AB [http://neotechnology.com]
 *
 * Modifications Copyright (c) 2018-2019 "GraphFoundation" [https://graphfoundation.org]
 *
 * The included source code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html)
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 */

namespace Neo4Net.Server.enterprise
{
	// See https://github.com/neo4j/neo4j/blob/3.2/enterprise/server-enterprise/src/main/java/org/neo4j/server/enterprise/EnterpriseEntryPoint.java

	public class EnterpriseEntryPoint
	{
		 private static Bootstrapper _bootstrapper;

		 /// 
		 private EnterpriseEntryPoint()
		 {
		 }

		 /// <param name="args"> </param>
		 public static void Main( string[] args )
		 {
			  int status = ServerBootstrapper.start( new EnterpriseBootstrapper(), args );
			  if ( status != 0 )
			  {
					Environment.Exit( status );
			  }
		 }

		 /// <param name="args"> </param>
		 public static void Start( string[] args )
		 {
			  _bootstrapper = new BlockingBootstrapper( new EnterpriseBootstrapper() );
			  Environment.Exit( ServerBootstrapper.start( _bootstrapper, args ) );
		 }

		 /// <param name="args"> </param>
		 public static void Stop( string[] args )
		 {
			  if ( _bootstrapper != null )
			  {
					_bootstrapper.stop();
			  }
		 }
	}

}