using System;
using System.Text;
using System.Threading;

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
namespace Neo4Net.Kernel.impl.store.kvstore
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;
	using RuleChain = org.junit.rules.RuleChain;
	using Timeout = org.junit.rules.Timeout;


	using Neo4Net.Functions;
	using Neo4Net.Functions;
	using Neo4Net.Functions;
	using Neo4Net.Helpers.Collections;
	using OpenMode = Neo4Net.Io.fs.OpenMode;
	using StoreChannel = Neo4Net.Io.fs.StoreChannel;
	using FileIsNotMappedException = Neo4Net.Io.pagecache.impl.FileIsNotMappedException;
	using EmptyVersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using TransactionIdStore = Neo4Net.Kernel.impl.transaction.log.TransactionIdStore;
	using Lifespan = Neo4Net.Kernel.Lifecycle.Lifespan;
	using Resources = Neo4Net.Test.rule.Resources;
	using ThreadingRule = Neo4Net.Test.rule.concurrent.ThreadingRule;
	using Clocks = Neo4Net.Time.Clocks;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.kvstore.DataProvider.EMPTY_DATA_PROVIDER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.rule.Resources.InitialLifecycle.STARTED;

	public class AbstractKeyValueStoreTest
	{
		private bool InstanceFieldsInitialized = false;

		public AbstractKeyValueStoreTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			RuleChain = RuleChain.outerRule( _expectedException ).around( _resourceManager ).around( _threading ).around( _timeout );
		}

		 private readonly ExpectedException _expectedException = ExpectedException.none();
		 private readonly Resources _resourceManager = new Resources();
		 private readonly ThreadingRule _threading = new ThreadingRule();
		 private readonly Timeout _timeout = Timeout.builder().withTimeout(20, TimeUnit.SECONDS).withLookingForStuckThread(true).build();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(expectedException).around(resourceManager).around(threading).around(timeout);
		 public RuleChain RuleChain;

		 private static readonly HeaderField<long> TX_ID = new HeaderFieldAnonymousInnerClass();

		 private class HeaderFieldAnonymousInnerClass : HeaderField<long>
		 {
			 public long? read( ReadableBuffer header )
			 {
				  return header.GetLong( header.Size() - 8 );
			 }

			 public void write( long? value, WritableBuffer header )
			 {
				  header.PutLong( header.Size() - 8, value.Value );
			 }

			 public override string ToString()
			 {
				  return "txId";
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Resources.Life(STARTED) @SuppressWarnings("unchecked") public void retryLookupOnConcurrentStoreStateChange() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void RetryLookupOnConcurrentStoreStateChange()
		 {
			  Store store = _resourceManager.managed( new Store( this ) );

			  ProgressiveState<string> workingState = StateWithLookup( () => true );
			  ProgressiveState<string> staleState = StateWithLookup(() =>
			  {
			  SetState( store, workingState );
			  throw new FileIsNotMappedException( new File( "/files/was/rotated/concurrently/during/lookup" ) );
			  });

			  SetState( store, staleState );

			  assertEquals( "New state contains stored value", "value", store.Lookup( "test", StringReader( "value" ) ) );

			  // Should have 2 invocations: first throws exception, second re-read value.
			  verify( staleState, times( 1 ) ).lookup( any(), any() );
			  verify( workingState, times( 1 ) ).lookup( any(), any() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Resources.Life(STARTED) public void accessClosedStateShouldThrow() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void AccessClosedStateShouldThrow()
		 {
			  Store store = _resourceManager.managed( new Store( this ) );
			  store.Put( "test", "value" );
			  store.PrepareRotation( 0 ).rotate();
			  ProgressiveState<string> lookupState = store.State;
			  store.PrepareRotation( 0 ).rotate();

			  _expectedException.expect( typeof( FileIsNotMappedException ) );
			  _expectedException.expectMessage( "File has been unmapped" );

			  lookupState.lookup( "test", new ValueSinkAnonymousInnerClass( this ) );
		 }

		 private class ValueSinkAnonymousInnerClass : ValueSink
		 {
			 private readonly AbstractKeyValueStoreTest _outerInstance;

			 public ValueSinkAnonymousInnerClass( AbstractKeyValueStoreTest outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 protected internal override void value( ReadableBuffer value )
			 {
				  // empty
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldStartAndStopStore()
		 public virtual void ShouldStartAndStopStore()
		 {
			  // given
			  _resourceManager.managed( new Store( this ) );

			  // when
			  _resourceManager.lifeStarts();
			  _resourceManager.lifeShutsDown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Resources.Life(STARTED) public void shouldRotateStore() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRotateStore()
		 {
			  // given
			  Store store = _resourceManager.managed( new Store( this ) );

			  // when
			  store.PrepareRotation( 0 ).rotate();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Resources.Life(STARTED) public void shouldStoreEntries() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldStoreEntries()
		 {
			  // given
			  Store store = _resourceManager.managed( new Store( this ) );

			  // when
			  store.Put( "message", "hello world" );
			  store.Put( "age", "too old" );

			  // then
			  assertEquals( "hello world", store.Get( "message" ) );
			  assertEquals( "too old", store.Get( "age" ) );

			  // when
			  store.PrepareRotation( 0 ).rotate();

			  // then
			  assertEquals( "hello world", store.Get( "message" ) );
			  assertEquals( "too old", store.Get( "age" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPickFileWithGreatestTransactionId() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPickFileWithGreatestTransactionId()
		 {
			  using ( Lifespan life = new Lifespan() )
			  {
					Store store = life.Add( CreateTestStore() );

					// when
					for ( long txId = 2; txId <= 10; txId++ )
					{
						 store.Updater( txId ).get().close();
						 store.PrepareRotation( txId ).rotate();
					}
			  }

			  // then
			  using ( Lifespan life = new Lifespan() )
			  {
					Store store = life.Add( CreateTestStore() );
					assertEquals( 10L, store.Headers().get(TX_ID).longValue() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotPickCorruptStoreFile() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotPickCorruptStoreFile()
		 {
			  // given
			  Store store = CreateTestStore();
			  RotationStrategy rotation = store.RotationStrategy;

			  // when
			  File[] files = new File[10];
			  {
					Pair<File, KeyValueStoreFile> file = rotation.Create( EMPTY_DATA_PROVIDER, 1 );
					files[0] = file.First();
					for ( int txId = 2, i = 1; i < Files.Length; txId <<= 1, i++ )
					{
						 KeyValueStoreFile old = file.Other();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int data = txId;
						 int data = txId;
						 file = rotation.Next(file.First(), Headers.HeadersBuilder().put(TX_ID, (long) txId).headers(), data((Entry)(key, value) =>
						 {
									 key.putByte( 0, ( sbyte ) 'f' );
									 key.putByte( 1, ( sbyte ) 'o' );
									 key.putByte( 2, ( sbyte ) 'o' );
									 value.putInt( 0, data );
						 }));
						 old.Dispose();
						 files[i] = file.First();
					}
					file.Other().Dispose();
			  }
			  // Corrupt the last files
			  using ( StoreChannel channel = _resourceManager.fileSystem().open(files[9], OpenMode.READ_WRITE) )
			  { // ruin the header
					channel.Position( 16 );
					ByteBuffer value = ByteBuffer.allocate( 16 );
					value.put( ( sbyte ) 0 );
					value.flip();
					channel.WriteAll( value );
			  }
			  using ( StoreChannel channel = _resourceManager.fileSystem().open(files[8], OpenMode.READ_WRITE) )
			  { // ruin the header
					channel.Position( 32 );
					ByteBuffer value = ByteBuffer.allocate( 16 );
					value.put( ( sbyte ) 17 );
					value.flip();
					channel.WriteAll( value );
			  }
			  using ( StoreChannel channel = _resourceManager.fileSystem().open(files[7], OpenMode.READ_WRITE) )
			  { // ruin the header
					channel.Position( 32 + 32 + 32 + 16 );
					ByteBuffer value = ByteBuffer.allocate( 16 );
					value.putLong( 0 );
					value.putLong( 0 );
					value.flip();
					channel.WriteAll( value );
			  }

			  // then
			  using ( Lifespan life = new Lifespan() )
			  {
					life.Add( store );

					assertEquals( 64L, store.Headers().get(TX_ID).longValue() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPickTheUncorruptedStoreWhenTruncatingAfterTheHeader() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPickTheUncorruptedStoreWhenTruncatingAfterTheHeader()
		 {
			  /*
			   * The problem was that if we were successful in writing the header but failing immediately after, we would
			   *  read 0 as counter for entry data and pick the corrupted store thinking that it was simply empty.
			   */

			  Store store = CreateTestStore();

			  Pair<File, KeyValueStoreFile> file = store.RotationStrategy.create( EMPTY_DATA_PROVIDER, 1 );
			  Pair<File, KeyValueStoreFile> next = store.RotationStrategy.next(file.First(), Headers.HeadersBuilder().put(TX_ID, (long) 42).headers(), Data((Entry)(key, value) =>
			  {
									 key.putByte( 0, ( sbyte ) 'f' );
									 key.putByte( 1, ( sbyte ) 'o' );
									 key.putByte( 2, ( sbyte ) 'o' );
									 value.putInt( 0, 42 );
			  }));
			  file.Other().Dispose();
			  File correct = next.First();

			  Pair<File, KeyValueStoreFile> nextNext = store.RotationStrategy.next(correct, Headers.HeadersBuilder().put(TX_ID, (long) 43).headers(), Data((key, value) =>
			  {
						  key.putByte( 0, ( sbyte ) 'f' );
						  key.putByte( 1, ( sbyte ) 'o' );
						  key.putByte( 2, ( sbyte ) 'o' );
						  value.putInt( 0, 42 );
			  }, ( key, value ) =>
			  {
						  key.putByte( 0, ( sbyte ) 'b' );
						  key.putByte( 1, ( sbyte ) 'a' );
						  key.putByte( 2, ( sbyte ) 'r' );
						  value.putInt( 0, 4242 );
					 }));
			  next.Other().Dispose();
			  File corrupted = nextNext.First();
			  nextNext.Other().Dispose();

			  using ( StoreChannel channel = _resourceManager.fileSystem().open(corrupted, OpenMode.READ_WRITE) )
			  {
					channel.Truncate( 16 * 4 );
			  }

			  // then
			  using ( Lifespan life = new Lifespan() )
			  {
					life.Add( store );

					assertNotNull( store.Get( "foo" ) );
					assertEquals( 42L, store.Headers().get(TX_ID).longValue() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Resources.Life(STARTED) public void shouldRotateWithCorrectVersion() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRotateWithCorrectVersion()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Store store = resourceManager.managed(createTestStore());
			  Store store = _resourceManager.managed( CreateTestStore() );
			  UpdateStore( store, 1 );

			  PreparedRotation rotation = store.PrepareRotation( 2 );
			  UpdateStore( store, 2 );
			  rotation.Rotate();

			  // then
			  assertEquals( 2, store.Headers().get(TX_ID).longValue() );
			  store.PrepareRotation( 2 ).rotate();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Resources.Life(STARTED) public void postStateUpdatesCountedOnlyForTransactionsGreaterThanRotationVersion() throws java.io.IOException, InterruptedException, java.util.concurrent.ExecutionException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void PostStateUpdatesCountedOnlyForTransactionsGreaterThanRotationVersion()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Store store = resourceManager.managed(createTestStore());
			  Store store = _resourceManager.managed( CreateTestStore() );

			  PreparedRotation rotation = store.PrepareRotation( 2 );
			  UpdateStore( store, 4 );
			  UpdateStore( store, 3 );
			  UpdateStore( store, 1 );
			  UpdateStore( store, 2 );

			  assertEquals( 2, rotation.Rotate() );

			  Future<long> rotationFuture = _threading.executeAndAwait( store.Rotation, 5L, thread => Thread.State.TIMED_WAITING == thread.State, 100, SECONDS );

			  Thread.Sleep( TimeUnit.SECONDS.toMillis( 1 ) );

			  assertFalse( rotationFuture.Done );
			  UpdateStore( store, 5 );

			  assertEquals( 5, rotationFuture.get().longValue() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Resources.Life(STARTED) public void shouldBlockRotationUntilRequestedTransactionsAreApplied() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBlockRotationUntilRequestedTransactionsAreApplied()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Store store = resourceManager.managed(createTestStore());
			  Store store = _resourceManager.managed( CreateTestStore() );

			  // when
			  UpdateStore( store, 1 );
			  Future<long> rotation = _threading.executeAndAwait(store.Rotation, 3L, thread =>
			  {
				switch ( thread.State )
				{
				case BLOCKED:
				case WAITING:
				case TIMED_WAITING:
				case TERMINATED:
					 return true;
				default:
					 return false;
				}
			  }, 100, SECONDS);
			  // rotation should wait...
			  assertFalse( rotation.Done );
			  SECONDS.sleep( 1 );
			  assertFalse( rotation.Done );
			  // apply update
			  UpdateStore( store, 3 );
			  // rotation should still wait...
			  assertFalse( rotation.Done );
			  SECONDS.sleep( 1 );
			  assertFalse( rotation.Done );
			  // apply update
			  UpdateStore( store, 4 );
			  // rotation should still wait...
			  assertFalse( rotation.Done );
			  SECONDS.sleep( 1 );
			  assertFalse( rotation.Done );
			  // apply update
			  UpdateStore( store, 2 );

			  // then
			  assertEquals( 3, rotation.get().longValue() );
			  assertEquals( 3, store.Headers().get(TX_ID).longValue() );
			  store.Rotation.apply( 4L );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Resources.Life(STARTED) public void shouldFailRotationAfterTimeout() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailRotationAfterTimeout()
		 {
			  // GIVEN
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Store store = resourceManager.managed(createTestStore(0));
			  Store store = _resourceManager.managed( CreateTestStore( 0 ) );

			  // THEN
			  _expectedException.expect( typeof( RotationTimeoutException ) );

			  // WHEN
			  store.PrepareRotation( 10L ).rotate();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Resources.Life(STARTED) public void shouldLeaveStoreInGoodStateAfterRotationFailure() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLeaveStoreInGoodStateAfterRotationFailure()
		 {
			  // GIVEN
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Store store = resourceManager.managed(createTestStore(0));
			  Store store = _resourceManager.managed( CreateTestStore( 0 ) );
			  long initialVersion = store.Version( store.Headers() );
			  // a key/value which is rotated into a persistent version
			  string permanentKey = "permakey";
			  string permanentValue = "here";
			  using ( EntryUpdater<string> updater = store.Updater( initialVersion + 1 ).get() )
			  {
					updater.Apply( permanentKey, Value( permanentValue ) );
			  }
			  store.PrepareRotation( initialVersion + 1 ).rotate();

			  // another key/value which is applied to the new version
			  string key = "mykey";
			  string value = "first";
			  using ( EntryUpdater<string> updater = store.Updater( initialVersion + 2 ).get() )
			  {
					updater.Apply( key, value( "first" ) );
			  }

			  // WHEN rotating a version which doesn't exist
			  try
			  {
					store.PrepareRotation( initialVersion + 3 ).rotate();
					fail( "Should've failed rotation, since that version doesn't exist yet" );
			  }
			  catch ( RotationTimeoutException )
			  {
					// THEN afterwards it should still be possible to read from the counts store
					assertEquals( permanentValue, store.Get( permanentKey ) );
					assertEquals( value, store.Get( key ) );

					// and also continue to make updates
					using ( EntryUpdater<string> updater = store.Updater( initialVersion + 2 ).get() )
					{
						 updater.Apply( key, value( "second" ) );
					}

					// and eventually rotation again successfully
					store.PrepareRotation( initialVersion + 3 ).rotate();
			  }
		 }

		 private Store CreateTestStore()
		 {
			  return CreateTestStore( TimeUnit.SECONDS.toMillis( 100 ) );
		 }

		 private Store CreateTestStore( long rotationTimeout )
		 {
			  return new StoreAnonymousInnerClass( this, rotationTimeout, TX_ID );
		 }

		 private class StoreAnonymousInnerClass : Store
		 {
			 private readonly AbstractKeyValueStoreTest _outerInstance;

			 public StoreAnonymousInnerClass( AbstractKeyValueStoreTest outerInstance, long rotationTimeout, UnknownType txId ) : base( outerInstance, rotationTimeout, txId )
			 {
				 this.outerInstance = outerInstance;
			 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Override <Value> Value initialHeader(HeaderField<Value> field)
			 internal override Value initialHeader<Value>( HeaderField<Value> field )
			 {
				  if ( field == TX_ID )
				  {
						return ( Value )( object ) 1L;
				  }
				  else
				  {
						return base.initialHeader( field );
				  }
			 }

			 protected internal override void updateHeaders( Headers.Builder headers, long version )
			 {
				  headers.Put( TX_ID, version );
			 }

			 protected internal override int compareHeaders( Headers lhs, Headers rhs )
			 {
				  return Long.compare( lhs.Get( TX_ID ), rhs.Get( TX_ID ) );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void updateStore(final Store store, long transaction) throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 private void UpdateStore( Store store, long transaction )
		 {
			  ThrowingConsumer<long, IOException> update = u =>
			  {
				using ( EntryUpdater<string> updater = store.Updater( u ).get() )
				{
					 updater.Apply( "key " + u, Value( "value " + u ) );
				}
			  };
			  update.Accept( transaction );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private static DataProvider data(final Entry... data)
		 private static DataProvider Data( params Entry[] data )
		 {
			  return new DataProviderAnonymousInnerClass( data );
		 }

		 private class DataProviderAnonymousInnerClass : DataProvider
		 {
			 private Neo4Net.Kernel.impl.store.kvstore.AbstractKeyValueStoreTest.Entry[] _data;

			 public DataProviderAnonymousInnerClass( Neo4Net.Kernel.impl.store.kvstore.AbstractKeyValueStoreTest.Entry[] data )
			 {
				 this._data = data;
			 }

			 internal int i;

			 public bool visit( WritableBuffer key, WritableBuffer value )
			 {
				  if ( i < _data.Length )
				  {
						_data[i++].write( key, value );
						return true;
				  }
				  return false;
			 }

			 public void close()
			 {
			 }
		 }

		 internal interface Entry
		 {
			  void Write( WritableBuffer key, WritableBuffer value );
		 }

		 private AbstractKeyValueStore.Reader<string> StringReader( string value )
		 {
			  return new ReaderAnonymousInnerClass( this, value );
		 }

		 private class ReaderAnonymousInnerClass : AbstractKeyValueStore.Reader<string>
		 {
			 private readonly AbstractKeyValueStoreTest _outerInstance;

			 private string _value;

			 public ReaderAnonymousInnerClass( AbstractKeyValueStoreTest outerInstance, string value )
			 {
				 this.outerInstance = outerInstance;
				 this._value = value;
			 }

			 protected internal override string parseValue( ReadableBuffer buffer )
			 {
				  return _value;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private ProgressiveState<String> stateWithLookup(org.neo4j.function.ThrowingSupplier<bool, java.io.IOException> valueSupplier) throws java.io.IOException
		 private ProgressiveState<string> StateWithLookup( ThrowingSupplier<bool, IOException> valueSupplier )
		 {
			  ProgressiveState<string> state = mock( typeof( ProgressiveState ) );
			  when( state.lookup( any(), any() ) ).thenAnswer(invocation =>
			  {
			  bool wasFound = valueSupplier.Get();
			  invocation.getArgument<ValueLookup<string>>( 1 ).value( null );
			  return wasFound;
			  });
			  return state;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void setState(Store store, ProgressiveState<String> workingState) throws java.io.IOException
		 private void SetState( Store store, ProgressiveState<string> workingState )
		 {
			  store.State.Dispose();
			  store.State = workingState;
		 }

		 [Rotation(Rotation.Strategy.INCREMENTING)]
		 internal class Store : AbstractKeyValueStore<string>
		 {
			 internal bool InstanceFieldsInitialized = false;

			 internal virtual void InitializeInstanceFields()
			 {
				 Rotation = version => PrepareRotation( version ).rotate();
			 }

			 private readonly AbstractKeyValueStoreTest _outerInstance;

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final HeaderField<?>[] headerFields;
			  internal readonly HeaderField<object>[] HeaderFields;
			  internal IOFunction<long, long> Rotation;

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private Store(HeaderField<?>... headerFields)
			  internal Store( AbstractKeyValueStoreTest outerInstance, params HeaderField<object>[] headerFields ) : this( outerInstance, TimeUnit.MINUTES.toMillis( 10 ), headerFields )
			  {
				  this._outerInstance = outerInstance;

				  if ( !InstanceFieldsInitialized )
				  {
					  InitializeInstanceFields();
					  InstanceFieldsInitialized = true;
				  }
			  }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private Store(long rotationTimeout, HeaderField<?>... headerFields)
			  internal Store( AbstractKeyValueStoreTest outerInstance, long rotationTimeout, params HeaderField<object>[] headerFields ) : base( outerInstance.resourceManager.FileSystem(), outerInstance.resourceManager.PageCache(), outerInstance.resourceManager.TestDirectory().databaseLayout(), null, null, new RotationTimerFactory(Clocks.nanoClock(), rotationTimeout), EmptyVersionContextSupplier.EMPTY, 16, 16, headerFields )
			  {
				  this._outerInstance = outerInstance;

				  if ( !InstanceFieldsInitialized )
				  {
					  InitializeInstanceFields();
					  InstanceFieldsInitialized = true;
				  }
					this.HeaderFields = headerFields;
					EntryUpdaterInitializer = new DataInitializerAnonymousInnerClass( this );
			  }

			  private class DataInitializerAnonymousInnerClass : DataInitializer<EntryUpdater<string>>
			  {
				  private readonly Store _outerInstance;

				  public DataInitializerAnonymousInnerClass( Store outerInstance )
				  {
					  this.outerInstance = outerInstance;
				  }

				  public void initialize( EntryUpdater<string> stringEntryUpdater )
				  {
				  }

				  public long initialVersion()
				  {
						return 0;
				  }
			  }

			  protected internal override Headers InitialHeaders( long version )
			  {
					Headers.Builder builder = Headers.HeadersBuilder();
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (HeaderField<?> field : headerFields)
					foreach ( HeaderField<object> field in HeaderFields )
					{
						 PutHeader( builder, field );
					}
					return builder.Headers();
			  }

			  internal virtual void PutHeader<Value>( Headers.Builder builder, HeaderField<Value> field )
			  {
					builder.Put( field, InitialHeader( field ) );
			  }

			  internal virtual Value InitialHeader<Value>( HeaderField<Value> field )
			  {
					return default( Value );
			  }

			  protected internal override int CompareHeaders( Headers lhs, Headers rhs )
			  {
					return 0;
			  }

			  protected internal override void WriteKey( string key, WritableBuffer buffer )
			  {
					AwriteKey( key, buffer );
			  }

			  protected internal override string ReadKey( ReadableBuffer key )
			  {
					StringBuilder result = new StringBuilder( 16 );
					for ( int i = 0; i < key.Size(); i++ )
					{
						 char c = ( char )( 0xFF & key.GetByte( i ) );
						 if ( c == ( char )0 )
						 {
							  break;
						 }
						 result.Append( c );
					}
					return result.ToString();
			  }

			  protected internal override void UpdateHeaders( Headers.Builder headers, long version )
			  {
					headers.Put( TX_ID, version );
			  }

			  protected internal override long Version( Headers headers )
			  {
					long? transactionId = headers.Get( TX_ID );
					return Math.Max( Neo4Net.Kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_ID, transactionId != null ? transactionId.Value : Neo4Net.Kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_ID );
			  }

			  protected internal override void WriteFormatSpecifier( WritableBuffer formatSpecifier )
			  {
					formatSpecifier.PutByte( 0, unchecked( ( sbyte ) 0xFF ) );
					formatSpecifier.PutByte( formatSpecifier.Size() - 1, unchecked((sbyte) 0xFF) );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void put(String key, final String value) throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
			  public virtual void Put( string key, string value )
			  {
					using ( EntryUpdater<string> updater = updater() )
					{
						 updater.Apply( key, value( value ) );
					}
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public String get(String key) throws java.io.IOException
			  public virtual string Get( string key )
			  {
					return Lookup( key, new ReaderAnonymousInnerClass( this ) );
			  }

			  private class ReaderAnonymousInnerClass : Reader<string>
			  {
				  private readonly Store _outerInstance;

				  public ReaderAnonymousInnerClass( Store outerInstance )
				  {
					  this.outerInstance = outerInstance;
				  }

				  protected internal override string parseValue( ReadableBuffer value )
				  {
						return outerInstance.readKey( value );
				  }
			  }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private static ValueUpdate value(final String value)
		 private static ValueUpdate Value( string value )
		 {
			  return target => awriteKey( value, target );
		 }

		 private static void AwriteKey( string key, WritableBuffer buffer )
		 {
			  for ( int i = 0; i < key.Length; i++ )
			  {
					char c = key[i];
					if ( c == ( char )0 || c >= ( char )128 )
					{
						 throw new System.ArgumentException( "Only ASCII keys allowed." );
					}
					buffer.PutByte( i, ( sbyte ) c );
			  }
		 }
	}

}