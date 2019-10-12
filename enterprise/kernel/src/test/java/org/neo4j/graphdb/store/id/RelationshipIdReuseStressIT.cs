using System;
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
namespace Org.Neo4j.Graphdb.store.id
{

	using Matchers = org.hamcrest.Matchers;
	using After = org.junit.After;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Org.Neo4j.Graphdb;
	using Iterables = Org.Neo4j.Helpers.Collection.Iterables;
	using Iterators = Org.Neo4j.Helpers.Collection.Iterators;
	using DeadlockDetectedException = Org.Neo4j.Kernel.DeadlockDetectedException;
	using EnterpriseEditionSettings = Org.Neo4j.Kernel.impl.enterprise.configuration.EnterpriseEditionSettings;
	using IdController = Org.Neo4j.Kernel.impl.storageengine.impl.recordstorage.id.IdController;
	using IdGeneratorFactory = Org.Neo4j.Kernel.impl.store.id.IdGeneratorFactory;
	using IdType = Org.Neo4j.Kernel.impl.store.id.IdType;
	using DatabaseRule = Org.Neo4j.Test.rule.DatabaseRule;
	using EnterpriseDatabaseRule = Org.Neo4j.Test.rule.EnterpriseDatabaseRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static System.currentTimeMillis;

	public class RelationshipIdReuseStressIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.DatabaseRule embeddedDatabase = new org.neo4j.test.rule.EnterpriseDatabaseRule().withSetting(org.neo4j.kernel.impl.enterprise.configuration.EnterpriseEditionSettings.idTypesToReuse, org.neo4j.kernel.impl.store.id.IdType.RELATIONSHIP.name());
		 public DatabaseRule EmbeddedDatabase = new EnterpriseDatabaseRule().withSetting(EnterpriseEditionSettings.idTypesToReuse, IdType.RELATIONSHIP.name());

		 private readonly ExecutorService _executorService = Executors.newCachedThreadPool();

