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
	using Before = org.junit.Before;
	using Test = org.junit.Test;


	using EmptyVersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using VersionContext = Neo4Net.Io.pagecache.tracing.cursor.context.VersionContext;
	using VersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.VersionContextSupplier;
	using TransactionVersionContextSupplier = Neo4Net.Kernel.impl.context.TransactionVersionContextSupplier;
	using Runnables = Neo4Net.Utils.Concurrent.Runnables;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class ConcurrentMapStateTest
	{

		 private readonly ReadableState<string> _store = mock( typeof( ReadableState ) );
		 private readonly File _file = mock( typeof( File ) );
		 private readonly Lock @lock = mock( typeof( Lock ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SetUp()
		 {
			  KeyFormat keyFormat = mock( typeof( KeyFormat ) );
			  when( keyFormat.valueSize() ).thenReturn(Long.BYTES);
			  when( _store.keyFormat() ).thenReturn(keyFormat);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateAnUpdaterForTheNextUnseenVersionUpdate()
		 public virtual void ShouldCreateAnUpdaterForTheNextUnseenVersionUpdate()
		 {
			  // given
			  long initialVersion = 42;
			  when( _store.version() ).thenReturn(initialVersion);
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: ConcurrentMapState<?> state = createMapState();
			  ConcurrentMapState<object> state = CreateMapState();

			  // when
			  long updateVersion = 43;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: EntryUpdater<?> updater = state.updater(updateVersion, lock);
			  EntryUpdater<object> updater = state.Updater( updateVersion, @lock );

			  // then
			  // it does not blow up
			  assertNotNull( updater );
			  assertEquals( updateVersion, state.Version() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateAnUpdaterForAnUnseenVersionUpdateWithAGap()
		 public virtual void ShouldCreateAnUpdaterForAnUnseenVersionUpdateWithAGap()
		 {
			  // given
			  long initialVersion = 42;
			  when( _store.version() ).thenReturn(initialVersion);
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: ConcurrentMapState<?> state = createMapState();
			  ConcurrentMapState<object> state = CreateMapState();

			  // when
			  long updateVersion = 45;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final EntryUpdater<?> updater = state.updater(updateVersion, lock);
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
			  EntryUpdater<object> updater = state.Updater( updateVersion, @lock );
			  updater.Close();

			  // then
			  // it does not blow up
			  assertNotNull( updater );
			  assertEquals( updateVersion, state.Version() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateAnUpdaterForMultipleVersionUpdatesInOrder()
		 public virtual void ShouldCreateAnUpdaterForMultipleVersionUpdatesInOrder()
		 {
			  // given
			  long initialVersion = 42;
			  when( _store.version() ).thenReturn(initialVersion);
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: ConcurrentMapState<?> state = createMapState();
			  ConcurrentMapState<object> state = CreateMapState();

			  // when
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: EntryUpdater<?> updater;
			  EntryUpdater<object> updater;

			  long updateVersion = 43;
			  updater = state.Updater( updateVersion, @lock );
			  updater.Close();

			  updateVersion = 44;
			  updater = state.Updater( updateVersion, @lock );
			  updater.Close();

			  updateVersion = 45;
			  updater = state.Updater( updateVersion, @lock );
			  updater.Close();

			  // then
			  // it does not blow up
			  assertNotNull( updater );
			  assertEquals( updateVersion, state.Version() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateAnUpdaterForMultipleVersionUpdatesNotInOrder()
		 public virtual void ShouldCreateAnUpdaterForMultipleVersionUpdatesNotInOrder()
		 {
			  // given
			  long initialVersion = 42;
			  when( _store.version() ).thenReturn(initialVersion);
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: ConcurrentMapState<?> state = createMapState();
			  ConcurrentMapState<object> state = CreateMapState();

			  // when
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: EntryUpdater<?> updater;
			  EntryUpdater<object> updater;

			  long updateVersion = 45;
			  updater = state.Updater( updateVersion, @lock );
			  updater.Close();

			  updateVersion = 43;
			  updater = state.Updater( updateVersion, @lock );
			  updater.Close();

			  updateVersion = 44;
			  updater = state.Updater( updateVersion, @lock );
			  updater.Close();

			  // then
			  // it does not blow up
			  assertNotNull( updater );
			  assertEquals( 45, state.Version() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUseEmptyUpdaterOnVersionLowerOrEqualToTheInitialVersion()
		 public virtual void ShouldUseEmptyUpdaterOnVersionLowerOrEqualToTheInitialVersion()
		 {
			  // given
			  long initialVersion = 42;
			  when( _store.version() ).thenReturn(initialVersion);
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: ConcurrentMapState<?> state = createMapState();
			  ConcurrentMapState<object> state = CreateMapState();

			  // when
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: EntryUpdater<?> updater = state.updater(initialVersion, lock);
			  EntryUpdater<object> updater = state.Updater( initialVersion, @lock );

			  // expected
			  assertEquals( "Empty updater should be used for version less or equal to initial", EntryUpdater.NoUpdates(), updater );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void markDirtyVersionLookupOnKeyUpdate() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MarkDirtyVersionLookupOnKeyUpdate()
		 {
			  long updaterVersionTxId = 25;
			  long lastClosedTxId = 20;
			  TransactionVersionContextSupplier versionContextSupplier = new TransactionVersionContextSupplier();
			  versionContextSupplier.Init( () => lastClosedTxId );
			  ConcurrentMapState<string> mapState = CreateMapState( versionContextSupplier );
			  VersionContext versionContext = versionContextSupplier.VersionContext;
			  using ( EntryUpdater<string> updater = mapState.Updater( updaterVersionTxId, @lock ) )
			  {
					updater.Apply( "a", new SimpleValueUpdate( 1 ) );
					updater.Apply( "b", new SimpleValueUpdate( 2 ) );
			  }

			  assertEquals( updaterVersionTxId, mapState.Version() );
			  versionContext.InitRead();
			  mapState.Lookup( "a", new EmptyValueSink() );
			  assertTrue( versionContext.Dirty );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void markDirtyVersionLookupOnKeyReset() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MarkDirtyVersionLookupOnKeyReset()
		 {
			  long updaterVersionTxId = 25;
			  long lastClosedTxId = 20;
			  when( _store.version() ).thenReturn(updaterVersionTxId);
			  TransactionVersionContextSupplier versionContextSupplier = new TransactionVersionContextSupplier();
			  versionContextSupplier.Init( () => lastClosedTxId );
			  VersionContext versionContext = versionContextSupplier.VersionContext;

			  ConcurrentMapState<string> mapState = CreateMapState( versionContextSupplier );

			  versionContext.InitRead();
			  mapState.ResettingUpdater( @lock, Runnables.EMPTY_RUNNABLE ).apply( "a", new SimpleValueUpdate( 1 ) );
			  mapState.Lookup( "a", new EmptyValueSink() );
			  assertTrue( versionContext.Dirty );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void doNotMarkVersionAsDirtyOnAnotherKeyUpdate() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void DoNotMarkVersionAsDirtyOnAnotherKeyUpdate()
		 {
			  long updaterVersionTxId = 25;
			  long lastClosedTxId = 20;
			  TransactionVersionContextSupplier versionContextSupplier = new TransactionVersionContextSupplier();
			  versionContextSupplier.Init( () => lastClosedTxId );
			  ConcurrentMapState<string> mapState = CreateMapState( versionContextSupplier );
			  VersionContext versionContext = versionContextSupplier.VersionContext;
			  using ( EntryUpdater<string> updater = mapState.Updater( updaterVersionTxId, @lock ) )
			  {
					updater.Apply( "b", new SimpleValueUpdate( 2 ) );
			  }

			  assertEquals( updaterVersionTxId, mapState.Version() );
			  versionContext.InitRead();
			  mapState.Lookup( "a", new EmptyValueSink() );
			  assertFalse( versionContext.Dirty );
		 }

		 private ConcurrentMapState<string> CreateMapState()
		 {
			  return CreateMapState( EmptyVersionContextSupplier.EMPTY );
		 }

		 private ConcurrentMapState<string> CreateMapState( VersionContextSupplier versionContextSupplier )
		 {
			  return new ConcurrentMapState<string>( _store, _file, versionContextSupplier );
		 }

		 private class SimpleValueUpdate : ValueUpdate
		 {
			  internal readonly long Value;

			  internal SimpleValueUpdate( long value )
			  {
					this.Value = value;
			  }

			  public override void Update( WritableBuffer target )
			  {
					target.PutLong( 0, Value );
			  }
		 }

		 private class EmptyValueSink : ValueSink
		 {
			  protected internal override void Value( ReadableBuffer value )
			  {

			  }
		 }
	}

}