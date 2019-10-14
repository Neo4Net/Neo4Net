using System;
using System.Threading;

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
namespace Neo4Net.@unsafe.Impl.Batchimport.staging
{
	using StatsProvider = Neo4Net.@unsafe.Impl.Batchimport.stats.StatsProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static System.nanoTime;

	/// <summary>
	/// <seealso cref="Step"/> that doesn't receive batches, doesn't send batches downstream; just processes data.
	/// </summary>
	public abstract class LonelyProcessingStep : AbstractStep<Void>
	{
		 private readonly int _batchSize;
		 private int _batch;
		 private long _lastProcessingTimestamp;

		 public LonelyProcessingStep( StageControl control, string name, Configuration config, params StatsProvider[] additionalStatsProviders ) : base( control, name, config, additionalStatsProviders )
		 {
			  this._batchSize = config.BatchSize();
		 }

		 public override long Receive( long ticket, Void nothing )
		 {
			  (new Thread(() =>
			  {
			  AssertHealthy();
			  try
			  {
				  try
				  {
					  _lastProcessingTimestamp = nanoTime();
					  Process();
					  EndOfUpstream();
				  }
				  catch ( Exception e )
				  {
					  IssuePanic( e );
				  }
			  }
			  catch ( Exception e )
			  {
				  if ( !Panic )
				  {
					  IssuePanic( e );
				  }
				  else
				  {
					  throw e;
				  }
			  }
			  })).Start();
			  return 0;
		 }

		 /// <summary>
		 /// Called once and signals the start of this step. Responsible for calling <seealso cref="progress(long)"/>
		 /// at least now and then.
		 /// </summary>
		 protected internal abstract void Process();

		 /// <summary>
		 /// Called from <seealso cref="process()"/>, reports progress so that statistics are updated appropriately.
		 /// </summary>
		 /// <param name="amount"> number of items processed since last call to this method. </param>
		 protected internal virtual void Progress( long amount )
		 {
			  _batch += ( int )amount;
			  if ( _batch >= _batchSize )
			  {
					int batches = _batch / _batchSize;
					_batch %= _batchSize;
					DoneBatches.addAndGet( batches );
					long time = nanoTime();
					TotalProcessingTime.add( time - _lastProcessingTimestamp );
					_lastProcessingTimestamp = time;
			  }
		 }
	}

}