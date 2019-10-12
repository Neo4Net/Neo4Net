using System;

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
namespace Neo4Net.Server
{
	public class CommunityEntryPoint
	{
		 private static Bootstrapper _bootstrapper;

		 private CommunityEntryPoint()
		 {
		 }

		 public static void Main( string[] args )
		 {
			  int status = ServerBootstrapper.Start( new CommunityBootstrapper(), args );
			  if ( status != 0 )
			  {
					Environment.Exit( status );
			  }
		 }

		 /// <summary>
		 /// Used by the windows service wrapper
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") public static void start(String[] args)
		 public static void Start( string[] args )
		 {
			  _bootstrapper = new BlockingBootstrapper( new CommunityBootstrapper() );
			  Environment.Exit( ServerBootstrapper.Start( _bootstrapper, args ) );
		 }

		 /// <summary>
		 /// Used by the windows service wrapper
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") public static void stop(@SuppressWarnings("UnusedParameters") String[] args)
		 public static void Stop( string[] args )
		 {
			  if ( _bootstrapper != null )
			  {
					_bootstrapper.stop();
			  }
		 }
	}

}