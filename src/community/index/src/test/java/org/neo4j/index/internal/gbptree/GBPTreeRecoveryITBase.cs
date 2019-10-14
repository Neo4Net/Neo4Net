using System;
using System.Collections;
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
namespace Neo4Net.Index.Internal.gbptree
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;


	using Neo4Net.Cursors;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using PageCacheRule = Neo4Net.Test.rule.PageCacheRule;
	using RandomRule = Neo4Net.Test.rule.RandomRule;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using EphemeralFileSystemRule = Neo4Net.Test.rule.fs.EphemeralFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.rules.RuleChain.outerRule;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.Internal.gbptree.ThrowingRunnable.throwing;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.pagecache.IOLimiter_Fields.UNLIMITED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.rule.PageCacheRule.config;

	public abstract class GBPTreeRecoveryITBase<KEY, VALUE>
	{
		private bool InstanceFieldsInitialized = false;

		public GBPTreeRecoveryITBase()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_directory = TestDirectory.testDirectory( this.GetType(), _fs.get() );
			Rules = outerRule( _fs ).around( _directory ).around( _pageCacheRule ).around( _random );
			_checkpoint = new CheckpointAction( this );
		}

		 private readonly EphemeralFileSystemRule _fs = new EphemeralFileSystemRule();
		 private TestDirectory _directory;
		 private readonly PageCacheRule _pageCacheRule = new PageCacheRule( config().withPageSize(PAGE_SIZE).withAccessChecks(true) );
		 private readonly RandomRule _random = new RandomRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.RuleChain rules = outerRule(fs).around(directory).around(pageCacheRule).around(random);
		 public RuleChain Rules;

		 // Test config
		 private int _loadCountTransactions;
		 private int _minInsertCountPerBatch;
		 private int _maxInsertCountPerBatch;
		 private int _minRemoveCountPerBatch;
		 private int _maxRemoveCountPerBatch;

		 private const int PAGE_SIZE = 256;
		 private Action _checkpoint;

		 private TestLayout<KEY, VALUE> _layout;

		 /* Global variables for recoverFromAnything test */
		 private bool _recoverFromAnythingInitialized;
		 private int _keyRange;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  this._layout = GetLayout( _random );
			  _loadCountTransactions = _random.intBetween( 300, 1_000 );
			  _minInsertCountPerBatch = 30;
			  _maxInsertCountPerBatch = 200;
			  _minRemoveCountPerBatch = 5;
			  _maxRemoveCountPerBatch = 20;
		 }

		 protected internal abstract TestLayout<KEY, VALUE> GetLayout( RandomRule random );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRecoverFromCrashBeforeFirstCheckpoint() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRecoverFromCrashBeforeFirstCheckpoint()
		 {
			  // GIVEN
			  // a tree with only small amount of data that has not yet seen checkpoint from outside
			  KEY key = key( 1L );
			  VALUE value = value( 10L );
			  File file = _directory.file( "index" );
			  {
					using ( PageCache pageCache = CreatePageCache(), GBPTree<KEY, VALUE> index = CreateIndex(pageCache, file), Writer<KEY, VALUE> writer = index.Writer() )
					{
						 writer.Put( key, value );
						 pageCache.FlushAndForce();
						 // No checkpoint
					}
			  }

			  // WHEN
			  using ( PageCache pageCache = CreatePageCache(), GBPTree<KEY, VALUE> index = CreateIndex(pageCache, file) )
			  {
					using ( Writer<KEY, VALUE> writer = index.Writer() )
					{
						 writer.Put( key, value );
					}

					// THEN
					// we should end up with a consistent index
					index.ConsistencyCheck();

					// ... containing all the stuff load says
					using ( RawCursor<Hit<KEY, VALUE>, IOException> cursor = index.Seek( key( long.MinValue ), key( long.MaxValue ) ) )
					{
						 assertTrue( cursor.Next() );
						 Hit<KEY, VALUE> hit = cursor.get();
						 AssertEqualsKey( key, hit.Key() );
						 AssertEqualsValue( value, hit.Value() );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRecoverFromAnythingReplayExactFromCheckpointHighKeyContention() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRecoverFromAnythingReplayExactFromCheckpointHighKeyContention()
		 {
			  InitializeRecoveryFromAnythingTest( 100 );
			  DoShouldRecoverFromAnything( true );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRecoverFromAnythingReplayFromBeforeLastCheckpointHighKeyContention() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRecoverFromAnythingReplayFromBeforeLastCheckpointHighKeyContention()
		 {
			  InitializeRecoveryFromAnythingTest( 100 );
			  DoShouldRecoverFromAnything( false );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRecoverFromAnythingReplayExactFromCheckpointLowKeyContention() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRecoverFromAnythingReplayExactFromCheckpointLowKeyContention()
		 {
			  InitializeRecoveryFromAnythingTest( 1_000_000 );
			  DoShouldRecoverFromAnything( true );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRecoverFromAnythingReplayFromBeforeLastCheckpointLowKeyContention() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRecoverFromAnythingReplayFromBeforeLastCheckpointLowKeyContention()
		 {
			  InitializeRecoveryFromAnythingTest( 1_000_000 );
			  DoShouldRecoverFromAnything( false );
		 }

		 private void InitializeRecoveryFromAnythingTest( int keyRange )
		 {
			  _recoverFromAnythingInitialized = true;
			  this._keyRange = keyRange;
		 }

		 private void AssertInitialized()
		 {
			  assertTrue( _recoverFromAnythingInitialized );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void doShouldRecoverFromAnything(boolean replayRecoveryExactlyFromCheckpoint) throws Exception
		 private void DoShouldRecoverFromAnything( bool replayRecoveryExactlyFromCheckpoint )
		 {
			  AssertInitialized();
			  // GIVEN
			  // a tree which has had random updates and checkpoints in it, load generated with specific seed
			  File file = _directory.file( "index" );
			  IList<Action> load = GenerateLoad();
			  IList<Action> shuffledLoad = RandomCausalAwareShuffle( load );
			  int lastCheckPointIndex = IndexOfLastCheckpoint( load );

			  {
					// _,_,_,_,_,_,_,c,_,_,_,_,_,_,_,_,_,_,_,_,_,_,_,_,c,_,_,_,_,_,_,_,_,_,_,_
					//                                                 ^             ^
					//                                                 |             |------------ crash flush index
					//                                                 |-------------------------- last checkpoint index
					//

					PageCache pageCache = CreatePageCache();
					GBPTree<KEY, VALUE> index = CreateIndex( pageCache, file );
					// Execute all actions up to and including last checkpoint ...
					Execute( shuffledLoad.subList( 0, lastCheckPointIndex + 1 ), index );
					// ... a random amount of the remaining "unsafe" actions ...
					int numberOfRemainingActions = shuffledLoad.Count - lastCheckPointIndex - 1;
					int crashFlushIndex = lastCheckPointIndex + _random.Next( numberOfRemainingActions ) + 1;
					Execute( shuffledLoad.subList( lastCheckPointIndex + 1, crashFlushIndex ), index );
					// ... flush ...
					pageCache.FlushAndForce();
					// ... execute the remaining actions
					Execute( shuffledLoad.subList( crashFlushIndex, shuffledLoad.Count ), index );
					// ... and finally crash
					_fs.snapshot(throwing(() =>
					{
					 index.Dispose();
					 pageCache.Close();
					}));
			  }

			  // WHEN doing recovery
			  IList<Action> recoveryActions;
			  if ( replayRecoveryExactlyFromCheckpoint )
			  {
					recoveryActions = recoveryActions( load, lastCheckPointIndex + 1 );
			  }
			  else
			  {
					recoveryActions = recoveryActions( load, _random.Next( lastCheckPointIndex + 1 ) );
			  }

			  // first crashing during recovery
			  int numberOfCrashesDuringRecovery = _random.intBetween( 0, 3 );
			  for ( int i = 0; i < numberOfCrashesDuringRecovery; i++ )
			  {
					using ( PageCache pageCache = CreatePageCache(), GBPTree<KEY, VALUE> index = CreateIndex(pageCache, file) )
					{
						 int numberOfActionsToRecoverBeforeCrashing = _random.intBetween( 1, recoveryActions.Count );
						 Recover( recoveryActions.subList( 0, numberOfActionsToRecoverBeforeCrashing ), index );
						 // ... crash
					}
			  }

			  // to finally apply all actions after last checkpoint and verify tree
			  using ( PageCache pageCache = CreatePageCache(), GBPTree<KEY, VALUE> index = CreateIndex(pageCache, file) )
			  {
					Recover( recoveryActions, index );

					// THEN
					// we should end up with a consistent index containing all the stuff load says
					index.ConsistencyCheck();
					long[] aggregate = ExpectedSortedAggregatedDataFromGeneratedLoad( load );
					using ( RawCursor<Hit<KEY, VALUE>, IOException> cursor = index.Seek( Key( long.MinValue ), Key( long.MaxValue ) ) )
					{
						 for ( int i = 0; i < aggregate.Length; )
						 {
							  assertTrue( cursor.Next() );
							  Hit<KEY, VALUE> hit = cursor.get();
							  AssertEqualsKey( Key( aggregate[i++] ), hit.Key() );
							  AssertEqualsValue( Value( aggregate[i++] ), hit.Value() );
						 }
						 assertFalse( cursor.Next() );
					}
			  }
		 }

		 /// <summary>
		 /// Shuffle actions without breaking causal dependencies, i.e. without affecting the end result
		 /// of the data ending up in the tree. Checkpoints cannot move.
		 /// 
		 /// On an integration level with neo4j, this is done because of the nature of how concurrent transactions
		 /// are applied in random order and recovery applies transactions in order of transaction id.
		 /// </summary>
		 private IList<Action> RandomCausalAwareShuffle( IList<Action> actions )
		 {
			  //noinspection unchecked
			  Action[] arrayToShuffle = actions.toArray( ( Action[] ) Array.CreateInstance( typeof( Action ), actions.Count ) );
			  int size = arrayToShuffle.Length;
			  int numberOfActionsToShuffle = _random.Next( size / 2 );

			  for ( int i = 0; i < numberOfActionsToShuffle; i++ )
			  {
					int actionIndexToMove = _random.Next( size );
					int stride = _random.nextBoolean() ? 1 : -1;
					int maxNumberOfSteps = _random.Next( 10 ) + 1;

					for ( int steps = 0; steps < maxNumberOfSteps; steps++ )
					{
						 Action actionToMove = arrayToShuffle[actionIndexToMove];
						 int actionIndexToSwap = actionIndexToMove + stride;
						 if ( actionIndexToSwap < 0 || actionIndexToSwap >= size )
						 {
							  break;
						 }
						 Action actionToSwap = arrayToShuffle[actionIndexToSwap];

						 if ( actionToMove.HasCausalDependencyWith( actionToSwap ) )
						 {
							  break;
						 }

						 arrayToShuffle[actionIndexToMove] = actionToSwap;
						 arrayToShuffle[actionIndexToSwap] = actionToMove;

						 actionIndexToMove = actionIndexToSwap;
					}
			  }
			  return Arrays.asList( arrayToShuffle );
		 }

		 private IList<Action> RecoveryActions( IList<Action> load, int fromIndex )
		 {
			  return load.subList( fromIndex, load.Count ).Where( action => !action.Checkpoint ).ToList();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void recover(java.util.List<Action> load, GBPTree<KEY,VALUE> index) throws java.io.IOException
		 private void Recover( IList<Action> load, GBPTree<KEY, VALUE> index )
		 {
			  Execute( load, index );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void execute(java.util.List<Action> load, GBPTree<KEY,VALUE> index) throws java.io.IOException
		 private void Execute( IList<Action> load, GBPTree<KEY, VALUE> index )
		 {
			  foreach ( Action action in load )
			  {
					action.Execute( index );
			  }
		 }

		 private long[] ExpectedSortedAggregatedDataFromGeneratedLoad( IList<Action> load )
		 {
			  SortedDictionary<long, long> map = new SortedDictionary<long, long>();
			  foreach ( Action action in load )
			  {
					long[] data = action.Data();
					if ( data != null )
					{
						 for ( int i = 0; i < data.Length; )
						 {
							  long key = data[i++];
							  long value = data[i++];
							  if ( action.Type() == ActionType.Insert )
							  {
									map[key] = value;
							  }
							  else if ( action.Type() == ActionType.Remove )
							  {
									map.Remove( key );
							  }
							  else
							  {
									throw new System.NotSupportedException( action.ToString() );
							  }
						 }
					}
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") java.util.Map.Entry<long,long>[] entries = map.entrySet().toArray(new java.util.Map.Entry[map.size()]);
			  KeyValuePair<long, long>[] entries = map.SetOfKeyValuePairs().toArray(new DictionaryEntry[map.Count]);
			  long[] result = new long[entries.Length * 2];
			  for ( int i = 0, c = 0; i < entries.Length; i++ )
			  {
					result[c++] = entries[i].Key;
					result[c++] = entries[i].Value;
			  }
			  return result;
		 }

		 private int IndexOfLastCheckpoint( IList<Action> actions )
		 {
			  int i = 0;
			  int lastCheckpoint = -1;
			  foreach ( Action action in actions )
			  {
					if ( action.Checkpoint )
					{
						 lastCheckpoint = i;
					}
					i++;
			  }
			  return lastCheckpoint;
		 }

		 private IList<Action> GenerateLoad()
		 {
			  IList<Action> actions = new LinkedList<Action>();
			  bool hasCheckPoint = false;
			  for ( int i = 0; i < _loadCountTransactions; i++ )
			  {
					Action action = RandomAction( true );
					actions.Add( action );
					if ( action == _checkpoint )
					{
						 hasCheckPoint = true;
					}
			  }

			  // Guarantee that there's at least one check point, i.e. if there's none then append one at the end
			  if ( !hasCheckPoint )
			  {
					actions.Add( _checkpoint );
			  }

			  // Guarantee that there are at least some non-checkpoint actions after last checkpoint
			  if ( actions[actions.Count - 1] == _checkpoint )
			  {
					int additional = _random.intBetween( 1, 10 );
					for ( int i = 0; i < additional; i++ )
					{
						 actions.Add( RandomAction( false ) );
					}
			  }
			  return actions;
		 }

		 private Action RandomAction( bool allowCheckPoint )
		 {
			  float randomized = _random.nextFloat();
			  if ( randomized <= 0.7 )
			  {
					// put
					long[] data = ModificationData( _minInsertCountPerBatch, _maxInsertCountPerBatch );
					return new InsertAction( this, data );
			  }
			  else if ( randomized <= 0.95 || !allowCheckPoint )
			  {
					// remove
					long[] data = ModificationData( _minRemoveCountPerBatch, _maxRemoveCountPerBatch );
					return new RemoveAction( this, data );
			  }
			  else
			  {
					return _checkpoint;
			  }
		 }

		 private long[] ModificationData( int min, int max )
		 {
			  int count = _random.intBetween( min, max );
			  long[] data = new long[count * 2];
			  for ( int i = 0, c = 0; i < count; i++ )
			  {
					data[c++] = _random.intBetween( 0, _keyRange ); // key
					data[c++] = _random.intBetween( 0, _keyRange ); // value
			  }
			  return data;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private GBPTree<KEY,VALUE> createIndex(org.neo4j.io.pagecache.PageCache pageCache, java.io.File file) throws java.io.IOException
		 private GBPTree<KEY, VALUE> CreateIndex( PageCache pageCache, File file )
		 {
			  return ( new GBPTreeBuilder<KEY, VALUE>( pageCache, file, _layout ) ).build();
		 }

		 private PageCache CreatePageCache()
		 {
			  return _pageCacheRule.getPageCache( _fs.get() );
		 }

		 internal enum ActionType
		 {
			  Insert,
			  Remove,
			  Checkpoint
		 }

		 internal abstract class Action
		 {
			 private readonly GBPTreeRecoveryITBase<KEY, VALUE> _outerInstance;

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal long[] DataConflict;
			  internal ISet<long> AllKeys;

			  internal Action( GBPTreeRecoveryITBase<KEY, VALUE> outerInstance, long[] data )
			  {
				  this._outerInstance = outerInstance;
					this.DataConflict = data;
					this.AllKeys = KeySet( data );
			  }

			  internal virtual long[] Data()
			  {
					return DataConflict;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: abstract void execute(GBPTree<KEY,VALUE> index) throws java.io.IOException;
			  internal abstract void Execute( GBPTree<KEY, VALUE> index );

			  internal abstract bool Checkpoint { get; }

			  internal abstract bool HasCausalDependencyWith( Action other );

			  internal virtual ISet<long> KeySet( long[] data )
			  {
					ISet<long> keys = new SortedSet<long>();
					for ( int i = 0; i < data.Length; i += 2 )
					{
						 keys.Add( data[i] );
					}
					return keys;
			  }

			  internal abstract ActionType Type();
		 }

		 internal abstract class DataAction : Action
		 {
			 private readonly GBPTreeRecoveryITBase<KEY, VALUE> _outerInstance;

			  internal DataAction( GBPTreeRecoveryITBase<KEY, VALUE> outerInstance, long[] data ) : base( outerInstance, data )
			  {
				  this._outerInstance = outerInstance;
			  }

			  internal override bool Checkpoint
			  {
				  get
				  {
						return false;
				  }
			  }

			  public override bool HasCausalDependencyWith( Action other )
			  {
					if ( other.Checkpoint )
					{
						 return true;
					}

					ISet<long> intersection = new SortedSet<long>( AllKeys );
//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the java.util.Collection 'retainAll' method:
					intersection.retainAll( other.AllKeys );

					return intersection.Count > 0;
			  }
		 }

		 internal class InsertAction : DataAction
		 {
			 private readonly GBPTreeRecoveryITBase<KEY, VALUE> _outerInstance;

			  internal InsertAction( GBPTreeRecoveryITBase<KEY, VALUE> outerInstance, long[] data ) : base( outerInstance, data )
			  {
				  this._outerInstance = outerInstance;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void execute(GBPTree<KEY,VALUE> index) throws java.io.IOException
			  public override void Execute( GBPTree<KEY, VALUE> index )
			  {
					using ( Writer<KEY, VALUE> writer = index.Writer() )
					{
						 for ( int i = 0; i < DataConflict.Length; )
						 {
							  writer.Put( outerInstance.key( DataConflict[i++] ), outerInstance.value( DataConflict[i++] ) );
						 }
					}
			  }

			  internal override ActionType Type()
			  {
					return ActionType.Insert;
			  }
		 }

		 internal class RemoveAction : DataAction
		 {
			 private readonly GBPTreeRecoveryITBase<KEY, VALUE> _outerInstance;

			  internal RemoveAction( GBPTreeRecoveryITBase<KEY, VALUE> outerInstance, long[] data ) : base( outerInstance, data )
			  {
				  this._outerInstance = outerInstance;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void execute(GBPTree<KEY,VALUE> index) throws java.io.IOException
			  public override void Execute( GBPTree<KEY, VALUE> index )
			  {
					using ( Writer<KEY, VALUE> writer = index.Writer() )
					{
						 for ( int i = 0; i < DataConflict.Length; )
						 {
							  KEY key = key( DataConflict[i++] );
							  i++; // value
							  writer.Remove( key );
						 }
					}
			  }

			  internal override ActionType Type()
			  {
					return ActionType.Remove;
			  }
		 }

		 internal class CheckpointAction : Action
		 {
			 private readonly GBPTreeRecoveryITBase<KEY, VALUE> _outerInstance;

			  internal CheckpointAction( GBPTreeRecoveryITBase<KEY, VALUE> outerInstance ) : base( outerInstance, new long[0] )
			  {
				  this._outerInstance = outerInstance;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void execute(GBPTree<KEY,VALUE> index) throws java.io.IOException
			  public override void Execute( GBPTree<KEY, VALUE> index )
			  {
					index.Checkpoint( UNLIMITED );
			  }

			  internal override bool Checkpoint
			  {
				  get
				  {
						return true;
				  }
			  }

			  public override bool HasCausalDependencyWith( Action other )
			  {
					return true;
			  }

			  internal override ActionType Type()
			  {
					return ActionType.Checkpoint;
			  }
		 }

		 private KEY Key( long seed )
		 {
			  return _layout.key( seed );
		 }

		 private VALUE Value( long seed )
		 {
			  return _layout.value( seed );
		 }

		 private void AssertEqualsKey( KEY expected, KEY actual )
		 {
			  assertEquals( string.Format( "expected equal, expected={0}, actual={1}", expected.ToString(), actual.ToString() ), 0, _layout.Compare(expected, actual) );
		 }

		 private void AssertEqualsValue( VALUE expected, VALUE actual )
		 {
			  assertEquals( string.Format( "expected equal, expected={0}, actual={1}", expected.ToString(), actual.ToString() ), 0, _layout.compareValue(expected, actual) );
		 }

		 private long KeySeed( KEY key )
		 {
			  return _layout.keySeed( key );
		 }

		 private long ValueSeed( VALUE value )
		 {
			  return _layout.valueSeed( value );
		 }
	}

}