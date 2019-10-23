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
namespace Neo4Net.Kernel.Impl.Api.index
{

	using Neo4Net.GraphDb;
	using InternalIndexState = Neo4Net.Kernel.Api.Internal.InternalIndexState;
	using IOLimiter = Neo4Net.Io.pagecache.IOLimiter;
	using IndexUpdater = Neo4Net.Kernel.Api.Index.IndexUpdater;
	using SwallowingIndexUpdater = Neo4Net.Kernel.Impl.Api.index.updater.SwallowingIndexUpdater;
	using CapableIndexDescriptor = Neo4Net.Kernel.Api.StorageEngine.schema.CapableIndexDescriptor;
	using IndexReader = Neo4Net.Kernel.Api.StorageEngine.schema.IndexReader;
	using PopulationProgress = Neo4Net.Kernel.Api.StorageEngine.schema.PopulationProgress;
	using Value = Neo4Net.Values.Storable.Value;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterators.emptyResourceIterator;

	public class IndexProxyAdapter : IndexProxy
	{
		 public override void Start()
		 {
		 }

		 public override IndexUpdater NewUpdater( IndexUpdateMode mode )
		 {
			  return SwallowingIndexUpdater.INSTANCE;
		 }

		 public override void Drop()
		 {
		 }

		 public virtual InternalIndexState State
		 {
			 get
			 {
				  throw new System.NotSupportedException();
			 }
		 }

		 public override void Force( IOLimiter ioLimiter )
		 {
		 }

		 public override void Refresh()
		 {
		 }

		 public override void Close()
		 {
		 }

		 public virtual CapableIndexDescriptor Descriptor
		 {
			 get
			 {
				  return null;
			 }
		 }

		 public override IndexReader NewReader()
		 {
			  return IndexReader.EMPTY;
		 }

		 public override bool AwaitStoreScanCompleted( long time, TimeUnit unit )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override void Activate()
		 {
		 }

		 public override void Validate()
		 {
		 }

		 public override void ValidateBeforeCommit( Value[] tuple )
		 {
		 }

		 public override ResourceIterator<File> SnapshotFiles()
		 {
			  return emptyResourceIterator();
		 }

		 public override IDictionary<string, Value> IndexConfig()
		 {
			  return Collections.emptyMap();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public IndexPopulationFailure getPopulationFailure() throws IllegalStateException
		 public virtual IndexPopulationFailure PopulationFailure
		 {
			 get
			 {
				  throw new System.InvalidOperationException( "This index isn't failed" );
			 }
		 }

		 public virtual PopulationProgress IndexPopulationProgress
		 {
			 get
			 {
				  return Neo4Net.Kernel.Api.StorageEngine.schema.PopulationProgress_Fields.None;
			 }
		 }
	}

}