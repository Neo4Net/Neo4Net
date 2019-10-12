/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.com
{
	using Test = org.junit.Test;

	using Log = Neo4Net.Logging.Log;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;

	public class LoggingResourcePoolMonitorTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testUpdatedCurrentPeakSizeLogsOnlyOnChange()
		 public virtual void TestUpdatedCurrentPeakSizeLogsOnlyOnChange()
		 {
			  Log log = mock( typeof( Log ) );
			  LoggingResourcePoolMonitor monitor = new LoggingResourcePoolMonitor( log );

			  monitor.UpdatedCurrentPeakSize( 10 );
			  verify( log, times( 1 ) ).debug( anyString() );

			  monitor.UpdatedCurrentPeakSize( 10 );
			  verify( log, times( 1 ) ).debug( anyString() );

			  monitor.UpdatedCurrentPeakSize( 11 );
			  verify( log, times( 2 ) ).debug( anyString() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testUpdatedTargetSizeOnlyOnChange()
		 public virtual void TestUpdatedTargetSizeOnlyOnChange()
		 {
			  Log log = mock( typeof( Log ) );
			  LoggingResourcePoolMonitor monitor = new LoggingResourcePoolMonitor( log );

			  monitor.UpdatedTargetSize( 10 );
			  verify( log, times( 1 ) ).debug( anyString() );

			  monitor.UpdatedTargetSize( 10 );
			  verify( log, times( 1 ) ).debug( anyString() );

			  monitor.UpdatedTargetSize( 11 );
			  verify( log, times( 2 ) ).debug( anyString() );
		 }
	}

}