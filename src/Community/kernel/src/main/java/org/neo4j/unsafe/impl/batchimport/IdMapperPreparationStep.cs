﻿using System;

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
namespace Neo4Net.@unsafe.Impl.Batchimport
{

	using ProgressListener = Neo4Net.Helpers.progress.ProgressListener;
	using IdMapper = Neo4Net.@unsafe.Impl.Batchimport.cache.idmapping.IdMapper;
	using Collector = Neo4Net.@unsafe.Impl.Batchimport.input.Collector;
	using LonelyProcessingStep = Neo4Net.@unsafe.Impl.Batchimport.staging.LonelyProcessingStep;
	using StageControl = Neo4Net.@unsafe.Impl.Batchimport.staging.StageControl;
	using Neo4Net.@unsafe.Impl.Batchimport.staging;
	using StatsProvider = Neo4Net.@unsafe.Impl.Batchimport.stats.StatsProvider;

	/// <summary>
	/// Preparation of an <seealso cref="IdMapper"/>, <seealso cref="IdMapper.prepare(LongFunction, Collector, ProgressListener)"/>
	/// under running as a normal <seealso cref="Step"/> so that normal execution monitoring can be applied.
	/// Useful since preparing an <seealso cref="IdMapper"/> can take a significant amount of time.
	/// </summary>
	public class IdMapperPreparationStep : LonelyProcessingStep
	{
		 private readonly IdMapper _idMapper;
		 private readonly System.Func<long, object> _allIds;
		 private readonly Collector _collector;

		 public IdMapperPreparationStep( StageControl control, Configuration config, IdMapper idMapper, System.Func<long, object> allIds, Collector collector, params StatsProvider[] additionalStatsProviders ) : base( control, "", config, additionalStatsProviders )
		 {
			  this._idMapper = idMapper;
			  this._allIds = allIds;
			  this._collector = collector;
		 }

		 protected internal override void Process()
		 {
			  _idMapper.prepare( _allIds, _collector, new ProgressListener_AdapterAnonymousInnerClass( this ) );
		 }

		 private class ProgressListener_AdapterAnonymousInnerClass : Neo4Net.Helpers.progress.ProgressListener_Adapter
		 {
			 private readonly IdMapperPreparationStep _outerInstance;

			 public ProgressListener_AdapterAnonymousInnerClass( IdMapperPreparationStep outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override void started( string task )
			 {
				  changeName( task );
			 }

			 public override void set( long progress )
			 {
				  throw new System.NotSupportedException( "Shouldn't be required" );
			 }

			 public override void failed( Exception e )
			 {
				  issuePanic( e );
			 }

			 public override void add( long progress )
			 { // Directly feed into the progress of this step.
				 lock ( this )
				 {
					  // Expected to be called by multiple threads, although quite rarely,
					  // so synchronization overhead should be negligible.
					  progress( progress );
				 }
			 }

			 public override void done()
			 { // Nothing to do
			 }
		 }
	}

}