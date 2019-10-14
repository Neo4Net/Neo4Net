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
namespace Neo4Net.Kernel.Impl.Api.index.sampling
{
	using IndexNotFoundKernelException = Neo4Net.Internal.Kernel.Api.exceptions.schema.IndexNotFoundKernelException;
	using DurationLogger = Neo4Net.Kernel.impl.util.DurationLogger;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using IndexReader = Neo4Net.Storageengine.Api.schema.IndexReader;
	using IndexSample = Neo4Net.Storageengine.Api.schema.IndexSample;
	using IndexSampler = Neo4Net.Storageengine.Api.schema.IndexSampler;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.Internal.kernel.api.InternalIndexState.ONLINE;

	internal class OnlineIndexSamplingJob : IndexSamplingJob
	{
		 private readonly long _indexId;
		 private readonly IndexProxy _indexProxy;
		 private readonly IndexStoreView _storeView;
		 private readonly Log _log;
		 private readonly string _indexUserDescription;

		 internal OnlineIndexSamplingJob( long indexId, IndexProxy indexProxy, IndexStoreView storeView, string indexUserDescription, LogProvider logProvider )
		 {
			  this._indexId = indexId;
			  this._indexProxy = indexProxy;
			  this._storeView = storeView;
			  this._log = logProvider.getLog( this.GetType() );
			  this._indexUserDescription = indexUserDescription;
		 }

		 public override long IndexId()
		 {
			  return _indexId;
		 }

		 public override void Run()
		 {
			  using ( DurationLogger durationLogger = new DurationLogger( _log, "Sampling index " + _indexUserDescription ) )
			  {
					try
					{
						 using ( IndexReader reader = _indexProxy.newReader(), IndexSampler sampler = reader.CreateSampler() )
						 {
							  IndexSample sample = sampler.SampleIndex();

							  // check again if the index is online before saving the counts in the store
							  if ( _indexProxy.State == ONLINE )
							  {
									_storeView.replaceIndexCounts( _indexId, sample.UniqueValues(), sample.SampleSize(), sample.IndexSize() );
									durationLogger.MarkAsFinished();
									_log.debug( format( "Sampled index %s with %d unique values in sample of avg size %d taken from " + "index containing %d entries", _indexUserDescription, sample.UniqueValues(), sample.SampleSize(), sample.IndexSize() ) );
							  }
							  else
							  {
									durationLogger.MarkAsAborted( "Index no longer ONLINE" );
							  }
						 }
					}
					catch ( IndexNotFoundKernelException )
					{
						 durationLogger.MarkAsAborted( "Attempted to sample missing/already deleted index " + _indexUserDescription );
					}
			  }
		 }

	}

}