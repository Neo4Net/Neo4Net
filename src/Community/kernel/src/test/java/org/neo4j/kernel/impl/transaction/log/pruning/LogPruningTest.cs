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
namespace Neo4Net.Kernel.impl.transaction.log.pruning
{
	using Before = org.junit.Before;
	using Test = org.junit.Test;
	using InOrder = org.mockito.InOrder;


	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using Config = Neo4Net.Kernel.configuration.Config;
	using LogFiles = Neo4Net.Kernel.impl.transaction.log.files.LogFiles;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doAnswer;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.inOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyNoMoreInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class LogPruningTest
	{
		 private readonly Config _config = Config.defaults();
		 private FileSystemAbstraction _fs;
		 private LogFiles _logFiles;
		 private LogProvider _logProvider;
		 private Clock _clock;
		 private LogPruneStrategyFactory _factory;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  _fs = mock( typeof( FileSystemAbstraction ) );
			  _logFiles = mock( typeof( LogFiles ) );
			  doAnswer( inv => new File( ( inv.Arguments[0] ).ToString() ) ).when(_logFiles).getLogFileForVersion(anyLong());
			  _logProvider = NullLogProvider.Instance;
			  _clock = mock( typeof( Clock ) );
			  _factory = mock( typeof( LogPruneStrategyFactory ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustDeleteLogFilesThatCanBePruned()
		 public virtual void MustDeleteLogFilesThatCanBePruned()
		 {
			  when( _factory.strategyFromConfigValue( eq( _fs ), eq( _logFiles ), eq( _clock ), anyString() ) ).thenReturn(upTo => LongStream.range(3, upTo));
			  LogPruning pruning = new LogPruningImpl( _fs, _logFiles,_logProvider,_factory, _clock, _config );
			  pruning.PruneLogs( 5 );
			  InOrder order = inOrder( _fs );
			  order.verify( _fs ).deleteFile( new File( "3" ) );
			  order.verify( _fs ).deleteFile( new File( "4" ) );
			  // Log file 5 is not deleted; it's the lowest version expected to remain after pruning.
			  verifyNoMoreInteractions( _fs );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustHaveLogFilesToPruneIfStrategyFindsFiles()
		 public virtual void MustHaveLogFilesToPruneIfStrategyFindsFiles()
		 {
			  when( _factory.strategyFromConfigValue( eq( _fs ), eq( _logFiles ), eq( _clock ), anyString() ) ).thenReturn(upTo => LongStream.range(3, upTo));
			  when( _logFiles.HighestLogVersion ).thenReturn( 4L );
			  LogPruning pruning = new LogPruningImpl( _fs, _logFiles, _logProvider, _factory, _clock, _config );
			  assertTrue( pruning.MightHaveLogsToPrune() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustNotHaveLogsFilesToPruneIfStrategyFindsNoFiles()
		 public virtual void MustNotHaveLogsFilesToPruneIfStrategyFindsNoFiles()
		 {
			  when( _factory.strategyFromConfigValue( eq( _fs ), eq( _logFiles ), eq( _clock ), anyString() ) ).thenReturn(x => LongStream.empty());
			  LogPruning pruning = new LogPruningImpl( _fs, _logFiles, _logProvider, _factory, _clock, _config );
			  assertFalse( pruning.MightHaveLogsToPrune() );
		 }
	}

}