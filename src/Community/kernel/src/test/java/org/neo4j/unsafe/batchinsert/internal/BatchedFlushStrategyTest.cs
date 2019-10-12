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
namespace Neo4Net.@unsafe.Batchinsert.@internal
{
	using Test = org.junit.Test;
	using Mockito = org.mockito.Mockito;

	public class BatchedFlushStrategyTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testFlush()
		 public virtual void TestFlush()
		 {
			  DirectRecordAccessSet recordAccessSet = Mockito.mock( typeof( DirectRecordAccessSet ) );
			  BatchInserterImpl.BatchedFlushStrategy flushStrategy = CreateFlushStrategy( recordAccessSet, 2 );
			  flushStrategy.Flush();
			  Mockito.verifyZeroInteractions( recordAccessSet );
			  flushStrategy.Flush();
			  Mockito.verify( recordAccessSet ).commit();
			  Mockito.reset( recordAccessSet );

			  flushStrategy.Flush();
			  flushStrategy.Flush();
			  flushStrategy.Flush();
			  flushStrategy.Flush();
			  flushStrategy.Flush();
			  flushStrategy.Flush();
			  flushStrategy.Flush();

			  Mockito.verify( recordAccessSet, Mockito.times( 3 ) ).commit();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testForceFlush()
		 public virtual void TestForceFlush()
		 {
			  DirectRecordAccessSet recordAccessSet = Mockito.mock( typeof( DirectRecordAccessSet ) );
			  BatchInserterImpl.BatchedFlushStrategy flushStrategy = CreateFlushStrategy( recordAccessSet, 2 );

			  flushStrategy.ForceFlush();
			  flushStrategy.ForceFlush();
			  Mockito.verify( recordAccessSet, Mockito.times( 2 ) ).commit();

			  flushStrategy.Flush();
			  flushStrategy.ForceFlush();
			  Mockito.verify( recordAccessSet, Mockito.times( 3 ) ).commit();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testResetBatchCounterOnForce()
		 public virtual void TestResetBatchCounterOnForce()
		 {
			  DirectRecordAccessSet recordAccessSet = Mockito.mock( typeof( DirectRecordAccessSet ) );
			  BatchInserterImpl.BatchedFlushStrategy flushStrategy = CreateFlushStrategy( recordAccessSet, 3 );

			  flushStrategy.Flush();
			  flushStrategy.Flush();
			  Mockito.verifyZeroInteractions( recordAccessSet );

			  flushStrategy.ForceFlush();
			  Mockito.verify( recordAccessSet ).commit();
			  Mockito.verifyNoMoreInteractions( recordAccessSet );

			  flushStrategy.Flush();
			  flushStrategy.Flush();
		 }

		 private BatchInserterImpl.BatchedFlushStrategy CreateFlushStrategy( DirectRecordAccessSet recordAccessSet, int batchSize )
		 {
			  return new BatchInserterImpl.BatchedFlushStrategy( recordAccessSet, batchSize );
		 }
	}

}