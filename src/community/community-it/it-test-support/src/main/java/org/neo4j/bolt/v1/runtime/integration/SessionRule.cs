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
namespace Neo4Net.Bolt.v1.runtime.integration
{
	using TestRule = org.junit.rules.TestRule;
	using Description = org.junit.runner.Description;
	using Statement = org.junit.runners.model.Statement;


	using BoltStateMachine = Neo4Net.Bolt.runtime.BoltStateMachine;
	using BoltStateMachineFactoryImpl = Neo4Net.Bolt.runtime.BoltStateMachineFactoryImpl;
	using Authentication = Neo4Net.Bolt.security.auth.Authentication;
	using BasicAuthentication = Neo4Net.Bolt.security.auth.BasicAuthentication;
	using DatabaseManager = Neo4Net.Dbms.database.DatabaseManager;
	using DependencyResolver = Neo4Net.GraphDb.DependencyResolver;
	using Neo4Net.GraphDb.config;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using AuthManager = Neo4Net.Kernel.api.security.AuthManager;
	using UserManagerSupplier = Neo4Net.Kernel.api.security.UserManagerSupplier;
	using Config = Neo4Net.Kernel.configuration.Config;
	using TransactionIdStore = Neo4Net.Kernel.impl.transaction.log.TransactionIdStore;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using NullLogService = Neo4Net.Logging.Internal.NullLogService;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using UsageData = Neo4Net.Udc.UsageData;

	public class SessionRule : TestRule
	{
		 private GraphDatabaseAPI _gdb;
		 private BoltStateMachineFactoryImpl _boltFactory;
		 private LinkedList<BoltStateMachine> _runningMachines = new LinkedList<BoltStateMachine>();
		 private bool _authEnabled;

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public org.junit.runners.model.Statement apply(final org.junit.runners.model.Statement super, org.junit.runner.Description description)
		 public override Statement Apply( Statement @base, Description description )
		 {
			  return new StatementAnonymousInnerClass( this, @base );
		 }

		 private class StatementAnonymousInnerClass : Statement
		 {
			 private readonly SessionRule _outerInstance;

			 private Statement @base;

			 public StatementAnonymousInnerClass( SessionRule outerInstance, Statement @base )
			 {
				 this.outerInstance = outerInstance;
				 this.@base = @base;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void evaluate() throws Throwable
			 public override void evaluate()
			 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Map<org.Neo4Net.graphdb.config.Setting<?>,String> config = new java.util.HashMap<>();
				  IDictionary<Setting<object>, string> config = new Dictionary<Setting<object>, string>();
				  config[GraphDatabaseSettings.auth_enabled] = Convert.ToString( _outerInstance.authEnabled );
				  _outerInstance.gdb = ( GraphDatabaseAPI ) ( new TestGraphDatabaseFactory() ).newImpermanentDatabase(config);
				  DependencyResolver resolver = _outerInstance.gdb.DependencyResolver;
				  DatabaseManager databaseManager = resolver.ResolveDependency( typeof( DatabaseManager ) );
				  Authentication authentication = authentication( resolver.ResolveDependency( typeof( AuthManager ) ), resolver.ResolveDependency( typeof( UserManagerSupplier ) ) );
				  _outerInstance.boltFactory = new BoltStateMachineFactoryImpl( databaseManager, new UsageData( null ), authentication, Clock.systemUTC(), Config.defaults(), NullLogService.Instance );
				  try
				  {
						@base.evaluate();
				  }
				  finally
				  {
						try
						{
							 if ( _outerInstance.runningMachines != null )
							 {
								  _outerInstance.runningMachines.forEach( BoltStateMachine.close );
							 }
						}
						catch ( Exception e )
						{
							 Console.WriteLine( e.ToString() );
							 Console.Write( e.StackTrace );
						}

						_outerInstance.gdb.shutdown();
				  }
			 }
		 }

		 private static Authentication Authentication( AuthManager authManager, UserManagerSupplier userManagerSupplier )
		 {
			  return new BasicAuthentication( authManager, userManagerSupplier );
		 }

		 internal virtual BoltStateMachine NewMachine( BoltChannel boltChannel )
		 {
			  return NewMachine( BoltProtocolV1.VERSION, boltChannel );
		 }

		 public virtual BoltStateMachine NewMachine( long version, BoltChannel boltChannel )
		 {
			  if ( _boltFactory == null )
			  {
					throw new System.InvalidOperationException( "Cannot access test environment before test is running." );
			  }
			  BoltStateMachine machine = _boltFactory.newStateMachine( version, boltChannel );
			  _runningMachines.AddLast( machine );
			  return machine;
		 }

		 internal virtual SessionRule WithAuthEnabled( bool authEnabled )
		 {
			  this._authEnabled = authEnabled;
			  return this;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.net.URL putTmpFile(String prefix, String suffix, String contents) throws java.io.IOException
		 public virtual URL PutTmpFile( string prefix, string suffix, string contents )
		 {
			  File tmpFile = File.createTempFile( prefix, suffix, null );
			  tmpFile.deleteOnExit();
			  using ( PrintWriter @out = new PrintWriter( tmpFile ) )
			  {
					@out.println( contents );
			  }
			  return tmpFile.toURI().toURL();
		 }

		 public virtual GraphDatabaseAPI Graph()
		 {
			  return _gdb;
		 }

		 public virtual long LastClosedTxId()
		 {
			  return _gdb.DependencyResolver.resolveDependency( typeof( TransactionIdStore ) ).LastClosedTransactionId;
		 }

	}

}