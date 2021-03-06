﻿using System.Collections.Generic;

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
namespace Org.Neo4j.Server.rest.web
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Test = org.junit.Test;


	using Node = Org.Neo4j.Graphdb.Node;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using GraphDatabaseFacade = Org.Neo4j.Kernel.impl.factory.GraphDatabaseFacade;
	using Database = Org.Neo4j.Server.database.Database;
	using WrappedDatabase = Org.Neo4j.Server.database.WrappedDatabase;
	using JsonHelper = Org.Neo4j.Server.rest.domain.JsonHelper;
	using TestGraphDatabaseFactory = Org.Neo4j.Test.TestGraphDatabaseFactory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.RelationshipType.withName;

	public class DatabaseMetadataServiceTest
	{
		 private GraphDatabaseFacade _db;
		 private long _relId;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  _db = ( GraphDatabaseFacade ) ( new TestGraphDatabaseFactory() ).newImpermanentDatabase();
			  using ( Transaction tx = _db.beginTx() )
			  {
					Node node = _db.createNode();
					node.CreateRelationshipTo( _db.createNode(), withName("a") );
					node.CreateRelationshipTo( _db.createNode(), withName("b") );
					_relId = node.CreateRelationshipTo( _db.createNode(), withName("c") ).Id;
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown()
		 public virtual void TearDown()
		 {
			  _db.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAdvertiseRelationshipTypesThatCurrentlyExistInTheDatabase() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAdvertiseRelationshipTypesThatCurrentlyExistInTheDatabase()
		 {
			  using ( Transaction tx = _db.beginTx() )
			  {
					_db.getRelationshipById( _relId ).delete();
					tx.Success();
			  }

			  Database database = new WrappedDatabase( _db );
			  DatabaseMetadataService service = new DatabaseMetadataService( database );

			  using ( Transaction tx = _db.beginTx() )
			  {
					Response response = service.GetRelationshipTypes( false );

					assertEquals( 200, response.Status );
					IList<IDictionary<string, object>> jsonList = JsonHelper.jsonToList( response.Entity.ToString() );
					assertEquals( 3, jsonList.Count );
			  }
			  database.Stop();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAdvertiseRelationshipTypesThatCurrentlyInUseInTheDatabase() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAdvertiseRelationshipTypesThatCurrentlyInUseInTheDatabase()
		 {
			  using ( Transaction tx = _db.beginTx() )
			  {
					_db.getRelationshipById( _relId ).delete();
					tx.Success();
			  }

			  Database database = new WrappedDatabase( _db );
			  DatabaseMetadataService service = new DatabaseMetadataService( database );

			  using ( Transaction tx = _db.beginTx() )
			  {
					Response response = service.GetRelationshipTypes( true );

					assertEquals( 200, response.Status );
					IList<IDictionary<string, object>> jsonList = JsonHelper.jsonToList( response.Entity.ToString() );
					assertEquals( 2, jsonList.Count );
			  }
			  database.Stop();
		 }
	}

}