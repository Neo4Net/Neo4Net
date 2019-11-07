using System;

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
namespace Neo4Net.Consistency.checking.full
{
	using Neo4Net.Consistency.checking.full.RecordDistributor;
	using Exceptions = Neo4Net.Helpers.Exceptions;
	using RelationshipRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord;

	/// <summary>
	/// Factory for creating <seealso cref="QueueDistribution"/>. Typically the distribution type is decided higher up
	/// in the call stack and the actual <seealso cref="QueueDistributor"/> is instantiated when more data is available
	/// deeper down in the call stack.
	/// </summary>
	public interface QueueDistribution
	{
		 QueueDistribution_QueueDistributor<RECORD> distributor<RECORD>( long recordsPerCpu, int numberOfThreads );

		 /// <summary>
		 /// Distributes records into <seealso cref="RecordConsumer"/>.
		 /// </summary>

		 /// <summary>
		 /// Distributes records round-robin style to all queues.
		 /// </summary>
	//JAVA TO C# CONVERTER TODO TASK: The following anonymous inner class could not be converted:
	//	 QueueDistribution ROUND_ROBIN = new QueueDistribution()
	//	 {
	//		  @@Override public <RECORD> QueueDistributor<RECORD> distributor(long recordsPerCpu, int numberOfThreads)
	//		  {
	//				return new RoundRobinQueueDistributor<>(numberOfThreads);
	//		  }
	//	 };

		 /// <summary>
		 /// Distributes <seealso cref="RelationshipRecord"/> depending on the start/end node ids.
		 /// </summary>
	//JAVA TO C# CONVERTER TODO TASK: The following anonymous inner class could not be converted:
	//	 QueueDistribution RELATIONSHIPS = new QueueDistribution()
	//	 {
	//		  @@Override public QueueDistributor<RelationshipRecord> distributor(long recordsPerCpu, int numberOfThreads)
	//		  {
	//				return new RelationshipNodesQueueDistributor(recordsPerCpu, numberOfThreads);
	//		  }
	//	 };
	}

	 public interface QueueDistribution_QueueDistributor<RECORD>
	 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void distribute(RECORD record, Neo4Net.consistency.checking.full.RecordDistributor.RecordConsumer<RECORD> consumer) throws InterruptedException;
		  void Distribute( RECORD record, RecordConsumer<RECORD> consumer );
	 }

	 public class QueueDistribution_RoundRobinQueueDistributor<RECORD> : QueueDistribution_QueueDistributor<RECORD>
	 {
		  internal readonly int NumberOfThreads;
		  internal int NextQIndex;

		  internal QueueDistribution_RoundRobinQueueDistributor( int numberOfThreads )
		  {
				this.NumberOfThreads = numberOfThreads;
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void distribute(RECORD record, Neo4Net.consistency.checking.full.RecordDistributor.RecordConsumer<RECORD> consumer) throws InterruptedException
		  public override void Distribute( RECORD record, RecordConsumer<RECORD> consumer )
		  {
				try
				{
					 consumer.Accept( record, NextQIndex );
				}
				finally
				{
					 NextQIndex = ( NextQIndex + 1 ) % NumberOfThreads;
				}
		  }
	 }

	 public class QueueDistribution_RelationshipNodesQueueDistributor : QueueDistribution_QueueDistributor<RelationshipRecord>
	 {
		  internal readonly long RecordsPerCpu;
		  internal readonly int MaxAvailableThread;
		  internal readonly int NumberOfThreads;

		  internal QueueDistribution_RelationshipNodesQueueDistributor( long recordsPerCpu, int numberOfThreads )
		  {
				this.RecordsPerCpu = recordsPerCpu;
				this.NumberOfThreads = numberOfThreads;
				this.MaxAvailableThread = numberOfThreads - 1;
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void distribute(Neo4Net.kernel.impl.store.record.RelationshipRecord relationship, Neo4Net.consistency.checking.full.RecordDistributor.RecordConsumer<Neo4Net.kernel.impl.store.record.RelationshipRecord> consumer) throws InterruptedException
		  public override void Distribute( RelationshipRecord relationship, RecordConsumer<RelationshipRecord> consumer )
		  {
				int qIndex1 = ( int ) Math.Min( MaxAvailableThread, Math.Abs( relationship.FirstNode ) / RecordsPerCpu );
				int qIndex2 = ( int ) Math.Min( MaxAvailableThread, Math.Abs( relationship.SecondNode ) / RecordsPerCpu );
				try
				{
					 consumer.Accept( relationship, qIndex1 );
					 if ( qIndex1 != qIndex2 )
					 {
						  consumer.Accept( relationship, qIndex2 );
					 }
				}
				catch ( System.IndexOutOfRangeException e )
				{
					 throw Exceptions.withMessage( e, e.Message + ", recordsPerCPU:" + RecordsPerCpu + ", relationship:" + relationship + ", number of threads: " + NumberOfThreads );
				}
		  }
	 }

}