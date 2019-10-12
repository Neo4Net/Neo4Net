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
namespace Org.Neo4j.Bolt.v1.runtime.integration
{
	using TestRule = org.junit.rules.TestRule;
	using Description = org.junit.runner.Description;
	using Statement = org.junit.runners.model.Statement;


	using BoltStateMachine = Org.Neo4j.Bolt.runtime.BoltStateMachine;
	using BoltStateMachineFactoryImpl = Org.Neo4j.Bolt.runtime.BoltStateMachineFactoryImpl;
	using Authentication = Org.Neo4j.Bolt.security.auth.Authentication;
	using BasicAuthentication = Org.Neo4j.Bolt.security.auth.BasicAuthentication;
	using DatabaseManager = Org.Neo4j.Dbms.database.DatabaseManager;
	using DependencyResolver = Org.Neo4j.Graphdb.DependencyResolver;
	using Org.Neo4j.Graphdb.config;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using AuthManager = Org.Neo4j.Kernel.api.security.AuthManager;
	using UserManagerSupplier = Org.Neo4j.Kernel.api.security.UserManagerSupplier;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using TransactionIdStore = Org.Neo4j.Kernel.impl.transaction.log.TransactionIdStore;
	using GraphDatabaseAPI = Org.Neo4j.Kernel.@internal.GraphDatabaseAPI;
	using NullLogService = Org.Neo4j.Logging.@internal.NullLogService;
	using TestGraphDatabaseFactory = Org.Neo4j.Test.TestGraphDatabaseFactory;
	using UsageData = Org.Neo4j.Udc.UsageData;

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
//ORIGINAL LINE: java.util.Map<org.neo4j.graphdb.config.Setting<?>,String> config = new java.util.HashMap<>();
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