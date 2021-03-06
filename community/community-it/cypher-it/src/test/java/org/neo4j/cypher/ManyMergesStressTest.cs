﻿using System;
using System.Text;

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
namespace Org.Neo4j.Cypher
{
	using Ignore = org.junit.Ignore;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using GraphDatabaseCypherService = Org.Neo4j.Cypher.@internal.javacompat.GraphDatabaseCypherService;
	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Label = Org.Neo4j.Graphdb.Label;
	using Result = Org.Neo4j.Graphdb.Result;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using Org.Neo4j.Helpers.Collection;
	using LoginContext = Org.Neo4j.@internal.Kernel.Api.security.LoginContext;
	using GraphDatabaseQueryService = Org.Neo4j.Kernel.GraphDatabaseQueryService;
	using KernelTransaction = Org.Neo4j.Kernel.api.KernelTransaction;
	using InternalTransaction = Org.Neo4j.Kernel.impl.coreapi.InternalTransaction;
	using EmbeddedDatabaseRule = Org.Neo4j.Test.rule.EmbeddedDatabaseRule;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Ignore("Too costly to run by default but useful for testing resource clean up and indexing") public class ManyMergesStressTest
	public class ManyMergesStressTest
	{
		 private Random _random = new Random();

		 private string[] _syllables = new string[] { "Om", "Pa", "So", "Hu", "Ma", "Ni", "Ru", "Gu", "Ha", "Ta" };

		 private const int TRIES = 8000;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.EmbeddedDatabaseRule dbRule = new org.neo4j.test.rule.EmbeddedDatabaseRule();
		 public EmbeddedDatabaseRule DbRule = new EmbeddedDatabaseRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWorkFine()
		 public virtual void ShouldWorkFine()
		 {
			  GraphDatabaseService db = DbRule.GraphDatabaseAPI;
			  GraphDatabaseQueryService graph = new GraphDatabaseCypherService( db );

			  Label person = Label.label( "Person" );

			  using ( Transaction tx = Db.beginTx() )
			  {
					// THIS USED TO CAUSE OUT OF FILE HANDLES
					// (maybe look at:  http://stackoverflow.com/questions/6210348/too-many-open-files-error-on-lucene)
					Db.schema().indexFor(person).on("id").create();

					// THIS SHOULD ALSO WORK
					Db.schema().constraintFor(person).assertPropertyIsUnique("id").create();

					tx.Success();
			  }

			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().indexFor(person).on("name").create();
					tx.Success();
			  }

			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().awaitIndexesOnline(1, TimeUnit.MINUTES);
					tx.Success();
			  }

			  for ( int count = 0; count < TRIES; count++ )
			  {
					Pair<string, string> stringPair = RandomName;
					string ident = stringPair.First();
					string name = stringPair.Other();
					string id = Convert.ToString( Math.Abs( _random.nextLong() ) );
					string query = format( "MERGE (%s:Person {id: %s}) ON CREATE SET %s.name = \"%s\";", ident, id, ident, name );

					using ( InternalTransaction tx = graph.BeginTransaction( KernelTransaction.Type.@implicit, LoginContext.AUTH_DISABLED ) )
					{
						 Result result = Db.execute( query );
						 result.Close();
						 tx.Success();
					}
			  }
		 }

		 public virtual Pair<string, string> RandomName
		 {
			 get
			 {
				  StringBuilder identBuilder = new StringBuilder();
				  StringBuilder nameBuilder = new StringBuilder();
   
				  for ( int j = 0; j < 10; j++ )
				  {
						string part = _syllables[_random.Next( _syllables.Length )];
						if ( j != 0 )
						{
							 identBuilder.Append( '_' );
							 nameBuilder.Append( ' ' );
						}
						identBuilder.Append( part );
						nameBuilder.Append( part );
				  }
   
				  return Pair.of( identBuilder.ToString(), nameBuilder.ToString() );
			 }
		 }
	}

}