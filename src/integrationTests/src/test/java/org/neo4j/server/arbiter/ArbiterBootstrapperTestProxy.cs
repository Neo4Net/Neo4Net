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
namespace Neo4Net.Server.arbiter
{

	using ArbiterBootstrapper = Neo4Net.Server.enterprise.ArbiterBootstrapper;

	public class ArbiterBootstrapperTestProxy
	{
		 public const string START_SIGNAL = "starting";

		 private ArbiterBootstrapperTestProxy()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void main(String[] argv) throws java.io.IOException
		 public static void Main( string[] argv )
		 {
			  ServerCommandLineArgs args = ServerCommandLineArgs.parse( argv );

			  // This sysout will be intercepted by the parent process and will trigger
			  // a start of a timeout. The whole reason for this class to be here is to
			  // split awaiting for the process to start and actually awaiting the cluster client to start.
			  Console.WriteLine( START_SIGNAL );
			  using ( ArbiterBootstrapper arbiter = new ArbiterBootstrapper() )
			  {
					arbiter.Start( args.HomeDir(), args.ConfigFile(), Collections.emptyMap() );
					Console.Read();
			  }
		 }
	}

}