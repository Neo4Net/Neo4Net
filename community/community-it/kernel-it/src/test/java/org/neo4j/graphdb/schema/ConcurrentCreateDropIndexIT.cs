using System;
using System.Collections.Generic;
using System.Threading;

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
namespace Org.Neo4j.Graphdb.schema
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Race = Org.Neo4j.Test.Race;
	using CleanupRule = Org.Neo4j.Test.rule.CleanupRule;
	using DatabaseRule = Org.Neo4j.Test.rule.DatabaseRule;
	using ImpermanentDatabaseRule = Org.Neo4j.Test.rule.ImpermanentDatabaseRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.single;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.singleOrNull;

	public class ConcurrentCreateDropIndexIT
	{
		 private const string KEY = "key";

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.DatabaseRule db = new org.neo4j.test.rule.ImpermanentDatabaseRule();
		 public readonly DatabaseRule Db = new ImpermanentDatabaseRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.CleanupRule cleanupRule = new org.neo4j.test.rule.CleanupRule();
		 public readonly CleanupRule CleanupRule = new CleanupRule();

		 private readonly int _threads = Runtime.Runtime.availableProcessors();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void createTokens()
		 public virtual void CreateTokens()
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					for ( int i = 0; i < _threads; i++ )
					{
						 Db.createNode( Label( i ) ).setProperty( KEY, i );
					}
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void concurrentCreatingOfIndexesShouldNotInterfere() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ConcurrentCreatingOfIndexesShouldNotInterfere()
		 {
			  // WHEN concurrently creating indexes for different labels
			  Race race = new Race();
			  for ( int i = 0; i < _threads; i++ )
			  {
					race.AddContestant( IndexCreate( i ), 1 );
			  }
			  race.Go();

			  // THEN they should all be observed as existing in the end
			  using ( Transaction tx = Db.beginTx() )
			  {
					IList<IndexDefinition> indexes = new IList<IndexDefinition> { Db.schema().Indexes };
					assertEquals( _threads, indexes.Count );
					ISet<string> labels = new HashSet<string>();
					foreach ( IndexDefinition index in indexes )
					{
						 assertTrue( labels.Add( single( index.Labels ).name() ) );
					}
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void concurrentDroppingOfIndexesShouldNotInterfere() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ConcurrentDroppingOfIndexesShouldNotInterfere()
		 {
			  // GIVEN created indexes
			  IList<IndexDefinition> indexes = new List<IndexDefinition>();
			  using ( Transaction tx = Db.beginTx() )
			  {
					for ( int i = 0; i < _threads; i++ )
					{
						 indexes.Add( Db.schema().indexFor(Label(i)).on(KEY).create() );
					}
					tx.Success();
			  }

			  // WHEN dropping them
			  Race race = new Race();
			  foreach ( IndexDefinition index in indexes )
			  {
					race.AddContestant( IndexDrop( index ), 1 );
			  }
			  race.Go();

			  // THEN they should all be observed as dropped in the end
			  using ( Transaction tx = Db.beginTx() )
			  {
					assertEquals( 0, asList( Db.schema().Indexes ).size() );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void concurrentMixedCreatingAndDroppingOfIndexesShouldNotInterfere() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ConcurrentMixedCreatingAndDroppingOfIndexesShouldNotInterfere()
		 {
			  // GIVEN created indexes
			  IList<IndexDefinition> indexesToDrop = new List<IndexDefinition>();
			  int creates = _threads / 2;
			  int drops = _threads - creates;
			  using ( Transaction tx = Db.beginTx() )
			  {
					for ( int i = 0; i < drops; i++ )
					{
						 indexesToDrop.Add( Db.schema().indexFor(Label(i)).on(KEY).create() );
					}
					tx.Success();
			  }

			  // WHEN dropping them
			  Race race = new Race();
			  ISet<string> expectedIndexedLabels = new HashSet<string>();
			  for ( int i = 0; i < creates; i++ )
			  {
					expectedIndexedLabels.Add( Label( drops + i ).name() );
					race.AddContestant( IndexCreate( drops + i ), 1 );
			  }
			  foreach ( IndexDefinition index in indexesToDrop )
			  {
					race.AddContestant( IndexDrop( index ), 1 );
			  }
			  race.Go();

			  // THEN they should all be observed as dropped in the end
			  using ( Transaction tx = Db.beginTx() )
			  {
					IList<IndexDefinition> indexes = new IList<IndexDefinition> { Db.schema().Indexes };
					assertEquals( creates, indexes.Count );
					tx.Success();

					foreach ( IndexDefinition index in indexes )
					{
						 assertTrue( expectedIndexedLabels.remove( single( index.Labels ).name() ) );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void concurrentCreatingUniquenessConstraint() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ConcurrentCreatingUniquenessConstraint()
		 {
			  // given
			  Race race = ( new Race() ).withMaxDuration(10, SECONDS);
			  Label label = label( 0 );
			  race.AddContestants(10, () =>
			  {
				try
				{
					using ( Transaction tx = Db.beginTx() )
					{
						 Db.schema().constraintFor(label).assertPropertyIsUnique(KEY).create();
						 tx.Success();
					}
				}
				catch ( Exception e ) when ( e is TransientFailureException || e is ConstraintViolationException )
				{ // It's OK
				}
			  }, 300);

			  // when
			  race.Go();

			  using ( Transaction tx = Db.beginTx() )
			  {
					// then
					ConstraintDefinition constraint = single( Db.schema().getConstraints(label) );
					assertNotNull( constraint );
					IndexDefinition index = single( Db.schema().getIndexes(label) );
					assertNotNull( index );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void concurrentCreatingUniquenessConstraintOnNonUniqueData() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ConcurrentCreatingUniquenessConstraintOnNonUniqueData()
		 {
			  // given
			  Label label = label( 0 );
			  using ( Transaction tx = Db.beginTx() )
			  {
					for ( int i = 0; i < 2; i++ )
					{
						 Db.createNode( label ).setProperty( KEY, "A" );
					}
					tx.Success();
			  }
			  Race race = ( new Race() ).withMaxDuration(10, SECONDS);
			  race.AddContestants(3, () =>
			  {
				try
				{
					using ( Transaction tx = Db.beginTx() )
					{
						 Db.schema().constraintFor(label).assertPropertyIsUnique(KEY).create();
						 tx.Success();
					}
				}
				catch ( Exception e ) when ( e is TransientFailureException || e is ConstraintViolationException )
				{ // It's OK
				}
			  }, 100);

			  // when
			  race.Go();

			  using ( Transaction tx = Db.beginTx() )
			  {
					// then
					ConstraintDefinition constraint = singleOrNull( Db.schema().getConstraints(label) );
					assertNull( constraint );
					IndexDefinition index = singleOrNull( Db.schema().getIndexes(label) );
					assertNull( index );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void concurrentCreatingAndAwaitingIndexesOnline() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ConcurrentCreatingAndAwaitingIndexesOnline()
		 {
			  ExecutorService executor = CleanupRule.add( Executors.newSingleThreadExecutor() );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.Future<?> indexCreate = executor.submit(() ->
			  Future<object> indexCreate = executor.submit(() =>
			  {
				using ( Transaction tx = Db.beginTx() )
				{
					 Db.schema().indexFor(Label(0)).on(KEY).create();
					 tx.Success();
				}
			  });
			  while ( !indexCreate.Done )
			  {
					using ( Transaction tx = Db.beginTx() )
					{
						 Db.schema().awaitIndexesOnline(1, TimeUnit.MINUTES);
						 tx.Success();
					}
			  }
			  indexCreate.get();
		 }

		 private ThreadStart IndexCreate( int labelIndex )
		 {
			  return () =>
			  {
				using ( Transaction tx = Db.beginTx() )
				{
					 Db.schema().indexFor(Label(labelIndex)).on(KEY).create();
					 tx.Success();
				}
			  };
		 }

		 private ThreadStart IndexDrop( IndexDefinition index )
		 {
			  return () =>
			  {
				using ( Transaction tx = Db.beginTx() )
				{
					 index.Drop();
					 tx.Success();
				}
			  };
		 }

		 private static Label Label( int i )
		 {
			  return Label.label( "L" + i );
		 }
	}

}