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
namespace Org.Neo4j.@unsafe.Impl.Batchimport.staging
{

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static System.nanoTime;

	public abstract class PullingProducerStep : ProducerStep
	{
		 public PullingProducerStep( StageControl control, Configuration config ) : base( control, config )
		 {
		 }

		 /// <summary>
		 /// Forms batches out of some sort of data stream and sends these batches downstream.
		 /// </summary>
		 protected internal override void Process()
		 {
			  object batch;
			  while ( true )
			  {
					long startTime = nanoTime();
					batch = NextBatchOrNull( DoneBatches.get(), BatchSize );
					if ( batch == null )
					{
						 break;
					}

					TotalProcessingTime.add( nanoTime() - startTime );
					SendDownstream( batch );
					AssertHealthy();
			  }
		 }

		 /// <summary>
		 /// Generates next batch object with a target size of {@code batchSize} items from its data stream in it. </summary>
		 /// <param name="batchSize"> number of items to grab from its data stream (whatever a subclass defines as a data stream). </param>
		 /// <returns> the batch object to send downstream, or null if the data stream came to an end. </returns>
		 protected internal abstract object NextBatchOrNull( long ticket, int batchSize );
	}

}