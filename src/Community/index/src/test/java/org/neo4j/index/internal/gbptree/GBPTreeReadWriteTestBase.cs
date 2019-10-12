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
namespace Neo4Net.Index.@internal.gbptree
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;


	using Neo4Net.Cursors;
	using PageCacheAndDependenciesRule = Neo4Net.Test.rule.PageCacheAndDependenciesRule;
	using RandomRule = Neo4Net.Test.rule.RandomRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	public abstract class GBPTreeReadWriteTestBase<KEY, VALUE>
	{
		private bool InstanceFieldsInitialized = false;

		public GBPTreeReadWriteTestBase()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			RuleChain = RuleChain.outerRule( _random ).around( _deps );
		}

		 private RandomRule _random = new RandomRule();
		 private PageCacheAndDependenciesRule _deps = new PageCacheAndDependenciesRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(random).around(deps);
		 public RuleChain RuleChain;

		 private TestLayout<KEY, VALUE> _layout;
		 private File _indexFile;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  _indexFile = _deps.directory().file("index");
			  _layout = Layout;
		 }

		 internal abstract TestLayout<KEY, VALUE> Layout { get; }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSeeSimpleInsertions() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSeeSimpleInsertions()
		 {
			  using ( GBPTree<KEY, VALUE> index = index() )
			  {
					int count = 1000;
					using ( Writer<KEY, VALUE> writer = index.Writer() )
					{
						 for ( int i = 0; i < count; i++ )
						 {
							  writer.Put( Key( i ), Value( i ) );
						 }
					}

					using ( RawCursor<Hit<KEY, VALUE>, IOException> cursor = index.Seek( Key( 0 ), Key( long.MaxValue ) ) )
					{
						 for ( int i = 0; i < count; i++ )
						 {
							  assertTrue( cursor.Next() );
							  AssertEqualsKey( Key( i ), cursor.get().key() );
						 }
						 assertFalse( cursor.Next() );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSeeSimpleInsertionsWithExactMatch() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSeeSimpleInsertionsWithExactMatch()
		 {
			  using ( GBPTree<KEY, VALUE> index = index() )
			  {
					int count = 1000;
					using ( Writer<KEY, VALUE> writer = index.Writer() )
					{
						 for ( int i = 0; i < count; i++ )
						 {
							  writer.Put( Key( i ), Value( i ) );
						 }
					}

					for ( int i = 0; i < count; i++ )
					{
						 using ( RawCursor<Hit<KEY, VALUE>, IOException> cursor = index.Seek( Key( i ), Key( i ) ) )
						 {
							  assertTrue( cursor.Next() );
							  AssertEqualsKey( Key( i ), cursor.get().key() );
							  assertFalse( cursor.Next() );
						 }
					}
			  }
		 }

		 /* Randomized tests */

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSplitCorrectly() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSplitCorrectly()
		 {
			  // GIVEN
			  using ( GBPTree<KEY, VALUE> index = index() )
			  {
					// WHEN
					int count = 1_000;
					IList<KEY> seen = new List<KEY>( count );
					using ( Writer<KEY, VALUE> writer = index.Writer() )
					{
						 for ( int i = 0; i < count; i++ )
						 {
							  KEY key;
							  do
							  {
									key = key( _random.Next( 100_000 ) );
							  } while ( ListContains( seen, key ) );
							  VALUE value = value( i );
							  writer.Put( key, value );
							  seen.Add( key );
						 }
					}

					// THEN
					using ( RawCursor<Hit<KEY, VALUE>, IOException> cursor = index.Seek( Key( 0 ), Key( long.MaxValue ) ) )
					{
						 long prev = -1;
						 while ( cursor.Next() )
						 {
							  KEY hit = cursor.get().key();
							  long hitSeed = _layout.keySeed( hit );
							  if ( hitSeed < prev )
							  {
									fail( hit + " smaller than prev " + prev );
							  }
							  prev = hitSeed;
							  assertTrue( RemoveFromList( seen, hit ) );
						 }

						 if ( seen.Count > 0 )
						 {
							  fail( "expected hits " + seen );
						 }
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private GBPTree<KEY,VALUE> index() throws java.io.IOException
		 private GBPTree<KEY, VALUE> Index()
		 {
			  return ( new GBPTreeBuilder<KEY, VALUE>( _deps.pageCache(), _indexFile, _layout ) ).build();
		 }

		 private bool RemoveFromList( IList<KEY> list, KEY item )
		 {
			  for ( int i = 0; i < list.Count; i++ )
			  {
					if ( _layout.Compare( list[i], item ) == 0 )
					{
						 list.RemoveAt( i );
						 return true;
					}
			  }
			  return false;
		 }

		 private bool ListContains( IList<KEY> list, KEY item )
		 {
			  foreach ( KEY key in list )
			  {
					if ( _layout.Compare( key, item ) == 0 )
					{
						 return true;
					}
			  }
			  return false;
		 }

		 private VALUE Value( long seed )
		 {
			  return _layout.value( seed );
		 }

		 private KEY Key( long seed )
		 {
			  return _layout.key( seed );
		 }

		 private void AssertEqualsKey( KEY expected, KEY actual )
		 {
			  assertEquals( string.Format( "expected equal, expected={0}, actual={1}", expected.ToString(), actual.ToString() ), 0, _layout.Compare(expected, actual) );
		 }
	}

}