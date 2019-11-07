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
namespace Neo4Net.Kernel.impl.transaction.log.pruning
{
	using Test = org.junit.Test;
	using Mockito = org.mockito.Mockito;

	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using ThresholdConfigValue = Neo4Net.Kernel.impl.transaction.log.pruning.ThresholdConfigParser.ThresholdConfigValue;
	using Clocks = Neo4Net.Time.Clocks;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.transaction.log.pruning.LogPruneStrategyFactory.getThresholdByType;

	public class LogPruneStrategyFactoryTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testLogPruneThresholdsByType()
		 public virtual void TestLogPruneThresholdsByType()
		 {
			  FileSystemAbstraction fsa = Mockito.mock( typeof( FileSystemAbstraction ) );
			  Clock clock = Clocks.systemClock();

			  assertThat( getThresholdByType( fsa, clock, new ThresholdConfigValue( "files", 25 ), "" ), instanceOf( typeof( FileCountThreshold ) ) );
			  assertThat( getThresholdByType( fsa, clock, new ThresholdConfigValue( "size", 16000 ), "" ), instanceOf( typeof( FileSizeThreshold ) ) );
			  assertThat( getThresholdByType( fsa, clock, new ThresholdConfigValue( "txs", 4000 ), "" ), instanceOf( typeof( EntryCountThreshold ) ) );
			  assertThat( getThresholdByType( fsa, clock, new ThresholdConfigValue( "entries", 4000 ), "" ), instanceOf( typeof( EntryCountThreshold ) ) );
			  assertThat( getThresholdByType( fsa, clock, new ThresholdConfigValue( "hours", 100 ), "" ), instanceOf( typeof( EntryTimespanThreshold ) ) );
			  assertThat( getThresholdByType( fsa, clock, new ThresholdConfigValue( "days", 100_000 ), "" ), instanceOf( typeof( EntryTimespanThreshold ) ) );
		 }
	}

}