		 private readonly string _nameProperty = "name";
		 private const int NUMBER_OF_BANDS = 3;
		 private const int NUMBER_OF_CITIES = 10;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown()
		 public virtual void TearDown()
		 {
			  _executorService.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void relationshipIdReused() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void RelationshipIdReused()
		 {
			  Label cityLabel = Label.label( "city" );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.graphdb.Label bandLabel = org.neo4j.graphdb.Label.label("band");
			  Label bandLabel = Label.label( "band" );
			  CreateBands( bandLabel );
			  CreateCities( cityLabel );

			  AtomicBoolean stopFlag = new AtomicBoolean( false );
			  RelationshipsCreator relationshipsCreator = new RelationshipsCreator( this, stopFlag, bandLabel, cityLabel );
			  RelationshipRemover relationshipRemover = new RelationshipRemover( this, bandLabel, cityLabel, stopFlag );
			  IdController idController = EmbeddedDatabase.DependencyResolver.resolveDependency( typeof( IdController ) );

			  assertNotNull( "idController was null for some reason", idController );

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.List<java.util.concurrent.Future<?>> futures = new java.util.ArrayList<>();
			  IList<Future<object>> futures = new List<Future<object>>();
			  futures.Add( _executorService.submit( relationshipRemover ) );
			  futures.Add( _executorService.submit( relationshipsCreator ) );
			  futures.Add( StartRelationshipTypesCalculator( bandLabel, stopFlag ) );
			  futures.Add( StartRelationshipCalculator( bandLabel, stopFlag ) );

			  long startTime = currentTimeMillis();
			  long currentTime;
			  long createdRelationships;
			  long removedRelationships;
			  do
			  {
					TimeUnit.MILLISECONDS.sleep( 500 );
					idController.Maintenance(); // just to make sure maintenance happens
					currentTime = currentTimeMillis();
					createdRelationships = relationshipsCreator.CreatedRelationships;
					removedRelationships = relationshipRemover.RemovedRelationships;
			  } while ( ( currentTime - startTime ) < 5_000 || createdRelationships < 1_000 || removedRelationships < 100 );
			  stopFlag.set( true );
			  _executorService.shutdown();
			  CompleteFutures( futures );

			  long highestPossibleIdInUse = HighestUsedIdForRelationships;
			  assertThat( "Number of created relationships should be higher then highest possible id, since those are " + "reused.", relationshipsCreator.CreatedRelationships, Matchers.greaterThan( highestPossibleIdInUse ) );
		 }

		 private long HighestUsedIdForRelationships
		 {
			 get
			 {
				  IdGeneratorFactory idGeneratorFactory = EmbeddedDatabase.DependencyResolver.resolveDependency( typeof( IdGeneratorFactory ) );
				  return idGeneratorFactory.Get( IdType.RELATIONSHIP ).HighestPossibleIdInUse;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void completeFutures(java.util.List<java.util.concurrent.Future<?>> futures) throws InterruptedException, java.util.concurrent.ExecutionException
		 private void CompleteFutures<T1>( IList<T1> futures )
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (java.util.concurrent.Future<?> future : futures)
			  foreach ( Future<object> future in futures )
			  {
					future.get();
			  }
		 }

		 private void CreateCities( Label cityLabel )
		 {
			  using ( Transaction transaction = EmbeddedDatabase.beginTx() )
			  {
					for ( int i = 1; i <= NUMBER_OF_CITIES; i++ )
					{
						 CreateLabeledNamedNode( cityLabel, "city" + i );
					}
					transaction.Success();
			  }
		 }

		 private void CreateBands( Label bandLabel )
		 {
			  using ( Transaction transaction = EmbeddedDatabase.beginTx() )
			  {
					for ( int i = 1; i <= NUMBER_OF_BANDS; i++ )
					{
						 CreateLabeledNamedNode( bandLabel, "band" + i );
					}
					transaction.Success();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private java.util.concurrent.Future<?> startRelationshipCalculator(final org.neo4j.graphdb.Label bandLabel, final java.util.concurrent.atomic.AtomicBoolean stopFlag)
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 private Future<object> StartRelationshipCalculator( Label bandLabel, AtomicBoolean stopFlag )
		 {
			  return _executorService.submit( new RelationshipCalculator( this, stopFlag, bandLabel ) );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private java.util.concurrent.Future<?> startRelationshipTypesCalculator(final org.neo4j.graphdb.Label bandLabel, final java.util.concurrent.atomic.AtomicBoolean stopFlag)
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 private Future<object> StartRelationshipTypesCalculator( Label bandLabel, AtomicBoolean stopFlag )
		 {
			  return _executorService.submit( new RelationshipTypeCalculator( this, stopFlag, bandLabel ) );
		 }

		 private Direction RandomDirection
		 {
			 get
			 {
				  return Direction.values()[ThreadLocalRandom.current().Next(Direction.values().length)];
			 }
		 }

		 private TestRelationshipTypes RandomRelationshipType
		 {
			 get
			 {
				  return Enum.GetValues( typeof( TestRelationshipTypes ) )[ThreadLocalRandom.current().Next(Enum.GetValues(typeof(TestRelationshipTypes)).length)];
			 }
		 }

		 private Node GetRandomCityNode( DatabaseRule embeddedDatabase, Label cityLabel )
		 {
			  return embeddedDatabase.FindNode( cityLabel, _nameProperty, "city" + ( ThreadLocalRandom.current().Next(1, NUMBER_OF_CITIES + 1) ) );
		 }

		 private Node GetRandomBandNode( DatabaseRule embeddedDatabase, Label bandLabel )
		 {
			  return embeddedDatabase.FindNode( bandLabel, _nameProperty, "band" + ( ThreadLocalRandom.current().Next(1, NUMBER_OF_BANDS + 1) ) );
		 }

		 private void CreateLabeledNamedNode( Label label, string name )
		 {
			  Node node = EmbeddedDatabase.createNode( label );
			  node.SetProperty( _nameProperty, name );
		 }

		 private enum TestRelationshipTypes
		 {
			  Like,
			  Hate,
			  Neutral
		 }

		 private class RelationshipsCreator : ThreadStart
		 {
			 private readonly RelationshipIdReuseStressIT _outerInstance;

			  internal readonly AtomicBoolean StopFlag;
			  internal readonly Label BandLabel;
			  internal readonly Label CityLabel;

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal volatile long CreatedRelationshipsConflict;

			  internal RelationshipsCreator( RelationshipIdReuseStressIT outerInstance, AtomicBoolean stopFlag, Label bandLabel, Label cityLabel )
			  {
				  this._outerInstance = outerInstance;
					this.StopFlag = stopFlag;
					this.BandLabel = bandLabel;
					this.CityLabel = cityLabel;
			  }

			  public override void Run()
			  {
					while ( !StopFlag.get() )
					{
						 int newRelationships = 0;
						 try
						 {
								 using ( Transaction transaction = outerInstance.EmbeddedDatabase.beginTx() )
								 {
								  Node bandNode = outerInstance.getRandomBandNode( outerInstance.EmbeddedDatabase, BandLabel );
								  int direction = ThreadLocalRandom.current().Next(3);
								  switch ( direction )
								  {
								  case 0:
										newRelationships += ConnectCitiesToBand( bandNode );
										break;
								  case 1:
										newRelationships += ConnectBandToCities( bandNode );
										break;
								  case 2:
										newRelationships += ConnectCitiesToBand( bandNode );
										newRelationships += ConnectBandToCities( bandNode );
										break;
								  default:
										throw new System.InvalidOperationException( "Unsupported direction value:" + direction );
								  }
								  transaction.Success();
								 }
						 }
						 catch ( DeadlockDetectedException )
						 {
							  // deadlocks ignored
						 }
						 CreatedRelationshipsConflict += newRelationships;
						 long millisToWait = ThreadLocalRandom.current().nextLong(10, 30);
						 LockSupport.parkNanos( TimeUnit.MILLISECONDS.toNanos( millisToWait ) );
					}
			  }

			  internal virtual int ConnectBandToCities( Node bandNode )
			  {
					Node city1 = outerInstance.getRandomCityNode( outerInstance.EmbeddedDatabase, CityLabel );
					Node city2 = outerInstance.getRandomCityNode( outerInstance.EmbeddedDatabase, CityLabel );
					Node city3 = outerInstance.getRandomCityNode( outerInstance.EmbeddedDatabase, CityLabel );
					Node city4 = outerInstance.getRandomCityNode( outerInstance.EmbeddedDatabase, CityLabel );
					Node city5 = outerInstance.getRandomCityNode( outerInstance.EmbeddedDatabase, CityLabel );

					bandNode.CreateRelationshipTo( city1, TestRelationshipTypes.Like );
					bandNode.CreateRelationshipTo( city2, TestRelationshipTypes.Like );
					bandNode.CreateRelationshipTo( city3, TestRelationshipTypes.Hate );
					bandNode.CreateRelationshipTo( city4, TestRelationshipTypes.Like );
					bandNode.CreateRelationshipTo( city5, TestRelationshipTypes.Neutral );
					return 5;
			  }

			  internal virtual int ConnectCitiesToBand( Node bandNode )
			  {
					Node city1 = outerInstance.getRandomCityNode( outerInstance.EmbeddedDatabase, CityLabel );
					Node city2 = outerInstance.getRandomCityNode( outerInstance.EmbeddedDatabase, CityLabel );
					Node city3 = outerInstance.getRandomCityNode( outerInstance.EmbeddedDatabase, CityLabel );
					Node city4 = outerInstance.getRandomCityNode( outerInstance.EmbeddedDatabase, CityLabel );
					city1.CreateRelationshipTo( bandNode, TestRelationshipTypes.Like );
					city2.CreateRelationshipTo( bandNode, TestRelationshipTypes.Hate );
					city3.CreateRelationshipTo( bandNode, TestRelationshipTypes.Like );
					city4.CreateRelationshipTo( bandNode, TestRelationshipTypes.Neutral );
					return 4;
			  }

			  internal virtual long CreatedRelationships
			  {
				  get
				  {
						return CreatedRelationshipsConflict;
				  }
			  }
		 }

		 private class RelationshipCalculator : ThreadStart
		 {
			 private readonly RelationshipIdReuseStressIT _outerInstance;

			  internal readonly AtomicBoolean StopFlag;
			  internal readonly Label BandLabel;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal int RelationshipSizeConflict;

			  internal RelationshipCalculator( RelationshipIdReuseStressIT outerInstance, AtomicBoolean stopFlag, Label bandLabel )
			  {
				  this._outerInstance = outerInstance;
					this.StopFlag = stopFlag;
					this.BandLabel = bandLabel;
			  }

			  public override void Run()
			  {
					while ( !StopFlag.get() )
					{
						 using ( Transaction transaction = outerInstance.EmbeddedDatabase.beginTx() )
						 {
							  Node randomBandNode = outerInstance.getRandomBandNode( outerInstance.EmbeddedDatabase, BandLabel );
							  RelationshipSizeConflict = Iterables.asList( randomBandNode.Relationships ).Count;
							  transaction.Success();
						 }
						 long millisToWait = ThreadLocalRandom.current().nextLong(10, 25);
						 LockSupport.parkNanos( TimeUnit.MILLISECONDS.toNanos( millisToWait ) );
					}
			  }

			  public virtual int RelationshipSize
			  {
				  get
				  {
						return RelationshipSizeConflict;
				  }
			  }
		 }

		 private class RelationshipTypeCalculator : ThreadStart
		 {
			 private readonly RelationshipIdReuseStressIT _outerInstance;

			  internal readonly AtomicBoolean StopFlag;
			  internal readonly Label BandLabel;
			  internal int RelationshipSize;

			  internal RelationshipTypeCalculator( RelationshipIdReuseStressIT outerInstance, AtomicBoolean stopFlag, Label bandLabel )
			  {
				  this._outerInstance = outerInstance;
					this.StopFlag = stopFlag;
					this.BandLabel = bandLabel;
			  }

			  public override void Run()
			  {
					while ( !StopFlag.get() )
					{
						 using ( Transaction transaction = outerInstance.EmbeddedDatabase.beginTx() )
						 {
							  Node randomBandNode = outerInstance.getRandomBandNode( outerInstance.EmbeddedDatabase, BandLabel );
							  RelationshipSize = Iterables.asList( randomBandNode.RelationshipTypes ).Count;
							  transaction.Success();
						 }
						 long millisToWait = ThreadLocalRandom.current().nextLong(10, 25);
						 LockSupport.parkNanos( TimeUnit.MILLISECONDS.toNanos( millisToWait ) );
					}
			  }
		 }

		 private class RelationshipRemover : ThreadStart
		 {
			 private readonly RelationshipIdReuseStressIT _outerInstance;

			  internal readonly Label BandLabel;
			  internal readonly Label CityLabel;
			  internal readonly AtomicBoolean StopFlag;

			  internal volatile int RemovalCount;

			  internal RelationshipRemover( RelationshipIdReuseStressIT outerInstance, Label bandLabel, Label cityLabel, AtomicBoolean stopFlag )
			  {
				  this._outerInstance = outerInstance;
					this.BandLabel = bandLabel;
					this.CityLabel = cityLabel;
					this.StopFlag = stopFlag;
			  }

			  public override void Run()
			  {
					while ( !StopFlag.get() )
					{
						 try
						 {
								 using ( Transaction transaction = outerInstance.EmbeddedDatabase.beginTx() )
								 {
								  bool deleteOnBands = ThreadLocalRandom.current().nextBoolean();
								  if ( deleteOnBands )
								  {
										DeleteRelationshipOfRandomType();
								  }
								  else
								  {
										DeleteRelationshipOnRandomNode();
      
								  }
								  transaction.Success();
								  RemovalCount++;
								 }
						 }
						 catch ( Exception ignored ) when ( ignored is DeadlockDetectedException || ignored is NotFoundException )
						 {
							  // ignore deadlocks
						 }
						 LockSupport.parkNanos( TimeUnit.MILLISECONDS.toNanos( 15 ) );
					}
			  }

			  internal virtual int RemovedRelationships
			  {
				  get
				  {
						return RemovalCount;
				  }
			  }

			  internal virtual void DeleteRelationshipOfRandomType()
			  {
					Node bandNode = outerInstance.getRandomBandNode( outerInstance.EmbeddedDatabase, BandLabel );
					TestRelationshipTypes relationshipType = outerInstance.RandomRelationshipType;
					IEnumerable<Relationship> relationships = bandNode.GetRelationships( relationshipType, outerInstance.RandomDirection );
					foreach ( Relationship relationship in relationships )
					{
						 relationship.Delete();
					}
			  }

			  internal virtual void DeleteRelationshipOnRandomNode()
			  {
					using ( ResourceIterator<Node> nodeResourceIterator = outerInstance.EmbeddedDatabase.findNodes( CityLabel ) )
					{
						 IList<Node> nodes = Iterators.asList( nodeResourceIterator );
						 int index = ThreadLocalRandom.current().Next(nodes.Count);
						 Node node = nodes[index];
						 foreach ( Relationship relationship in node.Relationships )
						 {
							  relationship.Delete();
						 }
					}
			  }
		 }
	}

}