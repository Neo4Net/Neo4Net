﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
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
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Org.Neo4j.Bolt
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Driver = Org.Neo4j.driver.v1.Driver;
	using GraphDatabase = Org.Neo4j.driver.v1.GraphDatabase;
	using Session = Org.Neo4j.driver.v1.Session;
	using Transaction = Org.Neo4j.driver.v1.Transaction;
	using ClientException = Org.Neo4j.driver.v1.exceptions.ClientException;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using Neo4jRule = Org.Neo4j.Harness.junit.Neo4jRule;
	using Settings = Org.Neo4j.Kernel.configuration.Settings;
	using OnlineBackupSettings = Org.Neo4j.Kernel.impl.enterprise.configuration.OnlineBackupSettings;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.empty;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.driver.v1.AuthTokens.basic;

	public class DeleteUserStressIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.harness.junit.Neo4jRule db = new org.neo4j.harness.junit.Neo4jRule().withConfig(org.neo4j.graphdb.factory.GraphDatabaseSettings.auth_enabled, "true").withConfig(org.neo4j.kernel.impl.enterprise.configuration.OnlineBackupSettings.online_backup_enabled, org.neo4j.kernel.configuration.Settings.FALSE);
		 public Neo4jRule Db = new Neo4jRule().withConfig(GraphDatabaseSettings.auth_enabled, "true").withConfig(OnlineBackupSettings.online_backup_enabled, Settings.FALSE);

		 private Driver _adminDriver;
		 private readonly ISet<Exception> _errors = ConcurrentDictionary.newKeySet();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  _adminDriver = GraphDatabase.driver( Db.boltURI(), basic("neo4j", "neo4j") );
			  using ( Session session = _adminDriver.session(), Transaction tx = session.beginTransaction() )
			  {
					tx.run( "CALL dbms.changePassword('abc')" ).consume();
					tx.success();
			  }
			  _adminDriver.close();
			  _adminDriver = GraphDatabase.driver( Db.boltURI(), basic("neo4j", "abc") );
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
   
						 using ( Session session = driver.session(), Transaction tx = session.beginTransaction() )
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
					using ( Session session = _adminDriver.session(), Transaction tx = session.beginTransaction() )
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
					using ( Session session = _adminDriver.session(), Transaction tx = session.beginTransaction() )
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