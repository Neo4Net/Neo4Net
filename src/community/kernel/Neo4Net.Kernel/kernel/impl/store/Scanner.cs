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
namespace Neo4Net.Kernel.impl.store
{
	using LongIterator = org.eclipse.collections.api.iterator.LongIterator;

	using Neo4Net.Graphdb;
	using Neo4Net.Graphdb;
	using Neo4Net.Helpers.Collections;
	using PageCursor = Neo4Net.Io.pagecache.PageCursor;
	using AbstractBaseRecord = Neo4Net.Kernel.Impl.Store.Records.AbstractBaseRecord;
	using RecordLoad = Neo4Net.Kernel.Impl.Store.Records.RecordLoad;

	/// <summary>
	/// Scans all used records in a store, returned <seealso cref="ResourceIterable"/> must be properly used such that
	/// its <seealso cref="ResourceIterable.iterator() resource iterators"/> are <seealso cref="ResourceIterator.close() closed"/>
	/// after use.
	/// </summary>
	public class Scanner
	{
		 private Scanner()
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SafeVarargs public static <R extends org.neo4j.kernel.impl.store.record.AbstractBaseRecord> org.neo4j.graphdb.ResourceIterable<R> scan(final RecordStore<R> store, final System.Predicate<? super R>... filters)
//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 public static ResourceIterable<R> Scan<R>( RecordStore<R> store, params System.Predicate<object>[] filters ) where R : Neo4Net.Kernel.Impl.Store.Records.AbstractBaseRecord
		 {
			  return Scan( store, true, filters );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SafeVarargs public static <R extends org.neo4j.kernel.impl.store.record.AbstractBaseRecord> org.neo4j.graphdb.ResourceIterable<R> scan(final RecordStore<R> store, final boolean forward, final System.Predicate<? super R>... filters)
//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 public static ResourceIterable<R> Scan<R>( RecordStore<R> store, bool forward, params System.Predicate<object>[] filters ) where R : Neo4Net.Kernel.Impl.Store.Records.AbstractBaseRecord
		 {
			  return () => new Scan<R>(store, forward, filters);
		 }

		 private class Scan<R> : PrefetchingResourceIterator<R> where R : Neo4Net.Kernel.Impl.Store.Records.AbstractBaseRecord
		 {
			  internal readonly LongIterator Ids;
			  internal readonly RecordStore<R> Store;
			  internal readonly PageCursor Cursor;
			  internal readonly R Record;
//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: private final System.Predicate<? super R>[] filters;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
			  internal readonly System.Predicate<object>[] Filters;

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: Scan(RecordStore<R> store, boolean forward, final System.Predicate<? super R>... filters)
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
			  internal Scan( RecordStore<R> store, bool forward, params System.Predicate<object>[] filters )
			  {
					this.Filters = filters;
					this.Ids = new StoreIdIterator( store, forward );
					this.Store = store;
					this.Cursor = store.OpenPageCursorForReading( 0 );
					this.Record = store.NewRecord();
			  }

			  protected internal override R FetchNextOrNull()
			  {
					while ( Ids.hasNext() )
					{
						 Store.getRecordByCursor( Ids.next(), Record, RecordLoad.CHECK, Cursor );
						 if ( Record.inUse() )
						 {
							  if ( PassesFilters( Record ) )
							  {
									return Record;
							  }
						 }
					}
					return default( R );
			  }

			  internal virtual bool PassesFilters( R record )
			  {
//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: for (System.Predicate<? super R> filter : filters)
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
					foreach ( System.Predicate<object> filter in Filters )
					{
						 if ( !filter( record ) )
						 {
							  return false;
						 }
					}
					return true;
			  }

			  public override void Close()
			  {
					Cursor.close();
			  }
		 }
	}

}