using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

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
namespace Neo4Net.Bolt
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Driver = Neo4Net.driver.v1.Driver;
	using GraphDatabase = Neo4Net.driver.v1.GraphDatabase;
	using Session = Neo4Net.driver.v1.Session;
	using Transaction = Neo4Net.driver.v1.Transaction;
	using ClientException = Neo4Net.driver.v1.exceptions.ClientException;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using Neo4NetRule = Neo4Net.Harness.junit.Neo4NetRule;
	using Settings = Neo4Net.Kernel.configuration.Settings;
	using OnlineBackupSettings = Neo4Net.Kernel.impl.enterprise.configuration.OnlineBackupSettings;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.empty;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.driver.v1.AuthTokens.basic;

	public class DeleteUserStressIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.Neo4Net.harness.junit.Neo4NetRule db = new org.Neo4Net.harness.junit.Neo4NetRule().withConfig(org.Neo4Net.graphdb.factory.GraphDatabaseSettings.auth_enabled, "true").withConfig(org.Neo4Net.kernel.impl.enterprise.configuration.OnlineBackupSettings.online_backup_enabled, org.Neo4Net.kernel.configuration.Settings.FALSE);
		 public Neo4NetRule Db = new Neo4NetRule().withConfig(GraphDatabaseSettings.auth_enabled, "true").withConfig(OnlineBackupSettings.online_backup_enabled, Settings.FALSE);

		 private Driver _adminDriver;
		 private readonly ISet<Exception> _errors = ConcurrentDictionary.newKeySet();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  _adminDriver = GraphDatabase.driver( Db.boltURI(), basic("Neo4Net", "Neo4Net") );
			  using ( Session session = _adminDriver.session(), Transaction tx = session.BeginTransaction() )
			  {
					tx.run( "CALL dbms.changePassword('abc')" ).consume();
					tx.success();
			  }
			  _adminDriver.close();
			  _adminDriver = GraphDatabase.driver( Db.boltURI(), basic("Neo4Net", "abc") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRun() throws InterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRun()
		 {
			  ExecutorService service = Executors.newFixedThreadPool( 3 );
			  service.submit( _createUserWork );
			  service.submit( _deleteUserWork );
			  service.submit( _transactionWork );

			  service.awaitTermination( 30, TimeUnit.SECONDS );

			  string msg = string.join( Environment.NewLine, _errors.Select( Exception.getMessage ).ToList() );
			  assertThat( msg, _errors, empty() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("InfiniteLoopStatement") private final Runnable transactionWork = () ->
		 private readonly ThreadStart _transactionWork = () =>
		 {

		  for ( ; ; )
		  {
				try
				{
					using ( Driver driver = GraphDatabase.driver( Db.boltURI(), basic("pontus", "sutnop") ) )
					{
   
						 using ( Session session = driver.session(), Transaction tx = session.BeginTransaction() )
						 {
							  tx.run( "UNWIND range(1, 100000) AS n RETURN n" ).consume();
							  tx.success();
						 }
					}
				}
				catch ( ClientException e )
				{
					 if ( !e.Message.Equals( "The client is unauthorized due to authentication failure." ) )
					 {
						  _errors.Add( e );
					 }
				}
		  }
		 };

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("InfiniteLoopStatement") private final Runnable deleteUserWork = () ->
		 private readonly ThreadStart _deleteUserWork = () =>
		 {

		  for ( ; ; )
		  {
				try
				{
					using ( Session session = _adminDriver.session(), Transaction tx = session.BeginTransaction() )
					{
						 tx.run( "CALL dbms.security.deleteUser('pontus')" ).consume();
						 tx.success();
					}
				}
				catch ( ClientException e )
				{
					 if ( !e.Message.Equals( "User 'pontus' does not exist." ) )
					 {
						  _errors.Add( e );
					 }
				}
		  }
		 };

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("InfiniteLoopStatement") private final Runnable createUserWork = () ->
		 private readonly ThreadStart _createUserWork = () =>
		 {
		  for ( ; ; )
		  {
				try
				{
					using ( Session session = _adminDriver.session(), Transaction tx = session.BeginTransaction() )
					{
						 tx.run( "CALL dbms.security.createUser('pontus', 'sutnop', false)" ).consume();
						 tx.success();
					}
				}
				catch ( ClientException e )
				{
					 if ( !e.Message.Equals( "The specified user 'pontus' already exists." ) )
					 {
						  _errors.Add( e );
					 }
				}
		  }
		 };
	}

}