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
namespace Org.Neo4j.Kernel.Impl.Index.Schema
{

	using PrimitiveLongResourceIterator = Org.Neo4j.Collection.PrimitiveLongResourceIterator;
	using IndexQuery = Org.Neo4j.@internal.Kernel.Api.IndexQuery;
	using IndexNotApplicableKernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.schema.IndexNotApplicableKernelException;
	using IndexEntryConflictException = Org.Neo4j.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using Org.Neo4j.Kernel.Api.Index;
	using IndexUpdater = Org.Neo4j.Kernel.Api.Index.IndexUpdater;
	using IndexDescriptor = Org.Neo4j.Storageengine.Api.schema.IndexDescriptor;
	using IndexReader = Org.Neo4j.Storageengine.Api.schema.IndexReader;
	using ValueTuple = Org.Neo4j.Values.Storable.ValueTuple;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.IndexQuery.exact;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.api.index.UpdateMode.REMOVED;

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
//ORIGINAL LINE: public void process(org.neo4j.kernel.api.index.IndexEntryUpdate<?> update) throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
		 public override void Process<T1>( IndexEntryUpdate<T1> update )
		 {
			  _actual.process( update );
			  if ( update.UpdateMode() != REMOVED )
			  {
					_touchedTuples.Add( ValueTuple.of( update.Values() ) );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
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