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
namespace Neo4Net.Cypher
{
	using After = org.junit.After;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using DatabaseRule = Neo4Net.Test.rule.DatabaseRule;
	using ImpermanentDatabaseRule = Neo4Net.Test.rule.ImpermanentDatabaseRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;

	public class CreateIndexStressIT
	{
		 private const int NUM_PROPS = 400;
		 private readonly AtomicBoolean _hasFailed = new AtomicBoolean( false );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.DatabaseRule db = new org.neo4j.test.rule.ImpermanentDatabaseRule().withSetting(org.neo4j.graphdb.factory.GraphDatabaseSettings.query_cache_size, "0");
		 public DatabaseRule Db = new ImpermanentDatabaseRule().withSetting(GraphDatabaseSettings.query_cache_size, "0");

		 private readonly ExecutorService _executorService = Executors.newFixedThreadPool( 10 );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown()
		 public virtual void TearDown()
		 {
			  _executorService.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleConcurrentIndexCreationAndUsage() throws InterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleConcurrentIndexCreationAndUsage()
		 {
			  // Given
			  Dictionary<string, object> @params = new Dictionary<string, object>();
			  @params["param"] = NUM_PROPS;
			  Db.execute( "FOREACH(x in range(0,$param) | CREATE (:A {prop:x})) ", @params );
			  Db.execute( "CREATE INDEX ON :A(prop) " );

			  // When
			  for ( int i = 0; i < NUM_PROPS; i++ )
			  {
					@params["param"] = i;
					ExecuteInThread( "MATCH (n:A) WHERE n.prop CONTAINS 'A' RETURN n.prop", @params );
			  }

			  // Then
			  AwaitAndAssertNoErrors();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void awaitAndAssertNoErrors() throws InterruptedException
		 private void AwaitAndAssertNoErrors()
		 {
			  _executorService.awaitTermination( 3L, TimeUnit.SECONDS );
			  assertFalse( _hasFailed.get() );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private void executeInThread(final String query, java.util.Map<String,Object> params)
		 private void ExecuteInThread( string query, IDictionary<string, object> @params )
		 {
			  _executorService.execute(() =>
			  {
				try
				{
					 Db.execute( query, @params ).resultAsString();
				}
				catch ( Exception )
				{
					 _hasFailed.set( true );
				}
			  });
		 }
	}

}