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

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.store.kvstore.DataProvider.EMPTY_DATA_PROVIDER;

	internal abstract class ReadableState<Key> : System.IDisposable
	{
		 protected internal abstract KeyFormat<Key> KeyFormat();

		 protected internal abstract Headers Headers();

		 protected internal abstract long Version();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected abstract boolean lookup(Key key, ValueSink sink) throws java.io.IOException;
		 protected internal abstract bool Lookup( Key key, ValueSink sink );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected abstract DataProvider dataProvider() throws java.io.IOException;
		 protected internal abstract DataProvider DataProvider();

		 protected internal abstract int StoredEntryCount();

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: static <Key> ReadableState<Key> store(final KeyFormat<Key> keys, final KeyValueStoreFile store)
		 internal static ReadableState<Key> Store<Key>( KeyFormat<Key> keys, KeyValueStoreFile store )
		 {
			  return new ReadableStateAnonymousInnerClass( keys, store );
		 }

		 private class ReadableStateAnonymousInnerClass : ReadableState<Key>
		 {
			 private Neo4Net.Kernel.impl.store.kvstore.KeyFormat<Key> _keys;
			 private Neo4Net.Kernel.impl.store.kvstore.KeyValueStoreFile _store;

			 public ReadableStateAnonymousInnerClass( Neo4Net.Kernel.impl.store.kvstore.KeyFormat<Key> keys, Neo4Net.Kernel.impl.store.kvstore.KeyValueStoreFile store )
			 {
				 this._keys = keys;
				 this._store = store;
			 }

			 protected internal override KeyFormat<Key> keyFormat()
			 {
				  return _keys;
			 }

			 protected internal override Headers headers()
			 {
				  return _store.headers();
			 }

			 protected internal override long version()
			 {
				  return _keys.version( outerInstance.headers() ); // TODO: 'keys' is not the right guy to have this responsibility
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected boolean lookup(Key key, ValueSink sink) throws java.io.IOException
			 protected internal override bool lookup( Key key, ValueSink sink )
			 {
				  return _store.scan( new KeyFormat_Searcher<>( _keys, key ), sink );
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected DataProvider dataProvider() throws java.io.IOException
			 protected internal override DataProvider dataProvider()
			 {
				  return _store.dataProvider();
			 }

			 protected internal override int storedEntryCount()
			 {
				  return _store.entryCount();
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
			 public override void close()
			 {
				  _store.Dispose();
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: static <Key> ReadableState<Key> empty(final KeyFormat<Key> keys, final long version)
		 internal static ReadableState<Key> Empty<Key>( KeyFormat<Key> keys, long version )
		 {
			  return new ReadableStateAnonymousInnerClass2( keys, version );
		 }

		 private class ReadableStateAnonymousInnerClass2 : ReadableState<Key>
		 {
			 private Neo4Net.Kernel.impl.store.kvstore.KeyFormat<Key> _keys;
			 private long _version;

			 public ReadableStateAnonymousInnerClass2( Neo4Net.Kernel.impl.store.kvstore.KeyFormat<Key> keys, long version )
			 {
				 this._keys = keys;
				 this._version = version;
			 }

			 protected internal override KeyFormat<Key> keyFormat()
			 {
				  return _keys;
			 }

			 protected internal override Headers headers()
			 {
				  return null;
			 }

			 protected internal override long version()
			 {
				  return _version;
			 }

			 protected internal override bool lookup( Key key, ValueSink sink )
			 {
				  return false;
			 }

			 protected internal override DataProvider dataProvider()
			 {
				  return EMPTY_DATA_PROVIDER;
			 }

			 protected internal override int storedEntryCount()
			 {
				  return 0;
			 }

			 public override void close()
			 {
			 }
		 }
	}

}