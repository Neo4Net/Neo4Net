﻿using System.Threading;

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
namespace Org.Neo4j.Test.server
{

	using NeoServer = Org.Neo4j.Server.NeoServer;
	using CommunityServerBuilder = Org.Neo4j.Server.helpers.CommunityServerBuilder;
	using ServerHelper = Org.Neo4j.Server.helpers.ServerHelper;

	internal sealed class ServerHolder : Thread
	{
		 private static AssertionError _allocation;
		 private static NeoServer _server;
		 private static CommunityServerBuilder _builder;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: static synchronized org.neo4j.server.NeoServer allocate() throws java.io.IOException
		 internal static NeoServer Allocate()
		 {
			 lock ( typeof( ServerHolder ) )
			 {
				  if ( _allocation != null )
				  {
						throw _allocation;
				  }
				  if ( _server == null )
				  {
						_server = StartServer();
				  }
				  _allocation = new AssertionError( "The server was allocated from here but not released properly" );
				  return _server;
			 }
		 }

		 internal static void Release( NeoServer server )
		 {
			 lock ( typeof( ServerHolder ) )
			 {
				  if ( server == null )
				  {
						return;
				  }
				  if ( server != ServerHolder._server )
				  {
						throw new AssertionError( "trying to suspend a server not allocated from here" );
				  }
				  if ( _allocation == null )
				  {
						throw new AssertionError( "releasing the server although it is not allocated" );
				  }
				  _allocation = null;
			 }
		 }

		 internal static void EnsureNotRunning()
		 {
			 lock ( typeof( ServerHolder ) )
			 {
				  if ( _allocation != null )
				  {
						throw _allocation;
				  }
				  Shutdown();
			 }
		 }

		 internal static void SetServerBuilderProperty( string key, string value )
		 {
			 lock ( typeof( ServerHolder ) )
			 {
				  InitBuilder();
				  _builder = _builder.withProperty( key, value );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static org.neo4j.server.NeoServer startServer() throws java.io.IOException
		 private static NeoServer StartServer()
		 {
			  InitBuilder();
			  return ServerHelper.createNonPersistentServer( _builder );
		 }

		 private static void Shutdown()
		 {
			 lock ( typeof( ServerHolder ) )
			 {
				  _allocation = null;
				  try
				  {
						if ( _server != null )
						{
							 _server.stop();
						}
				  }
				  finally
				  {
						_builder = null;
						_server = null;
				  }
			 }
		 }

		 private static void InitBuilder()
		 {
			  if ( _builder == null )
			  {
					_builder = CommunityServerBuilder.server();
			  }
		 }

		 public override void Run()
		 {
			  Shutdown();
		 }

		 static ServerHolder()
		 {
			  Runtime.Runtime.addShutdownHook( new ServerHolder() );
		 }

		 private ServerHolder() : base(typeof(ServerHolder).FullName)
		 {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		 }
	}

}