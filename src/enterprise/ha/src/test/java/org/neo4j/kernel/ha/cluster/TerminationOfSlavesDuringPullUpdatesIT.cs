using System;
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
namespace Neo4Net.Kernel.ha.cluster
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;
	using Parameter = org.junit.runners.Parameterized.Parameter;
	using StringUtils = org.parboiled.common.StringUtils;


	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Node = Neo4Net.GraphDb.Node;
	using IPropertyContainer = Neo4Net.GraphDb.PropertyContainer;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using TransactionTerminatedException = Neo4Net.GraphDb.TransactionTerminatedException;
	using TransientTransactionFailureException = Neo4Net.GraphDb.TransientTransactionFailureException;
	using Strings = Neo4Net.Helpers.Strings;
	using ClusterManager = Neo4Net.Kernel.impl.ha.ClusterManager;
	using IdController = Neo4Net.Kernel.impl.storageengine.impl.recordstorage.id.IdController;
	using Race = Neo4Net.Test.Race;
	using ClusterRule = Neo4Net.Test.ha.ClusterRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.collection.IsIn.isIn;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.runners.Parameterized.Parameters;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.RelationshipType.withName;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterables.asSet;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class TerminationOfSlavesDuringPullUpdatesIT
	public class TerminationOfSlavesDuringPullUpdatesIT
	{
		 private const int READER_CONTESTANTS = 20;
		 private const int STRING_LENGTH = 20000;
		 private const int PROPERTY_KEY_CHAIN_LENGTH = 100;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.Neo4Net.test.ha.ClusterRule clusterRule = new org.Neo4Net.test.ha.ClusterRule().withSharedSetting(org.Neo4Net.kernel.ha.HaSettings.pull_interval, "0").withSharedSetting(org.Neo4Net.kernel.ha.HaSettings.tx_push_factor, "0");
		 public ClusterRule ClusterRule = new ClusterRule().withSharedSetting(HaSettings.pull_interval, "0").withSharedSetting(HaSettings.tx_push_factor, "0");

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameter public ReadContestantActions action;
		 public ReadContestantActions Action;
		 [Parameter(1)]
		 public string Name;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameters(name = "{1}") public static Iterable<Object> data()
		 public static IEnumerable<object> Data()
		 {
			  return Arrays.asList(new object[][]
			  {
				  new object[]
				  {
					  new PropertyValueActions( LongString( 'a' ), LongString( 'b' ), true, ( db, IEntityId, key, node ) => GetEntity( db, IEntityId, node ).AllProperties[key] ),
					  "NodeStringProperty[allProps]"
				  },
				  new object[]
				  {
					  new PropertyValueActions( LongString( 'a' ), LongString( 'b' ), true, ( db, IEntityId, key, node ) => GetEntity( db, IEntityId, node ).getProperty( key, null ) ),
					  "NodeStringProperty[singleProp]"
				  },
				  new object[]
				  {
					  new PropertyValueActions( LongString( 'a' ), LongString( 'b' ), true, ( db, IEntityId, key, node ) => GetEntity( db, IEntityId, node ).getProperties( key )[key] ),
					  "NodeStringProperty[varArgsProp]"
				  },
				  new object[]
				  {
					  new PropertyValueActions( LongString( 'a' ), LongString( 'b' ), false, ( db, IEntityId, key, node ) => GetEntity( db, IEntityId, node ).AllProperties[key] ),
					  "RelationshipStringProperty[allProps]"
				  },
				  new object[]
				  {
					  new PropertyValueActions( LongString( 'a' ), LongString( 'b' ), false, ( db, IEntityId, key, node ) => GetEntity( db, IEntityId, node ).getProperty( key, null ) ),
					  "RelationshipStringProperty[singleProp]"
				  },
				  new object[]
				  {
					  new PropertyValueActions( LongString( 'a' ), LongString( 'b' ), false, ( db, IEntityId, key, node ) => GetEntity( db, IEntityId, node ).getProperties( key )[key] ),
					  "RelationshipStringProperty[varArgsProp]"
				  },
				  new object[]
				  {
					  new PropertyValueActions( LongArray( 'a' ), LongArray( 'b' ), true, ( db, IEntityId, key, node ) => GetEntity( db, IEntityId, node ).AllProperties[key] ),
					  "NodeArrayProperty[allProps]"
				  },
				  new object[]
				  {
					  new PropertyValueActions( LongArray( 'a' ), LongArray( 'b' ), true, ( db, IEntityId, key, node ) => GetEntity( db, IEntityId, node ).getProperty( key, null ) ),
					  "NodeArrayProperty[singleProp]"
				  },
				  new object[]
				  {
					  new PropertyValueActions( LongArray( 'a' ), LongArray( 'b' ), true, ( db, IEntityId, key, node ) => GetEntity( db, IEntityId, node ).getProperties( key )[key] ),
					  "NodeArrayProperty[varArgsProp]"
				  },
				  new object[]
				  {
					  new PropertyValueActions( LongArray( 'a' ), LongArray( 'b' ), false, ( db, IEntityId, key, node ) => GetEntity( db, IEntityId, node ).AllProperties[key] ),
					  "RelationshipArrayProperty[allProps]"
				  },
				  new object[]
				  {
					  new PropertyValueActions( LongArray( 'a' ), LongArray( 'b' ), false, ( db, IEntityId, key, node ) => GetEntity( db, IEntityId, node ).getProperty( key, null ) ),
					  "RelationshipArrayProperty[singleProp]"
				  },
				  new object[]
				  {
					  new PropertyValueActions( LongArray( 'a' ), LongArray( 'b' ), false, ( db, IEntityId, key, node ) => GetEntity( db, IEntityId, node ).getProperties( key )[key] ),
					  "RelationshipArrayProperty[varArgsProp]"
				  },
				  new object[]
				  {
					  new PropertyKeyActions( 'a', 'b', true ),
					  "NodePropertyKeys"
				  },
				  new object[]
				  {
					  new PropertyKeyActions( 'a', 'b', false ),
					  "RelationshipPropertyKeys"
				  }
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void slavesTerminateOrReadConsistentDataWhenApplyingBatchLargerThanSafeZone() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SlavesTerminateOrReadConsistentDataWhenApplyingBatchLargerThanSafeZone()
		 {
			  long safeZone = TimeUnit.MILLISECONDS.toSeconds( 0 );
			  ClusterRule.withSharedSetting( HaSettings.id_reuse_safe_zone_time, safeZone.ToString() );
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.ha.ClusterManager.ManagedCluster cluster = clusterRule.startCluster();
			  ClusterManager.ManagedCluster cluster = ClusterRule.startCluster();
			  HighlyAvailableGraphDatabase master = cluster.Master;

			  // when
			  // ... slaves and master has node with long string property
			  long IEntityId = Action.createInitialEntity( master );
			  cluster.Sync();
			  // ... and property is removed on master
			  Action.removeProperties( master, IEntityId );
			  Thread.Sleep( 100 );
			  // ... and maintenance is called to make sure "safe" ids are freed to be reused
			  ForceMaintenance( master );
			  // ... and a new property is created on master that
			  Action.setNewProperties( master, IEntityId );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.ha.HighlyAvailableGraphDatabase slave = cluster.getAnySlave();
			  HighlyAvailableGraphDatabase slave = cluster.AnySlave;
			  Race race = new Race();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicBoolean end = new java.util.concurrent.atomic.AtomicBoolean(false);
			  AtomicBoolean end = new AtomicBoolean( false );
			  for ( int i = 0; i < READER_CONTESTANTS; i++ )
			  {
					race.AddContestant( ReadContestant( Action, IEntityId, slave, end ) );
			  }

			  race.AddContestant( PullUpdatesContestant( slave, end ) );

			  race.Go();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void slavesDontTerminateAndReadConsistentDataWhenApplyingBatchSmallerThanSafeZone() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SlavesDontTerminateAndReadConsistentDataWhenApplyingBatchSmallerThanSafeZone()
		 {
			  long safeZone = TimeUnit.MINUTES.toSeconds( 1 );
			  ClusterRule.withSharedSetting( HaSettings.id_reuse_safe_zone_time, safeZone.ToString() );
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.ha.ClusterManager.ManagedCluster cluster = clusterRule.startCluster();
			  ClusterManager.ManagedCluster cluster = ClusterRule.startCluster();
			  HighlyAvailableGraphDatabase master = cluster.Master;

			  // when
			  // ... slaves and master has node with long string property
			  long IEntityId = Action.createInitialEntity( master );
			  cluster.Sync();
			  // ... and property is removed on master
			  Action.removeProperties( master, IEntityId );
			  // ... and maintenance is called to make sure "safe" ids are freed to be reused
			  ForceMaintenance( master );
			  // ... and a new property is created on master that
			  Action.setNewProperties( master, IEntityId );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.ha.HighlyAvailableGraphDatabase slave = cluster.getAnySlave();
			  HighlyAvailableGraphDatabase slave = cluster.AnySlave;
			  Race race = new Race();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicBoolean end = new java.util.concurrent.atomic.AtomicBoolean(false);
			  AtomicBoolean end = new AtomicBoolean( false );
			  for ( int i = 0; i < READER_CONTESTANTS; i++ )
			  {
					race.AddContestant( ReadContestant( Action, IEntityId, slave, end ) );
			  }

			  race.AddContestant( PullUpdatesContestant( slave, end ) );

			  race.Go();
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private Runnable readContestant(final ReadContestantActions action, final long IEntityId, final org.Neo4Net.kernel.ha.HighlyAvailableGraphDatabase slave, final java.util.concurrent.atomic.AtomicBoolean end)
		 private ThreadStart ReadContestant( ReadContestantActions action, long IEntityId, HighlyAvailableGraphDatabase slave, AtomicBoolean end )
		 {
			  return () =>
			  {
				while ( !end.get() )
				{
					 try
					 {
						 using ( Transaction tx = slave.BeginTx() )
						 {
							  for ( int i = 0; i < 10; i++ )
							  {
									action.VerifyProperties( slave, IEntityId );
							  }
   
							  tx.success();
						 }
					 }
					 catch ( Exception ignored ) when ( ignored is TransactionTerminatedException || ignored is TransientTransactionFailureException )
					 {
					 }
				}
			  };
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private Runnable pullUpdatesContestant(final org.Neo4Net.kernel.ha.HighlyAvailableGraphDatabase slave, final java.util.concurrent.atomic.AtomicBoolean end)
		 private ThreadStart PullUpdatesContestant( HighlyAvailableGraphDatabase slave, AtomicBoolean end )
		 {
			  return () =>
			  {
				try
				{
					 Random rnd = new Random();
					 Thread.Sleep( rnd.Next( 100 ) );
					 slave.DependencyResolver.resolveDependency( typeof( UpdatePuller ) ).pullUpdates();
				}
				catch ( InterruptedException e )
				{
					 throw new Exception( e );
				}
				finally
				{
					 end.set( true );
				}

			  };
		 }

		 private interface ReadContestantActions
		 {
			  long CreateInitialEntity( HighlyAvailableGraphDatabase db );

			  void RemoveProperties( HighlyAvailableGraphDatabase db, long IEntityId );

			  void SetNewProperties( HighlyAvailableGraphDatabase db, long IEntityId );

			  void VerifyProperties( HighlyAvailableGraphDatabase db, long IEntityId );
		 }

		 private delegate object FetchProperty( IGraphDatabaseService db, long IEntityId, string key, bool node );

		 private class PropertyValueActions : ReadContestantActions
		 {
			  internal const string KEY = "key";
			  internal readonly object ValueA;
			  internal readonly object ValueB;
			  internal readonly bool Node;
			  internal readonly FetchProperty FetchProperty;

			  internal PropertyValueActions( object valueA, object valueB, bool node, FetchProperty fetchProperty )
			  {
					this.ValueA = valueA;
					this.ValueB = valueB;
					this.Node = node;
					this.FetchProperty = fetchProperty;
			  }

			  public override long CreateInitialEntity( HighlyAvailableGraphDatabase db )
			  {
					using ( Transaction tx = Db.beginTx() )
					{
						 long id;
						 if ( Node )
						 {
							  id = CreateInitialNode( db );
						 }
						 else
						 {
							  id = CreateInitialRelationship( db );
						 }
						 tx.Success();
						 return id;
					}
			  }

			  internal virtual long CreateInitialNode( HighlyAvailableGraphDatabase db )
			  {
					Node node = Db.createNode();
					node.SetProperty( KEY, ValueA );
					return node.Id;
			  }

			  internal virtual long CreateInitialRelationship( HighlyAvailableGraphDatabase db )
			  {
					Node start = Db.createNode();
					Node end = Db.createNode();
					Relationship relationship = start.CreateRelationshipTo( end, withName( "KNOWS" ) );
					relationship.SetProperty( KEY, ValueA );
					return relationship.Id;
			  }

			  public override void RemoveProperties( HighlyAvailableGraphDatabase db, long IEntityId )
			  {
					using ( Transaction tx = Db.beginTx() )
					{
						 GetEntity( db, IEntityId, Node ).removeProperty( KEY );
						 tx.Success();
					}
			  }

			  public override void SetNewProperties( HighlyAvailableGraphDatabase db, long IEntityId )
			  {
					using ( Transaction tx = Db.beginTx() )
					{
						 GetEntity( db, IEntityId, Node ).setProperty( KEY, ValueB );
						 tx.Success();
					}
			  }

			  public override void VerifyProperties( HighlyAvailableGraphDatabase db, long IEntityId )
			  {
					object value = FetchProperty( db, IEntityId, KEY, Node );
					AssertPropertyValue( value, ValueA, ValueB );
			  }
		 }

		 private static IPropertyContainer GetEntity( IGraphDatabaseService db, long id, bool node )
		 {
			  return node ? Db.getNodeById( id ) : Db.getRelationshipById( id );
		 }

		 private class PropertyKeyActions : ReadContestantActions
		 {
			  internal readonly char KeyPrefixA;
			  internal readonly char KeyPrefixB;
			  internal readonly bool Node;

			  internal PropertyKeyActions( char keyPrefixA, char keyPrefixB, bool node )
			  {
					this.KeyPrefixA = keyPrefixA;
					this.KeyPrefixB = keyPrefixB;
					this.Node = node;
			  }

			  public override long CreateInitialEntity( HighlyAvailableGraphDatabase db )
			  {
					using ( Transaction tx = Db.beginTx() )
					{
						 long id;
						 if ( Node )
						 {
							  id = CreateInitialNode( db );
						 }
						 else
						 {
							  id = CreateInitialRelationship( db );
						 }
						 tx.Success();
						 return id;
					}
			  }

			  internal virtual long CreateInitialNode( HighlyAvailableGraphDatabase db )
			  {
					Node node = Db.createNode();
					CreatePropertyChain( node, KeyPrefixA );
					return node.Id;
			  }

			  internal virtual long CreateInitialRelationship( HighlyAvailableGraphDatabase db )
			  {
					Node start = Db.createNode();
					Node end = Db.createNode();
					Relationship relationship = start.CreateRelationshipTo( end, withName( "KNOWS" ) );
					CreatePropertyChain( relationship, KeyPrefixA );
					return relationship.Id;
			  }

			  internal virtual void CreatePropertyChain( IPropertyContainer IEntity, char prefix )
			  {
					for ( int i = 0; i < PROPERTY_KEY_CHAIN_LENGTH; i++ )
					{
						 IEntity.SetProperty( "" + prefix + i, i );
					}
			  }

			  public override void RemoveProperties( HighlyAvailableGraphDatabase db, long IEntityId )
			  {
					using ( Transaction tx = Db.beginTx() )
					{
						 IPropertyContainer IEntity = GetEntity( db, IEntityId );
						 foreach ( string key in IEntity.PropertyKeys )
						 {
							  IEntity.RemoveProperty( key );
						 }
						 tx.Success();
					}
			  }

			  public override void SetNewProperties( HighlyAvailableGraphDatabase db, long IEntityId )
			  {
					using ( Transaction tx = Db.beginTx() )
					{
						 CreatePropertyChain( GetEntity( db, IEntityId ), KeyPrefixB );
						 tx.Success();
					}
			  }

			  public override void VerifyProperties( HighlyAvailableGraphDatabase db, long IEntityId )
			  {
					AssertPropertyChain( asSet( GetEntity( db, IEntityId ).PropertyKeys ), KeyPrefixA, KeyPrefixB );
			  }

			  internal virtual IPropertyContainer GetEntity( HighlyAvailableGraphDatabase db, long id )
			  {
					return Node ? Db.getNodeById( id ) : Db.getRelationshipById( id );
			  }
		 }

		 private static object LongArray( char b )
		 {
			  return LongString( b ).ToCharArray();
		 }

		 private static string LongString( char ch )
		 {
			  return StringUtils.repeat( ch, STRING_LENGTH );
		 }

		 private void ForceMaintenance( HighlyAvailableGraphDatabase master )
		 {
			  master.DependencyResolver.resolveDependency( typeof( IdController ) ).maintenance();
		 }

		 private static void AssertPropertyValue( object property, params object[] candidates )
		 {
			  if ( property == null )
			  {
					return;
			  }
			  foreach ( object candidate in candidates )
			  {
					if ( Objects.deepEquals( property, candidate ) )
					{
						 return;
					}
			  }
			  fail( "property value was " + Strings.prettyPrint( property ) );
		 }

		 private static void AssertPropertyChain( ISet<string> allProperties, params Character[] keyPrefix )
		 {
			  bool first = true;
			  char? actualFirst = null;
			  foreach ( string key in allProperties )
			  {
					if ( first )
					{
						 first = false;
						 actualFirst = key[0];
						 assertThat( "Other prefix than expected", actualFirst, isIn( keyPrefix ) );
					}
					assertThat( "Property key chain is broken " + Arrays.ToString( allProperties.ToArray() ), key[0], equalTo(actualFirst) );
			  }
		 }
	}

}