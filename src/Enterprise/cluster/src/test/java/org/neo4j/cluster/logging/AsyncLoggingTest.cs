/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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
namespace Neo4Net.cluster.logging
{
	using Test = org.junit.Test;

	using AssertableLogProvider = Neo4Net.Logging.AssertableLogProvider;
	using AsyncLogProvider = Neo4Net.Logging.async.AsyncLogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.endsWith;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.logging.AssertableLogProvider.inLog;

	public class AsyncLoggingTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogMessages()
		 public virtual void ShouldLogMessages()
		 {
			  // given
			  AssertableLogProvider logs = new AssertableLogProvider();
			  AsyncLogging logging = new AsyncLogging( logs.GetLog( "meta" ) );

			  // when
			  logging.Start();
			  try
			  {
					( new AsyncLogProvider( logging.EventSender(), logs ) ).getLog("test").info("hello");
			  }
			  finally
			  {
					logging.Stop();
			  }
			  // then
			  logs.AssertExactly( inLog( "test" ).info( endsWith( "hello" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogWhenLoggingThreadStarts()
		 public virtual void ShouldLogWhenLoggingThreadStarts()
		 {
			  // given
			  AssertableLogProvider logs = new AssertableLogProvider();
			  AsyncLogging logging = new AsyncLogging( logs.GetLog( "meta" ) );

			  // when
			  ( new AsyncLogProvider( logging.EventSender(), logs ) ).getLog("test").info("hello");

			  // then
			  logs.AssertNoLoggingOccurred();

			  // when
			  logging.Start();
			  logging.Stop();

			  // then
			  logs.AssertExactly( inLog( "test" ).info( endsWith( "hello" ) ) );
		 }
	}

}