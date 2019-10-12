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
namespace Neo4Net.Graphdb
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using ArrayUtil = Neo4Net.Helpers.ArrayUtil;
	using Race = Neo4Net.Test.Race;
	using DatabaseRule = Neo4Net.Test.rule.DatabaseRule;
	using EmbeddedDatabaseRule = Neo4Net.Test.rule.EmbeddedDatabaseRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	/// <summary>
	/// Test for how properties are read and that they should be read consistently, i.e. adhere to neo4j's
	/// interpretation of the ACID guarantees.
	/// </summary>
	public class ConsistentPropertyReadsIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.DatabaseRule db = new org.neo4j.test.rule.EmbeddedDatabaseRule();
		 public DatabaseRule Db = new EmbeddedDatabaseRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReadConsistentPropertyValues() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReadConsistentPropertyValues()
		 {
			  // GIVEN
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Node[] nodes = new Node[10];
			  Node[] nodes = new Node[10];
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String[] keys = new String[] {"1", "2", "3"};
			  string[] keys = new string[] { "1", "2", "3" };
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String[] values = new String[] { longString('a'), longString('b'), longString('c')};
			  string[] values = new string[] { LongString( 'a' ), LongString( 'b' ), LongString( 'c' ) };
			  using ( Transaction tx = Db.beginTx() )
			  {
					for ( int i = 0; i < nodes.Length; i++ )
					{
						 nodes[i] = Db.createNode();
						 foreach ( string key in keys )
						 {
							  nodes[i].SetProperty( key, values[0] );
						 }
					}
					tx.Success();
			  }

			  int updaters = 10;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicLong updatersDone = new java.util.concurrent.atomic.AtomicLong(updaters);
			  AtomicLong updatersDone = new AtomicLong( updaters );
			  Race race = new Race();
			  for ( int i = 0; i < updaters; i++ )
			  {
					// Changers
					race.AddContestant(() =>
					{
					 try
					 {
						  ThreadLocalRandom random = ThreadLocalRandom.current();
						  for ( int j = 0; j < 100; j++ )
						  {
								Node node = nodes[random.Next( nodes.Length )];
								string key = keys[random.Next( keys.Length )];
								using ( Transaction tx = Db.beginTx() )
								{
									 node.RemoveProperty( key );
									 tx.Success();
								}
								using ( Transaction tx = Db.beginTx() )
								{
									 node.SetProperty( key, values[random.Next( values.Length )] );
									 tx.Success();
								}
						  }
					 }
					 finally
					 {
						  updatersDone.decrementAndGet();
					 }
					});
			  }
			  for ( int i = 0; i < 100; i++ )
			  {
					// Readers
					race.AddContestant(() =>
					{
					 ThreadLocalRandom random = ThreadLocalRandom.current();
					 while ( updatersDone.get() > 0 )
					 {
						  using ( Transaction tx = Db.beginTx() )
						  {
								string value = ( string ) nodes[random.Next( nodes.Length )].getProperty( keys[random.Next( keys.Length )], null );
								assertTrue( value, string.ReferenceEquals( value, null ) || ArrayUtil.contains( values, value ) );
								tx.Success();
						  }
					 }
					});
			  }

			  // WHEN
			  race.Go();
		 }

		 private string LongString( char c )
		 {
			  char[] chars = new char[ThreadLocalRandom.current().Next(800, 1000)];
			  Arrays.fill( chars, c );
			  return new string( chars );
		 }
	}

}