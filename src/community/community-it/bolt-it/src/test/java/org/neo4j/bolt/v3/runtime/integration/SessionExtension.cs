using System;
using System.Collections.Generic;

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
namespace Neo4Net.Bolt.v3.runtime.integration
{
	using AfterEachCallback = org.junit.jupiter.api.extension.AfterEachCallback;
	using BeforeEachCallback = org.junit.jupiter.api.extension.BeforeEachCallback;
	using ExtensionContext = org.junit.jupiter.api.extension.ExtensionContext;


	using BoltStateMachine = Neo4Net.Bolt.runtime.BoltStateMachine;
	using BoltStateMachineFactoryImpl = Neo4Net.Bolt.runtime.BoltStateMachineFactoryImpl;
	using Authentication = Neo4Net.Bolt.security.auth.Authentication;
	using BasicAuthentication = Neo4Net.Bolt.security.auth.BasicAuthentication;
	using IDatabaseManager = Neo4Net.Dbms.database.DatabaseManager;
	using DependencyResolver = Neo4Net.GraphDb.DependencyResolver;
	using Neo4Net.GraphDb.config;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using IOUtils = Neo4Net.Io.IOUtils;
	using AuthManager = Neo4Net.Kernel.Api.security.AuthManager;
	using UserManagerSupplier = Neo4Net.Kernel.Api.security.UserManagerSupplier;
	using Config = Neo4Net.Kernel.configuration.Config;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using NullLogService = Neo4Net.Logging.Internal.NullLogService;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using UsageData = Neo4Net.Udc.UsageData;

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
//ORIGINAL LINE: java.util.Map<Neo4Net.graphdb.config.Setting<?>,String> config = new java.util.HashMap<>();
			  IDictionary<Setting<object>, string> config = new Dictionary<Setting<object>, string>();
			  config[GraphDatabaseSettings.auth_enabled] = Convert.ToString( _authEnabled );
			  _gdb = ( GraphDatabaseAPI ) ( new TestGraphDatabaseFactory() ).newImpermanentDatabase(config);
			  DependencyResolver resolver = _gdb.DependencyResolver;
			  Authentication authentication = authentication( resolver.ResolveDependency( typeof( AuthManager ) ), resolver.ResolveDependency( typeof( UserManagerSupplier ) ) );
			  _boltFactory = new BoltStateMachineFactoryImpl( resolver.ResolveDependency( typeof( IDatabaseManager ) ), new UsageData( null ), authentication, Clock.systemUTC(), Config.defaults(), NullLogService.Instance );
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