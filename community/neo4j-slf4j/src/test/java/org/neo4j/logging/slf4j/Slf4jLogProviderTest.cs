using System.Collections.Generic;

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
namespace Org.Neo4j.Logging.slf4j
{
	using Level = org.apache.log4j.Level;
	using Logger = org.apache.log4j.Logger;
	using LoggingEvent = org.apache.log4j.spi.LoggingEvent;
	using BeforeEach = org.junit.jupiter.api.BeforeEach;
	using Test = org.junit.jupiter.api.Test;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.collection.IsCollectionWithSize.hasSize;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.Is.@is;

	internal class Slf4jLogProviderTest
	{
		 private readonly Slf4jLogProvider _logProvider = new Slf4jLogProvider();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeEach void clearLoggingEventsAccumulator()
		 internal virtual void ClearLoggingEventsAccumulator()
		 {
			  AccumulatingAppender.clearEventsList();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldLogDebug()
		 internal virtual void ShouldLogDebug()
		 {
			  Log log = _logProvider.getLog( this.GetType() );

			  log.Debug( "Holy debug batman!" );

			  AssertLogOccurred( Level.DEBUG, "Holy debug batman!" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldLogInfo()
		 internal virtual void ShouldLogInfo()
		 {
			  Log log = _logProvider.getLog( this.GetType() );

			  log.Info( "Holy info batman!" );

			  AssertLogOccurred( Level.INFO, "Holy info batman!" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldLogWarning()
		 internal virtual void ShouldLogWarning()
		 {
			  Log log = _logProvider.getLog( this.GetType() );

			  log.Warn( "Holy warning batman!" );

			  AssertLogOccurred( Level.WARN, "Holy warning batman!" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldLogError()
		 internal virtual void ShouldLogError()
		 {
			  Log log = _logProvider.getLog( this.GetType() );

			  log.Error( "Holy error batman!" );

			  AssertLogOccurred( Level.ERROR, "Holy error batman!" );
		 }

		 private void AssertLogOccurred( Level level, string message )
		 {
			  List<LoggingEvent> events = LoggingEvents;
			  assertThat( events, hasSize( 1 ) );
			  LoggingEvent @event = events[0];
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  assertThat( @event.LoggerName, @is( this.GetType().FullName ) );
			  assertThat( @event.Level, @is( level ) );
			  assertThat( @event.Message, @is( message ) );
		 }

		 private static List<LoggingEvent> LoggingEvents
		 {
			 get
			 {
				  return AccumulatingAppender.EventsList;
			 }
		 }

		 private static AccumulatingAppender AccumulatingAppender
		 {
			 get
			 {
				  return ( AccumulatingAppender ) Logger.RootLogger.getAppender( "accumulating" );
			 }
		 }
	}

}