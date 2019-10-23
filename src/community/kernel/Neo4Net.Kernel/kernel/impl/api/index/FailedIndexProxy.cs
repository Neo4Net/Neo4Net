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
	using IndexPopulationFailedKernelException = Neo4Net.Kernel.Api.Exceptions.index.IndexPopulationFailedKernelException;
	using IndexPopulator = Neo4Net.Kernel.Api.Index.IndexPopulator;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using CapableIndexDescriptor = Neo4Net.Kernel.Api.StorageEngine.schema.CapableIndexDescriptor;
	using Value = Neo4Net.Values.Storable.Value;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterators.emptyResourceIterator;

	public class FailedIndexProxy : AbstractSwallowingIndexProxy
	{
		 protected internal readonly IndexPopulator Populator;
		 private readonly string _indexUserDescription;
		 private readonly IndexCountsRemover _indexCountsRemover;
		 private readonly Log _log;

		 internal FailedIndexProxy( CapableIndexDescriptor capableIndexDescriptor, string indexUserDescription, IndexPopulator populator, IndexPopulationFailure populationFailure, IndexCountsRemover indexCountsRemover, LogProvider logProvider ) : base( capableIndexDescriptor, populationFailure )
		 {
			  this.Populator = populator;
			  this._indexUserDescription = indexUserDescription;
			  this._indexCountsRemover = indexCountsRemover;
			  this._log = logProvider.getLog( this.GetType() );
		 }

		 public override void Drop()
		 {
			  _indexCountsRemover.remove();
			  string message = "FailedIndexProxy#drop index on " + _indexUserDescription + " dropped due to:\n" + PopulationFailure.asString();
			  _log.info( message );
			  Populator.drop();
		 }

		 public override InternalIndexState State
		 {
			 get
			 {
				  return InternalIndexState.FAILED;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean awaitStoreScanCompleted(long time, java.util.concurrent.TimeUnit unit) throws org.Neo4Net.kernel.api.exceptions.index.IndexPopulationFailedKernelException
		 public override bool AwaitStoreScanCompleted( long time, TimeUnit unit )
		 {
			  throw FailureCause();
		 }

		 private IndexPopulationFailedKernelException FailureCause()
		 {
			  return PopulationFailure.asIndexPopulationFailure( Descriptor.schema(), _indexUserDescription );
		 }

		 public override void Activate()
		 {
			  throw new System.NotSupportedException( "Cannot activate a failed index." );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void validate() throws org.Neo4Net.kernel.api.exceptions.index.IndexPopulationFailedKernelException
		 public override void Validate()
		 {
			  throw FailureCause();
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
			  return Populator.indexConfig();
		 }
	}

}