using System;
using System.Collections.Generic;

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
namespace Org.Neo4j.Bolt.v3.runtime.integration
{
	using AfterEachCallback = org.junit.jupiter.api.extension.AfterEachCallback;
	using BeforeEachCallback = org.junit.jupiter.api.extension.BeforeEachCallback;
	using ExtensionContext = org.junit.jupiter.api.extension.ExtensionContext;


	using BoltStateMachine = Org.Neo4j.Bolt.runtime.BoltStateMachine;
	using BoltStateMachineFactoryImpl = Org.Neo4j.Bolt.runtime.BoltStateMachineFactoryImpl;
	using Authentication = Org.Neo4j.Bolt.security.auth.Authentication;
	using BasicAuthentication = Org.Neo4j.Bolt.security.auth.BasicAuthentication;
	using DatabaseManager = Org.Neo4j.Dbms.database.DatabaseManager;
	using DependencyResolver = Org.Neo4j.Graphdb.DependencyResolver;
	using Org.Neo4j.Graphdb.config;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using IOUtils = Org.Neo4j.Io.IOUtils;
	using AuthManager = Org.Neo4j.Kernel.api.security.AuthManager;
	using UserManagerSupplier = Org.Neo4j.Kernel.api.security.UserManagerSupplier;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using GraphDatabaseAPI = Org.Neo4j.Kernel.@internal.GraphDatabaseAPI;
	using NullLogService = Org.Neo4j.Logging.@internal.NullLogService;
	using TestGraphDatabaseFactory = Org.Neo4j.Test.TestGraphDatabaseFactory;
	using UsageData = Org.Neo4j.Udc.UsageData;

	public class SessionExtension : BeforeEachCallback, AfterEachCallback
	{
		 private GraphDatabaseAPI _gdb;
		 private BoltStateMachineFactoryImpl _boltFactory;
		 private IList<BoltStateMachine> _runningMachines = new List<BoltStateMachine>();
		 private bool _authEnabled;

		 private Authentication Authentication( AuthManager authManager, UserManagerSupplier userManagerSupplier )
		 {
			  return new BasicAuthentication( authManager, userManagerSupplier );
		 }

		 public virtual BoltStateMachine NewMachine( long version, BoltChannel boltChannel )
		 {
			  if ( _boltFactory == null )
			  {
					throw new System.InvalidOperationException( "Cannot access test environment before test is running." );
			  }
			  BoltStateMachine machine = _boltFactory.newStateMachine( version, boltChannel );
			  _runningMachines.Add( machine );
			  return machine;
		 }

		 public override void BeforeEach( ExtensionContext extensionContext )
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Map<org.neo4j.graphdb.config.Setting<?>,String> config = new java.util.HashMap<>();
			  IDictionary<Setting<object>, string> config = new Dictionary<Setting<object>, string>();
			  config[GraphDatabaseSettings.auth_enabled] = Convert.ToString( _authEnabled );
			  _gdb = ( GraphDatabaseAPI ) ( new TestGraphDatabaseFactory() ).newImpermanentDatabase(config);
			  DependencyResolver resolver = _gdb.DependencyResolver;
			  Authentication authentication = authentication( resolver.ResolveDependency( typeof( AuthManager ) ), resolver.ResolveDependency( typeof( UserManagerSupplier ) ) );
			  _boltFactory = new BoltStateMachineFactoryImpl( resolver.ResolveDependency( typeof( DatabaseManager ) ), new UsageData( null ), authentication, Clock.systemUTC(), Config.defaults(), NullLogService.Instance );
		 }

		 public override void AfterEach( ExtensionContext extensionContext )
		 {
			  try
			  {
					if ( _runningMachines != null )
					{
						 IOUtils.closeAll( _runningMachines );
					}
			  }
			  catch ( Exception e )
			  {
					Console.WriteLine( e.ToString() );
					Console.Write( e.StackTrace );
			  }

			  _gdb.shutdown();
		 }
	}

}