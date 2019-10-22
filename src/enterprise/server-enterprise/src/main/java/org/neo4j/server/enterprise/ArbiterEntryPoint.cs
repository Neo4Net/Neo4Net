using System;

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
namespace Neo4Net.Server.enterprise
{


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.commandline.Util.Neo4NetVersion;

	public class ArbiterEntryPoint
	{
		 private static Bootstrapper _bootstrapper;

		 private ArbiterEntryPoint()
		 {
		 }

		 public static void Main( string[] argv )
		 {
			  ServerCommandLineArgs args = ServerCommandLineArgs.parse( argv );
			  if ( args.Version() )
			  {
					Console.WriteLine( "Neo4Net " + Neo4NetVersion() );
			  }
			  else
			  {
					int status = ( new ArbiterBootstrapper() ).Start(args.HomeDir(), args.ConfigFile(), Collections.emptyMap());
					if ( status != 0 )
					{
						 Environment.Exit( status );
					}
			  }
		 }

		 /// <summary>
		 /// Used by the windows service wrapper
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") public static void start(String[] args)
		 public static void Start( string[] args )
		 {
			  _bootstrapper = new BlockingBootstrapper( new ArbiterBootstrapper() );
			  Environment.Exit( ServerBootstrapper.start( _bootstrapper, args ) );
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