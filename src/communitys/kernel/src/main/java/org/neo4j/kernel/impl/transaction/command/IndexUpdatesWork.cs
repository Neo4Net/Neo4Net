using System;
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
namespace Neo4Net.Kernel.impl.transaction.command
{

	using Neo4Net.Helpers.Collections;
	using SchemaDescriptor = Neo4Net.@internal.Kernel.Api.schema.SchemaDescriptor;
	using IndexEntryConflictException = Neo4Net.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using Neo4Net.Kernel.Api.Index;
	using Neo4Net.Kernel.Impl.Api.index;
	using IndexingUpdateService = Neo4Net.Kernel.Impl.Api.index.IndexingUpdateService;
	using UnderlyingStorageException = Neo4Net.Kernel.impl.store.UnderlyingStorageException;
	using IndexUpdates = Neo4Net.Kernel.impl.transaction.state.IndexUpdates;
	using Neo4Net.Utils.Concurrent;

	/// <summary>
	/// Combines <seealso cref="IndexUpdates"/> from multiple transactions into one bigger job.
	/// </summary>
	public class IndexUpdatesWork : Work<IndexingUpdateService, IndexUpdatesWork>
	{
		 private readonly IList<IndexUpdates> _updates = new List<IndexUpdates>();

		 public IndexUpdatesWork( IndexUpdates updates )
		 {
			  this._updates.Add( updates );
		 }

		 public override IndexUpdatesWork Combine( IndexUpdatesWork work )
		 {
			  ( ( IList<IndexUpdates> )_updates ).AddRange( work._updates );
			  return this;
		 }

		 public override void Apply( IndexingUpdateService material )
		 {
			  try
			  {
					material.Apply( CombinedUpdates() );
			  }
			  catch ( Exception e ) when ( e is IOException || e is IndexEntryConflictException )
			  {
					throw new UnderlyingStorageException( e );
			  }
		 }

		 private IndexUpdates CombinedUpdates()
		 {
			  return new IndexUpdatesAnonymousInnerClass( this );
		 }

		 private class IndexUpdatesAnonymousInnerClass : IndexUpdates
		 {
			 private readonly IndexUpdatesWork _outerInstance;

			 public IndexUpdatesAnonymousInnerClass( IndexUpdatesWork outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public IEnumerator<IndexEntryUpdate<SchemaDescriptor>> iterator()
			 {
				  return new NestingIteratorAnonymousInnerClass( this, _outerInstance.updates.GetEnumerator() );
			 }

			 private class NestingIteratorAnonymousInnerClass : NestingIterator<IndexEntryUpdate<SchemaDescriptor>, IndexUpdates>
			 {
				 private readonly IndexUpdatesAnonymousInnerClass _outerInstance;

				 public NestingIteratorAnonymousInnerClass( IndexUpdatesAnonymousInnerClass outerInstance, UnknownType iterator ) : base( iterator )
				 {
					 this.outerInstance = outerInstance;
				 }

				 protected internal IEnumerator<IndexEntryUpdate<SchemaDescriptor>> createNestedIterator( IndexUpdates item )
				 {
					  return item.GetEnumerator();
				 }
			 }

			 public void feed( EntityCommandGrouper<Command.NodeCommand>.Cursor nodeCommands, EntityCommandGrouper<Command.RelationshipCommand>.Cursor relationshipCommands )
			 {
				  throw new System.NotSupportedException();
			 }

			 public bool hasUpdates()
			 {
				  return true;
			 }
		 }
	}

}