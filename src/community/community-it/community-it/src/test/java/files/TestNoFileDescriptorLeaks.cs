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
namespace Files
{
	using SystemUtils = org.apache.commons.lang3.SystemUtils;
	using Assume = org.junit.Assume;
	using BeforeClass = org.junit.BeforeClass;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using Transaction = Neo4Net.GraphDb.Transaction;
	using OsBeanUtil = Neo4Net.Io.os.OsBeanUtil;
	using EmbeddedDatabaseRule = Neo4Net.Test.rule.EmbeddedDatabaseRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.lessThan;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.not;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.MapUtil.map;

	public class TestNoFileDescriptorLeaks
	{
		 private static readonly AtomicInteger _counter = new AtomicInteger();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.Neo4Net.test.rule.EmbeddedDatabaseRule db = new org.Neo4Net.test.rule.EmbeddedDatabaseRule();
		 public EmbeddedDatabaseRule Db = new EmbeddedDatabaseRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeClass public static void beforeClass()
		 public static void BeforeClass()
		 {
			  Assume.assumeFalse( SystemUtils.IS_OS_WINDOWS );
			  Assume.assumeThat( OsBeanUtil.OpenFileDescriptors, not( OsBeanUtil.VALUE_UNAVAILABLE ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustNotLeakFileDescriptorsFromMerge()
		 public virtual void MustNotLeakFileDescriptorsFromMerge()
		 {
			  // GIVEN
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.execute( "create constraint on (n:Node) assert n.id is unique" );
					tx.Success();
			  }
			  CycleMerge( 1 );

			  long initialFDs = OsBeanUtil.OpenFileDescriptors;

			  // WHEN
			  CycleMerge( 300 );

			  // THEN
			  long finalFDs = OsBeanUtil.OpenFileDescriptors;
			  long upperBoundFDs = initialFDs + 50; // allow some slack
			  assertThat( finalFDs, lessThan( upperBoundFDs ) );
		 }

		 private void CycleMerge( int iterations )
		 {
			  for ( int i = 0; i < iterations; i++ )
			  {
					using ( Transaction tx = Db.beginTx() )
					{
						 Db.execute( "MERGE (a:Node {id: {a}}) " + "MERGE (b:Node {id: {b}}) " + "MERGE (c:Node {id: {c}}) " + "MERGE (d:Node {id: {d}}) " + "MERGE (e:Node {id: {e}}) " + "MERGE (f:Node {id: {f}}) ", map( "a", NextId() % 100, "b", NextId() % 100, "c", NextId() % 100, "d", NextId(), "e", NextId(), "f", NextId() ) );
						 Db.execute( "MERGE (n:Node {id: {a}}) ", map( "a", NextId() % 100 ) );
						 Db.execute( "MERGE (n:Node {id: {a}}) ", map( "a", NextId() % 100 ) );
						 Db.execute( "MERGE (n:Node {id: {a}}) ", map( "a", NextId() % 100 ) );
						 Db.execute( "MERGE (n:Node {id: {a}}) ", map( "a", NextId() ) );
						 Db.execute( "MERGE (n:Node {id: {a}}) ", map( "a", NextId() ) );
						 Db.execute( "MERGE (n:Node {id: {a}}) ", map( "a", NextId() ) );
						 tx.Success();
					}
			  }
		 }

		 private static int NextId()
		 {
			  return _counter.incrementAndGet();
		 }
	}

}