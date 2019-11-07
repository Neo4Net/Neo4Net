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
namespace Neo4Net.Kernel.Impl.Index.Schema
{

	using PrimitiveLongResourceIterator = Neo4Net.Collections.PrimitiveLongResourceIterator;
	using IndexQuery = Neo4Net.Kernel.Api.Internal.IndexQuery;
	using IndexNotApplicableKernelException = Neo4Net.Kernel.Api.Internal.Exceptions.Schema.IndexNotApplicableKernelException;
	using IndexEntryConflictException = Neo4Net.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using Neo4Net.Kernel.Api.Index;
	using IndexUpdater = Neo4Net.Kernel.Api.Index.IndexUpdater;
	using IndexDescriptor = Neo4Net.Kernel.Api.StorageEngine.schema.IndexDescriptor;
	using IndexReader = Neo4Net.Kernel.Api.StorageEngine.schema.IndexReader;
	using ValueTuple = Neo4Net.Values.Storable.ValueTuple;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.Kernel.Api.Internal.IndexQuery.exact;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.api.index.UpdateMode.REMOVED;

	/// <summary>
	/// This deferring conflict checker solves e.g. a problem of applying updates to an index that is aware of,
	/// and also prevents, duplicates while applying. Consider this scenario:
	/// 
	/// <pre>
	///    GIVEN:
	///    Node A w/ property value P
	///    Node B w/ property value Q
	/// 
	///    WHEN Applying a transaction that:
	///    Sets A property value to Q
	///    Deletes B
	/// </pre>
	/// 
	/// Then an index that is conscious about conflicts when applying may see intermediary conflicts,
	/// depending on the order in which updates are applied. Remembering which value tuples have been altered and
	/// checking conflicts for those in <seealso cref="close()"/> works around that problem.
	/// 
	/// This updater wrapping should only be used in specific places to solve specific problems, not generally
	/// when applying updates to online indexes.
	/// </summary>
	public class DeferredConflictCheckingIndexUpdater : IndexUpdater
	{
		 private readonly IndexUpdater _actual;
		 private readonly System.Func<IndexReader> _readerSupplier;
		 private readonly IndexDescriptor _indexDescriptor;
		 private readonly ISet<ValueTuple> _touchedTuples = new HashSet<ValueTuple>();

		 public DeferredConflictCheckingIndexUpdater( IndexUpdater actual, System.Func<IndexReader> readerSupplier, IndexDescriptor indexDescriptor )
		 {
			  this._actual = actual;
			  this._readerSupplier = readerSupplier;
			  this._indexDescriptor = indexDescriptor;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void process(Neo4Net.kernel.api.index.IndexEntryUpdate<?> update) throws Neo4Net.kernel.api.exceptions.index.IndexEntryConflictException
		 public override void Process<T1>( IndexEntryUpdate<T1> update )
		 {
			  _actual.process( update );
			  if ( update.UpdateMode() != REMOVED )
			  {
					_touchedTuples.Add( ValueTuple.of( update.Values() ) );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws Neo4Net.kernel.api.exceptions.index.IndexEntryConflictException
		 public override void Close()
		 {
			  _actual.close();
			  try
			  {
					  using ( IndexReader reader = _readerSupplier.get() )
					  {
						foreach ( ValueTuple tuple in _touchedTuples )
						{
							 using ( PrimitiveLongResourceIterator results = reader.Query( QueryOf( tuple ) ) )
							 {
								  if ( results.hasNext() )
								  {
										long firstEntityId = results.next();
										if ( results.hasNext() )
										{
											 long secondEntityId = results.next();
											 throw new IndexEntryConflictException( firstEntityId, secondEntityId, tuple );
										}
								  }
							 }
						}
					  }
			  }
			  catch ( IndexNotApplicableKernelException e )
			  {
					throw new System.ArgumentException( "Unexpectedly the index reader couldn't handle this query", e );
			  }
		 }

		 private IndexQuery[] QueryOf( ValueTuple tuple )
		 {
			  IndexQuery[] predicates = new IndexQuery[tuple.Size()];
			  int[] propertyIds = _indexDescriptor.schema().PropertyIds;
			  for ( int i = 0; i < predicates.Length; i++ )
			  {
					predicates[i] = exact( propertyIds[i], tuple.ValueAt( i ) );
			  }
			  return predicates;
		 }
	}

}