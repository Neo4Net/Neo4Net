using System;

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
namespace Neo4Net.Kernel.Impl.Api.index.sampling
{

	using TokenNameLookup = Neo4Net.@internal.Kernel.Api.TokenNameLookup;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using Register_DoubleLongRegister = Neo4Net.Register.Register_DoubleLongRegister;
	using JobScheduler = Neo4Net.Scheduler.JobScheduler;
	using StoreIndexDescriptor = Neo4Net.Storageengine.Api.schema.StoreIndexDescriptor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.register.Registers.newDoubleLongRegister;

	public class IndexSamplingControllerFactory
	{
		 private readonly IndexSamplingConfig _config;
		 private readonly IndexStoreView _storeView;
		 private readonly JobScheduler _scheduler;
		 private readonly TokenNameLookup _tokenNameLookup;
		 private readonly LogProvider _logProvider;

		 public IndexSamplingControllerFactory( IndexSamplingConfig config, IndexStoreView storeView, JobScheduler scheduler, TokenNameLookup tokenNameLookup, LogProvider logProvider )
		 {
			  this._config = config;
			  this._storeView = storeView;
			  this._scheduler = scheduler;
			  this._tokenNameLookup = tokenNameLookup;
			  this._logProvider = logProvider;
		 }

		 public virtual IndexSamplingController Create( IndexMapSnapshotProvider snapshotProvider )
		 {
			  OnlineIndexSamplingJobFactory jobFactory = new OnlineIndexSamplingJobFactory( _storeView, _tokenNameLookup, _logProvider );
			  System.Predicate<long> samplingUpdatePredicate = CreateSamplingPredicate();
			  IndexSamplingJobQueue<long> jobQueue = new IndexSamplingJobQueue<long>( samplingUpdatePredicate );
			  IndexSamplingJobTracker jobTracker = new IndexSamplingJobTracker( _config, _scheduler );
			  IndexSamplingController.RecoveryCondition indexRecoveryCondition = CreateIndexRecoveryCondition( _logProvider, _tokenNameLookup );
			  return new IndexSamplingController( _config, jobFactory, jobQueue, jobTracker, snapshotProvider, _scheduler, indexRecoveryCondition );
		 }

		 private System.Predicate<long> CreateSamplingPredicate()
		 {
			  return new PredicateAnonymousInnerClass( this );
		 }

		 private class PredicateAnonymousInnerClass : System.Predicate<long>
		 {
			 private readonly IndexSamplingControllerFactory _outerInstance;

			 public PredicateAnonymousInnerClass( IndexSamplingControllerFactory outerInstance )
			 {
				 this.outerInstance = outerInstance;
				 output = newDoubleLongRegister();
			 }

			 private readonly Register_DoubleLongRegister output;

			 public override bool test( long? indexId )
			 {
				  _outerInstance.storeView.indexUpdatesAndSize( indexId.Value, output );
				  long updates = output.readFirst();
				  long size = output.readSecond();
				  long threshold = ( long )Math.Round( _outerInstance.config.updateRatio() * size, MidpointRounding.AwayFromZero );
				  return updates > threshold;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private IndexSamplingController.RecoveryCondition createIndexRecoveryCondition(final org.neo4j.logging.LogProvider logProvider, final org.neo4j.internal.kernel.api.TokenNameLookup tokenNameLookup)
		 private IndexSamplingController.RecoveryCondition CreateIndexRecoveryCondition( LogProvider logProvider, TokenNameLookup tokenNameLookup )
		 {
			  return new RecoveryConditionAnonymousInnerClass( this, logProvider, tokenNameLookup );
		 }

		 private class RecoveryConditionAnonymousInnerClass : IndexSamplingController.RecoveryCondition
		 {
			 private readonly IndexSamplingControllerFactory _outerInstance;

			 private LogProvider _logProvider;
			 private TokenNameLookup _tokenNameLookup;

			 public RecoveryConditionAnonymousInnerClass( IndexSamplingControllerFactory outerInstance, LogProvider logProvider, TokenNameLookup tokenNameLookup )
			 {
				 this.outerInstance = outerInstance;
				 this._logProvider = logProvider;
				 this._tokenNameLookup = tokenNameLookup;
				 log = logProvider.GetLog( typeof( IndexSamplingController ) );
				 register = newDoubleLongRegister();
			 }

			 private readonly Log log;
			 private readonly Register_DoubleLongRegister register;

			 public bool test( StoreIndexDescriptor descriptor )
			 {
				  bool result = _outerInstance.storeView.indexSample( descriptor.Id, register ).readSecond() == 0;
				  if ( result )
				  {
						log.debug( "Recovering index sampling for index %s", descriptor.Schema().userDescription(_tokenNameLookup) );
				  }
				  return result;
			 }
		 }
	}

}